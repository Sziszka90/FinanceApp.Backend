import { ApplicationConfig, importProvidersFrom, inject } from '@angular/core';
import { provideRouter, Router } from '@angular/router';

import { routes } from './app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  HttpClient,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
  provideHttpClient,
  withFetch,
  withInterceptors,
} from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideNativeDateAdapter } from '@angular/material/core';
import { catchError, Observable, throwError } from 'rxjs';
import { AuthenticationService } from '../services/authentication.service';
import { ErrorModalComponent } from './error-modal/error-modal.component';
import { MatDialog } from '@angular/material/dialog';

export const provideTranslation = () => ({
  defaultLanguage: 'en',
  loader: {
    provide: TranslateLoader,
    useFactory: HttpLoaderFactory,
    deps: [HttpClient],
  },
});

export const provideAuthInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<any> => {
  const authService = inject(AuthenticationService);

  const clonedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${authService.getToken()}`,
    },
  });

  return next(clonedRequest);
};

export const provideErrorHandlerInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<any> => {
  const matDialog = inject(MatDialog);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: any) => {
      switch (error.status) {
        case error.status === 400:
          matDialog.open(ErrorModalComponent, {
            width: '50rem',
            data: error,
          });
          break;

          case error.status === 404:
            router.navigateByUrl('/not-found')
            break;

        case error.status >= 401 && error.status < 500:
          if (
            error.error?.code === 'INVALID_PASSWORD' ||
            error.error?.code === 'USER_NOT_FOUND'
          ) {
            matDialog.open(ErrorModalComponent, {
              width: '50rem',
              data: error.error ?? error,
            });
          } else {
            router.navigateByUrl('/login');
          }
          break;

        default:
          matDialog.open(ErrorModalComponent, {
            width: '50rem',
            data:  { message: error.error?.message ?? error.message, details: error.error?.details ?? "" },
          });
          break;
      }

      console.error('Error during an HTTP call!', error);
      return throwError(() => error);
    })
  );
};

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/translations/', '.json');
}
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideRouter(routes),
    provideNativeDateAdapter(),
    importProvidersFrom(BrowserAnimationsModule),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideHttpClient(
      withInterceptors([provideAuthInterceptor, provideErrorHandlerInterceptor])
    ),
    importProvidersFrom([TranslateModule.forRoot(provideTranslation())]),
    provideAnimationsAsync(),
  ],
};
