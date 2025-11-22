# Security Module Outputs

output "key_vault_id" {
  description = "ID of the Key Vault"
  value       = azurerm_key_vault.main.id
}

output "key_vault_name" {
  description = "Name of the Key Vault"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI of the Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "application_insights_secret_id" {
  description = "ID of the Application Insights connection string secret"
  value       = azurerm_key_vault_secret.application_insights_connection_string.id
}

output "application_insights_secret_version" {
  description = "Version of the Application Insights connection string secret"
  value       = azurerm_key_vault_secret.application_insights_connection_string.version
}
