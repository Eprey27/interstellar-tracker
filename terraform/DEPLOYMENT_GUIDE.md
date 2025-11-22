# Deployment Guide - Application Insights with Terraform

This guide walks you through deploying Application Insights infrastructure to Azure using Terraform.

## Prerequisites Checklist

- [ ] Terraform installed (>= 1.9.0)
- [ ] Azure CLI installed
- [ ] Azure subscription access (ID: `9fff6f2c-c722-4906-bb36-19bcd059d6d6`)
- [ ] Your email address for alerts
- [ ] (Optional) Your public IP for Key Vault access

## Step 1: Install Required Tools

### Terraform

```powershell
# Check if already installed
terraform --version

# If not installed
winget install Hashicorp.Terraform

# Verify installation
terraform --version  # Should show >= 1.9.0
```

### Azure CLI

```powershell
# Check if already installed
az --version

# If not installed
winget install Microsoft.AzureCLI

# Verify installation
az --version
```

## Step 2: Login to Azure

```powershell
# Login with your Azure account
az login

# Set the subscription
az account set --subscription 9fff6f2c-c722-4906-bb36-19bcd059d6d6

# Verify you're on the correct subscription
az account show
```

## Step 3: Create Terraform Backend Storage

This is a **one-time setup** to store Terraform state in Azure:

```powershell
# Define variables
$RESOURCE_GROUP = "interstellar-tracker-tfstate-rg"
$LOCATION = "westeurope"
$STORAGE_ACCOUNT = "interstellartfstate"
$CONTAINER_NAME = "tfstate"

# Create resource group for Terraform state
az group create `
  --name $RESOURCE_GROUP `
  --location $LOCATION `
  --tags Project=InterstellarTracker Purpose=TerraformState

# Create storage account (may take 1-2 minutes)
az storage account create `
  --resource-group $RESOURCE_GROUP `
  --name $STORAGE_ACCOUNT `
  --sku Standard_LRS `
  --encryption-services blob `
  --location $LOCATION `
  --min-tls-version TLS1_2 `
  --tags Project=InterstellarTracker Purpose=TerraformState

# Create blob container
az storage container create `
  --name $CONTAINER_NAME `
  --account-name $STORAGE_ACCOUNT `
  --auth-mode login

# Verify creation
az storage account show --name $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP --output table
```

**Expected output:**

```
Name                  ResourceGroup                      Location    StatusOfPrimary
--------------------  ---------------------------------  ----------  -----------------
interstellartfstate   interstellar-tracker-tfstate-rg    westeurope  available
```

## Step 4: Configure Terraform Variables

### Get Your Public IP (optional, for Key Vault access)

```powershell
# Get your public IP
$MY_IP = (Invoke-WebRequest -Uri "https://ifconfig.me/ip").Content.Trim()
Write-Host "Your public IP: $MY_IP"
```

### Create terraform.tfvars File

```powershell
# Navigate to dev environment
cd terraform/environments/dev

# Copy example file
Copy-Item terraform.tfvars.example terraform.tfvars

# Open in editor
notepad terraform.tfvars
```

### Update terraform.tfvars

Replace `YOUR_EMAIL@example.com` with your actual email:

```hcl
subscription_id = "9fff6f2c-c722-4906-bb36-19bcd059d6d6"
owner_email     = "your-actual-email@example.com"
alert_email     = "your-actual-email@example.com"

# Optional: Add your IP for Key Vault direct access
# key_vault_allowed_ips = ["YOUR.IP.ADDRESS"]
```

**Save and close the file.**

## Step 5: Initialize Terraform

```powershell
# Make sure you're in terraform/environments/dev
cd terraform/environments/dev

# Initialize Terraform (downloads providers, configures backend)
terraform init

# Verify initialization
ls .terraform/
```

**Expected output:**

```
Initializing the backend...
Successfully configured the backend "azurerm"!

Initializing provider plugins...
- Finding hashicorp/azurerm versions matching "~> 4.0"...
- Installing hashicorp/azurerm v4.x.x...

Terraform has been successfully initialized!
```

## Step 6: Validate Configuration

```powershell
# Validate Terraform syntax
terraform validate

