import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { SharesApi } from '../../../core/api/shares.api';
import { LocalHistoryStorage } from '../../../core/storage/local-history.storage';
import { AppError } from '../../../core/errors/app-error.model';
import { toAppError } from '../../../core/http/api-error.mapper';
import { CodeEditorComponent } from '../../../shared/ui/code-editor/code-editor.component';
import { EmptyStateComponent } from '../../../shared/ui/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../shared/ui/status-badge/status-badge.component';
import { GetShareByCodeResponse, ReadTextResponse, ShareKind } from '../../../shared/models/app.models';
import { copyText } from '../../../shared/utils/clipboard';
import { formatBytes } from '../../../shared/utils/bytes';
import { formatDateTime, formatRelativeTime } from '../../../shared/utils/time';

const unlockStorageKey = (code: string) => `blinkshare.unlock.${code}`;

@Component({
  selector: 'app-share-details-page',
  standalone: true,
  imports: [
    CommonModule,
    CodeEditorComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  template: `
    <div class="grid gap-4 lg:grid-cols-[minmax(0,1fr)_18rem]">
      <section class="space-y-4">
        <article class="surface-card rounded-[1.5rem] p-4">
          <div class="flex flex-wrap items-start justify-between gap-3">
            <div>
              <p class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Share</p>
              <h2 class="mt-1 font-code text-3xl font-semibold tracking-[0.24em] text-slate-950">{{ code }}</h2>
            </div>
            <app-status-badge [label]="shareStatusLabel()" [tone]="statusTone()" />
          </div>

          @if (error(); as errorState) {
            <p class="mt-3 rounded-xl bg-rose-50 px-3 py-2 text-sm text-rose-800">{{ errorState.message }}</p>
          }

          @if (share(); as shareState) {
            <div class="mt-3 flex flex-wrap gap-2 text-sm text-slate-500">
              <span>{{ shareState.kind === shareKind.Text ? 'Text' : 'File' }}</span>
              <span>·</span>
              <span>{{ shareState.hasPasscode ? 'Locked' : 'Open' }}</span>
              <span>·</span>
              <span>{{ formatRelativeTime(shareState.expiresAtUtc) }}</span>
            </div>
          }
        </article>

        @if (requiresUnlock()) {
          <article class="surface-card rounded-[1.5rem] p-4">
            <p class="mb-2 text-sm font-semibold text-slate-950">Passcode</p>
            <div class="flex flex-wrap gap-2">
              <input
                class="field-input max-w-xl"
                [value]="passcode()"
                (input)="passcode.set(($any($event.target).value || ''))"
                placeholder="Passcode"
              />
              <button class="app-button app-button-primary" type="button" (click)="unlock()" [disabled]="loading()">
                Unlock
              </button>
            </div>
          </article>
        }

        @if (textContent(); as textState) {
          <article class="surface-card rounded-[1.5rem] p-4">
            <div class="mb-3 flex flex-wrap items-center justify-between gap-3">
              <p class="text-sm font-semibold text-slate-950">Text</p>
              <button class="app-button app-button-secondary" type="button" (click)="copy(textState.text)">
                {{ copied() ? 'Copied' : 'Copy' }}
              </button>
            </div>
            <app-code-editor [value]="textState.text" [readOnly]="true" [wrap]="true" />
          </article>
        } @else if (share()?.kind === shareKind.Text && !requiresUnlock()) {
          <article class="surface-card rounded-[1.5rem] p-4">
            <button class="app-button app-button-primary" type="button" (click)="readText()" [disabled]="loading()">
              Read text
            </button>
          </article>
        }

        @if (share()?.kind === shareKind.File && !requiresUnlock()) {
          <article class="surface-card rounded-[1.5rem] p-4">
            <button class="app-button app-button-primary" type="button" (click)="requestDownload()" [disabled]="loading()">
              Download
            </button>
          </article>
        }
      </section>

      <aside class="space-y-4">
        <article class="surface-card rounded-[1.5rem] p-4">
          <p class="text-sm font-semibold text-slate-950">Info</p>
          @if (share(); as shareState) {
            <div class="mt-3 space-y-2 text-sm text-slate-600">
              <div class="surface-panel rounded-xl px-3 py-2">{{ shareStatusLabel() }}</div>
              <div class="surface-panel rounded-xl px-3 py-2">{{ formatBytes(shareState.sizeBytes) }}</div>
              <div class="surface-panel rounded-xl px-3 py-2">{{ formatDateTime(shareState.expiresAtUtc) }}</div>
            </div>
          } @else {
            <app-empty-state eyebrow="Loading" title="Checking share" copy="" />
          }
        </article>
      </aside>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShareDetailsPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly sharesApi = inject(SharesApi);
  private readonly historyStorage = inject(LocalHistoryStorage);

  protected readonly code = this.route.snapshot.paramMap.get('code') ?? '';
  protected readonly shareKind = ShareKind;
  protected readonly share = signal<GetShareByCodeResponse | null>(null);
  protected readonly textContent = signal<ReadTextResponse | null>(null);
  protected readonly error = signal<AppError | null>(null);
  protected readonly loading = signal(false);
  protected readonly passcode = signal('');
  protected readonly copied = signal(false);
  protected readonly unlockProof = signal<string | null>(sessionStorage.getItem(unlockStorageKey(this.code)));

  private copiedHandle: ReturnType<typeof setTimeout> | null = null;

  public constructor() {
    void this.loadShare();
  }

  protected requiresUnlock(): boolean {
    return !!this.share()?.hasPasscode && !this.unlockProof();
  }

  protected shareStatusLabel(): string {
    const shareState = this.share();
    if (!shareState) {
      return 'Loading';
    }

    switch (shareState.status) {
      case 1:
        return 'Pending';
      case 2:
        return 'Ready';
      case 3:
        return 'Expired';
      case 4:
        return 'Deleted';
      default:
        return 'Unknown';
    }
  }

  protected statusTone(): 'neutral' | 'success' | 'warning' | 'danger' {
    const shareState = this.share();
    if (!shareState) {
      return 'neutral';
    }

    if (shareState.status === 2) {
      return 'success';
    }

    if (shareState.status === 1) {
      return 'warning';
    }

    if (shareState.status >= 3) {
      return 'danger';
    }

    return 'neutral';
  }

  protected async unlock(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(
        this.sharesApi.unlock(this.code, {
          passcode: this.passcode(),
        }),
      );

      this.unlockProof.set(response.unlockProof);
      sessionStorage.setItem(unlockStorageKey(this.code), response.unlockProof);

      if (this.share()?.kind === ShareKind.Text) {
        await this.readText();
      }
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async readText(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.sharesApi.readText(this.code, this.unlockProof()));
      this.textContent.set(response);

      this.historyStorage.add({
        id: crypto.randomUUID(),
        direction: 'received',
        kind: 'text',
        text: response.text,
        language: 'plaintext',
        createdAtUtc: new Date().toISOString(),
        shareCode: this.code,
      });
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async requestDownload(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.sharesApi.requestDownload(this.code, this.unlockProof()));
      window.open(response.downloadUrl, '_blank', 'noopener');
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected async copy(value: string): Promise<void> {
    const copied = await copyText(value);
    if (!copied) {
      return;
    }

    this.copied.set(true);

    if (this.copiedHandle) {
      clearTimeout(this.copiedHandle);
    }

    this.copiedHandle = setTimeout(() => {
      this.copied.set(false);
    }, 1200);
  }

  private async loadShare(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const response = await firstValueFrom(this.sharesApi.getByCode(this.code));
      this.share.set(response);
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.loading.set(false);
    }
  }

  protected readonly formatBytes = formatBytes;
  protected readonly formatDateTime = formatDateTime;
  protected readonly formatRelativeTime = formatRelativeTime;
}
