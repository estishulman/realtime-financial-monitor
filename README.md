# Financial Monitor

Real-time financial transaction monitor built with .NET 9, SignalR, React, TypeScript, PostgreSQL, SQLite, and Redis.

## Architecture

```text
React /add
    |
    | POST /api/transactions
    v
ASP.NET Core API
    |
    +--> EF transaction repository (SQLite locally or PostgreSQL in Compose/Kubernetes)
    |
    +--> Channel<Transaction>
             |
             v
       Background processing
             |
             +--> update repository: Pending -> Completed/Failed
             |
             +--> SignalR: ReceiveTransaction
                              |
                              v
                         React /monitor
```

The client loads the current snapshot through `GET /api/transactions` and receives future changes through SignalR at `/hubs/transactions`.

## Redis Backplane Decision

### Problem

With multiple API replicas, a client connected to replica A must also receive a transaction processed by replica B. Local SignalR state alone cannot distribute that message between replicas.

### Decision

When `ConnectionStrings:Redis` is configured, the API uses the SignalR Redis backplane:

```csharp
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
```

Redis distributes SignalR messages between API replicas. In multi-pod mode it also provides a shared short-lived cache for the transaction snapshot. It does not replace PostgreSQL as the source of truth.

### Current MVP limitation

The local configuration uses SQLite and an in-process memory cache. The Docker Compose configuration uses PostgreSQL and Redis, so multiple API replicas can share the database and cache.

### Production evolution

For stronger delivery guarantees, add an outbox table and publish outbox events to a broker or Redis Streams. Redis Pub/Sub is suitable for SignalR fan-out but is not a durable transaction queue. Cache entries have a short TTL and are invalidated after writes; the database remains authoritative.

## Run Locally

### Backend

```bash
dotnet test FinancialMonitor.sln
dotnet run --project FinancialMonitor.Api
```

### Frontend

```bash
cd FinancialMonitor.Client
npm install
npm run dev
```

Development URLs:

- API: `http://localhost:5058`
- Frontend: `http://localhost:5173`
- SignalR: `http://localhost:5058/hubs/transactions`

## Run with Docker Compose

Start Docker Desktop, then run from the repository root:

```bash
docker compose up --build
```

URLs:

- Frontend: `http://localhost:8081`
- API: `http://localhost:8080`
- Redis: `localhost:6379`

Compose injects `Database__Provider=Postgres`, `Cache__Provider=Redis`, and `ConnectionStrings__Redis=redis:6379`. This enables PostgreSQL as the shared source of truth, Redis as the shared snapshot cache, and Redis as the SignalR backplane.

## Kubernetes

The `k8s` directory contains deployments and services for the API, client, and Redis:

```bash
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/client-deployment.yaml
```

The API deployment has two replicas. Redis is configured as both the distributed cache and SignalR backplane. Kubernetes should use a shared PostgreSQL service or managed database; the current API manifest is a deployment example and should add Secrets, readiness/liveness probes, resource limits, and a PostgreSQL connection string for production.

## Transaction Processing Rule

New transactions enter as `Pending`. The MVP processor resolves them deterministically:

- Amount up to `10,000`: `Completed`.
- Amount above `10,000`: `Failed`.

The dashboard receives both lifecycle events and replaces the existing row by `transactionId`.

## Verification

The repository currently has 13 passing .NET tests covering validation, storage concurrency, processing, controller enqueueing, SignalR broadcasting, and concurrent HTTP ingestion. The React client is verified with `npm run build` and `npm run lint`.