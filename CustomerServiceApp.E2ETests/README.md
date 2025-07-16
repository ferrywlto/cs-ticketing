# End-to-End Tests

This directory contains Selenium-based End-to-End (E2E) tests for the Customer Service Ticketing System.

## Overview

The E2E tests validate the user interface and user workflows by automating a real browser to interact with the application. These tests provide confidence that the entire application stack works correctly from a user's perspective.

## Test Structure

### Test Classes
- `PlayerLoginE2ETests.cs` - Comprehensive E2E tests for player login functionality
- `PlayerLoginE2ETestsWithPageObject.cs` - Simplified E2E tests using Page Object Model pattern

### Infrastructure
- `SeleniumTestFixture.cs` - Full test fixture that can start applications automatically
- `SimpleSeleniumTestFixture.cs` - Simplified fixture that expects applications to be running
- `PlayerLoginPage.cs` - Page Object Model for the Player Login page

## Prerequisites

### 1. Chrome Browser
The tests use Chrome WebDriver, so you need Google Chrome installed on your system.

### 2. ChromeDriver
The `Selenium.WebDriver.ChromeDriver` NuGet package automatically provides the ChromeDriver executable.

### 3. Running Applications
The tests expect the application to be running. You have several options:

#### Option A: Using Aspire (Recommended)
```bash
dotnet run --project CustomerServiceApp.AppHost
```
This starts both the API and Web applications with the correct URLs.

#### Option B: Manual Start
Start both projects separately:
```bash
# Terminal 1 - API
dotnet run --project CustomerServiceApp.API --launch-profile https

# Terminal 2 - Web  
dotnet run --project CustomerServiceApp.Web --launch-profile https
```

#### Option C: Using the Full Test Fixture
The `SeleniumTestFixture` can attempt to start applications automatically, but this approach is more complex and less reliable.

## Running the Tests

### From Command Line
```bash
# Run all E2E tests
dotnet test CustomerServiceApp.E2ETests

# Run specific test class
dotnet test CustomerServiceApp.E2ETests --filter "FullyQualifiedName~PlayerLoginE2ETestsWithPageObject"

# Run with verbose output
dotnet test CustomerServiceApp.E2ETests --logger "console;verbosity=detailed"
```

### From Visual Studio / VS Code
1. Ensure applications are running (see Prerequisites)
2. Open Test Explorer
3. Run the E2E tests

## Test Configuration

### Browser Mode
By default, tests run in visible browser mode for debugging. To run in headless mode (faster, suitable for CI/CD):

1. Open `SimpleSeleniumTestFixture.cs`
2. Uncomment the headless option:
```csharp
options.AddArguments("--headless");
```

### URLs
The tests are configured to use these URLs:
- Web Application: `https://localhost:7145`
- API Application: `https://localhost:7234`

If your applications run on different ports, update the URLs in the test fixtures.

## Test Scenarios Covered

### Player Login E2E Tests
1. **Form Display** - Verifies login form renders correctly
2. **Field Validation** - Tests required field validation
3. **Email Validation** - Tests email format validation  
4. **Password Validation** - Tests password length validation
5. **Loading States** - Verifies loading indicators during submission
6. **Successful Login** - Tests redirect to player tickets on valid credentials
7. **Failed Login** - Tests error message display on invalid credentials
8. **Demo Credentials** - Verifies demo user information is displayed
9. **Page Structure** - Tests proper HTML structure and accessibility

## Page Object Model

The Page Object Model (POM) pattern is used to improve test maintainability:

### Benefits
- **Reusability** - Page objects can be reused across multiple tests
- **Maintainability** - UI changes only require updates in one place
- **Readability** - Tests focus on business logic rather than UI details
- **Encapsulation** - Page-specific logic is contained within page objects

### Usage Example
```csharp
[Fact]
public void PlayerLogin_ShouldWork()
{
    // Arrange
    _loginPage.NavigateToPage();

    // Act
    _loginPage.LoginWithValidPlayerCredentials();

    // Assert
    _loginPage.WaitForRedirectToPlayerTickets();
}
```

## Troubleshooting

### Common Issues

#### ChromeDriver Version Mismatch
If you get ChromeDriver compatibility errors:
1. Update Chrome browser to the latest version
2. Update the `Selenium.WebDriver.ChromeDriver` NuGet package

#### Application Not Running
If tests fail with connection errors:
1. Verify applications are running on the expected URLs
2. Check that HTTPS certificates are trusted
3. Ensure no firewall is blocking the connections

#### Timing Issues
If tests are flaky due to timing:
1. Increase wait timeouts in the test fixtures
2. Add explicit waits for specific elements
3. Use the `WaitForPageLoad()` method before interactions

#### SSL Certificate Errors
The test fixtures include arguments to ignore SSL certificate errors, but if you still encounter issues:
1. Trust the development certificates: `dotnet dev-certs https --trust`
2. Verify the applications are accessible in a regular browser

### Debug Mode
To debug tests visually:
1. Comment out the `--headless` argument in the ChromeOptions
2. Add breakpoints in your test code
3. Watch the browser automation in real-time

## Best Practices

### Test Design
1. **Independent Tests** - Each test should be able to run independently
2. **Clear Assertions** - Use descriptive assertion messages
3. **Page Object Model** - Use POM for better maintainability
4. **Explicit Waits** - Always wait for elements rather than using Thread.Sleep

### Performance
1. **Headless Mode** - Use headless mode for faster execution in CI/CD
2. **Shared Fixtures** - Use class fixtures to share browser instances
3. **Minimal Data** - Use minimal test data for faster execution

### Reliability
1. **Explicit Waits** - Wait for specific conditions rather than fixed time delays
2. **Error Handling** - Handle common exceptions like StaleElementReferenceException
3. **Cleanup** - Ensure proper browser cleanup in fixture disposal

## Future Enhancements

Potential improvements for the E2E test suite:

1. **Additional Pages** - Create page objects for Agent Login, Player Tickets, Agent Tickets
2. **Full User Workflows** - Test complete user journeys (login → create ticket → reply → resolve)
3. **Cross-Browser Testing** - Add support for Firefox, Edge, Safari
4. **Parallel Execution** - Configure tests to run in parallel for faster feedback
5. **Visual Testing** - Add screenshot comparison testing
6. **API Mocking** - Mock API responses for more controlled testing scenarios
7. **Mobile Testing** - Add mobile browser testing with responsive design validation
