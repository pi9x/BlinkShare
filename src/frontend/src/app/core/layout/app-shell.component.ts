import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { AuthStore } from '../auth/auth.store';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet, StatusBadgeComponent],
  template: `
    <div class="min-h-screen">
      <header class="border-b bg-white" style="border-color: var(--line-default);">
        <div class="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-4 py-3 sm:px-6">
          <a routerLink="/" class="flex items-center gap-3">
            <img
              src="brand/blinkshare-mark.svg"
              alt="BlinkShare"
              class="h-12 w-12 shrink-0 sm:h-[3.2rem] sm:w-[3.2rem]"
            />
            <div class="flex min-h-12 flex-col justify-center sm:min-h-[3.2rem]">
              <span class="text-[2rem] leading-none font-bold tracking-[-0.06em] sm:text-[2.2rem]" style="color: var(--text-strong);">
                Blink<span style="color: var(--surface-strong); font-weight: 400;">Share</span>
              </span>
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
              <a routerLink="/auth/login" class="app-button app-button-secondary h-[2.15rem] px-3.5 py-0 text-[0.95rem]">Login</a>
              <a routerLink="/auth/register" class="app-button app-button-primary h-[2.15rem] px-3.5 py-0 text-[0.95rem]">Register</a>
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

  protected readonly authLabel = computed(() =>
    this.authStore.authenticated() ? 'ACCOUNT' : this.authStore.status() === 'checking' ? 'CHECKING SESSION' : 'ANONYMOUS MODE',
  );

  protected readonly authTone = computed<'neutral' | 'success'>(() =>
    this.authStore.authenticated() ? 'success' : 'neutral',
  );
}
