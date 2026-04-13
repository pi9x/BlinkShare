import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { AuthStore } from '../../../core/auth/auth.store';
import { QuotasApi } from '../../../core/api/quotas.api';
import { AppError } from '../../../core/errors/app-error.model';
import { toAppError } from '../../../core/http/api-error.mapper';
import { QuotaUsageResponse } from '../../../shared/models/app.models';
import { formatBytes } from '../../../shared/utils/bytes';
import { formatDateTime } from '../../../shared/utils/time';

@Component({
  selector: 'app-me-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid gap-4 lg:grid-cols-2">
      <article class="surface-card rounded-[1.5rem] p-4">
        <p class="text-sm font-semibold text-slate-950">Account</p>
        @if (authStore.account(); as account) {
          <h2 class="mt-1 text-2xl font-semibold tracking-tight text-slate-950">{{ account.email }}</h2>

          <div class="mt-4 grid gap-2">
            <div class="surface-panel rounded-xl p-3">
              <p class="text-xs uppercase tracking-[0.18em] text-slate-500">ID</p>
              <p class="mt-1 font-code text-sm text-slate-900">{{ account.accountId }}</p>
            </div>
            <div class="surface-panel rounded-xl p-3">
              <p class="text-xs uppercase tracking-[0.18em] text-slate-500">Created</p>
              <p class="mt-1 text-sm font-semibold text-slate-900">{{ formatDateTime(account.createdAtUtc) }}</p>
            </div>
          </div>
        } @else {
          <p class="mt-3 text-sm text-slate-600">No authenticated account is available in the current store state.</p>
        }
      </article>

      <article class="surface-card rounded-[1.5rem] p-4">
        <p class="text-sm font-semibold text-slate-950">Quota</p>
        @if (quota(); as quotaState) {
          <h2 class="mt-1 text-2xl font-semibold tracking-tight text-slate-950">{{ formatBytes(quotaState.bytesUsedToday) }} / {{ formatBytes(quotaState.bytesLimitToday) }}</h2>

          <div class="mt-4 grid gap-2">
            <div class="surface-panel rounded-xl p-3">
              <p class="text-xs uppercase tracking-[0.18em] text-slate-500">Tier</p>
              <p class="mt-1 text-sm font-semibold text-slate-900">{{ quotaState.tier === 2 ? 'Free' : quotaState.tier }}</p>
            </div>
            <div class="surface-panel rounded-xl p-3">
              <p class="text-xs uppercase tracking-[0.18em] text-slate-500">Shares</p>
              <p class="mt-1 text-sm font-semibold text-slate-900">{{ quotaState.sharesCreatedToday }}</p>
            </div>
            <div class="surface-panel rounded-xl p-3">
              <p class="text-xs uppercase tracking-[0.18em] text-slate-500">Resets</p>
              <p class="mt-1 text-sm font-semibold text-slate-900">{{ formatDateTime(quotaState.windowEndsAtUtc) }}</p>
            </div>
          </div>
        } @else if (error(); as errorState) {
          <p class="mt-4 rounded-2xl bg-rose-50 px-4 py-3 text-sm leading-6 text-rose-800">{{ errorState.message }}</p>
        } @else {
          <p class="mt-3 text-sm text-slate-600">Loading quota usage…</p>
        }
      </article>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MePageComponent {
  private readonly quotasApi = inject(QuotasApi);
  protected readonly authStore = inject(AuthStore);
  protected readonly quota = signal<QuotaUsageResponse | null>(null);
  protected readonly error = signal<AppError | null>(null);

  public constructor() {
    void this.loadQuota();
  }

  private async loadQuota(): Promise<void> {
    try {
      this.quota.set(await firstValueFrom(this.quotasApi.getUsage()));
    } catch (error) {
      this.error.set(toAppError(error));
    }
  }

  protected readonly formatBytes = formatBytes;
  protected readonly formatDateTime = formatDateTime;
}
