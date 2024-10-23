# Luminary Visual Database Schema

## 1. Users Table
Holds information about all users (editors, clients, admin).

| Column         | Type         | Description                                         |
|----------------|--------------|-----------------------------------------------------|
| `UserId`       | INT (PK)     | Unique ID for each user                             |
| `GoogleId`     | VARCHAR(255) | Username for login                                  |
| `Username`     | VARCHAR(50)  | Encrypted password                                  |
| `Email`        | VARCHAR(50)  | User's email                                        |
| `RoleId`       | INT (FK)     | References `Roles.RoleId` (can be updated by Admin)  |
- **Default Role**: Set `RoleId` to default to the 'Client' role, which means that any new user starts as a client.

## 2. Roles Table
Defines the roles: Editor, Client, and Admin.
Default is Client, unless changed by Admin (to be an editor).
Can add Important Client role if needed.

| Column   | Type         | Description                          |
|----------|--------------|--------------------------------------|
| `RoleId` | INT (PK)     | Unique ID for each role              |
| `RoleName` | VARCHAR(50) | Role name (e.g., 'Admin', 'Editor', 'Client') |

## 3. Projects Table
Holds information about video editing projects.

| Column            | Type         | Description                          |
|-------------------|--------------|--------------------------------------|
| `ProjectId`       | INT (PK)     | Unique ID for each project           |
| `ProjectName`     | VARCHAR(255) | Name of the project                  |
| `Description`     | TEXT         | Description of the project           |
| `ShootDate`       | DATE         | Date of the shoot                    |
| `DueDate`         | DATE         | Due date for the project             |
| `ProgressBar`     | INT          | Percentage of progress (0-100)       |
| `WorkingMonth`    | DATE         | The month of active work             |
| `Delivered`       | BOOLEAN      | Indicates if the project is delivered|

## 4. VideoEditors Table
Associates editors and assistant editors with a project.

| Column          | Type         | Description                          |
|-----------------|--------------|--------------------------------------|
| `VideoEditorId` | INT (PK)     | Unique ID for each entry             |
| `ProjectId`     | INT (FK)     | References `Projects.ProjectId`      |
| `UserId`        | INT (FK)     | References `Users.UserId` (editor)   |
| `Role`          | VARCHAR(50)  | 'Editor' or 'Assistant Editor'       |

## 5. VideoStatus Table
Tracks the status of videos and their editors/clients.

| Column           | Type         | Description                          |
|------------------|--------------|--------------------------------------|
| `StatusId`       | INT (PK)     | Unique ID for each status entry      |
| `ProjectId`      | INT (FK)     | References `Projects.ProjectId`      |
| `UserId`         | INT (FK)     | References `Users.UserId`            |
| `StatusType`     | VARCHAR(50)  | Type of status ('Editor', 'Client')  |

## 6. Payments Table (In Progress)
Handles editor payments and client payment status.
(To be modified according to the needs (for example adding billable by seconds or minutes or different prices))

| Column             | Type          | Description                          |
|--------------------|---------------|--------------------------------------|
| `PaymentId`        | INT (PK)      | Unique ID for each payment           |
| `UserId`           | INT (FK)      | References `Users.UserId` (editor)   |
| `ProjectId`        | INT (FK)      | References `Projects.ProjectId`      |
| `WorkingHours`     | DECIMAL(5,2)  | Total working hours                  |
| `Overtime`         | DECIMAL(5,2)  | Overtime hours worked                |
| `Undertime`        | DECIMAL(5,2)  | Undertime hours                      |
| `EditorPayment`    | DECIMAL(10,2) | Amount paid to the editor            |
| `EditorDatePaid`   | DATE          | Date when payment was made           |
| `BillableHours`    | DECIMAL(5,2)  | Hours billable to the client         |
| `Price`            | DECIMAL(10,2) | Price for the billable hours         |
| `ClientVisible`    | BOOLEAN       | Is this payment visible to the client|
| `ClientPaymentStatus` | VARCHAR(50) | 'Pending', 'Completed', 'Failed'     |

## 7. Archive Table
Holds archived projects and can only be accessed by the admin.

| Column            | Type         | Description                          |
|-------------------|--------------|--------------------------------------|
| `ArchiveId`       | INT (PK)     | Unique ID for each archive           |
| `ProjectId`       | INT (FK)     | References `Projects.ProjectId`      |
| `ArchiveDate`     | DATE         | Date the project was archived        |
| `Reason`          | TEXT         | Reason for archiving the project     |

## 8. Chat Table
Facilitates communication between clients and the company. Clients can send any message, and editors can reply, but all editor messages require admin approval.

| Column         | Type         | Description                                           |
|----------------|--------------|-------------------------------------------------------|
| `ChatId`       | INT (PK)     | Unique ID for each chat message                       |
| `UserId`       | INT (FK)     | References `Users.UserId` (sender of the message)   |
| `ProjectId`    | INT (FK)     | References `Projects.ProjectId` (related project)    |
| `Message`      | TEXT         | Content of the message                                |
| `Timestamp`    | DATETIME     | Date and time the message was sent                   |
| `IsEditorMessage` | BOOLEAN   | Indicates if the message was sent by an editor       |
| `IsApproved`    | BOOLEAN     | Indicates if the message has been approved by admin   |

## Relationships and Access Permissions
- **Editors and Assistant Editors**: Linked to `Projects` through the `VideoEditors` table, where they can update fields like `ProgressBar`, `WorkingMonth`, and `Delivered`.
- **Clients**: Can view `ProgressBar`, `Delivered` status, and certain payment details if `ClientVisible` is true.
- **Admin**: Can view and access all tables, including the `Archive` and `Chat`.

## Summary of Permissions

| Field                  | Editor Access  | Client Access      | Admin Access        |
|------------------------|----------------|--------------------|---------------------|
| `Status`               | Full Access    | Full Access        | Full Access         |
| `Video Editor`         | View Only      | No Access          | Full Access         |
| `Assistant Editor`     | View Only      | No Access          | Full Access         |
| `ShootDate`            | View Only      | Full Access        | Full Access         |
| `ClientName`           | View Only      | View Only          | Full Access         |
| `ProjectName`          | View Only      | Full Access        | Full Access         |
| `ProjectDescription`   | View Only      | Full Access        | Full Access         |
| `DueDate`              | Full Access    | No Access          | Full Access         |
| `ProgressBar`          | Full Access    | View Only          | Full Access         |
| `WorkingMonth`         | View Only      | No Access          | Full Access         |
| `BillableHours`        | Full Access    | No Access          | Full Access         |
| `Delivered`            | Full Access    | View Only          | Full Access         |
| `WorkingHours`         | View Only      | No Access          | Full Access         |
| `Overtime`             | View Only      | No Access          | Full Access         |
| `EditorPayment`        | View Only      | No Access          | Full Access         |
| `EditorDatePaid`       | View Only      | No Access          | Full Access         |
| `Price`                | No Access      | Conditional Access | Full Access         |
| `ClientPaymentVisible` | No Access      | No Access          | Full Access         |
| `ClientPaymentStatus`  | Full Access    | View Only          | Full Access         |
| `Archive`              | No Access      | No Access          | Full Access         |
| `Chat`                 | Reply with Admin Approval | Send Messages | View All Messages |
