import { AfterViewInit, ChangeDetectorRef, Component, DestroyRef, DOCUMENT, effect, inject, OnInit, Renderer2, signal, WritableSignal } from '@angular/core';
import { DatePipe, DecimalPipe, NgClass, UpperCasePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ChartData, ChartDataset, ChartOptions } from 'chart.js';
import {
  ButtonDirective,
  ButtonGroupComponent,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  FormCheckLabelDirective,
  GutterDirective,
  RowComponent,
  TableDirective
} from '@coreui/angular';
import { ChartjsComponent } from '@coreui/angular-chartjs';
import { getStyle } from '@coreui/utils';
import { finalize } from 'rxjs';
import {
  AdminApiDashboardApiClient,
  DashboardOverviewDTO,
  DashboardPeriod,
  DashboardTrendDTO
} from '../../api/admin-api.service.generated';
import { AlertService } from '../../shared/services/alert.service';
import { DataCacheService } from '../../shared/services/data-cache.service';
import { WidgetsDropdownComponent } from '../widgets/widgets-dropdown/widgets-dropdown.component';

interface IChartProps {
  data?: ChartData<'line'>;
  options?: ChartOptions<'line'>;
  type: 'line';

  [propName: string]: unknown;
}

interface IKpiCard {
  title: string;
  value: string;
  change: string;
  trend: 'up' | 'down';
  subtitle: string;
  icon: string;
}

interface IInventoryAlert {
  title: string;
  description: string;
  severity: 'high' | 'medium' | 'low';
}

interface ITopProduct {
  id: string;
  name: string;
  unitsSold: number;
  revenue: number;
  stock: number;
}

interface ITopCategory {
  name: string;
  unitsSold: number;
  revenue: number;
}

interface ITopCustomer {
  customer: string;
  totalSpent: number;
  orderCount: number;
}

interface IOrderView {
  code: string;
  customer: string;
  total: number;
  paymentMethod: string;
  status: 'pending' | 'processing' | 'completed' | 'cancelled';
  createdAt?: Date;
}

@Component({
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss'],
  imports: [
    CardComponent,
    WidgetsDropdownComponent,
    CardBodyComponent,
    RowComponent,
    ColComponent,
    ButtonDirective,
    ReactiveFormsModule,
    ButtonGroupComponent,
    FormCheckLabelDirective,
    ChartjsComponent,
    GutterDirective,
    CardHeaderComponent,
    TableDirective,
    DatePipe,
    DecimalPipe,
    NgClass,
    UpperCasePipe
  ]
})
export class DashboardComponent implements OnInit, AfterViewInit {

  readonly #destroyRef: DestroyRef = inject(DestroyRef);
  readonly #document: Document = inject(DOCUMENT);
  readonly #renderer: Renderer2 = inject(Renderer2);
  readonly #dashboardApi: AdminApiDashboardApiClient = inject(AdminApiDashboardApiClient);
  readonly #alertService: AlertService = inject(AlertService);
  readonly #dataCacheService: DataCacheService = inject(DataCacheService);
  readonly #cdr: ChangeDetectorRef = inject(ChangeDetectorRef);

  private readonly dashboardCacheTtlMs = 5 * 60 * 1000;

  private readonly currencyFormatter = new Intl.NumberFormat('vi-VN');
  private dashboardLoadRequestId = 0;


  public isLoading = false;

  public kpiCards: IKpiCard[] = [
    {
      title: 'Tong doanh thu',
      value: '0 VND',
      change: '0%',
      trend: 'up',
      subtitle: 'So voi ky truoc',
      icon: 'cilWallet'
    },
    {
      title: 'So luong don hang',
      value: '0',
      change: '0%',
      trend: 'up',
      subtitle: 'So voi ky truoc',
      icon: 'cilCart'
    },
    {
      title: 'Khach hang moi',
      value: '0',
      change: '0%',
      trend: 'up',
      subtitle: 'Phat sinh trong ky',
      icon: 'cilUserPlus'
    },
    {
      title: 'Ty le chuyen doi',
      value: '0%',
      change: '0%',
      trend: 'up',
      subtitle: 'Don hoan tat / tong don',
      icon: 'cilChartLine'
    }
  ];

  public inventoryAlerts: IInventoryAlert[] = [];
  public topProducts: ITopProduct[] = [];
  public topCategories: ITopCategory[] = [];
  public topCustomers: ITopCustomer[] = [];
  public recentOrders: IOrderView[] = [];

