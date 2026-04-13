import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';

import { authInterceptor } from './core/auth/auth.interceptor';
import { API_BASE_URL } from './core/http/api-base-url.token';
import { APP_RUNTIME_CONFIG } from './core/config/app-runtime-config';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideRouter(
      routes,
      withInMemoryScrolling({
        anchorScrolling: 'enabled',
        scrollPositionRestoration: 'enabled',
      }),
    ),
    {
      provide: API_BASE_URL,
      useValue: APP_RUNTIME_CONFIG.apiBaseUrl,
    },
  ],
};
