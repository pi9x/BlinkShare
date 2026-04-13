import { Injectable } from '@angular/core';

import { EditorDraft } from '../../shared/models/app.models';

const STORAGE_KEY = 'blinkshare.workspace.draft';

const DEFAULT_DRAFT: EditorDraft = {
  text: '',
  language: 'plaintext',
  wrap: true,
  fullscreen: false,
};

@Injectable({ providedIn: 'root' })
export class DraftStorage {
  public load(): EditorDraft {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return DEFAULT_DRAFT;
    }

    try {
      return {
        ...DEFAULT_DRAFT,
        ...(JSON.parse(raw) as Partial<EditorDraft>),
      };
    } catch {
      return DEFAULT_DRAFT;
    }
  }

  public save(draft: EditorDraft): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(draft));
  }

  public clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  }
}
