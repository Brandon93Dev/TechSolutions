# TechSolutions IPS – Design & Build Decisions

> This document captures the main design and build choices made during development.
> It's a living reference for the project and for assessment purposes.


---
## So ... How to get solution running... See below
## A. Prerequisites (After Cloning the Repository)

### A.1 SQL Server Express (MSSQL 17 recommended)

1. Download and install **SQL Server Express**.
2. During installation, select **Mixed Mode (SQL Server and Windows Authentication)**.
3. Set a strong password for the `sa` account and remember it — you will need this later.
**Tip:** Keep your `sa` password and SMTP settings handy — you'll need them when configuring `appsettings.development.json`.

### A.2 SQL Server Management Studio (SSMS 22 recommended)

1. Install **SQL Server Management Studio**.
2. Open **SSMS** (preferably as Administrator).
3. Connect to your SQL Express instance using one of the following server names:
   - `.\SQLEXPRESS`
   - `(local)\SQLEXPRESS`
4. In the root folder of the cloned repository, locate the file `techsln.bak`.
5. Restore the database:
   - Right-click on **Databases** → **Restore Database**
   - Under **Source**, select **Device** → Add `techsln.bak`
   - Click **OK** to restore.

### A.3 Visual Studio Community 2026 (Insiders)

1. Install **Visual Studio Community 2026 Insiders**.
2. During installation (or by modifying an existing installation), add the **ASP.NET and web development** workload.
3. Navigate to the folder `TechSolutions_IPS_HW` inside the cloned repository.
4. Double-click `TechSolutions_IPS_HW.sln` to open the solution in Visual Studio.
5. Build the solution by pressing **Ctrl + Shift + B** (or right-click the solution → **Build Solution**).
6. Locate and open **`appsettings.Development.json`**.
7. Find the line that starts with `"DefaultConnection"`.
8. **Update your credentials** as follows:
   8.1   - Change `Password=YourStrongPasswordHere` to the **sa password** you created earlier.
         - The final connection string should look similar to this:
   ```json
   "DefaultConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=tecsln_main;Persist Security Info=True;User ID=sa;Password=YourStrongPasswordHere;Trust Server Certificate=True"
OR 
   8.2 If you prefer to use windows authentication
         - Replace `User ID=sa;Password=YourStrongPasswordHere;` with `Integrated Security=SSPI;`
      
**How to update it:
Replace YourStrongPasswordHere with the sa password you set during SQL Server Express installation. (or use Method 8.2)
Do not change any other part of the string unless you used a different instance name.**

### A.4 SMTP Mail Server Credentials
For **development**, you can use a local SMTP testing tool such as:
- **Papercut**
- **MailHog**
- **MailDev**
1. If you have a SMPT server available and ready to use update the following fields with your settings
   - "Host": ""
   - "Port": ""    
   - "User": ""
   - "Pass": ""
   - "FromEmail": ""
3. SAVE THE FILE!

## B. Starting the Application
Press F5 on your keyboard or click the start button <img width="77" height="36" alt="image" src="https://github.com/user-attachments/assets/cd0c133e-8c7b-4ebe-8235-5f6b74a7c939" />
You should initially get a few prompts to accepot certificate, just accept and continue until a web browser opens

## C. Using the Application
After starting the project up you should shortly see this page:
<img width="818" height="772" alt="image" src="https://github.com/user-attachments/assets/7b3976fc-cbd4-4446-ba7b-a9dc63d2c810" />

### C.1 Logging in
<img width="1471" height="829" alt="image" src="https://github.com/user-attachments/assets/17f5ef07-3b89-4036-941f-435c3df8086d" />
1. Click on the login button as shown above
   You will be redirected to the following page:   
<img width="567" height="675" alt="image" src="https://github.com/user-attachments/assets/a0d0a0b6-66ff-485b-ae12-fced8dab03fa" />

   - Enter either of the following credential sets:

      ```json
        ADMIN
        Email address : tsadmin@synsoft.co.za
        Password : F6aKhxM_u5[n?m3C

        Manager
        Email Address : tsmanagement@synsoft.co.za
        Password : #pl3sWk$X80l1YP-
      
3. Click the <img height="41" alt="image" src="https://github.com/user-attachments/assets/07ba96b2-8b7f-4faf-895b-c9b228bb09d1" /> button
   
   You will now be redirected to the Admin Dashboard
   <img width="1406" height="796" alt="image" src="https://github.com/user-attachments/assets/f609e9bb-3991-4271-bc70-8c86f08d985d" />
   dont worry toomuch if your numbers are different, you just havent registered any employees yet  

From here on out i will leave you to explore, in my opinion its the best way to learn a system.
---



## 1. Project Overview

| Property | Value |
|---|---|
| **Project Name** | TechSolutions |
| **Type** | ASP.NET Core Razor Pages + MVC Controllers --- AND external API login functionality |
| **IDE** | Visual Studio Community 2026 (Insiders) |
| **Database** | SQL Server (via SQL Express, `SQLEXPRESS` instance) |
| **Authentication** | ASP.NET Core Identity + JWT Bearer (for API clients) |
| **Source Control** | Git – branch `BB/Initial_Commit_DB_additions` |

📐 **Architecture Diagrams:** See [`Architecture/architecture.md`](Architecture/architecture.md) for Mermaid diagrams covering system layers, process flows, entity relationships, and DI registration.

---

## 2. Architecture Decisions

### 2.1 Razor Pages + MVC Hybrid + API Client functionality

**Choice:** Use Razor Pages as the primary UI pattern, with MVC controllers for API endpoints and admin actions.

**Reason:** Razor Pages keeps page logic self-contained (`.cshtml` + `.cshtml.cs`), which works well for most pages. 
MVC controllers are used where REST-style routing makes more sense (API endpoints, admin user management).

### 2.2 Service Layer Additions and organisation (Folder-Per-Concern)

**Choice:** Services are organised into sub-folders by concern rather than a flat folder.
Repositories follow the same convention with their own folder and interfaces sub-folder:

```
Repositories/
├── Interfaces/
│   ├── IAdminNotificationRepository
│   ├── IAuditRepository
│   ├── ICountryDialCodeRepository
│   ├── ICustomerRepository
│   └── IEmailQueueRepository
├── AdminNotificationRepository.cs
├── AuditRepository.cs
├── CountryDialCodeRepository.cs
├── CustomerRepository.cs
└── EmailQueueRepository.cs

Services/
├── AdminServices/
│   └── AdminDashboardService
├── AuditServices/
│   └── AuditService
├── AuthenticationServices/
│   ├── AppSignInManagerService
│   └── RsaCryptoService
├── CustomerServices/
│   ├── CustomerService
│   ├── CustomerAccessService
│   └── CustomerReferenceDataService
├── MailerServices/
│   ├── EmailQueue
│   ├── EmailDispatcherService
│   ├── SmtpEmailSender
│   └── EmailTemplates
├── NotificationServices/
│   ├── UserRegistrationNotifier
│   └── AdminNotificationService
├── Interfaces/
│   ├── IAdminDashboardService
│   ├── IAdminNotificationService
│   ├── IAuditService
│   ├── ICustomerAccessService
│   ├── ICustomerReferenceDataService
│   ├── ICustomerService
│   ├── IEmailSender
│   └── IUserRegistrationNotifier
├── DataSeeder.cs
└── DependencyInjectionService.cs
```

**Reason:** As the codebase grew, enforcing clear boundaries improved maintainability and testing.
Controllers call services only, services call repositories only, and repositories own EF Core data access.

### 2.5 Layered Flow (Controller → Service → Repository → Database)

**Choice:** Enforced strict layering across customer, dashboard, and notification features.

```
Controller  →  Service  →  Repository  →  ApplicationDbContext  →  Database
```

**Reason:**
- **Separation of concerns** – HTTP, business logic, and persistence are separated
- **Dependency inversion** – high-level layers depend on interfaces
- **Testability** – controllers/services can be unit-tested with mocks
- **Consistency** – all data access remains in repositories

**Examples of recent refactors:**
- `CustomerController` now uses `ICustomerService`, `ICustomerAccessService`, and `ICustomerReferenceDataService`
- `AdminDashboardController` now uses `IAdminDashboardService`
- `AdminUsersController` and `NotificationsController` now use `IAdminNotificationService`
- Services use repositories (no direct controller-to-repository calls for business features)

All repositories/services are registered in `DependencyInjectionService.cs`.

---

## 3. Data Model Decisions

### 3.1 Custom `ApplicationUser` (extends `IdentityUser`)

**Choice:** Added `DisplayName`, `IsApproved`, `ApprovedBy`, and `ApprovedAt` properties.

**Reason:** 
- `DisplayName` – Users selects their username when they register. The navbar greeting shows this instead of their email. They can change it on their profile page.
- `IsApproved` / `ApprovedBy` / `ApprovedAt` – New registrations need to be approved by an Administrator or Manager before the user can sign in. 
These fields track approval status and keep a record of who approved whom and when.

### 3.2 Role-Based Access Control (Three Roles)

**Choice:** Three roles seeded on startup:
- **Administrator** – Full system access, can approve/deny users
- **Management** – Can approve/deny users
- **Employee** – Default role for approved regular users

**Reason:** The system deals with sensitive data, so only authorised approvers should be able to grant access. 
The "Employee" role gives all non-admin approved users a default role, which makes it easier to gate features by role later on.

### 3.3 `Customer` Entity

**Choice:** Separate `Customer` table with unique email index, status (`Draft` / `Active`), soft-delete, and structured address fields:
- `AddressLine1`, `AddressLine2`, `City`, `ProvinceState`, `AddressCountry`, `AreaCode`.

**Reason:** Customers are domain data (not identity users). Structured address fields improve reporting/analytics (country charts, filtering) and data quality compared to a free-form address blob.

### 3.7 `CountryDialCode` Reference Entity

**Choice:** Added `CountryDialCodes` table to store country names and dial codes.

**Reason:** Country metadata is now centrally managed in the database instead of hardcoded frontend lists, enabling consistent server-driven dropdown population and phone prefix assistance.

---

## 4. Authentication & Authorisation Decisions

### 4.1 Custom `AppSignInManagerService`

**Choice:** Overrode `SignInManager<ApplicationUser>.PasswordSignInAsync()` to check `IsApproved` before allowing sign-in.

**Reason:** ASP.NET Core Identity doesn't have a built-in idea of "admin-approved accounts." By hooking into the sign-in flow at the `SignInManager` level, 
unapproved users get blocked with `SignInResult.NotAllowed` no matter where sign-in happens (Login page, API, etc.). One place to enforce it.

### 4.2 JWT Bearer Authentication (API Clients)

**Choice:** Added JWT Bearer authentication alongside cookie authentication, configured conditionally from `Jwt:Key/Issuer/Audience` settings.

**Reason:** External API clients (mobile apps, integrations) need stateless token-based auth. 
JWT only gets configured if the keys exist in configuration, so it won't cause errors in environments where API access isn't needed.

### 4.3 RSA Crypto Service

**Choice:** `RsaCryptoService` registered as a singleton, reading RSA keys from configuration.

**Reason:** The API token endpoint accepts encrypted login requests (`EncryptedLoginRequest`). 
RSA decryption means credentials aren't sent in plaintext even over HTTPS (defence in depth) — an extra layer of security for the system.

### 4.4 Policy-Based Authorisation (`CanApproveUsers`)

**Choice:** A named policy `CanApproveUsers` requires the user to be in one of the configured `ApproverRoles` (defaults: Administrator, Management).

**Reason:** Using a policy instead of hard-coded `[Authorize(Roles = "")]` means the allowed roles can be changed in config. 

### 4.5 Brute-Force Protection (Account Lockout)

**Choice:** Identity lockout configured to **3 failed attempts**, after which the account is locked for **24 hours**. `lockoutOnFailure: true` is set on the sign-in call.

**Reason:** The login page used to have `lockoutOnFailure: false`, which meant unlimited password guessing. Now it:
- Counts failed attempts at the Identity level
- Locks the account after 3 failures with a 24-hour cooldown
- Shows a message on the login page telling the user to contact `support@techsystems.co.za`
- Checks lockout status before attempting sign-in so it doesn't accidentally reset the timer

### 4.6 Username Enumeration Prevention

**Choice:** When a user registers with an email that already exists, the app silently redirects to the Login page instead of showing "this email is already taken."

**Reason:** Showing whether an account exists lets attackers build a list of valid usernames. By silently redirecting on `DuplicateUserName` / `DuplicateEmail` errors, the registration form doesn't reveal if the email is already taken.

### 4.7 Role-Based Navigation Visibility

**Choice:** Navigation links are conditionally rendered based on authentication state. The **Customers** link only appears when the user is signed in.

**Reason:** Users who aren't signed in shouldn't see links to features they can't use. The layout injects `SignInManager` and wraps protected links in `@if (SignInManager.IsSignedIn(User))` checks.

---

## 5. Email System Decisions

### 5.1 SMTP via `System.Net.Mail`

**Choice:** Used the built-in `System.Net.Mail.SmtpClient`.

**Reason:** The email needs are pretty simple (HTML body, single recipient), so `SmtpClient` does the job without adding another dependency. 
SMTP settings (host, port, credentials, SSL) come from the `Smtp:*` config section.

### 5.2 Database Queue + Background Dispatcher

**Choice:** Emails are enqueued to the `EmailQueueEntries` database table. 
A `BackgroundService` (`EmailDispatcherService`) polls every 5 MINUTES, processes up to 20 emails per cycle, and updates their status.

**Reason:**
- **Decouples** email sending from the request — the user doesn't sit waiting for SMTP
- **Persists** unsent emails across app restarts
- **Retries** with exponential back-off when SMTP fails temporarily
- **Logs** delivery status for troubleshooting

### 5.3 Email Templates

**Choice:** Static `EmailTemplates` class with methods returning HTML strings.

**Reason:** Simple and good enough for the current number of templates (just approval request emails). If the count grows a lot, this could be swapped for a Razor-based template engine, but for now it avoids over-engineering.

---

## 6. Notification System Decisions

### 6.1 Dual Notification (Email + In-App)

**Choice:** When a new user registers, all admins/managers receive both an email and an in-app notification.

**Reason:** Email covers approvers who aren't logged in at the time. In-app notifications catch them when they are. Between the two, no registration slips through the cracks.

### 6.2 Notification Bell with Bootstrap Modal

**Choice:** A notification bell icon with unread count badge in the navbar, opening a Bootstrap modal with approve/deny buttons.

**Reason:** Approvers can handle pending registrations from any page without navigating away. The modal uses AJAX (`/admin/employees/approve-ajax`, `/admin/employees/deny-ajax`) so it's instant — no full page reload. The bell only shows for users in the Administrator or Management roles.

### 6.3 `UserRegistrationNotifier` Service (Extracted from DbContext)

**Choice:** Notification logic was extracted from `ApplicationDbContext.SaveChangesAsync()` into a dedicated `UserRegistrationNotifier` service behind `IUserRegistrationNotifier`.

**Reason:** The DbContext should only worry about data access, not sending emails and creating notifications. Pulling the logic out means:
- **Separation of concerns** – DbContext stays focused on persistence
- **Testability** – the notifier can be mocked on its own
- **Readability** – `SaveChangesAsync` is now simple: spot new users → call notifier

The DbContext resolves the notifier via `IServiceScopeFactory` (new scope) to avoid circular DI issues.

---

## 7. Data Seeding Decisions

### 7.1 Seed on Startup (`DataSeeder`)

**Choice:** `DataSeeder.SeedInitialUsersAsync()` runs every time the app starts, creating roles and default admin/manager accounts if they don't exist.

**Reason:** Makes development easier — a fresh database is ready to use straight away without running any SQL manually. Credentials come from config (`SeedAdmin:*`, `SeedManager:*`) with safe defaults. The seeder checks if accounts already exist before creating them, so it's safe to run repeatedly.

### 7.2 Configuration-Driven Roles

**Choice:** Approver roles are read from `ApproverRoles` array in `appsettings.json`, with `["Administrator", "Management"]` as the default.

**Reason:** Roles can be changed per environment without touching code. The same config is used by the `CanApproveUsers` policy, the data seeder, and the notification service.

---

## 8. Middleware Pipeline Order

```
HTTPS Redirection
    → Routing
        → CORS ("DefaultCorsPolicy")
            → Authentication
                → Authorisation
                    → Static Assets
                        → Controllers (API + MVC)
                            → Razor Pages
```

**Key choices:**
- **CORS before Auth** – preflight requests need to be handled before auth rejects them
- **Authentication before Authorisation** – cookies/JWT need to be validated before policies kick in
- **Controllers mapped before Razor Pages** – API routes take priority; Razor Pages handle the UI

---

## 9. Database Migrations

| Migration | Tables / Columns Changed | Date |
|---|---|---|
| `Add_Customer_and_Audit_Log` | `Customers`, `AuditLogs` | 2026-03-21 |
| `Add_Audit_Entries` | `AuditEntries` | 2026-03-21 |
| `Add_AdminNotifications` | `AdminNotifications` | 2026-03-21 |
| `Add_EmailQueueEntries` | `EmailQueueEntries` | 2026-03-21 |
| `Add_Customer_Status` | Added `Status` column to `Customers` | 2026-03-21 |
| `Add_User_FirstName_LastName` | Added `FirstName`, `LastName` to `AspNetUsers` | 2026-03-21 |
| `Replace_Names_With_DisplayName` | Replaced `FirstName`/`LastName` with `DisplayName` on `AspNetUsers` | 2026-03-21 |
| `Add_Customer_Extended_Fields` | Added `Nationality`, `DateOfBirth`, `Address`, `IdNumber`, `Gender`, `DataSource`, `Notes` to `Customers` | 2026-03-22 |
| `Restructure_Customer_SHA256_CompositeId` | Customer composite ID restructure | 2026-03-22 |
| `Customer_IdNumber_As_PrimaryKey` | Moved `IdNumber` to primary key | 2026-03-22 |
| `Customer_Guid_PK_Optional_IdNumber` | Reverted to `Guid` PK, `IdNumber` optional | 2026-03-22 |
| `Widen_IdNumber_To_20` | Widened `IdNumber` from `nvarchar(13)` to `nvarchar(20)` for international ID support | 2026-03-22 |

---

## 10. NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.5 | Identity with EF Core stores |
| `Microsoft.AspNetCore.Identity.UI` | 10.0.5 | Scaffolded Identity UI pages |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.5 | SQL Server database provider |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.5 | EF migrations CLI tooling |
| `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` | 10.0.5 | Developer exception page for DB errors |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 | JWT token authentication for API |

**Choice:** No third-party packages beyond the Microsoft stack.

**Reason:** Keeps the dependency footprint small and updates in line with .NET releases. Everything we need right now is covered by the built-in libraries.

---

## 11. Configuration Structure

```
appsettings.json                  → Base config (connection string, logging)
appsettings.Development.json      → Dev overrides (SMTP creds, seed users, JWT keys, RSA keys)
```

| Section | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Smtp:*` | SMTP host, port, credentials, sender |
| `SeedAdmin:*` | Default admin email/password for seeding |
| `SeedManager:*` | Default manager email/password for seeding |
| `ApproverRoles` | Array of roles that can approve users |
| `App:BaseUrl` | Base URL for email links |
| `Jwt:Key/Issuer/Audience` | JWT signing configuration |
| `RSA:PrivateKeyPem/PublicKeyPem` | RSA keys for encrypted API login |
| `Cors:AllowedOrigins` | Allowed CORS origins (array) |

---

## 12. Project Folder Structure

```
TechSolutions_IPS_HW/
├── Architecture/
│   └── architecture.md
├── Areas/Identity/Pages/Account/
│   ├── Login, Register
│   └── Manage/...
├── Controllers/
│   ├── Api/
│   ├── AdminDashboardController.cs
│   ├── AdminEmployeesController.cs
│   ├── EmployeeDashboardController.cs
│   ├── CustomerController.cs
│   └── HomeController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DesignTimeDbContextFactory.cs
├── Migrations/
├── Models/
│   ├── Audit/
│   ├── Customer/
│   ├── Enums/
│   ├── Reference/                 → CountryDialCode
│   ├── User/
│   └── ViewModels/                → AdminDashboardViewModel, EmployeeDashboardViewModel
├── Repositories/
│   ├── Interfaces/
│   ├── AdminNotificationRepository.cs
│   ├── AuditRepository.cs
│   ├── CountryDialCodeRepository.cs
│   ├── CustomerRepository.cs
│   └── EmailQueueRepository.cs
├── Services/
│   ├── AdminServices/             → AdminDashboardService, EmployeeDashboardService
│   ├── AuditServices/
│   ├── AuthenticationServices/
│   ├── CustomerServices/
│   ├── Interfaces/                → includes IEmployeeDashboardService
│   ├── MailerServices/
│   ├── NotificationServices/
│   ├── DataSeeder.cs
│   └── DependencyInjectionService.cs
├── Views/
│   ├── AdminDashboard/
│   ├── AdminEmployees/
│   ├── EmployeeDashboard/
│   ├── Customer/
│   ├── Home/
│   └── Shared/
├── wwwroot/
│   ├── css/site.css
│   └── js/
│       ├── admin/                 → dashboard.js, employees.js
│       ├── customer/              → index.js, edit.js, details.js
│       ├── employee/              → dashboard.js
│       ├── home/                  → employee-metrics.js
│       └── shared/                → notifications.js
├── Program.cs
└── TechSolutions_IPS_HW.csproj
```

---

## Dev Notes

**Dashboard Split + Role Routing**
- Separated admin and employee experiences into dedicated dashboards
- `AdminDashboard` now focuses on employee management metrics and overview
- `EmployeeDashboard` now focuses on customer metrics and latest customer activity
- Role-based routing updated so employees land on `/employee/dashboard` and approvers on `/admin/dashboard`

**Admin Users Rename to Employees**
- Renamed management area from **Users** to **Employees**:
  - `AdminUsersController` → `AdminEmployeesController`
  - `/admin/users/*` → `/admin/employees/*`
  - `Views/AdminUsers` → `Views/AdminEmployees`
  - `js/admin/users.js` → `js/admin/employees.js`
- Updated notification approval links and AJAX endpoints to new route

**Customer Access + Filtering Improvements**
- Customer create/edit/update/delete restricted to role `Employee`
- Managers/Admins can view customer records but cannot create/edit/delete
- Added `Created By` column for privileged users
- Added advanced filtering for privileged users:
  - Created By
  - Country
  - Created From / Created To
- Advanced filters now use searchable dropdown controls and are populated from existing customer/audit data

**UI/UX and Navigation Updates**
- Improved employee dashboard visual density and card polish
- Added static privacy page content suitable for demo usage
- Corrected navbar active-tab highlighting and visibility behavior for role-gated pages

---

**Architecture / Layering Enforcement**
- Enforced Controller → Service → Repository flow for core business features
- Added service abstractions and implementations:
  - `ICustomerService`, `ICustomerAccessService`, `ICustomerReferenceDataService`
  - `IAdminDashboardService`, `IAdminNotificationService`
- Added repository abstractions/implementations for new responsibilities:
  - `ICountryDialCodeRepository` / `CountryDialCodeRepository`
  - expanded `ICustomerRepository` and `IAuditRepository` for metric and ownership queries

**Customer Domain Enhancements**
- Replaced legacy single address field with structured address columns
- Added `CountryDialCodes` table + migration-backed seed data
- Country dropdown + dial-code behavior now sourced from DB data
- Email duplication checks added client-side (debounced) + server-side enforcement
- Phone field client-side sanitization/validation added

**Access Control Hardening**
- Employees can only view/edit/delete customers they created
- Admin/Management retain full customer visibility

**Dashboard & Home Metrics**
- Expanded dashboard metrics/cards and country chart
- Added "last customer created by current user" card
- Employee-scoped dashboard data (limited view)
- Employee metrics now rendered on Home page via `home/employee-metrics.js`

**Identity / Account UX**
- Refreshed Manage/Account layout and navigation styling
- Disabled 2FA manage/login routes with redirect messaging

---

**Customer Page Redesign**
- Redesigned Customer Index with metric cards (Total / Active / Drafts), real-time search bar, and modern avatar table
- Customer names shown with colour-coded initial avatars and clickable links to details
- DataSource displayed as subtitle in the Customer column
- Status shown as dot-style pills (Active = green, Draft = amber)
- Action buttons (View / Edit / Delete) appear on row hover
- All filtering and search happens client-side for instant results

**Font Awesome 6 Migration**
- Replaced Bootstrap Icons CDN with Font Awesome 6.7.2 Free (cdnjs with SRI)
- Swapped all `bi bi-*` icon classes to `fa-solid fa-*` / `fa-regular fa-*` across all views and JS files
- Replaced notification bell emoji (`🔔`) with `fa-solid fa-bell` icon

**International ID Support**
- Widened `IdNumber` from 13 to 20 characters for international ID / passport numbers
- Relabelled field to "ID / Passport Number" across Edit form and Details view
- Migration: `Widen_IdNumber_To_20`

**JavaScript File Organisation**
- Moved JS files from flat `wwwroot/js/` into feature subfolders:
  - `js/admin/` — `dashboard.js`, `users.js`
  - `js/customer/` — `index.js`, `edit.js`, `details.js`
  - `js/shared/` — `notifications.js`
- Updated all script references in views

**Database Migrations**
- `Add_Customer_Extended_Fields` — Nationality, DoB, Address, IdNumber, Gender, DataSource, Notes
- `Restructure_Customer_SHA256_CompositeId` → `Customer_IdNumber_As_PrimaryKey` → `Customer_Guid_PK_Optional_IdNumber` — PK iteration (settled on Guid PK)
- `Widen_IdNumber_To_20` — IdNumber column `nvarchar(13)` → `nvarchar(20)`

---

**Customer Management**
- Full CRUD for customers (Index, Create, Edit, Details, Delete)
- Draft / Active status workflow with dual-submit buttons
- Status filter tabs (All / Drafts / Active) on the customer list
- Audit logging on all customer operations
- `ICustomerRepository` / `CustomerRepository` added to the repository layer

**User Profiles & Display Names**
- Users choose a **username** (DisplayName) at registration
- Navbar greeting shows username instead of email ("Hello Brandon!" not "Hello brandon@example.com!")
- Profile management page (`/Account/Manage`) to update username
- Seeded admin/manager accounts get default display names

**Security — Brute-Force Protection**
- Account lockout after 3 failed login attempts (24-hour cooldown)
- Locked-out users see a message to contact `support@techsystems.co.za`
- `lockoutOnFailure: true` enabled on sign-in
- Registration no longer reveals whether an email is already registered (username enumeration prevention)

**Security — Role-Based Navigation**
- Customers nav link only visible when signed in
- Layout injects `SignInManager` for conditional rendering

**UI & Theming**
- Dark gradient site-wide navbar matching login/register aesthetic
- Home page redesigned with hero section (icon badge, blue accents, styled divider)
- "POPIA Compliant · Secure by Design" added to site-wide footer

**Timezone Localisation**
- `DateTimeExtensions.ToLocalDisplay()` helper converts UTC to South Africa Standard Time
- All date displays in views use the helper
- Helper namespace added to both `_ViewImports.cshtml` files

**Database Migrations**
- `Add_Customer_Status` — Added `Status` column to `Customers`
- `Add_User_FirstName_LastName` — Added name fields to `AspNetUsers`
- `Replace_Names_With_DisplayName` — Consolidated to single `DisplayName` column

---

**Architecture**
- Moved all DI registrations to `DependencyInjectionService.cs`
- Introduced repository pattern (Controller → Service → Repository → DB)
- Extracted notification logic from `DbContext` into `UserRegistrationNotifier`

**Data Model**
- `ApplicationUser` with approval workflow (`IsApproved`, `ApprovedBy`, `ApprovedAt`)
- `Customer`, `AuditLog`, `AuditEntry`, `AdminNotification`, `EmailQueueEntry` entities
- Database-backed email queue replacing in-memory channel

**Authentication & Authorisation**
- Custom `AppSignInManagerService` blocking unapproved users
- JWT Bearer authentication for API clients
- RSA-encrypted login endpoint
- `CanApproveUsers` policy from configuration

**Notifications**
- Dual notification (email + in-app) on new user registration (so admin/manager can approve or deny new user registrations/profiles)
- Notification bell with Bootstrap modal for approve/deny

---

- Initial structure build of TechSolutions codebase
- ASP.NET Core Identity with SQL Server
- Login and Register Razor Page additions and customisation
- Basic project structure

---

## 13. Architecture Updates

### 13.1 ApplicationUser Enhancements

The `ApplicationUser` entity includes the following custom fields:
- `DisplayName`: User-selected username displayed in the navbar.
- `IsApproved`: Indicates if the user is approved.
- `ApprovedBy`: Tracks the approver.
- `ApprovedAt`: Timestamp of approval.

These fields are critical for the user approval workflow.

### 13.2 Role-Based Navigation Visibility

Navigation links are dynamically rendered based on the user's authentication state and role:
- **Notification Bell**: Visible to Administrators and Managers.
- **Customers Link**: Visible to Employees.
- **Protected Links**: Hidden for unauthenticated users.

### 13.3 Notification Bell Modal Flow

The notification bell uses AJAX for instant updates without a full page reload. Approvers can approve/deny user registrations directly from the modal.

### 13.4 CountryDialCode Usage

The `CountryDialCode` table dynamically populates dropdowns for consistent data across the application.

### 13.5 DataSeeder Process

The `DataSeeder` ensures roles and default users are created on startup. It checks for existing roles and users before inserting defaults.

### 13.6 JWT/RSA API Login Flow

The API login flow uses RSA encryption for credentials and issues JWT tokens for stateless authentication. This ensures secure communication between external clients and the API.

### 13.7 Audit Logging on Customer Operations

All customer operations (Create/Edit/Delete) are logged for auditing purposes. The `AuditService` handles logging, and entries are stored in the `AuditEntries` table.

---

## 14. Additional Notes

### 14.1 Alignment with Architecture Diagrams

The architecture diagrams in `architecture.md` have been updated to reflect the latest project implementation, including:
- Enhanced `ApplicationUser` fields.
- Role-based navigation visibility.
- Notification bell modal flow.
- CountryDialCode usage.
- DataSeeder process.
- JWT/RSA API login flow.
- Audit logging on customer operations.

### 14.2 Future Enhancements

- Consider adding more detailed diagrams for advanced workflows.
- Explore integrating additional security measures for API endpoints.
- Evaluate the need for a Razor-based email template engine as the number of templates grows.

---
