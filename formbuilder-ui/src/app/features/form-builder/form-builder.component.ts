import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  Client,
  FormDto,
  FormFieldDto,
  CreateFormFieldRequest,
  UpdateFormFieldRequest,
  UpdateFormRequest,
  UpdateContactSettingsRequest,
  ReorderFieldsRequest
} from '../../core/api/form-builder-api';
import { ToastService } from '../../core/services/toast.service';

export enum FieldType {
  Text = 'Text',
  TextArea = 'TextArea',
  Number = 'Number',
  Date = 'Date',
  Dropdown = 'Dropdown',
  Checkbox = 'Checkbox',
  Radio = 'Radio',
  Email = 'Email',
}

interface FieldFormState {
  label: string;
  fieldType: string;
  placeholder: string;
  isRequired: boolean;
  options: string;
  minLength: number | null;
  maxLength: number | null;
  minValue: number | null;
  maxValue: number | null;
}

const BLANK_FIELD: FieldFormState = {
  label: '',
  fieldType: FieldType.Text,
  placeholder: '',
  isRequired: false,
  options: '',
  minLength: null,
  maxLength: null,
  minValue: null,
  maxValue: null,
};

@Component({
  selector: 'app-form-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './form-builder.component.html',
  styleUrls: ['./form-builder.component.css']
})
export class FormBuilderComponent implements OnInit {
  public formId = signal<string>('');
  public form = signal<FormDto | null>(null);
  public fields = signal<FormFieldDto[]>([]);
  public isLoading = signal<boolean>(true);

  public fieldTypes = Object.values(FieldType);

  // Add-field panel state
  public isAddingField = signal<boolean>(false);
  public newField = signal<FieldFormState>({ ...BLANK_FIELD });

  // Edit-field panel state
  public editingFieldId = signal<string | null>(null);
  public editField = signal<FieldFormState>({ ...BLANK_FIELD });

  // Form meta (title/description) edit state
  public isEditingMeta = signal<boolean>(false);
  public editTitle = signal<string>('');
  public editDescription = signal<string>('');
  public isSavingMeta = signal<boolean>(false);

  // Contact field settings
  public collectName = signal<boolean>(true);
  public collectEmail = signal<boolean>(true);
  public nameRequired = signal<boolean>(false);
  public emailRequired = signal<boolean>(false);
  public isSavingContactSettings = signal<boolean>(false);

  // Confirmation modal for removing a contact field
  public removeContactFieldPending = signal<'name' | 'email' | null>(null);

  public hasOptionsType(fieldType: string): boolean {
    return fieldType === FieldType.Dropdown || fieldType === FieldType.Radio || fieldType === FieldType.Checkbox;
  }

  public hasLengthConstraints(fieldType: string): boolean {
    return fieldType === FieldType.Text || fieldType === FieldType.TextArea;
  }

  public hasValueConstraints(fieldType: string): boolean {
    return fieldType === FieldType.Number;
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private client: Client,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') || '';
    this.formId.set(id);
    if (id) {
      this.loadForm();
    }
  }

