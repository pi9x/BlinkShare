import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { AuthService } from './core/auth/auth.service';
import { AppShellComponent } from './core/layout/app-shell.component';

@Component({
  selector: 'app-root',
  imports: [AppShellComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly authService = inject(AuthService);

  public constructor() {
    void this.authService.bootstrap();
  }
}
