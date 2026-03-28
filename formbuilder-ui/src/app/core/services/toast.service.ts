import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private nextId = 0;
  public toasts = signal<Toast[]>([]);

  public show(message: string, type: ToastType = 'info', durationMs = 4000): void {
    const id = this.nextId++;
    const toast: Toast = { id, message, type };
    this.toasts.update(current => [...current, toast]);

    setTimeout(() => this.dismiss(id), durationMs);
  }

  public success(message: string): void {
    this.show(message, 'success');
  }

  public error(message: string): void {
    this.show(message, 'error', 5000);
  }

  public info(message: string): void {
    this.show(message, 'info');
  }

  public dismiss(id: number): void {
    this.toasts.update(current => current.filter(t => t.id !== id));
  }
}
