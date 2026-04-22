import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-candidate-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './candidate-register.component.html',
  styleUrl: './candidate-register.component.scss'
})
export class CandidateRegisterComponent {
  private readonly api = inject(ApiService);

  protected readonly step = signal(1);
  protected readonly submitting = signal(false);
  protected readonly success = signal('');
  protected readonly error = signal('');
  protected readonly progress = computed(() => `${Math.round((this.step() / 3) * 100)}%`);

  protected readonly form = {
    fullName: '',
    email: '',
    phone: '',
    location: '',
    education: '',
    careerGoal: '',
    bio: ''
  };

  protected nextStep(): void {
    this.error.set('');
    if (this.step() === 1 && (!this.form.fullName.trim() || !this.form.email.trim())) {
      this.error.set('Full name and email are required.');
      return;
    }
    if (this.step() < 3) this.step.update((v) => v + 1);
  }

  protected prevStep(): void {
    this.error.set('');
    if (this.step() > 1) this.step.update((v) => v - 1);
  }

  protected submit(): void {
    this.success.set('');
    this.error.set('');
    this.submitting.set(true);

    this.api.registerCandidate({
      fullName: this.form.fullName.trim(),
      email: this.form.email.trim(),
      phone: this.form.phone.trim() || undefined,
      location: this.form.location.trim() || undefined,
      education: this.form.education.trim() || undefined,
      careerGoal: this.form.careerGoal.trim() || undefined,
      bio: this.form.bio.trim() || undefined,
      role: 'youth'
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.success.set('Profile created successfully. You can now continue to the dashboard.');
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err?.error?.message ?? 'Registration failed. Please try again.');
      }
    });
  }
}
