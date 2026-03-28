You are a senior full-stack engineer. Build a production-quality Form Builder application in two parts: first the ASP.NET Core Web API backend, then the Angular frontend. Complete the backend first, make sure it runs correctly, then create the Angular app and integrate it with the API using NSwag-generated TypeScript services.

Tech stack:

* Backend: ASP.NET Core Web API, .NET 10, Entity Framework Core, SQL Server
* Auth: JWT authentication with ownership-based authorization, and refresh token rotation according to OAuth guidelines.
* Frontend: Angular
* API client generation: NSwag
* Use clean, maintainable code and a practical folder structure that already exists in the project.
* Add comments only where useful, not excessive
* Basic Entities are already created in the project. You can find them in the Entities folder.
* Use the EF Core migrations to create the database.
* Use Repository Pattern for data access.
* Use Unit of Work Pattern for data access.
* Use Dependency Injection for data access.
* Use SOLID principles for code design.
* It will follow a Monolithic Architecture.
* Use logging for debugging and monitoring. Use any good package that provide good readability of logs.
* Use FluentValidation for validation.
* Use AutoMapper for mapping.
* Use Swagger for API documentation.
* Use NSwag for API client generation.
* Use JWT authentication for authentication.
* Use Refresh Token Rotation for refresh token.
* Use Ownership-based authorization for authorization.
* Use SQL Server for database.
* Use Global Exception Handling.

Project concept:
This is a secure multi-user dynamic form platform where authenticated users can create and manage only their own forms, publish them through a public shareable link, and collect submissions. Public users can access only published forms through the public link and submit responses. They must never see admin data or submissions.

Critical functional rules:

1. Admin users must register and log in.
2. Each authenticated user can only access their own forms and their own submissions.
3. Public forms are accessible only through a slug-based link, not numeric IDs.
4. Public links should look like:
   /f/{title-slug}-{short-random-code}
   Example:
   /f/job-application-form-4f7a2c
5. A form is publicly accessible only if IsPublished = true.
6. Public slug must be unique and indexed.
7. Slug should be created once and should not automatically change when the title changes.
8. Public users can submit forms but cannot view submissions.
9. Admin APIs must require authentication.
10. Resource ownership must be enforced server-side, not just by frontend filtering.

Build order:
Phase 1: Backend API
Phase 2: Angular frontend
Do not start frontend until backend is complete and runnable.

Backend requirements:
Create a .NET 10 Web API project using SQL Server and EF Core migrations.

Install all required NuGet packages yourself. Include at least:

* Microsoft.EntityFrameworkCore.SqlServer
* Microsoft.EntityFrameworkCore.Tools
* Microsoft.AspNetCore.Authentication.JwtBearer
* Microsoft.AspNetCore.Identity.EntityFrameworkCore if needed, or use PasswordHasher manually
* Swashbuckle.AspNetCore
* NSwag.AspNetCore if helpful for OpenAPI generation
* FluentValidation.AspNetCore if you choose validation
* Any other necessary packages

Use a sensible architecture. Keep it practical, not overengineered. A layered structure is fine, for example:

* Controllers
* Services
* Repositories
* Interfaces
* Entities
* Context
* DTOs
* Helpers
* Validators
* Middleware

Authentication and authorization:

* Implement register and login endpoints
* Use JWT bearer authentication
* Secure admin endpoints with [Authorize]
* Enforce ownership checks in backend for form access, update, delete, publish, unpublish, and viewing submissions
* Return 404 or 403 when a user tries to access another user’s form; choose one approach consistently

Entities and schema:
Use GUIDs for identifiers.

User: (look in the entites folder for the user entity this is already created)

* Id
* Name
* Email
* PasswordHash
* PasswordSalt
* CreatedOn

Form: (look in the entites folder for the form entity this is already created)

* Id
* OwnerUserId
* Title
* Description
* PublicSlug
* IsPublished
* CreatedOn
* UpdatedOn

FormField: (look in the entites folder for the formfield entity this is already created)

* Id
* FormId
* Label
* FieldType
* Placeholder
* IsRequired
* SortOrder
* OptionsJson
* MinLength
* MaxLength
* MinValue
* MaxValue

FormSubmission: (look in the entites folder for the formsubmission entity this is already created)

* Id
* FormId
* SubmittedOn
* SubmitterEmail
* SubmitterName

FormSubmissionValue: (look in the entites folder for the formsubmissionvalue entity this is already created)

* Id
* SubmissionId
* FieldId
* Value

Data type guidance:

* All Id fields should be GUIDs
* Value should be string in C# and NVARCHAR(MAX) in SQL Server
* Value must support text, email, number, date, checkbox, dropdown, radio, and JSON string when needed
* Validation must happen in application/backend logic based on FieldType

Supported field types:

* Text
* TextArea
* Number
* Date
* Dropdown
* Checkbox
* Radio
* Email

Database and migrations:

* Configure SQL Server connection string in appsettings.json
* Create EF Core DbContext
* Add proper entity configurations, keys, relationships, required constraints, and indexes
* Add unique index on Form.PublicSlug
* Add migrations and update database
* Ensure delete behavior is sensible

Slug generation:
Implement slug generation using sanitized title plus a short random suffix.
Example:
Title: Job Application Form
Slug: job-application-form-4f7a2c
Create a reusable slug service.
Do not regenerate automatically on title update.

