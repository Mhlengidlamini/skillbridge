import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateJobRequest, Job, JobMatchScoreRequest, JobMatchScoreResponse } from '../models/job.model';
import { HealthStatus } from '../models/health.model';
import { CandidateRegistrationRequest, UserProfile } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getHealth(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>(`${this.baseUrl}/health`);
  }

  getJobs(activeOnly = true): Observable<Job[]> {
    return this.http.get<Job[]>(`${this.baseUrl}/jobs`, {
      params: { activeOnly }
    });
  }

  getEmployerJobs(employerId: string): Observable<Job[]> {
    return this.http.get<Job[]>(`${this.baseUrl}/jobs/employer/${employerId}`);
  }

  createJob(payload: CreateJobRequest): Observable<Job> {
    return this.http.post<Job>(`${this.baseUrl}/jobs`, payload);
  }

  getJobMatchScore(jobId: string, payload: JobMatchScoreRequest): Observable<JobMatchScoreResponse> {
    return this.http.post<JobMatchScoreResponse>(`${this.baseUrl}/jobs/${jobId}/match-score`, payload);
  }

  registerCandidate(payload: CandidateRegistrationRequest): Observable<UserProfile> {
    return this.http.post<UserProfile>(`${this.baseUrl}/users`, payload);
  }
}
