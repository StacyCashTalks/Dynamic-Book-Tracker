# Azure Function Workflows

This repository contains GitHub Actions workflows for provisioning and deploying Azure Functions.

## Workflows

### 1. Provision Azure Function (`provision-azure-function.yml`)

This workflow creates the necessary Azure infrastructure for the Function App.

**Trigger:** Manual (`workflow_dispatch`)

**Required Secrets:**
- `AZURE_CLIENT_ID` - Azure service principal client ID
- `AZURE_DIRECTORY_ID` - Azure tenant/directory ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID
- `AZURE_RESOURCE_GROUP` - Base name of the Azure resource group (environment suffix will be added)
- `AZURE_FUNCTION_APP_NAME` - Base name of the Azure Function App (environment suffix will be added)
- `AZURE_LOCATION` - Azure region (e.g., `eastus`, `westeurope`)
- `AZURE_STORAGE_ACCOUNT_NAME` - Base name of the storage account (environment suffix will be added)

**What it does:**
- Creates an Azure Resource Group with environment suffix (e.g., `rg-booktracker-dev`)
- Creates a Storage Account with environment suffix (e.g., `stbooktrackerdev`)
- Creates an Azure Function App with environment suffix (e.g., `func-booktracker-api-dev`)
- Configures the Function App with:
  - Consumption plan
  - .NET 8 runtime
  - Functions v4
  - Linux OS
- Includes idempotency checks to allow re-running without errors

**How to run:**
1. Go to Actions tab
2. Select "Provision Azure Function"
3. Click "Run workflow"
4. Select the environment (dev/staging/production)

**Note:** Resource names will automatically include the environment suffix, enabling multi-environment deployments.

### 2. Deploy Azure Function (`deploy-azure-function.yml`)

This workflow builds and deploys the Azure Function application.

**Triggers:**
- Push to `main` branch (only when files in `Api/`, `Shared/`, or the workflow file change) - deploys to production
- Pull request to `main` branch (builds only, no deployment)
- Manual (`workflow_dispatch`) - allows selecting target environment

**Required Secrets:**
- `AZURE_CLIENT_ID` - Azure service principal client ID
- `AZURE_DIRECTORY_ID` - Azure tenant/directory ID
- `AZURE_SUBSCRIPTION_ID` - Azure subscription ID
- `AZURE_FUNCTION_APP_NAME` - Base name of the Azure Function App (environment suffix will be added)

**What it does:**
- Builds the .NET Function App from the `Api` folder
- Publishes build artifacts (retained for 7 days)
- Deploys to Azure Function App (only on push to main or manual trigger)
- Supports multi-environment deployment via environment selection

**How to deploy to specific environment:**
1. Go to Actions tab
2. Select "Deploy Azure Function"
3. Click "Run workflow"
4. Select the target environment (dev/staging/production)

## Setup Instructions

### 1. Create Azure Service Principal and Configure OIDC

```bash
# Create the service principal
az ad sp create-for-rbac --name "github-actions-sp" --role contributor \
    --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group-name}

# Note the appId (client ID), tenant (directory ID) from the output
# Also note your subscription ID
```

### 2. Configure GitHub Secrets

Go to your repository Settings → Secrets and variables → Actions, and add:

1. **AZURE_CLIENT_ID**: The appId from the service principal creation
2. **AZURE_DIRECTORY_ID**: The tenant ID from the service principal
3. **AZURE_SUBSCRIPTION_ID**: Your Azure subscription ID
4. **AZURE_RESOURCE_GROUP**: Base name for your resource group (e.g., `rg-booktracker`)
5. **AZURE_FUNCTION_APP_NAME**: Base name for your function app (e.g., `func-booktracker-api`)
6. **AZURE_LOCATION**: Azure region (e.g., `eastus`)
7. **AZURE_STORAGE_ACCOUNT_NAME**: Base name for storage account (e.g., `stbooktracker`)

**Note:** The workflows will automatically append environment suffixes to these base names (e.g., `rg-booktracker-dev`, `func-booktracker-api-production`).

### 3. First-time Setup

1. Run the "Provision Azure Function" workflow first to create the infrastructure
2. Once provisioning is complete, push changes to trigger deployment

## Project Structure

The workflows expect the following folder structure:

```
Dynamic-Book-Tracker/
├── Api/              # Azure Function project
│   └── Api.csproj
├── Shared/           # Shared code library
└── .github/
    └── workflows/
        ├── provision-azure-function.yml
        └── deploy-azure-function.yml
```

## Notes

- The deployment workflow deploys to production by default on push to `main` branch
- Pull requests will build but not deploy
- The provisioning workflow can be run multiple times (includes idempotency checks)
- Storage account names must be globally unique and contain only lowercase letters and numbers
- Both workflows support multi-environment deployments via environment suffixes
- Uses OIDC authentication for enhanced security
