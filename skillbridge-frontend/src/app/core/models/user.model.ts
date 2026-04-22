export interface CandidateRegistrationRequest {
  fullName: string;
  email: string;
  phone?: string;
  location?: string;
  bio?: string;
  education?: string;
  careerGoal?: string;
  role: 'youth';
}

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phone?: string;
  location?: string;
  bio?: string;
  education?: string;
  careerGoal?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}
