import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { Job } from '../../core/models/job.model';

@Component({
  selector: 'app-cover-letter',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './cover-letter.component.html',
  styleUrl: './cover-letter.component.scss'
})
export class CoverLetterComponent implements OnInit {
  private readonly api = inject(ApiService);

  protected readonly loadingJobs = signal(false);
  protected readonly generating = signal(false);
  protected readonly copied = signal(false);
  protected readonly error = signal('');
  protected readonly jobs = signal<Job[]>([]);
  protected readonly generatedLetter = signal('');

  protected readonly selectedJob = computed(() => {
    const id = this.form.selectedJobId;
    return this.jobs().find((job) => job.id === id) ?? null;
  });

  protected readonly form = {
    selectedJobId: '',
    candidateName: '',
    candidateEmail: '',
    candidatePhone: '',
    strengths: '',
    achievements: '',
    motivation: ''
  };

  ngOnInit(): void {
    this.loadJobs();
  }

  protected loadJobs(): void {
    this.loadingJobs.set(true);
    this.error.set('');

    this.api.getJobs(true).subscribe({
      next: (jobs) => {
        this.jobs.set(jobs);
        if (jobs.length && !this.form.selectedJobId) {
          this.form.selectedJobId = jobs[0].id;
        }
        this.loadingJobs.set(false);
      },
      error: () => {
        this.error.set('Unable to load jobs from the API. Start backend and try again.');
        this.loadingJobs.set(false);
      }
    });
  }

  protected generateLetter(): void {
    this.error.set('');
    this.copied.set(false);

    const job = this.selectedJob();
    if (!job) {
      this.error.set('Please select a job first.');
      return;
    }

    if (!this.form.candidateName || !this.form.candidateEmail || !this.form.strengths || !this.form.motivation) {
      this.error.set('Name, email, strengths, and motivation are required.');
      return;
    }

    this.generating.set(true);

    const strengths = this.toSentence(this.form.strengths);
    const achievements = this.toSentence(this.form.achievements);
    const motivation = this.form.motivation.trim();
    const location = job.location?.trim() || 'your organization';

    const letter = [
      `Dear Hiring Team at ${job.employerName},`,
      '',
      `I am excited to apply for the ${job.title} (${job.type}) opportunity at ${job.employerName}. With my background in ${strengths}, I am confident I can add value to your team and contribute to impactful outcomes from day one.`,
      '',
      `What attracts me most to this role is the chance to support meaningful work in ${location}. ${motivation}`,
      '',
      `In my recent work and projects, I have demonstrated ${achievements}. These experiences have prepared me to handle the core responsibilities described in your posting, including building practical solutions, collaborating effectively, and continuously improving quality.`,
      '',
      `I would welcome the opportunity to discuss how my profile aligns with your needs. Thank you for your time and consideration.`,
      '',
      'Sincerely,',
      this.form.candidateName.trim(),
      `${this.form.candidateEmail.trim()} | ${this.form.candidatePhone.trim()}`
    ].join('\n');

    this.generatedLetter.set(letter);
    this.generating.set(false);
  }

  protected async copyLetter(): Promise<void> {
    if (!this.generatedLetter()) {
      return;
    }

    try {
      await navigator.clipboard.writeText(this.generatedLetter());
      this.copied.set(true);
    } catch {
      this.error.set('Could not copy automatically. Please copy from the preview area.');
    }
  }

  private toSentence(value: string): string {
    return value
      .split(',')
      .map((part) => part.trim())
      .filter(Boolean)
      .join(', ');
  }
}
