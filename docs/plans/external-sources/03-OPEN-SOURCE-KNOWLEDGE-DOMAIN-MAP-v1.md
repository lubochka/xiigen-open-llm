# Open Source Technology Knowledge Domain Map v1
## Maps every source ID from the registry to the eight XIIGEN domains
## Date: 2026-05-04

---

## Domain A — Topology and Orchestration

Sources:
- KUBERNETES-KEPS: rollout strategies (rolling update, blue/green, canary), StatefulSet volume management, readiness/liveness probe design, workload scheduling
- OPENTELEMETRY-SPEC: distributed trace topology, span parent-child relationships, context propagation across process boundaries
- MICROSOFT-LEARN-ARCH: circuit breaker state machine, bulkhead isolation, queue-based load leveling topology, strangler fig migration topology, sidecar co-location pattern
- KAFKA-KIPS: distributed broker topology decisions, ZooKeeper removal (KIP-500), consumer group rebalance protocol (KIP-848)
- DOTNET-MICROSERVICES: microservice topology, service decomposition, API gateway patterns
- SYSTEM-DESIGN-PRIMER: distributed system topology, load balancing, caching topology, sharding strategies
- AOSA-VOL1: Nginx event loop vs. thread-per-connection architecture, nginx upstream topology
- AOSA-VOL2: distributed architecture decisions in production systems

Dataset row types from this domain:
- circuit-breaker-decision rows (open/half-open/closed)
- bulkhead-isolation rows (shared pool vs. isolated pool)
- migration-strategy rows (big-bang vs. strangler vs. blue-green)
- rollout-strategy rows (rolling vs. canary vs. blue-green)
- process-boundary-propagation rows (trace context, auth token, tenant scope)

Arbiters sourced here:
- circuitBreakerArbiter
- bulkheadIsolationArbiter
- stranglerMigrationArbiter
- protocolMigrationArbiter
- dependencyCycleArbiter (from XIIGEN DependencyGraph invariant)

---

## Domain B — Routing and Selection

Sources:
- OPENAPI-SPEC: API schema routing, content negotiation, schema composition (allOf, oneOf, anyOf), parameter binding rules
- GOOGLE-AIP: resource naming and routing conventions, standard methods (Get, List, Create, Update, Delete), custom methods
- MICROSOFT-LEARN-ARCH: API gateway routing, sidecar routing, load balancer selection
- NESTJS-ADVANCED: guard-based routing, interceptor chain, pipe validation before routing decision
- ANGULAR-STYLE: dependency injection routing, module-level vs. component-level routing decisions
- PHP-FIG-PSRS: PSR-7 HTTP message routing, PSR-11 container-based dependency resolution
- SYSTEM-DESIGN-PRIMER: DNS routing, CDN routing, load balancer algorithm selection

Dataset row types from this domain:
- API-schema routing rows (which schema object applies given a request shape)
- resource-naming validation rows (valid vs. invalid resource name patterns)
- pagination-strategy rows (cursor vs. offset selection)
- routing-table decision rows

Arbiters sourced here:
- apiVersioningArbiter
- idempotentCreateArbiter
- longRunningOpArbiter

---

## Domain C — Semantic Translation

Sources:
- PYTHON-PEPS: language evolution decisions, PEP translation of user intent into language specification
- OPENAPI-SPEC: schema translation between request formats and canonical form
- REACT-ADVANCED: state shape translation, derived state computation
- EFFECTIVE-GO: translation of concurrent patterns into idiomatic Go
- AOSA-500L: minimal implementation translation examples

Dataset row types from this domain:
- format-translation rows (user-facing format to internal format)
- schema-coercion rows (input schema to canonical schema)
- language-pattern rows (pattern in one language expressed in another)

---

## Domain D — Execution and Applier

Sources:
- SWE-BENCH: issue-to-patch execution rows, test suite execution, patch application to repository
- DOCKER-BEST-PRACTICES: container build execution, layer caching, multi-stage build patterns
- GITHUB-ACTIONS-ADVANCED: CI/CD pipeline execution, reusable workflow composition, matrix build execution
- NESTJS-ADVANCED: request execution pipeline (guard → interceptor → pipe → handler → interceptor → filter)
- REACT-ADVANCED: effect execution lifecycle, state update batching
- DOTNET-ASPNET-PERF: request execution pipeline, middleware order, async execution patterns
- EFCORE-CONCURRENCY: database operation execution, optimistic concurrency, query execution plans
- RUST-ASYNC: async execution, Future polling, task scheduling
- EFFECTIVE-GO: goroutine execution, channel communication, select statement patterns
- AOSA-500L: minimal applier execution examples

