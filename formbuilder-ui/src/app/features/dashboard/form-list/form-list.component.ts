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
      error: (err) => {
        this.isLoading.set(false);
        this.toast.apiError(err);
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
      error: (err) => {
        this.isCreating.set(false);
        this.toast.apiError(err);
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
      this.formClient.unpublish(form.id!).subscribe({
        next: () => {
          this.forms.update(list =>
            list.map(f => f.id === form.id ? { ...f, isPublished: false } as FormDto : f)
          );
          this.toast.success('Form unpublished.');
        },
        error: (err) => this.toast.apiError(err)
      });
    } else {
      this.formClient.publish(form.id!).subscribe({
        next: () => {
          this.forms.update(list =>
            list.map(f => f.id === form.id ? { ...f, isPublished: true } as FormDto : f)
          );
          this.toast.success('Form published!');
        },
        error: (err) => this.toast.apiError(err)
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
