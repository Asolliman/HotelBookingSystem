# Hotel Booking Management System API

## Overview
A comprehensive REST API for hotel booking management built with ASP.NET Core Web API (.NET 8), Entity Framework Core, and SQLite. The system supports role-based access for guests, receptionists, and admins to manage rooms, reservations, and profiles.

## Technologies Used
- **ASP.NET Core Web API**: Framework for building RESTful APIs with .NET 8.
- **Entity Framework Core**: ORM for database operations and migrations.
- **SQLite**: Lightweight database for development and production.
- **ASP.NET Core Identity**: User authentication and role management.
- **JWT Bearer Authentication**: Secure token-based authentication.
- **Hangfire**: Background job processing for automated tasks.
- **LINQ**: Query language for data manipulation and projections.

## How to Run the Project

### Prerequisites
- .NET 8 SDK installed
- Terminal/Command Prompt access

### Quick Start (3 Steps)

**Step 1: Navigate to Project**
```bash
cd "c:\Users\mrsol\Desktop\Web project\HotelBookingSystem\HotelBookingAPI"
```

**Step 2: Run the Application**
```bash
dotnet run
```

**Step 3: Access the API**
- API: `https://localhost:5001`
- Hangfire Dashboard: `https://localhost:5001/hangfire` (Admin role required)

### What Happens on First Run
✅ SQLite database created automatically (HotelBookingAPI.db)  
✅ All database tables created (no migrations needed)  
✅ Three roles created: Admin, Receptionist, Guest  
✅ Admin user created automatically:
  - **Email**: `admin@hotel.com`
  - **Password**: `Admin123!`  
✅ Hangfire background job scheduler initialized  

### No Manual Setup Required
- No `dotnet restore` needed - happens automatically
- No `dotnet ef database update` needed - schema created on startup
- No SQL scripts to run - fully automated initialization

---

## Quick Testing Guide

**See [TESTING_GUIDE.md](TESTING_GUIDE.md) for complete testing instructions with:**
- Postman request examples
- Step-by-step workflow for all features
- Authentication and authorization examples
- Role-based access control examples
- Error handling reference
- Database and security information

**Or** use the included `.http` file in VS Code REST Client extension:
- File: `HotelBookingAPI.http`
- Install: [REST Client Extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
- Click "Send Request" on any endpoint

---

## Why HTTP-Only Cookies Are Commonly Used as an Industry Standard for Authentication Security

HTTP-only cookies are widely adopted in web authentication for several critical security reasons. First, they prevent client-side scripts from accessing cookie values through JavaScript, mitigating cross-site scripting (XSS) attacks where malicious scripts could steal session tokens. This isolation ensures that sensitive authentication data remains server-side only, reducing the attack surface significantly.

Second, HTTP-only cookies automatically include authentication credentials in every HTTP request to the server, eliminating the need for manual token management in client code. This simplifies implementation while maintaining security, as the browser handles transmission transparently.

Third, when combined with the Secure flag, HTTP-only cookies ensure encrypted transmission over HTTPS, protecting against man-in-the-middle attacks. The SameSite attribute further prevents cross-site request forgery (CSRF) by restricting cookie sending to same-origin requests.

Fourth, HTTP-only cookies support automatic expiration and renewal mechanisms, enabling sliding expiration windows that balance security with user experience. This prevents indefinite session validity while allowing seamless continued access.

Finally, major web frameworks and authentication providers like ASP.NET Core Identity, OAuth 2.0, and OpenID Connect standardize on HTTP-only cookies, creating a consistent security baseline across applications. This industry consensus ensures interoperability and reduces custom implementation risks that could introduce vulnerabilities.

In summary, HTTP-only cookies provide a robust, standardized defense-in-depth approach that addresses multiple attack vectors while maintaining usability, making them the industry standard for secure web authentication.