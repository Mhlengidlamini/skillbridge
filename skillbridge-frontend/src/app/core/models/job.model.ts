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
  title: string;
  description: string;
  type: string;
  location?: string;
  isRemote: boolean;
}
