import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Job } from '../../core/models/job.model';
import { HealthStatus } from '../../core/models/health.model';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(ApiService);

  protected readonly title = 'SkillBridge';
  protected readonly jobs = signal<Job[]>([]);
  protected readonly employerJobs = signal<Job[]>([]);
  protected readonly health = signal<HealthStatus | null>(null);
  protected readonly loading = signal(false);
  protected readonly posting = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected readonly totalJobs = computed(() => this.jobs().length);

  protected readonly form = {
    employerName: '',
    employerEmail: '',
    title: '',
    description: '',
    type: 'internship',
    location: '',
    isRemote: false
  };

  ngOnInit(): void {
    this.loadHealth();
    this.loadJobs();
  }

  protected loadHealth(): void {
    this.api.getHealth().subscribe({
      next: (res) => this.health.set(res),
      error: () => this.health.set(null)
    });
  }

  protected loadJobs(): void {
    this.loading.set(true);
    this.error.set('');

    this.api.getJobs(true).subscribe({
      next: (jobs) => {
        this.jobs.set(jobs);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load jobs. Make sure backend is running.');
        this.loading.set(false);
      }
    });
  }

  protected createJob(): void {
    this.message.set('');
    this.error.set('');

    if (!this.form.employerName || !this.form.employerEmail || !this.form.title || !this.form.description) {
      this.error.set('Employer name, employer email, title and description are required.');
      return;
    }

    this.posting.set(true);
    this.api.createJob({
      employerName: this.form.employerName.trim(),
      employerEmail: this.form.employerEmail.trim(),
      title: this.form.title.trim(),
      description: this.form.description.trim(),
      type: this.form.type.trim(),
      location: this.form.location.trim(),
      isRemote: this.form.isRemote
    }).subscribe({
      next: (job) => {
        this.posting.set(false);
        this.message.set(`Job posted successfully. Backend generated Employer ID: ${job.employerId}`);
        this.loadJobs();
        this.loadEmployerJobs(job.employerId);
        this.form.title = '';
        this.form.description = '';
        this.form.location = '';
      },
      error: (err) => {
        this.posting.set(false);
        this.error.set(err?.error?.message ?? 'Failed to post job.');
      }
    });
  }

  protected loadEmployerJobs(employerId?: string): void {
    const id = employerId?.trim();
    if (!id) {
      this.employerJobs.set([]);
      return;
    }

    this.api.getEmployerJobs(id).subscribe({
      next: (jobs) => this.employerJobs.set(jobs),
      error: () => this.employerJobs.set([])
    });
  }
}
