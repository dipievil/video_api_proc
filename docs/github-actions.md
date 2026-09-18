# GitHub Actions Workflows

This repository includes automated quality pipelines using GitHub Actions that run on self-hosted runners.

## Workflows Overview

### 1. Build Application (`build.yml`)

**Triggers:**
- All pushes to `main` and `develop` branches
- All pull requests targeting `main` and `develop` branches

**Purpose:**
- Validates that the application builds successfully
- Restores dependencies and builds in Release configuration
- Uploads build artifacts for debugging if needed

**Steps:**
1. Checkout code
2. Setup .NET 8.0 SDK
3. Restore NuGet packages
4. Build application in Release mode
5. Upload build artifacts (retention: 1 day)

### 2. Unit Tests (`unit-tests.yml`)

**Triggers:**
- Pull requests targeting `main` and `develop` branches

**Purpose:**
- Runs all unit tests in the `VideoProcessingApi.UnitTests` project
- Ensures code changes don't break existing functionality
- Generates test reports for review

**Steps:**
1. Checkout code
2. Setup .NET 8.0 SDK
3. Restore dependencies
4. Build application
5. Run unit tests with detailed logging
6. Upload test results (retention: 7 days)

### 3. Integration Tests (`integration-tests.yml`)

**Triggers:**
- Pull requests targeting `main` and `develop` branches

**Purpose:**
- Runs full integration tests with Docker services
- Tests API endpoints and database interactions
- Validates the complete application stack

**Steps:**
1. Checkout code
2. Setup .NET 8.0 SDK
3. Restore dependencies
4. Build application
5. Start Docker services for testing
6. Run integration tests
7. Stop Docker services (cleanup)
8. Upload test results (retention: 7 days)

## Self-Hosted Runner Requirements

All workflows are configured to run on `self-hosted` runners. Ensure your runner has:

- .NET 8.0 SDK
- Docker and Docker Compose
- Sufficient storage for build artifacts
- Network access to restore NuGet packages

## Test Results

Test results are uploaded as artifacts and can be downloaded from the Actions tab:
- Unit test results: Available for 7 days
- Integration test results: Available for 7 days
- Build artifacts: Available for 1 day

## Monitoring

Check the Actions tab in your GitHub repository to monitor:
- Build status for all commits
- Test results for pull requests
- Performance trends and failure patterns