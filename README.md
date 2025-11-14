# MyPortal Campaign Management API (.NET)

## Overview

This is a complete .NET 8 conversion of the Node.js/TypeScript MyPortal Campaign Management System. The application manages affiliate marketing campaigns with sophisticated fraud detection and IP validation capabilities.

## Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
MyPortal.CampaignManagement/
├── src/
│   ├── MyPortal.Api              # ASP.NET Core Web API (Presentation Layer)
│   ├── MyPortal.Application      # Business Logic & Services
│   ├── MyPortal.Core             # Domain Entities & Interfaces
│   └── MyPortal.Infrastructure   # Data Access & External Services
└── MyPortal.CampaignManagement.sln
```

### Technology Stack

- **Framework**: .NET 8 / ASP.NET Core Web API
- **Database**: SQL Server (via Entity Framework Core)
- **Authentication**: JWT Bearer Tokens
- **Documentation**: Swagger/OpenAPI
- **Patterns**: Repository Pattern, Unit of Work, Dependency Injection

## Key Features

### ✅ Campaign Management
- Create, update, delete, and retrieve campaigns
- Unique promotion links for each campaign
- Separate links for approved vs rejected traffic

### ✅ Advanced IP Fraud Detection
The system validates every click using **IPData API** and rejects traffic from:
- ✋ VPN/Proxy users
- ✋ Tor network
- ✋ Datacenter IPs (bots)
- ✋ Anonymous proxies
- ✋ iCloud Relay
- ✋ Known attackers/abusers
- ✋ Low trust scores (< 50)
- ✋ High threat scores (> 50)

### ✅ Click Tracking & Reporting
- Records all campaign clicks with metadata
- Tracks: IP, country, browser, device, status, rejection reason
- Base64-encoded parameters for email/name tracking

### ✅ User Management
- JWT-based authentication
- Role-based access (Admin, Manager, User, Affiliate)
- User profiles with detailed information
- Multi-tenant network support

### ✅ Security
- Password hashing (SHA256)
- JWT token expiration
- CORS configuration
- SQL injection protection (EF Core parameterized queries)

## Database Configuration

The application connects to SQL Server using the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SQL8011.site4now.net;Initial Catalog=db_abeda1_securedb;User Id=db_abeda1_securedb_admin;Password=Passion@001;TrustServerCertificate=True"
  }
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (configured with provided connection string)
- IPData API Key (for IP validation)

### Setup Instructions

1. **Clone the repository**
   ```bash
   cd /Users/olu-johnsonoluwatosin/Documents/MyProject/dotnet/secureportal
   ```

2. **Configure API Keys**
   
   Update `appsettings.json` with your IPData API key:
   ```json
   {
     "IPDataApiKey": "YOUR_IPDATA_API_KEY_HERE"
   }
   ```

3. **Run Database Migrations**
   ```bash
   dotnet ef database update --project src/MyPortal.Infrastructure/MyPortal.Infrastructure.csproj --startup-project src/MyPortal.Api/MyPortal.Api.csproj
   ```

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run the API**
   ```bash
   cd src/MyPortal.Api
   dotnet run
   ```

6. **Access Swagger Documentation**
   
   Navigate to: `https://localhost:7XXX/swagger`

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/validate` - Validate JWT token

### Campaign Management
- `POST /api/campaigndetails` - Create campaign
- `GET /api/campaigndetails` - List all campaigns
- `GET /api/campaigndetails/{id}` - Get campaign by ID
- `PUT /api/campaigndetails/{id}` - Update campaign
- `DELETE /api/campaigndetails/{id}` - Delete campaign

### Campaign Tracking (Public Endpoint)
- `GET /cpg/{promotionLink}/{base64Params}` - Track click and redirect
  - **Parameters**: Base64 encoded `email-firstname-lastname`
  - Validates IP and redirects to appropriate URL

## Database Schema

### Core Tables
- **Users** - User accounts
- **UserProfiles** - Extended user information
- **UserTypes** - Role definitions
- **Networks** - Multi-tenant network support
- **Statuses** - Common status lookups
- **Tokens** - Authentication tokens

### Campaign Tables
- **CampaignDetails** - Campaign configuration
- **CampaignReports** - Click tracking records

### Contact Management
- **Contacts** - Contact list
- **Groups** - Contact groups
- **ContactGroups** - Many-to-many relationship

### Communication
- **Emails** - Email queue/history
- **SmtpSetups** - SMTP configurations
- **SecuritySetups** - Security settings

## Campaign Click Flow

1. User clicks promotion link: `/cpg/{promotionLink}/{base64Params}`
2. System decodes parameters (email, firstname, lastname)
3. Extracts client IP address (supports proxies/load balancers)
4. Validates IP using IPData API
5. Creates `CampaignReport` record with validation result
6. Redirects to:
   - **Approved**: Campaign's main link
   - **Rejected**: Campaign's rejected link
   - **Error**: Default fallback (Google)

## Default Credentials

After running migrations, the system creates a default admin account:

- **Email**: `admin@myportal.com`
- **Password**: `Admin@123`

**⚠️ IMPORTANT**: Change this password immediately in production!

## Development Tips

### Running in Development Mode
```bash
dotnet watch run --project src/MyPortal.Api/MyPortal.Api.csproj
```

### Adding New Migrations
```bash
dotnet ef migrations add MigrationName --project src/MyPortal.Infrastructure --startup-project src/MyPortal.Api
```

### Viewing Database
```bash
dotnet ef database update --project src/MyPortal.Infrastructure --startup-project src/MyPortal.Api
```

## Project Structure Details

### MyPortal.Core
- **Entities**: Domain models (User, Campaign, Contact, etc.)
- **Interfaces**: Repository and service contracts
- No external dependencies (pure domain layer)

### MyPortal.Application
- **Services**: Business logic implementation
- **DTOs**: Data transfer objects for API
- **Interfaces**: Service contracts
- Dependencies: Core layer only

### MyPortal.Infrastructure
- **Data**: DbContext and configurations
- **Repositories**: Data access implementation
- **Migrations**: EF Core migrations
- Dependencies: Core, Application

### MyPortal.Api
- **Controllers**: HTTP endpoints
- **Middleware**: Authentication, CORS, exception handling
- **Configuration**: appsettings.json, Program.cs
- Dependencies: All layers

## Key Differences from Node.js Version

| Feature | Node.js | .NET |
|---------|---------|------|
| ORM | Sequelize | Entity Framework Core |
| Auth | Manual JWT | ASP.NET Core Identity + JWT |
| DI | Manual | Built-in DI Container |
| Async | Promises | async/await (Task-based) |
| Type Safety | TypeScript | C# (compile-time) |
| Performance | Good | Excellent (compiled) |
| Deployment | Node process | IIS, Kestrel, Docker |

## Security Considerations

- ✅ JWT tokens expire after 60 minutes
- ✅ Passwords hashed with SHA256
- ✅ CORS configured for specific origins
- ✅ SQL injection prevented by EF Core
- ✅ HTTPS enforced in production
- ⚠️ Update JWT secret key for production
- ⚠️ Store sensitive data in Azure Key Vault or User Secrets

## Future Enhancements

- [ ] Email service implementation
- [ ] Contact management endpoints
- [ ] User management endpoints
- [ ] File upload handling
- [ ] Background jobs (Hangfire)
- [ ] Logging (Serilog)
- [ ] Unit tests
- [ ] Integration tests
- [ ] Docker containerization

## Troubleshooting

### Connection Issues
- Verify SQL Server is accessible
- Check firewall rules
- Confirm connection string credentials

### Migration Errors
- Ensure EF Core tools are installed: `dotnet tool install --global dotnet-ef`
- Check DbContext configuration

### JWT Token Issues
- Verify JWT settings in appsettings.json
- Ensure secret key is at least 32 characters

## License

This project is proprietary software for MyPortal Campaign Management System.

## Contact

For support or questions, contact the development team.

---

**Built with ❤️ using .NET 8 and Clean Architecture principles**
