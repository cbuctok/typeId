# GitHub Actions Workflows

This directory contains automated CI/CD workflows for the TypeId project.

## Workflows

### 1. CI - Build and Test (`ci.yml`)
**Triggers:** Push to main/master/develop/claude/** branches, pull requests, manual dispatch

**Purpose:** Main continuous integration pipeline that builds and tests the project.

**Features:**
- Tests on multiple .NET versions (6.0.x, 8.0.x)
- Builds in both Debug and Release configurations
- Runs all unit tests including performance tests
- Validates code formatting
- Packages NuGet artifacts
- Provides comprehensive CI status checks

**Jobs:**
- `build-and-test`: Compiles and tests the solution
- `validation`: Checks code format and creates NuGet package
- `status-check`: Final validation of all CI steps

### 2. Performance Tests (`performance.yml`)
**Triggers:** Push to main/master, pull requests, scheduled (Mondays 9 AM UTC), manual dispatch

**Purpose:** Dedicated performance testing and benchmarking.

**Features:**
- Runs performance-critical tests
- Measures TypeID generation speed (target: < 0.003ms per ID)
- Tests encode/decode performance
- Generates performance reports in job summaries
- Scheduled weekly runs to track performance over time

**Target Metrics:**
- TypeID generation: < 0.003ms per operation
- 2.1M+ TypeIDs generated in performance test

### 3. Pull Request Validation (`pr-validation.yml`)
**Triggers:** PR opened, synchronized, reopened, or marked ready for review

**Purpose:** Comprehensive validation of pull requests before merge.

**Features:**
- Runs only on non-draft PRs
- Executes all test suites (spec tests, general tests)
- Collects code coverage metrics
- Validates API compatibility
- Provides detailed test results in PR summary
- Separate coverage reporting job

**Jobs:**
- `validate-pr`: Main validation pipeline
- `test-coverage`: Generates code coverage reports

## Status Badges

Add these badges to your README.md:

```markdown
[![CI](https://github.com/cbuctok/typeId/actions/workflows/ci.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/ci.yml)
[![Performance Tests](https://github.com/cbuctok/typeId/actions/workflows/performance.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/performance.yml)
[![PR Validation](https://github.com/cbuctok/typeId/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/cbuctok/typeId/actions/workflows/pr-validation.yml)
```

## Workflow Requirements

All workflows require:
- Ubuntu latest runner
- .NET SDK 6.0.x and/or 8.0.x
- TypeId.sln solution file
- TypeIdTests project with MSTest framework

## Test Organization

The workflows expect tests to be organized as:
- `TypeIdGeneralTests`: General functionality tests
- `SpecTests`: Specification compliance tests
- `PerformanceTest`: Performance benchmark tests

## Development Guidelines

### Running Workflows Locally

To simulate the CI pipeline locally:

```bash
# Restore dependencies
dotnet restore TypeId.sln

# Build both configurations
dotnet build TypeId.sln --configuration Debug
dotnet build TypeId.sln --configuration Release

# Run all tests
dotnet test TypeId.sln --configuration Release --verbosity normal

# Run specific test categories
dotnet test --filter "FullyQualifiedName~PerformanceTest"
dotnet test --filter "FullyQualifiedName~SpecTests"
```

### Best Practices

1. **Before committing:** Ensure all tests pass locally
2. **Performance tests:** Should consistently meet the < 0.003ms target
3. **Breaking changes:** Update API compatibility checks if making breaking changes
4. **Draft PRs:** Use draft status to prevent running full validation until ready

## Maintenance

### Updating .NET Versions

To add/update .NET versions, modify the matrix in `ci.yml`:

```yaml
strategy:
  matrix:
    dotnet-version: ['6.0.x', '8.0.x', '9.0.x']  # Add new versions here
```

### Modifying Performance Targets

Update target metrics in `performance.yml`:

```yaml
echo "**Target:** < 0.003ms per TypeID generation" >> $GITHUB_STEP_SUMMARY
```

## Troubleshooting

### Workflow not triggering
- Check branch name matches patterns in workflow triggers
- Ensure workflows are enabled in repository settings

### Tests failing in CI but passing locally
- Verify .NET version matches CI environment
- Check for timing-sensitive tests in performance suite
- Review test output in Actions logs

### Performance degradation
- Review recent commits affecting hot paths
- Check if new allocations were introduced
- Run performance tests locally with profiler
