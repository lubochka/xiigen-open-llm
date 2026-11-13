# DPO Pair Authoring Guidance — ARCH and PLAN Domains
# Version: 2.0
# Date: 2026-05-08
# Supersedes: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.0

---

## What changed from v1

**Reward margin mechanics added.** Every section now explains why the examples
produce positive margin — not just what the correct content is. Authors need
to understand the token-level mechanism to make reliable authoring decisions.

**Token overlap test added.** A concrete test to apply before submitting any
pair — prevents the most common cause of negative margin without requiring a
training run to discover the problem.

**Calibration run data added.** Numbers from run 2026-05-06 are cited where
they explain why specific patterns succeed or fail.

**PLAN prompt quality section expanded.** The characteristics that distinguished
batch 16/17 (acc=0.618, 0.771) from batch 30 (acc=0.306) are now explicit rules.

**Correction event extraction added.** XIIGen session corrections are pre-made
pairs. Extraction rules and quality criteria are now part of this document.

**Pre-submission checklist expanded.** Seven new checks covering token overlap,
reward margin prediction, compiled prompt requirement, and annotation quality.

---

## Why this guidance exists

DPO training requires pairs where chosen and rejected have genuinely different
token distributions. When they share most of their tokens, the optimizer has no
signal and the reward margin stays near zero or goes negative.

Calibration run 2026-05-06: 29 of 40 batches had negative reward margin. Loss
stayed at 0.693 = ln(2) across all batches — random-guess level. The cause was
that rejected was the template `"Do not generate a capability for sub-phase X"`,
which shares `capability`, `sub-phase`, and the sub-phase number with chosen.
The model could not distinguish them.

This document defines how to write rejected responses that produce positive reward
margin for ARCH and PLAN domains. Every example maps to actual sub-phase definitions.

---

## The reward margin mechanism

DPO computes for each pair:

```
chosen_reward  = log P(chosen  | prompt)  under policy model
rejected_reward = log P(rejected | prompt) under policy model
reward_margin  = chosen_reward - rejected_reward
```

`reward_margin > 0` means the model already assigns higher probability to chosen.
The optimizer reinforces this. The pair contributes a useful gradient.

`reward_margin < 0` means the model assigns higher probability to rejected.
The optimizer runs but pushes in the wrong direction. Accumulated negative-margin
pairs degrade the model.

The token distribution difference between chosen and rejected is what determines
the sign of reward_margin. Larger distribution difference = stronger positive margin.

---

## The token overlap test

Before adding any pair to a batch, count:

```
shared_tokens = tokens in both chosen and rejected that are NOT also in the prompt
total_tokens  = tokens in chosen + tokens in rejected (excluding prompt tokens)
overlap_ratio = shared_tokens / total_tokens
```

If `overlap_ratio > 0.60`, the pair will likely produce negative reward margin.
Do not submit it. Revise the rejected to address a more visibly different
sub-phase or responsibility.

The negation template always fails this test:

```
chosen:   "Generated capability for sub-phase 3.4: Security and Authorization Model.
           Execute the requested work through task type security-and-authorization-model."
rejected: "Do not generate a capability for sub-phase 3.4."

Shared tokens: "capability", "sub-phase", "3.4", "generate"
Overlap ratio: ~0.70 → fails
```

A Type A rejected passes this test because it uses the vocabulary of a different
sub-phase — class names and method signatures for a 4.1 rejected versus boundary
rules and ownership statements for a 3.1 chosen.

---

## The core rule

**A rejected response must be a plausible wrong answer — not a negation of the
correct one.**

It must look like what a reasonable person would say if they misread the prompt
or confused two adjacent sub-phases. It contains real content, real technical
terms, and a coherent argument — but it addresses the wrong responsibility.

After the rejected response, include a `[WRONG: ...]` explanation that names
exactly why this response misses the mark.

**Why the annotation improves reward margin:** The annotation adds tokens that
describe why the rejected is wrong. These tokens appear in chosen (indirectly —
the model learns the association) but not in rejected. The annotation widens the
distributional difference beyond what the content alone provides. A pair without
an annotation relies entirely on content-level differences. A pair with a specific
annotation is more robust.

