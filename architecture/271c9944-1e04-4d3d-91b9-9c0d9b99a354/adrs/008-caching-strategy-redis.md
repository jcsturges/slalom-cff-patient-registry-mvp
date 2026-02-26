# ADR-008: Caching Strategy - Redis

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system requires caching for:
- User session management (JWT refresh tokens, session state)
- Frequently accessed data (form definitions, user permissions)
- Rate limiting for API protection
- Query result caching for reporting performance

The caching solution must support high availability, data persistence options, and integrate well with the Python/FastAPI backend.

## Decision

We will use **Azure Cache for Redis (Premium tier)** for all caching needs.

### Cache Usage Patterns
| Use Case | TTL | Eviction |
|----------|-----|----------|
| User Sessions | 30 minutes | On logout/timeout |
| Form Definitions | 1 hour | On publish |
| User Permissions | 5 minutes | On role change |
| Rate Limiting | 1 minute windows | Automatic |
| Report Query Cache | 15 minutes | LRU |

## Consequences

### Positive
- Sub-millisecond latency for cached data
- Native session store support for FastAPI
- Built-in data structures (sorted sets for rate limiting, hashes for sessions)
- Azure managed service with automatic failover
- Persistence options (RDB/AOF) for session durability
- Cluster mode for horizontal scaling if needed
- TLS encryption in transit

### Negative
- Additional infrastructure component to manage
- Memory-based storage has cost implications for large datasets
- Cache invalidation complexity for distributed systems

### Risks
- Cache stampede on cold start or invalidation (mitigated by staggered TTLs)
- Redis unavailability impacts session management (mitigated by Premium tier HA)
- Stale data if invalidation fails (mitigated by short TTLs for critical data)

## Alternatives Considered

### In-Memory Caching (Application Level)
- **Pros:** No additional infrastructure, simple implementation
- **Cons:** Not shared across instances, lost on restart, memory pressure on app servers
- **Rejected because:** Kubernetes horizontal scaling requires shared cache

### Memcached
- **Pros:** Simple, fast, proven
- **Cons:** No persistence, limited data structures, no native Azure managed service
- **Rejected because:** Redis provides more features with equivalent performance

### Azure Cosmos DB (as cache)
- **Pros:** Global distribution, multiple consistency levels
- **Cons:** Higher latency than Redis, more expensive for cache workload, overkill for caching
- **Rejected because:** Redis is purpose-built for caching with better performance

### PostgreSQL (for sessions)
- **Pros:** No additional infrastructure, ACID compliance
- **Cons:** Higher latency, database load for session operations
- **Rejected because:** Redis provides better performance for high-frequency session access

### No Caching
- **Pros:** Simplicity, no cache invalidation concerns
- **Cons:** Higher database load, slower response times, no rate limiting
- **Rejected because:** Performance and security requirements necessitate caching
