# Source Examination: Distributed Data and Cloud Patterns
## Sources: MICROSOFT-LEARN-ARCH (CQRS, Event Sourcing, Saga, Outbox, Queue-based Load Leveling, Compensating Transaction)
## Priority: 2 (families 1, 2, 5, 8)
## Date examined: 2026-05-04

---

## 1. Sources Examined

| Source ID | Title | Trust tier | Why included |
|-----------|-------|-----------|-------------|
| MICROSOFT-LEARN-ARCH / CQRS | CQRS pattern | 1 | CC-BY 4.0; clear When to use vs. When to avoid guidance with concrete failure modes |
| MICROSOFT-LEARN-ARCH / Event Sourcing | Event sourcing pattern | 1 | CC-BY 4.0; architectural decision with tradeoffs table and explicit when-not-to-use criteria |
| MICROSOFT-LEARN-ARCH / Queue-based Load Leveling | Queue-based load leveling | 1 | CC-BY 4.0; load topology decision with sizing guidance |
| MICROSOFT-LEARN-ARCH / Compensating Transaction | Compensating transaction pattern | 1 | CC-BY 4.0; rollback decision for distributed operations |
| MICROSOFT-LEARN-ARCH / Sidecar | Sidecar pattern | 1 | CC-BY 4.0; co-location topology decision |
| DOTNET-MICROSERVICES | .NET Microservices Architecture Guide — chapters on data ownership and CQRS | 1 | CC-BY 4.0; detailed implementation guidance for distributed data patterns |

---

## 2. Architecture Challenges Found

| Challenge ID | Challenge statement | Source anchor | XIIGEN domain |
|-------------|--------------------|--------------|--------------| 
| DATA-01 | A service needs both fast reads and complex writes; a single data model is too slow for one or the other | MICROSOFT-LEARN-ARCH CQRS §Context and problem | F_REGISTRY_STORAGE |
| DATA-02 | A saga needs to undo a previously committed step because a later step failed; the team must decide which compensation strategy to apply | MICROSOFT-LEARN-ARCH Compensating Transaction §Context and problem | A_TOPOLOGY |
| DATA-03 | A service must publish a domain event after a database write, but a publish failure after a successful write would leave state inconsistent | DOTNET-MICROSERVICES §Implementing event-based communication (outbox pattern) | F_REGISTRY_STORAGE |
| DATA-04 | An upstream service bursts requests to a downstream service that cannot scale proportionally; the team must decide between synchronous throttling and a queue buffer | MICROSOFT-LEARN-ARCH Queue-based Load Leveling §Context and problem | A_TOPOLOGY |
| DATA-05 | An audit log must be reconstructible from stored events; the team must decide between storing current state only versus an append-only event log | MICROSOFT-LEARN-ARCH Event Sourcing §Context and problem | F_REGISTRY_STORAGE |
| DATA-06 | A cross-service operation spans three services; one service fails midway; the remaining services must coordinate rollback without a distributed lock | MICROSOFT-LEARN-ARCH Compensating Transaction §Issues and considerations | A_TOPOLOGY |
| DATA-07 | A Kubernetes pod needs a log-forwarding sidecar; the sidecar must share the pod's network namespace but be independently deployable | MICROSOFT-LEARN-ARCH Sidecar §Context and problem | A_TOPOLOGY |
| DATA-08 | A CQRS read model is out of date; a user query returns stale results; the arbiter must decide whether eventual consistency is acceptable or a synchronous read-model refresh is required | MICROSOFT-LEARN-ARCH CQRS §Issues and considerations | F_REGISTRY_STORAGE |
| DATA-09 | A service currently has a single large aggregate that mixes command and query concerns; a refactor is needed without breaking the API | DOTNET-MICROSERVICES §Applying CQRS; MICROSOFT-LEARN-ARCH CQRS §When to use | F_REGISTRY_STORAGE |
| DATA-10 | An event-sourced aggregate needs to be reconstructed from a long event stream on every request; the reconstruction time is becoming the bottleneck | MICROSOFT-LEARN-ARCH Event Sourcing §Issues and considerations (snapshotting) | F_REGISTRY_STORAGE |

---

## 3. Patterns Found

