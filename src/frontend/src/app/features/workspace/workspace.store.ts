import { inject, Injectable, computed, DestroyRef, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';

import { AnonymousSessionsApi } from '../../core/api/anonymous-sessions.api';
import { HealthApi } from '../../core/api/health.api';
import { QuotasApi } from '../../core/api/quotas.api';
import { SharesApi } from '../../core/api/shares.api';
import { AnonymousSessionRealtimeService } from '../../core/realtime/anonymous-session-realtime.service';
import {
  ContentFileMetadataReceivedMessage,
  ContentTextReceivedMessage,
  PeerConnectedMessage,
} from '../../core/realtime/realtime-message.models';
import { RealtimeSessionStore } from '../../core/realtime/realtime-session.store';
import { DraftStorage } from '../../core/storage/draft.storage';
import { LocalHistoryStorage } from '../../core/storage/local-history.storage';
import { SessionRestoreStorage } from '../../core/storage/session-restore.storage';
import { AppError } from '../../core/errors/app-error.model';
import { toAppError } from '../../core/http/api-error.mapper';
import {
  CreateOrJoinAnonymousSessionResponse,
  EditorDraft,
  EditorLanguage,
  HealthResponse,
  LocalClipboardItem,
  QuotaUsageResponse,
  ShareTier,
  StoredAnonymousSession,
} from '../../shared/models/app.models';

type SessionStatus = 'idle' | 'creating' | 'connected' | 'reconnecting' | 'error';
type BusyAction = 'share' | 'session' | 'file-share' | 'file-session' | null;

interface WorkspaceState {
  initialized: boolean;
  draft: EditorDraft;
  history: LocalClipboardItem[];
  session: StoredAnonymousSession | null;
  sessionStatus: SessionStatus;
  sessionError: AppError | null;
  busyAction: BusyAction;
  latestShare:
    | {
        code: string;
        kind: 'text' | 'file';
        expiresAtUtc: string;
        note: string;
      }
    | null;
  health: HealthResponse | null;
  quota: QuotaUsageResponse | null;
  diagnosticsError: AppError | null;
  quotaError: AppError | null;
}

@Injectable()
export class WorkspaceStore {
  private readonly destroyRef = inject(DestroyRef);
  private readonly sharesApi = inject(SharesApi);
  private readonly anonymousSessionsApi = inject(AnonymousSessionsApi);
  private readonly healthApi = inject(HealthApi);
  private readonly quotasApi = inject(QuotasApi);
  private readonly draftStorage = inject(DraftStorage);
  private readonly historyStorage = inject(LocalHistoryStorage);
  private readonly sessionRestoreStorage = inject(SessionRestoreStorage);
  private readonly realtimeService = inject(AnonymousSessionRealtimeService);
  private readonly realtimeStore = inject(RealtimeSessionStore);

  private draftSaveHandle: ReturnType<typeof setTimeout> | null = null;

  private readonly state = signal<WorkspaceState>({
    initialized: false,
    draft: this.draftStorage.load(),
    history: this.historyStorage.load(),
    session: null,
    sessionStatus: 'idle',
    sessionError: null,
    busyAction: null,
    latestShare: null,
    health: null,
    quota: null,
    diagnosticsError: null,
    quotaError: null,
  });

  public readonly draft = computed(() => this.state().draft);
  public readonly history = computed(() => this.state().history);
  public readonly session = computed(() => this.state().session);
  public readonly sessionStatus = computed(() => this.state().sessionStatus);
  public readonly sessionError = computed(() => this.state().sessionError);
  public readonly busyAction = computed(() => this.state().busyAction);
  public readonly latestShare = computed(() => this.state().latestShare);
  public readonly health = computed(() => this.state().health);
  public readonly quota = computed(() => this.state().quota);
  public readonly diagnosticsError = computed(() => this.state().diagnosticsError);
  public readonly quotaError = computed(() => this.state().quotaError);
  public readonly realtimeStatus = computed(() => this.realtimeStore.status());

  public constructor() {
    this.realtimeService.messages$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((message) => {
        switch (message.type) {
          case 'peer.connected':
            this.handlePeerConnected(message);
            break;
          case 'content.text.received':
            this.handleTextReceived(message);
            break;
          case 'content.file-metadata.received':
            this.handleFileMetadataReceived(message);
            break;
        }
      });
  }

  public async initialize(autoJoinCode?: string | null): Promise<void> {
    if (this.state().initialized) {
      if (autoJoinCode) {
        await this.joinSession(autoJoinCode);
      }

      return;
    }

    this.state.update((state) => ({
      ...state,
      initialized: true,
      draft: this.draftStorage.load(),
      history: this.historyStorage.load(),
    }));

    void this.loadHealth();
    void this.loadQuota();

    const restored = this.sessionRestoreStorage.load();
    if (restored) {
      await this.resumeSession(restored);
    } else if (autoJoinCode) {
      await this.joinSession(autoJoinCode);
    }
  }

  public setDraftText(text: string): void {
    this.updateDraft({
      ...this.state().draft,
      text,
    });
  }

  public setDraftLanguage(language: EditorLanguage): void {
    this.updateDraft({
      ...this.state().draft,
      language,
    });
  }

  public setDraftWrap(wrap: boolean): void {
    this.updateDraft({
      ...this.state().draft,
      wrap,
    });
  }

  public setDraftFullscreen(fullscreen: boolean): void {
    this.updateDraft({
      ...this.state().draft,
      fullscreen,
    });
  }

  public clearDraft(): void {
    this.updateDraft({
      ...this.state().draft,
      text: '',
    });
  }

  public async createSession(): Promise<void> {
    await this.createOrJoinSession(null);
  }

  public async joinSession(code: string): Promise<void> {
    if (!code.trim()) {
      this.state.update((state) => ({
        ...state,
        sessionStatus: 'error',
        sessionError: {
          kind: 'validation',
          message: 'Enter a session code to join a peer session.',
        },
      }));
      return;
    }

    await this.createOrJoinSession(code.trim());
  }

  public async disconnectSession(): Promise<void> {
    this.sessionRestoreStorage.clear();
    await this.realtimeService.reset();
    this.state.update((state) => ({
      ...state,
      session: null,
      sessionStatus: 'idle',
      sessionError: null,
    }));
  }

  public async createTextShare(): Promise<void> {
    this.state.update((state) => ({
      ...state,
      busyAction: 'share',
      sessionError: null,
    }));

    try {
      const response = await firstValueFrom(
        this.sharesApi.createText({
          tier: ShareTier.Free,
          text: this.state().draft.text,
        }),
      );

      this.appendHistory({
        id: crypto.randomUUID(),
        direction: 'sent',
        kind: 'text',
        text: this.state().draft.text,
        language: this.state().draft.language,
        createdAtUtc: new Date().toISOString(),
        shareCode: response.code,
      });

      this.state.update((state) => ({
        ...state,
        busyAction: null,
        latestShare: {
          code: response.code,
          kind: 'text',
          expiresAtUtc: response.expiresAtUtc,
          note: 'Stored text share created.',
        },
      }));
      void this.loadQuota();
    } catch (error) {
      this.failSession(error, 'share');
    }
  }

  public async sendDraftToSession(textToSend?: string): Promise<boolean> {
    const session = this.state().session;
    if (!session) {
      this.state.update((state) => ({
        ...state,
        sessionError: {
          kind: 'validation',
          message: 'Create or join a session before relaying text.',
        },
      }));
      return false;
    }

    const payload = textToSend ?? this.state().draft.text;

    this.state.update((state) => ({
      ...state,
      busyAction: 'session',
      sessionError: null,
    }));

    try {
      const response = await this.realtimeService.publishText(session, payload);

      this.appendHistory({
        id: crypto.randomUUID(),
        direction: 'sent',
        kind: 'text',
        text: response.text,
        language: this.state().draft.language,
        createdAtUtc: response.publishedAtUtc,
        sessionCode: session.code,
      });

      this.state.update((state) => ({
        ...state,
        busyAction: null,
        sessionStatus: 'connected',
      }));
      return true;
    } catch (error) {
      this.failSession(error, 'session');
      return false;
    }
  }

  public async publishFileMetadata(file: File): Promise<void> {
    const session = this.state().session;
    if (!session) {
      this.state.update((state) => ({
        ...state,
        sessionError: {
          kind: 'validation',
          message: 'Join a session before relaying file metadata.',
        },
      }));
      return;
    }

    this.state.update((state) => ({
      ...state,
      busyAction: 'file-session',
      sessionError: null,
    }));

    try {
      const response = await this.realtimeService.publishFileMetadata(
        session,
        file.name,
        file.type || 'application/octet-stream',
        file.size,
      );

      this.appendHistory({
        id: crypto.randomUUID(),
        direction: 'sent',
        kind: 'file-metadata',
        fileName: response.fileName,
        contentType: response.contentType,
        sizeBytes: response.sizeBytes,
        createdAtUtc: response.publishedAtUtc,
        sessionCode: session.code,
      });

      this.state.update((state) => ({
        ...state,
        busyAction: null,
      }));
    } catch (error) {
      this.failSession(error, 'file-session');
    }
  }

  public async createFileShare(file: File): Promise<void> {
    this.state.update((state) => ({
      ...state,
      busyAction: 'file-share',
      sessionError: null,
    }));

    try {
      const created = await firstValueFrom(
        this.sharesApi.createFileUpload({
          tier: ShareTier.Free,
          fileName: file.name,
          contentType: file.type || 'application/octet-stream',
          sizeBytes: file.size,
        }),
      );

      const uploadResponse = await fetch(created.uploadUrl, {
        method: 'PUT',
        headers: {
          'Content-Type': file.type || 'application/octet-stream',
        },
        body: file,
      });

      if (!uploadResponse.ok) {
        throw new Error('File upload failed before the share could be completed.');
      }

      await firstValueFrom(this.sharesApi.completeFileUpload(created.code));

      this.appendHistory({
        id: crypto.randomUUID(),
        direction: 'sent',
        kind: 'file-metadata',
        fileName: file.name,
        contentType: file.type || 'application/octet-stream',
        sizeBytes: file.size,
        createdAtUtc: new Date().toISOString(),
        shareCode: created.code,
      });

      this.state.update((state) => ({
        ...state,
        busyAction: null,
        latestShare: {
          code: created.code,
          kind: 'file',
          expiresAtUtc: created.expiresAtUtc,
          note: 'Stored file share uploaded and finalized.',
        },
      }));
      void this.loadQuota();
    } catch (error) {
      this.failSession(error, 'file-share');
    }
  }

  public reuseItem(item: LocalClipboardItem): void {
    if (item.kind !== 'text') {
      return;
    }

    this.updateDraft({
      ...this.state().draft,
      text: item.text ?? '',
      language: item.language ?? 'plaintext',
    });
  }

  public clearHistory(): void {
    this.historyStorage.clear();
    this.state.update((state) => ({
      ...state,
      history: [],
    }));
  }

  private async createOrJoinSession(code: string | null): Promise<void> {
    this.realtimeService.markReconnecting();
    this.state.update((state) => ({
      ...state,
      busyAction: 'session',
      sessionStatus: 'creating',
      sessionError: null,
    }));

    try {
      const response = await firstValueFrom(this.anonymousSessionsApi.createOrJoin({ code }));
      await this.applySessionResponse(response);
      this.state.update((state) => ({
        ...state,
        busyAction: null,
      }));
    } catch (error) {
      this.failSession(error, 'session');
    }
  }

  private async resumeSession(restored: StoredAnonymousSession): Promise<void> {
    this.realtimeService.markReconnecting();
    this.state.update((state) => ({
      ...state,
      session: restored,
      sessionStatus: 'reconnecting',
      sessionError: null,
    }));

    try {
      const response = await firstValueFrom(
        this.anonymousSessionsApi.resume({
          sessionId: restored.sessionId,
          peerId: restored.peerId,
          resumeToken: restored.resumeToken,
        }),
      );

      await this.applySessionResponse({
        sessionId: response.sessionId,
        code: response.code,
        peerId: response.peerId,
        resumeToken: restored.resumeToken,
        peerCount: response.peerCount,
        reconnectGraceSeconds: restored.reconnectGraceSeconds,
      });
    } catch (error) {
      this.sessionRestoreStorage.clear();
      await this.realtimeService.disconnect();
      this.state.update((state) => ({
        ...state,
        session: null,
        sessionStatus: 'error',
        sessionError: toAppError(error),
      }));
    }
  }

  private async applySessionResponse(response: CreateOrJoinAnonymousSessionResponse): Promise<void> {
    const stored: StoredAnonymousSession = {
      sessionId: response.sessionId,
      code: response.code,
      peerId: response.peerId,
      resumeToken: response.resumeToken,
      peerCount: response.peerCount,
      reconnectGraceSeconds: response.reconnectGraceSeconds,
      restoredAtUtc: new Date().toISOString(),
    };

    this.sessionRestoreStorage.save(stored);
    await this.realtimeService.connect(stored);

    this.state.update((state) => ({
      ...state,
      session: stored,
      sessionStatus: 'connected',
      sessionError: null,
    }));
  }

  private handlePeerConnected(message: PeerConnectedMessage): void {
    const session = this.state().session;
    if (!session || session.sessionId !== message.sessionId) {
      return;
    }

    this.updateSession({
      ...session,
      peerCount: message.peerCount,
    });
  }

  private handleTextReceived(message: ContentTextReceivedMessage): void {
    const session = this.state().session;
    if (!session || session.sessionId !== message.sessionId || session.peerId === message.peerId) {
      return;
    }

    const itemId = `received:text:${message.sessionId}:${message.peerId}:${message.publishedAtUtc}`;
    if (this.state().history.some((item) => item.id === itemId)) {
      return;
    }

    this.appendHistory({
      id: itemId,
      direction: 'received',
      kind: 'text',
      text: message.text,
      language: this.state().draft.language,
      createdAtUtc: message.publishedAtUtc,
      sessionCode: session.code,
    });
  }

  private handleFileMetadataReceived(message: ContentFileMetadataReceivedMessage): void {
    const session = this.state().session;
    if (!session || session.sessionId !== message.sessionId || session.peerId === message.peerId) {
      return;
    }

    const itemId = `received:file:${message.sessionId}:${message.peerId}:${message.publishedAtUtc}:${message.fileName}`;
    if (this.state().history.some((item) => item.id === itemId)) {
      return;
    }

    this.appendHistory({
      id: itemId,
      direction: 'received',
      kind: 'file-metadata',
      fileName: message.fileName,
      contentType: message.contentType,
      sizeBytes: message.sizeBytes,
      createdAtUtc: message.publishedAtUtc,
      sessionCode: session.code,
    });
  }

  private updateSession(session: StoredAnonymousSession): void {
    this.sessionRestoreStorage.save(session);
    this.state.update((state) => ({
      ...state,
      session,
      sessionStatus: 'connected',
      sessionError: null,
    }));
  }

  private appendHistory(item: LocalClipboardItem): void {
    const history = this.historyStorage.add(item);
    this.state.update((state) => ({
      ...state,
      history,
    }));
  }

  private updateDraft(draft: EditorDraft): void {
    this.state.update((state) => ({
      ...state,
      draft,
    }));

    if (this.draftSaveHandle) {
      clearTimeout(this.draftSaveHandle);
    }

    this.draftSaveHandle = window.setTimeout(() => {
      this.draftStorage.save(draft);
    }, 180);
  }

  private async loadHealth(): Promise<void> {
    try {
      const response = await firstValueFrom(this.healthApi.get());
      this.state.update((state) => ({
        ...state,
        health: response,
        diagnosticsError: null,
      }));
    } catch (error) {
      this.state.update((state) => ({
        ...state,
        diagnosticsError: toAppError(error),
      }));
    }
  }

  private async loadQuota(): Promise<void> {
    try {
      const response = await firstValueFrom(this.quotasApi.getUsage());
      this.state.update((state) => ({
        ...state,
        quota: response,
        quotaError: null,
      }));
    } catch (error) {
      this.state.update((state) => ({
        ...state,
        quotaError: toAppError(error),
      }));
    }
  }

  private failSession(error: unknown, busyAction: BusyAction): void {
    const appError = toAppError(error);
    if (busyAction === 'session' || busyAction === 'file-session') {
      void this.realtimeService.disconnect();
    }

    this.state.update((state) => ({
      ...state,
      busyAction: null,
      sessionStatus: busyAction === 'share' || busyAction === 'file-share' ? state.sessionStatus : 'error',
      sessionError: appError,
    }));
  }
}
