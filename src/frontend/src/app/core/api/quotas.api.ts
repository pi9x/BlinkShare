import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { QuotaUsageResponse } from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class QuotasApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  public getUsage() {
    return this.http.get<QuotaUsageResponse>(`${this.baseUrl}/api/v1/quotas/usage`);
  }
}
