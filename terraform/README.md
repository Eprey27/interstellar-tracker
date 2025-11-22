# Terraform Infrastructure for Interstellar Tracker

This directory contains Terraform configurations for deploying Azure infrastructure for the Interstellar Tracker project.

## Structure

```
terraform/
├── modules/                    # Reusable Terraform modules
│   ├── monitoring/            # Application Insights + Log Analytics
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   └── security/              # Key Vault for secrets
│       ├── main.tf
│       ├── variables.tf
│       └── outputs.tf
└── environments/              # Environment-specific configurations
    ├── dev/                  # Development environment
    │   ├── main.tf
    │   ├── variables.tf
    │   ├── outputs.tf
    │   └── terraform.tfvars.example
    └── prod/                 # Production environment (future)
```

## Prerequisites

1. **Terraform** >= 1.9.0

   ```powershell
   winget install Hashicorp.Terraform
   terraform --version
   ```

2. **Azure CLI**

   ```powershell
   winget install Microsoft.AzureCLI
   az --version
   ```

3. **Azure Subscription**
   - Subscription ID: `9fff6f2c-c722-4906-bb36-19bcd059d6d6`

## Initial Setup

### 1. Login to Azure

```powershell
az login
az account set --subscription 9fff6f2c-c722-4906-bb36-19bcd059d6d6
az account show
```

### 2. Create Backend Storage (One-time setup)

Terraform state must be stored in Azure Storage for team collaboration:

```powershell
# Variables
$RESOURCE_GROUP = "interstellar-tracker-tfstate-rg"
$LOCATION = "westeurope"
$STORAGE_ACCOUNT = "interstellartfstate"  # Must be globally unique
$CONTAINER_NAME = "tfstate"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create storage account
az storage account create `
  --resource-group $RESOURCE_GROUP `
  --name $STORAGE_ACCOUNT `
  --sku Standard_LRS `
  --encryption-services blob `
  --location $LOCATION

# Create blob container
az storage container create `
  --name $CONTAINER_NAME `
  --account-name $STORAGE_ACCOUNT
```

### 3. Configure Terraform Variables

```powershell
cd terraform/environments/dev

# Copy example file
cp terraform.tfvars.example terraform.tfvars

# Edit with your values
notepad terraform.tfvars
```

**Update these values in `terraform.tfvars`:**

```hcl
subscription_id = "9fff6f2c-c722-4906-bb36-19bcd059d6d6"
owner_email     = "your-email@example.com"
alert_email     = "your-email@example.com"

# Optional: Add your IP for Key Vault access
# key_vault_allowed_ips = ["YOUR.IP.ADDRESS.HERE"]
```

**IMPORTANT:** Never commit `terraform.tfvars` to git (already in .gitignore)

## Deployment

### Development Environment

```powershell
cd terraform/environments/dev

# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Plan deployment (review changes)
terraform plan

# Apply changes
terraform apply

# After successful apply, view outputs
terraform output
```

### Get Application Insights Connection String

```powershell
# View sensitive outputs
terraform output -json | ConvertFrom-Json | Select-Object -ExpandProperty application_insights_connection_string

# Or use Azure CLI
az monitor app-insights component show `
  --app interstellar-tracker-dev-ai `
  --resource-group interstellar-tracker-dev-rg `
  --query connectionString `
  --output tsv
```

### Store Connection String Locally

```powershell
# Navigate to CalculationService
cd ../../../src/Services/CalculationService/InterstellarTracker.CalculationService

# Initialize user secrets
dotnet user-secrets init

# Store connection string (replace with actual value)
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=...;IngestionEndpoint=..."

# Navigate to WebUI
cd ../../../Web/InterstellarTracker.WebUI

# Initialize user secrets
dotnet user-secrets init