**Annotation quality standard:**

Weak (does not help margin): `[WRONG: this is the wrong sub-phase.]`

Strong (names the confusion explicitly):

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow —
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

The strong annotation names: the prompt verb that signals ARCH scope, what ARCH
actually owns, and why the rejected sub-phase (4.1 implementation) cannot satisfy
that scope. Each of these tokens contributes to distributional separation.

---

## The boundary that matters most for ARCH

Every ARCH sub-phase owns a *decision* — about boundaries, shape, strategy,
or criteria. The most common mistake is answering with *implementation* instead
of a decision.

The implementation test: if the rejected response contains class names, method
bodies, DI registrations, or specific framework calls — it is implementation
vocabulary, not architecture vocabulary. The distributional difference from a
3.1 boundary-rule chosen will be large. This is a strong Type A pair.

The monitoring test: if the rejected response contains tool names (Grafana,
PagerDuty, Prometheus), configuration actions (set up alert, configure dashboard)
— it is operational vocabulary. The difference from a 3.5 signal-design chosen
will also be large.

---

## Sub-phase 3.1 — Architecture and Component Topology

**Owns:** component boundaries, dependency direction, lifecycle topology,
system responsibilities.

**The question this sub-phase answers:** who owns what, where do dependencies
flow, what is forbidden.

**Why Type A rejected produces positive margin here:** Chosen uses boundary
vocabulary (owns, depends on, forbidden, direction). Rejected uses implementation
vocabulary (class, method, inject, register). These are largely non-overlapping
token sets. The overlap ratio is typically below 0.30.

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

**Why this pair works:** Chosen tokens: `owns`, `stateless`, `depends on`,
`forbidden`, `direction`, `no shared`. Rejected tokens: `Implement`, `class`,
`methods`, `Inject`, `Register`, `scoped`. Non-overlapping vocabulary sets.
Overlap ratio ≈ 0.15.

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
[WRONG: "who should own" is a topology question — where does the decision and
state live, what is the dependency direction. It is not a security configuration
question. The ownership boundary must be decided before any configuration detail
is specified.]
```

**Why this pair works:** 3.1 chosen uses `owns`, `stateless`, `lifecycle`,
`trade-off`. 3.4 rejected uses `Configure`, `token expiry`, `endpoint`, `POST`.
Both relate to auth, so there is some overlap, but the vocabulary of boundary
decision versus security configuration is distinct. Overlap ratio ≈ 0.35 —
acceptable.

---

**Example 4 — architecture review confused with architecture design**

Prompt: `"review the dependency direction in the current solution — does it match our rules?"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Review checklist:
1. List all project references — verify no implementation project references
   a higher-level orchestration project.
2. Confirm that 00.Core has no references to 21.AgentInfrastructure.
3. Confirm that 16.AutonomousLearning does not reference 21.AgentInfrastructure.
4. Document findings: which violations exist, which are accepted exceptions,
   which must be fixed before the next release.

