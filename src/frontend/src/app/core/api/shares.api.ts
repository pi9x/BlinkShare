import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import {
  CompleteFileUploadResponse,
  CreateFileUploadRequest,
  CreateFileUploadResponse,
  CreateTextRequest,
  CreateTextResponse,
  GetShareByCodeResponse,
  ReadTextResponse,
  RequestDownloadResponse,
  UnlockRequest,
  UnlockResponse,
} from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';

const UNLOCK_HEADER = 'X-BlinkShare-Unlock-Proof';

@Injectable({ providedIn: 'root' })
export class SharesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  public createText(request: CreateTextRequest) {
    return this.http.post<CreateTextResponse>(`${this.baseUrl}/api/v1/shares/text`, request);
  }

  public getByCode(code: string) {
    return this.http.get<GetShareByCodeResponse>(`${this.baseUrl}/api/v1/shares/${code}`);
  }

  public readText(code: string, unlockProof?: string | null) {
    return this.http.get<ReadTextResponse>(`${this.baseUrl}/api/v1/shares/${code}/text`, {
      headers: this.unlockHeaders(unlockProof),
    });
  }

  public unlock(code: string, request: UnlockRequest) {
    return this.http.post<UnlockResponse>(`${this.baseUrl}/api/v1/shares/${code}/unlock`, request);
  }

  public createFileUpload(request: CreateFileUploadRequest) {
    return this.http.post<CreateFileUploadResponse>(`${this.baseUrl}/api/v1/shares/file`, request);
  }

  public completeFileUpload(code: string) {
    return this.http.post<CompleteFileUploadResponse>(
      `${this.baseUrl}/api/v1/shares/${code}/file/complete`,
      {},
    );
  }

  public requestDownload(code: string, unlockProof?: string | null) {
    return this.http.post<RequestDownloadResponse>(
      `${this.baseUrl}/api/v1/shares/${code}/download`,
      {},
      {
        headers: this.unlockHeaders(unlockProof),
      },
    );
  }

  private unlockHeaders(unlockProof?: string | null): HttpHeaders | undefined {
    return unlockProof ? new HttpHeaders({ [UNLOCK_HEADER]: unlockProof }) : undefined;
  }
}