# Store connection string
dotnet user-secrets set "ApplicationInsights:ConnectionString" "InstrumentationKey=...;IngestionEndpoint=..."
```

## Resources Created

### Monitoring Module

- **Log Analytics Workspace**: `interstellar-tracker-dev-law`
  - SKU: PerGB2018
  - Retention: 30 days
  - Cost: ~$2.30/GB after first 5GB free

- **Application Insights**: `interstellar-tracker-dev-ai`
  - Workspace-based
  - Sampling: 100% (dev), adjust for production
  - Smart Detection: Failure and Performance Anomalies enabled

- **Action Group**: `interstellar-tracker-dev-alerts`
  - Email notifications to configured address

- **Metric Alerts**:
  - High Response Time: >1 second average
  - High Failure Rate: >10 failures in 15 minutes
  - Dependency Failures: >5 failures in 5 minutes

- **Workbook**: Service Health Dashboard

### Security Module

- **Key Vault**: `interstellar-dev-kv`
  - SKU: Standard
  - RBAC authorization enabled
  - Soft delete: 7 days
  - Network: Allow all (dev), restricted in production

- **Secrets**:
  - `ApplicationInsights--ConnectionString`: Auto-stored from monitoring module

## Cost Estimation

### Development Environment (Monthly)

| Resource                  | SKU/Tier    | Estimated Cost |
|---------------------------|-------------|----------------|
| Log Analytics Workspace   | PerGB2018   | $0 - $11.50    |
| Application Insights      | Data ingestion | $0 - $11.50 |
| Key Vault                 | Standard    | $0.03          |
| Storage (Terraform state) | Standard_LRS | $0.02         |
| **Total**                 |             | **~$0 - $25**  |

**Notes:**

- First 5GB/month of telemetry is free
- Low traffic dev environment typically stays within free tier
- Alerts and metrics are included in Application Insights cost

## Terraform Commands

### Basic Workflow

```powershell
terraform init          # Initialize backend and providers
terraform validate      # Validate configuration syntax
terraform fmt           # Format .tf files
terraform plan          # Preview changes
terraform apply         # Apply changes
terraform destroy       # Destroy all resources (careful!)
```

### State Management

```powershell
terraform state list                    # List all resources
terraform state show <resource>         # Show resource details
terraform refresh                       # Sync state with Azure
terraform import <resource> <azure-id>  # Import existing resource
```

### Outputs

```powershell
terraform output                        # Show all outputs
terraform output -json                  # JSON format
terraform output <output-name>          # Specific output
```

## Azure Portal Access

After deployment, access resources directly:

```powershell
# Get portal URLs from outputs
terraform output azure_portal_application_insights_url
terraform output azure_portal_log_analytics_url
terraform output azure_portal_key_vault_url
```

Or view all resources:

```
https://portal.azure.com/#@/resource/subscriptions/9fff6f2c-c722-4906-bb36-19bcd059d6d6/resourceGroups/interstellar-tracker-dev-rg/overview
```

## Troubleshooting

### Backend Initialization Fails

```powershell
# Verify storage account exists
az storage account show --name interstellartfstate --resource-group interstellar-tracker-tfstate-rg

# Verify you have access
az storage container list --account-name interstellartfstate
```

### Key Vault Name Already Exists

Key Vault names are globally unique. If `interstellar-dev-kv` is taken:

1. Edit `terraform/environments/dev/terraform.tfvars`:

   ```hcl
   key_vault_name = "interstellar-dev-kv-yourname"
   ```

2. Re-run `terraform plan` and `terraform apply`

### Permission Denied on Key Vault

Ensure you have RBAC permissions:

```powershell
# Assign Key Vault Administrator role to yourself
$VAULT_ID = (az keyvault show --name interstellar-dev-kv --query id --output tsv)
$USER_ID = (az ad signed-in-user show --query id --output tsv)

az role assignment create `
  --role "Key Vault Administrator" `
  --assignee $USER_ID `
  --scope $VAULT_ID
```

## Production Environment

Production configuration (future):

```powershell
cd terraform/environments/prod

# Key differences from dev:
# - Retention: 90 days
# - Sampling: 10% (cost optimization)
# - Purge protection: enabled
# - Network rules: restricted
# - High availability: zone redundancy
```

## Security Best Practices

1. **Never commit secrets**
   - `terraform.tfvars` is in .gitignore
   - Use Azure Key Vault for all secrets
   - Use user secrets for local development

2. **Use RBAC instead of access policies**
   - Key Vault RBAC enabled by default
   - Assign least privilege roles

3. **Enable audit logging**
   - Key Vault diagnostics send to Log Analytics
   - Review AuditEvent logs regularly

4. **Protect state files**
   - State stored in Azure Storage (encrypted)
   - Enable blob versioning for recovery
   - Restrict access to state storage

## References

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview)
- [Terraform Best Practices](https://developer.hashicorp.com/terraform/language/style)