Dataset row types from this domain:
- execution-sequence rows (steps to apply an operation)
- patch-application rows (SWE-bench issue to code change)
- build-execution rows (Docker, CI/CD)
- concurrency-execution rows (goroutine, async/await patterns)

---

## Domain E — Arbiter and Quality

Sources:
- OWASP-ASVS: security verification requirements — 14 chapters, 286 requirements, each with PASS/FAIL criteria
- OWASP-CHEAT-SHEETS: secure implementation checklists, per-technology security rules
- CWE-TOP25: 25 weakness entries, each with a concrete flaw description, consequence, and mitigation
- IETF-RFC-OAUTH2: OAuth 2.0 security considerations section, grant type requirements
- IETF-RFC-JWT: JWT validation algorithm, required claim validation, security considerations
- IETF-RFC-TLS13: TLS version and cipher suite requirements, prohibited configurations
- KUBERNETES-KEPS: graduation criteria as quality gates (alpha → beta → stable)
- OPENTELEMETRY-SPEC: semantic convention conformance, span attribute naming conventions
- GOOGLE-AIP: API design quality checks (AIP linter rules), resource naming conformance
- GCP-ARCH-FRAMEWORK: architecture quality pillars (operational excellence, security, reliability)
- NESTJS-ADVANCED: guard-based access control checks, exception filter classification

Dataset row types from this domain (highest priority — families 5):
- security-control verification rows (ASVS requirements as arbiter rules)
- vulnerability-pattern rows (CWE entries as detection rules)
- protocol-compliance rows (RFC requirements as conformance checks)
- API-quality rows (AIP linter rule violations)
- graduation-criteria rows (KEP readiness gates as quality arbiters)

Arbiters sourced here (all security/governance arbiters):
- jwtValidationArbiter
- oauthGrantArbiter
- passwordStorageArbiter
- sqlInjectionArbiter
- secretPresenceArbiter
- tenantIsolationArbiter
- tlsConfigArbiter
- fileUploadArbiter
- tracePropagationArbiter
- apiVersioningArbiter
- proposalCompletenessArbiter

---

## Domain F — Registry and Storage

Sources:
- MICROSOFT-LEARN-ARCH / CQRS: command/query model separation, read model synchronization
- MICROSOFT-LEARN-ARCH / Event Sourcing: append-only event log, snapshot strategy, temporal queries
- MICROSOFT-LEARN-ARCH / Queue-based Load Leveling: queue as a buffer between producer and consumer
- DOTNET-MICROSERVICES: transactional outbox, event-based communication, database-per-service
- OPENAPI-SPEC: API schema as a registry contract (schema objects, component references)
- KAFKA-KIPS: offset management, consumer group state storage, log compaction
- CLOUDEVENTS-SPEC: event schema registry, event attribute governance
- EFCORE-CONCURRENCY: optimistic concurrency tokens, rowversion, concurrency conflict resolution
- AOSA-VOL2: database architecture decisions (PostgreSQL MVCC, SQLite WAL mode)

Dataset row types from this domain:
- CQRS-fit rows (when to apply CQRS vs. simple model)
- event-sourcing rows (append-only vs. mutable state decisions)
- outbox-pattern rows (atomic write + event publish)
- schema-registry rows (API schema versioning and compatibility)

Arbiters sourced here:
- cqrsFitArbiter
- eventSourcingFitArbiter
- transactionalOutboxArbiter
- eventStoreImmutabilityArbiter

---

## Domain G — Learning and Training

Sources:
- NIST-SSDF: secure development lifecycle practices, training requirements for developers, PO.2 "Implement Roles and Responsibilities"
- SWE-BENCH: agent learning from real repository issues — 2,294 training examples
- KUBERNETES-KEPS: KEP lifecycle as a learning signal (accepted/rejected/deferred classification)
- RUST-RFCS: proposal-outcome pairs as training signal for governance decisions
- PYTHON-PEPS: PEP acceptance history as training signal
- AOSA-VOL1, AOSA-VOL2: architecture narrative as long-form training material
- DOTNET-MICROSERVICES: architecture guide as structured learning source

Dataset row types from this domain:
- training-lifecycle rows (what triggers retraining, what constitutes sufficient data)
- learning-signal rows (issue-to-patch pairs from SWE-bench)
- proposal-outcome pairs (accepted/rejected governance decisions)

---

## Domain H — Governance and Contracts

