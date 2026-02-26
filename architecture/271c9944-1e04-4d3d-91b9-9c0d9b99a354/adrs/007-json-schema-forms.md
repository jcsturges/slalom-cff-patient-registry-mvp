# ADR-007: JSON Schema for Dynamic Form Definitions

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system must support electronic Case Report Forms (eCRFs) with:
- Hundreds of fields per form (e.g., Annual Report)
- Conditional logic (field visibility based on other fields)
- Field-level validation rules
- Form versioning for regulatory compliance
- Foundation-managed form definitions (no code changes required)
- Machine-readable form specifications (per PRD requirement)

The Foundation will provide form specifications that the system must render dynamically.

## Decision

We will use **JSON Schema** for form definitions with a custom extension for UI rendering:

### Schema Structure
```json
{
  "id": "annual-report-2024",
  "version": 1,
  "title": "Annual Report 2024",
  "sections": [
    {
      "id": "demographics",
      "title": "Demographics",
      "fields": [
        {
          "id": "height",
          "type": "number",
          "label": "Height (cm)",
          "validation": {
            "required": true,
            "min": 50,
            "max": 250
          },
          "conditions": {
            "visible": { "field": "age", "operator": ">=", "value": 2 }
          }
        }
      ]
    }
  ]
}
```

### Implementation
- **Storage**: JSON column in Azure SQL (FormDefinitions.Schema)
- **Rendering**: React component library for dynamic form generation
- **Validation**: Zod schemas generated from JSON Schema at runtime
- **Versioning**: Immutable form versions with effective dates

## Consequences

### Positive
- **No-Code Updates**: Foundation can modify forms without deployments
- **Version Control**: Form changes are tracked and auditable
- **Flexibility**: Supports complex conditional logic
- **Standards-Based**: JSON Schema is a well-known standard
- **Testability**: Form definitions can be validated independently

### Negative
- **Complexity**: Custom schema extensions add development overhead
- **Performance**: Runtime schema parsing adds latency
- **Validation Gaps**: Not all validation rules map to JSON Schema
- **Learning Curve**: Foundation staff need schema training

### Mitigations
- Build a form designer UI for Foundation staff
- Cache parsed schemas in memory
- Create comprehensive validation rule library
- Document schema format with examples

## Alternatives Considered

### XForms
- **Rejected**: XML-based, less developer-friendly
- Limited tooling support in modern web frameworks
- Steeper learning curve

### Custom DSL
- **Rejected**: Would require building parser and tooling
- No existing ecosystem support
- Higher maintenance burden

### Hardcoded Forms
- **Rejected**: Requires code changes for form updates
- Does not meet Foundation's requirement for GUI-based management
- Slower iteration on form changes
