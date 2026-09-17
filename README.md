# Purchase Management System — Backend API (.NET 8)

Backend Web API built with **ASP.NET Core (.NET 8)**, **C#**, **Entity Framework Core**, and **SQL Server** for the Enhanzer Full Stack Developer Assessment.

---

## Table of Contents

- [Overview & Architecture](#overview--architecture)
- [Technology Stack](#technology-stack)
- [Architecture & Folder Structure](#architecture--folder-structure)
- [Database Setup](#database-setup)
- [Configuration](#configuration)
- [Getting Started](#getting-started)
- [API Endpoints Reference](#api-endpoints-reference)
  - [1. Authentication](#1-authentication)
  - [2. Locations](#2-locations)
  - [3. Purchase Bills](#3-purchase-bills)
- [Business Logic & Calculations](#business-logic--calculations)
- [Security & Authentication Strategy](#security--authentication-strategy)
- [CORS Configuration](#cors-configuration)

---

## Overview & Architecture

The application implements a clean multi-tier architecture adhering to separation of concerns:

```
Angular (Client) ──HTTP (Bearer JWT)──► ASP.NET Core Web API ──HTTP (JSON)──► External Enhanzer API
                                              │
                                              ▼
                                         SQL Server
                             (Location_Details, Purchase_Bills)
```

- **Non-Negotiable Isolation:** Angular frontend only communicates with this ASP.NET Core API. The backend handles external authentication against the Enhanzer POS API and manages persistence with SQL Server.
- **Clean Layering:** Strict one-way dependency flow: `Controller ➔ Service ➔ Repository ➔ Database (EF Core DbContext)`.

---

## Technology Stack

| Layer / Concern | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core Web API (.NET 8.0) |
| **Language** | C# 12 |
| **ORM & Data Access** | Entity Framework Core 8.0 (SQL Server Provider) |
| **Database** | Microsoft SQL Server |
| **Authentication** | JWT (JSON Web Tokens) with HMAC-SHA256 |
| **API Documentation** | Swagger / OpenAPI with JWT Bearer scheme |
| **Configuration** | `appsettings.json` + `dotenv.net` (.env support) |
| **External Service** | Enhanzer POS Staging API (`/api/External_Api/POS_Api/Invoke`) |

---

## Architecture & Folder Structure

```
PurchaseManagement.slnx
├── database/
│   └── EnhanzerAssignment.sql          # SQL Server initialization script
└── PurchaseManagement.Api/
    ├── Controllers/                    # HTTP endpoints & request validation
    │   ├── AuthController.cs           # POST /api/auth/login
    │   ├── LocationsController.cs      # GET /api/locations
    │   └── PurchaseBillController.cs   # /api/purchase-bills (CRUD & summary)
    ├── DTOs/                           # Strongly-typed request/response models
    │   ├── ApiResponseDto.cs
    │   ├── ExternalLoginRequestDto.cs
    │   ├── ExternalLoginResponseDto.cs
    │   ├── ItemSummaryDto.cs
    │   ├── LocationDto.cs
    │   ├── LoginRequestDto.cs
    │   ├── LoginResponseDto.cs
    │   ├── PurchaseBillListResponseDto.cs
    │   ├── PurchaseBillRequestDto.cs
    │   └── PurchaseBillResponseDto.cs
    ├── Models/                         # Domain & database entities
    │   ├── Location.cs                 # Location_Details table
    │   └── PurchaseBill.cs             # Purchase_Bills table
    ├── Services/                       # Business logic & external API integration
    │   ├── AuthService.cs              # Authenticates with Enhanzer, issues JWT
    │   ├── LocationService.cs          # Retrieves saved locations
    │   ├── PurchaseBillService.cs      # Performs exact item calculations
    │   └── JwtService.cs               # Generates and signs JWT tokens
    ├── Repositories/                   # EF Core database access abstractions
    │   ├── ILocationRepository.cs
    │   ├── LocationRepository.cs       # Transactional bulk replace of locations
    │   ├── IPurchaseBillRepository.cs
    │   └── PurchaseBillRepository.cs   # Adds & queries purchase bills & summary
    ├── Data/
    │   └── ApplicationDbContext.cs     # EF Core DbContext with fluent configurations
    ├── appsettings.json                # Base configuration & connection string
    ├── Program.cs                      # Service registration, DI, pipeline, & CORS
    └── PurchaseManagement.Api.csproj
```

---

## Database Setup

1. Open **SQL Server Management Studio (SSMS)** or Azure Data Studio.
2. Execute the script located at:
   ```
   database/EnhanzerAssignment.sql
   ```
3. This creates the database `EnhanzerAssignmentDB` and sets up the required tables:
   - **`Location_Details`**: Stores `Location_Code`, `Location_Name`, `CreatedAt`.
   - **`Purchase_Bills`**: Stores `Item`, `Batch`, `Standard_Cost`, `Standard_Price`, `Quantity`, `Discount`, `Total_Cost`, `Total_Selling`, `CreatedAt`.

*(Note: The application also includes automated table initialization in `Program.cs` on startup if the database exists).*

---

## Configuration

The application reads configuration from `appsettings.json` and optionally overrides with `.env` (a template is provided in `.env.example`).

### `appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EnhanzerAssignmentDB;TrustServerCertificate=True;"
  }
}
```

### Environment Variables (`.env`)
You can copy `.env.example` to `.env` to customize settings:
```ini
# SQL Server Database Configuration
DB_SERVER=localhost
DB_NAME=EnhanzerAssignmentDB
DB_USER=sa
DB_PASSWORD=your_password
DB_TRUST_SERVER_CERTIFICATE=True

# External Enhanzer Login API
EXTERNAL_LOGIN_API_URL=https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke

# JWT Authentication Configuration
JWT_SECRET_KEY=EnhanzerSecretKeyForJwtAuthentication2026_LongSecureKey!
JWT_ISSUER=PurchaseManagementApi
JWT_AUDIENCE=PurchaseManagementClient
JWT_EXPIRY_HOURS=8
```

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (LocalDB, Express, or Developer edition)

### Run the Application
From the repository root:
```bash
# 1. Restore dependencies
dotnet restore

# 2. Build and run
dotnet run --project PurchaseManagement.Api
```

The API will start listening (typically at `http://localhost:5048` or `https://localhost:7048`).

### Swagger Documentation
Once running, navigate to:
```
http://localhost:5048/swagger
```
You can inspect and execute all endpoints interactively through Swagger UI.

---

## API Endpoints Reference

### 1. Authentication

#### `POST /api/auth/login`
Validates user credentials against the external Enhanzer API. Upon successful login:
1. Replaces stored user locations in SQL Server table `Location_Details`.
2. Returns an authorization JWT token and the user's assigned locations.

- **Authentication:** None (Public)
- **Request Headers:** `Content-Type: application/json`
- **Request Body:**
```json
{
  "email": "info@enhanzer.com",
  "password": "Welcome#5"
}
```

- **Success Response (`200 OK`):**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "info@enhanzer.com",
  "userLocations": [
    {
      "Location_Code": "EZCMP1/EZLOC-29",
      "Location_Name": "Block C"
    },
    {
      "Location_Code": "EZCMP1/EZLOC-16",
      "Location_Name": "Head Office"
    }
  ]
}
```

- **Failure Responses:**
  - `400 Bad Request`: Input validation failure (e.g. empty email/password, invalid email format).
  - `401 Unauthorized`: Invalid credentials (`{"success": false, "message": "Invalid username or password"}`).
  - `500 Internal Server Error`: External service unreachable (`{"success": false, "message": "Unable to authenticate. Please try again."}`).

---

### 2. Locations

#### `GET /api/locations`
Returns saved locations from the `Location_Details` table in SQL Server to populate the Batch dropdown on the Purchase Bill page. Also aliased to `/api/location`.

- **Authentication:** `Bearer <JWT_TOKEN>`
- **Request Headers:**
  - `Authorization: Bearer <token>`
- **Success Response (`200 OK`):**
```json
[
  {
    "Location_Code": "EZCMP1/EZLOC-29",
    "Location_Name": "Block C"
  },
  {
    "Location_Code": "EZCMP1/EZLOC-16",
    "Location_Name": "Head Office"
  }
]
```

- **Error Responses:**
  - `401 Unauthorized`: Missing or expired token.
  - `500 Internal Server Error`: Database failure (`{"message": "Unable to load locations."}`).

---

### 3. Purchase Bills

#### `GET /api/purchase-bills`
Returns all created purchase bill rows and the overall item summary.

- **Authentication:** `Bearer <JWT_TOKEN>`
- **Success Response (`200 OK`):**
```json
{
  "items": [
    {
      "id": 1,
      "item": "Mango",
      "batch": "Head Office",
      "standardCost": 100.00,
      "standardPrice": 150.00,
      "quantity": 5,
      "discount": 20.00,
      "totalCost": 400.00,
      "totalSelling": 750.00,
      "createdAt": "2026-09-17T08:00:00Z"
    }
  ],
  "summary": {
    "totalItems": 1,
    "totalQuantity": 5
  }
}
```

---

#### `POST /api/purchase-bills`
Adds a new purchase bill item with server-side validation and calculation.

- **Authentication:** `Bearer <JWT_TOKEN>`
- **Request Body:**
```json
{
  "item": "Mango",
  "batch": "Head Office",
  "standardCost": 100,
  "standardPrice": 150,
  "quantity": 5,
  "discount": 20
}
```

- **Field Constraints:**
  - `item`: Must be one of the allowed fruit items: `Mango`, `Apple`, `Banana`, `Orange`, `Grapes`, `Kiwi`, `Strawberry`.
  - `batch`: Required string matching a location.
  - `standardCost`: Numeric, `>= 0`.
  - `standardPrice`: Numeric, `>= 0`.
  - `quantity`: Integer, `> 0`.
  - `discount`: Numeric percentage, `0` to `100`.

- **Success Response (`201 Created`):**
```json
{
  "id": 1,
  "item": "Mango",
  "batch": "Head Office",
  "standardCost": 100.00,
  "standardPrice": 150.00,
  "quantity": 5,
  "discount": 20.00,
  "totalCost": 400.00,
  "totalSelling": 750.00,
  "createdAt": "2026-09-17T08:00:00Z"
}
```

---

#### `GET /api/purchase-bills/summary`
Returns the aggregate summary for all purchase bills.

- **Authentication:** `Bearer <JWT_TOKEN>`
- **Success Response (`200 OK`):**
```json
{
  "totalItems": 3,
  "totalQuantity": 10
}
```

---

#### `GET /api/purchase-bills/items`
Returns the list of predefined allowed fruit items for autocomplete dropdown.

- **Authentication:** `Bearer <JWT_TOKEN>`
- **Success Response (`200 OK`):**
```json
[
  "Mango",
  "Apple",
  "Banana",
  "Orange",
  "Grapes",
  "Kiwi",
  "Strawberry"
]
```

---

## Business Logic & Calculations

### Calculation Rules
All calculations are performed via pure functions in `PurchaseBillService.cs`:

1. **Total Cost:**
   $$\text{Gross} = \text{Standard Cost} \times \text{Quantity}$$
   $$\text{Discount Amount} = \text{Gross} \times \left(\frac{\text{Discount}}{100}\right)$$
   $$\text{Total Cost} = \text{Gross} - \text{Discount Amount}$$

   *Worked Example:*
   - Standard Cost = 100, Quantity = 5, Discount = 20%
   - Gross = $100 \times 5 = 500$
   - Discount = $20\% \text{ of } 500 = 100$
   - Total Cost = $500 - 100 = 400$

2. **Total Selling:**
   $$\text{Total Selling} = \text{Standard Price} \times \text{Quantity}$$

   *Worked Example:*
   - Standard Price = 150, Quantity = 5
   - Total Selling = $150 \times 5 = 750$

3. **Item Summary:**
   - **Total Items:** Total number of line items (rows) in the table.
   - **Total Quantity:** Sum of `Quantity` values across all line items.

---

## Security & Authentication Strategy

1. **External Authentication:** Backend posts credentials to `ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`.
2. **Session Generation:** Upon verification, backend signs and issues a short-lived **JWT (JSON Web Token)** using HMAC-SHA256 (`JwtSecurityToken`).
3. **Claims:** Includes `sub`, `email`, `jti`, and `ClaimTypes.Name`.
4. **Client-Side Storage:** Angular client stores this token (e.g. in `sessionStorage`) and passes it in the `Authorization: Bearer <token>` header via an `HttpInterceptor`.
5. **Route Protection:** Protected backend endpoints use ASP.NET Core `[Authorize]`.

---

## CORS Configuration

Configured in `Program.cs` to allow the Angular development server:
- Allowed Origins: `http://localhost:4200`, `https://localhost:4200`
- Allowed Methods: Any (`GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`)
- Allowed Headers: Any
- Credentials: Allowed (`AllowCredentials()`)
