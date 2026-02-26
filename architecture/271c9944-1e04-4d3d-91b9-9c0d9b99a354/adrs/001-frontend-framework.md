# ADR-001: Frontend Framework Selection

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The Next Generation Registry (NGR) requires a modern, maintainable frontend framework to build a single-page application (SPA) that supports complex form handling, dynamic reporting, and integration with Okta authentication. The application must support ~3,000+ users across 136 care centers with varying technical capabilities.

Key requirements:
- Complex dynamic form rendering for eCRFs with hundreds of fields
- Real-time validation and conditional logic
- Integration with Okta OIDC authentication
- Responsive design for various screen sizes
- Long-term maintainability and developer availability
- TypeScript support for type safety

## Decision

We will use **React 18 with TypeScript** as the frontend framework, hosted on Azure App Service.

**Specific versions and key libraries:**
- React 18.2.0
- TypeScript 5.3.3
- Vite 5.0.10 (build tool)
- @okta/okta-react 6.7.0 (authentication)
- @tanstack/react-query 5.17.1 (server state management)
- react-hook-form 7.49.2 (form handling)
- zod 3.22.4 (schema validation)
- @mui/material 5.15.2 (UI components)
- react-router-dom 6.21.1 (routing)

## Consequences

### Positive
- **Mandated by tech stack**: Aligns with Foundation's required technology stack
- **Mature ecosystem**: Extensive library support for forms, validation, and UI components
- **Okta SDK support**: Official @okta/okta-react SDK provides seamless authentication integration
- **Developer availability**: Large talent pool familiar with React and TypeScript
- **Performance**: React 18's concurrent features improve perceived performance for complex forms
- **Type safety**: TypeScript catches errors at compile time, reducing runtime bugs

### Negative
- **Bundle size**: React applications can have larger bundle sizes; mitigated by code splitting
- **Learning curve**: Team members unfamiliar with React hooks may need training
- **State management complexity**: Complex forms require careful state management design

### Risks
- **Library updates**: Must monitor for breaking changes in major dependencies
- **Performance with large forms**: eCRFs with hundreds of fields require optimization (virtualization, memoization)

## Alternatives Considered

### Angular
- **Rejected**: Not in mandated tech stack; larger bundle size; steeper learning curve

### Vue.js
- **Rejected**: Not in mandated tech stack; smaller ecosystem for enterprise features

### Server-rendered (Razor Pages)
- **Rejected**: Not in mandated tech stack; less interactive user experience for complex forms
