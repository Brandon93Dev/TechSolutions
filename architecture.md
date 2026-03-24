# TechSolutions IPS — Architecture Diagrams

> Mermaid diagrams describing the system architecture, data flow, and key processes.
> View these on GitHub or in any Markdown renderer with Mermaid support (VS Code with Mermaid extension, etc.).

---

## 1. System Architecture (Layered Overview)

```mermaid
graph TB
    subgraph Clients
        Browser["🌐 Browser<br/>(Razor Views + jQuery AJAX)"]
        API["📱 External API Client<br/>(JWT Bearer)"]
    end

    subgraph Middleware Pipeline
        HTTPS["HTTPS Redirection"]
        Routing["Routing"]
        CORS["CORS Policy"]
        AuthN["Authentication<br/>(Cookie + JWT)"]
        AuthZ["Authorisation<br/>(Roles + Policies)"]
    end

    subgraph Controllers
        Home["HomeController"]
        Cust["CustomerController"]
        AdminDash["AdminDashboardController"]
        EmployeeDash["EmployeeDashboardController"]
        AdminEmployees["AdminEmployeesController"]
        TokenAPI["Api/TokenController"]
        NotifAPI["Api/NotificationsController"]
    end

    subgraph Services
        CustSvc["CustomerService"]
        CustAccessSvc["CustomerAccessService"]
        CustRefSvc["CustomerReferenceDataService"]
        DashSvc["AdminDashboardService"]
        EmployeeDashSvc["EmployeeDashboardService"]
        AdminNotifSvc["AdminNotificationService"]
        AuditSvc["AuditService"]
        SignInMgr["AppSignInManagerService"]
        RSA["RsaCryptoService"]
        EmailQ["EmailQueue"]
        EmailDisp["EmailDispatcherService<br/>(BackgroundService)"]
        SmtpSend["SmtpEmailSender"]
        Notifier["UserRegistrationNotifier"]
    end

    subgraph Repositories
        CustRepo["CustomerRepository"]
        AuditRepo["AuditRepository"]
        NotifRepo["AdminNotificationRepository"]
        EmailRepo["EmailQueueRepository"]
        CountryRepo["CountryDialCodeRepository"]
    end

    subgraph Data
        DbCtx["ApplicationDbContext<br/>(EF Core)"]
        Identity["ASP.NET Core Identity"]
    end

    DB[("SQL Server<br/>(SQLEXPRESS)")]

    Browser --> HTTPS --> Routing --> CORS --> AuthN --> AuthZ
    API --> HTTPS

    AuthZ --> Home
    AuthZ --> Cust
    AuthZ --> AdminDash
    AuthZ --> EmployeeDash
    AuthZ --> AdminEmployees
    AuthZ --> TokenAPI
    AuthZ --> NotifAPI

    Cust --> CustSvc
    Cust --> CustAccessSvc
    Cust --> CustRefSvc
    Cust --> AuditSvc

    AdminDash --> DashSvc
    EmployeeDash --> EmployeeDashSvc
    AdminEmployees --> AdminNotifSvc
    AdminEmployees --> AuditSvc
    NotifAPI --> AdminNotifSvc
    TokenAPI --> SignInMgr
    TokenAPI --> RSA

    DashSvc --> CustRepo
    DashSvc --> Identity
    EmployeeDashSvc --> CustRepo
    EmployeeDashSvc --> AuditRepo
    CustSvc --> CustRepo
    CustSvc --> AuditRepo
    CustAccessSvc --> AuditRepo
    CustRefSvc --> CountryRepo
    AdminNotifSvc --> NotifRepo
    AdminNotifSvc --> Identity

    AuditSvc --> AuditRepo
    Notifier --> NotifRepo
    Notifier --> EmailQ
    EmailQ --> EmailRepo
    EmailDisp --> EmailRepo
    EmailDisp --> SmtpSend

    CustRepo --> DbCtx
    AuditRepo --> DbCtx
    NotifRepo --> DbCtx
    EmailRepo --> DbCtx
    CountryRepo --> DbCtx
    Identity --> DbCtx

    DbCtx --> DB
    SmtpSend --> SMTP["📧 SMTP Server"]
```

---

## 2. User Registration & Approval Flow

