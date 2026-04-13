import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { APP_RUNTIME_CONFIG } from '../config/app-runtime-config';
import { AuthService } from '../auth/auth.service';
import { AuthStore } from '../auth/auth.store';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, StatusBadgeComponent],
  template: `
    <div class="min-h-screen">
      <header class="border-b border-slate-200 bg-white">
        <div class="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-6">
          <a routerLink="/" class="flex items-center gap-3">
            <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-slate-950 text-base font-semibold text-white">
                B
            </div>
            <div>
              <h1 class="text-lg font-semibold tracking-tight text-slate-950">{{ appName }}</h1>
              <p class="text-xs text-slate-500">Copy. Connect. Send.</p>
            </div>
          </a>

          <div class="flex flex-wrap items-center gap-2">
            <app-status-badge [label]="authLabel()" [tone]="authTone()" />
            @if (authStore.authenticated()) {
              <a
                routerLink="/me"
                routerLinkActive="icon-button-active"
                class="icon-button"
                aria-label="Account"
                title="Account"
              >
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M20 21a8 8 0 0 0-16 0" />
                  <circle cx="12" cy="8" r="4" />
                </svg>
              </a>
              <button class="icon-button" type="button" (click)="authService.logout()" aria-label="Logout" title="Logout">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                  <path d="m16 17 5-5-5-5" />
                  <path d="M21 12H9" />
                </svg>
              </button>
            } @else {
              <a routerLink="/auth/login" class="app-button app-button-secondary px-3 py-2 text-sm">Login</a>
              <a routerLink="/auth/register" class="app-button app-button-primary px-3 py-2 text-sm">Register</a>
            }
          </div>
        </div>
      </header>

      <main class="mx-auto max-w-6xl px-4 py-4 sm:px-6">
        <router-outlet />
      </main>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  protected readonly authStore = inject(AuthStore);
  protected readonly authService = inject(AuthService);
  protected readonly appName = APP_RUNTIME_CONFIG.appName;

  protected readonly authLabel = computed(() =>
    this.authStore.authenticated() ? 'Account ready' : this.authStore.status() === 'checking' ? 'Checking session' : 'Anonymous mode',
  );

  protected readonly authTone = computed<'neutral' | 'success'>(() =>
    this.authStore.authenticated() ? 'success' : 'neutral',
  );
}
