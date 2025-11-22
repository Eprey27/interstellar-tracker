# Application Insights and Log Analytics Workspace Module
# This module creates the monitoring infrastructure for Interstellar Tracker

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "main" {
  name                = var.log_analytics_workspace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.log_analytics_sku
  retention_in_days   = var.retention_in_days

  tags = merge(
    var.common_tags,
    {
      Component = "Monitoring"
      Service   = "LogAnalytics"
    }
  )
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = var.application_insights_name
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"

  # Sampling configuration
  sampling_percentage = var.sampling_percentage

  # Data retention
  retention_in_days = var.retention_in_days

  # Enable features
  disable_ip_masking = false
  local_authentication_disabled = false

  tags = merge(
    var.common_tags,
    {
      Component = "Monitoring"
      Service   = "ApplicationInsights"
    }
  )
}

# Action Group for Alerts
resource "azurerm_monitor_action_group" "main" {
  name                = var.action_group_name
  resource_group_name = var.resource_group_name
  short_name          = var.action_group_short_name

  email_receiver {
    name          = "DevTeam"
    email_address = var.alert_email
  }

  tags = merge(
    var.common_tags,
    {
      Component = "Monitoring"
      Service   = "Alerts"
    }
  )
}

# Smart Detection - Failure Anomalies (built-in)
resource "azurerm_application_insights_smart_detection_rule" "failure_anomalies" {
  name                    = "Failure Anomalies"
  application_insights_id = azurerm_application_insights.main.id
  enabled                 = true
}

# Smart Detection - Performance Anomalies
resource "azurerm_application_insights_smart_detection_rule" "performance_anomalies" {
  name                    = "Slow page load time"
  application_insights_id = azurerm_application_insights.main.id
  enabled                 = true
}

# Metric Alert - High Response Time
resource "azurerm_monitor_metric_alert" "high_response_time" {
  name                = "${var.application_insights_name}-high-response-time"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.main.id]
  description         = "Alert when average response time exceeds threshold"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "requests/duration"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 1000 # 1 second
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Metric Alert - High Failure Rate
resource "azurerm_monitor_metric_alert" "high_failure_rate" {
  name                = "${var.application_insights_name}-high-failure-rate"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.main.id]
  description         = "Alert when request failure rate exceeds 5%"
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "requests/failed"
    aggregation      = "Count"
    operator         = "GreaterThan"
    threshold        = 10
  }

  action {
    action_group_id = azurerm_monitor_action_group.main.id
  }

  tags = var.common_tags
}

# Log Analytics Query Alert - Dependency Failures
resource "azurerm_monitor_scheduled_query_rules_alert_v2" "dependency_failures" {
  name                = "${var.application_insights_name}-dependency-failures"
  resource_group_name = var.resource_group_name
  location            = var.location
  
  evaluation_frequency = "PT5M"
  window_duration      = "PT5M"
  scopes               = [azurerm_application_insights.main.id]
  severity             = 2

  criteria {
    query = <<-QUERY
      dependencies
      | where timestamp > ago(5m)
      | where success == false
      | summarize failures = count() by name
      | where failures > 5
    QUERY

    time_aggregation_method = "Count"
    threshold               = 1
    operator                = "GreaterThan"

    dimension {
      name     = "name"
      operator = "Include"
      values   = ["*"]
    }
  }

  action {
    action_groups = [azurerm_monitor_action_group.main.id]
  }

  tags = var.common_tags
}

# Workbook - Service Health Dashboard
resource "azurerm_application_insights_workbook" "service_health" {
  name                = "service-health-dashboard"
  resource_group_name = var.resource_group_name
  location            = var.location
  display_name        = "Interstellar Tracker - Service Health"
  source_id           = azurerm_application_insights.main.id

  data_json = jsonencode({
    version = "Notebook/1.0"
    items = [
      {
        type = 1
        content = {
          json = "## Interstellar Tracker - Service Health Dashboard\nReal-time monitoring of all services"
        }
      },
      {
        type = 3
        content = {
          version = "KqlItem/1.0"
          query = <<-QUERY
            requests
            | where timestamp > ago(1h)
            | summarize 
                TotalRequests = count(),
                FailedRequests = countif(success == false),
                AvgDuration = avg(duration)
            by name
            | extend FailureRate = (FailedRequests * 100.0) / TotalRequests
            | order by TotalRequests desc
          QUERY
          size = 0
          title = "Request Summary (Last Hour)"
          timeContext = {
            durationMs = 3600000
          }
          queryType = 0
          resourceType = "microsoft.insights/components"
        }
      }
    ]
  })

  tags = var.common_tags
}
