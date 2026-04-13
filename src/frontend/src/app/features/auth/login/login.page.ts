import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { AppError } from '../../../core/errors/app-error.model';
import { toAppError } from '../../../core/http/api-error.mapper';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="mx-auto max-w-3xl">
      <article class="surface-card grid gap-6 p-6 md:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)] md:p-7">
        <div class="space-y-3">
          <span class="inline-flex rounded-full bg-[var(--accent-soft)] px-3 py-1 text-xs font-bold uppercase tracking-[0.18em] text-[var(--surface-strong)]">
            Login
          </span>
          <div class="space-y-2">
            <h2 class="text-2xl font-semibold tracking-tight text-[var(--text-strong)]">Welcome back</h2>
            <p class="section-copy max-w-sm">
              Sign in with your email. If this account has a password, you can add it below before continuing.
            </p>
          </div>
          <div class="surface-panel space-y-2 p-4 text-sm text-[var(--text-muted)]">
            <p class="font-semibold text-[var(--text-strong)]">Flexible sign-in</p>
            <p class="m-0">Use email only for lightweight access, or include your password when needed.</p>
          </div>
        </div>

        <form class="space-y-4" [formGroup]="form" (ngSubmit)="submit()">
          <div>
            <label class="field-label">Email</label>
            <input class="field-input mt-2 h-[2.15rem] px-3 py-0 text-sm leading-[2.15rem]" type="email" formControlName="email" placeholder="dev@example.com" />
          </div>

          <label class="surface-panel flex cursor-pointer items-start gap-3 p-4">
            <input class="mt-0.5 h-4 w-4 accent-[var(--surface-strong)]" type="checkbox" formControlName="usePassword" />
            <span class="space-y-1">
              <span class="block text-sm font-semibold text-[var(--text-strong)]">This account uses a password</span>
              <span class="block text-sm text-[var(--text-muted)]">Check this if you want to sign in with email and password.</span>
            </span>
          </label>

          @if (form.controls.usePassword.value) {
            <div>
              <label class="field-label">Password</label>
              <input class="field-input mt-2 h-[2.15rem] px-3 py-0 text-sm leading-[2.15rem]" type="password" formControlName="password" placeholder="Enter your password" />
            </div>
          }

          @if (error(); as errorState) {
            <p class="field-error">{{ errorState.message }}</p>
          }

          <div class="flex flex-wrap gap-2 pt-1">
            <button class="app-button app-button-primary h-[2.15rem] min-w-28 px-3.5 py-0 text-sm" type="submit" [disabled]="submitting()">
              Sign in
            </button>
            <a class="app-button app-button-secondary h-[2.15rem] px-3.5 py-0 text-sm" routerLink="/auth/register">Create account</a>
          </div>
        </form>
      </article>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly form = this.fb.nonNullable.group({
    email: '',
    usePassword: false,
    password: '',
  });
  protected readonly error = signal<AppError | null>(null);
  protected readonly submitting = signal(false);

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      const { email, password, usePassword } = this.form.getRawValue();
      await this.authService.login(email.trim(), usePassword ? password : null);
      await this.router.navigateByUrl('/me');
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.submitting.set(false);
    }
  }
}
