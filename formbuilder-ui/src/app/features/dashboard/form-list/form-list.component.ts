import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Client, FormDto, CreateFormRequest } from '../../../core/api/form-builder-api';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-form-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './form-list.component.html',
  styleUrls: ['./form-list.component.css']
})
export class FormListComponent implements OnInit {
  public forms = signal<FormDto[]>([]);
  public isLoading = signal<boolean>(true);

  // Modal state
  public showCreateModal = signal<boolean>(false);
  public newFormTitle = signal<string>('');
  public newFormDescription = signal<string>('');
  public isCreating = signal<boolean>(false);

  constructor(
    private formClient: Client,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadForms();
  }

  public loadForms(): void {
    this.isLoading.set(true);
    this.formClient.formsAll().subscribe({
      next: (forms) => {
        this.forms.set(forms);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  public openCreateModal(): void {
    this.newFormTitle.set('');
    this.newFormDescription.set('');
    this.showCreateModal.set(true);
  }

  public closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  public createNewForm(): void {
    const title = this.newFormTitle().trim();
    if (!title) return;

    this.isCreating.set(true);
    this.formClient.formsPOST(new CreateFormRequest({
      title: title,
      description: this.newFormDescription().trim()
    })).subscribe({
      next: (form) => {
        this.isCreating.set(false);
        this.showCreateModal.set(false);
        this.router.navigate(['/dashboard/builder', form.id]);
      },
      error: () => {
        this.isCreating.set(false);
        this.toast.error('Failed to create form. Please try again.');
      }
    });
  }

  public editForm(id: string): void {
    this.router.navigate(['/dashboard/builder', id]);
  }

  public viewSubmissions(id: string): void {
    this.router.navigate(['/dashboard/submissions', id]);
  }

  public togglePublish(form: FormDto): void {
    if (form.isPublished) {
      this.formClient.unpublish(form.id!).subscribe(() => {
        form.isPublished = false;
      });
    } else {
      this.formClient.publish(form.id!).subscribe(() => {
        form.isPublished = true;
      });
    }
  }

  public copyLink(slug: string): void {
    const url = `${window.location.origin}/f/${slug}`;
    navigator.clipboard.writeText(url).then(() => {
      this.toast.success('Link copied to clipboard!');
    });
  }
}
