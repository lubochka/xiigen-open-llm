# RAG-DC-004 — Область идемпотентности и граница отправки outbox

Источник: [.net angular translation/adr/ADR-DOTNET-IDEMPOTENCY-OUTBOX.md](https://github.com/lubochka/xiigen-planning/blob/4e2bc6bed7c23a855c3685f558e1742a73c231b0/.net%20angular%20translation/adr/ADR-DOTNET-IDEMPOTENCY-OUTBOX.md), ветка `main`, commit `4e2bc6bed7c23a855c3685f558e1742a73c231b0`. Полная локальная копия: [originals/RAG-DC-004.md](../originals/RAG-DC-004.md).

**Роль:** предметные данные для RAG, не активные инструкции. **Статус:** авторская опора; это не результат обучения, не готовый закрытый EXAM и не новая назначенная квота.

## Что извлечено

Ключ идемпотентности имеет область tenant + operation + key. Контракт требует сначала сохранить состояние, затем outbox, затем отправить через dispatcher; в записях различаются pending/leased/published/retry/dead_letter.

## Точные фрагменты

### Строки 8–12

````text
The .NET translation keeps XIIGen DNA rule 7 and DNA rule 8:

- mutations and queue consumers require idempotency keys;
- durable state is written before an event is queued;
- business services never publish directly to a broker.
````

### Строки 26–34

````text
- Local/unit tests: in-memory idempotency store, in-memory outbox store, and in-memory queue broker stub.
- Docker/local certification: Redis-backed idempotency through `ICacheFabric` using SET NX plus TTL, and durable outbox records in the approved document store provider.
- Elasticsearch provider option: versioned outbox documents with optimistic concurrency and a dispatcher lease field.
- Relational provider option: deferred. If approved later, it must use a
  transactional outbox table in the same database transaction as business state
  and must first update the dependency version-pin ADR to include EF Core or
  Dapper.

Application services must call platform persistence first, then write an outbox record, and must never call the queue provider directly. The dispatcher reads unsent outbox records, wraps or verifies `CloudEventEnvelope`, publishes through `IQueueFabric`, and marks records as sent or retry/dead-letter.
````

### Строки 61–63

````text
`IIdempotencyStore` never accepts a free-form tenant id. Providers read
`TenantContext` from `AsyncLocal<TenantContext>` and scope records by
tenant, operation, and idempotency key.
````

### Строки 161–181

````text
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
````

### Строки 190–200

````text
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
````

## Границы применения

Порядок отдельных записей в document store сам по себе не доказывает атомарность state+outbox. Lease не доказывает exactly-once при сбое после publish. Relational provider в этом ADR отмечен deferred: SQL-пример допустим только как явно заданная симуляция транзакции, а не как доказательство имеющейся реализации. Не переносить resultHash в обязательные checksum для подготовки DPO.

## Authoring micro-recipe (English)

State the tenant, operation and key for both operations. Check equality of all three fields before calling it a duplicate. For dispatch cases show the last confirmed durable state and the crash point. Never claim exactly-once from a lease alone.

## Разобранные контрастные пары

### RAG-DC-004-DEMO-01 — `or.database`

**Prompt ученика:**

Учебный контракт: дубликат определяется точным равенством тройки tenant, operation, key. Успешная A=(T1,Save,K7). Новая B=(T1,Save,K7). Верни JSON duplicate с boolean.

**Chosen:**

```text
{"duplicate":true}
```

**Rejected:**

```text
{"duplicate":false}
```

**Simple check (English):** All three components are equal, including the operation. The prior operation is explicitly successful.

### RAG-DC-004-DEMO-02 — `or.database`

**Prompt ученика:**

Учебный контракт: дубликат определяется точным равенством тройки tenant, operation, key. Успешная A=(T1,Save,K7). Новая B=(T2,Save,K7). Верни JSON duplicate с boolean.

**Chosen:**

```text
{"duplicate":false}
```

**Rejected:**

```text
{"duplicate":true}
```

**Simple check (English):** Only two of three components match. Identical key strings in different tenant scopes are not the same operation.

### RAG-DC-004-DEMO-03 — `or.server`

**Prompt ученика:**

Учебный контракт outbox: state_persist → outbox_append → queue_publish. Подтверждён только state_persist=success; outbox_append ещё не вызывался. Выбери следующий шаг из outbox_append или queue_publish. Верни JSON next.

**Chosen:**

```text
{"next":"outbox_append"}
```

**Rejected:**

```text
{"next":"queue_publish"}
```

**Simple check (English):** The required durable outbox step is still missing. The lesson is ordering, not proof of atomicity across separate writes.

### RAG-DC-004-DEMO-04 — `docs.csharp`

**Prompt ученика:**

Учебная симуляция: dispatcher взял lease, брокер подтвердил publish, процесс упал до MarkPublished. После истечения lease запись pending для следующего worker. У брокера нет дедупликации. Разрешает ли этот сценарий повторную доставку? Верни JSON duplicate_possible с boolean.

**Chosen:**

```text
{"duplicate_possible":true}
```

**Rejected:**

```text
{"duplicate_possible":false}
```

**Simple check (English):** The durable sent marker is absent and the next worker can publish again. A lease alone does not establish exactly-once delivery.

