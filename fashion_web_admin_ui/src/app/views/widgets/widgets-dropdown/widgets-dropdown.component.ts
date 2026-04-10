import { AfterViewInit, Component, input, OnInit, viewChild } from '@angular/core';
import { getStyle } from '@coreui/utils';
import { ChartjsComponent } from '@coreui/angular-chartjs';
import { RouterLink } from '@angular/router';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonDirective,
  ColComponent,
  DropdownComponent,
  DropdownDividerDirective,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  RowComponent,
  TemplateIdDirective,
  WidgetStatAComponent
} from '@coreui/angular';

@Component({
  selector: 'app-widgets-dropdown',
  templateUrl: './widgets-dropdown.component.html',
  styleUrls: ['./widgets-dropdown.component.scss'],
  imports: [RowComponent, ColComponent, WidgetStatAComponent, TemplateIdDirective, IconDirective, DropdownComponent, ButtonDirective, DropdownToggleDirective, DropdownMenuDirective, DropdownItemDirective, RouterLink, DropdownDividerDirective, ChartjsComponent]
})
export class WidgetsDropdownComponent implements OnInit, AfterViewInit {
  private numberFormatter = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 1
  });

  readonly kpiCards = input<Array<{
    title: string;
    value: string;
    change: string;
    trend: 'up' | 'down';
  }>>([]);

  data: any[] = [];
  options: any[] = [];
  labels = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
    'January',
    'February',
    'March',
    'April'
  ];
  datasets = [
    [{
      label: 'My First dataset',
      backgroundColor: 'transparent',
      borderColor: 'rgba(255,255,255,.55)',
      pointBackgroundColor: getStyle('--cui-primary'),
      pointHoverBorderColor: getStyle('--cui-primary'),
      data: [65, 59, 84, 84, 51, 55, 40]
    }], [{
      label: 'My Second dataset',
      backgroundColor: 'transparent',
      borderColor: 'rgba(255,255,255,.55)',
      pointBackgroundColor: getStyle('--cui-info'),
      pointHoverBorderColor: getStyle('--cui-info'),
      data: [1, 18, 9, 17, 34, 22, 11]
    }], [{
      label: 'My Third dataset',
      backgroundColor: 'rgba(255,255,255,.2)',
      borderColor: 'rgba(255,255,255,.55)',
      pointBackgroundColor: getStyle('--cui-warning'),
      pointHoverBorderColor: getStyle('--cui-warning'),
      data: [78, 81, 80, 45, 34, 12, 40],
      fill: true
    }], [{
      label: 'My Fourth dataset',
      backgroundColor: 'rgba(255,255,255,.2)',
      borderColor: 'rgba(255,255,255,.55)',
      data: [78, 81, 80, 45, 34, 12, 40, 85, 65, 23, 12, 98, 34, 84, 67, 82],
      barPercentage: 0.7
    }]
  ];
  optionsDefault = {
    plugins: {
      legend: {
        display: false
      }
    },
    maintainAspectRatio: false,
    scales: {
      x: {
        border: {
          display: false
        },
        grid: {
          display: false,
          drawBorder: false
        },
        ticks: {
          display: false
        }
      },
      y: {
        min: 30,
        max: 89,
        display: false,
        grid: {
          display: false
        },
        ticks: {
          display: false
        }
      }
    },
    elements: {
      line: {
        borderWidth: 1,
        tension: 0.4
      },
      point: {
        radius: 4,
        hitRadius: 10,
        hoverRadius: 4
      }
    }
  };

  ngOnInit(): void {
    // Keep ngOnInit lightweight to avoid expression-changed errors on first check.
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.setData();
    }, 0);
  }

  setData() {
    for (let idx = 0; idx < 4; idx++) {
      this.data[idx] = {
        labels: idx < 3 ? this.labels.slice(0, 7) : this.labels,
        datasets: this.datasets[idx]
      };
    }
    this.setOptions();
  }

  setOptions() {
    for (let idx = 0; idx < 4; idx++) {
      const options = JSON.parse(JSON.stringify(this.optionsDefault));
      switch (idx) {
        case 0: {
          this.options.push(options);
          break;
        }
        case 1: {
          options.scales.y.min = -9;
          options.scales.y.max = 39;
          options.elements.line.tension = 0;
          this.options.push(options);
          break;
        }
        case 2: {
          options.scales.x = { display: false };
          options.scales.y = { display: false };
          options.elements.line.borderWidth = 2;
          options.elements.point.radius = 0;
          this.options.push(options);
          break;
        }
        case 3: {
          options.scales.x.grid = { display: false, drawTicks: false };
          options.scales.x.grid = { display: false, drawTicks: false, drawBorder: false };
          options.scales.y.min = undefined;
          options.scales.y.max = undefined;
          options.elements = {};
          this.options.push(options);
          break;
        }
      }
    }
  }

  getCard(index: number) {
    const cards = this.kpiCards();
    const fallback = [
      { title: 'Users', value: '26K', change: '-12.4%', trend: 'down' as const },
      { title: 'Income', value: '$6.200', change: '40.9%', trend: 'up' as const },
      { title: 'Conversion Rate', value: '2.49', change: '84.7%', trend: 'up' as const },
      { title: 'Sessions', value: '44K', change: '-23.6%', trend: 'down' as const }
    ];

    return cards[index] ?? fallback[index];
  }

  getTrendIcon(index: number) {
    return this.getCard(index).trend === 'up' ? 'cilArrowTop' : 'cilArrowBottom';
  }

  getFormattedValue(index: number): string {
    const card = this.getCard(index);
    return this.formatKpiValue(card.title, card.value);
  }

  private formatKpiValue(title: string, value: string): string {
    if (!value) {
      return value;
    }

    const isPercent = value.includes('%');
    if (isPercent) {
      return value;
    }

    const hasVnd = /vnd/i.test(value) || /doanh thu/i.test(title);
    const numeric = this.parseNumeric(value);
    if (numeric === null) {
      return value;
    }

    const compact = this.toCompactNumber(numeric);
    return hasVnd ? `${compact} VND` : compact;
  }

  private parseNumeric(value: string): number | null {
    const cleaned = value.replace(/[^\d.,-]/g, '');
    if (!cleaned) {
      return null;
    }

    const normalized = cleaned.replace(/,/g, '');
    const parsed = Number(normalized);
    return Number.isNaN(parsed) ? null : parsed;
  }

  private toCompactNumber(value: number): string {
    const abs = Math.abs(value);
    if (abs >= 1_000_000_000) {
      return `${this.numberFormatter.format(value / 1_000_000_000)}B`;
    }

    if (abs >= 1_000_000) {
      return `${this.numberFormatter.format(value / 1_000_000)}M`;
    }

    if (abs >= 1_000) {
      return `${this.numberFormatter.format(value / 1_000)}K`;
    }

    return this.numberFormatter.format(value);
  }
}

