# Development Environment Outputs

# Resource Group
output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "resource_group_id" {
  description = "ID of the resource group"
  value       = azurerm_resource_group.main.id
}

# Monitoring
output "log_analytics_workspace_id" {
  description = "ID of the Log Analytics workspace"
  value       = module.monitoring.log_analytics_workspace_id
}

output "log_analytics_workspace_name" {
  description = "Name of the Log Analytics workspace"
  value       = module.monitoring.log_analytics_workspace_name
}

output "application_insights_id" {
  description = "ID of the Application Insights resource"
  value       = module.monitoring.application_insights_id
}

output "application_insights_name" {
  description = "Name of the Application Insights resource"
  value       = module.monitoring.application_insights_name
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = module.monitoring.application_insights_instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}

output "application_insights_app_id" {
  description = "App ID for Application Insights"
  value       = module.monitoring.application_insights_app_id
}

# Security
output "key_vault_id" {
  description = "ID of the Key Vault"
  value       = module.security.key_vault_id
}

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = module.security.key_vault_name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = module.security.key_vault_uri
}

# Azure Portal Links
output "azure_portal_application_insights_url" {
  description = "URL to Application Insights in Azure Portal"
  value       = "https://portal.azure.com/#@/resource${module.monitoring.application_insights_id}/overview"
}

output "azure_portal_log_analytics_url" {
  description = "URL to Log Analytics workspace in Azure Portal"
  value       = "https://portal.azure.com/#@/resource${module.monitoring.log_analytics_workspace_id}/overview"
}

output "azure_portal_key_vault_url" {
  description = "URL to Key Vault in Azure Portal"
  value       = "https://portal.azure.com/#@/resource${module.security.key_vault_id}/overview"
}

# Configuration for Services
output "service_configuration" {
  description = "Configuration values for services"
  value = {
    application_insights_connection_string_secret = "@Microsoft.KeyVault(SecretUri=${module.security.key_vault_uri}secrets/ApplicationInsights--ConnectionString/)"
    key_vault_name                                = module.security.key_vault_name
    log_analytics_workspace_id                    = module.monitoring.log_analytics_workspace_id
  }
}
