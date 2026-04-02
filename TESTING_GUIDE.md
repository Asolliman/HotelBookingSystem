# Hotel Booking Management System API - Testing Guide

## Quick Start

### Prerequisites
- .NET 8 SDK installed
- Postman or similar REST client
- Terminal/Command Prompt

### Step 1: Run the Application
Navigate to the project directory and start the API:

```bash
cd "c:\Users\mrsol\Desktop\Web project\HotelBookingSystem\HotelBookingAPI"
dotnet run
```

On first run, the API will:
- Create SQLite database (HotelBookingAPI.db) automatically
- Create all database tables and schemas
- Seed 3 roles: Admin, Receptionist, Guest
- Create admin user: `admin@hotel.com` / `Admin123!`
- Register Hangfire background job scheduler

**Expected Output:**
- Database tables created
- Admin user created
- Hangfire recurring job registered
- Server listening on `https://localhost:5001`

### Step 2: Authenticate (Get JWT Token)

**POST** `https://localhost:5001/api/auth/login`

**Body (JSON):**
```json
{
  "email": "admin@hotel.com",
  "password": "Admin123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-refresh-token-123",
  "fullName": "Admin User",
  "email": "admin@hotel.com",
  "role": "Admin",
  "expiresIn": 3600
}
```

**Copy the `accessToken`** - you'll need it for all subsequent requests.

### Step 3: Set Authorization Header in Postman

For all API requests, add this header:
```
Authorization: Bearer <accessToken>
```

(Replace `<accessToken>` with the token from Step 2)

---

## Core API Endpoints

### 1. Room Type Management (Admin Only)

#### Create Room Type
**POST** `https://localhost:5001/api/roomtypes`

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "name": "Deluxe Suite",
  "description": "Luxury room with ocean view and private balcony"
}
```

**Expected Response (201 Created):**
```json
{
  "id": 1,
  "name": "Deluxe Suite",
  "description": "Luxury room with ocean view and private balcony"
}
```

#### Get All Room Types
**GET** `https://localhost:5001/api/roomtypes`

#### Get Room Type by ID
**GET** `https://localhost:5001/api/roomtypes/1`

#### Update Room Type
**PUT** `https://localhost:5001/api/roomtypes/1`

**Body:**
```json
{
  "name": "Deluxe Suite",
  "description": "Updated description"
}
```

#### Delete Room Type
**DELETE** `https://localhost:5001/api/roomtypes/1`

---

### 2. Room Management (Admin Only)

#### Create Room
**POST** `https://localhost:5001/api/rooms`

**Body:**
```json
{
  "roomNumber": "101",
  "floor": 1,
  "pricePerNight": "150.00",
  "roomTypeId": 1
}
```

**Expected Response (201 Created):**
```json
{
  "id": 1,
  "roomNumber": "101",
  "floor": 1,
  "isAvailable": true,
  "pricePerNight": 150.00,
  "roomTypeName": "Deluxe Suite"
}
```

#### Get All Rooms
**GET** `https://localhost:5001/api/rooms`

#### Get Room by ID
**GET** `https://localhost:5001/api/rooms/1`

#### Update Room
**PUT** `https://localhost:5001/api/rooms/1`

**Body:**
```json
{
  "roomNumber": "101",
  "floor": 1,
  "pricePerNight": "160.00",
  "roomTypeId": 1
}
```

#### Delete Room
**DELETE** `https://localhost:5001/api/rooms/1`

---

### 3. User Registration (Guest)

#### Register New User
**POST** `https://localhost:5001/api/auth/register`

**No authorization required** - open to all

**Body:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

**Expected Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random-refresh-token-456",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "role": "Guest",
  "expiresIn": 3600
}
```

---

### 4. Guest Profile Management

#### Create/Update Guest Profile
**POST** `https://localhost:5001/api/guestprofiles`

**Body:**
```json
{
  "phoneNumber": "+1-555-0123",
  "address": "123 Main St, New York, NY 10001",
  "nationalId": "123-45-6789"
}
```

**Expected Response (201 Created):**
```json
{
  "id": 1,
  "userId": "user-id-guid",
  "phoneNumber": "+1-555-0123",
  "address": "123 Main St, New York, NY 10001",
  "nationalId": "123-45-6789",
  "fullName": "John Doe",
  "email": "john.doe@example.com"
}
```

#### Get Own Profile
**GET** `https://localhost:5001/api/guestprofiles/me`

#### Get Profile by ID (Admin/Receptionist)
**GET** `https://localhost:5001/api/guestprofiles/1`

---

### 5. Reservation Management

#### Create Reservation (Guest)
**POST** `https://localhost:5001/api/reservations`

**Body:**
```json
{
  "roomId": 1,
  "checkInDate": "2024-12-20",
  "checkOutDate": "2024-12-23"
}
```

**Expected Response (201 Created):**
```json
{
  "id": 1,
  "userId": "user-id-guid",
  "roomNumber": "101",
  "roomType": "Deluxe Suite",
  "checkInDate": "2024-12-20",
  "checkOutDate": "2024-12-23",
  "status": "Pending",
  "isPaid": false,
  "totalPrice": 450.00,
  "createdAt": "2024-12-18T10:30:00Z"
}
```

#### Get All Reservations (Admin/Receptionist)
**GET** `https://localhost:5001/api/reservations`

#### Get Own Reservations (Guest)
**GET** `https://localhost:5001/api/reservations/my`

#### Get Reservation by ID
**GET** `https://localhost:5001/api/reservations/1`

#### Update Reservation Status (Admin/Receptionist)
**PUT** `https://localhost:5001/api/reservations/1`

