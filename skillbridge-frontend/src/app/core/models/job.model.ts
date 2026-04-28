export interface Job {
  id: string;
  employerId: string;
  employerName: string;
  employerEmail: string;
  title: string;
  description: string;
  type: string;
  location?: string | null;
  isRemote: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface CreateJobRequest {
  employerName: string;
  employerEmail: string;
  title: string;
  description: string;
  type: string;
  location?: string;
  isRemote: boolean;
}

export interface JobMatchScoreRequest {
  candidateSkills: string;
  targetRole?: string;
  preferredLocation?: string;
}

export interface JobMatchScoreResponse {
  jobId: string;
  score: number;
  summary: string;
}
