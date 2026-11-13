# DPO Pair Authoring Guidance — ARCH and PLAN Domains
# Version: 1.0
# Date: 2026-05-08

---

## Why this guidance exists

DPO training requires pairs where the chosen response and the rejected response
are genuinely different in meaning — not just in phrasing. When rejected is a
template phrase like "Do not generate a capability for sub-phase 3.1", the model
sees nearly identical tokens on both sides and learns nothing. The reward margin
stays negative and accuracy stalls near chance level.

This document defines how to write rejected responses that teach real distinctions
for the ARCH and PLAN domains. Every example in this document maps to the actual
sub-phase definitions in the topology registry.

---

## The core rule

**A rejected response must be a plausible wrong answer — not a negation of the
correct one.**

It must look like what a reasonable person would say if they misread the prompt
or confused two adjacent sub-phases. It contains real content, real technical
terms, and a coherent argument — but it addresses the wrong responsibility.

After the rejected response, include a `[WRONG: ...]` explanation that names
exactly why this response misses the mark. This explanation is the training
signal the model needs.

---

## The boundary that matters most for ARCH

Every ARCH sub-phase owns a *decision* — about boundaries, shape, strategy,
or criteria. The most common mistake is answering with *implementation* instead
of a decision.

The test: if the response contains class names, method bodies, DI registrations,
or specific framework calls — it is implementation, not architecture. A correct
ARCH response contains rules, ownership boundaries, dependency directions, entity
shapes, measurement thresholds, and acceptance criteria.

---

## Sub-phase 3.1 — Architecture and Component Topology

**Owns:** component boundaries, dependency direction, lifecycle topology,
system responsibilities.

**The question this sub-phase answers:** who owns what, where do dependencies
flow, what is forbidden.

**Most common wrong responses:**
- Writing the class or service that implements the component (→ 4.1 Backend Services)
- Extracting methods or restructuring existing code (→ 4.4 Refactoring)
- Describing the security configuration of a component (→ 3.4 Security Model)

---

**Example 1 — boundary definition confused with class implementation**

Prompt: `"define the component boundaries for the notification service"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Three components own this capability:
  NotificationDispatcher — owns sending, stateless, no persistence.
  TemplateStore — owns template registry, read-only, versioned.
  DeliveryTracker — owns delivery state and retry scheduling, owns its own storage.
Dependency direction: Dispatcher depends on TemplateStore and DeliveryTracker.
No reverse dependency permitted. DeliveryTracker must not be called by
TemplateStore under any circumstance.
Forbidden: no shared database between Dispatcher and DeliveryTracker.

REJECTED — 4.1 Backend Services:
Implement the NotificationService class with three methods: Send(),
GetTemplate(), and TrackDelivery(). Inject ITemplateRepository and
IDeliveryRepository through the constructor. Register the service as scoped
in the DI container.
[WRONG: "define boundaries" asks who owns what and where dependencies flow —
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

---

**Example 2 — boundary redesign confused with code refactoring**

Prompt: `"the analytics pipeline is too coupled — redesign the boundaries"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Three boundaries replace the current monolithic pipeline:
  IngestionGateway — public-facing, stateless, owns schema validation only.
  StreamProcessor — internal, owns aggregation logic, exposes no public API.
  QueryFacade — read-only view, no write path to processor state.
Rule: IngestionGateway never calls QueryFacade directly. All coupling
flows through domain events, not method calls.
StreamProcessor is the only writer; QueryFacade is the only reader.

REJECTED — 4.4 Refactoring and Code Quality:
Extract the aggregation logic from AnalyticsController into a separate
AnalyticsAggregationService. Move the query methods into AnalyticsQueryService.
Update the DI registrations to wire the new classes.
[WRONG: "redesign boundaries" is a structural decision about ownership and
allowed dependencies — not a class extraction exercise. Refactoring classes
without first deciding the boundary rules will reproduce the same coupling
inside the new class names.]
```

---

**Example 3 — ownership question confused with security configuration**

Prompt: `"who should own the user session — the auth service or the API gateway?"`

```
CHOSEN — 3.1 Architecture and Component Topology:
Session ownership belongs to the auth service. The API gateway is stateless.
The auth service owns the full session lifecycle: create, refresh, invalidate.
The API gateway owns only token verification — it calls the auth service for
validation and does not replicate or cache session state.
Consequence: if the auth service is down, the gateway cannot validate new sessions.
This is a deliberate trade-off for clean ownership.