  public loadForm(): void {
    this.isLoading.set(true);
    this.client.formsGET(this.formId()).subscribe({
      next: (f) => {
        this.form.set(f);
        const sortedFields = (f.fields || []).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
        this.fields.set(sortedFields);
        this.collectName.set(f.collectSubmitterName ?? true);
        this.collectEmail.set(f.collectSubmitterEmail ?? true);
        this.nameRequired.set(f.submitterNameRequired ?? false);
        this.emailRequired.set(f.submitterEmailRequired ?? false);
        this.isLoading.set(false);
      },
      error: () => {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  // ── Meta editing ────────────────────────────────────────────────
  public startEditMeta(): void {
    const f = this.form();
    this.editTitle.set(f?.title || '');
    this.editDescription.set(f?.description || '');
    this.isEditingMeta.set(true);
  }

  public cancelEditMeta(): void {
    this.isEditingMeta.set(false);
  }

  public saveMeta(): void {
    const title = this.editTitle().trim();
    if (!title) return;
    this.isSavingMeta.set(true);
    this.client.formsPUT(this.formId(), new UpdateFormRequest({ title, description: this.editDescription().trim() })).subscribe({
      next: (updated) => {
        this.form.update(f => f ? { ...f, title: updated.title, description: updated.description } as FormDto : f);
        this.isEditingMeta.set(false);
        this.isSavingMeta.set(false);
        this.toast.success('Form details saved.');
      },
      error: (err) => {
        this.isSavingMeta.set(false);
        this.toast.apiError(err);
      }
    });
  }

  // ── Contact field settings ──────────────────────────────────────
  private saveContactSettings(): void {
    this.isSavingContactSettings.set(true);
    const req = new UpdateContactSettingsRequest({
      collectSubmitterName: this.collectName(),
      collectSubmitterEmail: this.collectEmail(),
      submitterNameRequired: this.nameRequired(),
      submitterEmailRequired: this.emailRequired(),
    });
    this.client.contactSettings(this.formId(), req).subscribe({
      next: (updated) => {
        this.form.update(f => f ? { ...f,
          collectSubmitterName: updated.collectSubmitterName,
          collectSubmitterEmail: updated.collectSubmitterEmail,
          submitterNameRequired: updated.submitterNameRequired,
          submitterEmailRequired: updated.submitterEmailRequired,
        } as FormDto : f);
        this.isSavingContactSettings.set(false);
      },
      error: (err) => {
        this.isSavingContactSettings.set(false);
        this.toast.apiError(err);
      }
    });
  }

  public toggleNameRequired(): void {
    this.nameRequired.update(v => !v);
    this.saveContactSettings();
  }

  public toggleEmailRequired(): void {
    this.emailRequired.update(v => !v);
    this.saveContactSettings();
  }

  public requestRemoveContactField(which: 'name' | 'email'): void {
    this.removeContactFieldPending.set(which);
  }

  public cancelRemoveContactField(): void {
    this.removeContactFieldPending.set(null);
  }

  public confirmRemoveContactField(): void {
    const which = this.removeContactFieldPending();
    if (which === 'name') {
      this.collectName.set(false);
      this.nameRequired.set(false);
    } else if (which === 'email') {
      this.collectEmail.set(false);
      this.emailRequired.set(false);
    }
    this.removeContactFieldPending.set(null);
    this.saveContactSettings();
  }

  public addBackContactField(which: 'name' | 'email'): void {
    if (which === 'name') this.collectName.set(true);
    else this.collectEmail.set(true);
    this.saveContactSettings();
  }

  // ── Publish / Unpublish ─────────────────────────────────────────
  public togglePublish(): void {
    const f = this.form();
    if (!f) return;
    const action = f.isPublished
      ? this.client.unpublish(this.formId())
      : this.client.publish(this.formId());

    action.subscribe({
      next: () => {
        this.form.update(current => current ? { ...current, isPublished: !current.isPublished } as FormDto : current);
        this.toast.success(f.isPublished ? 'Form unpublished.' : 'Form published!');
      },
      error: (err) => this.toast.apiError(err)
    });
  }

  public copyPublicLink(): void {
    const slug = this.form()?.publicSlug;
    if (!slug) return;
    const url = `${window.location.origin}/f/${slug}`;
    navigator.clipboard.writeText(url).then(() => this.toast.success('Link copied!'));
  }

  // ── Add field ───────────────────────────────────────────────────
  public toggleAddField(): void {
    this.isAddingField.set(!this.isAddingField());
    if (!this.isAddingField()) {
      this.newField.set({ ...BLANK_FIELD });
    }
    // Close edit panel if open
    this.editingFieldId.set(null);
  }

  public updateNewField(patch: Partial<FieldFormState>): void {
    this.newField.update(current => ({ ...current, ...patch }));
  }

  public addField(): void {
    const f = this.newField();
    if (!f.label?.trim()) return;

    const request = new CreateFormFieldRequest({
      label: f.label.trim(),
      fieldType: f.fieldType,
      placeholder: f.placeholder,
      isRequired: f.isRequired,
      options: this.hasOptionsType(f.fieldType)
        ? f.options.split(',').map(o => o.trim()).filter(o => o)
        : [],
      minLength: f.minLength ?? undefined,
      maxLength: f.maxLength ?? undefined,
      minValue: f.minValue ?? undefined,
      maxValue: f.maxValue ?? undefined,
    });

    this.client.fieldsPOST(this.formId(), request).subscribe({
      next: (field) => {
        this.fields.update(current => [...current, field]);
        this.newField.set({ ...BLANK_FIELD });
        this.isAddingField.set(false);
        this.toast.success('Field added.');
      },
      error: (err) => this.toast.apiError(err)
    });
  }

  // ── Edit field ──────────────────────────────────────────────────
  public startEditField(field: FormFieldDto): void {
    this.editingFieldId.set(field.id!);
    this.editField.set({
      label: field.label || '',
      fieldType: field.fieldType || FieldType.Text,
      placeholder: field.placeholder || '',
      isRequired: field.isRequired || false,
      options: (field.options || []).join(', '),
      minLength: field.minLength || null,
      maxLength: field.maxLength || null,
      minValue: field.minValue || null,
      maxValue: field.maxValue || null,
    });
    // Close add panel
    this.isAddingField.set(false);
  }

  public cancelEditField(): void {
    this.editingFieldId.set(null);
  }

  public updateEditField(patch: Partial<FieldFormState>): void {
    this.editField.update(current => ({ ...current, ...patch }));
  }

  public saveEditField(fieldId: string): void {
    const f = this.editField();
    if (!f.label?.trim()) return;

    const request = new UpdateFormFieldRequest({
      label: f.label.trim(),
      fieldType: f.fieldType,
      placeholder: f.placeholder,
      isRequired: f.isRequired,
      options: this.hasOptionsType(f.fieldType)
        ? f.options.split(',').map(o => o.trim()).filter(o => o)
        : [],
      minLength: f.minLength ?? undefined,
      maxLength: f.maxLength ?? undefined,
      minValue: f.minValue ?? undefined,
      maxValue: f.maxValue ?? undefined,
    });

    this.client.fieldsPUT(this.formId(), fieldId, request).subscribe({
      next: (updated) => {
        this.fields.update(current =>
          current.map(field => field.id === fieldId ? updated : field)
        );
        this.editingFieldId.set(null);
        this.toast.success('Field updated.');
      },
      error: (err) => this.toast.apiError(err)
    });
  }

  // ── Delete field ────────────────────────────────────────────────
  public deleteField(fieldId: string): void {
    if (!confirm('Remove this field?')) return;
    this.client.fieldsDELETE(this.formId(), fieldId).subscribe({
      next: () => {
        this.fields.update(current => current.filter(f => f.id !== fieldId));
        if (this.editingFieldId() === fieldId) {
          this.editingFieldId.set(null);
        }
      },
      error: (err) => this.toast.apiError(err)
    });
  }

  // ── Reorder ─────────────────────────────────────────────────────
  public moveUp(index: number): void {
    if (index === 0) return;
    this.swap(index, index - 1);
  }

  public moveDown(index: number): void {
    if (index === this.fields().length - 1) return;
    this.swap(index, index + 1);
  }

  private swap(indexA: number, indexB: number): void {
    const current = [...this.fields()];
    [current[indexA], current[indexB]] = [current[indexB], current[indexA]];

    const fieldOrders: { [key: string]: number } = {};
    current.forEach((f, i) => {
      f.sortOrder = i;
      fieldOrders[f.id!] = i;
    });

    this.fields.set(current);
    this.client.reorder(this.formId(), new ReorderFieldsRequest({ fieldOrders })).subscribe();
  }

  // ── Preview helpers ─────────────────────────────────────────────
  public getPreviewInputType(fieldType: string | undefined): string {
    switch (fieldType) {
      case 'Email': return 'email';
      case 'Number': return 'number';
      case 'Date': return 'date';
      default: return 'text';
    }
  }

  public isSimpleInput(fieldType: string | undefined): boolean {
    return !['TextArea', 'Dropdown', 'Radio', 'Checkbox'].includes(fieldType || '');
  }
}
