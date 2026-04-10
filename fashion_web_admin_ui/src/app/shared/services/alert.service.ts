import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Alert {
    message: string;
    type: 'success' | 'error' | 'warning' | 'info';
    duration?: number;
    
}

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private alertSubject = new BehaviorSubject<Alert | null>(null);
    public alert$: Observable<Alert | null> = this.alertSubject.asObservable();
    private messageService = inject(MessageService);

    constructor() { }

    success(message: string, duration: number = 3000): void {
        this.show(message, 'success', duration);
    }

    error(message: string, duration: number = 3000): void {
        this.show(message, 'error', duration);
    }

    warning(message: string, duration: number = 3000): void {
        this.show(message, 'warning', duration);
    }

    info(message: string, duration: number = 3000): void {
        this.show(message, 'info', duration);
    }

    private show(message: string, type: Alert['type'], duration: number): void {
        this.messageService.add({
            severity: type === 'warning' ? 'warn' : type,
            summary: type.toUpperCase(),
            detail: message,
            life: duration
        });
        this.alertSubject.next({ message, type, duration });

        if (duration > 0) {
            setTimeout(() => {
                this.alertSubject.next(null);
            }, duration);
        }
    }

    clear(): void {
        this.alertSubject.next(null);
    }
}