REJECTED — 3.4 Security and Authorization Model:
Configure JWT token validation at the API gateway using the auth service public key.
Set token expiry to 15 minutes. Add a refresh token endpoint at POST /auth/refresh.
[WRONG: "who should own" is a topology question — where does the decision and state
live, what is the dependency direction. It is not a security configuration question.
The ownership boundary must be decided before any configuration detail is specified.]
```

---

**Example 4 — architecture review prompt confused with architecture design**

Prompt: `"review the dependency direction in the current solution — does it match our rules?"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Review checklist:
1. List all project references — verify no implementation project references
   a higher-level orchestration project.
2. Confirm that 00.Core has no references to 21.AgentInfrastructure.
3. Confirm that 16.AutonomousLearning does not reference 21.AgentInfrastructure
   (this would be circular).
4. Document findings: which violations exist, which are accepted exceptions,
   which must be fixed before the next release.

REJECTED — 3.1 Architecture and Component Topology:
Define the correct dependency direction: 00.Core → 12.SemanticCompiler →
21.AgentInfrastructure → 16.AutonomousLearning. No reverse references allowed.
Extract ITrustScorer into 00.Core to break the current circular dependency.
[WRONG: "review the current solution" asks for an audit of what exists against
the stated rules — that is a review activity. Defining the correct direction
and fixing violations is design and implementation work. The review must
happen first to establish what needs to change.]
```

---

## Sub-phase 3.2 — Data Model and Persistence Design

**Owns:** entity shape, storage access patterns, consistency rules,
persistence constraints.

**The question this sub-phase answers:** what does the data look like, where
does it live, what is forbidden at the storage level.

**Most common wrong responses:**
- Writing the repository class (→ 4.1 Backend Services)
- Writing the migration SQL (→ 4.1 or 4.3)
- Writing the adapter that fetches data from another service (→ 4.5 Integration)

---

**Example 5 — data model confused with repository implementation**

Prompt: `"design the data model for multi-tenant user preferences"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
Entity: UserPreference { tenantId (partition key), userId, preferenceKey,
preferenceValue, lastModified }.
Partition by tenantId — no cross-tenant reads permitted at storage level.
Consistency: eventually consistent, last-write-wins per key.
Coupling rule: no foreign key to the User entity. userId is a string identifier,
not a relational reference, to avoid cross-service schema coupling.
Index: (tenantId, userId) composite for reads, no global index across tenants.

REJECTED — 4.1 Backend Services:
Create a UserPreferencesRepository with GetByUserId(tenantId, userId),
Set(tenantId, userId, key, value), and DeleteAll(tenantId, userId) methods.
Apply a tenantId filter on the EF Core DbContext to enforce isolation.
[WRONG: "design the data model" asks for entity shape, partitioning strategy,
consistency rules, and coupling constraints. Writing the repository class is
implementation that follows the model decision — it cannot replace it.]
```

---

**Example 6 — storage boundary question confused with integration adapter**

Prompt: `"order and payment share a database — should they?"`

```
CHOSEN — 3.2 Data Model and Persistence Design:
No. Orders own order state and line items. Payments own payment state and
transaction records. They share an orderId reference (a string, not a
foreign key) but must have separate storage schemas.
Rule: the payment service must not query order tables directly. If payment
processing needs order data, it receives it through a domain event published
at the time the order is created — not through a join.
Consequence: payment can be deployed independently and scaled independently.

REJECTED — 4.5 Integration and Provider Connectors:
Add a PaymentOrderAdapter that calls the Order service API to retrieve
order details when payment processing requires them. Add Polly retry logic
to handle transient failures in the API call.
[WRONG: the question is whether they should share a database — a persistence
boundary decision. The adapter is the consequence of correctly answering "no".
The boundary decision must come first; the adapter is its implementation.]
```

---

## Sub-phase 3.5 — Observability and Operational Design

**Owns:** logging strategy, metric definitions, trace design, alert thresholds,
and what must never appear in operational signals.

**The question this sub-phase answers:** what must be measured, what must trigger
an alert, and what must be kept out of traces.

**Most common wrong responses:**
- Wiring a logger or tracer in code (→ 4.1 Backend Services)
- Configuring a dashboard or alert in a monitoring tool (→ 7.3 Monitoring and Feedback Loop)

---

**Example 7 — observability strategy confused with logger wiring**

Prompt: `"design the observability strategy for the payment processing service"`

```
CHOSEN — 3.5 Observability and Operational Design:
Three signal categories required:

Metrics:
  payment_attempt_total — counter, labels: outcome (success/failure), provider.
  payment_latency_ms — histogram, labels: provider, amount_tier.
  fraud_score_distribution — histogram, no PII labels.

Traces:
  One span per payment attempt. Required tags: provider_name, amount_range,
  tenant_id (hashed, not raw). Forbidden: card number, CVV, raw amount,
  customer name must never appear in any span attribute.

Logs:
  Structured JSON. Level INFO for success with correlation_id.
  Level ERROR for failure with correlation_id and failure_reason enum.

Alert: payment_failure_rate > 5% in a 5-minute window → page on-call.

REJECTED — 4.1 Backend Services:
Inject ILogger<PaymentService> and call _logger.LogInformation() on success
and _logger.LogError() on failure. Add OpenTelemetry activity with
activity.SetTag("provider", providerName) and activity.SetTag("outcome", result).
[WRONG: "design the observability strategy" asks what to measure, what thresholds
matter, and what must never appear in traces. Writing the ILogger call is
implementation — it follows the strategy. Without the strategy first, the
implementation will log inconsistently, miss alert thresholds, and risk
leaking sensitive data into traces.]
```

---

**Example 8 — observability design confused with monitoring tool setup**

Prompt: `"we have no visibility into why tenant onboarding fails"`

```
CHOSEN — 3.5 Observability and Operational Design:
Three checkpoints must be added to the onboarding flow, each emitting a
structured event:
  1. tenant_created — always emitted, attributes: tenantId, plan_tier.
  2. admin_invited — attributes: tenantId, success (bool), failure_reason if false.
  3. first_login_completed — attributes: tenantId, elapsed_seconds_since_creation.

Alert rule: if tenant_created fires but first_login_completed does not fire
within 48 hours → emit onboarding_stall alert with tenantId.
This design identifies exactly which step tenants abandon.

REJECTED — 7.3 Monitoring and Feedback Loop:
Configure a Grafana funnel dashboard from the existing application logs.
Set up a PagerDuty alert for onboarding failures. Connect the log pipeline
to the monitoring stack.
[WRONG: "we have no visibility" means the signals do not exist yet. Connecting
existing logs to Grafana does not help when the onboarding flow does not emit
the required structured events. Observability design (deciding what events
to emit and what attributes they carry) must precede monitoring setup.]
```

---

## Sub-phase 6.1 — Manual QA and Acceptance Review

**Owns:** human acceptance criteria, exploratory validation, release-facing
quality judgment.

**The question this sub-phase answers:** what must a human verify before this
ships, and what counts as passing.

**Most common wrong responses:**
- Writing automated tests (→ 5.1 Unit Test Design or 5.3 End-to-End)
- Writing a test automation harness (→ 7.1 Automated Test Harness)

---

**Example 9 — acceptance review confused with automated test authoring**

Prompt: `"define acceptance criteria for the tenant onboarding flow before release"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
The following must be verified by a human reviewer, not by an automated suite:

1. A new tenant can register, receive the invitation email, and complete first
   login within one uninterrupted session. No error screen appears at any step.
2. The admin dashboard shows the correct tenant name and plan tier immediately
   after first login — no page refresh required.
3. Attempting to access another tenant's data returns a 403 response with a
   clear error message — not a 500 or a silent empty result.
4. The invitation email arrives within 2 minutes. Reviewer uses a real inbox,
   not a mock or a log assertion.

Edge case the reviewer must check manually: tenant name containing unicode
characters (e.g. "Müller & Söhne") renders correctly in the email subject
and in the dashboard header.

REJECTED — 5.3 End-to-End Scenario Testing:
Write a Playwright test that registers a new tenant, follows the invitation
link, and asserts the dashboard loads with the correct tenant name. Add a
cross-tenant access test that verifies the 403 response.
[WRONG: acceptance criteria before release requires human judgment — including
timing of the real email, rendering of edge case characters, and the overall
experience quality that automation cannot assess. Automated tests verify
known paths; acceptance review covers the release judgment call.]
```

---

**Example 10 — security sign-off confused with automated security tests**

Prompt: `"the security team needs to sign off on the auth implementation before we ship"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Security sign-off checklist for the reviewer:

