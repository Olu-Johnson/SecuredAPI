# Entity Audit Results - Node.js vs .NET

## Overview
Systematic comparison of all Sequelize models (Node.js) against EF Core entities (.NET) to ensure frontend compatibility.

---

## ✅ ENTITIES THAT MATCH PERFECTLY

### 1. User ✅
- **Node.js fields**: email, password, userTypeId, providerTypeId, networkId, statusId
- **. NET fields**: Email, Password, UserTypeId, ProviderTypeId, NetworkId, StatusId
- **Status**: ✅ PERFECT MATCH

### 2. Network ✅
- **Node.js fields**: company, taxDetails, accountSecreteKey, networkLogo, networkUrl, networkName, networkSignupUrl, statusId
- **.NET fields**: Company, TaxDetails, AccountSecretKey, NetworkLogo, NetworkUrl, NetworkName, NetworkSignupUrl, StatusId
- **Status**: ✅ PERFECT MATCH

### 3. UserProfile ✅
- **Node.js fields**: userId, firstName, lastName, middleName, phoneNumber, company, socialContact, customProfileDetails, taxDetails, accountSecreteKey, picture, address, country, state, city, postalCode, statusId
- **.NET fields**: UserId, FirstName, LastName, MiddleName, PhoneNumber, Company, SocialContact, CustomProfileDetails, TaxDetails, AccountSecretKey, Picture, Address, Country, State, City, PostalCode, StatusId
- **Status**: ✅ PERFECT MATCH

### 4. Contact ✅
- **Node.js fields**: firstname, lastname, email, phone, company, skype, country, manager, networkId, userId, statusId
- **.NET fields**: FirstName, LastName, Email, Phone, Company, Skype, Country, Manager, NetworkId, UserId, StatusId
- **Status**: ✅ PERFECT MATCH

### 5. Group ✅
- **Node.js fields**: groupName, networkId, userId, statusId
- **.NET fields**: GroupName, NetworkId, UserId, StatusId
- **Status**: ✅ PERFECT MATCH

### 6. ContactGroup ✅
- **Node.js fields**: contactId, groupId, networkId, userId, statusId
- **.NET fields**: ContactId, GroupId, NetworkId, UserId, StatusId
- **Status**: ✅ PERFECT MATCH

### 7. Email ✅
- **Node.js fields**: from, to, subject, message, typeId, retryCount, attachment, fileName, networkId, statusId
- **.NET fields**: From, To, Subject, Message, TypeId, RetryCount, Attachment, FileName, NetworkId, StatusId
- **Status**: ✅ PERFECT MATCH

### 8. CampaignDetails ✅
- **Node.js fields**: name, link, promotionLink, rejectedLink, networkId, statusId
- **.NET fields**: Name, Link, PromotionLink, RejectedLink, NetworkId, StatusId
- **Status**: ✅ PERFECT MATCH

### 9. Token ✅
- **Node.js fields**: value, purpose, isUsed, expiredAt, userId, statusId
- **.NET fields**: Value, Purpose, IsUsed, ExpiredAt, UserId, StatusId
- **Status**: ✅ PERFECT MATCH

### 10. Guarantor ✅
- **Node.js fields**: userId, firstName, lastName, phoneNumber, address, relationship, documentPath
- **.NET fields**: UserId, FirstName, LastName, PhoneNumber, Address, Relationship, DocumentPath
- **Status**: ✅ PERFECT MATCH

### 11. Leave ✅
- **Node.js fields**: userId, leaveType, startDate, endDate, reason, status, leaveDays
- **.NET fields**: UserId, LeaveType, StartDate, EndDate, Reason, Status, LeaveDays
- **Status**: ✅ PERFECT MATCH

---

## ⚠️ ENTITIES WITH MISMATCHES

### 1. CampaignReport ⚠️

**Node.js Model** (CampaignReport.js):
```javascript
{
    clickTime: STRING,
    email: STRING,
    firstname: STRING,
    lastname: STRING,
    campaign: STRING,
    status: STRING,
    Reason: STRING,           // Capital R
    clickIP: STRING,
    country: STRING,
    osName: STRING,
    deviceType: STRING,
    broswerName: STRING,      // ⚠️ TYPO: "broswer" not "browser"
    networkId: INTEGER
}
```

