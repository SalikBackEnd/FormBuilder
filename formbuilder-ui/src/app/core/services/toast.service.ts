import { Injectable, signal } from '@angular/core';
import { extractApiError } from '../utils/api-error';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
  errors?: string[];
  durationMs: number;
}

const MAX_TOASTS = 5;

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 0;
  public toasts = signal<Toast[]>([]);

  public show(message: string, type: ToastType = 'info', durationMs = 4000, errors?: string[]): void {
    const id = this.nextId++;
    const toast: Toast = { id, message, type, errors, durationMs };
    this.toasts.update(ts => {
      const next = [...ts, toast];
      return next.length > MAX_TOASTS ? next.slice(next.length - MAX_TOASTS) : next;
    });
    setTimeout(() => this.dismiss(id), durationMs);
  }

  public success(message: string): void { this.show(message, 'success', 4000); }
  public error(message: string, errors?: string[]): void { this.show(message, 'error', 6000, errors); }
  public warn(message: string): void { this.show(message, 'warning', 5000); }
  public info(message: string): void { this.show(message, 'info', 4000); }

  public apiError(err: any): void {
    const { message, errors } = extractApiError(err);
    this.error(message, errors);
  }

  public dismiss(id: number): void {
    this.toasts.update(ts => ts.filter(t => t.id !== id));
  }
}
