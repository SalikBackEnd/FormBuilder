import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Client, FormDto, FormFieldDto, SubmitFormRequest, SubmissionValueDto } from '../../core/api/form-builder-api';

@Component({
  selector: 'app-public-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './public-form.component.html',
  styleUrls: ['./public-form.component.css']
})
export class PublicFormComponent implements OnInit {
  public slug = signal<string>('');
  public form = signal<FormDto | null>(null);
  public fields = signal<FormFieldDto[]>([]);
  public isLoading = signal<boolean>(true);
  public isSubmitting = signal<boolean>(false);
  public isSubmitted = signal<boolean>(false);
  public errorMessage = signal<string>('');

  public submitterName = signal<string>('');
  public submitterEmail = signal<string>('');
  public fieldValues = signal<{ [fieldId: string]: string }>({});

  constructor(
    private route: ActivatedRoute,
    private client: Client
  ) {}

  public updateFieldValue(fieldId: string, value: string): void {
    this.fieldValues.update(current => ({ ...current, [fieldId]: value }));
  }

  ngOnInit(): void {
    const s = this.route.snapshot.paramMap.get('slug') || '';
    this.slug.set(s);
    if (s) {
      this.loadForm();
    }
  }

  public loadForm(): void {
    this.isLoading.set(true);
    this.client.formsGET2(this.slug()).subscribe({
      next: (f) => {
        this.form.set(f);
        const sortedFields = (f.fields || []).sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
        this.fields.set(sortedFields);
        
        // Initialize empty values
        const values: { [fieldId: string]: string } = {};
        for (const field of sortedFields) {
          values[field.id!] = '';
        }
        this.fieldValues.set(values);
        
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('This form could not be found or is no longer published.');
        this.isLoading.set(false);
      }
    });
  }

  public onSubmit(): void {
    this.errorMessage.set('');
    
    // Validate required fields
    const currentFields = this.fields();
    const currentValues = this.fieldValues();
    
    for (const field of currentFields) {
      if (field.isRequired && !currentValues[field.id!]) {
        this.errorMessage.set(`"${field.label}" is required.`);
        return;
      }
    }

    this.isSubmitting.set(true);

    const values = currentFields.map(f => new SubmissionValueDto({
      fieldId: f.id,
      value: currentValues[f.id!]
    }));

    const request = new SubmitFormRequest({
      formId: this.form()?.id,
      submitterEmail: this.submitterEmail() || undefined,
      submitterName: this.submitterName() || undefined,
      values: values
    });

    this.client.submit(this.slug(), request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isSubmitted.set(true);
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('Something went wrong. Please try again.');
      }
    });
  }

  public getInputType(fieldType: string | undefined): string {
    switch (fieldType?.toLowerCase()) {
      case 'email': return 'email';
      case 'number': return 'number';
      default: return 'text';
    }
  }

  public toggleCheckbox(fieldId: string, option: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const currentValues = { ...this.fieldValues() };
    const currentOptionString = currentValues[fieldId] ? currentValues[fieldId].split(',').filter(v => v) : [];
    
    if (checked) {
      currentOptionString.push(option);
    } else {
      const idx = currentOptionString.indexOf(option);
      if (idx > -1) currentOptionString.splice(idx, 1);
    }
    
    currentValues[fieldId] = currentOptionString.join(',');
    this.fieldValues.set(currentValues);
  }
}
