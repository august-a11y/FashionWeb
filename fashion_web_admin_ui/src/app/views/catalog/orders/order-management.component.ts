import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  AdminApiOrderApiClient,
  OrderDTO,
  OrderStatus,
  OrderStatusUpdateDTO,
  PaymentMethod,
  PaymentStatus,
  ShipmentStatus
} from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';
import { DataCacheService } from '../../../shared/services/data-cache.service';

interface OrderListItem {
  id: string;
  code: string;
  customer: string;
  total: number;
  paymentMethod: string;
  status: string;
  createdAt?: Date;
}

interface OrderDetailView {
  id: string;
  code: string;
  statusLabel: string;
  paymentMethodLabel: string;
  paymentStatusLabel: string;
  shippingStatusLabel: string;
  customer: string;
  phone: string;
  address: string;
  note: string;
  subTotal: number;
  shippingFee: number;
  grandTotal: number;
  createdAt?: Date;
  expectedDeliveryDate?: Date;
  items: Array<{
    productName: string;
    variant: string;
    quantity: number;
    unitPrice: number;
    lineTotal: number;
  }>;
}

@Component({
  selector: 'app-order-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './order-management.component.html',
  styleUrl: './order-management.component.scss'
})
export class OrderManagementComponent implements OnInit, AfterViewInit {
  readonly statusOptions = ['All', 'Pending', 'Processing', 'Shipping', 'Delivered', 'Cancelled'];

  readonly filterForm = this.fb.group({
    keyword: [''],
    status: ['All']
  });

  readonly detailForm = this.fb.group({
    status: ['Pending']
  });

  orders: OrderListItem[] = [];
  filteredOrders: OrderListItem[] = [];
  selectedOrderDetail: OrderDetailView | null = null;
  selectedOrderKey = '';

  isLoading = false;
  isLoadingDetail = false;
  isUpdatingStatus = false;

  private readonly listCacheTtlMs = 60 * 1000;
  private listLoadRequestId = 0;
  private detailLoadRequestId = 0;

