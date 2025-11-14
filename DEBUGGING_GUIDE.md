# VS Code Debugging Setup for .NET API

## Configuration Files Created

### 1. `.vscode/launch.json`
Provides debugging configurations for both .NET and Node.js projects.

### 2. `.vscode/tasks.json`
Provides build tasks for the .NET project.

---

## How to Debug the .NET API

### Option 1: Using VS Code UI
1. Open the **Run and Debug** view (Ctrl+Shift+D / Cmd+Shift+D)
2. Select "**.NET Core Launch (web)**" from the dropdown
3. Press **F5** or click the green play button
4. The API will build, launch, and automatically open Swagger at `http://localhost:5000/swagger`

### Option 2: Using Command Palette
1. Press **Ctrl+Shift+P** (or **Cmd+Shift+P** on Mac)
2. Type "Debug: Select and Start Debugging"
3. Choose "**.NET Core Launch (web)**"

---

## Available Debug Configurations

### 1. `.NET Core Launch (web)` ✅ PRIMARY
- **Purpose**: Launch and debug the .NET API
- **Port**: Default port configured in `launchSettings.json`
- **Auto-opens**: Swagger UI at startup
- **Environment**: Development
- **Pre-build**: Automatically builds the project before launch

### 2. `.NET Core Attach`
- **Purpose**: Attach debugger to running .NET process
- **Use case**: Debug already running API instance

### 3. `Launch Node.js Server`
- **Purpose**: Debug the Node.js/TypeScript API
- **Use case**: Compare behavior with legacy API

---

## Available Tasks

Run tasks via **Terminal → Run Task** or **Ctrl+Shift+P** → "Tasks: Run Task"

### 1. `build`
Builds the .NET API project
```bash
dotnet build MyPortal.Api.csproj
```

### 2. `publish`
Publishes the .NET API for deployment
```bash
dotnet publish MyPortal.Api.csproj
```

### 3. `watch`
Runs the API in watch mode (auto-reload on file changes)
```bash
dotnet watch run --project MyPortal.Api.csproj
```

### 4. `build-node`
Builds the Node.js TypeScript project

---

## Breakpoint Debugging

### Setting Breakpoints
1. Open any `.cs` file (e.g., `NetworkController.cs`)
2. Click in the left gutter next to a line number
3. A red dot appears = breakpoint set
4. Press **F5** to start debugging
5. Make an API call that hits that endpoint
6. Debugger will pause at the breakpoint

### Debug Actions
- **F5**: Continue
- **F10**: Step Over
- **F11**: Step Into
- **Shift+F11**: Step Out
- **Ctrl+Shift+F5**: Restart
- **Shift+F5**: Stop

### Inspecting Variables
- Hover over variables to see values
- Use the **Variables** pane in Debug view
- Use **Watch** pane to track specific expressions
- Use **Debug Console** to execute code

---

## Testing the API

### 1. Start Debugging
```bash
Press F5
```

### 2. Swagger Opens Automatically
The browser will open: `http://localhost:5000/swagger`

### 3. Test Endpoints
- **POST /api/networks** - Create network
- **GET /api/networks** - Get all networks
- **POST /api/contacts/upload** - Bulk upload contacts
- **POST /api/emails/campaign** - Send campaign emails

### 4. Set Breakpoints
Set breakpoints in:
- `NetworkService.cs` → `CreateNetworkAsync()` method
- `ContactService.cs` → `UploadContactsAsync()` method
- `EmailService.cs` → `SaveCampaignEmailAsync()` method

### 5. Make API Call
- Use Swagger UI "Try it out" button
- Or use Postman/cURL
- Debugger will pause at breakpoints

---

## Common Issues & Solutions

### Issue: "Cannot find project"
**Solution**: Update paths in `launch.json` and `tasks.json` to match your project location

### Issue: "Port already in use"
**Solution**: 
1. Stop any running instances
2. Or change port in `Properties/launchSettings.json`

### Issue: Build fails
**Solution**: 
```bash
cd MyPortalCampaign/src/MyPortal.Api
dotnet restore
dotnet build
```

### Issue: Debugger doesn't stop at breakpoints
**Solution**:
1. Ensure you're running in Debug mode (not Release)
2. Rebuild the project
3. Clean solution: `dotnet clean`

---

## Environment Variables

Edit in `launch.json` under `env` section:

```json
"env": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "ConnectionStrings__DefaultConnection": "your-connection-string"
}
```

---

## Next Steps

1. ✅ Press **F5** to start debugging
2. ✅ Swagger UI opens automatically
3. ✅ Set breakpoints in service methods
4. ✅ Test all 6 fixed services:
   - NetworkService
   - ContactService
   - EmailService
   - GroupService
   - SecuritySetupService
   - UserProfileService
5. ✅ Compare responses with Node.js API

---

## Quick Reference

| Action | Shortcut |
|--------|----------|
| Start Debugging | F5 |
| Stop Debugging | Shift+F5 |
| Restart | Ctrl+Shift+F5 |
| Step Over | F10 |
| Step Into | F11 |
| Step Out | Shift+F11 |
| Toggle Breakpoint | F9 |
| Debug Console | Ctrl+Shift+Y |

---

**Status**: ✅ Ready to Debug
**Last Updated**: November 7, 2025
