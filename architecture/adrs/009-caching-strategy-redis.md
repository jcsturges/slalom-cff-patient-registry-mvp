# ADR-009: Caching Strategy - Redis

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR requires caching for:
- User session management
- API response caching for frequently accessed data
- Rate limiting at the API gateway
- Temporary storage for background job coordination
- Reducing database load for read-heavy operations

## Decision

We will use **Redis 7.2** via **Azure Cache for Redis Premium** as the caching layer.

Use cases:
- **Session Storage**: User sessions with 8-hour TTL
- **Form Definitions**: Cached form schemas with 1-hour TTL
- **User Permissions**: RBAC permissions with 5-minute TTL
- **Patient Lists**: Program patient rosters with 5-minute TTL
- **Rate Limiting**: API rate limit counters
- **Distributed Locks**: Coordination for background jobs

## Consequences

### Positive
- **Performance**: In-memory storage provides sub-millisecond latency
- **Versatility**: Supports strings, hashes, lists, sets, sorted sets
- **Pub/Sub**: Built-in pub/sub for cache invalidation
- **Persistence**: Optional persistence for session durability
- **Managed Service**: Azure manages clustering, failover, patching
- **Celery Integration**: Native support as Celery result backend

### Negative
- **Memory Cost**: In-memory storage more expensive than disk
- **Data Loss Risk**: Potential data loss on failure (mitigated by persistence)
- **Complexity**: Cache invalidation requires careful design
- **Single Point**: Cache failures can impact performance (not availability)

### Mitigations
- Use Redis persistence (RDB + AOF) for session durability
- Implement cache-aside pattern with graceful degradation
- Design cache keys with clear TTL and invalidation strategies
- Use Redis Cluster for high availability
- Monitor cache hit rates and memory usage

## Alternatives Considered

### Memcached
- **Pros**: Simple, fast, widely used
- **Cons**: No persistence, limited data structures, no pub/sub
- **Rejected**: Redis's additional features (persistence, pub/sub) valuable

### Azure Cosmos DB (as cache)
- **Pros**: Global distribution, managed scaling
- **Cons**: Higher latency than Redis, more expensive, overkill for caching
- **Rejected**: Redis better suited for caching use case

### Application-Level Caching (in-memory)
- **Pros**: No external dependency, lowest latency
- **Cons**: Not shared across pods, memory pressure on application
- **Rejected**: Distributed cache needed for multi-pod deployment

### PostgreSQL (for sessions)
- **Pros**: No additional infrastructure, ACID guarantees
- **Cons**: Higher latency, database load, not designed for caching
- **Rejected**: Redis's performance characteristics better for sessions
