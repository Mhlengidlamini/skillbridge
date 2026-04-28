import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { MentorConnection, UserProfile } from '../../core/models/user.model';

@Component({
  selector: 'app-mentor-profiles',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './mentor-profiles.component.html',
  styleUrl: './mentor-profiles.component.scss'
})
export class MentorProfilesComponent implements OnInit {
  private readonly api = inject(ApiService);

  protected readonly loading = signal(false);
  protected readonly submitting = signal(false);
  protected readonly connecting = signal(false);
  protected readonly success = signal('');
  protected readonly requestSuccess = signal('');
  protected readonly error = signal('');
  protected readonly mentors = signal<UserProfile[]>([]);
  protected readonly requests = signal<MentorConnection[]>([]);

  protected readonly filters = {
    keyword: '',
    location: ''
  };

  protected readonly form = {
    fullName: '',
    email: '',
    phone: '',
    location: '',
    expertise: '',
    industry: '',
    bio: ''
  };

  protected readonly connectionForm = {
    mentorId: '',
    menteeName: '',
    menteeEmail: '',
    menteeGoal: '',
    message: ''
  };

  protected readonly filteredMentors = computed(() => {
    const keyword = this.filters.keyword.trim().toLowerCase();
    const location = this.filters.location.trim().toLowerCase();

    return this.mentors().filter((mentor) => {
      const searchable = `${mentor.fullName} ${mentor.bio ?? ''} ${mentor.education ?? ''} ${mentor.careerGoal ?? ''}`.toLowerCase();
      const mentorLocation = (mentor.location ?? '').toLowerCase();

      const keywordMatch = !keyword || searchable.includes(keyword);
      const locationMatch = !location || mentorLocation.includes(location);
      return keywordMatch && locationMatch;
    });
  });

  ngOnInit(): void {
    this.loadMentors();
  }

  protected loadMentors(): void {
    this.loading.set(true);
    this.error.set('');

    this.api.getMentors().subscribe({
      next: (mentors) => {
        this.mentors.set(mentors);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load mentors right now.');
        this.loading.set(false);
      }
    });
  }

  protected openConnectionRequest(mentor: UserProfile): void {
    this.requestSuccess.set('');
    this.error.set('');
    this.connectionForm.mentorId = mentor.id;
  }

  protected cancelConnectionRequest(): void {
    this.connectionForm.mentorId = '';
    this.requestSuccess.set('');
  }

  protected submitMentorProfile(): void {
    this.success.set('');
    this.requestSuccess.set('');
    this.error.set('');

    if (!this.form.fullName.trim() || !this.form.email.trim() || !this.form.expertise.trim()) {
      this.error.set('Full name, email, and expertise are required.');
      return;
    }

    this.submitting.set(true);

    this.api
      .registerUser({
        fullName: this.form.fullName.trim(),
        email: this.form.email.trim(),
        phone: this.form.phone.trim() || undefined,
        location: this.form.location.trim() || undefined,
        education: this.form.expertise.trim() || undefined,
        careerGoal: this.form.industry.trim() || undefined,
        bio: this.form.bio.trim() || undefined,
        role: 'mentor'
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.success.set('Mentor profile created. It is now visible in browse results.');
          this.form.fullName = '';
          this.form.email = '';
          this.form.phone = '';
          this.form.location = '';
          this.form.expertise = '';
          this.form.industry = '';
          this.form.bio = '';
          this.loadMentors();
        },
        error: (err) => {
          this.submitting.set(false);
          this.error.set(err?.error?.message ?? 'Unable to create mentor profile.');
        }
      });
  }

  protected submitConnectionRequest(): void {
    this.requestSuccess.set('');
    this.error.set('');

    if (
      !this.connectionForm.mentorId ||
      !this.connectionForm.menteeName.trim() ||
      !this.connectionForm.menteeEmail.trim()
    ) {
      this.error.set('Select a mentor and provide your name and email.');
      return;
    }

    this.connecting.set(true);
    this.api
      .createMentorConnectionRequest({
        mentorId: this.connectionForm.mentorId,
        menteeName: this.connectionForm.menteeName.trim(),
        menteeEmail: this.connectionForm.menteeEmail.trim(),
        menteeGoal: this.connectionForm.menteeGoal.trim() || undefined,
        message: this.connectionForm.message.trim() || undefined
      })
      .subscribe({
        next: () => {
          this.connecting.set(false);
          this.requestSuccess.set('Connection request sent successfully.');
          this.loadMyRequests();
          this.connectionForm.mentorId = '';
          this.connectionForm.message = '';
        },
        error: (err) => {
          this.connecting.set(false);
          this.error.set(err?.error?.message ?? 'Unable to send request.');
        }
      });
  }

  protected loadMyRequests(): void {
    const email = this.connectionForm.menteeEmail.trim();
    if (!email) {
      this.error.set('Add your mentee email to load your requests.');
      return;
    }

    this.api.getMenteeConnectionRequests(email).subscribe({
      next: (requests) => this.requests.set(requests),
      error: () => this.error.set('Unable to load your connection requests.')
    });
  }

  protected isMentorPending(mentorId: string): boolean {
    return this.requests().some((x) => x.mentorId === mentorId && x.status === 'pending');
  }
}