1. Read the controller files — verify that @UseGuards(AuthGuard) is present
   on every protected controller. Do not rely on test coverage; read the code.
2. Open bypass-paths.registry.ts — verify that every listed path has a
   documented justification and was intentionally added.
3. Send a request with an expired token — verify the response is 401,
   not 500 and not a silent 200.
4. Send a request with a valid token for Tenant A to a Tenant B resource —
   verify the response is 403.
5. Reviewer documents findings in the release sign-off artifact. If any
   check fails, the release is blocked.

REJECTED — 5.1 Unit Test Design:
Write unit tests for AuthGuard.canActivate() that mock JwtStrategy and
assert correct behavior for valid, expired, and missing tokens. Add a
cross-tenant test using JWT fixtures for two different tenants.
[WRONG: the security team sign-off is a human review activity. It requires
reading the actual implementation, not running the tests. Tests may pass
while a route remains unguarded. The human reviewer catches what automation
cannot: missing guards, undocumented bypass paths, and unreviewed exceptions.
Human acceptance is the gate; automated tests are supporting evidence, not
the gate itself.]
```

---

## PLAN domain — what makes a good rejected

For PLAN, the most damaging error pattern is different from ARCH. The issue is
not design vs implementation — it is **sequence**. PLAN is about what order
things happen and what depends on what.

A correct PLAN response names the branches, assigns a dependency order to each,
and states which branches must complete before others can start.

A good rejected for PLAN is one of:
- The correct branches in the wrong order
- The correct sub-phases but missing the dependency constraint
- An explanation of concepts instead of a plan with ordered steps

---

**Example 11 — planning prompt confused with conceptual explanation**

Prompt: `"how should we structure the authentication flow for a multi-tenant SaaS product"`

```
CHOSEN — PLAN, branches 3.4 and 3.1:
Branch 1 — 3.4 Security and Authorization Model (dependency order 1):
  Define the JWT structure: include tenant_id claim, set 15-minute expiry,
  specify the refresh token rotation policy. Define which routes are public
  and document them in bypass-paths.registry.ts.

Branch 2 — 3.1 Architecture and Component Topology (dependency order 2):
  Define the component topology: AuthGateway owns token validation,
  TenantResolver owns tenant context injection, ResourceServer owns
  business logic. AuthGateway depends on TenantResolver. No component
  calls ResourceServer without passing through AuthGateway.
  Dependency: branch 2 begins after branch 1 JWT structure is agreed.

REJECTED — CODE_Q:
Multi-tenant authentication can be structured several ways: shared auth server
with tenant claims, separate auth server per tenant, or OIDC federation.
JWT with tenant_id is the most common approach. The claim identifies the tenant
and the resource server enforces isolation by filtering queries.
[WRONG: "how should we structure" asks for a plan with ordered decisions —
which choices to make, in what order, with what dependencies. A conceptual
survey of options is CODE_Q (explaining how things work), not PLAN (deciding
what to build and in what order).]
```

---

**Example 12 — multi-step plan with wrong dependency order**

Prompt: `"add JWT authentication and then write integration tests for tenant isolation"`

```
CHOSEN — PLAN, correct order:
Branch 1 — 3.4 Security and Authorization Model (dependency order 1):
  Implement the AuthGuard with JWT validation and tenant_id claim extraction.
  Define bypass paths. This branch must complete before testing can begin.

Branch 2 — 5.2 Integration Test Design (dependency order 2):
  Design integration tests that verify cross-tenant data isolation.
  Tests require a working AuthGuard to issue and validate real tokens.
  Dependency: branch 2 depends on branch 1.

REJECTED — PLAN, reversed order:
Branch 1 — 5.2 Integration Test Design (dependency order 1):
  Write the tenant isolation integration tests first, using mock JWT tokens.

Branch 2 — 3.4 Security and Authorization Model (dependency order 2):
  Implement the real AuthGuard after the tests define what it must satisfy.
[WRONG: integration tests for tenant isolation require a functioning AuthGuard
that issues real tokens. Mock tokens cannot test real tenant isolation because
they bypass the actual validation logic. The implementation must exist before
the integration tests can meaningfully run. Tests-first is valid for unit tests;
it breaks for integration tests that depend on real infrastructure.]
```

---

**Example 13 — plan with concrete technical work confused with planning the plan**

Prompt: `"plan the implementation of the search feature — elasticsearch backend"`

```
CHOSEN — PLAN:
Branch 1 — 3.2 Data Model and Persistence Design (dependency order 1):
  Define the index mapping: document structure, field types, analyzers,
  and shard configuration. This decision gates all other branches.

