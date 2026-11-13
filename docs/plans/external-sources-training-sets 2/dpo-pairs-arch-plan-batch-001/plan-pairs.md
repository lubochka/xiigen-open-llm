# PLAN Pairs — Multi-branch Routing and Dependency Order
# Batch: dpo-pairs-arch-plan-batch-001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Domain: PLAN — naming branches, ordering them, stating dependencies
# Pair count: 6

---

## Pair AP1-PLAN-001 — multi-task plan with reversed dependency order

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-001
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-B-wrong-dependency-order
chosen_branches: ["3.1", "3.4", "5.2"]
rejected_branches: ["3.1", "3.4", "5.2"]
```

Prompt: `"add per-tenant API rate limiting to the gateway and ship integration tests that prove tenant A cannot exhaust tenant B's budget"`

```
CHOSEN — PLAN, correct order:

Branch 1 — 3.1 Architecture and Component Topology (dependency order 1):
  Decide where rate limiting lives (the v1 ARCH guidance pair AP1-3.1-003 is
  applicable). Decide that per-tenant limits belong at the gateway and that
  the gateway must read tenant identity from the validated token.
  Decide that the gateway must not import auth-domain code; tenant identity
  arrives via the token claim only.
  Output: a one-page boundary document.

Branch 2 — 3.4 Security and Authorization Model (dependency order 2,
depends on Branch 1):
  Define how the gateway extracts tenant identity from the token. Define what
  happens when the token is missing (anonymous bucket), invalid (rejected
  before rate limit), or claims a tenant that does not exist (rejected with
  audit log entry).
  Output: the rate-limit-key derivation rule and the failure-mode table.
  Dependency: Branch 2 begins after Branch 1 fixes the boundary that the
  gateway holds the limiting state.

Branch 3 — 5.2 Integration Test Design (dependency order 3,
depends on Branch 2):
  Design integration tests that exhaust Tenant A's budget and verify that
  Tenant B's requests succeed during the same window. Tests issue real tokens
  for two distinct tenants through the real auth path; mock tokens are
  forbidden because the test is verifying tenant isolation, which depends on
  the real token validation.
  Test catalog: budget-exhaustion test, cross-tenant fairness test,
  per-tenant isolation under load.
  Dependency: Branch 3 cannot begin until the rate-limit-key rule from
  Branch 2 is finalized — the test infrastructure is built around that key.

REJECTED — PLAN, reversed order:

Branch 1 — 5.2 Integration Test Design (dependency order 1):
  Write the integration tests first. Use mock JWT tokens with a tenant_id
  claim. Send N+1 requests to the gateway with mock tokens for Tenant A and
  assert the (N+1)th request returns 429. Send a request with a mock token
  for Tenant B in the same window and assert it succeeds.

Branch 2 — 3.4 Security and Authorization Model (dependency order 2):
  Define the JWT structure to match the mock tokens used in the tests.
  Implement the validation logic so the tests pass.

Branch 3 — 3.1 Architecture and Component Topology (dependency order 3):
  Document where rate limiting lives in the architecture diagram, after
  the implementation has settled.
[WRONG: the rejected order tests cross-tenant isolation with mock tokens.
Mock tokens bypass the real validation path — the tests would pass even
if the gateway accepted unsigned tokens or trusted the tenant_id claim
without verification. The integration tests for tenant isolation must run
against the real auth path; that path must exist before the tests are
designed. Reversing the order produces tests that pass while the real
isolation is broken. Architecture documented retroactively is not the
boundary decision the chosen order requires — it is a write-up of whatever
implementation happened to ship.]
```

---

## Pair AP1-PLAN-002 — planning prompt confused with conceptual explanation

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-002
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-C-wrong-domain
chosen_domain: PLAN
rejected_domain: CODE_Q
```

Prompt: `"how should we structure background job processing for a multi-tenant SaaS — we have email sending, image processing, and report generation"`