**Body:**
```json
{
  "status": "Confirmed",
  "isPaid": true
}
```

#### Cancel Reservation (Admin/Receptionist)
**DELETE** `https://localhost:5001/api/reservations/1`

---

### 6. JWT Token Management

#### Refresh Token
**POST** `https://localhost:5001/api/auth/refresh`

**Body:**
```json
{
  "refreshToken": "random-refresh-token-123"
}
```

**Expected Response (200 OK):**
```json
{
  "accessToken": "new-access-token...",
  "refreshToken": "new-refresh-token...",
  "expiresIn": 3600
}
```

Token automatically expires in 60 minutes.

#### Revoke Token (Admin Only)
**POST** `https://localhost:5001/api/auth/revoke`

**Body:**
```json
{
  "refreshToken": "random-refresh-token-123"
}
```

---

## Role-Based Access Control

### Admin Role
Full access to all endpoints:
- Create/update/delete room types
- Create/update/delete rooms
- View all reservations
- Manage guest profiles
- Revoke refresh tokens
- Access Hangfire dashboard

### Receptionist Role
Limited management access:
- View all reservations
- Update reservation status
- View guest profiles
- View available rooms
- Cannot create/delete resources

### Guest Role
Self-service access:
- Register and login
- Create/manage own reservations
- View own reservations
- Create/update own profile
- Cannot view other guests' data

---

## Background Jobs & Hangfire Dashboard

The API includes a recurring Hangfire job that runs every hour:
- **Job**: `CancelUnpaidReservations`
- **Schedule**: 3:00 AM UTC daily (can be configured)
- **Action**: Automatically cancels reservations that remain unpaid after 24 hours

### Access Hangfire Dashboard
**URL**: `https://localhost:5001/hangfire`

**Requires**: Admin role authentication via login first

**Features**:
- View job execution history
- Monitor recurring jobs
- Check job status and logs
- Manually trigger jobs (if needed)

---

## Testing Workflow Example

### Complete Flow: Admin Setup → Guest Booking

**1. Admin Login**
- POST `/api/auth/login` with admin credentials
- Save the accessToken

**2. Admin Creates Room Type**
- POST `/api/roomtypes` with room type data
- Copy the returned `id`

**3. Admin Creates Room(s)**
- POST `/api/rooms` with room data using the roomTypeId
- Copy the returned `id`

**4. Guest Registers**
- POST `/api/auth/register` with guest details
- Save the guest's accessToken

**5. Guest Creates Profile**
- POST `/api/guestprofiles` with profile details (using guest token)

**6. Guest Books Room**
- POST `/api/reservations` with checkIn/checkOut dates and roomId (using guest token)

**7. Admin Confirms Booking**
- PUT `/api/reservations/{id}` to update status to "Confirmed" and set isPaid = true
- Room automatically becomes unavailable

**8. Optional: Review in Hangfire**
- Visit `/hangfire` dashboard to see job execution logs

---

## Error Handling

### Common Error Responses

**401 Unauthorized** - Invalid or missing JWT token
```json
{
  "error": "Authorization header missing or invalid"
}
```

**403 Forbidden** - Insufficient permissions for role
```json
{
  "error": "User does not have required role"
}
```

**400 Bad Request** - Invalid input data
```json
{
  "error": "Model validation failed",
  "details": { "roomNumber": "Room number is required" }
}
```

**404 Not Found** - Resource doesn't exist
```json
{
  "error": "Room with ID 999 not found"
}
```

**500 Internal Server Error** - Server error
```json
{
  "error": "An unexpected error occurred"
}
```

---

## Database

The API uses SQLite for data persistence:
- **File**: `HotelBookingAPI.db` (created in project root on first run)
- **Tables Created Automatically**:
  - AspNetUsers (Identity users)
  - AspNetRoles (Identity roles)
  - AspNetUserRoles (User-role mapping)
  - GuestProfiles
  - Rooms
  - RoomTypes
  - Reservations
  - RefreshTokens

No manual migrations needed - database schema created automatically on app startup.

---

## Security Features Implemented

✅ **JWT Authentication**: Secure token-based authentication with refresh tokens  
✅ **Role-Based Authorization**: Admin, Receptionist, Guest roles with granular permissions  
✅ **Password Hashing**: ASP.NET Core Identity with PBKDF2 hashing  
✅ **Token Expiration**: Access tokens expire in 60 minutes  
✅ **Token Revocation**: Admin can revoke refresh tokens  
✅ **SQL Injection Prevention**: EF Core parameterized queries  
✅ **HTTPS**: HTTPS enforced in production  
✅ **CORS**: Ready for frontend integration  

---

## Troubleshooting

### Database Already Exists
If you get "database is locked" error:
```bash
# Delete the old database
rm HotelBookingAPI.db
# Run the app again - it will recreate the database
dotnet run
```

### Port Already in Use
If port 5001 is already in use:
```bash
# Check the appsettings files or modify launchSettings.json
# Or use a different port:
dotnet run --urls "https://localhost:5002"
```

### Token Expired
If you get a 401 error:
- Use the `refreshToken` from the previous response
- POST to `/api/auth/refresh` to get a new accessToken

### Seed Data Issues
If you see "Role creation failed" in logs:
- The app will continue to run - seeding failures are non-critical
- Manually create roles via API if needed

---

## Next Steps

1. ✅ Review this guide and test all endpoints
2. ✅ Create test cases for your specific requirements
3. ✅ Integrate with frontend application
4. ✅ Deploy to production (Azure, AWS, etc.)
5. ✅ Monitor Hangfire jobs in production
6. ✅ Set up database backups

---

**Happy Testing! 🚀**
