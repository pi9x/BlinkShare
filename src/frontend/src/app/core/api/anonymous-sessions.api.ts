import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import {
  CreateOrJoinAnonymousSessionRequest,
  CreateOrJoinAnonymousSessionResponse,
  PublishAnonymousFileMetadataRequest,
  PublishAnonymousFileMetadataResponse,
  PublishAnonymousTextRequest,
  PublishAnonymousTextResponse,
  ResumeAnonymousSessionRequest,
  ResumeAnonymousSessionResponse,
} from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class AnonymousSessionsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  public createOrJoin(request: CreateOrJoinAnonymousSessionRequest) {
    return this.http.post<CreateOrJoinAnonymousSessionResponse>(
      `${this.baseUrl}/api/v1/anonymous-sessions/join`,
      request,
    );
  }

  public resume(request: ResumeAnonymousSessionRequest) {
    return this.http.post<ResumeAnonymousSessionResponse>(
      `${this.baseUrl}/api/v1/anonymous-sessions/resume`,
      request,
    );
  }

  public publishText(sessionId: string, request: PublishAnonymousTextRequest) {
    return this.http.post<PublishAnonymousTextResponse>(
      `${this.baseUrl}/api/v1/anonymous-sessions/${sessionId}/text`,
      request,
    );
  }

  public publishFileMetadata(sessionId: string, request: PublishAnonymousFileMetadataRequest) {
    return this.http.post<PublishAnonymousFileMetadataResponse>(
      `${this.baseUrl}/api/v1/anonymous-sessions/${sessionId}/file-metadata`,
      request,
    );
  }
}
