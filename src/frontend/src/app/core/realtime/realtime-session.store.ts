import { Injectable, computed, signal } from '@angular/core';

export type RealtimeConnectionStatus =
  | 'idle'
  | 'connected'
  | 'reconnecting'
  | 'disconnected';

@Injectable({ providedIn: 'root' })
export class RealtimeSessionStore {
  private readonly statusState = signal<RealtimeConnectionStatus>('idle');

  public readonly status = computed(() => this.statusState());

  public markIdle(): void {
    this.statusState.set('idle');
  }

  public markConnected(): void {
    this.statusState.set('connected');
  }

  public markReconnecting(): void {
    this.statusState.set('reconnecting');
  }

  public markDisconnected(): void {
    this.statusState.set('disconnected');
  }
}
