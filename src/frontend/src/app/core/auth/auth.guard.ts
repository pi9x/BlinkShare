import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { AuthTokenStorage } from './auth-token.storage';

export const authGuard: CanActivateFn = () => {
  const token = inject(AuthTokenStorage).read();
  return token ? true : inject(Router).createUrlTree(['/auth/login']);
};
