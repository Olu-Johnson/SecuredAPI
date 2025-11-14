# Service Implementation Fixes - Node.js to .NET Migration

## Current Status: IN PROGRESS

### ✅ Completed Updates:

#### 1. ContactService ✅ 
**File**: `MyPortal.Application/Services/ContactService.cs`

**Changes Made:**
- ✅ Added email validation using Regex
- ✅ Added required field validation (firstname, lastname, email)
- ✅ Added proper error messages matching Node.js ("email is required", "firstname is required", etc.)
- ✅ Updated `CreateContactAsync` to check for duplicates by email+networkId+userId
- ✅ Updated `GetAllContactsAsync` to filter by networkId and userId
- ✅ **Major Fix**: Rewrote `UploadContactsAsync` to match Node.js logic:
  - Validates each contact (email format, required fields)
  - Detects duplicates before insertion
  - Bulk inserts only unique contacts
  - Optionally assigns contacts to a group (if GroupId provided)
  - Returns count of successfully inserted contacts

**DTO Changes:**
- ✅ Updated `UploadContactDto` to include `GroupId` property

---

### 🔄 Services Requiring Updates:

#### 2. EmailService (CRITICAL) ⚠️
**File**: `MyPortal.Application/Services/EmailService.cs`

**Current Issues:**
- ❌ `SaveOfferEmailAsync` - Too simple, missing:
  - Get UserProfile with included User/Network data
  - Get Group with included Contacts
  - Render email templates using EJS/Razor
  - Include network branding (logo, url, signup url)
  - Create email records with proper "from" address from SMTP setup
  
- ❌ `SaveCampaignEmailAsync` - Missing:
  - Get Campaign details
  - Get UserProfile, Network, SmtpSetup data
  - Render templates from `templates/{networkId}/{templateName}.ejs`
  - Create personalized campaign links: `/cpg/{promotionLink}/{Base64(email-firstname)}`
  - Use SMTP username as "from" email
  
- ❌ Missing actual SMTP email sending implementation

**Node.js Logic to Implement:**
```typescript
// SaveOfferEmail:
1. Get UserProfile with User (includes networkId)
2. Get Network details (logo, url, company, signup url)
3. Get Group with Contacts
4. For each contact:
   - Render template with data: {offers, note, email, firstname, networklogo, etc.}
   - Create Email record with statusId=7, retryCount=0
   - Use network company as "from" name

// SaveCampaignEmail:
1. Verify campaign exists
2. Get UserProfile + User + Network + SmtpSetup
3. Get Group with Contacts
4. For each contact:
   - Create personalized link: protocol://host/cpg/{promotionLink}/{Base64(email-firstname)}
   - Render template from templates/{networkId}/{templateName}.ejs
   - Create Email record with SMTP username as "from"
```

---

#### 3. NetworkService (CRITICAL) ⚠️
**File**: `MyPortal.Application/Services/NetworkService.cs`

**Current Issues:**
- ❌ `CreateNetworkAsync` is too simple. Node.js does ATOMIC multi-step creation:
  1. Validate ALL required fields (email, password, firstName, lastName, company, country, etc.)
  2. Check for duplicate email in Users table
  3. Check for duplicate company in Networks table
  4. Create Network record
  5. Create User record (with hashed password, link to network)
  6. Create UserProfile record (with user details)
  7. Return success only if ALL three succeed

**Current .NET** only creates Network. Missing User and UserProfile creation!

**Node.js Validation Required:**
- Email format validation
- Password matching (password == confirmPassword)
- Required fields: firstName, middleName, lastName, phoneNumber, company, country
- Optional fields: networkUrl, networkSignupUrl, state, city, postalCode

---

#### 4. GroupService ⚠️
**File**: `MyPortal.Application/Services/AdditionalServices.cs`

**Current Issues:**
- ❌ `CreateGroupAsync` - Missing:
  - Check for duplicate group name (groupName+networkId+userId)
  - Support creating group with initial contacts list
  - If `request.contact` provided, bulk create ContactGroup records
  