```mermaid
sequenceDiagram
    actor User
    participant Register as Register Page
    participant Identity as ASP.NET Identity
    participant DbCtx as ApplicationDbContext
    participant Notifier as UserRegistrationNotifier
    participant EmailQ as EmailQueue
    participant DB as SQL Server
    participant Dispatch as EmailDispatcherService
    participant SMTP as SMTP Server
    actor Admin

    User->>Register: Submit registration form
    Register->>Identity: CreateAsync(user)
    Identity->>DbCtx: SaveChangesAsync()

    Note over DbCtx: Detects new ApplicationUser<br/>(IsApproved = false)

    DbCtx->>Notifier: NotifyNewRegistrationAsync(user)

    par In-App Notification
        Notifier->>DB: Insert AdminNotification<br/>(one per approver)
    and Email Notification
        Notifier->>EmailQ: EnqueueAsync(approvalEmail)
        EmailQ->>DB: Insert EmailQueueEntry<br/>(Status = Pending)
    end

    DbCtx-->>Register: Save complete
    Register-->>User: "Registration pending approval"

    loop Every 5 minutes
        Dispatch->>DB: Fetch pending emails
        Dispatch->>SMTP: Send email
        Dispatch->>DB: Update status → Sent
    end

    Note over Admin: Sees 🔔 notification badge<br/>or receives email

    Admin->>DB: Approve user (IsApproved = true)
    Admin->>DB: Mark notification as read
```

---

## 3. User Login Flow (with Lockout Protection)

```mermaid
flowchart TD
    A["User submits<br/>email + password"] --> B{Account<br/>locked out?}
    B -- Yes --> C["❌ Show lockout message<br/>'Contact support'"]
    B -- No --> D{Password<br/>correct?}
    D -- No --> E["Increment failed<br/>attempt counter"]
    E --> F{Failed attempts<br/>≥ 3?}
    F -- Yes --> G["🔒 Lock account<br/>for 24 hours"]
    F -- No --> H["❌ Invalid credentials"]
    D -- Yes --> I{IsApproved<br/>= true?}
    I -- No --> J["❌ SignInResult.NotAllowed<br/>'Account pending approval'"]
    I -- Yes --> K["✅ Sign in successful<br/>Issue cookie / JWT"]

    style C fill:#f8d7da,stroke:#dc3545
    style G fill:#f8d7da,stroke:#dc3545
    style H fill:#f8d7da,stroke:#dc3545
    style J fill:#fff3cd,stroke:#ffc107
    style K fill:#d1e7dd,stroke:#198754
```

---

## 4. Customer CRUD Workflow

```mermaid
stateDiagram-v2
    [*] --> Draft: Employee creates<br/>new customer

    Draft --> Draft: Edit & save as Draft
    Draft --> Active: Save & Activate<br/>(ID Number required)
    Active --> Active: Edit & save
    Active --> Draft: Save as Draft<br/>(demote)
    Draft --> Deleted: Delete
    Active --> Deleted: Delete
    Deleted --> [*]

    note right of Draft
        Partial records allowed.
        ID / Passport Number optional.
    end note

    note right of Active
        Fully verified record.
        ID / Passport Number required.
    end note
```

```mermaid
sequenceDiagram
    actor Employee
    participant UI as Customer Index<br/>(jQuery AJAX)
    participant Ctrl as CustomerController
    participant CustSvc as CustomerService
    participant CustRepo as CustomerRepository
    participant AuditSvc as AuditService
    participant AuditRepo as AuditRepository
    participant DB as SQL Server

    Employee->>UI: Load customer page
    UI->>Ctrl: GET /Customer/List
    Ctrl->>CustSvc: GetCustomersAsync(status)
    CustSvc->>CustRepo: GetAllAsync() / GetByStatusAsync()
    CustRepo->>DB: SELECT * FROM Customers
    DB-->>CustRepo: Customer list
    CustRepo-->>CustSvc: List<Customer>
    CustSvc-->>Ctrl: List<Customer>
    Ctrl-->>UI: JSON response
    UI-->>Employee: Render metric cards,<br/>search bar, avatar table

    Employee->>UI: Fill form → "Save & Activate"
    UI->>Ctrl: POST /Customer/Create<br/>{...data, submitAction: "active"}
    Ctrl->>CustSvc: AddAsync(customer)
    CustSvc->>CustRepo: AddAsync(customer)
    CustRepo->>DB: INSERT INTO Customers

    Ctrl->>AuditSvc: LogAsync("CustomerCreated", ...)
    AuditSvc->>AuditRepo: AddAsync(auditEntry)
    AuditRepo->>DB: INSERT INTO AuditEntries

    Ctrl-->>UI: { success: true, message: "..." }
    UI-->>Employee: Show success modal
```

---

## 5. Email Queue & Background Dispatch

