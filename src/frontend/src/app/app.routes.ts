import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/workspace/workspace.page').then((m) => m.WorkspacePageComponent),
  },
  {
    path: 'join',
    loadComponent: () =>
      import('./features/workspace/workspace.page').then((m) => m.WorkspacePageComponent),
  },
  {
    path: 'anonymous',
    loadComponent: () =>
      import('./features/workspace/workspace.page').then((m) => m.WorkspacePageComponent),
  },
  {
    path: 'anonymous/:code',
    loadComponent: () =>
      import('./features/workspace/workspace.page').then((m) => m.WorkspacePageComponent),
  },
  {
    path: 'share/:code',
    loadComponent: () =>
      import('./features/shares/share-details/share-details.page').then(
        (m) => m.ShareDetailsPageComponent,
      ),
  },
  {
    path: 'auth/login',
    loadComponent: () =>
      import('./features/auth/login/login.page').then((m) => m.LoginPageComponent),
  },
  {
    path: 'auth/register',
    loadComponent: () =>
      import('./features/auth/register/register.page').then((m) => m.RegisterPageComponent),
  },
  {
    path: 'me',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auth/me/me.page').then((m) => m.MePageComponent),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