| Pattern ID | Pattern name | When to use | When NOT to use | Source anchor |
|-----------|-------------|------------|----------------|--------------|
| PAT-DATA-01 | CQRS (Command Query Responsibility Segregation) | Use when read and write workloads have different scalability or consistency requirements; use when collaborative domain models need separate optimization | Do not use for simple CRUD (Create Read Update Delete) applications; the separate read/write model adds complexity that is not justified without distinct load profiles | MICROSOFT-LEARN-ARCH CQRS §When to use |
| PAT-DATA-02 | Event sourcing | Use when you need a complete audit trail; use when the business requires temporal queries ("what was the state at time T?"); use when event replay is a required recovery mechanism | Do not use when you only need the current state; the event replay cost and snapshot strategy add complexity that is not justified for simple state storage | MICROSOFT-LEARN-ARCH Event Sourcing §When to use |
| PAT-DATA-03 | Queue-based load leveling | Use when the production rate of a component is unpredictably higher than the processing rate; use when the downstream service cannot scale as fast as the burst rate | Do not use when low-latency is required — the queue adds latency; do not use when the downstream service can autoscale fast enough to absorb bursts | MICROSOFT-LEARN-ARCH Queue-based Load Leveling §When to use |
| PAT-DATA-04 | Compensating transaction | Use when a saga step cannot be atomically included in a database transaction; use when you need semantic undo rather than database rollback | Do not use when all steps can be included in a single database transaction — use database ACID transactions instead | MICROSOFT-LEARN-ARCH Compensating Transaction §When to use |
| PAT-DATA-05 | Transactional outbox | Use when a service must atomically update its own database and publish an event to a message broker | Do not use when you can afford two-phase commit across the database and the broker — though in practice this is almost always more expensive than the outbox pattern | DOTNET-MICROSERVICES §Implementing event-based communication |
| PAT-DATA-06 | Event sourcing with snapshots | Use when event stream reconstruction time has become measurable and affects response time | Do not take snapshots too frequently — snapshot storage cost and snapshot correctness guarantees must be maintained; every snapshot format change is a migration | MICROSOFT-LEARN-ARCH Event Sourcing §Issues and considerations |

---

## 4. Anti-Patterns Found

| Anti-pattern ID | Bad design | Failure mode | Better design | Source anchor |
|----------------|-----------|-------------|--------------|--------------|
| ANTI-DATA-01 | CQRS on a simple CRUD application | The write model and read model must be kept in sync; any synchronization lag is visible to users; the complexity is not justified | Use a single data model with a standard REST or repository pattern; add CQRS only when distinct read and write optimizations are needed | MICROSOFT-LEARN-ARCH CQRS §When not to use |
| ANTI-DATA-02 | Publishing an event before the database write commits | If the database write fails after the event is published, the event represents state that never existed | Use the transactional outbox pattern — write to the outbox table in the same transaction as the business data; publish from the outbox only after the transaction commits | DOTNET-MICROSERVICES §Implementing event-based communication |
| ANTI-DATA-03 | Synchronous downstream call during a burst | The downstream service cannot process requests as fast as they arrive; the upstream service experiences timeouts and latency spikes that cascade | Buffer requests in a queue; the downstream service consumes from the queue at its own rate; the upstream service returns immediately after enqueue | MICROSOFT-LEARN-ARCH Queue-based Load Leveling §Context and problem |
| ANTI-DATA-04 | Missing compensation in a multi-step saga | Step 3 of a 5-step saga fails; steps 1 and 2 have already committed; without compensation they remain committed and leave the system in an inconsistent state | Define a compensating action for every saga step that can commit before the saga completes; compensation actions must be idempotent | MICROSOFT-LEARN-ARCH Compensating Transaction §Issues and considerations |
| ANTI-DATA-05 | Deleting events from an event store | The audit trail is broken; temporal queries fail; event replay cannot reconstruct past states | Event stores are append-only by design; mark superseded events as replaced, not deleted; use soft deletion if removal is legally required | MICROSOFT-LEARN-ARCH Event Sourcing §Issues and considerations |
| ANTI-DATA-06 | Stale read model without staleness indication | A CQRS read model that is minutes behind the write model returns stale data; the user sees inconsistent state but the UI gives no indication | Expose a staleness timestamp on the read model; allow callers to specify a consistency requirement; provide a synchronous read path for cases that cannot tolerate staleness | MICROSOFT-LEARN-ARCH CQRS §Issues and considerations |

---

