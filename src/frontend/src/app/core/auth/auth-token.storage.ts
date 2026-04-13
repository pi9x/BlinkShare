import { Injectable } from '@angular/core';

const STORAGE_KEY = 'blinkshare.auth.session-token';

@Injectable({ providedIn: 'root' })
export class AuthTokenStorage {
  public read(): string | null {
    return localStorage.getItem(STORAGE_KEY);
  }

  public write(token: string): void {
    localStorage.setItem(STORAGE_KEY, token);
  }

  public clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  }
}
