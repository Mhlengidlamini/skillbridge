import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateJobRequest, Job } from '../models/job.model';
import { HealthStatus } from '../models/health.model';

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
}
