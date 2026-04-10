import { Routes } from '@angular/router';


export const catalog_Routes: Routes = [
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  },
  {
    path: 'products',
    loadComponent: () => import('./products/product-list/product-list.component').then(m => m.ProductListComponent)
  },
  {
    path: 'catagories',
    loadComponent: () => import('./catagories/catagories.component').then(m => m.CatagoriesComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./orders/order-management.component').then(m => m.OrderManagementComponent)
  },
  {
    path: 'product/:id',
    loadComponent: () => import('./products/product-detail/product-detail.component').then(m => m.ProductDetailComponent),

    children: [
      {
        path: '',
        redirectTo: 'info',
        pathMatch: 'full'
      },
      {
        path: 'info',
        loadComponent: () => import('./products/product-detail/product-info/product-info.component').then(m => m.ProductInfoComponent)
      },
      {
        path: 'variants',
        loadComponent: () => import('./products/product-detail/product-variants/product-variants.component').then(m => m.ProductVariantsComponent)
      }

    ]
  }

];

