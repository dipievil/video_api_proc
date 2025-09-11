# Unit Tests

This project contains unit tests for the Video Processing API using xUnit, Moq, and FluentAssertions.

## Structure

```
VideoProcessingApi.UnitTests/
├── Services/
│   ├── FileServiceTests.cs
│   ├── JobServiceTests.cs
│   ├── FFmpegErrorHandlerServiceTests.cs
│   └── EnvironmentValidationServiceTests.cs
├── GlobalUsings.cs
└── VideoProcessingApi.UnitTests.csproj
```

## Running Tests

### Prerequisites
- .NET 8.0 SDK
- All project dependencies restored

### Commands

**Run all unit tests:**
```bash
dotnet test tests/VideoProcessingApi.UnitTests
```

**Run with detailed output:**
```bash
dotnet test tests/VideoProcessingApi.UnitTests --verbosity normal
```

**Run tests and generate coverage report:**
```bash
dotnet test tests/VideoProcessingApi.UnitTests --collect:"XPlat Code Coverage"
```

**Run specific test class:**
```bash
dotnet test tests/VideoProcessingApi.UnitTests --filter "ClassName=FileServiceTests"
```

## Test Coverage

The unit tests cover the following services:

### ✅ FileService
- File validation (type and size)
- Storage service integration
- Stream handling

### ✅ FFmpegErrorHandlerService  
- Error message mapping
- Known error pattern detection
- Case-insensitive error handling

### ✅ JobService
- Job creation and management
- Status tracking
- Cancellation handling
- Output file retrieval

### ⚠️ EnvironmentValidationService
- Basic validation structure
- Configuration setup
- Error handling (partial coverage)

## Testing Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking library for dependencies  
- **FluentAssertions**: Assertion library for readable tests
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database for testing

## Best Practices

1. **Arrange, Act, Assert**: All tests follow the AAA pattern
2. **Mocking**: External dependencies are mocked to isolate unit under test
3. **Descriptive Names**: Test method names clearly describe what is being tested
4. **Independent Tests**: Each test can run independently
5. **Theory Tests**: Parameterized tests for multiple scenarios

## Notes

- Tests use in-memory databases to avoid dependencies on external systems
- Some environment validation tests may require additional setup for full coverage
- All tests are isolated and do not affect the actual file system or databases