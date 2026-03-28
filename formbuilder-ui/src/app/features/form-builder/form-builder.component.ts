import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Client, FormDto, FormFieldDto, CreateFormFieldRequest, ReorderFieldsRequest } from '../../core/api/form-builder-api';
import { ToastService } from '../../core/services/toast.service';

export enum FieldType {
  Text = 'Text',
  Number = 'Number',
  Email = 'Email',
  Textarea = 'Textarea',
  Select = 'Select',
  Radio = 'Radio',
  Checkbox = 'Checkbox'
}

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
  public isAddingField = signal<boolean>(false);
  public newField = signal<any>({
    label: '',
    fieldType: FieldType.Text,
    placeholder: '',
    isRequired: false,
    options: ''
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private client: Client,
    private toast: ToastService
  ) {}

  public updateNewField(patch: any): void {
    this.newField.update(current => ({ ...current, ...patch }));
  }

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
        const sortedFields = (f.fields || []).sort((a,b) => (a.sortOrder || 0) - (b.sortOrder || 0));
        this.fields.set(sortedFields);
        this.isLoading.set(false);
      },
      error: () => {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  public toggleAddField(): void {
    this.isAddingField.set(!this.isAddingField());
    if (!this.isAddingField()) {
      this.resetNewField();
    }
  }

  private resetNewField(): void {
    this.newField.set({
      label: '',
      fieldType: FieldType.Text,
      placeholder: '',
      isRequired: false,
      options: ''
    });
  }

  public addField(): void {
    const currentNewField = this.newField();
    if (!currentNewField.label) return;

    let optionsArray: string[] = [];
    if (currentNewField.options && (currentNewField.fieldType === FieldType.Select || currentNewField.fieldType === FieldType.Radio || currentNewField.fieldType === FieldType.Checkbox)) {
       optionsArray = currentNewField.options.split(',').map((o: string) => o.trim()).filter((o: string) => o);
    }

    const request = new CreateFormFieldRequest({
      label: currentNewField.label,
      fieldType: currentNewField.fieldType,
      placeholder: currentNewField.placeholder,
      isRequired: currentNewField.isRequired,
      options: optionsArray
    });

    this.client.fieldsPOST(this.formId(), request).subscribe({
      next: (field) => {
        this.fields.update(current => [...current, field]);
        this.toggleAddField();
      },
      error: () => {
        this.toast.error('Failed to add field. Please try again.');
      }
    });
  }

  public deleteField(fieldId: string): void {
    if (!confirm('Are you sure you want to remove this field?')) return;
    this.client.fieldsDELETE(this.formId(), fieldId).subscribe(() => {
      this.fields.update(current => current.filter(f => f.id !== fieldId));
    });
  }

  public moveUp(index: number): void {
    if (index === 0) return;
    this.swap(index, index - 1);
  }

  public moveDown(index: number): void {
    const currentFields = this.fields();
    if (index === currentFields.length - 1) return;
    this.swap(index, index + 1);
  }

  private swap(indexA: number, indexB: number): void {
    const currentFields = [...this.fields()];
    const fieldA = currentFields[indexA];
    const fieldB = currentFields[indexB];
    
    currentFields[indexA] = fieldB;
    currentFields[indexB] = fieldA;

    const fieldOrders: { [key: string]: number } = {};
    for (let i = 0; i < currentFields.length; i++) {
        currentFields[i].sortOrder = i;
        fieldOrders[currentFields[i].id!] = i;
    }

    this.fields.set(currentFields);

    this.client.reorder(this.formId(), new ReorderFieldsRequest({
       fieldOrders: fieldOrders
    })).subscribe();
  }
}