@Component({
  selector: 'app-chart-sample',
  template: '<c-chart type="line" [data]="data" [options]="options" width="300" #chart />',
  imports: [ChartjsComponent]
})
export class ChartSample implements AfterViewInit {

  constructor() {}

  readonly chartComponent = viewChild.required<ChartjsComponent>('chart');

  colors = {
    label: 'My dataset',
    backgroundColor: 'rgba(77,189,116,.2)',
    borderColor: '#4dbd74',
    pointHoverBackgroundColor: '#fff'
  };

  labels = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];

  data = {
    labels: this.labels,
    datasets: [{
      data: [65, 59, 84, 84, 51, 55, 40],
      ...this.colors,
      fill: { value: 65 }
    }]
  };

  options = {
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    elements: {
      line: {
        tension: 0.4
      }
    }
  };

  ngAfterViewInit(): void {
    setTimeout(() => {
      const data = () => {
        return {
          ...this.data,
          labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
          datasets: [{
            ...this.data.datasets[0],
            data: [42, 88, 42, 66, 77],
            fill: { value: 55 }
          }, { ...this.data.datasets[0], borderColor: '#ffbd47', data: [88, 42, 66, 77, 42] }]
        };
      };
      const newLabels = ['Jan', 'Feb', 'Mar', 'Apr', 'May'];
      const newData = [42, 88, 42, 66, 77];
      let { datasets, labels } = { ...this.data };
      // @ts-ignore
      const before = this.chartComponent()?.chart?.data.datasets.length;
      console.log('before', before);
      // console.log('datasets, labels', datasets, labels)
      // @ts-ignore
      // this.data = data()
      this.data = {
        ...this.data,
        datasets: [{ ...this.data.datasets[0], data: newData }, {
          ...this.data.datasets[0],
          borderColor: '#ffbd47',
          data: [88, 42, 66, 77, 42]
        }],
        labels: newLabels
      };
      // console.log('datasets, labels', { datasets, labels } = {...this.data})
      // @ts-ignore
      setTimeout(() => {
        const after = this.chartComponent()?.chart?.data.datasets.length;
        console.log('after', after);
      });
    }, 5000);
  }
}