REJECTED — 3.1 Architecture and Component Topology:
Define the correct dependency direction: 00.Core → 12.SemanticCompiler →
21.AgentInfrastructure → 16.AutonomousLearning. No reverse references allowed.
Extract ITrustScorer into 00.Core to break the current circular dependency.
[WRONG: "review the current solution" asks for an audit of what exists against
the stated rules — a review activity. Defining the correct direction and fixing
violations is design and implementation work. The review must happen first to
establish what needs to change.]
```

---

## Sub-phase 3.2 — Data Model and Persistence Design

**Owns:** entity shape, storage access patterns, consistency rules,
persistence constraints.

**Why Type A rejected produces positive margin here:** Chosen uses model
vocabulary (entity, partition, consistency, coupling rule, index). Rejected uses
implementation vocabulary (Repository, method, EF Core, DbContext, filter).
Non-overlapping sets even though both discuss data.

**Most common wrong responses:**
- Writing the repository class (→ 4.1 Backend Services)
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

**Why Type A rejected produces positive margin here:** Chosen uses signal design
vocabulary (metric, histogram, counter, label, threshold, forbidden). Rejected
uses either implementation vocabulary (ILogger, SetTag, inject) or tooling
vocabulary (Grafana, PagerDuty, configure, connect). Both rejected options are
non-overlapping with chosen signal vocabulary.

**Most common wrong responses:**
- Wiring a logger or tracer in code (→ 4.1 Backend Services)
- Configuring a dashboard or alert in a monitoring tool (→ 7.3 Monitoring)

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

**Why this pair works:** Chosen tokens: `checkpoint`, `structured event`,
`attributes`, `alert rule`, `emit`. Rejected tokens: `Grafana`, `dashboard`,
`PagerDuty`, `configure`, `connect`, `pipeline`. These are fully different
technical domains. Overlap ratio ≈ 0.10 — very strong.

---

## Sub-phase 6.1 — Manual QA and Acceptance Review

**Owns:** human acceptance criteria, exploratory validation, release-facing
quality judgment.

**Why Type A rejected produces positive margin here:** Chosen uses review
vocabulary (human reviewer, checklist, verify by reading, judgment, sign-off).
Rejected uses automation vocabulary (write test, mock, assert, Playwright, test
runner). Non-overlapping even though both discuss quality.

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
characters renders correctly in the email subject and dashboard header.

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
5. Reviewer documents findings in the release sign-off artifact.

REJECTED — 5.1 Unit Test Design:
Write unit tests for AuthGuard.canActivate() that mock JwtStrategy and
assert correct behavior for valid, expired, and missing tokens. Add a
cross-tenant test using JWT fixtures for two different tenants.
[WRONG: the security team sign-off is a human review activity. It requires
reading the actual implementation, not running the tests. Tests may pass
while a route remains unguarded. The human reviewer catches what automation
cannot: missing guards, undocumented bypass paths, and unreviewed exceptions.]
```

---

## PLAN domain — what makes a good pair

For PLAN, the primary failure mode is wrong dependency order, not wrong domain.
The model needs to learn that `branch_2.depends_on=["branch_1"]` is correct
and `branch_2.depends_on=[]` is wrong for specific sequences.

**Type B rejected is required for every PLAN pair where the prompt covers
multiple tasks.** Type B is the primary signal for PLAN accuracy.

**Why Type B is harder than Type A:** Chosen and rejected name the same
sub-phases. The distributional difference comes from the `depends_on` field
and the explanation in the annotation. This is a smaller difference than Type A.
The annotation is therefore more important for Type B than for Type A. A Type B
pair without an annotation that explains the dependency will likely have a
near-zero or negative margin.

---

**What makes a good PLAN prompt**

From calibration run analysis — batch 16 (acc=0.618) and batch 17 (acc=0.771)
contained prompts with specific characteristics. Batch 30 (acc=0.306) did not.

**Required: at least one of these three characteristics:**

1. Named artifact: `"Flow 10"`, `"Phase 3"`, `"Factory Interface"`, `"AuthGuard"`,
   `"TenantResolver"`, specific component or file name.

2. Dependency language: `"before"`, `"after"`, `"once X is complete"`,
   `"depends on"`, `"when X is in place"`, `"then write"`.

3. Technical specificity: predicate vocabulary, sub-phase references, technology
   names that map to predicates (jwt, api, architecture).

**Batch 16 example prompt (good):**
```
"continue from where you stopped — Plan complete. Now executing Phase 1 —
Factory Interfaces and Engine Processing"
```
Named artifact: Phase 1, Factory Interfaces. Technical specificity: Engine.

**Batch 30 example prompt (bad — do not use for PLAN training):**
```
"Based on the review and the response you've shared, Option A is clearly
the right path — the research is too..."
```
No named artifact. No dependency language. No predicate vocabulary.
Meta-commentary about a decision, not a technical planning request.

**Prompts to exclude from PLAN training:**
```
"Option A is clearly the right path"
"this approach is grounded in existing documents"
"we're making good progress"
"let's continue with the plan"
```
These prompts contain no predicate vocabulary. The compiled predicate set will
be empty or contain only DOCUMENT. The TrainingText carries no routing signal
and reward margin will be near zero regardless of rejected quality.

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
  business logic. AuthGateway depends on TenantResolver.
  Dependency: branch 2 begins after branch 1 JWT structure is agreed.

