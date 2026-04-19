import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ViewChild, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { WorkspaceStore } from './workspace.store';
import { SharesApi } from '../../core/api/shares.api';
import { ClipboardItemCardComponent } from '../../shared/ui/clipboard-item-card/clipboard-item-card.component';
import { CodeEditorComponent } from '../../shared/ui/code-editor/code-editor.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { EDITOR_LANGUAGES, EditorLanguage, LocalClipboardItem } from '../../shared/models/app.models';
import { toAppError } from '../../core/http/api-error.mapper';
import { copyText } from '../../shared/utils/clipboard';
import { formatBytes } from '../../shared/utils/bytes';
import { formatDateTime, formatRelativeTime } from '../../shared/utils/time';

type PreviewState =
  | {
      kind: 'text';
      title: string;
      text: string;
      language: EditorLanguage;
    }
  | {
      kind: 'image';
      title: string;
      loading: boolean;
      expired: boolean;
      imageUrl: string | null;
      downloadUrl: string | null;
    };

@Component({
  selector: 'app-workspace-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ClipboardItemCardComponent,
    CodeEditorComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  providers: [WorkspaceStore],
  template: `
    <div class="grid gap-3.5 lg:grid-cols-[minmax(0,1.4fr)_27rem]">
      <section class="space-y-3.5">
        <article class="surface-card p-3 sm:p-3.5">
          <div class="grid gap-3.5 lg:grid-cols-[minmax(0,1fr)_14rem] lg:items-stretch">
            <div class="flex h-full flex-col justify-between gap-2.5">
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <p class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Your code</p>
                  <div class="mt-1 flex min-h-[2.1rem] items-center sm:h-[2.3rem]">
                    @if (store.session(); as session) {
                      <h2 class="font-code text-[1.65rem] leading-none font-bold tracking-[0.08em] sm:text-[1.8rem]" style="color: var(--text-strong);">{{ session.code }}</h2>
                    } @else {
                      <h2 class="text-[1.65rem] leading-none font-bold tracking-tight sm:whitespace-nowrap sm:text-[1.8rem]" style="color: var(--text-strong);">Not connected</h2>
                    }
                  </div>
                </div>

                <div class="flex min-h-[2.1rem] shrink-0 flex-col items-end justify-start gap-1 pt-0.5 text-right sm:min-h-[2.9rem] sm:pt-0">
                  <app-status-badge [label]="store.sessionStatus()" [tone]="sessionTone()" />
                  @if (store.session(); as session) {
                    <div class="text-xs font-semibold leading-none sm:leading-normal" style="color: var(--muted-label);">
                      <span>{{ session.peerCount }} peer{{ session.peerCount === 1 ? '' : 's' }}</span>
                    </div>
                  }
                </div>
              </div>

              <div class="flex min-h-[2.15rem] flex-wrap items-center gap-1.5 overflow-hidden">
                <button class="icon-button icon-button-active" type="button" (click)="store.createSession()" [disabled]="store.busyAction() !== null" aria-label="New code" title="New code">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="M12 5v14" />
                    <path d="M5 12h14" />
                  </svg>
                </button>
                @if (store.session(); as session) {
                  <button class="icon-button icon-button-copy" [class.icon-button-success]="isCopied('session-code')" type="button" (click)="copy(session.code, 'session-code')" [attr.aria-label]="isCopied('session-code') ? 'Copied code' : 'Copy code'" [attr.title]="isCopied('session-code') ? 'Copied to clipboard' : 'Copy code'">
                    @if (isCopied('session-code')) {
                      <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                        <path d="m5 12 5 5L20 7" />
                      </svg>
                    } @else {
                      <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                        <rect x="9" y="9" width="11" height="11" rx="2" />
                        <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                      </svg>
                    }
                  </button>
                  <button class="icon-button icon-button-close" type="button" (click)="store.disconnectSession()" aria-label="Clear session" title="Clear session">
                    <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                      <path d="M18 6 6 18" />
                      <path d="m6 6 12 12" />
                    </svg>
                  </button>
                }
              </div>
            </div>

            <div class="surface-panel flex h-full min-w-0 flex-col justify-start p-2.5">
              <label class="field-label">Connect to code</label>
              <div class="mt-2 flex items-center gap-1.5">
                <input
                  class="field-input h-[2.15rem] min-w-0 px-2.5 py-0 text-sm leading-[2.15rem] font-code uppercase tracking-[0.16em]"
                  [value]="joinCode()"
                  (input)="joinCode.set(($any($event.target).value || '').toUpperCase())"
                  placeholder="ABCD1234"
                />
                <button class="icon-button shrink-0" type="button" (click)="store.joinSession(joinCode())" [disabled]="store.busyAction() !== null" aria-label="Connect" title="Connect">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="m5 12 14 0" />
                    <path d="m13 6 6 6-6 6" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </article>

        <article class="surface-card p-3 sm:p-3.5">
          <div class="mb-2.5 flex items-center justify-between gap-2 overflow-x-auto pb-1">
            <div class="workspace-compact-select-shell">
              <select
                class="workspace-compact-select"
                [value]="store.draft().language"
                (change)="store.setDraftLanguage($any($event.target).value)"
              >
                @for (language of languages; track language.id) {
                  <option [value]="language.id">{{ language.label }}</option>
                }
              </select>
            </div>
            @if (hasDraftText()) {
              <button class="icon-button icon-button-active shrink-0" type="button" (click)="sendDraft()" [disabled]="store.busyAction() !== null" aria-label="Send to session" title="Send to session">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M22 2 11 13" />
                  <path d="M22 2 15 22l-4-9-9-4Z" />
                </svg>
              </button>
            }
          </div>

          <div>
            <app-code-editor
              [value]="store.draft().text"
              [language]="store.draft().language"
              [wrap]="store.draft().wrap"
              (contentChanged)="store.setDraftText($event)"
            />
          </div>
        </article>

        <article class="surface-card p-3 sm:p-3.5">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p class="text-[1.2rem] font-semibold" style="color: var(--text-strong);">Files</p>
              @if (selectedFileName()) {
                <p class="text-sm" style="color: var(--muted-label);">{{ selectedFileName() }} · {{ selectedFileMeta() }}</p>
              } @else {
                <p class="text-sm" style="color: var(--muted-label);">Pick a file, then upload it.</p>
              }
            </div>

            <div class="flex items-center gap-2">
              <button class="icon-button" type="button" (click)="fileInput.click()" aria-label="Choose file" title="Choose file">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                  <path d="M17 8 12 3 7 8" />
                  <path d="M12 3v12" />
                </svg>
              </button>
              @if (selectedFileName()) {
                <button class="icon-button icon-button-active" type="button" (click)="uploadSelectedFile()" [disabled]="store.busyAction() !== null" aria-label="Upload file" title="Upload file">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="M22 2 11 13" />
                    <path d="M22 2 15 22l-4-9-9-4Z" />
                  </svg>
                </button>
              }
            </div>
          </div>

          @if (selectedFileName() && fileUploadProgress() !== null) {
            <div class="mt-2.5">
              <div class="mb-1 flex items-center justify-between text-xs font-semibold" style="color: var(--muted-label);">
                <span>Uploading</span>
                <span>{{ fileUploadProgress() }}%</span>
              </div>
              <div class="h-1.5 overflow-hidden rounded-[0.35rem]" style="background: var(--line-default);">
                <div class="h-full rounded-[0.35rem] transition-[width] duration-150" style="background: var(--surface-strong);" [style.width.%]="fileUploadProgress() ?? 0"></div>
              </div>
            </div>
          }

          <input #fileInput type="file" class="hidden" (change)="onFileChosen($event)" />
        </article>
      </section>

      <aside class="space-y-3.5">
        @if (store.latestShare(); as latestShare) {
          <article class="surface-card p-3.5">
            <p class="text-xs font-bold uppercase tracking-[0.04em]" style="color: var(--muted-label);">Last share</p>
            <h3 class="mt-1 text-[1.7rem] font-bold tracking-tight" style="color: var(--text-strong);">{{ latestShare.code }}</h3>
            <p class="mt-1.5 text-sm" style="color: var(--muted-label);">{{ formatRelativeTime(latestShare.expiresAtUtc) }}</p>
            <div class="mt-2.5 flex gap-2">
              <button class="icon-button icon-button-copy" [class.icon-button-success]="isCopied('latest-share')" type="button" (click)="copy(latestShare.code, 'latest-share')" [attr.aria-label]="isCopied('latest-share') ? 'Copied share code' : 'Copy share code'" [attr.title]="isCopied('latest-share') ? 'Copied to clipboard' : 'Copy share code'">
                @if (isCopied('latest-share')) {
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="m5 12 5 5L20 7" />
                  </svg>
                } @else {
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <rect x="9" y="9" width="11" height="11" rx="2" />
                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                  </svg>
                }
              </button>
              <a class="icon-button" [routerLink]="['/share', latestShare.code]" aria-label="Open share" title="Open share">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M15 3h6v6" />
                  <path d="M10 14 21 3" />
                  <path d="M21 14v5a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5" />
                </svg>
              </a>
            </div>
          </article>
        }

        <article class="surface-card flex max-h-[34rem] min-h-[12rem] flex-col overflow-hidden p-0">
          <div class="sticky top-0 z-10 flex items-center justify-between gap-2 border-b px-3.5 py-3" style="border-color: var(--line-default); background: var(--surface-default);">
            <p class="text-[1.12rem] font-semibold" style="color: var(--text-strong);">Recent</p>
            <button class="icon-button icon-button-danger h-9 w-9" type="button" (click)="store.clearHistory()" aria-label="Clear history" title="Clear history">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M3 6h18" />
                <path d="M8 6V4h8v2" />
                <path d="m19 6-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
              </svg>
            </button>
          </div>

          <div class="min-h-0 flex-1 space-y-2.5 overflow-y-auto px-3.5 py-3 recent-scroll">
            @if (store.history().length) {
              @for (item of store.history(); track item.id; let i = $index) {
                <div class="pt-2.5" [class.pt-0]="i === 0" [style.border-top]="i === 0 ? 'none' : '1px solid var(--line-default)'">
                  <app-clipboard-item-card [item]="item" [copied]="isCopied('history:' + item.id)" (copy)="copyItem(item)" (download)="downloadItem(item)" (preview)="previewItem(item)" />
                </div>
              }
            } @else {
              <app-empty-state eyebrow="Empty" title="Nothing yet" copy="Sent and received items show up here." />
            }
          </div>
        </article>

        <article class="surface-card p-3.5 text-sm" style="color: var(--text-muted);">
          <p class="text-[1.02rem] font-semibold" style="color: var(--text-strong);">Quota</p>
          @if (store.quota(); as quota) {
            <p class="mt-1 text-lg font-semibold tracking-tight" style="color: var(--text-strong);">
              {{ formatBytes(quota.bytesUsedToday) }} / {{ formatBytes(quota.bytesLimitToday) }}
            </p>

            <div class="mt-2.5 grid gap-2">
              <div class="surface-panel p-2.5">
                <p class="text-[0.68rem] font-semibold uppercase tracking-[0.16em]" style="color: var(--muted-label);">Tier</p>
                <p class="mt-1 text-sm font-semibold" style="color: var(--text-strong);">{{ quotaTierLabel(quota.tier) }}</p>
              </div>
              <div class="surface-panel p-2.5">
                <p class="text-[0.68rem] font-semibold uppercase tracking-[0.16em]" style="color: var(--muted-label);">Shares today</p>
                <p class="mt-1 text-sm font-semibold" style="color: var(--text-strong);">{{ quota.sharesCreatedToday }}</p>
              </div>
              <div class="surface-panel p-2.5">
                <p class="text-[0.68rem] font-semibold uppercase tracking-[0.16em]" style="color: var(--muted-label);">Resets</p>
                <p class="mt-1 text-sm font-semibold" style="color: var(--text-strong);">{{ formatDateTime(quota.windowEndsAtUtc) }}</p>
              </div>
            </div>
          } @else if (store.quotaError(); as quotaError) {
            <p class="mt-2 rounded-[0.4rem] px-3 py-2 text-sm" style="background: var(--danger-soft); color: var(--danger);">{{ quotaError.message }}</p>
          } @else {
            <p class="mt-2 text-sm" style="color: var(--text-muted);">Loading quota usage…</p>
          }
        </article>
      </aside>
    </div>

    @if (previewState(); as preview) {
      <div class="fixed inset-0 z-40 flex items-center justify-center bg-slate-950/45 px-4 py-6" (click)="closePreview()">
        <article class="surface-card max-h-[90vh] w-full max-w-4xl overflow-hidden" (click)="$event.stopPropagation()">
          <div class="flex items-center justify-between gap-3 border-b px-4 py-3" style="border-color: var(--line-default);">
            <div class="min-w-0">
              <p class="text-xs font-bold uppercase tracking-[0.12em]" style="color: var(--muted-label);">Preview</p>
              <h3 class="truncate text-lg font-semibold" style="color: var(--text-strong);">{{ preview.title }}</h3>
            </div>

            <div class="flex shrink-0 items-center gap-2">
              @if (preview.kind === 'text') {
                <button class="icon-button icon-button-copy" [class.icon-button-success]="isCopied('preview-text')" type="button" (click)="copy(preview.text, 'preview-text')" [attr.aria-label]="isCopied('preview-text') ? 'Copied text' : 'Copy text'" [attr.title]="isCopied('preview-text') ? 'Copied to clipboard' : 'Copy text'">
                  @if (isCopied('preview-text')) {
                    <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                      <path d="m5 12 5 5L20 7" />
                    </svg>
                  } @else {
                    <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                      <rect x="9" y="9" width="11" height="11" rx="2" />
                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                    </svg>
                  }
                </button>
              } @else if (preview.kind === 'image' && !preview.expired && preview.downloadUrl) {
                <button class="icon-button icon-button-active" type="button" (click)="downloadPreviewImage()" aria-label="Download image" title="Download image">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                    <path d="M7 10 12 15 17 10" />
                    <path d="M12 15V3" />
                  </svg>
                </button>
              }

              <button class="icon-button icon-button-close" type="button" (click)="closePreview()" aria-label="Close preview" title="Close preview">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M18 6 6 18" />
                  <path d="m6 6 12 12" />
                </svg>
              </button>
            </div>
          </div>

          <div class="max-h-[calc(90vh-4.5rem)] overflow-auto p-4">
            @if (preview.kind === 'text') {
              <app-code-editor [value]="preview.text" [language]="preview.language" [readOnly]="true" [wrap]="true" />
            } @else if (preview.kind === 'image') {
              @if (preview.loading) {
                <div class="surface-panel p-4 text-sm" style="color: var(--text-muted);">Preparing image preview...</div>
              } @else if (preview.expired) {
                <div class="surface-panel p-4 text-sm" style="color: var(--text-muted);">
                  This image share has expired. Preview and download are no longer available.
                </div>
              } @else if (preview.imageUrl) {
                <div class="flex justify-center rounded-[var(--radius-ui)] p-3" style="background: var(--code-dark);">
                  <img [src]="preview.imageUrl" [alt]="preview.title" class="max-h-[70vh] max-w-full rounded-[var(--radius-ui)] object-contain" />
                </div>
              }
            }
          </div>
        </article>
      </div>
    }

    @if (snackbarMessage(); as message) {
      <div class="pointer-events-none fixed right-4 bottom-4 z-50 max-w-sm sm:right-6 sm:bottom-6">
        <div class="pointer-events-auto flex items-start gap-3 rounded-[0.6rem] border px-4 py-3 shadow-sm" style="border-color: #f3d4cf; background: var(--danger-soft); color: var(--danger);">
          <svg viewBox="0 0 24 24" class="mt-0.5 h-5 w-5 shrink-0 fill-none stroke-current stroke-2">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 8v5" />
            <path d="M12 16h.01" />
          </svg>
          <div class="min-w-0 flex-1">
            <p class="text-sm font-semibold">Something went wrong</p>
            <p class="mt-1 text-sm leading-5">{{ message }}</p>
          </div>
          <button class="icon-button icon-button-close h-8 w-8 shrink-0" type="button" (click)="dismissSnackbar()" aria-label="Dismiss error" title="Dismiss error">
            <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
              <path d="M18 6 6 18" />
              <path d="m6 6 12 12" />
            </svg>
          </button>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkspacePageComponent {
  private readonly route = inject(ActivatedRoute);
  protected readonly store = inject(WorkspaceStore);
  private readonly sharesApi = inject(SharesApi);
  protected readonly joinCode = signal('');
  protected readonly copiedKey = signal<string | null>(null);
  protected readonly snackbarMessage = signal<string | null>(null);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly fileUploadProgress = signal<number | null>(null);
  protected readonly previewState = signal<PreviewState | null>(null);
  protected readonly languages = EDITOR_LANGUAGES;

  private copiedHandle: ReturnType<typeof setTimeout> | null = null;
  private snackbarHandle: ReturnType<typeof setTimeout> | null = null;
  @ViewChild('fileInput') private readonly fileInput?: { nativeElement: HTMLInputElement };
  @ViewChild(CodeEditorComponent) private readonly codeEditor?: CodeEditorComponent;

  public constructor() {
    const routeCode = this.route.snapshot.paramMap.get('code');
    const queryCode = this.route.snapshot.queryParamMap.get('code');
    const joinCode = routeCode ?? queryCode;

    this.joinCode.set((joinCode ?? '').toUpperCase());
    void this.store.initialize(joinCode);

    effect(() => {
      const error = this.store.sessionError();
      if (!error) {
        return;
      }

      this.showSnackbar(error.message);
    });
  }

  protected sessionTone(): 'neutral' | 'success' | 'warning' | 'danger' {
    switch (this.store.sessionStatus()) {
      case 'connected':
        return 'success';
      case 'reconnecting':
        return 'warning';
      case 'error':
        return 'danger';
      default:
        return 'neutral';
    }
  }

  protected onFileChosen(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
    this.fileUploadProgress.set(null);
  }

  protected selectedFileName(): string {
    return this.selectedFile()?.name ?? '';
  }

  protected selectedFileMeta(): string {
    const file = this.selectedFile();
    if (!file) {
      return '';
    }

    return `${file.type || 'application/octet-stream'} · ${formatBytes(file.size)}`;
  }

  protected async uploadSelectedFile(): Promise<void> {
    const file = this.selectedFile();
    if (!file) {
      return;
    }

    this.fileUploadProgress.set(0);
    const uploaded = await this.store.createFileShare(file, (percent) => {
      this.fileUploadProgress.set(percent);
    });

    if (uploaded) {
      this.resetSelectedFile();
    }

    this.fileUploadProgress.set(null);
  }

  protected isCopied(key: string): boolean {
    return this.copiedKey() === key;
  }

  protected async copy(value: string, key: string): Promise<void> {
    const copied = await copyText(value);
    if (copied) {
      this.flashCopied(key);
    }
  }

  protected async copyItem(item: { id?: string; kind: string; text?: string; fileName?: string }): Promise<void> {
    if (item.kind === 'text') {
      const copied = await copyText(item.text ?? '');
      if (copied && item.id) {
        this.flashCopied(`history:${item.id}`);
      }
      return;
    }

    const copied = await copyText(item.fileName ?? '');
    if (copied && item.id) {
      this.flashCopied(`history:${item.id}`);
    }
  }

  protected async downloadItem(item: { kind: string; shareCode?: string }): Promise<void> {
    if (item.kind !== 'file-metadata') {
      return;
    }

    if (!item.shareCode) {
      this.showSnackbar('This file item contains metadata only and has no downloadable share.');
      return;
    }

    try {
      const response = await firstValueFrom(this.sharesApi.requestDownload(item.shareCode));
      window.open(response.downloadUrl, '_blank', 'noopener');
    } catch (error) {
      this.showSnackbar(toAppError(error).message);
    }
  }

  protected async previewItem(item: LocalClipboardItem): Promise<void> {
    if (item.kind === 'text') {
      this.previewState.set({
        kind: 'text',
        title: item.language ?? 'Plain text',
        text: item.text ?? '',
        language: item.language ?? 'plaintext',
      });
      return;
    }

    if (!this.isImageItem(item)) {
      return;
    }

    const loadingState: PreviewState = {
      kind: 'image',
      title: item.fileName ?? 'Image',
      loading: true,
      expired: false,
      imageUrl: null,
      downloadUrl: null,
    };
    this.previewState.set(loadingState);

    if (!item.shareCode) {
      this.previewState.set({
        ...loadingState,
        loading: false,
        expired: true,
      });
      return;
    }

    try {
      await firstValueFrom(this.sharesApi.getByCode(item.shareCode));
      const response = await firstValueFrom(this.sharesApi.requestDownload(item.shareCode));
      this.previewState.set({
        ...loadingState,
        loading: false,
        imageUrl: response.downloadUrl,
        downloadUrl: response.downloadUrl,
      });
    } catch (error) {
      const appError = toAppError(error);
      if (appError.kind === 'expired' || appError.code === 'share.expired') {
        this.previewState.set({
          ...loadingState,
          loading: false,
          expired: true,
        });
        return;
      }

      this.closePreview();
      this.showSnackbar(appError.message);
    }
  }

  protected closePreview(): void {
    this.previewState.set(null);
  }

  protected downloadPreviewImage(): void {
    const preview = this.previewState();
    if (preview?.kind !== 'image' || !preview.downloadUrl || preview.expired) {
      return;
    }

    window.open(preview.downloadUrl, '_blank', 'noopener');
  }

  private isImageItem(item: LocalClipboardItem): boolean {
    return item.kind === 'file-metadata' && (item.contentType ?? '').toLowerCase().startsWith('image/');
  }

  protected async sendDraft(): Promise<void> {
    const currentText = this.store.draft().text;
    if (!currentText.trim()) {
      return;
    }

    const sent = await this.store.sendDraftToSession(currentText);
    if (sent) {
      this.store.clearDraft();
      this.codeEditor?.clearContent();
    }
  }

  private flashCopied(key: string): void {
    this.copiedKey.set(key);

    if (this.copiedHandle) {
      clearTimeout(this.copiedHandle);
    }

    this.copiedHandle = setTimeout(() => {
      if (this.copiedKey() === key) {
        this.copiedKey.set(null);
      }
    }, 1200);
  }

  protected dismissSnackbar(): void {
    if (this.snackbarHandle) {
      clearTimeout(this.snackbarHandle);
      this.snackbarHandle = null;
    }

    this.snackbarMessage.set(null);
  }

  private showSnackbar(message: string): void {
    this.snackbarMessage.set(message);

    if (this.snackbarHandle) {
      clearTimeout(this.snackbarHandle);
    }

    this.snackbarHandle = setTimeout(() => {
      if (this.snackbarMessage() === message) {
        this.snackbarMessage.set(null);
      }
    }, 4200);
  }

  protected readonly formatDateTime = formatDateTime;
  protected readonly formatRelativeTime = formatRelativeTime;
  protected readonly formatBytes = formatBytes;

  protected hasDraftText(): boolean {
    return this.store.draft().text.trim().length > 0;
  }

  private resetSelectedFile(): void {
    this.selectedFile.set(null);
    this.fileUploadProgress.set(null);

    const input = this.fileInput?.nativeElement;
    if (input) {
      input.value = '';
    }
  }

  protected quotaTierLabel(tier: number): string {
    switch (tier) {
      case 1:
        return 'Anonymous';
      case 2:
        return 'Free';
      case 3:
        return 'Premium';
      default:
        return String(tier);
    }
  }
}