```
CHOSEN — PLAN, branches 3.1 and 3.2:

Branch 1 — 3.1 Architecture and Component Topology (dependency order 1):
  Decide whether the three job kinds share one queue or have separate queues.
  Decide where tenant identity lives in the job envelope.
  Decide whether jobs can call cross-tenant data (no — the worker reads
  tenant context from the job envelope and is forbidden to execute under
  another tenant's identity).
  Output: a topology with one queue per priority class (high/normal/low)
  and a dispatcher that routes by job kind to the worker pool that owns it.
  Dependency: this branch precedes everything because the queue topology
  determines the data contract every other branch produces.

Branch 2 — 3.2 Data Model and Persistence Design (dependency order 2,
depends on Branch 1):
  Define the job envelope: jobId, tenantId, jobKind, priorityClass,
  scheduledAtUtc, attempt_count, payload (kind-specific), idempotencyKey.
  Define the persistence rule: jobs are persisted in the queue itself, not
  in a side database. A worker that crashes mid-job releases the message
  back to the queue with attempt_count incremented.
  Define the cross-tenant rule at storage level: per-tenant queue budgets
  with explicit overflow behavior.
  Dependency: starts after the queue topology decides one-queue-per-priority.

REJECTED — CODE_Q (conceptual explanation, not a plan):

Background job processing in multi-tenant SaaS typically uses one of three
patterns. Pattern A: one shared queue with all tenants and all job kinds,
with priority and tenant signaling on each message. This is the simplest to
operate but a noisy tenant can starve the others. Pattern B: per-tenant
queues — strong isolation, more operational complexity. Pattern C: per-job-
kind queues with tenant priority on each message — a middle ground.
Email-sending jobs are typically I/O-bound and short-running, so a small
worker pool with high concurrency works well. Image processing is CPU-bound
and benefits from dedicated workers with limited concurrency to avoid
saturation. Report generation is long-running and benefits from a separate
worker pool that does not block faster jobs.
[WRONG: "how should we structure" asks for a plan with named branches,
ordered dependencies, and concrete decisions. The rejected response is a
conceptual survey of patterns and considerations — that is CODE_Q (explaining
how things work), not PLAN (deciding what to build and in what order). A
plan names branches in the topology vocabulary (3.1, 3.2 etc.), assigns a
dependency order, and states what each branch produces. The conceptual
survey leaves all decisions undone.]
```

---

## Pair AP1-PLAN-003 — multi-task plan missing the dependency constraint

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-003
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-B-missing-dependency
chosen_branches: ["3.5", "3.1", "4.1"]
rejected_branches: ["3.5", "3.1", "4.1"]
```

Prompt: `"design and implement the new fraud detection pipeline including the alerting and dashboard"`

```
CHOSEN — PLAN, dependencies stated:

Branch 1 — 3.1 Architecture and Component Topology (dependency order 1,
no dependencies):
  Define component boundaries: TransactionIngestor, FeatureExtractor,
  ScoreEvaluator, AlertEmitter. Define the dependency direction: each
  component reads from the previous one's output queue. Forbidden: any
  reverse calls.

Branch 2 — 3.5 Observability and Operational Design (dependency order 2,
depends on Branch 1):
  Define the metrics, traces, and alert thresholds for each of the four
  components. Define what fields are forbidden in any signal (PAN, full
  card details, customer name in any aggregation label).
  Output: the alert rules with thresholds and the dashboard's required
  panels.
  Dependency: this branch must complete before Branch 3 begins because
  the observability design tells implementers what to emit. Without it,
  each component will emit ad-hoc signals that the dashboard cannot
  aggregate.

Branch 3 — 4.1 Backend Services (dependency order 3,
depends on Branches 1 and 2):
  Implement the four components and their queue contracts. Each component
  emits the metrics, traces, and logs designed in Branch 2. The dashboard
  is wired to those signals as the final implementation step.
  Dependency: starts after Branch 1 fixes the components and Branch 2
  fixes the signals. Implementing without these branches first produces
  components whose observability cannot meet the alert thresholds.

REJECTED — PLAN, no dependencies stated:

Branch 1 — 3.1 Architecture and Component Topology:
  Define the four components and their boundaries.

Branch 2 — 3.5 Observability and Operational Design:
  Define the metrics and alert thresholds.

Branch 3 — 4.1 Backend Services:
  Implement the four components.

