import { registerLocaleData } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import localeEsCl from '@angular/common/locales/es-CL';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';

// Formato chileno para montos (ej: 1.010,00).
registerLocaleData(localeEsCl);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    { provide: LOCALE_ID, useValue: 'es-CL' },
  ],
};