# Development Documentation

## Features

1. Basic
   - User Mangement
     - User Roles and Groups
     - Lock/Unlock
     - Authorization
   - Submission Period
   - Departments
2. Department Key KPI
   - Key Metrics
   - Department Keys
   - Key KPI Submission Rules
3. Department Case Feedback Submission
4. Department KPI Score Submission (base kpi)
5. Department Key KPI Score Submission (key kpi)
6. Department Case Feedback Score Submission (case feedback)
7. Reports

## Database

```mermaid
erDiagram


    department {
        long id
        string department_code
        string department_name
    }
    key_metric {
      long id
      guid metric_code
      string metric_title
      string description
      bool is_deleted
      timestampz created_at
      timestampz modified_at
    }

    key_metric ||--o{ department_key_metric : contains
    kpi_submission_period ||--o{ department_key_metric : contains
    department ||--o{ department_key_metric : contains

    department_key_metric {
        long id
        guid department_key_metric_code
        long kpi_submission_period_id
        long department_id
        long key_metric_id
    }
    kpi_submission_period {
        long id
        string period_name
        timestampz submission_start_date
        timestampz submission_end_date
        bool is_deleted
        timestampz created_at
        timestampz modified_at
    }

    kpi_submission {
        long id
    }

    case_feedback_score_submission {
        long id
    }

    department ||--o{key_kpi_submission_constraint : contains
    department_key_metric||--o{key_kpi_submission_constraint : contains

    key_kpi_submission_constraint {
        long id
        guid lookup_id
        long department_id
        long department_key_metric_id
        bool is_deleted
        timestampz created_at
        timestampz modified_at
    }

    department_key_metric ||--o{key_kpi_submission : contains
    application_user||--o{key_kpi_submission : contains

    key_kpi_submission {
        long id
        timestampz submitted_at
        long department_key_metric_id
        string submitter_id
        decimal score_value
        string comments
        bool is_deleted
        timestampz created_at
        timestampz modified_at
    }

    user_title {
        long id
        guid title_code
        string title_name
    }

    user_title||--o{application_user: contains

    application_user {
        string id
        string username
        string email
        long user_title_id
    }
```

## Initial Setup

ပထမဦးဆုံး စနစ်ကိုစတင်အသုံးပြုလျှင် sysadmin အကောင့်တစ်ခုထားရှိရမည်ဖြစ်သည်။ ၎င်းအကောင့်မှတဆင့် စနစ်အသုံးပြုသူ အကောင့်များကို ဆက်လက်တည်ဆောက်ရမည်။

## User Management

အသုံးပြုသူ/user များကို အသစ်ထည့်သွင်းခြင်း၊ ဖျက်သိမ်းခြင်း၊ ခေတ္တရပ်တန့်ခြင်းများ လုပ်ဆောင်နိုင်သည်။

### User Roles and Groups

#### User Role

User roles ၂ မျိုးရှိသည့်အနက် Admin role သည်စနစ်အသုံးပြုသူနှင့် မသက်ဆိုင်ပါ။ စနစ်အသုံးပြုသူသည် Staff role ဖြစ်ရမည်။ ဥပမာ sysadmin အကောင့်သည် Admin role ဖြစ်သည်။

- Admin
- Staff

#### User Group or User Title

User group ဖြင့်အသုံးပြုသူ အမျိုးအစားခွဲခြားခြင်းထားနိုင်သည်။

စနစ်အသုံးပြုသူတိုင်းသည် Staff role များဖြစ်ကြသော် user group ဖြင့်ထပ်ဆင့်အမျိုးအစားခွဲခြားနိုင်သည်။

User group ကိုအသုံးပြုပြိး authorization များကန့်သတ်ထားသည်။

User Groups:

- Staff
- HOD
- Management

---

### Authorization

| User with Role | manage users | give feedback | submit feedback score | submit department kpi score | submit key kpi score |
| -------------- | ------------ | ------------- | --------------------- | --------------------------- | -------------------- |
| ADMIN          | YES          | NO            | NO                    | NO                          | NO                   |
| STAFF          | NO           | YES           | NO                    | YES                         | NO                   |
| HOD            | NO           | NO            | NO                    | YES                         | YES                  |
| MANAGEMENT     | NO           | NO            | YES                   | YES                         | YES                  |

#### Policies

1. A Policy can have one or more requirements
   - A Policy is fufilled, if all of requirements are satisfied.
2. A Requirement can have one or more handlers
   - A Requirement is fufilled, if any of handlers are statisfied.

- CanAccessAdminFeaturesPolicy
- CanGiveFeedbackPolicy
- CanSubmitKpiScorePolicy
- CanSubmitKeyKpiScorePolicy
- CanSubmitFeedbackScorePolicy
- CanGiveFeedbackPolicy

#### Requirements

- AccessAdminFeaturesRequirement
- GiveFeedbackRequirement
- SubmitKpiScoreRequirement
- SubmitKeyKpiScoreRequirement
- SubmitFeedbackScoreRequirement

#### Requirement Handlers

- AccessAdminFeaturesHandler
- GiveFeedbackHandler
- SubmitKpiScoreHandler
- SubmitKeyKpiScoreHandler
- SubmitFeedbackScoreHandler

## Submission Period

- Period name
- Start Date
- End Date

## Departments

- Department name

---

## Department Key KPI

### Key Metrics

```mermaid
erDiagram
   key_metrics {
      long id
      guid metric_code
      string metric_title
      string description
      bool is_deleted
      timestampz created_at
      timestampz modified_at
   }
```

### Department Keys

## Key KPI Submission Rules

## Department Case Feedback Submission

## Department KPI Score Submission (base kpi)

## Department Key KPI Score Submission (key kpi)

### Flowchart

```mermaid
flowchart TD
    A([Start]) -- param: Period Name --> B(sdfsf)
    B --> C{check period}
    C -- YES --> D
    C -- NO --> E


    %% A[Period Name] --> B[Fetch Period]
    %% A[Christmas] -->|Get money| B(Go shopping)
    %% B --> C{Let me think}
    %% C -->|One| D[Laptop]
    %% C -->|Two| E[iPhone]
    %% C -->|Three| F[fa:fa-car Car]
```

## Department Case Feedback Score Submission (case feedback)

### Department

a department