# Format .tf files (optional, but recommended)
terraform fmt -recursive
```

**Expected output:**

```
Success! The configuration is valid.
```

## Step 7: Plan Deployment

This shows what Terraform will create **without actually creating it**:

```powershell
# Generate and review execution plan
terraform plan

# Save plan to file for review (optional)
terraform plan -out=tfplan
```

**Review the output carefully.** You should see:

- Resource Group: `interstellar-tracker-dev-rg`
- Log Analytics Workspace: `interstellar-tracker-dev-law`
- Application Insights: `interstellar-tracker-dev-ai`
- Key Vault: `interstellar-dev-kv` (or custom name)
- Action Group for alerts
- 2 Metric Alerts
- 1 Scheduled Query Alert
- 2 Smart Detection Rules
- 1 Workbook

**Total resources to create: ~12-14**

## Step 8: Apply Infrastructure

**⚠️ This will create real Azure resources and may incur costs (minimal for dev).**

```powershell
# Apply the Terraform configuration
terraform apply

# Review the plan, type 'yes' when prompted
```

**Deployment time: 2-5 minutes**

**Expected output:**

```
azurerm_resource_group.main: Creating...
azurerm_resource_group.main: Creation complete after 1s
module.monitoring.azurerm_log_analytics_workspace.main: Creating...
...
Apply complete! Resources: 14 added, 0 changed, 0 destroyed.

Outputs:

application_insights_connection_string = <sensitive>
application_insights_name = "interstellar-tracker-dev-ai"
...
```

## Step 9: Verify Deployment

### View Outputs

```powershell
# View all outputs (sensitive values hidden)
terraform output

# View specific output
terraform output application_insights_name

# View sensitive outputs (connection string)
terraform output -raw application_insights_connection_string
```

### Open in Azure Portal

```powershell
# Get portal URLs
$AI_URL = terraform output -raw azure_portal_application_insights_url
$KV_URL = terraform output -raw azure_portal_key_vault_url

# Open in browser
Start-Process $AI_URL
Start-Process $KV_URL
```

### Verify Resources in Azure CLI

```powershell
# List resources in resource group
az resource list `
  --resource-group interstellar-tracker-dev-rg `
  --output table

# Check Application Insights
az monitor app-insights component show `
  --app interstellar-tracker-dev-ai `
  --resource-group interstellar-tracker-dev-rg `
  --output table

# Check Key Vault
az keyvault show `
  --name interstellar-dev-kv `
  --output table
```

## Step 10: Configure Local Services

### Get Connection String

```powershell
# Get Application Insights connection string
$CONN_STRING = terraform output -raw application_insights_connection_string

# Copy to clipboard (Windows)
$CONN_STRING | Set-Clipboard
Write-Host "Connection string copied to clipboard!"
```

### Configure CalculationService

```powershell
# Navigate to CalculationService
cd ../../../../src/Services/CalculationService/InterstellarTracker.CalculationService

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Store connection string
dotnet user-secrets set "ApplicationInsights:ConnectionString" $CONN_STRING

# Verify
dotnet user-secrets list
```

### Configure WebUI

```powershell
# Navigate to WebUI
cd ../../../Web/InterstellarTracker.WebUI

# Initialize user secrets (if not already done)
dotnet user-secrets init

# Store connection string
dotnet user-secrets set "ApplicationInsights:ConnectionString" $CONN_STRING

# Verify
dotnet user-secrets list
```

## Step 11: Test Telemetry Collection

### Start Services Locally

```powershell
# Terminal 1: Start CalculationService
cd src/Services/CalculationService/InterstellarTracker.CalculationService
dotnet run

# Terminal 2: Start WebUI
cd src/Web/InterstellarTracker.WebUI
dotnet run
```

### Generate Test Traffic

```powershell
# Open WebUI in browser
Start-Process "http://localhost:5000"

# Make some requests:
# - Load the homepage
# - Navigate between pages
# - Trigger orbit calculations
# - Intentionally cause an error (invalid input)
```

### View Live Metrics

