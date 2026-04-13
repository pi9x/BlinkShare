import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  ViewChild,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { Compartment, EditorState } from '@codemirror/state';
import { EditorView, lineNumbers } from '@codemirror/view';
import { css } from '@codemirror/lang-css';
import { html } from '@codemirror/lang-html';
import { javascript } from '@codemirror/lang-javascript';
import { json } from '@codemirror/lang-json';

import { EditorLanguage } from '../../models/app.models';

@Component({
  selector: 'app-code-editor',
  standalone: true,
  imports: [CommonModule],
  template: ` <div #host class="min-h-[18rem] w-full overflow-hidden rounded-[1.25rem] border border-slate-200 bg-slate-950"></div> `,
  styles: [
    `
      :host {
        display: block;
      }

      :host ::ng-deep .cm-editor {
        min-height: 18rem;
        background: transparent;
        color: #e2e8f0;
      }

      :host ::ng-deep .cm-scroller {
        font-family:
          "IBM Plex Mono",
          "SFMono-Regular",
          Consolas,
          monospace;
        line-height: 1.6;
        padding: 1rem 0;
      }

      :host ::ng-deep .cm-gutters {
        border-right: 1px solid rgb(148 163 184 / 0.18);
        background: rgb(15 23 42 / 0.72);
        color: rgb(148 163 184 / 0.9);
      }

      :host ::ng-deep .cm-content,
      :host ::ng-deep .cm-gutter {
        min-height: 18rem;
      }

      :host ::ng-deep .cm-activeLine,
      :host ::ng-deep .cm-activeLineGutter {
        background: rgb(30 41 59 / 0.85);
      }

      :host ::ng-deep .cm-focused {
        outline: none;
      }

      :host ::ng-deep .cm-selectionBackground {
        background: rgb(13 148 136 / 0.38) !important;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeEditorComponent {
  private readonly languageCompartment = new Compartment();
  private readonly readOnlyCompartment = new Compartment();
  private readonly wrapCompartment = new Compartment();
  private readonly destroyRef = inject(DestroyRef);

  private view: EditorView | null = null;

  public readonly value = input('');
  public readonly language = input<EditorLanguage>('plaintext');
  public readonly readOnly = input(false);
  public readonly wrap = input(true);
  public readonly contentChanged = output<string>();

  @ViewChild('host', { static: true }) private readonly hostElement?: ElementRef<HTMLDivElement>;

  public constructor() {
    effect(() => {
      if (!this.view) {
        return;
      }

      const nextValue = this.value();
      const currentValue = this.view.state.doc.toString();
      if (nextValue !== currentValue) {
        this.view.dispatch({
          changes: {
            from: 0,
            to: currentValue.length,
            insert: nextValue,
          },
        });
      }
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: this.languageCompartment.reconfigure(this.createLanguageExtension(this.language())),
      });
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: this.readOnlyCompartment.reconfigure(EditorState.readOnly.of(this.readOnly())),
      });
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: this.wrapCompartment.reconfigure(this.wrap() ? EditorView.lineWrapping : []),
      });
    });
  }

  public ngAfterViewInit(): void {
    const host = this.hostElement?.nativeElement;
    if (!host) {
      return;
    }

    this.view = new EditorView({
      state: EditorState.create({
        doc: this.value(),
        extensions: [
          lineNumbers(),
          this.languageCompartment.of(this.createLanguageExtension(this.language())),
          this.readOnlyCompartment.of(EditorState.readOnly.of(this.readOnly())),
          this.wrapCompartment.of(this.wrap() ? EditorView.lineWrapping : []),
          EditorView.theme({
            '&': {
              height: '100%',
            },
          }),
          EditorView.updateListener.of((update) => {
            if (update.docChanged) {
              this.contentChanged.emit(update.state.doc.toString());
            }
          }),
        ],
      }),
      parent: host,
    });

    this.destroyRef.onDestroy(() => {
      this.view?.destroy();
      this.view = null;
    });
  }

  private createLanguageExtension(language: EditorLanguage) {
    switch (language) {
      case 'javascript':
        return javascript();
      case 'json':
        return json();
      case 'html':
        return html();
      case 'css':
        return css();
      default:
        return [];
    }
  }
}
