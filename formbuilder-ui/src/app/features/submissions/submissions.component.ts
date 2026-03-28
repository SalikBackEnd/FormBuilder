import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Client, FormDto, FormSubmissionDto } from '../../core/api/form-builder-api';

@Component({
  selector: 'app-submissions',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe],
  templateUrl: './submissions.component.html',
  styleUrls: ['./submissions.component.css']
})
export class SubmissionsComponent implements OnInit {
  public formId = signal<string>('');
  public form = signal<FormDto | null>(null);
  public submissions = signal<FormSubmissionDto[]>([]);
  public isLoading = signal<boolean>(true);
  public selectedSubmission = signal<FormSubmissionDto | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private client: Client
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') || '';
    this.formId.set(id);
    if (id) {
      this.loadData();
    }
  }

  public loadData(): void {
    this.isLoading.set(true);

    // Load form details for the header
    this.client.formsGET(this.formId()).subscribe({
      next: (f) => {
        this.form.set(f);
      }
    });

    // Load submissions
    this.client.submissionsAll(this.formId()).subscribe({
      next: (subs) => {
        this.submissions.set(subs);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  public selectSubmission(sub: FormSubmissionDto): void {
    const current = this.selectedSubmission();
    this.selectedSubmission.set(current?.id === sub.id ? null : sub);
  }

  public getFieldLabel(fieldId: string | undefined): string {
    const currentForm = this.form();
    if (!fieldId || !currentForm?.fields) return 'Unknown Field';
    const field = currentForm.fields.find(f => f.id === fieldId);
    return field?.label || 'Unknown Field';
  }

  public goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
