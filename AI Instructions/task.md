# Phase 1: Backend API

## Project Setup
- [ ] Install required NuGet packages (EF Core SQL Server, EF Core Tools, JwtBearer, Swashbuckle, FluentValidation, etc.)
- [ ] Configure [appsettings.json](file:///c:/Salik/Project/Form%20Builder%20Project/FormBuilder/FormBuilder/appsettings.json) with SQL Server connection string and JWT settings
- [ ] Create Options classes (e.g., `JwtOptions`) and register them using the Options pattern
- [ ] Create `ServiceCollectionExtensions` to register Repositories and Services cleanly
- [ ] Call the extension method in [Program.cs](file:///c:/Salik/Project/Form%20Builder%20Project/FormBuilder/FormBuilder/Program.cs)

## Data Layer (EF Core)
- [ ] Create `FormBuilderContext` inheriting from `DbContext`
- [ ] Configure Entity relationships and constraints (Fluent API)
- [ ] Add unique index on `Form.PublicSlug`
- [ ] Create initial EF Core Migration
- [ ] Apply migration to create database

## Repositories & Unit of Work
- [ ] Create generic `IRepository<T>` and `Repository<T>`
- [ ] Create `IUnitOfWork` and `UnitOfWork`

## Core Services & Helpers
- [ ] Create `AuthService` (Password hashing, JWT generation, Refresh token handling)
- [ ] Create `SlugService` for generating unique public slugs
- [ ] Create `FormService` (CRUD for forms, publish/unpublish)
- [ ] Create `FormFieldService` (CRUD for fields, reordering)
- [ ] Create `FormSubmissionService` (Handling public submissions, viewing admin submissions)
- [ ] Create DTOs for Requests and Responses
- [ ] Create Validators using FluentValidation

## API Controllers
- [ ] Create `AuthController` (Register, Login, Refresh, Logout)
- [ ] Create `FormController` (Admin: CRUD forms, Publish/Unpublish)
- [ ] Create `FormFieldController` (Admin: CRUD fields, Reorder)
- [ ] Create `FormSubmissionController` (Admin: View submissions)
- [ ] Create `PublicFormController` (Public: Get form by slug, Submit form)

## Middleware & Utilities
- [ ] Implement Global Exception Handling Middleware
- [ ] Configure Swagger with JWT Authorization support
- [ ] Test all endpoints to ensure they meet requirements

# Phase 2: Angular Frontend
- [ ] Scaffold Angular application
- [ ] Install frontend dependencies (Router, Forms, HTTP Client)
- [ ] Generate API client using NSwag
- [ ] Implement Auth module (Register, Login guards)
- [ ] Implement Admin Dashboard (My Forms list, Form Builder, Submissions view)
- [ ] Implement Public Form Renderer
- [ ] Integrate generated NSwag client