1. Open Azure Portal → Application Insights
2. Navigate to **Live Metrics**
3. You should see:
   - Incoming request rate
   - Request duration
   - Failure rate
   - Server metrics (CPU, memory)
   - Sample telemetry events

**Wait 2-5 minutes for data to appear.**

### Query Telemetry Data

```powershell
# Open Logs in Azure Portal
$RG = "interstellar-tracker-dev-rg"
$AI = "interstellar-tracker-dev-ai"

az monitor app-insights query `
  --app $AI `
  --resource-group $RG `
  --analytics-query "requests | where timestamp > ago(1h) | summarize count() by name" `
  --output table
```

## Step 12: Grant Key Vault Access (if needed)

If you need to access Key Vault secrets directly:

```powershell
# Get your user ID
$USER_ID = (az ad signed-in-user show --query id --output tsv)

# Get Key Vault resource ID
$KV_ID = (az keyvault show --name interstellar-dev-kv --query id --output tsv)

# Assign Key Vault Administrator role
az role assignment create `
  --role "Key Vault Administrator" `
  --assignee $USER_ID `
  --scope $KV_ID

# Verify access
az keyvault secret list --vault-name interstellar-dev-kv --output table
```

## Troubleshooting

### Error: Key Vault name already exists

Key Vault names are globally unique. Update `terraform.tfvars`:

```hcl
key_vault_name = "interstellar-dev-kv-yourname"
```

Then re-run `terraform plan` and `terraform apply`.

### Error: Backend initialization failed

Verify storage account exists:

```powershell
az storage account show `
  --name interstellartfstate `
  --resource-group interstellar-tracker-tfstate-rg
```

If missing, repeat **Step 3**.

### Error: Insufficient permissions

Ensure you have Contributor role on the subscription:

```powershell
az role assignment list --assignee (az ad signed-in-user show --query id --output tsv) --output table
```

### No telemetry appearing in Application Insights

1. **Check connection string**: `dotnet user-secrets list`
2. **Verify services are running**: Check console output for Application Insights initialization
3. **Wait longer**: Telemetry can take 2-5 minutes to appear
4. **Check sampling**: Dev environment uses 100% sampling
5. **Verify network**: Ensure outbound HTTPS to `*.applicationinsights.azure.com`

### Can't access Key Vault

1. **RBAC permissions**: Run Step 12 to grant yourself Key Vault Administrator
2. **Network rules**: Dev environment allows all by default
3. **Check firewall**: Ensure outbound HTTPS to `*.vault.azure.net`

## Clean Up (Optional)

To destroy all resources (careful!):

```powershell
cd terraform/environments/dev

# Destroy all resources
terraform destroy

# Type 'yes' when prompted

# Delete Terraform state storage (optional)
az group delete --name interstellar-tracker-tfstate-rg --yes --no-wait
```

## Next Steps

✅ **Infrastructure deployed!**

1. **Configure CI/CD**: Update GitHub Actions to use Terraform outputs
2. **Deploy to Container Apps**: Use Application Insights connection string
3. **Set up custom metrics**: Track orbit calculations, cache hits, etc.
4. **Configure alerts**: Tune thresholds based on actual traffic
5. **Create dashboards**: Build custom workbooks for team visibility

## Cost Management

Monitor spending:

```powershell
# Check current costs
az consumption usage list `
  --start-date (Get-Date).AddDays(-30).ToString("yyyy-MM-dd") `
  --end-date (Get-Date).ToString("yyyy-MM-dd") `
  --output table

# Set up budget alert
az consumption budget create `
  --budget-name "interstellar-tracker-monthly" `
  --amount 50 `
  --time-grain Monthly `
  --start-date (Get-Date).ToString("yyyy-MM-01") `
  --end-date (Get-Date).AddMonths(12).ToString("yyyy-MM-01") `
  --resource-group interstellar-tracker-dev-rg
```

## References

- [Terraform README](../README.md) - Detailed documentation
- [Application Insights Setup Guide](../../docs/application-insights-setup.md) - Usage and best practices
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure CLI Reference](https://learn.microsoft.com/en-us/cli/azure/)