REJECTED — CODE_Q:
Multi-tenant authentication can be structured several ways: shared auth server
with tenant claims, separate auth server per tenant, or OIDC federation.
JWT with tenant_id is the most common approach.
[WRONG: "how should we structure" asks for a plan with ordered decisions —
which choices to make, in what order, with what dependencies. A conceptual
survey of options is CODE_Q. This is PLAN scope, not CODE_Q scope.]
```

**Why this pair works:** Chosen uses PLAN vocabulary (`branch`, `dependency order`,
`depends on`, `begins after`). Rejected uses CODE_Q vocabulary (`can be structured`,
`several ways`, `most common approach`). Strong distributional difference.

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
the integration tests can meaningfully run.]
```

**Why the annotation is critical for Type B:** Chosen and rejected name the same
sub-phases (3.4, 5.2). The distributional difference comes from `depends_on`
field content and the annotation. Without the annotation, the overlap ratio
would be ~0.55 — borderline. The annotation tokens (`cannot test`, `mock tokens
bypass`, `must exist before`) appear in chosen's semantic neighbourhood but not
in rejected's content, widening the distribution gap to ~0.40.

---

**Example 13 — plan with technical work confused with abstract phases**

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
  mapping from branch 1 and the response shape from branch 2.
  Dependency: branches 1 and 2 must be complete.

REJECTED — PLAN, too abstract:
Phase 1: design the search feature.
Phase 2: implement the search feature.
Phase 3: test the search feature.
Phase 4: deploy the search feature.
[WRONG: a plan must name the specific sub-phases, their dependency order, and
what decision or artifact each branch produces. "Design, implement, test, deploy"
is a label structure, not a plan. It carries no routing signal to the orchestrator
about what kind of work each branch requires.]
```

**Why this pair works:** Chosen uses specific sub-phase vocabulary (`3.2`, `3.3`,
`4.1`, `index mapping`, `endpoint contract`, `depends on`). Rejected uses only
abstract phase labels (`design`, `implement`, `test`, `deploy`). Very strong
distributional difference.

---

## Correction events as pre-made pairs

Every Luba correction in a XIIGen session document is a pre-made pair. These
require no invented rejected content.

**Extraction pattern:**

```
Session contains:
  Claude's wrong answer    → rejected
                             (add [WRONG: {the correction itself}] as annotation)
  Luba's correction        → chosen
  The original prompt      → A side

Example:
  Prompt:   "design the data model for multi-tenant user preferences"
  Wrong:    "Create a UserPreferencesRepository with GetByUserId..."
  Correct:  "Entity: UserPreference {tenantId (partition key), userId...}"

Extracted pair:
  A:        "design the data model for multi-tenant user preferences"
  Chosen:   "Entity: UserPreference {tenantId (partition key)...}"
  Rejected: "Create a UserPreferencesRepository with GetByUserId..."
  [WRONG: "design the data model" asks for entity shape and partitioning rules,
  not the repository class. The repository is built after the model is decided.]
```

**Why correction events produce better margin:** The wrong answer came from a
different reasoning path than the correct answer. They were not constructed to be
different — they are genuinely different answers to the same question. Token
distributions are naturally non-overlapping. Overlap ratio is typically below 0.25.

**Quality rating for correction events:**

Rate each correction before including it:

```
High: chosen and rejected have clearly different vocabulary
      — include unconditionally

Medium: chosen and rejected address different responsibilities
        but share some vocabulary (overlap ratio 0.30–0.50)
        — include with strong annotation

Low: correction is only phrasing, not substance
     (chosen and rejected say the same thing differently)
     — exclude