**.NET Entity** (CampaignReport.cs):
```csharp
{
    ClickTime: string,
    Email: string,
    FirstName: string,        // ⚠️ MISMATCH: Should be "firstname" (lowercase)
    LastName: string,         // ⚠️ MISMATCH: Should be "lastname" (lowercase)
    Campaign: string,
    Status: string,
    Reason: string,
    ClickIP: string,
    Country: string,
    OsName: string,
    DeviceType: string,
    BrowserName: string,      // ⚠️ MISMATCH: Node.js has "broswerName" (typo)
    NetworkId: int
}
```

**Issues**:
1. ❌ **firstname** vs **FirstName** - Database likely has lowercase
2. ❌ **lastname** vs **LastName** - Database likely has lowercase
3. ❌ **broswerName** vs **BrowserName** - Node.js has typo, database likely has typo too

**Impact**: Frontend will send/expect lowercase fields. API responses will fail.

**Fix Required**: 
- Use `[Column("firstname")]` attribute to map FirstName → firstname
- Use `[Column("lastname")]` attribute to map LastName → lastname
- Use `[Column("broswerName")]` attribute to map BrowserName → broswerName (match the typo!)

---

### 2. SmtpSetup ⚠️ CRITICAL

**Node.js Model** (SmtpSetup.js):
```javascript
{
    host: STRING,
    port: INTEGER,
    isSecure: BOOLEAN,        // ⚠️ Node.js field name
    username: STRING,
    password: STRING,
    networkId: INTEGER
    // NO fromEmail field
    // NO fromName field
    // NO statusId field
}
```

**.NET Entity** (SmtpSetup.cs):
```csharp
{
    Host: string,
    Port: int,
    EnableSsl: bool,          // ⚠️ WRONG NAME - should be "isSecure"
    Username: string,
    Password: string,
    FromEmail: string,        // ⚠️ EXTRA FIELD - not in Node.js
    FromName: string,         // ⚠️ EXTRA FIELD - not in Node.js
    NetworkId: int,
    StatusId: int             // ⚠️ EXTRA FIELD - not in Node.js
}
```

**Issues**:
1. ❌ **isSecure** vs **EnableSsl** - Frontend sends "isSecure", .NET expects "EnableSsl"
2. ❌ **.NET has 3 extra fields** not in Node.js database: FromEmail, FromName, StatusId
3. ❌ **Database table likely has "isSecure" column**, not "EnableSsl"

**Impact**: 
- POST /smtp-setup will fail - frontend sends `isSecure`, .NET ignores it
- GET /smtp-setup will return null for `isSecure`, frontend breaks
- Migration will try to add columns that don't exist in production DB

**Fix Required**:
- Rename `EnableSsl` → `IsSecure` 
- Remove `FromEmail`, `FromName`, `StatusId` OR make them nullable and optional
- Use `[Column("isSecure")]` attribute if database column name differs

---

## 📊 Summary Statistics

- **Total Entities Compared**: 13
- **Perfect Matches**: 11 ✅
- **Mismatches Found**: 2 ⚠️
  - CampaignReport: 3 field name issues (minor - can use Column attributes)
  - SmtpSetup: 4 field issues (CRITICAL - will break API)

---

## 🔧 REQUIRED FIXES

### Priority 1: SmtpSetup (CRITICAL - BREAKS API)

**Current .NET Entity**:
```csharp
public class SmtpSetup : BaseEntity
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; }      // ❌ WRONG
    public string? FromEmail { get; set; }    // ❌ EXTRA
    public string? FromName { get; set; }     // ❌ EXTRA
    public int? NetworkId { get; set; }
    public int StatusId { get; set; }         // ❌ EXTRA
}
```

**Required Fix**:
```csharp
public class SmtpSetup : BaseEntity
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    
    [Column("isSecure")]
    public bool IsSecure { get; set; }        // ✅ FIXED
    
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int? NetworkId { get; set; }
    
    // Navigation properties
    public Network? Network { get; set; }
}
```

**Remove**: EnableSsl, FromEmail, FromName, StatusId

---

### Priority 2: CampaignReport (MINOR - Column Mapping)

