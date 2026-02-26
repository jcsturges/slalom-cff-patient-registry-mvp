# ADR-004: React 18 with TypeScript for Frontend

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires a modern, responsive web application for:
- Patient roster management across 136 care centers
- Complex electronic Case Report Forms (eCRFs) with conditional logic
- Dynamic report builder interface
- CSV upload and data preview
- Content management for Foundation staff

Requirements include:
- Support for 3,000+ concurrent users
- Accessible UI (WCAG 2.1 AA compliance)
- Responsive design for various screen sizes
- Integration with Okta authentication
- Maintainable codebase for long-term support

The RFP tech stack mandates React 18 (TypeScript).

## Decision

We will use **React 18 with TypeScript** and the following ecosystem:

### Core Stack
- **React**: 18.2.0 - UI framework
- **TypeScript**: 5.3.x - Type safety
- **Vite**: 5.0.x - Build tooling (fast HMR, optimized builds)
- **React Router**: 6.21.x - Client-side routing

### State Management
- **React Query (TanStack Query)**: 5.17.x - Server state management
- **React Hook Form**: 7.49.x - Form state management
- **Zod**: 3.22.x - Runtime validation

### UI Components
- **Radix UI**: 1.x - Accessible, unstyled primitives
- **Tailwind CSS**: 3.4.x - Utility-first styling
- **Recharts**: 2.10.x - Data visualization

### Authentication
- **@okta/okta-react**: 6.8.0 - Okta React SDK
- **@okta/okta-auth-js**: 7.5.1 - Okta Auth JS

## Consequences

### Positive
- **Type Safety**: TypeScript catches errors at compile time
- **Component Reusability**: React's component model enables code reuse
- **Large Ecosystem**: Extensive library support for healthcare UIs
- **Developer Experience**: Fast refresh, excellent tooling
- **Accessibility**: Radix UI provides WCAG-compliant primitives
- **Performance**: React 18's concurrent features improve responsiveness
- **Testability**: Well-established testing patterns with React Testing Library

### Negative
- **Learning Curve**: TypeScript adds complexity for junior developers
- **Bundle Size**: React + dependencies increase initial load time
- **State Complexity**: Multiple state management approaches can confuse
- **Upgrade Churn**: React ecosystem evolves rapidly

### Mitigations
- Use code splitting and lazy loading for large components
- Establish clear patterns for state management (React Query for server, local state for UI)
- Document component patterns and provide examples
- Pin dependency versions and test upgrades thoroughly

## Alternatives Considered

### Angular
- **Rejected**: Not in approved tech stack
- More opinionated framework with steeper learning curve
- Larger bundle size

### Vue.js
- **Rejected**: Not in approved tech stack
- Smaller ecosystem for enterprise healthcare applications
- Less TypeScript integration maturity

### Server-Side Rendering (Next.js)
- **Rejected**: Adds complexity without clear benefit for this use case
- NGR is a data entry application, not content-focused
- Would require additional hosting configuration