```mermaid
flowchart LR
    subgraph Request Thread
        A["Service calls<br/>EmailQueue.EnqueueAsync()"] --> B[("EmailQueueEntries<br/>Status: Pending")]
    end

    subgraph Background Service
        C["EmailDispatcherService<br/>polls every 5 min"] --> B
        B --> D{Pending<br/>emails?}
        D -- Yes --> E["SmtpEmailSender<br/>sends email"]
        E --> F{Success?}
        F -- Yes --> G["Update status → Sent<br/>Set SentAt timestamp"]
        F -- No --> H{RetryCount<br/>< 3?}
        H -- Yes --> I["Increment RetryCount<br/>Set NextRetryAt<br/>(exponential backoff)"]
        H -- No --> J["Update status → Failed<br/>Record LastError"]
    end

    E --> K["📧 SMTP Server"]

    style G fill:#d1e7dd,stroke:#198754
    style J fill:#f8d7da,stroke:#dc3545
    style I fill:#fff3cd,stroke:#ffc107
```

---

## 6. Dependency Injection Registration

```mermaid
graph LR
    subgraph Program.cs
        P["builder.Services<br/>.AddAppServices()"]
    end

    subgraph DependencyInjectionService.cs
        P --> DB["DbContext<br/><i>Scoped</i>"]
        P --> ID["Identity + Roles<br/><i>Scoped</i>"]
        P --> SM["AppSignInManagerService<br/><i>Scoped</i>"]
        P --> JWT["JWT Bearer<br/><i>conditional</i>"]
        P --> CORS["CORS Policy"]
        P --> AUTH["Authorisation Policies"]

        P --> R1["IAdminNotificationRepository<br/><i>Scoped</i>"]
        P --> R2["IAuditRepository<br/><i>Scoped</i>"]
        P --> R3["IEmailQueueRepository<br/><i>Scoped</i>"]
        P --> R4["ICustomerRepository<br/><i>Scoped</i>"]
        P --> R5["ICountryDialCodeRepository<br/><i>Scoped</i>"]

        P --> S1["IAuditService<br/><i>Scoped</i>"]
        P --> S2["ICustomerService<br/><i>Scoped</i>"]
        P --> S3["ICustomerAccessService<br/><i>Scoped</i>"]
        P --> S4["ICustomerReferenceDataService<br/><i>Scoped</i>"]
        P --> S5["IAdminDashboardService<br/><i>Scoped</i>"]
        P --> S6["IEmployeeDashboardService<br/><i>Scoped</i>"]
        P --> S7["IAdminNotificationService<br/><i>Scoped</i>"]
        P --> S8["RsaCryptoService<br/><i>Singleton</i>"]
        P --> S9["EmailQueue<br/><i>Scoped</i>"]
        P --> S10["IEmailSender<br/><i>Transient</i>"]
        P --> S11["EmailDispatcherService<br/><i>HostedService</i>"]
        P --> S12["IUserRegistrationNotifier<br/><i>Scoped</i>"]
    end
```

---

## 7. Entity Relationship Diagram

```mermaid
erDiagram
    ApplicationUser ||--o{ AdminNotification : "receives"
    ApplicationUser ||--o{ AuditActor : "snapshot"
    AuditActor ||--o{ AuditEntry : "subject"
    AuditActor ||--o{ AuditEntry : "performer"
    Customer ||--o{ AuditEntry : "relates to"
    AdminNotification }o--o| ApplicationUser : "about"

    Customer {
        guid CustomerId PK
        string FirstName
        string Surname
        string Email
        string Phone
        string Nationality
        datetime DateOfBirth
        string AddressLine1
        string AddressLine2
        string City
        string ProvinceState
        string AddressCountry
        string AreaCode
        string IdNumber
        string Gender
        string DataSource
        string Notes
        enum Status "Draft | Active"
        datetime CreatedAt
        datetime UpdatedAt
    }

    CountryDialCode {
        int Id PK
        string CountryName
        string DialCode
    }
```

---

## 8. Middleware Pipeline

