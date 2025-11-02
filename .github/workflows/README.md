# Azure Function Workflows

This repository contains GitHub Actions workflows for provisioning and deploying Azure Functions.

## Workflows

### 1. Provision Azure Function (`provision-azure-function.yml`)

This workflow creates the necessary Azure infrastructure for the Function App.

**Trigger:** Manual (`workflow_dispatch`)

**Required Secrets:**
- `AZURE_CREDENTIALS` - Azure service principal credentials in JSON format
- `AZURE_RESOURCE_GROUP` - Name of the Azure resource group
- `AZURE_FUNCTION_APP_NAME` - Name of the Azure Function App
- `AZURE_LOCATION` - Azure region (e.g., `eastus`, `westeurope`)
- `AZURE_STORAGE_ACCOUNT_NAME` - Name of the storage account (must be globally unique, lowercase, no special characters)

**What it does:**
- Creates an Azure Resource Group
- Creates a Storage Account
- Creates an Azure Function App with:
  - Consumption plan
  - .NET 8 runtime
  - Functions v4
  - Linux OS

**How to run:**
1. Go to Actions tab
2. Select "Provision Azure Function"
3. Click "Run workflow"
4. Select the environment (dev/staging/production)

### 2. Deploy Azure Function (`deploy-azure-function.yml`)

This workflow builds and deploys the Azure Function application.

**Triggers:**
- Push to `main` branch (only when files in `Api/`, `Shared/`, or the workflow file change)
- Pull request to `main` branch (builds only, no deployment)
- Manual (`workflow_dispatch`)

**Required Secrets:**
- `AZURE_CREDENTIALS` - Azure service principal credentials
- `AZURE_FUNCTION_APP_NAME` - Name of the Azure Function App

**What it does:**
- Builds the .NET Function App from the `Api` folder
- Publishes build artifacts
- Deploys to Azure Function App (only on push to main or manual trigger)

## Setup Instructions

### 1. Create Azure Service Principal

```bash
az ad sp create-for-rbac --name "github-actions-sp" --role contributor \
    --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group-name} \
    --sdk-auth
```

Copy the JSON output and save it as the `AZURE_CREDENTIALS` secret.

### 2. Configure GitHub Secrets

Go to your repository Settings → Secrets and variables → Actions, and add:

1. **AZURE_CREDENTIALS**: The JSON output from the service principal creation
2. **AZURE_RESOURCE_GROUP**: Your resource group name (e.g., `rg-booktracker-prod`)
3. **AZURE_FUNCTION_APP_NAME**: Your function app name (e.g., `func-booktracker-api`)
4. **AZURE_LOCATION**: Azure region (e.g., `eastus`)
5. **AZURE_STORAGE_ACCOUNT_NAME**: Storage account name (e.g., `stbooktrackerprod`)

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

- The deployment workflow only runs on the `main` branch
- Pull requests will build but not deploy
- The provisioning workflow can be run multiple times (it will update existing resources)
- Storage account names must be globally unique and contain only lowercase letters and numbers
