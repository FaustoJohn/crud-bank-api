# VS Code Launch and Debug Configuration

This document explains the VS Code launch and debug configurations for the CRUD Bank API project.

## Available Launch Configurations

### 1. **Launch API (Development)**
- **Type**: `dotnet`
- **Purpose**: Launches the API in development mode
- **Pre-launch Task**: Builds the API project
- **Usage**: Use this for debugging the main API application

### 2. **Launch API (Watch Mode)**
- **Type**: `dotnet`
- **Purpose**: Launches the API with hot reload (watch mode)
- **Pre-launch Task**: Starts the watch task
- **Usage**: Use this for development with automatic recompilation on file changes

### 3. **Run All Tests**
- **Type**: `dotnet`
- **Purpose**: Runs all tests in the test project
- **Pre-launch Task**: Builds the test project
- **Usage**: Quick test execution without detailed console output

### 4. **Debug Tests (Console Output)**
- **Type**: `coreclr`
- **Purpose**: Runs tests with detailed console output
- **Features**: 
  - Integrated terminal output
  - Detailed logging
  - Breakpoint support in test code
- **Usage**: Use this when you need to debug test execution

### 5. **Debug SpecFlow Tests**
- **Type**: `coreclr`
- **Purpose**: Runs only SpecFlow tests (filtered by Category=SpecFlow)
- **Features**:
  - Runs feature file scenarios
  - Detailed console output
  - Breakpoint support in step definitions
- **Usage**: Use this to debug BDD scenarios

### 6. **Debug Single Test**
- **Type**: `coreclr`
- **Purpose**: Runs a specific test based on name pattern
- **Features**:
  - Prompts for test name pattern
  - Supports wildcards (e.g., "*UserTest*")
  - Detailed console output
- **Usage**: Use this to debug a specific test method

### 7. **Attach to Process**
- **Type**: `coreclr`
- **Purpose**: Attaches debugger to a running .NET process
- **Usage**: Use this to debug a running application instance

## How to Use

### Debugging the API
1. Set breakpoints in your API code
2. Select "Launch API (Development)" from the debug dropdown
3. Press F5 or click the play button
4. The API will start and automatically open in your browser

### Debugging Tests
1. Set breakpoints in your test code or step definitions
2. Select the appropriate test configuration:
   - "Debug Tests (Console Output)" for all tests
   - "Debug SpecFlow Tests" for BDD scenarios only
   - "Debug Single Test" for a specific test
3. Press F5 or click the play button

### Debugging SpecFlow Tests
1. Open the feature file (`CrudBankApi.Tests/Features/UserManagement.feature`)
2. Set breakpoints in the step definitions (`CrudBankApi.Tests/StepDefinitions/UserManagementSteps.cs`)
3. Select "Debug SpecFlow Tests"
4. Press F5 to start debugging

## Available VS Code Tasks

The following tasks are available through the Command Palette (Ctrl+Shift+P > "Tasks: Run Task"):

- **build**: Build the entire solution
- **build-api**: Build only the API project
- **build-tests**: Build only the test project
- **clean**: Clean the solution
- **restore**: Restore NuGet packages
- **run-api**: Run the API without debugging
- **test-all**: Run all tests
- **test-specflow**: Run only SpecFlow tests
- **watch-api**: Run API in watch mode

## Test Structure

### Unit Tests
- Located in `CrudBankApi.Tests/Class1.cs`
- Basic NUnit tests for unit testing
- Tagged with `[Category("UnitTest")]`

### SpecFlow Tests
- Feature files: `CrudBankApi.Tests/Features/*.feature`
- Step definitions: `CrudBankApi.Tests/StepDefinitions/*Steps.cs`
- Tagged with `@SpecFlow` in feature files
- Uses NUnit as the test runner

## Recommended Extensions

Install these VS Code extensions for the best experience:

```
ms-dotnettools.csharp
ms-dotnettools.csdevkit
ms-dotnettools.vscodeintellicode-csharp
amillard98.specflow-tools
alexkrechik.cucumberautocomplete
```

## Troubleshooting

### If IntelliSense is not working:
1. Open Command Palette (Ctrl+Shift+P)
2. Run "C#: Restart Language Server"
3. Wait for the language server to initialize

### If tests are not discovered:
1. Run the "restore" task
2. Run the "build-tests" task
3. Check the Test Explorer panel

### If debugging doesn't work:
1. Ensure the project builds successfully
2. Check that breakpoints are set in the correct files
3. Verify the launch configuration is selected properly

## Environment Variables

The API uses the following environment variables:
- `ASPNETCORE_ENVIRONMENT`: Set to "Development" for debugging
- `ASPNETCORE_URLS`: Set to "http://localhost:5134" by default

## Port Configuration

- API runs on: `http://localhost:5134`
- Swagger UI available at: `http://localhost:5134/swagger`