Validation:
Validate both at API level and business logic level.
Public submission validation must verify:

* required fields
* email format
* numeric parsing
* date parsing
* min/max length
* min/max numeric values
* dropdown/radio values must exist in options
  Do not rely on frontend validation alone.

API endpoints:

Auth:

* POST /api/auth/register
* POST /api/auth/login
* POST /api/auth/refresh
* POST /api/auth/logout

Admin forms:

* GET /api/my/forms
* POST /api/my/forms
* GET /api/my/forms/{id}
* PUT /api/my/forms/{id}
* DELETE /api/my/forms/{id}
* POST /api/my/forms/{id}/publish
* POST /api/my/forms/{id}/unpublish

Admin fields:

* POST /api/my/forms/{formId}/fields
* PUT /api/my/forms/{formId}/fields/{fieldId}
* DELETE /api/my/forms/{formId}/fields/{fieldId}
* POST /api/my/forms/{formId}/fields/reorder

Admin submissions:

* GET /api/my/forms/{id}/submissions
* GET /api/my/forms/{id}/submissions/{submissionId}

Public:

* GET /api/public/forms/{slug}
* POST /api/public/forms/{slug}/submit

DTOs:
Create request/response DTOs for all endpoints.
For public form rendering, return a clean DTO like:

* id
* title
* description
* fields[]
  Each field DTO should include:
* id
* label
* fieldType
* isRequired
* placeholder
* sortOrder
* options
* minLength
* maxLength
* minValue
* maxValue


For form submission request:

* formId
* submitterEmail
* submitterName
* values: array of { fieldId, value }

Swagger/OpenAPI:

* Enable Swagger
* Ensure JWT auth is documented in Swagger
* Ensure OpenAPI spec is available for NSwag consumption

Seeding:
Optionally seed one demo user and one sample form if helpful, but do not hardcode insecure defaults unless clearly documented.

Code quality:

* Use async/await properly
* Add error handling middleware or equivalent
* Return consistent API responses
* Keep controllers thin
* Use services for core logic
* Keep authorization and ownership checks reusable
* Make the project compile and run

After backend is complete:

1. Run the API and verify it builds successfully
2. Then create the Angular frontend
3. Install Angular dependencies yourself
4. Use NSwag to generate Angular TypeScript API clients/services from the backend OpenAPI spec
5. Do not hand-write duplicate API service methods if NSwag can generate them

Frontend requirements:
Create a clean Angular app for this product.

Install required frontend packages yourself, including:

* Angular router
* Forms support
* HTTP client
* Any UI library only if needed; keep it lightweight
* NSwag-related generation setup
* Any package required for auth token handling if needed

Frontend pages:
Public:

* Public form renderer page accessible by slug
* Submit success state

Auth:

* Register page
* Login page

Admin:

* My Forms list page
* Create/Edit Form page
* Submissions list page
* Submission detail page if needed

Frontend behavior:

* Admin pages require authentication
* Store JWT securely in a practical way for this project
* Add auth guard for protected routes
* Use NSwag-generated services for API calls
* Configure interceptor to attach bearer token
* Build a dynamic form renderer based on API field definitions
* Support these field types:
  Text, TextArea, Number, Date, Dropdown, Checkbox, Radio, Email
* Add client-side validation matching backend rules where practical
* In admin form editor, allow:

  * create form
  * update title and description
  * add/edit/remove field
  * reorder field with move up/down
  * publish/unpublish
  * preview form
* In submissions page, show responses only for the logged-in user’s own forms

UI structure:
Form create/edit page should have:

* left panel: form metadata and field list/editor
* right panel: live preview
  Keep the UI simple, professional, and functional.
  Do not add drag-and-drop in v1. Use move up/down buttons for ordering.
  Do not build unnecessary features like teams, organizations, or per-form collaborators.

NSwag integration:

* Configure backend OpenAPI properly
* Generate Angular client code from the backend Swagger/OpenAPI document
* Place generated services/models in a clear folder
* Make frontend consume only generated services for API communication where possible
* Include the NSwag config file and generation command/script
* Ensure generated code can be regenerated cleanly if API changes

Deliverables:

* Complete backend code
* Complete Angular frontend code
* EF Core migrations
* NSwag configuration
* Commands used to install required packages
* Commands used to create/update database
* Commands used to generate Angular services from NSwag
* Short README with setup/run instructions for both backend and frontend

Important implementation constraints:

* Do not overengineer
* Do not build a full ERP or framework
* Build a polished, client-friendly showcase project
* Make decisions that highlight practical product thinking, security, and maintainability
* Prefer a working end-to-end MVP over excessive abstraction

Execution instructions:

1. Scaffold and implement the backend completely
2. Install all required NuGet packages
3. Implement entities, DbContext, migrations, auth, authorization, services, controllers, validation, Swagger, and slug generation
4. Ensure the backend builds successfully
5. Then scaffold Angular app
6. Install frontend dependencies
7. Configure NSwag and generate API clients
8. Implement auth, admin pages, public form page, route guards, interceptor, dynamic rendering, and form management UI
9. Ensure the final solution is runnable

At the end, provide:

* a summary of what was built
* all package install commands
* all migration commands
* the NSwag generation command
* how to run backend and frontend locally


Most Important: 
* If you find any instruction missing or unclear, ask me for clarification before proceeding.