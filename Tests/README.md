# Test Instructions

## Running Tests

To run all tests:
```bash
dotnet test
```

## Running Tests with Coverage

To run tests with code coverage:
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=./coverage/
```

Or using coverlet directly:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Generate Coverage Report

After running tests with coverage, generate an HTML report:
```bash
# Install ReportGenerator tool if not already installed
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate report
reportgenerator -reports:"./coverage/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:Html
```

## View Coverage Report

Open `./coverage/report/index.html` in your browser to view the detailed coverage report.

## Test Organization

- **Models/** - Tests for model classes (DropdownItem, GridCell, PropertyGridItem, etc.)
- **Attributes/** - Tests for all attribute classes
- **Services/** - Tests for service implementations
- **Extensions/** - Tests for extension methods
- **Events/** - Tests for event classes
- **Integration/** - Integration tests for complete workflows

## Coverage Goals

- Target: >50% code coverage
- Focus areas: Core models, attributes, services, and public APIs
- Excluded: UI controls (require WinForms test framework), Native code, Generated code
