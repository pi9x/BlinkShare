import { Injectable, computed, signal } from '@angular/core';

import { CurrentAccountResponse } from '../../shared/models/app.models';

type AuthStatus = 'checking' | 'anonymous' | 'authenticated';

interface AuthState {
  status: AuthStatus;
  account: CurrentAccountResponse | null;
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly state = signal<AuthState>({
    status: 'checking',
    account: null,
  });

  public readonly snapshot = computed(() => this.state());
  public readonly status = computed(() => this.state().status);
  public readonly account = computed(() => this.state().account);
  public readonly authenticated = computed(() => this.state().status === 'authenticated');

  public markChecking(): void {
    this.state.update((state) => ({ ...state, status: 'checking' }));
  }

  public setAuthenticated(account: CurrentAccountResponse): void {
    this.state.set({
      status: 'authenticated',
      account,
    });
  }

  public setAnonymous(): void {
    this.state.set({
      status: 'anonymous',
      account: null,
    });
  }
}
