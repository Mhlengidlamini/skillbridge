# SkillBridge Improvement Tickets

This backlog is organized as practical tickets you can implement next.

## P0 - Critical Foundation

- `SB-001` JWT Authentication + role claims (`youth`, `employer`, `admin`).
- `SB-002` Protect employer job posting using token identity (remove identity fields from request).
- `SB-003` Add backend validation layer (FluentValidation or equivalent) for all DTOs.
- `SB-004` Add centralized API error response format with correlation ID.
- `SB-005` Add EF Core migrations workflow and initial seed data script.

## P1 - Core Product Features

- `SB-006` Candidate registration + profile setup wizard.
- `SB-007` Job search filters (type, location, remote, date posted).
- `SB-008` Job details page + one-click application flow.
- `SB-009` Saved jobs feature with personal shortlist.
- `SB-010` Employer dashboard analytics (views, applications, shortlist count).

## P2 - AI Features

- `SB-011` AI CV Builder page with export to PDF.
- `SB-012` AI Cover Letter generator per selected job.
- `SB-013` AI Job Match Score API + UI badge integration.
- `SB-014` Interview coach simulation with structured feedback report.
- `SB-015` Skill-gap detector with learning links and estimated timeline.

## P3 - Mentors + Notifications

- `SB-016` Mentor profile onboarding and browse page.
- `SB-017` Mentor-mentee connection request flow.
- `SB-018` Notifications center (job updates, mentor requests, system alerts).
- `SB-019` Email integration for critical events (application status, invites).

## P4 - Quality + Delivery

- `SB-020` Unit tests for services and endpoint tests for API.
- `SB-021` Frontend e2e smoke tests for landing + dashboard flows.
- `SB-022` CI pipeline (build, test, lint, artifact publish).
- `SB-023` Deployment scripts for frontend and backend environments.
