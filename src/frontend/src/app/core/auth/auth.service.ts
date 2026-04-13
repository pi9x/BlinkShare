import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { AuthApi } from '../api/auth.api';
import { toAppError } from '../http/api-error.mapper';
import { AuthStore } from './auth.store';
import { AuthTokenStorage } from './auth-token.storage';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApi);
  private readonly store = inject(AuthStore);
  private readonly tokenStorage = inject(AuthTokenStorage);

  public async bootstrap(): Promise<void> {
    const token = this.tokenStorage.read();
    if (!token) {
      this.store.setAnonymous();
      return;
    }

    this.store.markChecking();

    try {
      const account = await firstValueFrom(this.api.me());
      this.store.setAuthenticated(account);
    } catch (error) {
      const appError = toAppError(error);
      if (appError.kind === 'unauthorized' || appError.kind === 'forbidden') {
        this.tokenStorage.clear();
      }
      this.store.setAnonymous();
    }
  }

  public async register(email: string, password: string | null): Promise<void> {
    const response = await firstValueFrom(
      this.api.register({
        email,
        password,
      }),
    );

    this.tokenStorage.write(response.sessionToken);
    await this.bootstrap();
  }

  public async login(email: string, password: string | null): Promise<void> {
    const response = await firstValueFrom(
      this.api.login({
        email,
        password,
      }),
    );

    this.tokenStorage.write(response.sessionToken);
    await this.bootstrap();
  }

  public logout(): void {
    this.tokenStorage.clear();
    this.store.setAnonymous();
  }
}
