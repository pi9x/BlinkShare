import { Injectable } from '@angular/core';

import { StoredAnonymousSession } from '../../shared/models/app.models';

const STORAGE_KEY = 'blinkshare.anonymous-session';

@Injectable({ providedIn: 'root' })
export class SessionRestoreStorage {
  public load(): StoredAnonymousSession | null {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as StoredAnonymousSession;
    } catch {
      return null;
    }
  }

  public save(session: StoredAnonymousSession): void {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  public clear(): void {
    sessionStorage.removeItem(STORAGE_KEY);
  }
}
