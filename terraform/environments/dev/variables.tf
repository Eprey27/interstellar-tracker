# Development Environment Variables

# Azure Subscription
variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
  sensitive   = true
}

# General Configuration
variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "West Europe"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "interstellar-tracker-dev-rg"
}

variable "owner_email" {
  description = "Email of the resource owner"
  type        = string
}

# Monitoring Configuration
variable "log_analytics_workspace_name" {
  description = "Name of the Log Analytics workspace"
  type        = string
  default     = "interstellar-tracker-dev-law"
}

variable "log_analytics_sku" {
  description = "SKU for Log Analytics workspace"
  type        = string
  default     = "PerGB2018"
}

variable "retention_in_days" {
  description = "Data retention period in days (dev: 30 days)"
  type        = number
  default     = 30
}

variable "application_insights_name" {
  description = "Name of the Application Insights resource"
  type        = string
  default     = "interstellar-tracker-dev-ai"
}

variable "sampling_percentage" {
  description = "Sampling percentage for telemetry (dev: 100%)"
  type        = number
  default     = 100
}

# Alerts Configuration
variable "action_group_name" {
  description = "Name of the action group for alerts"
  type        = string
  default     = "interstellar-tracker-dev-alerts"
}

variable "action_group_short_name" {
  description = "Short name for action group (max 12 chars)"
  type        = string
  default     = "itdev"
}

variable "alert_email" {
  description = "Email address for alert notifications"
  type        = string
}

# Key Vault Configuration
variable "key_vault_name" {
  description = "Name of the Key Vault (must be globally unique)"
  type        = string
  default     = "interstellar-dev-kv"
}

variable "key_vault_sku" {
  description = "SKU for Key Vault"
  type        = string
  default     = "standard"
}

variable "soft_delete_retention_days" {
  description = "Number of days to retain deleted items"
  type        = number
  default     = 7
}

variable "purge_protection_enabled" {
  description = "Enable purge protection for Key Vault (dev: false)"
  type        = bool
  default     = false
}

variable "key_vault_network_default_action" {
  description = "Default action for Key Vault network rules (dev: Allow)"
  type        = string
  default     = "Allow"
}

variable "key_vault_allowed_ips" {
  description = "List of IP addresses allowed to access Key Vault"
  type        = list(string)
  default     = []
}
