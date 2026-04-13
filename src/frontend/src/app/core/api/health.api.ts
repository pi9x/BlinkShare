import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { HealthResponse } from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class HealthApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  public get() {
    return this.http.get<HealthResponse>(`${this.baseUrl}/healthz`);
  }
}
