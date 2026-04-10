import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  AddressDTO,
  AdminApiCartApiClient,
  AdminApiCategoryApiClient,
  AdminApiOrderApiClient,
  AdminApiProductsApiClient,
  AdminApiVariantsApiClient,
  CartItemDTO,
  CartItemCreateDTO,
  CartItemRemoveDTO,
  CartItemUpdateDTO,
  CategoryDTO,
  CreateOrderDTO,
  OrderItemDTO,
  PaymentMethod,
  ProductResponseDTO,
  ProductResponseDTOPageResponseApiResponse,
  VariantDTO
} from '../../api/admin-api.service.generated';
import { AlertService } from '../../shared/services/alert.service';
import { AuthSessionService } from '../../shared/services/auth-session.service';
import { DataCacheService } from '../../shared/services/data-cache.service';

interface MainCategoryView {
  id: string;
  name: string;
  description: string;
  visualClass: string;
}

interface MainProductView {
  id: string;
  name: string;
  description: string;
  price: number;
  thumbnailUrl: string;
  categoryId?: string;
}

interface MainVariantView {
  id: string;
  size: string;
  color: string;
  price: number;
  stockQuantity: number;
}

interface MainCartItemView {
  id: string;
  productId?: string;
  variantId?: string;
  productName: string;
  color: string;
  size: string;
  price: number;
  quantity: number;
  lineTotal: number;
}

interface CheckoutPaymentOption {
  value: PaymentMethod;
  label: string;
}

@Component({
  selector: 'app-main-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './main_page.component.html',
  styleUrls: ['./main_page.component.scss']
})
export class MainPageComponent implements AfterViewInit {
  @ViewChild('categoryProductsTop') categoryProductsTop?: ElementRef<HTMLElement>;

  categories: MainCategoryView[] = [];
  parentCategories: { ao: MainCategoryView | null; quan: MainCategoryView | null } = { ao: null, quan: null };
  menuCategories: { ao: MainCategoryView[]; quan: MainCategoryView[] } = { ao: [], quan: [] };
  categoryProducts: MainProductView[] = [];
  featuredProducts: MainProductView[] = [];
  heroProduct: MainProductView | null = null;
  variantOptions: MainVariantView[] = [];
  cartItems: MainCartItemView[] = [];
  selectedProductForCart: MainProductView | null = null;

  activeCategoryMenu: 'ao' | 'quan' | null = null;

  selectedCategoryId = 'all';
  hasChosenCategory = false;
  isVariantPickerOpen = false;
  isCartPanelOpen = false;
  isCheckoutFormOpen = false;
  isLoadingCart = false;
  isPlacingOrder = false;
  cartPanelNotice = '';
  cartPanelNoticeType: 'success' | 'error' | 'info' = 'info';
  isLoadingVariants = false;
  isLoadingMenuCategories = false;
  isLoadingCategoryProducts = false;
  isLoadingFeaturedProducts = false;

  categoryPageIndex = 1;
  readonly categoryPageSize = 16;
  categoryTotalPages = 1;

  readonly featuredLimit = 8;
  readonly menuHideDelayMs = 1000;
  readonly cartQuantityStep = 1;
  readonly checkoutPaymentOptions: CheckoutPaymentOption[] = [
    { value: PaymentMethod.COD, label: 'Thanh toan khi nhan hang (COD)' },
    { value: PaymentMethod.CreditCard, label: 'The tin dung' },
    { value: PaymentMethod.Momo, label: 'Vi MoMo' },
    { value: PaymentMethod.PayPal, label: 'PayPal' }
  ];

  checkoutForm = {
    fullName: '',
    phoneNumber: '',
    streetLine: '',
    ward: '',
    district: '',
    city: '',
    note: '',
    paymentMethod: PaymentMethod.COD as PaymentMethod
  };

  cartTotalItems = 0;
  cartTotalPrice = 0;

  private readonly cacheTtlMs = 3 * 60 * 1000;
  private activeCartVariantId: string | null = null;
  private readonly addingProductIds = new Set<string>();
  private hideMenuTimer: ReturnType<typeof setTimeout> | null = null;
  private categoriesRequestId = 0;
  private parentCategoriesRequestId = 0;
  private menuCategoriesRequestId = 0;
  private categoryProductsRequestId = 0;
  private variantsRequestId = 0;
  private cartRequestId = 0;
  private featuredProductsRequestId = 0;

