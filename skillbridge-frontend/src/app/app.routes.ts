import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/candidate-register/candidate-register.component').then(m => m.CandidateRegisterComponent)
  },
  {
    path: 'cv-builder',
    loadComponent: () => import('./features/cv-builder/cv-builder.component').then(m => m.CvBuilderComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
