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

export interface UserRegistrationRequest {
  fullName: string;
  email: string;
  phone?: string;
  location?: string;
  bio?: string;
  education?: string;
  careerGoal?: string;
  role: 'youth' | 'mentor' | 'employer' | 'admin';
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

export interface MentorConnectionRequest {
  mentorId: string;
  menteeName: string;
  menteeEmail: string;
  menteeGoal?: string;
  message?: string;
}

export interface MentorConnection {
  id: string;
  mentorId: string;
  mentorName: string;
  menteeName: string;
  menteeEmail: string;
  menteeGoal?: string;
  message?: string;
  status: string;
  requestedAt: string;
}