  constructor(
    private cartApi: AdminApiCartApiClient,
    private categoryApi: AdminApiCategoryApiClient,
    private orderApi: AdminApiOrderApiClient,
    private productApi: AdminApiProductsApiClient,
    private variantsApi: AdminApiVariantsApiClient,
    private dataCacheService: DataCacheService,
    private alertService: AlertService,
    private cdr: ChangeDetectorRef,
    private authSessionService: AuthSessionService,
    private router: Router
  ) {}

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadCategories();
      this.loadParentCategories();
      this.loadHeroProduct();
      this.loadFeaturedProducts();
      this.loadCartItems();
    }, 0);
  }

  selectCategory(categoryId: string): void {
    if (this.isLoadingCategoryProducts) {
      return;
    }

    this.hasChosenCategory = true;
    this.selectedCategoryId = categoryId;
    this.categoryPageIndex = 1;
    this.loadCategoryProducts(categoryId, this.categoryPageIndex);
  }

  goToCategoryPage(pageIndex: number): void {
    if (
      this.isLoadingCategoryProducts
      || pageIndex < 1
      || pageIndex > this.categoryTotalPages
      || pageIndex === this.categoryPageIndex
    ) {
      return;
    }

    this.categoryPageIndex = pageIndex;
    this.scrollToCategoryProductsTop();
    this.loadCategoryProducts(this.selectedCategoryId, pageIndex);
  }

  openCategoryMenu(menu: 'ao' | 'quan'): void {
    this.cancelCloseCategoryMenu();
    this.activeCategoryMenu = this.activeCategoryMenu === menu ? null : menu;
    if (!this.activeCategoryMenu) {
      return;
    }
    if (this.parentCategories[menu]?.id) {
      this.loadMenuCategories(menu);
      return;
    }
    this.loadParentCategories(menu);
  }

  selectMenuCategory(categoryId: string): void {
    this.selectCategory(categoryId);
    this.closeCategoryMenuNow();
  }

  closeCategoryMenu(): void {
    this.scheduleCloseCategoryMenu();
  }

  scheduleCloseCategoryMenu(): void {
    if (this.hideMenuTimer) {
      clearTimeout(this.hideMenuTimer);
    }

    this.hideMenuTimer = setTimeout(() => {
      this.activeCategoryMenu = null;
      this.hideMenuTimer = null;
    }, this.menuHideDelayMs);
  }

  cancelCloseCategoryMenu(): void {
    if (this.hideMenuTimer) {
      clearTimeout(this.hideMenuTimer);
      this.hideMenuTimer = null;
    }
  }

  closeCategoryMenuNow(): void {
    this.cancelCloseCategoryMenu();
    this.activeCategoryMenu = null;
  }

  private scrollToCategoryProductsTop(): void {
    this.categoryProductsTop?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  getParentMenuLabel(menu: 'ao' | 'quan'): string {
    if (menu === 'ao') {
      return this.parentCategories.ao?.name ?? 'Ao';
    }
    return this.parentCategories.quan?.name ?? 'Quan';
  }

  get hasCategoryPagination(): boolean {
    return this.categoryTotalPages > 1;
  }

  get categoryPageNumbers(): number[] {
    return Array.from({ length: this.categoryTotalPages }, (_value, index) => index + 1);
  }

  get cartBadgeLabel(): string {
    if (this.cartTotalItems <= 0) {
      return '0';
    }

    return this.cartTotalItems > 99 ? '99+' : String(this.cartTotalItems);
  }

  trackProduct(_index: number, product: MainProductView): string {
    return product.id;
  }

  trackVariant(_index: number, variant: MainVariantView): string {
    return variant.id;
  }

  trackCartItem(_index: number, item: MainCartItemView): string {
    return item.id;
  }

  openCartPanel(): void {
    this.isCartPanelOpen = true;
    this.loadCartItems(true);
  }

  onProfileClick(): void {
    if (!this.authSessionService.isAuthenticated()) {
      void this.router.navigate(['/auth/login'], {
        queryParams: {
          returnUrl: this.router.url
        }
      });
      return;
    }

    void this.router.navigate(['/system/user']);
  }

  closeCartPanel(): void {
    this.isCartPanelOpen = false;
    this.isCheckoutFormOpen = false;
    this.cartPanelNotice = '';
  }

  get isCheckoutFormReady(): boolean {
    return this.checkoutForm.fullName.trim().length > 0
      && this.checkoutForm.phoneNumber.trim().length > 0
      && this.checkoutForm.streetLine.trim().length > 0
      && this.checkoutForm.ward.trim().length > 0
      && this.checkoutForm.district.trim().length > 0
      && this.checkoutForm.city.trim().length > 0;
  }

  beginCheckout(): void {
    if (this.isPlacingOrder) {
      return;
    }

    if (this.cartItems.length === 0) {
      this.setCartPanelNotice('Gio hang dang trong, khong the tao don.', 'error');
      return;
    }

    this.isCheckoutFormOpen = true;
    this.cartPanelNotice = '';
  }

  cancelCheckout(): void {
    this.isCheckoutFormOpen = false;
  }

  submitCheckout(): void {
    if (this.isPlacingOrder) {
      return;
    }

    if (this.cartItems.length === 0) {
      this.setCartPanelNotice('Gio hang dang trong, khong the tao don.', 'error');
      return;
    }

    const fullName = this.checkoutForm.fullName.trim();
    const phoneNumber = this.checkoutForm.phoneNumber.trim();
    const streetLine = this.checkoutForm.streetLine.trim();
    const ward = this.checkoutForm.ward.trim();
    const district = this.checkoutForm.district.trim();
    const city = this.checkoutForm.city.trim();
    const note = this.checkoutForm.note.trim();

    if (!fullName || !phoneNumber || !streetLine || !ward || !district || !city) {
      this.setCartPanelNotice('Vui long nhap day du ho ten, so dien thoai va 4 vi tri dia chi.', 'error');
      return;
    }

    if (!/^[0-9+]{9,15}$/.test(phoneNumber)) {
      this.setCartPanelNotice('So dien thoai khong hop le.', 'error');
      return;
    }

    const orderItems = this.cartItems
      .filter(item => !!item.productId && !!item.variantId && item.quantity > 0)
      .map(item => new OrderItemDTO({
        productId: item.productId,
        variantId: item.variantId,
        quantity: item.quantity,
        unitPrice: item.price,
        productName: item.productName,
        color: item.color,
        size: item.size,
        lineTotal: item.lineTotal
      }));

    if (orderItems.length === 0) {
      this.setCartPanelNotice('Du lieu gio hang khong hop le de tao don.', 'error');
      return;
    }

    const payload = new CreateOrderDTO({
      orderItems,
      shippingAddress: new AddressDTO({
        fullName,
        phoneNumber,
        streetLine,
        ward,
        district,
        city
      }),
      phoneNumber,
      note: note || undefined,
      paymentMethod: this.checkoutForm.paymentMethod
    });

    this.isPlacingOrder = true;
    this.orderApi.ordersPOST(payload)
      .pipe(finalize(() => {
        this.isPlacingOrder = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.setCartPanelNotice(response.message ?? 'Khong the tao don hang.', 'error');
            return;
          }

          this.setCartPanelNotice(response?.message ?? 'Tao don hang thanh cong.', 'success');
          this.dataCacheService.invalidateByPrefix('main:cart:me');
          this.isCheckoutFormOpen = false;
          this.loadCartItems(true);
          this.checkoutForm.note = '';
        },
        error: () => {
          this.setCartPanelNotice('Khong the tao don hang.', 'error');
        }
      });
  }

  isAddingToCart(productId?: string): boolean {
    if (!productId) {
      return false;
    }
    return this.addingProductIds.has(productId);
  }

  isAddingVariant(variantId: string): boolean {
    return this.activeCartVariantId === variantId;
  }

  onAddToCart(product: MainProductView): void {
    if (!product.id) {
      this.setCartPanelNotice('San pham khong hop le de them vao gio.', 'error');
      this.openCartPanel();
      return;
    }

    this.selectedProductForCart = product;
    this.variantOptions = [];
    this.activeCartVariantId = null;
    this.isVariantPickerOpen = true;
    this.loadVariantsByProduct(product.id);
  }

  closeVariantPicker(): void {
    this.isVariantPickerOpen = false;
    this.isLoadingVariants = false;
    this.variantOptions = [];
    this.selectedProductForCart = null;
    this.activeCartVariantId = null;
  }

  selectVariantForCart(variant: MainVariantView): void {
    if (!this.selectedProductForCart?.id || !variant.id || this.isAddingVariant(variant.id) || this.isAddingToCart(this.selectedProductForCart.id)) {
      return;
    }

    const productId = this.selectedProductForCart.id;
    const productName = this.selectedProductForCart.name;

    const payload = new CartItemCreateDTO({
      productId,
      variantId: variant.id,
      quantity: this.cartQuantityStep
    });

    this.activeCartVariantId = variant.id;
    this.addingProductIds.add(productId);
    this.cartApi.itemsPOST(payload)
      .pipe(finalize(() => {
        this.addingProductIds.delete(productId);
        this.activeCartVariantId = null;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.setCartPanelNotice(response.message ?? 'Khong the them san pham vao gio hang.', 'error');
            this.openCartPanel();
            return;
          }

          this.alertService.success(response?.message ?? `Da them ${productName || 'san pham'} vao gio hang.`);
          this.dataCacheService.invalidateByPrefix('main:cart:me');
          this.loadCartItems(true);
          this.closeVariantPicker();
        },
        error: () => {
          this.setCartPanelNotice('Khong the them san pham vao gio hang.', 'error');
          this.openCartPanel();
        }
      });
  }

  deleteFromCart(item: MainCartItemView): void {
    if (!item.productId || !item.variantId || this.isLoadingCart) {
      return;
    }

    this.isLoadingCart = true;
    const payload = new CartItemRemoveDTO({
      productId: item.productId,
      variantId: item.variantId
    });

    this.cartApi.itemsDELETE(payload)
      .pipe(finalize(() => {
        this.isLoadingCart = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.setCartPanelNotice(response.message ?? 'Khong the xoa san pham.', 'error');
            return;
          }

          this.alertService.success(response?.message ?? 'Da xoa san pham khoi gio hang.');
          this.dataCacheService.invalidateByPrefix('main:cart:me');
          this.loadCartItems(true);
        },
        error: () => {
          this.setCartPanelNotice('Khong the xoa san pham.', 'error');
        }
      });
  }

  decreaseQuantity(item: MainCartItemView): void {
    if (!item.productId || !item.variantId || item.quantity <= 1 || this.isLoadingCart) {
      return;
    }

    this.isLoadingCart = true;
    const payload = new CartItemUpdateDTO({
      productId: item.productId,
      variantId: item.variantId,
      quantity: -1
    });

    this.cartApi.itemsPATCH(payload)
      .pipe(finalize(() => {
        this.isLoadingCart = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.setCartPanelNotice(response.message ?? 'Khong the cap nhat so luong.', 'error');
            return;
          }

          this.dataCacheService.invalidateByPrefix('main:cart:me');
          this.loadCartItems(true);
        },
        error: () => {
          this.setCartPanelNotice('Khong the cap nhat so luong.', 'error');
        }
      });
  }

  increaseQuantity(item: MainCartItemView): void {
    if (!item.productId || !item.variantId || this.isLoadingCart) {
      return;
    }

    this.isLoadingCart = true;
    const payload = new CartItemUpdateDTO({
      productId: item.productId,
      variantId: item.variantId,
      quantity: 1
    });

    this.cartApi.itemsPATCH(payload)
      .pipe(finalize(() => {
        this.isLoadingCart = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          if (response?.success === false) {
            this.setCartPanelNotice(response.message ?? 'Khong the cap nhat so luong.', 'error');
            return;
          }

          this.dataCacheService.invalidateByPrefix('main:cart:me');
          this.loadCartItems(true);
        },
        error: () => {
          this.setCartPanelNotice('Khong the cap nhat so luong.', 'error');
        }
      });
  }

  toPriceLabel(price: number): string {
    return `${new Intl.NumberFormat('vi-VN').format(price)}d`;
  }

  productBackground(product: MainProductView, index: number): string {
    const fallbackImages = [
      'https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=900&q=80',
      'https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?auto=format&fit=crop&w=900&q=80',
      'https://images.unsplash.com/photo-1618354691467-67f38f4f39f3?auto=format&fit=crop&w=900&q=80',
      'https://images.unsplash.com/photo-1516826957135-700dedea698c?auto=format&fit=crop&w=900&q=80'
    ];

    const source = product.thumbnailUrl?.trim() || fallbackImages[index % fallbackImages.length];
    return `url('${source.replace(/'/g, "%27")}')`;
  }

  getHeroBackgroundImage(): string {
    if (!this.heroProduct) {
      return `url('https://images.unsplash.com/photo-1617127365659-c47fa864d8bc?auto=format&fit=crop&w=1600&q=80')`;
    }
    const source = this.heroProduct.thumbnailUrl?.trim() || 'https://images.unsplash.com/photo-1617127365659-c47fa864d8bc?auto=format&fit=crop&w=1600&q=80';
    return `url('${source.replace(/'/g, "%27")}')`;
  }

  private loadCategories(): void {
    const requestId = ++this.categoriesRequestId;

    this.dataCacheService.getOrSet(
      'main:categories:list',
      () => this.categoryApi.categoriesGET(),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.categoriesRequestId) {
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.categoriesRequestId) {
            return;
          }

          const rows = response.data ?? [];
          this.categories = rows.map((item: CategoryDTO, index: number) => this.toMainCategory(item, index));

          const exists = this.selectedCategoryId === 'all'
            || this.categories.some(category => category.id === this.selectedCategoryId);
          if (!exists) {
            this.selectedCategoryId = 'all';
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.warning('Khong the tai danh muc cho trang chu.');
        }
      });
  }

  private loadParentCategories(targetMenu?: 'ao' | 'quan'): void {
    const requestId = ++this.parentCategoriesRequestId;

    this.dataCacheService.getOrSet(
      'main:categories:parents:list',
      () => this.categoryApi.parents(),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.parentCategoriesRequestId) {
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.parentCategoriesRequestId) {
            return;
          }

          const rows = response.data ?? [];
          const mappedRows = rows.map((item: CategoryDTO, index: number) => this.toMainCategory(item, index));
          this.parentCategories.ao = mappedRows.find(category => this.normalizeText(category.name).includes('ao')) ?? null;
          this.parentCategories.quan = mappedRows.find(category => this.normalizeText(category.name).includes('quan')) ?? null;

          if (targetMenu && this.activeCategoryMenu === targetMenu && this.parentCategories[targetMenu]?.id) {
            this.loadMenuCategories(targetMenu);
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.warning('Khong the tai danh muc cha.');
        }
      });
  }

  private loadMenuCategories(menu: 'ao' | 'quan'): void {
    const requestId = ++this.menuCategoriesRequestId;
    const parentCategoryId = this.parentCategories[menu]?.id;
    if (!parentCategoryId) {
      this.menuCategories[menu] = [];
      this.isLoadingMenuCategories = false;
      this.cdr.detectChanges();
      return;
    }

    this.isLoadingMenuCategories = true;

    this.dataCacheService.getOrSet(
      `main:categories:sub:${parentCategoryId}`,
      () => this.categoryApi.sub(parentCategoryId),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.menuCategoriesRequestId) {
          this.isLoadingMenuCategories = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.menuCategoriesRequestId) {
            return;
          }

          const rows = response.data ?? [];
          this.menuCategories[menu] = rows.map((item: CategoryDTO, index: number) => this.toMainCategory(item, index));
          this.cdr.detectChanges();
        },
        error: () => {
          this.menuCategories[menu] = [];
          this.alertService.warning('Khong the tai danh muc con.');
        }
      });
  }

  private loadCategoryProducts(categoryId: string = this.selectedCategoryId, pageIndex: number = this.categoryPageIndex): void {
    const requestId = ++this.categoryProductsRequestId;
    const normalizedCategory = categoryId || 'all';
    this.isLoadingCategoryProducts = true;

    const cacheKey = `main:products:category:${normalizedCategory}:${pageIndex}:${this.categoryPageSize}`;
    this.dataCacheService.getOrSet(
      cacheKey,
      () => normalizedCategory === 'all'
        ? this.productApi.productsGET2(pageIndex, this.categoryPageSize)
        : this.categoryApi.productsGET(normalizedCategory, pageIndex, this.categoryPageSize),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.categoryProductsRequestId) {
          this.isLoadingCategoryProducts = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: (response: ProductResponseDTOPageResponseApiResponse) => {
          if (requestId !== this.categoryProductsRequestId) {
            return;
          }

          const rows = response.data?.items ?? [];
          this.categoryProducts = rows.map((item: ProductResponseDTO, index: number) => this.toMainProduct(item, index));
          this.categoryTotalPages = response.data?.totalPages ?? 1;
          this.categoryPageIndex = response.data?.pageIndex?._value ?? pageIndex;
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai san pham theo danh muc.');
          this.categoryProducts = [];
          this.categoryTotalPages = 1;
        }
      });
  }

  private loadFeaturedProducts(): void {
    const requestId = ++this.featuredProductsRequestId;
    this.isLoadingFeaturedProducts = true;

    this.dataCacheService.getOrSet(
      `main:products:featured:1:${this.featuredLimit}`,
      () => this.productApi.productsGET2(1, this.featuredLimit),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.featuredProductsRequestId) {
          this.isLoadingFeaturedProducts = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: (response: ProductResponseDTOPageResponseApiResponse) => {
          if (requestId !== this.featuredProductsRequestId) {
            return;
          }

          const rows = response.data?.items ?? [];
          this.featuredProducts = rows.map((item: ProductResponseDTO, index: number) => this.toMainProduct(item, index));
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai san pham noi bat.');
          this.featuredProducts = [];
        }
      });
  }

  private loadVariantsByProduct(productId: string): void {
    const requestId = ++this.variantsRequestId;
    this.isLoadingVariants = true;

    this.dataCacheService.getOrSet(
      `main:products:variants:${productId}`,
      () => this.variantsApi.variantsGET(productId),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.variantsRequestId) {
          this.isLoadingVariants = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.variantsRequestId) {
            return;
          }

          const rows = response.data ?? [];
          this.variantOptions = rows.map((item: VariantDTO, index: number) => this.toMainVariant(item, index));
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai bien the san pham.');
          this.variantOptions = [];
        }
      });
  }

  private loadCartItems(forceRefresh = false): void {
    const requestId = ++this.cartRequestId;
    this.isLoadingCart = true;

    if (forceRefresh) {
      this.dataCacheService.invalidateByPrefix('main:cart:me');
    }

    this.dataCacheService.getOrSet(
      'main:cart:me',
      () => this.cartApi.meGET(),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.cartRequestId) {
          this.isLoadingCart = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.cartRequestId) {
            return;
          }

          const cart = response.data;
          const rows = cart?.items ?? [];
          this.cartItems = rows.map((item: CartItemDTO, index: number) => this.toMainCartItem(item, index));
          this.cartTotalItems = Number(cart?.totalItems ?? 0);
          this.cartTotalPrice = Number(cart?.totalPrice ?? 0);
          if (this.cartItems.length === 0) {
            this.isCheckoutFormOpen = false;
          }
          this.cdr.detectChanges();
        },
        error: () => {
          this.setCartPanelNotice('Khong the tai gio hang.', 'error');
          this.cartItems = [];
          this.cartTotalItems = 0;
          this.cartTotalPrice = 0;
        }
      });
  }

  private setCartPanelNotice(message: string, type: 'success' | 'error' | 'info' = 'info'): void {
    this.cartPanelNotice = message;
    this.cartPanelNoticeType = type;
  }

  private loadHeroProduct(): void {
    this.dataCacheService.getOrSet(
      'main:products:hero:1',
      () => this.productApi.productsGET2(1, 1),
      this.cacheTtlMs
    )
      .pipe(finalize(() => {
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (response: ProductResponseDTOPageResponseApiResponse) => {
          const rows = response.data?.items ?? [];
          this.heroProduct = rows[0] ? this.toMainProduct(rows[0], 0) : null;
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.warning('Khong the tai san pham dac biet.');
          this.heroProduct = null;
        }
      });
  }

  private toMainCategory(item: CategoryDTO, index: number): MainCategoryView {
    const visualClasses = ['shirt', 'pants', 'polo', 'accessories'];

    return {
      id: item.id ?? `category-${index + 1}`,
      name: item.name ?? `Danh muc ${index + 1}`,
      description: item.description ?? 'Xem bo suu tap moi nhat',
      visualClass: visualClasses[index % visualClasses.length]
    };
  }

  private toMainProduct(item: ProductResponseDTO, index: number): MainProductView {
    return {
      id: item.id ?? `product-${index + 1}`,
      name: item.name ?? `San pham ${index + 1}`,
      description: item.description ?? 'Thiet ke toi gian, de phoi do, phu hop moi ngay.',
      price: Number(item.price ?? 0),
      thumbnailUrl: item.thumbnailUrl ?? '',
      categoryId: item.categoryId
    };
  }

  private toMainVariant(item: VariantDTO, index: number): MainVariantView {
    return {
      id: item.id ?? `variant-${index + 1}`,
      size: item.size ?? 'N/A',
      color: item.color ?? 'N/A',
      price: Number(item.price ?? 0),
      stockQuantity: Number(item.stockQuantity ?? 0)
    };
  }

  private toMainCartItem(item: CartItemDTO, index: number): MainCartItemView {
    return {
      id: `${item.productId ?? 'product'}-${item.variantId ?? 'variant'}-${index}`,
      productId: item.productId,
      variantId: item.variantId,
      productName: item.productName ?? 'San pham',
      color: item.color ?? 'N/A',
      size: item.size ?? 'N/A',
      price: Number(item.price ?? 0),
      quantity: Number(item.quantity ?? 0),
      lineTotal: Number(item.lineTotal ?? item.totalPrice ?? 0)
    };
  }

  private normalizeText(value?: string): string {
    return String(value ?? '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }
}