**Current .NET Entity**:
```csharp
public class CampaignReport : BaseEntity
{
    public string? ClickTime { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }    // ❌ Should map to "firstname"
    public string? LastName { get; set; }     // ❌ Should map to "lastname"
    public string? Campaign { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string? OsName { get; set; }
    public string? DeviceType { get; set; }
    public string? BrowserName { get; set; }  // ❌ Should map to "broswerName"
    public int? NetworkId { get; set; }
}
```

**Required Fix** (Option 1 - Column Attributes):
```csharp
public class CampaignReport : BaseEntity
{
    public string? ClickTime { get; set; }
    public string? Email { get; set; }
    
    [Column("firstname")]
    public string? FirstName { get; set; }    // ✅ Maps to lowercase
    
    [Column("lastname")]
    public string? LastName { get; set; }     // ✅ Maps to lowercase
    
    public string? Campaign { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string? OsName { get; set; }
    public string? DeviceType { get; set; }
    
    [Column("broswerName")]
    public string? BrowserName { get; set; }  // ✅ Maps to typo column
    
    public int? NetworkId { get; set; }
}
```

**Required Fix** (Option 2 - Match Typo Exactly):
```csharp
public class CampaignReport : BaseEntity
{
    public string? ClickTime { get; set; }
    public string? Email { get; set; }
    public string? Firstname { get; set; }    // ✅ Lowercase
    public string? Lastname { get; set; }     // ✅ Lowercase
    public string? Campaign { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? ClickIP { get; set; }
    public string? Country { get; set; }
    public string? OsName { get; set; }
    public string? DeviceType { get; set; }
    public string? BroswerName { get; set; }  // ✅ Match typo
    public int? NetworkId { get; set; }
}
```

---

## 🎯 RECOMMENDED ACTION PLAN

### Step 1: Fix SmtpSetup Entity (CRITICAL)
1. Rename `EnableSsl` to `IsSecure`
2. Add `[Column("isSecure")]` attribute
3. Remove `FromEmail`, `FromName`, `StatusId` fields
4. Update SmtpSetupService to use `IsSecure` instead of `EnableSsl`
5. Update SmtpSetupDto to use `IsSecure`

### Step 2: Fix CampaignReport Entity
**Option A**: Add Column attributes (RECOMMENDED - keeps C# naming conventions)
```csharp
[Column("firstname")] public string? FirstName
[Column("lastname")] public string? LastName
[Column("broswerName")] public string? BrowserName
```

**Option B**: Match exact casing (simpler but breaks C# conventions)
```csharp
public string? Firstname
public string? Lastname
public string? BroswerName
```

### Step 3: Generate Migration
```bash
cd src/MyPortal.Infrastructure
dotnet ef migrations add FixEntityMismatches --startup-project ../MyPortal.Api
```

### Step 4: Review Migration
- Verify migration only modifies SmtpSetup table
- CampaignReport changes should be metadata-only (no DB changes if using Column attributes)

### Step 5: Update Services
- SmtpSetupService: Change all `EnableSsl` references to `IsSecure`
- SmtpSetupDto: Update field names

### Step 6: Test
- Test POST /smtp-setup with `isSecure: true`
- Test GET /smtp-setup returns `isSecure` field
- Test CampaignReport API returns `firstname`, `lastname`, `broswerName`

---

## 🚨 CRITICAL WARNING

**DO NOT run `dotnet ef database update` yet!**

The current SmtpSetup entity has 4 fields not in the database:
- EnableSsl (should be isSecure)
- FromEmail
- FromName  
- StatusId

Running migration before fixing will try to:
1. ADD FromEmail column (breaks production)
2. ADD FromName column (breaks production)
3. ADD StatusId column (breaks production)
4. ADD EnableSsl column (should be renaming isSecure)

**Fix entities FIRST, then generate migration.**

---

## ✅ NEXT STEPS

1. ✅ Fix SmtpSetup.cs entity
2. ✅ Fix CampaignReport.cs entity
3. ✅ Update SmtpSetupService
4. ✅ Update SmtpSetupDto
5. ✅ Generate migration
6. ✅ Review migration SQL
7. ✅ Apply migration to development DB
8. ✅ Test all endpoints
9. ✅ Deploy to production

---

**Document Version**: 1.0  
**Last Updated**: November 7, 2025  
**Status**: ⚠️ CRITICAL FIXES REQUIRED BEFORE DEPLOYMENT
