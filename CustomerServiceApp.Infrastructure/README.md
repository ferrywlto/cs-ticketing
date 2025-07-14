# CustomerServiceApp.Infrastructure

## Configuration

### Password Hasher Configuration

The password hasher requires proper configuration for security. The following settings are required:

#### Development Setup

Use user secrets to configure the password hasher:

```bash
# Set the salt (minimum 32 characters required)
dotnet user-secrets set "PasswordHasher:Salt" "your-secure-salt-here" --project CustomerServiceApp.API

# Set iterations (optional, defaults to 100,000)
dotnet user-secrets set "PasswordHasher:Iterations" "100000" --project CustomerServiceApp.API
```

#### Production Setup

Configure via environment variables or appsettings:

```json
{
  "PasswordHasher": {
    "Salt": "REPLACE_WITH_SECURE_SALT_IN_PRODUCTION",
    "Iterations": 100000
  }
}
```

#### Configuration Validation

The system will validate configuration at startup and throw descriptive errors if:

- **Salt is missing**: Provides instructions for setting user secrets or environment variables
- **Salt is too short**: Requires minimum 32 characters for security
- **Iterations out of range**: Must be between 10,000 and 1,000,000

#### Error Examples

```
PasswordHasher:Salt is required. For development, set it using: dotnet user-secrets set "PasswordHasher:Salt" "your-secure-salt-here" For production, set it via environment variables or appsettings.json
```

```
PasswordHasher:Salt must be at least 32 characters long for security.
```

```
PasswordHasher:Iterations must be between 10,000 and 1,000,000.
```

## Security Features

- **PBKDF2 with SHA-256**: Industry-standard secure password hashing
- **Configurable salt**: Prevents rainbow table attacks
- **Configurable iterations**: Balance between security and performance
- **Comprehensive validation**: Clear error messages for configuration issues
- **User secrets integration**: Secure development configuration
