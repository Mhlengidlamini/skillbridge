import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
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
  protected readonly scoring = signal(false);
  protected readonly posting = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly matchSummary = signal('');
  protected readonly jobMatchScores = signal<Record<string, number>>({});

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

  protected readonly candidateProfile = {
    skills: '',
    targetRole: '',
    preferredLocation: ''
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
        this.jobMatchScores.set({});
        this.matchSummary.set('');
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

  protected generateMatchScores(): void {
    this.error.set('');
    this.matchSummary.set('');

    const skills = this.candidateProfile.skills.trim();
    if (!skills) {
      this.error.set('Add candidate skills first to calculate match scores.');
      return;
    }

    const currentJobs = this.jobs();
    if (!currentJobs.length) {
      this.error.set('No jobs available to score yet.');
      return;
    }

    this.scoring.set(true);

    const calls = currentJobs.map((job) =>
      this.api.getJobMatchScore(job.id, {
        candidateSkills: skills,
        targetRole: this.candidateProfile.targetRole.trim(),
        preferredLocation: this.candidateProfile.preferredLocation.trim()
      })
    );

    forkJoin(calls).subscribe({
      next: (responses) => {
        const map: Record<string, number> = {};
        responses.forEach((result) => {
          map[result.jobId] = result.score;
        });
        this.jobMatchScores.set(map);
        this.scoring.set(false);
        this.matchSummary.set('AI match badges updated for listed jobs.');
      },
      error: () => {
        this.scoring.set(false);
        this.error.set('Could not calculate job match scores right now.');
      }
    });
  }

  protected scoreFor(jobId: string): number | null {
    const score = this.jobMatchScores()[jobId];
    return score ?? null;
  }

  protected scoreBadgeClass(score: number | null): string {
    if (score === null) {
      return 'pending';
    }

    if (score >= 75) {
      return 'high';
    }

    if (score >= 50) {
      return 'medium';
    }

    return 'low';
  }
}
