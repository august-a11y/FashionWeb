import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { delay, filter, map, tap } from 'rxjs/operators';
import { ColorModeService } from '@coreui/angular';
import { IconSetService } from '@coreui/icons-angular';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { iconSubset } from './icons/icon-subset';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-root',
    template: `
        <p-toast />
        <router-outlet />
    `,
    imports: [RouterOutlet, ToastModule, ButtonModule, RippleModule]
})
export class AppComponent implements OnInit {
  title = 'CoreUI Angular Admin Template';

  readonly #destroyRef: DestroyRef = inject(DestroyRef);
  readonly #activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  readonly #router = inject(Router);
  readonly #titleService = inject(Title);

  readonly #colorModeService = inject(ColorModeService);
  readonly #iconSetService = inject(IconSetService);
  readonly #messageService = inject(MessageService);

  constructor() {
    this.#titleService.setTitle(this.title);
    // iconSet singleton
    this.#iconSetService.icons = { ...iconSubset };
    this.#colorModeService.localStorageItemName.set('coreui-free-angular-admin-template-theme-default');
    this.#colorModeService.eventName.set('ColorSchemeChange');
  }

  ngOnInit(): void {

    this.#router.events.pipe(
        takeUntilDestroyed(this.#destroyRef)
      ).subscribe((evt) => {
      if (!(evt instanceof NavigationEnd)) {
        return;
      }
    });

    this.#activatedRoute.queryParams
      .pipe(
        delay(1),
        map(params => <string>params['theme']?.match(/^[A-Za-z0-9\s]+/)?.[0]),
        filter(theme => ['dark', 'light', 'auto'].includes(theme)),
        tap(theme => {
          this.#colorModeService.colorMode.set(theme);
        }),
        takeUntilDestroyed(this.#destroyRef)
      )
      .subscribe();
  }

  showSuccess(): void {
    this.#messageService.add({ severity: 'success', summary: 'Success', detail: 'Message Content' });
  }

  showInfo(): void {
    this.#messageService.add({ severity: 'info', summary: 'Info', detail: 'Message Content' });
  }

  showWarn(): void {
    this.#messageService.add({ severity: 'warn', summary: 'Warn', detail: 'Message Content' });
  }

  showError(): void {
    this.#messageService.add({ severity: 'error', summary: 'Error', detail: 'Message Content' });
  }

  showSecondary(): void {
    this.#messageService.add({ severity: 'secondary', summary: 'Secondary', detail: 'Message Content' });
  }

  showContrast(): void {
    this.#messageService.add({ severity: 'contrast', summary: 'Contrast', detail: 'Message Content' });
  }
}
