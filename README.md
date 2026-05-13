# FormBuilder

A full-stack dynamic form builder web application that lets authenticated users create, publish, and manage custom forms — and collect responses through a shareable public link. Built with Angular 21 on the frontend and ASP.NET Core 10 on the backend.

---

## Features

### Form Management
- Create forms with a title and optional description
- Visual two-panel editor — configure fields on the left, see a live preview on the right in real time
- Edit form title and description inline without leaving the builder
- Publish and unpublish forms with a single click
- Copy the public shareable link directly from the builder
- Delete forms from the dashboard

### Field Builder
- **8 field types:** Text, TextArea, Email, Number, Date, Dropdown, Radio, Checkbox
- Set a label, placeholder, and required/optional toggle per field
- Configure min/max length constraints for text fields
- Configure min/max value constraints for number fields
- Add comma-separated options for Dropdown, Radio, and Checkbox fields (up to 20 options, 200 characters each)
- Reorder fields using up/down controls — order is persisted to the database
- Edit or delete any field inline without a page reload

### Contact Fields
- Every form includes **Your Name** and **Your Email** collector fields by default
- Toggle each contact field between Required and Optional independently
- Remove either field with a confirmation dialog that warns the owner they will lose submitter identity data
- Add removed fields back at any time

### Public Form
- Respondents access the form via a clean public URL — no account required
- Contact fields and all custom fields render according to the owner's configuration
- Required fields are validated before submission
- Success screen with a **Submit another response** button after submission

### Submissions Dashboard
- View all responses for any form in a dedicated submissions page
- Displays submitter name, email, submission date, and all field answers

### Authentication
- Register and login with email and password
- JWT access tokens with refresh token rotation — sessions persist across page reloads
- Protected routes redirect unauthenticated users to the login page

### UX
- Toast notification system with colour-coded success, error, warning, and info messages
- Inline validation error display with field-level detail from the backend
- Loading and empty states throughout the application
- Fully responsive layout

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21 (zoneless, standalone components, signals) |
| Backend | ASP.NET Core 10 Web API |
| Database | SQL Server with Entity Framework Core 10 |
| Auth | JWT + Refresh Token rotation |
| Validation | FluentValidation |
| API Client | NSwag-generated TypeScript client |
| Styling | Component-scoped CSS with CSS custom properties |

---

## Architecture

```
FormBuilder/
├── FormBuilder/                  # ASP.NET Core backend
│   ├── Controllers/              # REST API endpoints
│   ├── Services/                 # Business logic
│   ├── Entities/                 # EF Core entity models
│   ├── Dtos/                     # Request / response DTOs
│   ├── Validators/               # FluentValidation rules
│   ├── Middleware/               # Global exception handler
│   ├── Exceptions/               # Typed exception hierarchy
│   ├── Interfaces/               # Service contracts
│   └── Migrations/               # EF Core migrations
│
└── formbuilder-ui/               # Angular frontend
    └── src/app/
        ├── core/
        │   ├── api/              # NSwag-generated API client
        │   ├── interceptors/     # JWT attach + token refresh
        │   ├── services/         # Toast service
        │   └── utils/            # API error parser
        ├── features/
        │   ├── auth/             # Login & register
        │   ├── dashboard/        # Form list + submissions
        │   ├── form-builder/     # Visual form editor
        │   └── public-form/      # Public submission page
        └── shared/
            └── toast-container/  # Toast notification UI
```

### API Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/Auth/register` | Register a new user |
| POST | `/api/Auth/login` | Login and receive tokens |
| POST | `/api/Auth/refresh` | Refresh access token |
| GET | `/api/my/forms` | List all forms for the current user |
| POST | `/api/my/forms` | Create a new form |
| GET | `/api/my/forms/{id}` | Get a single form with fields |
| PUT | `/api/my/forms/{id}` | Update form title and description |
| DELETE | `/api/my/forms/{id}` | Delete a form |
| POST | `/api/my/forms/{id}/publish` | Publish a form |
| POST | `/api/my/forms/{id}/unpublish` | Unpublish a form |
| PATCH | `/api/my/forms/{id}/contact-settings` | Update contact field settings |
| POST | `/api/my/forms/{id}/fields` | Add a field |
| PUT | `/api/my/forms/{id}/fields/{fieldId}` | Update a field |
| DELETE | `/api/my/forms/{id}/fields/{fieldId}` | Delete a field |
| POST | `/api/my/forms/{id}/fields/reorder` | Reorder fields |
| GET | `/api/my/forms/{id}/submissions` | View submissions for a form |
| GET | `/api/public/forms/{slug}` | Load a published form by slug |
| POST | `/api/public/forms/{slug}/submit` | Submit a response |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server (local or Azure)

### Backend Setup

```bash
cd FormBuilder

# Update the connection string in appsettings.json
# "DefaultConnection": "Server=...;Database=FormBuilder;..."

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run
```

The API starts on `https://localhost:7001` by default.

### Frontend Setup

```bash
cd formbuilder-ui

# Install dependencies
npm install

# Start the dev server
ng serve
```

The app opens at `http://localhost:4200`.

### Production Build

```bash
cd formbuilder-ui
ng build --configuration production
```

Output is in `formbuilder-ui/dist/formbuilder-ui/`.

---

## Environment Configuration

**Backend — `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<your SQL Server connection string>"
  },
  "Jwt": {
    "SecretKey": "<your secret key>",
    "Issuer": "<your issuer>",
    "Audience": "<your audience>"
  }
}
```

**Frontend — `src/environments/environment.prod.ts`**
```typescript
export const environment = {
  production: true,
  apiBaseUrl: 'https://your-api-domain.com'
};
```

---

## License

This project is open source and available under the [MIT License](LICENSE.txt).