- ❌ `UpdateGroupAsync` - Missing:
  - Support updating group contacts (adding contacts to group)
  - Should handle two scenarios:
    1. Update group name only
    2. Update group contacts only (if groupName empty but contacts provided)

- ❌ `GetAllGroupsAsync` - Missing:
  - Should include related Contacts in response
  - Node.js uses: `include: [{ model: Contact, as: 'contacts' }]`

- ❌ `GetGroupByIdAsync` - Missing:
  - Should include ContactGroup with nested Contact data
  - Node.js: `include: [{ model: ContactGroup, as: 'contactgroup', include: [Contact] }]`

---

#### 5. SecuritySetupService ⚠️
**File**: `MyPortal.Application/Services/AdditionalServices.cs` (line 335+)

**Current Issues:**
- ❌ Storing data incorrectly. Node.js SecuritySetup has fields:
  - `parameters` (string)
  - `approvedPage` (string)
  - `rejectedPage` (string)
  - `isMonitor` (boolean)
  
- ❌ `.NET is using "Name" and "Value" fields incorrectly!**
  - Should map to actual SecuritySetup entity fields
  
- ❌ `GetAllSecuritySetupsAsync` - Wrong filter logic:
  - Node.js filters by: `where: { id: request.networkId }`
  - This looks wrong - should probably be: `where: { networkId: request.networkId }`

---

#### 6. UserProfileService ⚠️
**File**: Needs to be created/found in AdditionalServices.cs

**Current Issues:**
- ❌ `GetAllUserProfilesAsync` - Missing complex filtering:
  - If userTypeId is 1 or 2 (admin/super-admin): show ALL profiles
  - Otherwise: filter by networkId
  - Should include relations:
    - User (with email, userTypeId, networkId, etc.)
    - Guarantor[] (all guarantors)
    - Leave[] (all leaves)
    
- ❌ `GetUserProfileByIdAsync` - Missing:
  - Get by userId (not id)
  - Include User, Guarantor[], Leave[] relations
  - Build custom response with imageurl (if picture exists)

---

#### 7. SmtpSetupService ⚠️
**Status**: Need to verify implementation

**Potential Issues to Check:**
- Validation of SMTP settings
- Secure password storage
- networkId association

---

### 🛠️ Entity Schema Issues:

#### SecuritySetup Entity
Currently stores data in generic "Name"/"Value" fields, but should have:
```csharp
public string Parameters { get; set; }
public string ApprovedPage { get; set; }
public string RejectedPage { get; set; }
public bool IsMonitor { get; set; }
public int? NetworkId { get; set; }
public int StatusId { get; set; }
```

Need to check actual entity definition and possibly create a migration.

---

## Priority Order for Fixes:

1. **HIGH**: NetworkService.CreateNetworkAsync - Critical for onboarding
2. **HIGH**: Email Service - Critical for campaign functionality
3. **MEDIUM**: GroupService - Needed for contact management
4. **MEDIUM**: UserProfileService - Needed for user management
5. **MEDIUM**: SecuritySetupService - Needed for fraud detection configuration
6. **LOW**: SmtpSetupService - Verify only

---

## Testing Checklist:

After all fixes:
- [ ] Test Network creation (should create Network + User + UserProfile)
- [ ] Test Contact upload with group assignment
- [ ] Test SaveOfferEmail (email template rendering)
- [ ] Test SaveCampaignEmail (personalized campaign links)
- [ ] Test Group creation with contacts
- [ ] Test Group getall/getbyid (should include contacts)
- [ ] Test UserProfile getall with filtering by userType
- [ ] Test SecuritySetup with proper field mapping

---

## Next Steps:

1. ✅ ContactService - COMPLETED
2. ⏳ Fix NetworkService (HIGH PRIORITY)
3. ⏳ Fix EmailService (HIGH PRIORITY)
4. ⏳ Fix GroupService
5. ⏳ Fix UserProfileService
6. ⏳ Fix SecuritySetupService
7. ⏳ Test all endpoints
8. ⏳ Update documentation

