# ADR: .NET Idempotency Store And Outbox Mechanism

Status: Approved for implementation
Date: 2026-05-14

## Context

The .NET translation keeps XIIGen DNA rule 7 and DNA rule 8:

- mutations and queue consumers require idempotency keys;
- durable state is written before an event is queued;
- business services never publish directly to a broker.

The prior plan named `IdempotencyStore.cs` and "outbox-before-queue tests" but did not name the mechanism that makes those tests executable. P-01 cannot close until the kernel has a concrete idempotency and outbox contract, and P-03 cannot close until the queue/cache providers can run those contracts in memory and in Docker.

## Decision

Use these platform contracts:

- `IIdempotencyStore` in `Xiigen.Kernel`.
- `IOutboxStore` in `Xiigen.Kernel`.
- `IOutboxDispatcher` plus `OutboxDispatcherHostedService` in the platform host.

Use these default providers:

- Local/unit tests: in-memory idempotency store, in-memory outbox store, and in-memory queue broker stub.
- Docker/local certification: Redis-backed idempotency through `ICacheFabric` using SET NX plus TTL, and durable outbox records in the approved document store provider.
- Elasticsearch provider option: versioned outbox documents with optimistic concurrency and a dispatcher lease field.
- Relational provider option: deferred. If approved later, it must use a
  transactional outbox table in the same database transaction as business state
  and must first update the dependency version-pin ADR to include EF Core or
  Dapper.

Application services must call platform persistence first, then write an outbox record, and must never call the queue provider directly. The dispatcher reads unsent outbox records, wraps or verifies `CloudEventEnvelope`, publishes through `IQueueFabric`, and marks records as sent or retry/dead-letter.

Runtime policies for default idempotency TTLs, outbox polling interval,
dispatcher batch size, lease duration, max attempts, `MicroserviceBase`, and
background-service cancellation are owned by
`ADR-DOTNET-KERNEL-RUNTIME-POLICIES.md`. P-01 must implement both ADRs as one
kernel unit.

Operational deployment, multi-instance leasing, dead-letter inspection,
requeue endpoints, and RabbitMQ/MassTransit resilience are owned by
`ADR-DOTNET-OPERATIONS-HEALTH-RESILIENCE.md`. P-01/P-03/P-09 must implement
the kernel contracts here together with that operational ADR:

- production dispatch runs in `src/Xiigen.Worker.Outbox`;
- API-hosted dispatch is limited to local or single-instance profiles;
- Elasticsearch leases use optimistic concurrency so two workers cannot
  publish the same record;
- exhausted records emit `xiigen.outbox.dead_lettered`;
- platform/admin operations endpoints inspect and requeue dead-letter records;
- MassTransit retry, reconnect, and error queue behavior follows the
  FREEDOM-backed policy in the operations ADR.

## Interface Sketches

These signatures are implementation-start contracts. They may be refined by
normal code review, but P-01 must not invent a different shape silently.

`IIdempotencyStore` never accepts a free-form tenant id. Providers read
`TenantContext` from `AsyncLocal<TenantContext>` and scope records by
tenant, operation, and idempotency key.

```csharp
public interface IIdempotencyStore
{
    Task<DataProcessResult<IdempotencyReservation>> TryReserveAsync(
        string operation,
        string idempotencyKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<IdempotencyRecord?>> GetAsync(
        string operation,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkSucceededAsync(
        string operation,
        string idempotencyKey,
        IdempotencyCompletion completion,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkFailedAsync(
        string operation,
        string idempotencyKey,
        string errorCode,
        bool retryable,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<int>> ReleaseExpiredAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}
```

Minimum record fields:

```text
tenantId               set from TenantContext, not method parameter
operation              stable operation name
idempotencyKey         caller-provided key
status                 reserved | succeeded | failed | expired
resultHash             optional hash/pointer for deterministic duplicate reply
errorCode              optional failure code
retryable              optional failure retry flag
reservedAt/expiresAt
completedAt
correlationId/causationId
concurrencyToken
```

`IOutboxStore` also reads tenant scope from `TenantContext`. Outbox records are
platform metadata records, not tenant-configurable business DTOs.

```csharp
public interface IOutboxStore
{
    Task<DataProcessResult<OutboxRecord>> AppendAsync(
        CloudEventEnvelope envelope,
        OutboxAppendOptions options,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<IReadOnlyList<OutboxRecord>>> LeaseBatchAsync(
        int maxBatchSize,
        TimeSpan leaseDuration,
        string dispatcherId,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkPublishedAsync(
        string outboxId,
        string brokerMessageId,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkRetryAsync(
        string outboxId,
        string errorCode,
        DateTimeOffset nextAttemptAt,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkDeadLetterAsync(
        string outboxId,
        string errorCode,
        string reason,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<int>> ReleaseExpiredLeasesAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

public interface IOutboxDispatcher
{
    Task<DataProcessResult<OutboxDispatchSummary>> DispatchOnceAsync(
        int maxBatchSize,
        CancellationToken cancellationToken = default);
}
```

Minimum outbox record fields:

```text
outboxId
tenantId               set from TenantContext, not method parameter
streamName
eventType
cloudEventId
cloudEventEnvelope
status                 pending | leased | published | retry | dead_letter
attemptCount
nextAttemptAt
leaseOwner
leaseUntil
publishedAt
brokerMessageId
lastErrorCode
lastErrorMessage
createdAt/updatedAt
correlationId/causationId
concurrencyToken
```

Elasticsearch providers store this as a versioned outbox document with
optimistic concurrency. Relational providers store the same fields in a
transactional outbox table.

## Acceptance Criteria

- Duplicate mutation with the same idempotency key returns the first result or a deterministic duplicate response.
- Duplicate queue message with the same idempotency key is ignored after the first successful handling.
- Tests prove state is persisted before outbox append and outbox append happens before queue publish.
- Dispatcher retry and dead-letter behavior is tested.
- Multi-instance dispatcher leasing is tested with two workers and one publish.
- Dead-letter inspection and requeue endpoints are tested through the
  operations API.
- Dispatcher polling and idempotency TTLs are resolved from FREEDOM config,
  not hard-coded at call sites.
- Dispatcher cancellation respects `stoppingToken` between batches and does
  not abandon leased records silently.
- Tenant id, correlation id, causation id, and CloudEvents metadata are preserved through outbox dispatch.
- Direct queue provider imports in application services fail the DNA/fabric guard.

## Consequences

This makes DNA-7 and DNA-8 executable in the .NET stack instead of only declarative. It also gives FLOW-39 and other queue-touching flows a broker stub for contract tests without requiring a live broker.

