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
    <section class="mx-auto max-w-2xl">
      <article class="surface-card rounded-[1.5rem] p-6">
        <h2 class="text-2xl font-semibold tracking-tight text-slate-950">Login</h2>

        <form class="mt-5 space-y-4" [formGroup]="form" (ngSubmit)="submit()">
          <div>
            <label class="field-label">Email</label>
            <input class="field-input mt-2" type="email" formControlName="email" placeholder="dev@example.com" />
          </div>

          <div>
            <label class="field-label">Password</label>
            <input class="field-input mt-2" type="password" formControlName="password" placeholder="Optional by backend rules" />
          </div>

          @if (error(); as errorState) {
            <p class="field-error">{{ errorState.message }}</p>
          }

          <div class="flex flex-wrap gap-2">
            <button class="app-button app-button-primary" type="submit" [disabled]="submitting()">
              Login
            </button>
            <a class="app-button app-button-secondary" routerLink="/auth/register">Register</a>
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
    password: '',
  });
  protected readonly error = signal<AppError | null>(null);
  protected readonly submitting = signal(false);

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.authService.login(this.form.getRawValue().email, this.form.getRawValue().password);
      await this.router.navigateByUrl('/me');
    } catch (error) {
      this.error.set(toAppError(error));
    } finally {
      this.submitting.set(false);
    }
  }
}