```mermaid
flowchart TD
    REQ["Incoming HTTP Request"] --> A["UseHttpsRedirection"]
    A --> B["UseRouting"]
    B --> C["UseCors<br/>('DefaultCorsPolicy')"]
    C --> D["UseAuthentication<br/>(Cookie + JWT)"]
    D --> E["UseAuthorisation<br/>(Roles + Policies)"]
    E --> F["MapStaticAssets"]
    F --> G{Route match?}
    G -- "/api/*" --> H["API Controllers<br/>(Token, Notifications)"]
    G -- "/{controller}/{action}" --> I["MVC Controllers<br/>(Customer, Admin, Home)"]
    G -- "/Identity/*" --> J["Razor Pages<br/>(Login, Register, Manage)"]
    G -- Static file --> K["wwwroot/<br/>(CSS, JS, libs)"]

    style REQ fill:#e3f2fd
    style H fill:#fff3e0
    style I fill:#fff3e0
    style J fill:#e8f5e9
    style K fill:#f3e5f5

```

---

## 9. ApplicationUser Enhancements

```mermaid
erDiagram
    ApplicationUser {
        string DisplayName
        bool IsApproved
        string ApprovedBy
        datetime ApprovedAt
    }
```

**Note:** The `ApplicationUser` entity includes custom fields for user display names and approval tracking. These fields are critical for the user approval workflow.

---

## 10. Role-Based Navigation Visibility

```mermaid
flowchart TD
    Navbar["Navbar"] --> AuthCheck["Check if user is signed in"]
    AuthCheck -- "Signed in" --> RoleCheck["Check user role"]
    RoleCheck -- "Administrator/Manager" --> ShowNotifBell["Show Notification Bell"]
    RoleCheck -- "Employee" --> ShowCustomerLink["Show Customers Link"]
    AuthCheck -- "Not signed in" --> HideProtectedLinks["Hide all protected links"]
```

**Note:** Navigation links are dynamically rendered based on the user's authentication state and role.

---

## 11. Notification Bell Modal Flow

```mermaid
sequenceDiagram
    actor Admin
    Admin->>Bell: Click Notification Bell
    Bell->>Modal: Open Notification Modal
    Modal->>Server: AJAX GET /admin/employees/notifications
    Server-->>Modal: JSON (Notification List)
    Admin->>Modal: Click Approve/Deny
    Modal->>Server: AJAX POST /admin/employees/approve-ajax or deny-ajax
    Server-->>Modal: JSON (Success/Failure)
    Modal-->>Bell: Update Notification Count
```

**Note:** The notification bell uses AJAX for instant updates without a full page reload.

---

## 12. CountryDialCode Usage

```mermaid
sequenceDiagram
    actor User
    User->>UI: Open Country Dropdown
    UI->>Server: AJAX GET /api/country-dial-codes
    Server-->>UI: JSON (Country List)
    UI-->>User: Render Dropdown Options
```

**Note:** The `CountryDialCode` table is used to populate dropdowns dynamically, ensuring consistent data.

---

## 13. DataSeeder Process

```mermaid
sequenceDiagram
    participant App as Application Startup
    participant Seeder as DataSeeder
    participant DB as SQL Server

    App->>Seeder: SeedInitialUsersAsync()
    Seeder->>DB: Check if roles exist
    DB-->>Seeder: Roles exist/not exist
    Seeder->>DB: Insert roles if missing
    Seeder->>DB: Check if default users exist
    DB-->>Seeder: Users exist/not exist
    Seeder->>DB: Insert default users if missing
```

**Note:** The `DataSeeder` ensures roles and default users are created on startup.

---

## 14. JWT/RSA API Login Flow

```mermaid
sequenceDiagram
    actor APIClient
    participant TokenAPI as TokenController
    participant RSA as RsaCryptoService
    participant Identity as ASP.NET Identity
    participant DB as SQL Server

    APIClient->>TokenAPI: Encrypted Login Request
    TokenAPI->>RSA: Decrypt Request
    RSA-->>TokenAPI: Plaintext Credentials
    TokenAPI->>Identity: Validate Credentials
    Identity->>DB: Check User
    DB-->>Identity: User Valid/Invalid
    Identity-->>TokenAPI: SignInResult
    TokenAPI-->>APIClient: JWT Token
```

**Note:** The API login flow uses RSA encryption for credentials and issues JWT tokens for stateless authentication.

---

## 15. Audit Logging on Customer Operations

```mermaid
sequenceDiagram
    actor Employee
    participant Ctrl as CustomerController
    participant AuditSvc as AuditService
    participant AuditRepo as AuditRepository
    participant DB as SQL Server

    Employee->>Ctrl: Perform Customer Action (Create/Edit/Delete)
    Ctrl->>AuditSvc: LogAsync(action, details)
    AuditSvc->>AuditRepo: AddAsync(auditEntry)
    AuditRepo->>DB: INSERT INTO AuditEntries
```

**Note:** All customer operations are logged for auditing purposes.
