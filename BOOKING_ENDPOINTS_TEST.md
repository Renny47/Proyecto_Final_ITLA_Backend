# 🧪 TESTING PLAN - BOOKING ENDPOINTS

## 📋 Test Data for Swagger

### 1. 🔧 Admin - Create Booking
**POST /api/Admin**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "dateAndTime": "2026-02-25T10:00:00Z",
  "bookingState": 2,
  "bookedByClientName": null
}
```

### 2. 🔧 Admin - Create Another Booking
**POST /api/Admin**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002", 
  "dateAndTime": "2026-02-25T14:00:00Z",
  "bookingState": 2,
  "bookedByClientName": null
}
```

### 3. 👤 Client - Get Available Bookings
**GET /api/Client**

### 4. 👤 Client - Make a Booking
**POST /api/Client?bookingId=550e8400-e29b-41d4-a716-446655440001&clientName=Juan Perez**

### 5. 🏢 Reception - View All Bookings
**GET /api/Reception**

### 6. 🏢 Reception - View Specific Booking
**GET /api/Reception/550e8400-e29b-41d4-a716-446655440001**

## 📊 Expected Results

**BookingState Values:**
- BOOKED = 1
- NOT_BOOKED = 2

**Workflow:**
1. Admin creates bookings (NOT_BOOKED = 2)
2. Client views available bookings
3. Client books one (becomes BOOKED = 1) 
4. Reception can monitor all bookings