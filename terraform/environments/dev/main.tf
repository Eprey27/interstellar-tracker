# Interstellar Tracker - Development Environment
# Main Terraform configuration for Azure infrastructure

terraform {
  required_version = ">= 1.0.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  # Backend configuration for state management
  backend "azurerm" {
    resource_group_name  = "interstellar-tracker-tfstate-rg"
    storage_account_name = "interstellartfstate"
    container_name       = "tfstate"
    key                  = "dev.terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }

    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }

  subscription_id = var.subscription_id
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = local.common_tags
}

# Monitoring Module
module "monitoring" {
  source = "../../modules/monitoring"

  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  log_analytics_workspace_name = var.log_analytics_workspace_name
  log_analytics_sku            = var.log_analytics_sku
  retention_in_days            = var.retention_in_days
  application_insights_name    = var.application_insights_name
  sampling_percentage          = var.sampling_percentage
  action_group_name            = var.action_group_name
  action_group_short_name      = var.action_group_short_name
  alert_email                  = var.alert_email
  common_tags                  = local.common_tags
}

# Security Module
module "security" {
  source = "../../modules/security"

  resource_group_name                    = azurerm_resource_group.main.name
  location                               = azurerm_resource_group.main.location
  key_vault_name                         = var.key_vault_name
  key_vault_sku                          = var.key_vault_sku
  soft_delete_retention_days             = var.soft_delete_retention_days
  purge_protection_enabled               = var.purge_protection_enabled
  key_vault_network_default_action       = var.key_vault_network_default_action
  key_vault_allowed_ips                  = var.key_vault_allowed_ips
  application_insights_connection_string = module.monitoring.application_insights_connection_string
  log_analytics_workspace_id             = module.monitoring.log_analytics_workspace_id
  common_tags                            = local.common_tags

  depends_on = [module.monitoring]
}

# Local values
locals {
  common_tags = {
    Project     = "InterstellarTracker"
    Environment = var.environment
    ManagedBy   = "Terraform"
    Owner       = var.owner_email
    CreatedDate = formatdate("YYYY-MM-DD", timestamp())
  }
}
