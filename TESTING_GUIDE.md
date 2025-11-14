# Testing Guide - MyPortal Campaign Management API

## 🧪 API Testing Steps

### 1. Start the Application
```bash
cd MyPortalCampaign/src/MyPortal.Api
dotnet run
```
Application will start at: **http://localhost:5108**

---

## 📝 Test Scenarios

### Scenario 1: User Authentication

#### A. Login with Default Admin
```http
POST http://localhost:5108/api/auth/login
Content-Type: application/json

{
  "email": "admin@myportal.com",
  "password": "Admin@123"
}
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "admin@myportal.com",
    "userTypeId": 1,
    "statusId": 1,
    "profile": {
      "firstName": "System",
      "lastName": "Administrator"
    }
  }
}
```

#### B. Register New User
```http
POST http://localhost:5108/api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "SecurePass@123",
  "firstName": "John",
  "lastName": "Doe",
  "userTypeId": 3,
  "networkId": null
}
```

---

### Scenario 2: Campaign Management

#### A. Create Campaign
```http
POST http://localhost:5108/api/campaigndetails
Authorization: Bearer {your_token_here}
Content-Type: application/json

{
  "name": "Summer Sale 2025",
  "link": "https://example.com/offer",
  "rejectedLink": "https://example.com/sorry",
  "networkId": null
}
```

**Expected Response:**
```json
{
  "message": "Campaign created successfully",
  "campaign": {
    "id": 1,
    "name": "Summer Sale 2025",
    "link": "https://example.com/offer",
    "promotionLink": "SummerSale2025",
    "rejectedLink": "https://example.com/sorry",
    "statusId": 1
  }
}
```

#### B. Get All Campaigns
```http
GET http://localhost:5108/api/campaigndetails
Authorization: Bearer {your_token_here}
```

#### C. Get Campaign by ID
```http
GET http://localhost:5108/api/campaigndetails/1
Authorization: Bearer {your_token_here}
```

#### D. Update Campaign
```http
PUT http://localhost:5108/api/campaigndetails/1
Authorization: Bearer {your_token_here}
Content-Type: application/json

{
  "name": "Summer Sale 2025 Updated",
  "link": "https://example.com/new-offer",
  "rejectedLink": "https://example.com/sorry"
}
```

#### E. Delete Campaign
```http
DELETE http://localhost:5108/api/campaigndetails/1
Authorization: Bearer {your_token_here}
```

---

### Scenario 3: Campaign Click Tracking

#### A. Simulate Campaign Click
```bash
# First, create Base64 encoded parameters: email-firstname-lastname
# Example: john@example.com-John-Doe
# Base64: am9obkBleGFtcGxlLmNvbS1Kb2huLURvZQ==

# Then access the tracking URL:
curl -L http://localhost:5108/cpg/SummerSale2025/am9obkBleGFtcGxlLmNvbS1Kb2huLURvZQ==
```

**What happens:**
1. System extracts email, firstname, lastname
2. Validates your IP address
3. Creates a CampaignReport entry
4. Redirects to approved or rejected link

#### B. Check Campaign Report (via SQL)
```sql
SELECT TOP 10 * 
FROM CampaignReports 
ORDER BY CreatedAt DESC
```

---

## 🔧 Using Swagger UI

1. **Open Swagger:**
   ```
   http://localhost:5108/swagger
   ```

2. **Authorize:**
   - Click "Authorize" button (top right)
   - Enter: `Bearer {your_token_from_login}`
   - Click "Authorize" then "Close"

3. **Test Endpoints:**
   - Expand any endpoint
   - Click "Try it out"
   - Fill in parameters
   - Click "Execute"

---

## 📊 Database Verification

### Check Seeded Data
```sql
-- Check Statuses
SELECT * FROM Statuses;

-- Check User Types
SELECT * FROM UserTypes;

-- Check Admin User
SELECT u.*, up.* 
FROM Users u
LEFT JOIN UserProfiles up ON u.Id = up.UserId
WHERE u.Email = 'admin@myportal.com';
```

### Verify Campaign Reports
```sql
-- View all click tracking data
SELECT 
    cr.Campaign,
    cr.Email,
    cr.FirstName,
    cr.LastName,
    cr.Status,
    cr.Reason,
    cr.ClickIP,
    cr.Country,
    cr.CreatedAt
FROM CampaignReports cr
ORDER BY cr.CreatedAt DESC;
```

---

## 🧩 Encoding Parameters for Click Tracking

### Using PowerShell (Windows)
```powershell
$text = "john@example.com-John-Doe"
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($text))
```

### Using Terminal (Mac/Linux)
```bash
echo -n "john@example.com-John-Doe" | base64
```

### Using Node.js
```javascript
const params = "john@example.com-John-Doe";
const encoded = Buffer.from(params).toString('base64');
console.log(encoded);
```

### Using C#
```csharp
var parameters = "john@example.com-John-Doe";
var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(parameters));
```

---

## 🔍 Testing IP Validation

The system validates IP addresses against:
- ✅ VPN detection
- ✅ Proxy detection
- ✅ Tor network
- ✅ Datacenter IPs
- ✅ Trust scores

**To test locally:**
Your local IP (127.0.0.1) will be used, but production will validate real IPs.

---

## 📈 Expected Results

### Valid Click (No VPN/Proxy)
- Status: "Approved"
- Redirect: Campaign link
- Report Reason: null

### Invalid Click (VPN/Proxy Detected)
- Status: "Rejected"
- Redirect: Rejected link
- Report Reason: "VPN", "Proxy", "Bot", etc.

---

## 🐛 Common Issues

### Issue 1: 401 Unauthorized
**Solution:** Make sure you're including the Bearer token in the Authorization header

### Issue 2: Connection String Error
**Solution:** Verify SQL Server connection in appsettings.json

### Issue 3: IPData API Error
**Solution:** Add valid IPData API key in appsettings.json

### Issue 4: Migration Error
**Solution:** Run migrations:
```bash
dotnet ef database update --project src/MyPortal.Infrastructure --startup-project src/MyPortal.Api
```

---

## ✅ Test Checklist

- [ ] Application starts without errors
- [ ] Swagger UI loads at /swagger
- [ ] Login with admin credentials works
- [ ] Can create a new user
- [ ] Can create a campaign
- [ ] Can list all campaigns
- [ ] Can update a campaign
- [ ] Can delete a campaign
- [ ] Campaign click tracking redirects properly
- [ ] CampaignReport is created in database
- [ ] JWT token expires after configured time
- [ ] Unauthorized requests return 401

---

## 🎯 Performance Testing

### Load Test Campaign Clicks
```bash
# Using Apache Bench (if installed)
ab -n 1000 -c 10 http://localhost:5108/cpg/SummerSale2025/am9obkBleGFtcGxlLmNvbS1Kb2huLURvZQ==
```

---

## 📞 Getting Help

If tests fail:
1. Check application logs in console
2. Verify database connection
3. Check Swagger for API documentation
4. Review entity relationships in ApplicationDbContext.cs

---

**Happy Testing! 🚀**
