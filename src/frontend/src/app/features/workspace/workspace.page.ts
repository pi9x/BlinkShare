import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { AuthStore } from '../../core/auth/auth.store';
import { WorkspaceStore } from './workspace.store';
import { ClipboardItemCardComponent } from '../../shared/ui/clipboard-item-card/clipboard-item-card.component';
import { CodeEditorComponent } from '../../shared/ui/code-editor/code-editor.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/ui/status-badge/status-badge.component';
import { EDITOR_LANGUAGES, EditorLanguage } from '../../shared/models/app.models';
import { copyText, readClipboardText } from '../../shared/utils/clipboard';
import { formatBytes } from '../../shared/utils/bytes';
import { formatDateTime, formatRelativeTime } from '../../shared/utils/time';

@Component({
  selector: 'app-workspace-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ClipboardItemCardComponent,
    CodeEditorComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  providers: [WorkspaceStore],
  template: `
    <div class="grid gap-4 lg:grid-cols-[minmax(0,1.4fr)_22rem]">
      <section class="space-y-4">
        <article class="surface-card rounded-[1.5rem] p-4">
          <div class="grid gap-4 lg:grid-cols-[minmax(0,1fr)_18rem]">
            <div class="space-y-3">
              <div class="flex items-center justify-between gap-3">
                <div>
                  <p class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Your code</p>
                  @if (store.session(); as session) {
                    <h2 class="mt-1 font-code text-4xl font-semibold tracking-[0.28em] text-slate-950">{{ session.code }}</h2>
                  } @else {
                    <h2 class="mt-1 text-2xl font-semibold tracking-tight text-slate-950">Not connected</h2>
                  }
                </div>

                <app-status-badge [label]="store.sessionStatus()" [tone]="sessionTone()" />
              </div>

              <div class="flex flex-wrap gap-2">
                <button class="icon-button icon-button-active" type="button" (click)="store.createSession()" [disabled]="store.busyAction() !== null" aria-label="New code" title="New code">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="M12 5v14" />
                    <path d="M5 12h14" />
                  </svg>
                </button>
                @if (store.session(); as session) {
                  <button class="icon-button" type="button" (click)="copy(session.code)" aria-label="Copy code" title="Copy code">
                    <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                      <rect x="9" y="9" width="11" height="11" rx="2" />
                      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                    </svg>
                  </button>
                  <button class="icon-button" type="button" (click)="store.disconnectSession()" aria-label="Clear session" title="Clear session">
                    <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                      <path d="M18 6 6 18" />
                      <path d="m6 6 12 12" />
                    </svg>
                  </button>
                }
              </div>
            </div>

            <div class="surface-panel rounded-[1.25rem] p-4">
              <label class="field-label">Connect to code</label>
              <div class="mt-2 flex items-center gap-2">
                <input
                  class="field-input font-code uppercase tracking-[0.22em]"
                  [value]="joinCode()"
                  (input)="joinCode.set(($any($event.target).value || '').toUpperCase())"
                  placeholder="ABC123"
                />
                <button class="icon-button" type="button" (click)="store.joinSession(joinCode())" [disabled]="store.busyAction() !== null" aria-label="Connect" title="Connect">
                  <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                    <path d="m5 12 14 0" />
                    <path d="m13 6 6 6-6 6" />
                  </svg>
                </button>
              </div>

              @if (store.session(); as session) {
                <div class="mt-3 text-xs text-slate-500">
                  <span>{{ session.peerCount }} peer{{ session.peerCount === 1 ? '' : 's' }}</span>
                  <span class="mx-2">·</span>
                  <span>{{ store.realtimeStatus() }}</span>
                </div>
              }
            </div>
          </div>

          @if (store.sessionError(); as error) {
            <p class="mt-3 text-sm text-rose-700">{{ error.message }}</p>
          }
        </article>

        <article class="surface-card rounded-[1.5rem] p-4">
          <div class="mb-3 flex flex-wrap items-center justify-between gap-2">
            <div class="flex flex-wrap items-center gap-2">
              <select
                class="field-input min-w-0 max-w-[12rem] py-2"
                [value]="store.draft().language"
                (change)="store.setDraftLanguage($any($event.target).value)"
              >
                @for (language of languages; track language.id) {
                  <option [value]="language.id">{{ language.label }}</option>
                }
              </select>
              <button class="icon-button" [class.icon-button-active]="store.draft().wrap" type="button" (click)="store.setDraftWrap(!store.draft().wrap)" aria-label="Toggle wrap" title="Toggle wrap">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M4 7h11a4 4 0 1 1 0 8H9" />
                  <path d="m9 11-4 4 4 4" />
                  <path d="M4 17h5" />
                </svg>
              </button>
            </div>

            <div class="flex flex-wrap gap-2">
              <button class="icon-button" type="button" (click)="pasteIntoEditor()" aria-label="Paste" title="Paste">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2v-2" />
                  <rect x="4" y="2" width="8" height="8" rx="2" />
                  <path d="M8 10v8" />
                  <path d="M4 14h8" />
                </svg>
              </button>
              <button class="icon-button" type="button" (click)="store.clearDraft()" aria-label="Clear draft" title="Clear draft">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M3 6h18" />
                  <path d="M8 6V4h8v2" />
                  <path d="m19 6-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
                </svg>
              </button>
            </div>
          </div>

          <div [class]="store.draft().fullscreen ? 'fixed inset-3 z-40 rounded-[1.5rem] bg-[#020617] p-3 shadow-2xl' : ''">
            <app-code-editor
              [value]="store.draft().text"
              [language]="store.draft().language"
              [wrap]="store.draft().wrap"
              (contentChanged)="store.setDraftText($event)"
            />
          </div>

          <div class="mt-3 grid gap-2 sm:grid-cols-4">
            <button class="icon-button icon-button-active w-full sm:w-auto justify-self-start" type="button" (click)="store.sendDraftToSession()" [disabled]="store.busyAction() !== null" aria-label="Send to session" title="Send to session">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M22 2 11 13" />
                <path d="M22 2 15 22l-4-9-9-4Z" />
              </svg>
            </button>
            <button class="icon-button w-full sm:w-auto justify-self-start" type="button" (click)="store.createTextShare()" [disabled]="store.busyAction() !== null" aria-label="Create share" title="Create share">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <circle cx="18" cy="5" r="3" />
                <circle cx="6" cy="12" r="3" />
                <circle cx="18" cy="19" r="3" />
                <path d="m8.6 13.5 6.8 4" />
                <path d="m15.4 6.5-6.8 4" />
              </svg>
            </button>
            <button class="icon-button w-full sm:w-auto justify-self-start" [class.icon-button-active]="store.draft().fullscreen" type="button" (click)="toggleFullscreen()" aria-label="Toggle fullscreen" title="Toggle fullscreen">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M8 3H3v5" />
                <path d="M21 8V3h-5" />
                <path d="M3 16v5h5" />
                <path d="M16 21h5v-5" />
              </svg>
            </button>
            <a class="icon-button w-full sm:w-auto justify-self-start" [routerLink]="store.latestShare() ? ['/share', store.latestShare()!.code] : ['/']" aria-label="Open last share" title="Open last share">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M15 3h6v6" />
                <path d="M10 14 21 3" />
                <path d="M21 14v5a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5" />
              </svg>
            </a>
          </div>
        </article>

        <article class="surface-card rounded-[1.5rem] p-4">
          <div class="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p class="text-sm font-semibold text-slate-950">Files</p>
              @if (selectedFileName()) {
                <p class="text-sm text-slate-500">{{ selectedFileName() }} · {{ selectedFileMeta() }}</p>
              } @else {
                <p class="text-sm text-slate-500">Pick a file, then share or send metadata.</p>
              }
            </div>

            <button class="icon-button" type="button" (click)="fileInput.click()" aria-label="Choose file" title="Choose file">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                <path d="M17 8 12 3 7 8" />
                <path d="M12 3v12" />
              </svg>
            </button>
          </div>

          <input #fileInput type="file" class="hidden" (change)="onFileChosen($event)" />

          @if (selectedFileName()) {
            <div class="mt-3 grid gap-2 sm:grid-cols-2">
              <button class="icon-button icon-button-active w-full sm:w-auto justify-self-start" type="button" (click)="shareFile()" [disabled]="store.busyAction() !== null" aria-label="Create file share" title="Create file share">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                  <path d="M14 2v6h6" />
                </svg>
              </button>
              <button class="icon-button w-full sm:w-auto justify-self-start" type="button" (click)="relayFileMetadata()" [disabled]="store.busyAction() !== null" aria-label="Send file info" title="Send file info">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M22 2 11 13" />
                  <path d="M22 2 15 22l-4-9-9-4Z" />
                </svg>
              </button>
            </div>
          }
        </article>
      </section>

      <aside class="space-y-4">
        @if (store.latestShare(); as latestShare) {
          <article class="surface-card rounded-[1.5rem] p-4">
            <p class="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">Last share</p>
            <h3 class="mt-1 font-code text-3xl font-semibold tracking-[0.24em] text-slate-950">{{ latestShare.code }}</h3>
            <p class="mt-2 text-sm text-slate-500">{{ formatRelativeTime(latestShare.expiresAtUtc) }}</p>
            <div class="mt-3 flex gap-2">
              <button class="icon-button icon-button-active" type="button" (click)="copy(latestShare.code)" aria-label="Copy share code" title="Copy share code">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <rect x="9" y="9" width="11" height="11" rx="2" />
                  <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
                </svg>
              </button>
              <a class="icon-button" [routerLink]="['/share', latestShare.code]" aria-label="Open share" title="Open share">
                <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                  <path d="M15 3h6v6" />
                  <path d="M10 14 21 3" />
                  <path d="M21 14v5a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5" />
                </svg>
              </a>
            </div>
          </article>
        }

        <article class="surface-card rounded-[1.5rem] p-4">
          <div class="mb-3 flex items-center justify-between gap-2">
            <p class="text-sm font-semibold text-slate-950">Recent</p>
            <button class="icon-button h-9 w-9" type="button" (click)="store.clearHistory()" aria-label="Clear history" title="Clear history">
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M3 6h18" />
                <path d="M8 6V4h8v2" />
                <path d="m19 6-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
              </svg>
            </button>
          </div>

          <div class="space-y-3">
            @if (store.history().length) {
              @for (item of store.history(); track item.id) {
                <app-clipboard-item-card [item]="item" (copy)="copyItem(item)" />
              }
            } @else {
              <app-empty-state eyebrow="Empty" title="Nothing yet" copy="Sent and received items show up here." />
            }
          </div>
        </article>

        <article class="surface-card rounded-[1.5rem] p-4 text-sm text-slate-500">
          <div class="flex items-center justify-between gap-2">
            <span>{{ authStore.authenticated() ? 'Account' : 'Anonymous' }}</span>
            <span>{{ store.health()?.status || 'offline' }}</span>
          </div>
        </article>
      </aside>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkspacePageComponent {
  private readonly route = inject(ActivatedRoute);
  protected readonly store = inject(WorkspaceStore);
  protected readonly authStore = inject(AuthStore);
  protected readonly joinCode = signal('');
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly languages = EDITOR_LANGUAGES;

  @ViewChild('fileInput') private readonly fileInput?: { nativeElement: HTMLInputElement };

  public constructor() {
    const routeCode = this.route.snapshot.paramMap.get('code');
    const queryCode = this.route.snapshot.queryParamMap.get('code');
    const joinCode = routeCode ?? queryCode;

    this.joinCode.set((joinCode ?? '').toUpperCase());
    void this.store.initialize(joinCode);
  }

  protected sessionTone(): 'neutral' | 'success' | 'warning' | 'danger' {
    switch (this.store.sessionStatus()) {
      case 'connected':
        return 'success';
      case 'reconnecting':
        return 'warning';
      case 'error':
        return 'danger';
      default:
        return 'neutral';
    }
  }

  protected async pasteIntoEditor(): Promise<void> {
    const text = await readClipboardText();
    if (text) {
      this.store.setDraftText(text);
    }
  }

  protected toggleFullscreen(): void {
    this.store.setDraftFullscreen(!this.store.draft().fullscreen);
  }

  protected onFileChosen(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  protected selectedFileName(): string {
    return this.selectedFile()?.name ?? '';
  }

  protected selectedFileMeta(): string {
    const file = this.selectedFile();
    if (!file) {
      return '';
    }

    return `${file.type || 'application/octet-stream'} · ${formatBytes(file.size)}`;
  }

  protected async shareFile(): Promise<void> {
    const file = this.selectedFile();
    if (!file) {
      return;
    }

    await this.store.createFileShare(file);
  }

  protected async relayFileMetadata(): Promise<void> {
    const file = this.selectedFile();
    if (!file) {
      return;
    }

    await this.store.publishFileMetadata(file);
  }

  protected async copy(value: string): Promise<void> {
    await copyText(value);
  }

  protected async copyItem(item: { kind: string; text?: string; fileName?: string }): Promise<void> {
    if (item.kind === 'text') {
      await copyText(item.text ?? '');
      return;
    }

    await copyText(item.fileName ?? '');
  }

  protected readonly formatDateTime = formatDateTime;
  protected readonly formatRelativeTime = formatRelativeTime;
}
