import { Injectable } from '@angular/core';

import { LocalClipboardItem } from '../../shared/models/app.models';

const STORAGE_KEY = 'blinkshare.history.items';
const MAX_ITEMS = 40;

@Injectable({ providedIn: 'root' })
export class LocalHistoryStorage {
  public load(): LocalClipboardItem[] {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return [];
    }

    try {
      return JSON.parse(raw) as LocalClipboardItem[];
    } catch {
      return [];
    }
  }

  public add(item: LocalClipboardItem): LocalClipboardItem[] {
    const next = [item, ...this.load()].slice(0, MAX_ITEMS);
    this.save(next);
    return next;
  }

  public save(items: LocalClipboardItem[]): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items.slice(0, MAX_ITEMS)));
  }

  public clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  }
}