```

---

## Dependency confusion table for PLAN

| Chosen sequence | Plausible wrong order | Why the wrong order fails |
|---|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 retroactively | Implementing without decided boundaries creates coupling that must be undone |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model derived from contract | Contract shape depends on model shape; reversing leaks implementation into public API |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mock tokens | Mock tokens cannot test real tenant isolation — they bypass validation logic |
| 3.1 + 3.4 parallel → 4.1 after both | All three parallel | 4.1 cannot implement boundaries that 3.1 has not defined |
| 2.2 Acceptance Criteria → 5.1 Unit Tests | 5.1 first, criteria from tests | Tests define what passes; criteria define what should pass — tests cannot generate criteria |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 first, design retroactively | Cannot configure collection of signals that have not been defined |
| 3.3 API Contract → 4.5 Integration Connectors | 4.5 first, contract from implementation | Implementation detail leaks into public contract |

---

## Confusion table for ARCH sub-phases

| Sub-phase | Correct scope | Most plausible rejected | Overlap ratio (typical) |
|---|---|---|---|
| 3.1 Architecture and Component Topology | Boundary decisions, dependency rules | 4.1 Backend Services | ~0.15 — strong |
| 3.1 Architecture and Component Topology | Boundary decisions, dependency rules | 4.4 Refactoring | ~0.20 — strong |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules | 4.1 Backend Services | ~0.20 — strong |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules | 4.5 Integration | ~0.25 — strong |
| 3.5 Observability and Operational Design | What to measure, alert thresholds | 4.1 Backend Services | ~0.15 — very strong |
| 3.5 Observability and Operational Design | What to measure, alert thresholds | 7.3 Monitoring and Feedback Loop | ~0.10 — very strong |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.1 Unit Test Design | ~0.20 — strong |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.3 End-to-End Scenario Testing | ~0.20 — strong |

Overlap ratios below 0.30 reliably produce positive reward margin.
Overlap ratios between 0.30 and 0.50 typically produce positive margin with a
strong annotation.
Overlap ratios above 0.50 reliably produce negative margin.

---

## Batch composition requirements

| Pair type | Minimum fraction | Notes |
|---|---|---|
| ARCH: design vs implementation (Type A) | 40% of ARCH pairs | Primary failure mode. Lowest overlap ratio. Reliable positive margin. |
| ARCH: design vs monitoring/tooling (Type A) | 20% of ARCH pairs | 3.5 and 6.1 confusion. Very low overlap ratio. |
| ARCH: design vs review (Type A) | 20% of ARCH pairs | 3.1 confused with 6.1. Moderate overlap — strong annotation required. |
| PLAN: correct branches, wrong order (Type B) | 40% of PLAN pairs | Primary failure mode. Moderate overlap — annotation is critical. |
| PLAN: plan vs explanation (Type C) | 30% of PLAN pairs | PLAN vs CODE_Q. Strong distributional difference. |
| PLAN: plan with vs without dependencies (Type B) | 30% of PLAN pairs | Missing `depends_on`. Annotation carries most of the signal. |

---

## Pre-submission checklist

Before adding any pair to a batch:

```
□ Token overlap test: overlap_ratio < 0.60
  (count shared tokens not in prompt / total tokens)

□ Rejected is a complete, coherent response — not a negation phrase
  ("Do not...", "Skip...", "Not applicable..." all fail this check)

□ Rejected contains real technical content from the wrong sub-phase's vocabulary

□ Rejected addresses a different but plausible sub-phase — adjacent, not random

□ [WRONG: ...] explanation names the specific confusion:
  — which sub-phase the rejected incorrectly addresses
  — why that sub-phase does not own this responsibility
  — what the prompt actually asked for that rejected missed

□ Annotation is specific, not generic
  ("this is wrong" fails — "X asks for Y, rejected provides Z instead" passes)

□ Chosen and rejected differ in the decision they make, not only in wording

□ For PLAN pairs: prompt contains named artifact OR dependency language OR
  technical specificity (no meta-commentary, no abstract phase labels)

□ For Type B PLAN pairs: annotation explains why the wrong order fails
  (not just "this is the wrong order")

□ For correction events: quality rated High or Medium
  (Low = phrasing-only correction, exclude)

□ TrainingText will use compiledText as promptSide — not the raw prompt string
  (verify PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md is implemented)

□ The prompt appears exactly once in this batch as chosen
```

---

## Last Updated

2026-05-08.
