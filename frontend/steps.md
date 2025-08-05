# Epic 7: API Test Coverage Checklist
### User Stories
- As a developer or QA, I want every API endpoint to be covered by automated tests (happy and negative paths), so that the backend remains robust and safe during changes.

- [x] Requirement endpoints: Create, Get by ID, List, Update, Delete, error paths
- [x] TestCase endpoints: Create, Get by ID, List, Update, Delete, error paths
- [x] TestCase steps: Add step, Remove step, error paths
- [x] TestSuite endpoints: Create, Get by ID, List, Update, Delete, error paths
- [x] TestPlan endpoints: Create, Get by ID, List, Update, Delete, error paths
- [x] User endpoints: Create, Get by ID, List, Update, Delete, error paths
- [x] User roles: Assign, Remove, Get roles, error paths
- [x] Role endpoints: Create, List, Delete, error paths
- [x] Requirement-TestCaseLink: Create, Get by requirement, Get by testcase, Delete, error paths
- [ ] Redline endpoints: Requirement and TestCase version listing, get version, redline comparison
- [ ] Test positive (success) and negative/error scenarios (invalid ID, missing fields, unauthorized, forbidden, not found, bad request).
- [ ] Test relationship and traceability endpoints (link/unlink requirements to test cases, hierarchical requirements, trace matrices).
- [ ] Test role-based access (different users/roles: Admin, QA, Developer, Viewer; ensure forbidden actions are blocked).
- [ ] Test versioning/history endpoints: creation, retrieval, rollback, redline comparison.
- [ ] Test attachment upload/download endpoints (if present).
- [ ] Test audit log retrieval and user action logging endpoints.
- [ ] Test list endpoints for pagination, filtering, and search parameters.
- [ ] Use code coverage tools to monitor and enforce coverage goals (90–100% for core, 70–90% for auxiliary endpoints).

## Epic 6: Automated Testing & Coverage
### User Stories
- As a developer or architect, I want comprehensive unit and integration tests for all backend features, so that I can be confident in the stability and correctness of the system as it evolves.

#### Tasks
- [x] Ensure all CRUD endpoints for Requirements, Test Cases, Test Plans, Test Suites, and Users have positive and negative unit tests.
- [x] Add tests for linkage APIs (Requirement-TestCase, Requirement-TestPlan): create, retrieve, and delete links.
- [x] Add tests for version history and redline endpoints, including edge/error cases.
- [x] Add tests for user and role management, including authorization/permission logic (if applicable).
- [ ] Integrate code coverage tooling (e.g., Coverlet, ReportGenerator) and monitor in CI.
- [ ] (Future) Add frontend/component tests for Blazor UI using bUnit or similar.
# Frontend Development Plan (Blazor WebAssembly)

## Epic 1: Application Shell & Navigation
### User Stories
- As a user, I want a consistent layout with a navigation menu so I can easily access all areas of the application.
- As a user, I want a home/dashboard page with a summary of requirements and test activities.

#### Tasks
- [x] Create main layout and navigation bar (sidebar or top nav)
- [x] Implement routing for major sections (Home, Requirements, Test Suites, Test Cases, Test Plans, Users)
- [x] Build Home/Dashboard page with summary widgets

---

## Epic 2: Requirements Management
### User Stories
- As a user, I want to view a list of requirements so I can see all current requirements.
- As a user, I want to create, edit, and delete requirements.
- As a user, I want to view details and version history of a requirement.

#### Tasks
- [x] Build Requirements list page (table/grid)
- [x] Add create/edit requirement form (modal or page)
- [x] Add delete requirement action
- [x] Connect to backend API for CRUD operations
- [x] Show requirement details and version history

---

## Epic 3: Test Management
### User Stories
- As a user, I want to manage test suites, test cases, and test plans.
- As a user, I want to link requirements to test cases and plans.
- As a user, I want to see test case version history and redlines.

#### Tasks
- [x] Build Test Suites list & detail pages
- [x] Build Test Cases list & detail pages
- [x] Build Test Plans list & detail pages
- [x] Implement linking between requirements and test cases/plans
- [ ] Show test case version history and redline comparisons

---

## Epic 4: User Management & Security
### User Stories
- As an admin, I want to manage users and their roles.
- As a user, I want to log in and see only the features I’m authorized for.

#### Tasks
- [ ] Add authentication (Azure AD or similar)
- [x] Build user list & role assignment page
- [ ] Implement authorization checks for UI features

---

## Epic 5: Traceability & Reporting
### User Stories
- As a user, I want to trace requirements to test cases and see coverage.
- As a user, I want to export reports (PDF, Excel, etc.).

#### Tasks
- [ ] Build traceability matrix UI
- [ ] Implement report generation and export

---

## Epic 6: UI/UX Polish & Accessibility
### User Stories
- As a user, I want a modern, responsive UI that is accessible.

#### Tasks
- [ ] Apply styling and themes
- [ ] Ensure responsive layouts
- [ ] Add accessibility features (ARIA, keyboard nav)

---

## How to Use This Plan
- Work through each Epic in order or in parallel as team size allows
- Check off tasks as you complete them
- Add new user stories or tasks as requirements evolve
