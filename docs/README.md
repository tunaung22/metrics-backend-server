# Documentation

Project Documentation

## What?

### Definition

#### Department

Each employee must be in a department.

---

#### User Account

An account must be created to interact with the system

#### Employee/Staff

Each employee owns an employee/staff account

#### Roles

User account have roles to control who can interact with certain part of the system.
Common roles are:

1. Staff
2. HOD
3. Management

---

#### KPI Score

A KPI score, or Key Performance Indicator score, is a measurable value that demonstrates how effectively an organization is achieving key business objectives.

#### KPI Score Submission

KPI Score are submitted by the following category:

1. Submissions by Staff
2. Submissions by HOD
3. Submissions by Management

#### Case Feedback & HR Record

#### Key KPI Score

---

### Database Migrations

```bash
  # Create new migration
  dotnet ef migrations add "InitDb"
  # Remove previous migration
  dotnet ef migrations remove
  # Run migration
  dotnet ef database update
  # Move migration to [MigrationName]
  dotnet ef database update [MigrationName]
  # Undo migration to initial
  dotnet ef database update 0
```

### Packages Used

- Entity Framework Core  
  [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore)

- EFCore Tools (CLI)  
  [Microsoft.EntityFrameworkCore.Tools](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Tools)

- EFCore Tools (Design)  
  [Microsoft.EntityFrameworkCore.Design](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design)

- .NET Identiy Entity Framework Core  
  [Microsoft.AspNetCore.Identity.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.AspNetCore.Identity.EntityFrameworkCore)

- PostgreSQL Database Provider  
  [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL)

- EF Core Naming Conventions  
  [EFCore.NamingConventions](https://www.nuget.org/packages/EFCore.NamingConventions)

## URL Routes

| Admin Dashboard | Description     |
| --------------- | --------------- |
| `/dashboard`    | admin dashboard |

| Manage User Accounts                          | Description        |
| --------------------------------------------- | ------------------ |
| `/manage/users`                               | staff list         |
| `/manage/users/register`                      | register new staff |
| `/manage/users/{account-uuid}/password/reset` | reset a password   |
| `/manage/users/roles`                         | role list          |
| `/manage/users/roles/create`                  | create new role    |

| Manage Departments           | Description           |
| ---------------------------- | --------------------- |
| `/manage/departments`        | department list       |
| `/manage/departments/create` | create new department |
| `/manage/departments/edit`   | edit department       |

| Manage submission periods            | Description           |
| ------------------------------------ | --------------------- |
| `/manage/submissions/periods`        | kpi period list       |
| `/manage/submissions/periods/create` | create new kpi period |
| `/manage/submissions/periods/edit`   | edit kpi period       |

| Manage score metrics                           | Description                         |
| ---------------------------------------------- | ----------------------------------- |
| `/manage/key-metrics`                          | list score metrics                  |
| `/manage/key-metrics/{department-code}/view`   | view score metrics for department   |
| `/manage/key-metrics/{department-code}/create` | create score metrics for department |
| `/manage/key-metrics/{department-code}/edit`   | edit score metrics for department   |

| KPI Submission                                              | Description                         |
| ----------------------------------------------------------- | ----------------------------------- |
| `/submissions/department-scores`                            | list avaiable submissions           |
| `/submissions/department-scores/succcess`                   | score submission success            |
| `/submissions/department-scores/{period-name}/apply`        | apply for general department scores |
| `/submissions/department-metric-scores`                     | list avaiable submissions           |
| `/submissions/department-metric-scores/{period-name}/apply` | apply for department metrics scores |

| Reports                                                            | Description |
| ------------------------------------------------------------------ | ----------- |
| `/reports/submissions/department-scores`                           |             |
| `/reports/submissions/department-scores/view/{period-name}`        |             |
| `/reports/submissions/department-metric-scores`                    |             |
| `/reports/submissions/department-metric-scores/view/{period-name}` |             |

| Route                     | Description                |
| ------------------------- | -------------------------- |
| `/account/me`             | view profile               |
| `/account/password/reset` | request password reset     |
| `/account/login`          | login                      |
| `/account/logout`         | logout                     |
| `/account/success`        | register new staff success |

| Route    | Description |
| -------- | ----------- |
| `/about` | about page  |

### Razor Pages

#### AUTHENTICATED (ADMIN ROLE)

| Url                             | Description                          |
| ------------------------------- | ------------------------------------ |
| /departments                    | manage departments                   |
| /kpi                            | manage kpis                          |
| /kpi/periods                    | manage kpi periods                   |
| /kpi/submissions                | manage kpi submissions               |
| /kpi/submissions?period=2025-01 | manage kpi submissions by kpi period |
|                                 |                                      |

#### AUTHENTICATED (EMPLOYEE ROLE)

| Url                      | Description       |
| ------------------------ | ----------------- |
| /account/profile         | manage self       |
| /services/kpi/submission | Submit a kpi form |
|                          |                   |

#### PUBLIC (NON AUTHENTICATED)

| Url      | Description  |
| -------- | ------------ |
| /about   | info page    |
| /contact | contact page |
|          |              |

## Infrastructure

![Infrastructure](./images/infrastructure.png)

### Clearn Architecture

Reference from the Microsoft's Common web application architectures article, section [Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)

![onion view of clean architecture layers](./images/clean_archi_layers_onion_view.png)
![clean architecture layers](./images/clean_archi_layers.png)

### Project Structure

- Metrics.Application (application core)
  - Domains/Entities
  - Interfaces
  - DTOs
  - Exceptions
- Metrics.Infrastructure
  - DbContext
  - FluentAPI Entity Configurations
  - Data Seedings
  - Repositories
  - Services
- Metrics.Shared
  - Filters
  - Utils
- Metrics.Web
  - Main ASP.NET app

## ERD Diagrams

![ERD](./images/erd_v0.1.png)
![ERD](./images/erd_v0.2.png)
![ERD](./images/erd_v0.3.png)

## Client UIs

- Web browser
- Mobile applicaiton

### UI Mockup

![ui mock](./images/ui_mockup__submission_form.png)
![ui mock v1](./images/ui__submission_form_v1.png)
![ui mock v1.1](./images/ui__submission_form_v1.1.png)
