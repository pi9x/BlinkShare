import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import {
  AuthSessionResponse,
  CurrentAccountResponse,
  LoginRequest,
  RegisterRequest,
} from '../../shared/models/app.models';
import { API_BASE_URL } from '../http/api-base-url.token';

@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  public register(request: RegisterRequest) {
    return this.http.post<AuthSessionResponse>(`${this.baseUrl}/api/v1/auth/register`, request);
  }

  public login(request: LoginRequest) {
    return this.http.post<AuthSessionResponse>(`${this.baseUrl}/api/v1/auth/login`, request);
  }

  public me() {
    return this.http.get<CurrentAccountResponse>(`${this.baseUrl}/api/v1/auth/me`);
  }
}
