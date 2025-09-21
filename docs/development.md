# Development Docs

## Features

1. Managing Users
2. Managing Key Metrics (key metrics and department key metrics)
3. Giving Feedback (case feedback)
4. Submit Feedback Scores (case feedback score)
5. Submit Base Score (kpi score)
6. Submit Metric Score (key kpi score)

## Authorization

| User with Role | manage users | give feedback | submit feedback score | submit department kpi score | submit key kpi score |
| -------------- | ------------ | ------------- | --------------------- | --------------------------- | -------------------- |
| ADMIN          | YES          | NO            | NO                    | NO                          | NO                   |
| STAFF          | NO           | YES           | NO                    | YES                         | NO                   |
| HOD            | NO           | NO            | NO                    | YES                         | YES                  |
| MANAGEMENT     | NO           | NO            | YES                   | YES                         | YES                  |

### Policies

1. A Policy can have one or more requirements
   - A Policy is fufilled, if all of requirements are satisfied.
2. A Requirement can have one or more handlers
   - A Requirement is fufilled, if any of handlers are statisfied.

- CanAccessAdminFeaturesPolicy
- CanGiveFeedbackPolicy
- CanSubmitKpiScorePolicy
- CanSubmitKeyKpiScorePolicy
- CanSubmitFeedbackScorePolicy

#### CanAccessAdminFeaturesPolicy

- AccessAdminFeaturesRequirement
  - AccessAdminFeaturesHandler

#### CanGiveFeedbackPolicy

- GiveFeedbackRequirement
  - GiveFeedbackHandler

#### CanSubmitKpiScorePolicy

- SubmitKpiScoreRequirement
  - SubmitKpiScoreHandler

#### CanSubmitKeyKpiScorePolicy

- SubmitKeyKpiScoreRequirement
  - SubmitKeyKpiScoreHandler

#### CanSubmitFeedbackScorePolicy

- SubmitFeedbackScoreRequirement
  - SubmitFeedbackScoreHandler
