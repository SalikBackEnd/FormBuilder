import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" role="region" aria-label="Notifications" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast--' + toast.type" role="alert">

          <div class="toast__accent"></div>

          <div class="toast__icon">
            @switch (toast.type) {
              @case ('success') {
                <svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z" clip-rule="evenodd" />
                </svg>
              }
              @case ('error') {
                <svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z" clip-rule="evenodd" />
                </svg>
              }
              @case ('warning') {
                <svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M8.485 2.495c.673-1.167 2.357-1.167 3.03 0l6.28 10.875c.673 1.167-.17 2.625-1.516 2.625H3.72c-1.347 0-2.189-1.458-1.515-2.625L8.485 2.495zM10 5a.75.75 0 01.75.75v3.5a.75.75 0 01-1.5 0v-3.5A.75.75 0 0110 5zm0 9a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                </svg>
              }
              @case ('info') {
                <svg viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a.75.75 0 000 1.5h.253a.25.25 0 01.244.304l-.459 2.066A1.75 1.75 0 0010.747 15H11a.75.75 0 000-1.5h-.253a.25.25 0 01-.244-.304l.459-2.066A1.75 1.75 0 009.253 9H9z" clip-rule="evenodd" />
                </svg>
              }
            }
          </div>

          <div class="toast__body">
            <p class="toast__message">{{ toast.message }}</p>
            @if (toast.errors && toast.errors.length > 0) {
              <ul class="toast__errors">
                @for (e of toast.errors; track e) {
                  <li>{{ e }}</li>
                }
              </ul>
            }
          </div>

          <button class="toast__close" (click)="toastService.dismiss(toast.id)" aria-label="Dismiss">
            <svg viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
              <path d="M3.72 3.72a.75.75 0 011.06 0L8 6.94l3.22-3.22a.75.75 0 111.06 1.06L9.06 8l3.22 3.22a.75.75 0 11-1.06 1.06L8 9.06l-3.22 3.22a.75.75 0 01-1.06-1.06L6.94 8 3.72 4.78a.75.75 0 010-1.06z" />
            </svg>
          </button>

          <div class="toast__progress" [style.animation-duration]="toast.durationMs + 'ms'"></div>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 1.25rem;
      right: 1.25rem;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 0.625rem;
      width: 380px;
      max-width: calc(100vw - 2.5rem);
      pointer-events: none;
    }

    /* ── Toast shell ─────────────────────────────────────── */
    .toast {
      position: relative;
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.875rem 1rem 0.875rem 0.875rem;
      border-radius: 10px;
      border: 1px solid transparent;
      overflow: hidden;
      pointer-events: all;
      animation: toast-in 0.28s cubic-bezier(0.34, 1.56, 0.64, 1) both;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.35);
    }

    /* ── Accent bar (left edge) ──────────────────────────── */
    .toast__accent {
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 4px;
      border-radius: 10px 0 0 10px;
    }

    /* ── Per-type colours ────────────────────────────────── */
    .toast--success {
      background: rgba(15, 23, 18, 0.92);
      border-color: rgba(34, 197, 94, 0.25);
      color: #dcfce7;
    }
    .toast--success .toast__accent { background: #22c55e; }
    .toast--success .toast__icon   { color: #22c55e; }
    .toast--success .toast__progress { background: #22c55e; }

    .toast--error {
      background: rgba(23, 12, 12, 0.92);
      border-color: rgba(239, 68, 68, 0.25);
      color: #fee2e2;
    }
    .toast--error .toast__accent { background: #ef4444; }
    .toast--error .toast__icon   { color: #ef4444; }
    .toast--error .toast__progress { background: #ef4444; }

    .toast--warning {
      background: rgba(22, 18, 10, 0.92);
      border-color: rgba(245, 158, 11, 0.25);
      color: #fef3c7;
    }
    .toast--warning .toast__accent { background: #f59e0b; }
    .toast--warning .toast__icon   { color: #f59e0b; }
    .toast--warning .toast__progress { background: #f59e0b; }

    .toast--info {
      background: rgba(10, 16, 26, 0.92);
      border-color: rgba(59, 130, 246, 0.25);
      color: #dbeafe;
    }
    .toast--info .toast__accent { background: #3b82f6; }
    .toast--info .toast__icon   { color: #3b82f6; }
    .toast--info .toast__progress { background: #3b82f6; }

    /* ── Icon ────────────────────────────────────────────── */
    .toast__icon {
      flex-shrink: 0;
      margin-left: 4px; /* clear the accent bar */
    }
    .toast__icon svg {
      width: 20px;
      height: 20px;
      display: block;
    }

    /* ── Body ────────────────────────────────────────────── */
    .toast__body {
      flex: 1;
      min-width: 0;
    }

    .toast__message {
      margin: 0;
      font-size: 0.875rem;
      font-weight: 500;
      line-height: 1.4;
      word-break: break-word;
    }

    .toast__errors {
      margin: 0.4rem 0 0;
      padding-left: 1.1rem;
      list-style: disc;
      font-size: 0.8rem;
      opacity: 0.8;
      display: flex;
      flex-direction: column;
      gap: 0.15rem;
    }

    /* ── Close button ────────────────────────────────────── */
    .toast__close {
      flex-shrink: 0;
      background: transparent;
      border: none;
      color: inherit;
      opacity: 0.45;
      cursor: pointer;
      padding: 0;
      line-height: 0;
      transition: opacity 0.15s;
      margin-top: 1px;
    }
    .toast__close:hover { opacity: 1; }
    .toast__close svg { width: 14px; height: 14px; display: block; }

    /* ── Progress bar ────────────────────────────────────── */
    .toast__progress {
      position: absolute;
      bottom: 0;
      left: 0;
      height: 3px;
      width: 100%;
      opacity: 0.6;
      animation: toast-progress linear forwards;
      transform-origin: left center;
    }

    /* ── Animations ──────────────────────────────────────── */
    @keyframes toast-in {
      from { opacity: 0; transform: translateX(18px) scale(0.97); }
      to   { opacity: 1; transform: translateX(0)    scale(1);    }
    }

    @keyframes toast-progress {
      from { transform: scaleX(1); }
      to   { transform: scaleX(0); }
    }
  `]
})
export class ToastContainerComponent {
  constructor(public toastService: ToastService) {}
}
