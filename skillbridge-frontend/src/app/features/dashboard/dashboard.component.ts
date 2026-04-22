import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
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
  private readonly auth = inject(AuthService);

  protected readonly title = 'SkillBridge';
  protected readonly jobs = signal<Job[]>([]);
  protected readonly employerJobs = signal<Job[]>([]);
  protected readonly health = signal<HealthStatus | null>(null);
  protected readonly loading = signal(false);
  protected readonly posting = signal(false);
  protected readonly authBusy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected readonly totalJobs = computed(() => this.jobs().length);
  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly currentUser = this.auth.currentUser;

  protected readonly authForm = {
    fullName: '',
    email: '',
    password: ''
  };

  protected readonly form = {
    title: '',
    description: '',
    type: 'internship',
    location: '',
    isRemote: false
  };

  ngOnInit(): void {
    this.loadHealth();
    this.loadJobs();
    const u = this.auth.currentUser();
    if (u?.userId) {
      this.loadEmployerJobs(u.userId);
    }
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

  protected registerEmployer(): void {
    this.message.set('');
    this.error.set('');
    const { fullName, email, password } = this.authForm;
    if (!fullName?.trim() || !email?.trim() || !password) {
      this.error.set('Full name, email and password are required.');
      return;
    }
    if (password.length < 8) {
      this.error.set('Password must be at least 8 characters.');
      return;
    }

    this.authBusy.set(true);
    this.auth.registerEmployer(fullName.trim(), email.trim(), password).subscribe({
      next: () => {
        this.authBusy.set(false);
        this.message.set('Employer account created. You can post jobs.');
        this.authForm.password = '';
        this.loadEmployerJobs(this.auth.currentUser()!.userId);
      },
      error: (err) => {
        this.authBusy.set(false);
        this.error.set(err?.error?.message ?? 'Registration failed.');
      }
    });
  }

  protected loginEmployer(): void {
    this.message.set('');
    this.error.set('');
    const { email, password } = this.authForm;
    if (!email?.trim() || !password) {
      this.error.set('Email and password are required.');
      return;
    }

    this.authBusy.set(true);
    this.auth.login(email.trim(), password).subscribe({
      next: () => {
        this.authBusy.set(false);
        this.message.set('Signed in.');
        this.authForm.password = '';
        this.loadEmployerJobs(this.auth.currentUser()!.userId);
      },
      error: () => {
        this.authBusy.set(false);
        this.error.set('Invalid email or password.');
      }
    });
  }

  protected logout(): void {
    this.auth.logout();
    this.employerJobs.set([]);
    this.message.set('Signed out.');
  }

  protected createJob(): void {
    this.message.set('');
    this.error.set('');

    if (!this.auth.isAuthenticated()) {
      this.error.set('Sign in as an employer to post a job.');
      return;
    }

    if (!this.form.title || !this.form.description) {
      this.error.set('Title and description are required.');
      return;
    }

    this.posting.set(true);
    this.api
      .createJob({
        title: this.form.title.trim(),
        description: this.form.description.trim(),
        type: this.form.type.trim(),
        location: this.form.location.trim(),
        isRemote: this.form.isRemote
      })
      .subscribe({
        next: (job) => {
          this.posting.set(false);
          this.message.set(`Job posted successfully. Employer ID: ${job.employerId}`);
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