## 5. Arbiter Rules Extracted

| Arbiter ID | What it checks | Positive signal | Negative signal | Source anchor |
|-----------|--------------|----------------|----------------|--------------|
| cqrsFitArbiter | CQRS is applied only where its complexity is justified | Service has distinct read and write load profiles; read model is denormalized for query performance; write model enforces business rules; staleness is acceptable or managed | CQRS is applied to a simple CRUD service; single-table read and write; no distinct scalability requirement between reads and writes | MICROSOFT-LEARN-ARCH CQRS §When to use / When not to use |
| eventSourcingFitArbiter | Event sourcing is applied only where its benefits justify its cost | Service requires full audit trail; temporal queries are a business requirement; event replay is a recovery mechanism; snapshotting strategy is defined | Event sourcing applied for audit only when a simple audit log table would suffice; no snapshot strategy; event stream reconstruction time is unbounded | MICROSOFT-LEARN-ARCH Event Sourcing §When to use / Issues and considerations |
| transactionalOutboxArbiter | Domain events are published atomically with the business state change | Outbox table is written in the same transaction as the business data; event relay publishes from outbox only after commit; consumer idempotency key is present | Event is published directly to broker after database write with no atomicity guarantee; event publish failure after database commit leaves an unpublished event | DOTNET-MICROSERVICES §Implementing event-based communication |
| sagaCompensationArbiter | Every committed saga step has a defined compensation action | Every step that can commit before the saga ends has a documented and tested compensation action; compensation actions are idempotent | One or more saga steps have no compensation action; the saga can reach an inconsistent committed state with no recovery path | MICROSOFT-LEARN-ARCH Compensating Transaction |
| queueLoadLevelingArbiter | Synchronous calls to burst-prone downstream services are buffered through a queue | Upstream enqueues and returns immediately; downstream consumes at its own rate; queue depth is monitored; back-pressure signal is defined | Upstream calls downstream synchronously; timeout is the only load control; burst causes direct latency increase and timeout failures | MICROSOFT-LEARN-ARCH Queue-based Load Leveling |
| eventStoreImmutabilityArbiter | Event store entries are never deleted or modified | All events are append-only; no UPDATE or DELETE on the events table; superseded events are marked replaced by a newer event, not removed | Events are deleted when their aggregate is deleted; events are updated when the business rule changes; audit trail is not complete | MICROSOFT-LEARN-ARCH Event Sourcing §Issues and considerations |

---

## 6. Candidate Training Rows

| Row ID | Dataset family | Domain | Status |
|--------|--------------|--------|-------|
| EXT-DATA-001 | 5 — arbitrateResultI_with_resultsContext | F_REGISTRY_STORAGE | draft — CQRS fit arbiter |
| EXT-DATA-002 | 5 — arbitrateResultI_with_resultsContext | F_REGISTRY_STORAGE | draft — event sourcing fit arbiter |
| EXT-DATA-003 | 5 — arbitrateResultI_with_resultsContext | F_REGISTRY_STORAGE | draft — transactional outbox arbiter |
| EXT-DATA-004 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — saga compensation arbiter |
| EXT-DATA-005 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — queue load leveling arbiter |
| EXT-DATA-006 | 5 — arbitrateResultI_with_resultsContext | F_REGISTRY_STORAGE | draft — event store immutability arbiter |
| EXT-DATA-007 | 8 — historyAndVersions_to_reprocessingDecision | A_TOPOLOGY | draft — saga step failure compensation decision |
| EXT-DATA-008 | 8 — historyAndVersions_to_reprocessingDecision | F_REGISTRY_STORAGE | draft — stale read model refresh decision |
| EXT-DATA-009 | 1 — userPrompt_to_machineCommands | F_REGISTRY_STORAGE | draft — CQRS vs. simple model selection |
| EXT-DATA-010 | 1 — userPrompt_to_machineCommands | F_REGISTRY_STORAGE | draft — event sourcing vs. audit log selection |

---

## 7. Gaps

- Microsoft Learn Saga pattern page (Chris Richardson microservices.io content is removed; MS Learn has its own saga content) — needed for orchestration vs. choreography rows
- DOTNET-MICROSERVICES §Resiliency patterns — needed for retry/circuit-breaker rows in .NET context
- DOTNET-MICROSERVICES §Testing microservices — needed for integration test arbiter rows