Sources:
- NIST-SSDF: 4 practice groups (PO, PS, PW, RV) — organizational governance practices
- KUBERNETES-KEPS: KEP acceptance process, graduation criteria, deprecation policy
- OPENTELEMETRY-SPEC: semantic convention governance, signal stability levels (experimental, stable, deprecated)
- OPENAPI-SPEC: schema backward compatibility rules, deprecation fields
- GOOGLE-AIP: API design governance, versioning policy, resource lifecycle management
- MICROSOFT-LEARN-ARCH: architecture pattern lifecycle, pattern applicability governance
- IETF-RFCS: protocol governance, RFC lifecycle (proposed standard → internet standard), update/obsolete chain
- OWASP-ASVS: security standard governance, verification requirement lifecycle
- RUST-RFCS: language governance, RFC acceptance process, disposition rules
- PYTHON-PEPS: PEP governance, BDFL (Benevolent Dictator for Life) and steering council decisions
- GO-PROPOSALS: Go governance, proposal review and acceptance
- KAFKA-KIPS: KIP governance, backward compatibility policy
- CLOUDEVENTS-SPEC: spec governance, attribute lifecycle (required/optional/extension)
- GITHUB-ACTIONS-ADVANCED: CI/CD governance, reusable workflow policy, secret management governance

Dataset row types from this domain:
- governance-decision rows (when a process change is approved vs. blocked)
- lifecycle rows (what triggers deprecation, sunset, removal)
- supply-chain governance rows (SBOM, provenance, dependency approval)
- protocol-compliance rows (RFC MUST/SHOULD requirements as governance gates)

Arbiters sourced here:
- supplyChainArbiter
- proposalCompletenessArbiter
- backwardCompatibilityArbiter
- protocolMigrationArbiter
- specAttributePromotionArbiter
- secretPresenceArbiter
- apiVersioningArbiter
- tracePropagationArbiter (observability as a governance contract)

---

## Source Coverage Matrix

| Source ID | A | B | C | D | E | F | G | H |
|-----------|---|---|---|---|---|---|---|---|
| KUBERNETES-KEPS | ✓ | | | | ✓ | | ✓ | ✓ |
| OPENTELEMETRY-SPEC | ✓ | | | | ✓ | | | ✓ |
| OPENAPI-SPEC | | ✓ | | | | ✓ | | ✓ |
| GOOGLE-AIP | | ✓ | | | ✓ | | | ✓ |
| MICROSOFT-LEARN-ARCH | ✓ | | | | ✓ | ✓ | | ✓ |
| OWASP-ASVS | | | | | ✓ | | | ✓ |
| OWASP-CHEAT-SHEETS | | | | | ✓ | | | ✓ |
| NIST-SSDF | | | | | | | ✓ | ✓ |
| CWE-TOP25 | | | | | ✓ | | | ✓ |
| IETF-RFC-OAUTH2 | | ✓ | | | ✓ | | | ✓ |
| IETF-RFC-JWT | | | | | ✓ | | | ✓ |
| IETF-RFC-TLS13 | | | | | ✓ | | | ✓ |
| SWE-BENCH | ✓ | | | ✓ | ✓ | | ✓ | |
| RUST-RFCS | | | | | ✓ | | ✓ | ✓ |
| PYTHON-PEPS | | | ✓ | | | | ✓ | ✓ |
| GO-PROPOSALS | | | | | | | | ✓ |
| KAFKA-KIPS | ✓ | | | | | ✓ | | ✓ |
| CLOUDEVENTS-SPEC | ✓ | | | | | ✓ | | ✓ |
| AOSA-VOL1 | ✓ | | | ✓ | | | ✓ | |
| AOSA-VOL2 | ✓ | | | ✓ | | ✓ | ✓ | |
| AOSA-500L | | | ✓ | ✓ | | | | |
| DOTNET-MICROSERVICES | ✓ | | | | | ✓ | ✓ | |
| SYSTEM-DESIGN-PRIMER | ✓ | ✓ | | | | | | |
| EFFECTIVE-GO | ✓ | | ✓ | ✓ | | | | |
| RUST-ASYNC | ✓ | | | ✓ | | | | |
| REACT-ADVANCED | | | ✓ | ✓ | | | | |
| ANGULAR-STYLE | | ✓ | | ✓ | | | | |
| DOTNET-ASPNET-PERF | | | | ✓ | ✓ | | | |
| EFCORE-CONCURRENCY | | | | ✓ | | ✓ | | |
| NESTJS-ADVANCED | | ✓ | | ✓ | ✓ | | | |
| PHP-FIG-PSRS | | ✓ | | | | ✓ | | |
| DOCKER-BEST-PRACTICES | ✓ | | | ✓ | | | | |
| GITHUB-ACTIONS-ADVANCED | | | | ✓ | | | | ✓ |
| GCP-ARCH-FRAMEWORK | | | | | ✓ | | | ✓ |

**Acceptance gate:** Every source ID in Section 1 of EXTERNAL-SOURCE-REGISTRY-v1.0.md appears in at least one domain column. Gate PASSED.