Branch 2 — 3.3 API Contract Definition (dependency order 2):
  Define the search endpoint contract: GET /api/search?q=&filters=&page=.
  Define the response shape: { results: [], total: int, page: int }.
  Dependency: index mapping must be finalized before the response shape
  can reference field names.

Branch 3 — 4.1 Backend Services (dependency order 3):
  Implement the SearchService that queries Elasticsearch using the index
  mapping from branch 1 and returns the response shape from branch 2.
  Dependency: branches 1 and 2 must be complete.

REJECTED — PLAN, too abstract:
Phase 1: design the search feature.
Phase 2: implement the search feature.
Phase 3: test the search feature.
Phase 4: deploy the search feature.
[WRONG: a plan must name the specific sub-phases, their dependency order,
and what decision or artifact each branch produces. "Design, implement, test,
deploy" is a label structure, not a plan. It provides no routing signal to
the orchestrator about what kind of work each branch requires.]
```

---

## Dependency confusion table for PLAN

Use this table to select a plausible rejected when writing PLAN pairs. Pick a
rejected that reverses or removes a real dependency.

| Chosen sequence | Plausible wrong order in rejected |
|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 as afterthought |
| 3.2 Data Model → 3.3 API Contract | 3.3 first (contract before model) |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mocks, 3.4 later |
| 3.1 + 3.4 in parallel → 4.1 after | All three in parallel (missing dependency) |
| 2.2 Acceptance Criteria → 5.1 Unit Tests | 5.1 first, criteria inferred from tests |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 setup first, design retroactively |

---

## Confusion table for ARCH sub-phases

Use this table to select a plausible rejected when writing ARCH pairs. Pick a
rejected that addresses adjacent work in a different phase.

| Sub-phase | Correct scope | Most plausible rejected | Why it is wrong |
|---|---|---|---|
| 3.1 Architecture and Component Topology | Boundary decisions, dependency rules | 4.1 Backend Services | Writes the class instead of the boundary |
| 3.1 Architecture and Component Topology | Boundary decisions, dependency rules | 4.4 Refactoring | Moves code instead of defining ownership |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules | 4.1 Backend Services | Writes the repository instead of the model |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules | 4.5 Integration | Writes the adapter instead of the boundary |
| 3.5 Observability and Operational Design | What to measure, what to alert | 4.1 Backend Services | Wires the logger instead of defining the strategy |
| 3.5 Observability and Operational Design | What to measure, what to alert | 7.3 Monitoring and Feedback Loop | Sets up the dashboard instead of defining the signals |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.1 Unit Test Design | Automates instead of defining human acceptance |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.3 End-to-End Scenario Testing | Automates the user journey instead of the human checklist |

---

## Batch composition requirements

Every batch must include at minimum:

| Pair type | Minimum fraction | Notes |
|---|---|---|
| ARCH: design vs implementation | 40% of ARCH pairs | Primary failure mode |
| ARCH: design vs monitoring/tooling | 20% of ARCH pairs | 3.5 and 6.1 confusion |
| ARCH: design vs review | 20% of ARCH pairs | 3.1 confused with 6.1 |
| PLAN: correct branches, wrong order | 40% of PLAN pairs | Primary failure mode |
| PLAN: plan vs explanation | 30% of PLAN pairs | PLAN vs CODE_Q boundary |
| PLAN: plan with dependencies vs plan without | 30% of PLAN pairs | Missing dependency signal |

No prompt may appear as chosen in one pair and rejected in another within the
same batch.

---

## Pre-submission checklist

Before adding a pair to the dataset:

```
□ Rejected is a complete, coherent response — not a negation phrase
□ Rejected contains real technical content that relates to the prompt
□ Rejected addresses a different but plausible sub-phase — not a random one
□ The [WRONG: ...] explanation names the specific confusion, not generic criticism
□ Chosen and rejected differ in the decision they make, not only in wording
□ For PLAN: the rejected reverses or omits a real dependency
□ For ARCH: the rejected implements instead of deciding, or monitors instead of designing
□ The prompt appears exactly once in this batch as chosen
```