  public mainChart: IChartProps = { type: 'line' };
  public mainChartRef: WritableSignal<any> = signal(undefined);
  #mainChartRefEffect = effect(() => {
    if (this.mainChartRef()) {
      this.setChartStyles();
    }
  });
  public chart: Array<IChartProps> = [];
  public trafficRadioGroup = new FormGroup({
    trafficRadio: new FormControl('Month')
  });

  ngOnInit(): void {
    this.buildTrendChart(undefined);
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadDashboardData();
      this.updateChartOnColorModeChange();
    }, 0);
  }

  forceReloadDashboard(): void {
    this.loadDashboardData(true);
  }

  setTrafficPeriod(value: string): void {
    this.trafficRadioGroup.patchValue({ trafficRadio: value });
    this.loadDashboardData();
  }

  handleChartRef($chartRef: any) {
    this.mainChartRef.set($chartRef ?? undefined);
  }

  updateChartOnColorModeChange() {
    const unListen = this.#renderer.listen(this.#document.documentElement, 'ColorSchemeChange', () => {
      this.setChartStyles();
    });

    this.#destroyRef.onDestroy(() => {
      unListen();
    });
  }

  setChartStyles() {
    const chartRef = this.mainChartRef();
    if (!chartRef) {
      return;
    }

    setTimeout(() => {
      const currentRef = this.mainChartRef();
      if (!currentRef || currentRef !== chartRef) {
        return;
      }

      const chartInstance = currentRef.chart;
      const canvas = chartInstance?.canvas;
      if (!canvas || !canvas.isConnected || !canvas.ownerDocument) {
        return;
      }

      const options = { ...(this.mainChart.options ?? {}) } as any;
      currentRef.options = options;
      currentRef.update();
    }, 0);
  }

  orderStatusLabel(status: IOrderView['status']): string {
    const labels: Record<IOrderView['status'], string> = {
      pending: 'Cho xu ly',
      processing: 'Dang dong goi',
      completed: 'Hoan tat',
      cancelled: 'Da huy'
    };

    return labels[status];
  }

  orderStatusClass(status: IOrderView['status']): string {
    const classMap: Record<IOrderView['status'], string> = {
      pending: 'bg-warning-subtle text-warning-emphasis',
      processing: 'bg-info-subtle text-info-emphasis',
      completed: 'bg-success-subtle text-success-emphasis',
      cancelled: 'bg-danger-subtle text-danger-emphasis'
    };

    return classMap[status];
  }

  alertSeverityClass(severity: IInventoryAlert['severity']): string {
    const classMap: Record<IInventoryAlert['severity'], string> = {
      high: 'bg-danger-subtle text-danger-emphasis',
      medium: 'bg-warning-subtle text-warning-emphasis',
      low: 'bg-info-subtle text-info-emphasis'
    };

    return classMap[severity];
  }

  private loadDashboardData(forceRefresh = false): void {
    const requestId = ++this.dashboardLoadRequestId;
    this.isLoading = true;
    const period = this.toDashboardPeriod(this.trafficRadioGroup.value.trafficRadio ?? 'Month');
    const cacheKey = `dashboard:overview:${period}`;

    if (forceRefresh) {
      this.#dataCacheService.invalidateByPrefix('dashboard:overview:');
    }

    this.#dataCacheService.getOrSet(
      cacheKey,
      () => this.#dashboardApi.overview(period, undefined, undefined, 5, 5),
      this.dashboardCacheTtlMs
    ).pipe(
      finalize(() => {
        if (requestId === this.dashboardLoadRequestId) {
          this.isLoading = false;
          this.#cdr.detectChanges();
        }
      })
    ).subscribe({
      next: (response) => {
        if (requestId !== this.dashboardLoadRequestId) {
          return;
        }

        this.applyDashboardData(response?.data);
        this.#cdr.detectChanges();
      },
      error: () => {
        this.#alertService.error('Khong the tai du lieu dashboard.');
      }
    });
  }

  private applyDashboardData(overview?: DashboardOverviewDTO): void {
    const kpis = overview?.kpis;
    const revenueChange = Number(kpis?.revenueChangePct ?? 0);
    const ordersChange = Number(kpis?.ordersChangePct ?? 0);
    const customersChange = Number(kpis?.newCustomersChangePct ?? 0);
    const conversionChange = Number(kpis?.conversionChangePct ?? 0);

    this.kpiCards = [
      {
        title: 'Tong doanh thu',
        value: `${this.currencyFormatter.format(Math.round(Number(kpis?.totalRevenue ?? 0)))} VND`,
        change: this.formatPercentChange(revenueChange),
        trend: revenueChange >= 0 ? 'up' : 'down',
        subtitle: 'So voi ky truoc',
        icon: 'cilWallet'
      },
      {
        title: 'So luong don hang',
        value: this.currencyFormatter.format(Number(kpis?.totalOrders ?? 0)),
        change: this.formatPercentChange(ordersChange),
        trend: ordersChange >= 0 ? 'up' : 'down',
        subtitle: 'So voi ky truoc',
        icon: 'cilCart'
      },
      {
        title: 'Khach hang moi',
        value: this.currencyFormatter.format(Number(kpis?.newCustomers ?? 0)),
        change: this.formatPercentChange(customersChange),
        trend: customersChange >= 0 ? 'up' : 'down',
        subtitle: 'Phat sinh trong ky',
        icon: 'cilUserPlus'
      },
      {
        title: 'Ty le chuyen doi',
        value: `${Number(kpis?.conversionRate ?? 0).toFixed(2)}%`,
        change: this.formatPercentChange(conversionChange),
        trend: conversionChange >= 0 ? 'up' : 'down',
        subtitle: 'Don hoan tat / tong don',
        icon: 'cilChartLine'
      }
    ];

    this.inventoryAlerts = (overview?.alerts ?? []).map((alert, index) => ({
      title: alert.title ?? `Canh bao ${index + 1}`,
      description: alert.description ?? '',
      severity: this.normalizeAlertSeverity(alert.severity)
    }));

    this.topProducts = (overview?.topProducts ?? []).map((item, index) => ({
      id: item.id ?? `top-product-${index + 1}`,
      name: item.name ?? `San pham ${index + 1}`,
      unitsSold: Number(item.unitsSold ?? 0),
      revenue: Number(item.revenue ?? 0),
      stock: Number(item.stock ?? 0)
    }));

    this.topCategories = (overview?.topCategories ?? []).map((item, index) => ({
      name: item.name ?? `Danh muc ${index + 1}`,
      unitsSold: Number(item.unitsSold ?? 0),
      revenue: Number(item.revenue ?? 0)
    }));

    this.topCustomers = (overview?.topCustomers ?? []).map((item, index) => ({
      customer: item.customer ?? `Khach hang ${index + 1}`,
      totalSpent: Number(item.totalSpent ?? 0),
      orderCount: Number(item.orderCount ?? 0)
    }));

    this.recentOrders = (overview?.recentOrders ?? []).map((item, index) => ({
      code: item.code ?? `ORDER-${index + 1}`,
      customer: item.customer ?? 'Khach le',
      total: Number(item.total ?? 0),
      paymentMethod: item.paymentMethod ?? 'Khac',
      status: this.mapDashboardOrderStatus(item.status),
      createdAt: item.createdAt ? new Date(item.createdAt) : undefined
    }));

    this.buildTrendChart(overview?.trend);
    this.setChartStyles();
  }

  private buildTrendChart(trend?: DashboardTrendDTO): void {
    const labels = trend?.labels ?? [];
    const revenueSeries = (trend?.revenueSeries ?? []).map((item) => Number(item ?? 0));
    const orderSeries = (trend?.orderSeries ?? []).map((item) => Number(item ?? 0));

    const colorInfo = '#1677a1';
    const colorInfoRgb = '22, 119, 161';
    const colorSuccess = '#2a98c7';
    const colorBody = '#5c7b8a';
    const colorBorder = 'rgba(184, 222, 239, 0.85)';

    const datasets: ChartDataset<'line'>[] = [
      {
        label: 'Doanh thu',
        data: revenueSeries,
        borderColor: colorInfo,
        backgroundColor: `rgba(${colorInfoRgb}, .12)`,
        yAxisID: 'y',
        fill: true,
        borderWidth: 2,
        pointRadius: 2,
        tension: 0.35
      },
      {
        label: 'Don hang',
        data: orderSeries,
        borderColor: colorSuccess,
        backgroundColor: 'transparent',
        yAxisID: 'y1',
        fill: false,
        borderWidth: 2,
        pointRadius: 2,
        tension: 0.35
      }
    ];

    const data: ChartData<'line'> = {
      labels,
      datasets
    };

    const options: ChartOptions<'line'> = {
      maintainAspectRatio: false,
      interaction: {
        mode: 'index',
        intersect: false
      },
      plugins: {
        legend: {
          display: true,
          labels: {
            color: colorBody,
            usePointStyle: true,
            pointStyle: 'circle'
          }
        }
      },
      scales: {
        x: {
          grid: {
            color: colorBorder,
            drawOnChartArea: false
          },
          ticks: {
            color: colorBody
          }
        },
        y: {
          beginAtZero: true,
          grid: {
            color: colorBorder
          },
          ticks: {
            color: colorBody,
            callback: (value) => `${this.currencyFormatter.format(Number(value))}`
          }
        },
        y1: {
          position: 'right',
          beginAtZero: true,
          grid: {
            drawOnChartArea: false
          },
          ticks: {
            color: colorBody,
            stepSize: 1
          }
        }
      }
    };

    this.mainChart = {
      type: 'line',
      data,
      options
    };
  }

  private formatPercentChange(change: number): string {
    const sign = change >= 0 ? '+' : '';
    return `${sign}${change.toFixed(1)}%`;
  }

  private normalizeAlertSeverity(severity?: string): IInventoryAlert['severity'] {
    const normalized = (severity ?? '').trim().toLowerCase();
    if (normalized === 'high') {
      return 'high';
    }

    if (normalized === 'medium') {
      return 'medium';
    }

    return 'low';
  }

  private mapDashboardOrderStatus(status?: string): IOrderView['status'] {
    const normalized = (status ?? '').trim().toLowerCase();
    if (normalized.includes('complete') || normalized.includes('hoan tat')) {
      return 'completed';
    }

    if (normalized.includes('cancel') || normalized.includes('huy')) {
      return 'cancelled';
    }

    if (normalized.includes('process') || normalized.includes('dong goi')) {
      return 'processing';
    }

    return 'pending';
  }

  private toDashboardPeriod(period: string): DashboardPeriod {
    if (period === 'Day') {
      return DashboardPeriod.Day;
    }

    if (period === 'Year') {
      return DashboardPeriod.Year;
    }

    return DashboardPeriod.Month;
  }
}