  constructor(
    private fb: FormBuilder,
    private orderApi: AdminApiOrderApiClient,
    private alertService: AlertService,
    private dataCacheService: DataCacheService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.filterForm.valueChanges.subscribe(() => this.applyFilters());
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadOrders();
    }, 0);
  }

  loadOrders(forceRefresh = false): void {
    const requestId = ++this.listLoadRequestId;
    this.isLoading = true;

    if (forceRefresh) {
      this.dataCacheService.invalidateByPrefix('catalog:orders:all');
    }

    this.dataCacheService.getOrSet(
      'catalog:orders:all',
      () => this.orderApi.ordersGET3(),
      this.listCacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.listLoadRequestId) {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.listLoadRequestId) {
            return;
          }

          const rows = response.data ?? [];
          this.orders = rows
            .map((item: OrderDTO, index: number) => this.toOrderListItem(item, index));
          this.applyFilters();

          if (this.selectedOrderKey) {
            const stillExists = this.orders.some(item => item.id === this.selectedOrderKey);
            if (!stillExists) {
              this.selectedOrderKey = '';
              this.selectedOrderDetail = null;
            }
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai danh sach don hang.');
        }
      });
  }

  clearFilters(): void {
    this.filterForm.reset({ keyword: '', status: 'All' });
    this.applyFilters();
  }

  trackOrder(_index: number, item: OrderListItem): string {
    return item.id;
  }

  selectOrder(order: OrderListItem): void {
    const orderId = order.id;
    if (!orderId) {
      this.alertService.warning('Don hang nay khong co dinh danh hop le de tai chi tiet.');
      return;
    }

    this.selectedOrderKey = order.id;

    const requestId = ++this.detailLoadRequestId;
    this.isLoadingDetail = true;

    this.orderApi.ordersGET2(orderId)
      .pipe(finalize(() => {
        if (requestId === this.detailLoadRequestId) {
          this.isLoadingDetail = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: response => {
          if (requestId !== this.detailLoadRequestId) {
            return;
          }

          const order = response.data;
          if (!order) {
            this.selectedOrderDetail = null;
            return;
          }

          const statusLabel = this.orderStatusFromEnum(order.status);
          this.selectedOrderDetail = this.toOrderDetailView(orderId, order);
          this.detailForm.patchValue({ status: statusLabel }, { emitEvent: false });
          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai chi tiet don hang.');
        }
      });
  }

  updateOrderStatus(): void {
    if (!this.selectedOrderDetail || this.isUpdatingStatus) {
      return;
    }

    const nextStatus = String(this.detailForm.value.status ?? '').trim();
    const mappedStatus = this.toOrderStatus(nextStatus);
    if (!mappedStatus) {
      this.alertService.warning('Trang thai khong hop le. Vui long chon lai.');
      return;
    }

    this.isUpdatingStatus = true;
    const payload = new OrderStatusUpdateDTO({ status: mappedStatus });

    this.orderApi.status(this.selectedOrderDetail.id, payload)
      .pipe(finalize(() => {
        this.isUpdatingStatus = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Cap nhat trang thai don hang thanh cong.');
          const selectedId = this.selectedOrderDetail!.id;
          this.loadOrders(true);
          this.selectOrderById(selectedId);
        },
        error: () => {
          this.alertService.error('Cap nhat trang thai don hang that bai.');
        }
      });
  }

  statusBadgeClass(status: string): string {
    const normalized = status.trim().toLowerCase();
    if (normalized.includes('deliver') || normalized.includes('complete')) {
      return 'bg-success-subtle text-success-emphasis';
    }

    if (normalized.includes('cancel')) {
      return 'bg-danger-subtle text-danger-emphasis';
    }

    if (normalized.includes('ship')) {
      return 'bg-info-subtle text-info-emphasis';
    }

    if (normalized.includes('process')) {
      return 'bg-primary-subtle text-primary-emphasis';
    }

    return 'bg-warning-subtle text-warning-emphasis';
  }

  private applyFilters(): void {
    const keyword = String(this.filterForm.value.keyword ?? '').trim().toLowerCase();
    const status = String(this.filterForm.value.status ?? 'All').trim().toLowerCase();

    this.filteredOrders = this.orders.filter(item => {
      const keywordMatch = !keyword
        || item.code.toLowerCase().includes(keyword)
        || item.customer.toLowerCase().includes(keyword);

      const statusMatch = status === 'all' || item.status.toLowerCase() === status;
      return keywordMatch && statusMatch;
    });
  }

  private toOrderListItem(item: OrderDTO, index: number): OrderListItem {
    const orderCode = item.orderCode ?? `ORDER-${index + 1}`;
    const id = item.id ?? '';

    return {
      id,
      code: orderCode,
      customer: item.shippingAddress?.fullName ?? 'Khach le',
      total: Number(item.grandTotal ?? 0),
      paymentMethod: this.paymentMethodLabel(item.paymentMethod),
      status: this.orderStatusFromEnum(item.status),
      createdAt: item.createdAt ? new Date(item.createdAt) : undefined
    };
  }

  private selectOrderById(orderId: string): void {
    this.orderApi.ordersGET2(orderId)
      .subscribe({
        next: response => {
          const order = response.data;
          if (!order) {
            this.selectedOrderDetail = null;
            return;
          }

          const statusLabel = this.orderStatusFromEnum(order.status);
          this.selectedOrderDetail = this.toOrderDetailView(orderId, order);
          this.detailForm.patchValue({ status: statusLabel }, { emitEvent: false });
          this.cdr.detectChanges();
        }
      });
  }

  private toOrderDetailView(id: string, order: OrderDTO): OrderDetailView {
    const address = order.shippingAddress;
    const addressLine = [
      address?.streetLine,
      address?.ward,
      address?.district,
      address?.city
    ]
      .filter(Boolean)
      .join(', ');

    return {
      id,
      code: order.orderCode ?? '-',
      statusLabel: this.orderStatusFromEnum(order.status),
      paymentMethodLabel: this.paymentMethodLabel(order.paymentMethod),
      paymentStatusLabel: this.paymentStatusLabel(order.paymentStatus),
      shippingStatusLabel: this.shippingStatusLabel(order.shippingStatus),
      customer: address?.fullName ?? 'Khach le',
      phone: address?.phoneNumber ?? '-',
      address: addressLine || '-',
      note: order.note ?? '-',
      subTotal: Number(order.subTotal ?? 0),
      shippingFee: Number(order.shippingFee ?? 0),
      grandTotal: Number(order.grandTotal ?? 0),
      createdAt: order.createdAt ? new Date(order.createdAt) : undefined,
      expectedDeliveryDate: order.expectedDeliveryDate ? new Date(order.expectedDeliveryDate) : undefined,
      items: (order.items ?? []).map(item => ({
        productName: item.productName ?? '-',
        variant: `${item.color ?? '-'} / ${item.size ?? '-'}`,
        quantity: Number(item.quantity ?? 0),
        unitPrice: Number(item.unitPrice ?? 0),
        lineTotal: Number(item.lineTotal ?? 0)
      }))
    };
  }

  private orderStatusFromEnum(status?: OrderStatus): string {
    switch (status) {
      case OrderStatus.Processing:
        return 'Processing';
      case OrderStatus.Shipping:
        return 'Shipping';
      case OrderStatus.Delivered:
        return 'Delivered';
      case OrderStatus.Cancelled:
        return 'Cancelled';
      default:
        return 'Pending';
    }
  }

  private toOrderStatus(value: string): OrderStatus | null {
    switch (value) {
      case OrderStatus.Pending:
        return OrderStatus.Pending;
      case OrderStatus.Processing:
        return OrderStatus.Processing;
      case OrderStatus.Shipping:
        return OrderStatus.Shipping;
      case OrderStatus.Delivered:
        return OrderStatus.Delivered;
      case OrderStatus.Cancelled:
        return OrderStatus.Cancelled;
      default:
        return null;
    }
  }

  private paymentMethodLabel(method?: PaymentMethod): string {
    switch (method) {
      case PaymentMethod.COD:
        return 'COD';
      case PaymentMethod.CreditCard:
        return 'Credit Card';
      case PaymentMethod.Momo:
        return 'E-Wallet';
      case PaymentMethod.PayPal:
        return 'PayPal';
      default:
        return 'Khac';
    }
  }

  private paymentStatusLabel(status?: PaymentStatus): string {
    switch (status) {
      case PaymentStatus.Pending:
        return 'Cho xu ly';
      case PaymentStatus.Success:
        return 'Da thanh toan';
      case PaymentStatus.Failed:
        return 'That bai';
      case PaymentStatus.Refunded:
        return 'Da hoan tien';
      default:
        return 'Chua thanh toan';
    }
  }

  private shippingStatusLabel(status?: ShipmentStatus): string {
    switch (status) {
      case ShipmentStatus.InTransit:
        return 'Dang giao';
      case ShipmentStatus.Delivered:
        return 'Da giao';
      case ShipmentStatus.Returned:
        return 'Da hoan';
      case ShipmentStatus.Preparing:
        return 'Cho dong goi';
      default:
        return 'Cho dong goi';
    }
  }
}
