import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { AuthStore } from '../auth/auth.store';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="min-h-screen">
      <header class="border-b bg-white" style="border-color: var(--line-default);">
        <div class="mx-auto flex max-w-6xl items-center justify-between gap-3 px-4 py-3 sm:px-6">
          <a routerLink="/" class="flex min-w-0 items-center gap-2.5 sm:gap-3">
            <img
              src="brand/blinkshare-mark.svg"
              alt="BlinkShare"
              class="h-9 w-9 shrink-0 sm:h-[3.2rem] sm:w-[3.2rem]"
            />
            <div class="flex min-h-9 min-w-0 flex-col justify-center sm:min-h-[3.2rem]">
              <span class="truncate text-[1.45rem] leading-none font-bold tracking-[-0.06em] sm:text-[2.2rem]" style="color: var(--text-strong);">
                Blink<span style="color: var(--surface-strong); font-weight: 400;">Share</span>
              </span>
            </div>
          </a>

          <div class="flex shrink-0 items-center justify-end gap-1.5 sm:gap-2">
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
              <a routerLink="/auth/login" class="app-button app-button-secondary h-[2rem] min-w-[4.7rem] px-2.5 py-0 text-[0.9rem] sm:h-[2.15rem] sm:min-w-[5.5rem] sm:px-3 sm:text-[0.95rem]">Login</a>
              <a routerLink="/auth/register" class="app-button app-button-primary h-[2rem] min-w-[5.2rem] px-2.5 py-0 text-[0.9rem] sm:h-[2.15rem] sm:min-w-[5.9rem] sm:px-3 sm:text-[0.95rem]">Register</a>
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
}
