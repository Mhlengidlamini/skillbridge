import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import html2canvas from 'html2canvas';
import jsPDF from 'jspdf';

type Experience = {
  role: string;
  company: string;
  highlights: string[];
};

type GeneratedCv = {
  fullName: string;
  email: string;
  phone: string;
  location: string;
  summary: string;
  skills: string[];
  experience: Experience[];
  education: string;
  targetRole: string;
};

@Component({
  selector: 'app-cv-builder',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './cv-builder.component.html',
  styleUrl: './cv-builder.component.scss'
})
export class CvBuilderComponent {
  @ViewChild('cvPreview') protected cvPreview?: ElementRef<HTMLElement>;

  protected readonly generating = signal(false);
  protected readonly exporting = signal(false);
  protected readonly error = signal('');
  protected readonly generatedCv = signal<GeneratedCv | null>(null);

  protected readonly form = {
    fullName: '',
    email: '',
    phone: '',
    location: '',
    targetRole: '',
    education: '',
    skills: '',
    experience: '',
    achievements: ''
  };

  protected buildCvWithAi(): void {
    this.error.set('');

    if (!this.form.fullName || !this.form.email || !this.form.targetRole || !this.form.skills) {
      this.error.set('Full name, email, target role, and skills are required.');
      return;
    }

    this.generating.set(true);
    const skillList = this.form.skills
      .split(',')
      .map((s) => s.trim())
      .filter(Boolean);

    const experienceLines = this.form.experience
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean);

    const achievementLines = this.form.achievements
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean);

    const cv: GeneratedCv = {
      fullName: this.form.fullName.trim(),
      email: this.form.email.trim(),
      phone: this.form.phone.trim(),
      location: this.form.location.trim(),
      targetRole: this.form.targetRole.trim(),
      education: this.form.education.trim(),
      skills: skillList,
      summary: this.createSummary(skillList),
      experience: this.createExperience(experienceLines, achievementLines),
      // Ensure generated CV always has at least one education line.
      ...(this.form.education.trim() ? {} : { education: 'Education details to be added.' })
    };

    this.generatedCv.set(cv);
    this.generating.set(false);
  }

  protected async exportToPdf(): Promise<void> {
    if (!this.cvPreview?.nativeElement) {
      this.error.set('CV preview is not ready yet. Generate your CV first.');
      return;
    }

    this.exporting.set(true);
    this.error.set('');

    try {
      const element = this.cvPreview.nativeElement;
      const canvas = await html2canvas(element, {
        scale: 2,
        backgroundColor: '#ffffff'
      });

      const imageData = canvas.toDataURL('image/png');
      const pdf = new jsPDF('p', 'mm', 'a4');
      const pageWidth = pdf.internal.pageSize.getWidth();
      const pageHeight = pdf.internal.pageSize.getHeight();
      const imageWidth = pageWidth;
      const imageHeight = (canvas.height * imageWidth) / canvas.width;

      let position = 0;
      let heightLeft = imageHeight;

      pdf.addImage(imageData, 'PNG', 0, position, imageWidth, imageHeight);
      heightLeft -= pageHeight;

      while (heightLeft > 0) {
        position = heightLeft - imageHeight;
        pdf.addPage();
        pdf.addImage(imageData, 'PNG', 0, position, imageWidth, imageHeight);
        heightLeft -= pageHeight;
      }

      const safeName = (this.generatedCv()?.fullName || 'cv').replace(/\s+/g, '-').toLowerCase();
      pdf.save(`${safeName}-skillbridge-cv.pdf`);
    } catch {
      this.error.set('Failed to export PDF. Please try again.');
    } finally {
      this.exporting.set(false);
    }
  }

  private createSummary(skills: string[]): string {
    const role = this.form.targetRole.trim();
    const primarySkills = skills.slice(0, 4).join(', ');

    return `Motivated candidate targeting ${role} roles with practical exposure to ${primarySkills}. Focused on delivering measurable outcomes, learning quickly, and collaborating effectively across teams.`;
  }

  private createExperience(experienceLines: string[], achievementLines: string[]): Experience[] {
    if (!experienceLines.length) {
      return [
        {
          role: `Aspiring ${this.form.targetRole.trim()}`,
          company: 'Project-Based Learning',
          highlights: [
            'Built practical portfolio projects aligned with career goals.',
            'Applied feedback loops to continuously improve quality.'
          ]
        }
      ];
    }

    return [
      {
        role: this.form.targetRole.trim(),
        company: 'Recent Experience',
        highlights: [...experienceLines.slice(0, 3), ...achievementLines.slice(0, 2)]
      }
    ];
  }
}
