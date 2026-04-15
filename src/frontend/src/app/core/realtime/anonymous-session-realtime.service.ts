import { inject, Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';

import {
  PublishAnonymousFileMetadataResponse,
  PublishAnonymousTextResponse,
  StoredAnonymousSession,
} from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';
import {
  ContentFileMetadataReceivedMessage,
  ContentTextReceivedMessage,
  PeerConnectedMessage,
  RealtimeIncomingMessage,
} from './realtime-message.models';
import { RealtimeSessionStore } from './realtime-session.store';

@Injectable({ providedIn: 'root' })
export class AnonymousSessionRealtimeService {
  private readonly store = inject(RealtimeSessionStore);
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly messagesSubject = new Subject<RealtimeIncomingMessage>();

  private connection: HubConnection | null = null;
  private activeSession: StoredAnonymousSession | null = null;
  private connectedSessionKey: string | null = null;

  public readonly messages$: Observable<RealtimeIncomingMessage> = this.messagesSubject.asObservable();

  public async connect(session: StoredAnonymousSession): Promise<void> {
    this.activeSession = session;

    const connection = this.ensureConnection();
    if (connection.state === HubConnectionState.Disconnected) {
      this.store.markReconnecting();
      await connection.start();
    }

    const sessionKey = `${session.sessionId}:${session.peerId}`;
    if (this.connectedSessionKey !== sessionKey) {
      await connection.invoke('ConnectSession', session.sessionId, session.peerId, session.resumeToken);
      this.connectedSessionKey = sessionKey;
    }

    this.store.markConnected();
  }

  public async publishText(
    session: StoredAnonymousSession,
    text: string,
  ): Promise<PublishAnonymousTextResponse> {
    await this.connect(session);

    return this.connection!.invoke<PublishAnonymousTextResponse>(
      'PublishText',
      session.sessionId,
      session.peerId,
      session.resumeToken,
      text,
    );
  }

  public async publishFileMetadata(
    session: StoredAnonymousSession,
    fileName: string,
    contentType: string,
    sizeBytes: number,
    shareCode?: string | null,
  ): Promise<PublishAnonymousFileMetadataResponse> {
    await this.connect(session);

    return this.connection!.invoke<PublishAnonymousFileMetadataResponse>(
      'PublishFileMetadata',
      session.sessionId,
      session.peerId,
      session.resumeToken,
      fileName,
      contentType,
      sizeBytes,
      shareCode ?? null,
    );
  }

  public markReconnecting(): void {
    this.store.markReconnecting();
  }

  public async disconnect(): Promise<void> {
    this.activeSession = null;
    this.connectedSessionKey = null;

    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      await this.connection.stop();
    }

    this.store.markDisconnected();
  }

  public async reset(): Promise<void> {
    await this.disconnect();
    this.store.markIdle();
  }

  private ensureConnection(): HubConnection {
    if (this.connection) {
      return this.connection;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/hubs/anonymous-session`)
      .withAutomaticReconnect([0, 1000, 2000, 5000])
      .build();

    connection.on('peerConnected', (message: Omit<PeerConnectedMessage, 'type'>) => {
      this.messagesSubject.next({
        type: 'peer.connected',
        ...message,
      });
    });

    connection.on('contentTextReceived', (message: Omit<ContentTextReceivedMessage, 'type'>) => {
      this.messagesSubject.next({
        type: 'content.text.received',
        ...message,
      });
    });

    connection.on(
      'contentFileMetadataReceived',
      (message: Omit<ContentFileMetadataReceivedMessage, 'type'>) => {
        this.messagesSubject.next({
          type: 'content.file-metadata.received',
          ...message,
        });
      },
    );

    connection.onreconnecting(() => {
      this.connectedSessionKey = null;
      this.store.markReconnecting();
      return Promise.resolve();
    });

    connection.onreconnected(async () => {
      this.connectedSessionKey = null;

      if (!this.activeSession) {
        this.store.markDisconnected();
        return;
      }

      try {
        await this.connect(this.activeSession);
      } catch {
        this.store.markDisconnected();
      }
    });

    connection.onclose(() => {
      this.connectedSessionKey = null;

      if (this.activeSession) {
        this.store.markDisconnected();
      } else {
        this.store.markIdle();
      }

      return Promise.resolve();
    });

    this.connection = connection;
    return connection;
  }
}
