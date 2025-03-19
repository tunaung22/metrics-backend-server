# Database Table Design

sql database table design

## Database Table

### Department

| key | column         | type   | length | NULLABLE | UNIQUE | DESCRIPTION |
| --- | -------------- | ------ | ------ | -------- | ------ | ----------- |
| PK  | Id             | BIGINT |        |          |        |             |
|     | DepartmentName | STRING | 500    | NO       | YES    |             |

### KpiPeriod

| key | column     | type     | length | NULLABLE | UNIQUE | DESCRIPTION |
| --- | ---------- | -------- | ------ | -------- | ------ | ----------- |
| PK  | Id         | BIGINT   |        |          |        |             |
|     | PeriodName | STRING   | 20     | NO       | YES    |             |
|     | StartDate  | DATETIME |        | NO       | YES    |             |
|     | EndDate    | DATETIME |        | NO       | YES    |             |

### Employee

| key | column       | type   | length | NULLABLE | UNIQUE | DESCRIPTION |
| --- | ------------ | ------ | ------ | -------- | ------ | ----------- |
| PK  | Id           | BIGINT |        |          |        |             |
|     | EmployeeCode | STRING | 100    | NO       | YES    |             |
|     | EmployeeName | STRING | 200    | NO       | YES    |             |
|     | Address      | STRING | MAX    | YES      | NO     |             |
|     | PhoneNumber  | STRING | 500    | YES      | NO     |             |
| FK  | DepartmentId | BIGINT |        |          |        |             |
| FK  | AspnetUser   |        |        |          |        |             |

### KpiSubmission

| key | column                      | type     | length   | NULLABLE | UNIQUE | DESCRIPTION      |
| --- | --------------------------- | -------- | -------- | -------- | ------ | ---------------- |
| PK  | Id                          | BIGINT   |          |          |        |                  |
|     | SubmissionTime              | DATETIME |          | NO       | YES    | with time zone   |
|     | PerformanceScore            | DECIMAL  | p=4, s=2 | NO       | YES    |                  |
|     | Comments                    | STRING   | MAX      | YES      | NO     |                  |
| FK  | KpiPeriodId (KpiPeriod)     | BIGINT   |          |          |        |                  |
| FK  | SubmissionFor(DepartmentId) | BIGINT   |          |          |        |                  |
|     |                             |          |          |          |        |                  |
| FK  | SubmittedBy (Employee)      | BIGINT   |          |          |        |                  |
|     | SubmissionDate              | DATE     |          |          |        | generated column |
