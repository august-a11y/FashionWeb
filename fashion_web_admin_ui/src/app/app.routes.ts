import { Routes } from '@angular/router';
import { authChildGuard, authGuard, guestOnlyGuard } from './shared/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    redirectTo: 'auth/login',
    pathMatch: 'full'
  },
  {
    path: 'register',
    redirectTo: 'auth/register',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    canMatch: [guestOnlyGuard],
    loadChildren: () => import('./views/auth/auth.routes').then(m => m.auth_Routes)
  },
  {
    path: 'main_page',
    loadComponent: () => import('./views/main_page/main_page.component').then((m) => m.MainPageComponent),
    data: {
      title: 'Main Page'
    }
  },
  {
    path: '',
    loadComponent: () => import('./layout').then(m => m.DefaultLayoutComponent),
    canActivate: [authGuard],
    canActivateChild: [authChildGuard],
    data: {
      title: 'Home'
    },
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./views/dashboard/dashboard.routes').then((m) => m.dashboard_Routes)
      },
      {
        path: 'system',
        loadChildren: () => import('./views/system/system.routes').then((m) => m.system_Routes)
      },
      {
        path: 'catalog',
        loadChildren: () => import('./views/catalog/catalog.routes').then((m) => m.catalog_Routes)
      }

    ]
  },
  
  {
    path: '404',
    loadComponent: () => import('./views/auth/page404/page404.component').then(m => m.Page404Component),
    data: {
      title: 'Page 404'
    }
  },
  {
    path: '500',
    loadComponent: () => import('./views/auth/page500/page500.component').then(m => m.Page500Component),
    data: {
      title: 'Page 500'
    }
  },

  { path: '**', redirectTo: 'dashboard' }
];
