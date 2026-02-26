# ADR-009: Azure Application Insights for Monitoring

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires comprehensive monitoring for:
- Application performance tracking
- Error detection and alerting
- User behavior analytics
- Distributed tracing across services
- Compliance audit logging
- Capacity planning and optimization

Requirements include:
- Real-time visibility into system health
- Proactive alerting for issues
- Historical data for trend analysis
- Integration with Azure services

The RFP tech stack mandates Azure Application Insights.

## Decision

We will use **Azure Application Insights** with the following configuration:

### Telemetry Collection
- **Requests**: All API requests with timing and status
- **Dependencies**: Database queries, external HTTP calls
- **Exceptions**: All unhandled exceptions with stack traces
- **Traces**: Structured logging via Serilog
- **Custom Events**: Business events (form submissions, patient operations)
- **Custom Metrics**: Performance counters, queue depths

### Integration
- **API**: Microsoft.ApplicationInsights.AspNetCore SDK
- **Web App**: Application Insights JavaScript SDK
- **Logging**: Serilog.Sinks.ApplicationInsights

### Alerting Rules
| Alert | Condition | Severity |
|-------|-----------|----------|
| High Error Rate | Exceptions > 10/min | Critical |
| Slow Response | P95 latency > 2s | Warning |
| Failed Requests | 5xx errors > 5% | Critical |
| Database Slow | SQL queries > 5s | Warning |

### Retention
- Raw telemetry: 90 days
- Aggregated metrics: 13 months
- Continuous export to Log Analytics for long-term retention

## Consequences

### Positive
- **Native Integration**: Seamless with Azure App Service and SQL
- **Distributed Tracing**: End-to-end request tracking
- **Smart Detection**: AI-powered anomaly detection
- **Live Metrics**: Real-time performance monitoring
- **Application Map**: Visual dependency mapping
- **Cost Effective**: Pay-per-GB pricing model

### Negative
- **Sampling**: High-volume telemetry may be sampled
- **Latency**: ~2-5 minute delay for some metrics
- **Query Complexity**: Kusto Query Language learning curve
- **Data Volume**: PHI must be excluded from telemetry

### Mitigations
- Configure adaptive sampling thresholds
- Use custom telemetry processors to filter PHI
- Create saved queries and dashboards for common scenarios
- Set up continuous export for compliance retention

## Alternatives Considered

### Datadog
- **Rejected**: Not in approved tech stack
- Would provide similar capabilities
- Additional licensing cost

### Elastic APM
- **Rejected**: Requires self-hosted infrastructure
- Higher operational overhead
- Less Azure-native integration

### New Relic
- **Rejected**: Not in approved tech stack
- Similar capabilities to App Insights
- Different pricing model