All three branches are independent and can be assigned to parallel teams.
[WRONG: the rejected response names the same branches but states all three
as independent. Without the dependency from Branch 1 to Branch 3 (you cannot
implement components whose boundaries are not yet decided) and Branch 2 to
Branch 3 (you cannot emit signals whose thresholds are not yet decided),
the parallel teams will produce incompatible work — the implementers will
choose component shapes that the architects later disagree with, and the
metrics emitted will not match the alert thresholds defined late. The
dependency constraint is the routing signal the orchestrator needs;
omitting it produces a plan that looks complete but cannot be executed
in parallel without rework.]
```

---

## Pair AP1-PLAN-004 — multi-task plan with reversed model/contract order

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-004
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-B-wrong-dependency-order
chosen_branches: ["3.2", "3.3", "4.1"]
rejected_branches: ["3.3", "3.2", "4.1"]
```

Prompt: `"build the new search feature with an API for clients and a backing index for storage"`

```
CHOSEN — PLAN, model before contract:

Branch 1 — 3.2 Data Model and Persistence Design (dependency order 1):
  Define the index mapping: document shape, field types (keyword vs text vs
  numeric), analyzers, the shard/replica configuration. Decide what is
  stored in the index and what is fetched on demand from the source-of-truth
  store.
  Output: the index mapping document.
  No dependencies — this branch is the foundation.

Branch 2 — 3.3 API Contract Definition (dependency order 2,
depends on Branch 1):
  Define the API contract for the search endpoint. Request: query, filters,
  page, pageSize, sortBy. Response: results (a list of result objects),
  total, page, pageSize.
  The result object's field names must match the index mapping field names
  defined in Branch 1 — this is why the dependency direction is model-first.
  Output: an OpenAPI fragment for the endpoint.

Branch 3 — 4.1 Backend Services (dependency order 3,
depends on Branches 1 and 2):
  Implement the SearchService that translates the request shape into an
  index query and the index response into the response shape.

REJECTED — PLAN, contract before model:

Branch 1 — 3.3 API Contract Definition (dependency order 1):
  Define the API contract first based on the client team's wireframes:
  request fields and response shape are agreed in a kickoff with the client
  team.

Branch 2 — 3.2 Data Model and Persistence Design (dependency order 2):
  Define the index mapping to match the response shape agreed in Branch 1.
  If a field in the response shape is hard to compute from the index,
  derive it at query time.

Branch 3 — 4.1 Backend Services (dependency order 3):
  Implement the SearchService.
[WRONG: the rejected order writes the API contract before the index mapping.
Response shapes are constrained by what the underlying storage can return
efficiently — a contract that promises a "popularity score" without an index
that can compute it forces the implementation into a query-time aggregation
that does not scale. The model-before-contract order is the correct one for
search features because the index mapping decides what is fast and what is
slow; the contract is the publication of that decision. Reversing the order
ships a contract whose performance characteristics emerge late and force
contract changes after clients have integrated.]
```

---

## Pair AP1-PLAN-005 — planning prompt confused with conceptual explanation

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-005
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-C-wrong-domain
chosen_domain: PLAN
rejected_domain: CODE_Q
```

Prompt: `"plan the migration from monolith to microservices for the order management module"`

```
CHOSEN — PLAN, ordered branches with dependencies:

Branch 1 — 6.1 Manual QA and Acceptance Review (dependency order 1):
  Reviewer audits the current monolith to enumerate what the order management
  module owns: tables, queues, public endpoints, scheduled jobs, and shared
  utilities used by other modules.
  Output: a one-page inventory of every coupling that must be addressed.
  No dependencies; this is the first step because the rest depends on
  knowing what exists.

Branch 2 — 3.1 Architecture and Component Topology (dependency order 2,
depends on Branch 1):
  Decide the boundary of the new OrderManagement service: what it owns,
  what it stops owning, and what dependencies are forbidden.
  Decide the migration shape: strangler-fig with a routing layer that
  sends a fraction of traffic to the new service. Forbidden in the new
  service: any direct read of the monolith's database.

