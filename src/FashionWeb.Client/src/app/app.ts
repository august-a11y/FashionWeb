import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {HeaderLayoutComponent} from './shared/header-layout/header-layout.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, HeaderLayoutComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('FashionWeb.Client');
}