Branch 3 — 3.2 Data Model and Persistence Design (dependency order 3,
depends on Branch 2):
  Decide the new service's storage. Decide the data sync rule: dual-write
  during migration, then cut over reads, then drop monolith writes. Define
  the eventual-consistency window allowed during the dual-write period.

Branch 4 — 4.5 Integration and Provider Connectors (dependency order 4,
depends on Branches 2 and 3):
  Implement the routing layer and the dual-write integration. Order matters:
  routing layer first (zero-impact change because traffic still goes to
  monolith), then dual-write, then read cutover, then monolith write removal.

Branch 5 — 6.1 Manual QA and Acceptance Review (dependency order 5,
depends on all prior branches):
  Reviewer verifies the migration at each rollout step before promoting to
  the next: 1% traffic, 10%, 50%, 100%. Each step has its own go/no-go gate.

REJECTED — CODE_Q (conceptual explanation):

Migrating from monolith to microservices typically follows the strangler-fig
pattern. You introduce a routing layer that sends a subset of traffic to
the new service while the rest continues to the monolith. The new service
owns its own data and the migration uses dual-write or change-data-capture
to keep both stores in sync during transition. Common pitfalls include
distributed transactions, latency from cross-service calls, and
eventually-consistent reads. The boundary should follow domain ownership:
order management is a natural seam because orders have well-defined
relationships with customers, products, and payments.
[WRONG: "plan the migration" asks for ordered branches with dependencies and
named decisions. The rejected response is a conceptual overview of the
strangler-fig pattern and common pitfalls — that is CODE_Q. A plan names
specific branches (3.1, 3.2, etc.), assigns a dependency order, names the
decision to be made at each branch, and identifies which branch produces
the input each later branch depends on. The conceptual survey leaves the
decision unresolved.]
```

---

## Pair AP1-PLAN-006 — multi-task plan missing the dependency constraint

```
source_id: xiigen-arch-plan-batch-001-AP1-PLAN-006
source_license_state: first_party
pair_kind: dpo_plan
pair_type: Type-B-missing-dependency
chosen_branches: ["3.5", "7.3"]
rejected_branches: ["3.5", "7.3"]
```

Prompt: `"set up monitoring and alerting for the new payments service"`

```
CHOSEN — PLAN, dependency stated:

Branch 1 — 3.5 Observability and Operational Design (dependency order 1):
  Decide what to measure: the metrics and their labels, the trace span
  hierarchy, the log levels, what is forbidden in any signal (no PAN, no
  full card details, no customer names in aggregation labels).
  Decide alert rules: which thresholds fire, which severities (page vs
  silent), which response is expected of the on-call.
  Output: the signal allow-list and the alert rule table.

Branch 2 — 7.3 Monitoring and Feedback Loop (dependency order 2,
depends on Branch 1):
  Configure the monitoring stack to ingest the signals defined in Branch 1.
  Build the dashboards from the alert rules. Wire the alert routing to
  PagerDuty for severity-1 alerts and to a Slack channel for silent alerts.
  Document the on-call runbook with the action expected for each alert.
  Dependency: Branch 2 cannot begin meaningfully until Branch 1 fixes
  what is being measured. Configuring a dashboard against signals that
  do not exist yet produces a dashboard that displays nothing.

REJECTED — PLAN, no dependencies stated:

Branch 1 — 3.5 Observability and Operational Design:
  Design the observability strategy.

Branch 2 — 7.3 Monitoring and Feedback Loop:
  Set up Datadog dashboards and PagerDuty alerts.

Both branches can be assigned to parallel teams to ship faster.
[WRONG: the rejected response treats the design and the configuration as
independent. The configuration of monitoring tools depends on the signals
existing — a Datadog dashboard cannot display a metric whose label set has
not been decided, and a PagerDuty alert cannot route on a signal that is
not being emitted. Running these branches in parallel produces a dashboard
that is wired to fictional signals; when the design lands, the dashboard
must be reworked. The dependency from 3.5 to 7.3 is real and must be
stated explicitly in the plan; the orchestrator uses this to sequence the
work and to tell the parallel-team-assigner that this is one of the cases
where parallelism is not safe.]
```
