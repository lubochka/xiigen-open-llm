# DPO Pair Authoring Guidance — ARCH and PLAN Domains
# Version: 4.0
# Date: 2026-05-08
# Supersedes: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v3.0

---

## What changed from v3

**Gap remediation pairs added (Type G).** Gap remediation documents (U14 —
GAP-IMPLEMENTATION-PLAN, FIX-PLAN, REMEDIATION-PLAN) record an issue→wrong-fix→
correct-fix chain. This three-step structure is a pre-made DPO pair requiring no
rejected construction. Wrong fix = rejected, correct fix = chosen. False-pass entries
(gate claimed pass but implementation was wrong) produce the highest-value ArbiterUnit
pairs in this document type. Annotation rule: name the call-site ordering or detection
gap, not just "this is wrong". Quality gate: reasoning error = High, typo = Low (exclude).

**Generated code and debug correction pairs added (Type H).** Generated code debug
histories (U23 — css-debug-history.json, CORRECTED-DEFECT-CATALOG) record generation
→defect→correction cycles. Structurally identical to correction events but in the
implementation domain. Critical rule: A-side is the task description, NOT the generated
code. Annotation must name the silent failure mode — why the wrong code appears correct
but fails in a specific runtime condition. Low-quality entries (typos, missing imports)
excluded.

**Batch composition updated.** Type G (10% of any batch) and Type H (10% of BUG/REFACTOR
batches) added as minimum fractions.

**Checklist extended** with Type G and Type H specific checks.

**Decision document pairs added (Type D).** Every locked A-series and D-series decision
in DECISIONS-LOCKED.md contains a pre-made rejected: the alternative that was considered
and not taken. These pairs require no rejected construction — the wrong answer is already
documented. Section "Decision document pairs — Type D" added with extraction pattern,
annotation rules, and two examples.

**Contract accuracy pairs added (Type E).** When a document has been revised and an
older version described a contract that is now superseded, the old contract is a ready-made
rejected. The current version's contract is the chosen. Section "Contract accuracy pairs
— Type E" added with the key superseded contracts from the XIIGen corpus, overlap
analysis, and worked examples.

**Structural completeness pairs added for PLAN — Type F.** A plan without executable
Gate B commands looks complete but cannot be dispatched. The structural incompleteness
is harder to detect than wrong dependency order. Section added with examples showing
the difference between a dispatchable plan and a prose plan.

**Section on review sub-phases expanded.** The existing 6.1 section covered human
review vs automated tests. A new sub-section on code review (6.4 code review pairing
with 7.1 experiment status) is added, with the specific vocabulary table that produces
low overlap and the A-side rule for status-document pairs.

**Overlap ratio reference table updated.** All new pair types added with their typical
overlap ratios. The table now covers 14 pair types across ARCH, PLAN, CODE_Q, and review
sub-phases.

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

## Sub-phase 6.4 — Code review and implementation status

**Owns:** verification that proven capabilities are correctly wired in the
production path — not just that they exist in the codebase.

**The critical distinction for 6.4:** An experiment class that exists in project 12
or 29 but has no reference from project 21 is NOT in the execution path. A review
that says "this is implemented" based on the class's existence is wrong. The
correct review finding checks: is it referenced from project 21?

**A-side rule:** The A side must be a Type 1 developer prompt using intent verbs:
`"does", "is", "review", "check", "verify"`. Status document text is never the A side.

**Vocabulary that distinguishes 6.4 chosen from wrong responses:**

| Chosen (accurate review) | Rejected (overstated status) |
|---|---|
| NOT IMPLEMENTED | implements, uses, combines |
| zero hits for 'X' | the class has X |
| not referenced from project 21 | this is wired in the production path |
| gap in the execution path | the experiment is active |
| requires E34V2 but fusion is absent | BM25 and KNN are fused |

The vocabulary difference is reliable: overlap ratio 0.10–0.20.

---

**Example 14 — code review with accurate implementation status**

Prompt: `"review the retrieval path — is E34V2 BM25+KNN fusion actually in place?"`

```
CHOSEN — 6.4 Code Review:
E34V2 rank fusion: NOT IMPLEMENTED.
AstRelevanceRanker.cs contains BM25 (E04 — implemented) and KNN (E09 —
implemented) as separate signals. Zero hits for "fusion", "alpha", "blend",
or "fuse" in the file. The +8% hit@1 requires alpha × BM25_score +
(1-alpha) × KNN_score with auto-tuned alpha — this calculation does not exist.
The two signals exist but are never combined.
Impact: the +8% hit@1 gain is left unrealised in production.

REJECTED — 6.4 Code Review, overstated:
The retrieval path uses both BM25 and KNN. Both E04 and E09 are implemented
in AstRelevanceRanker. The fusion combines them with an auto-tuned alpha parameter.
[WRONG: the fusion calculation does not exist in AstRelevanceRanker.cs.
Reporting implemented capabilities as present without checking the file is
not a review — it is a paraphrase of the experiment results list. The review
must grep the production file for the fusion logic. "Implemented" requires
evidence: the class exists AND the logic is present AND it is referenced from
project 21.]
```

**Why this pair works:** Chosen uses NOT IMPLEMENTED, zero hits, does not exist,
gap vocabulary. Rejected uses implements, uses, combines, auto-tuned vocabulary.
Non-overlapping token sets. Overlap ratio ≈ 0.10–0.15.

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

## Decision document pairs — Type D

Every A-series and D-series decision in DECISIONS-LOCKED.md is a pre-made pair.
The decision made is the chosen. The alternative that was considered and not taken
is the rejected. No rejected content needs to be constructed — the wrong answer
is already documented in the rationale.

**Why Type D pairs produce the lowest overlap ratios in this document (0.05–0.20):**
The chosen decision and the rejected alternative typically use different type names,
different dependency structures, and different implementation patterns. They were
not written to be similar — they are genuinely different architectural paths.

**Extraction pattern:**

```
Decision entry contains:
  The decision made               → chosen
  The rationale                   → the annotation source
  The alternative not taken       → rejected
    (derive from: "instead of X", "not Y because Z", prior session history)

Example from DECISIONS-LOCKED.md:
  A61: DomainClassifierUnit must be pre-seeded at static construction.
       A new() instance has _trained=false permanently — E18 +15% KNN path
       unreachable without pre-seeding.

  Alternative not taken: lazy seeding — construct with new() and seed on first
  runtime prompt arrival.

Extracted pair:
  A: "should DomainClassifierUnit be seeded at static init or lazily
      on first prompt?"

  Chosen (A61 — locked decision):
    "Pre-seed at static construction via BuildSeededClassifier(). Seed with
     one representative predicate profile per D-04 domain. This ensures
     _trained=true on the first call. The E18 +15% KNN path requires
     _trained=true — it is permanently unreachable with new() + no Train()."

  Rejected (alternative not taken):
    "Lazy seeding: construct with new() and seed on first prompt arrival.
     This avoids startup cost and allows seed data to come from real
     runtime prompts rather than hardcoded predicate profiles."
  [WRONG: lazy seeding means _trained=false for all prompts before the first
  runtime seed. Health checks, warm-up calls, and any prompt that arrives
  before the seed runs all produce keyword-fallback domain classification.
  The E18 gain requires the KNN path from the first call — not eventually.
  Startup cost is negligible; the cost of wrong classification on early calls
  is not.]
```

**Why the annotation must name the consequence, not the rule:**

The annotation `[WRONG: lazy seeding violates A61]` contributes no distributional
difference — "lazy", "seeding", "A61" are in both sides. The useful annotation
names what goes wrong in production: `health checks`, `warm-up calls`, `wrong
classification`, `from the first call`. These tokens are not in the rejected
response. They widen the distributional gap without inventing new content.

**D-series decisions as ARCH rule-compliance pairs:**

D-series decisions define project-wide rules (D-11: no external ML frameworks,
D-06: replay buffer 70/30). These produce ARCH/IMPLEMENT pairs where chosen
follows the rule and rejected violates it.

```
Prompt:   "implement the capability trainer's replay mechanism"

Chosen (follows D-06 and D-11):
  "Use project 16's existing ReplayBuffer.cs — 70% current examples, 30%
   replay from prior manifests. Cap at DefaultMaxCapacity=4096. Call
   ClearCache() at batch boundary to release memory."

Rejected (violates D-11: external ML framework):
  "Add ML.NET's memory-efficient replay implementation. It handles the 70/30
   split automatically and supports dynamic capacity adjustment based on RAM."
[WRONG: D-11 prohibits external ML frameworks. ML.NET is an external ML
framework. The replay mechanism must use project 16's existing ReplayBuffer.cs
which is already proven in production. ML.NET would require adding a dependency
that conflicts with the project's core architectural constraint.]
```

**Overlap ratio for D-series rule-violation pairs: 0.05–0.15.** Chosen uses
internal vocabulary (ReplayBuffer.cs, ClearCache, DefaultMaxCapacity). Rejected
uses the prohibited framework name (ML.NET) — entirely non-overlapping.

---

## Contract accuracy pairs — Type E

When a document has been revised and an older version described a contract that
is now superseded, the old contract is a ready-made rejected. The current version's
contract is the chosen. These pairs teach the model to recognise superseded
vocabulary — the most important class of pairs for avoiding confident wrong answers.

**Why Type E pairs have the lowest overlap ratios of any pair type (0.05–0.15):**
Old and new contracts use entirely different type names. The token sets are
non-overlapping because the renaming was intentional — the change was made
precisely because the old names caused confusion.

**The key superseded contracts in the XIIGen corpus:**

| Superseded (rejected) | Current (chosen) | Document that changed it |
|---|---|---|
| Node 01 receives `UserPrompt` (raw string) | Node 01 receives `SelectedLogicalTransition` | DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2 |
| Model: 1 layer, hidden dim 16 | Model: 4 layers, hidden dim 32 | IMPLEMENTATION-DOCUMENTATION-v8 (A58) |
| Inference path uses raw prompt for KNN | Inference compiles prompt before KNN lookup | IMPLEMENTATION-DOCUMENTATION-v8 (A59) |
| E24 DO NOT IMPLEMENT (A46) | E24 safe with per-segment contract (A50) | PROJECT-HISTORY-v6 Part 16 Addendum |
| Layer 1 transition orchestrator planned | Layer 1 locked out (A47) | WHAT-TO-DO-WHAT-NOT-TO-DO-v11 |

**Three-check rule before building any Type E pair:**
1. Is the "rejected" from a genuinely superseded version? Check the document
   state register — if the old version is marked current, do not use its content
   as rejected.
2. Does a newer version explicitly state the change? If only implied, do not build
   the pair — ambiguous supersession is not a reliable source.
3. Is the "chosen" confirmed in the most recent version? Not enough to be in a
   recent version — must be in the current one.

---

**Example 15 — Node 01 contract: superseded vs current**

Prompt: `"what is the correct input type for Node 01 in the nine-node flow?"`

```
CHOSEN (current — v2+ contract):
  "Node 01 receives SelectedLogicalTransition. The transition orchestrator has
   already selected the transition before Node 01 runs. Node 01 splits that
   selected transition into DomainPackets — it does not re-classify the raw
   prompt. Contract: ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>."

REJECTED (superseded — v1 contract):
  "Node 01 receives UserPrompt. It is the first stage that sees the developer's
   raw text and produces domain packets from it. Contract:
   ILogicalUnit<UserPrompt, DomainPacket[]>."
[WRONG: the v1 contract was superseded in DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.
Routing and transition selection happen before Node 01. Node 01 receives the already-
decided SelectedLogicalTransition, not the raw prompt. A pair or plan that passes
UserPrompt to Node 01 builds on a contract that no longer exists in the codebase.]
```

**Why this pair works:** Chosen tokens: `SelectedLogicalTransition`, `already selected`,
`splits`. Rejected tokens: `UserPrompt`, `first stage`, `raw text`. Entirely
non-overlapping type names. Overlap ratio ≈ 0.05 — the strongest pair type in this
document.

---

**Example 16 — model configuration: superseded vs current**

Prompt: `"what are the current model configuration values for the capability trainer?"`

```
CHOSEN (current — A58, locked):
  "4 layers, hidden dimension 32, feed-forward hidden 64, 4 attention heads,
   2 KV heads. 69,920 parameters, 273 KB per checkpoint. This was locked in
   decision A58 — the 1-layer configuration was insufficient to learn predicate
   patterns."

REJECTED (superseded — pre-A58):
  "1 layer, hidden dimension 16, feed-forward hidden 32, 4 attention heads,
   2 KV heads. 18,720 parameters. This is the tiny config used in the teaching
   trainer."
[WRONG: the 1-layer 16-dim configuration was superseded by A58. The calibration
run batch-20 used this config and the manifests are marked obsolete (A60). All
new runs must use the 4-layer 32-dim config. Using the old values produces a
model that cannot learn predicate patterns — the proven reason for the upgrade.]
```

**Overlap ratio ≈ 0.15.** Chosen and rejected share `layers`, `hidden`, `attention
heads`, but differ on the critical numbers (`4` vs `1`, `32` vs `16`, `64` vs `32`,
`69920` vs `18720`) and on the decision vocabulary (`locked`, `superseded`, `A58`,
`obsolete`).

---

## Structural completeness pairs for PLAN — Type F

A plan that describes work without executable Gate B commands looks like a PLAN
output but cannot be dispatched to an executor. This is a structural completeness
failure — harder to detect than wrong dependency order because the content may
be entirely correct.

**Why Type F pairs are different from Type B:** Type B pairs have correct vs wrong
dependency order — both chosen and rejected have the same structure. Type F pairs
have complete vs incomplete structure — the rejected is missing a required component.

**Why the overlap ratio is higher than Type B (0.25–0.40):** Chosen and rejected
describe the same implementation work in the same phases. The difference is the
presence or absence of Gate B commands. The annotation carries more of the signal
than in Type A pairs.

**The Gate B requirement:**
Every phase in a plan dispatched to an executor must include exact bash commands
that define what "done" looks like. A plan phase that says "verify tests pass" is
not executable — the executor has no command to run. A plan phase that says:
```
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: floor or higher, 0 failed
```
is executable. The executor knows exactly what to run and what the correct output is.

---

**Example 17 — plan with Gate B vs plan without Gate B**

Prompt: `"plan the Phase 1 implementation of CompiledDomainClassifierUnit"`

```
CHOSEN — PLAN with Gate B:
Branch 1 — create CompiledDomainClassifierUnit wrapper (dependency order 1):
  Create CompiledDomainClassifierUnit : ILogicalUnit<DomainClassifierRequest,
  DomainClassification> in project 21. Delegate all logic to project 23's
  DomainClassifierUnit. No domain logic in the wrapper.

  Gate B:
    dotnet build LlmFromScratch.sln 2>&1 | tail -3
    # Expected: Build succeeded, 0 errors
    dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
    # Expected: floor or higher, 0 failed
    grep -r "CompiledDomainClassifierUnit" Tests/ | head -5
    # Expected: test file present

Branch 2 — wire into orchestrator (dependency order 2):
  Update AdaptiveLogicalOrchestrator.ClassifyDomainAsync to resolve
  CompiledDomainClassifierUnit from the registry.
  Dependency: branch 1 must be complete — the class must exist before wiring.

  Gate B:
    dotnet test Tests/LlmFromScratch.Tests.csproj --filter "DomainClassifier" 2>&1 | tail -5
    # Expected: all domain classifier tests pass

REJECTED — PLAN without Gate B:
Branch 1 — create CompiledDomainClassifierUnit wrapper:
  Create CompiledDomainClassifierUnit : ILogicalUnit<DomainClassifierRequest,
  DomainClassification> in project 21. Delegate all logic to project 23.
  Verify: build succeeds and tests pass.

Branch 2 — wire into orchestrator:
  Update AdaptiveLogicalOrchestrator.ClassifyDomainAsync.
  Dependency: branch 1 must be complete.
  Verify: domain classifier tests pass.
[WRONG: "verify: build succeeds and tests pass" is not executable — it
is an instruction to verify without specifying what to run. An executor
session has no command to execute. The plan cannot be dispatched. Gate B
must contain exact bash commands with expected output. Without this, the
plan looks complete but cannot be acted on — and cannot be mechanically
checked for completion.]
```

**Why the annotation is load-bearing here:** The content difference between chosen
and rejected is structural (bash commands vs "verify" prose). The vocabulary overlap
is moderate (~0.35) because both describe the same implementation work. The annotation
must name the specific failure mode: "instruction to verify without specifying what
to run" + "no command to execute" + "cannot be dispatched". These tokens are present
in chosen's context but absent from rejected.

---

## Gap remediation pairs — Type G

Gap remediation documents (U14 — GAP-IMPLEMENTATION-PLAN, FIX-PLAN, REMEDIATION-PLAN,
PER-FLOW-FIX-MAP) record an issue-to-fix chain: what was wrong, what was attempted
first, and what the correct fix was. This three-step structure is a pre-made DPO pair
that requires no rejected construction — the wrong fix is already documented.

**Why Type G pairs produce better margin than constructed pairs:** The wrong fix
was produced by a real reasoning path, not written to sound wrong. It is often
plausible and technically coherent — it just targets the wrong call site, wrong
guard, or wrong layer. This natural plausibility means the token distributions are
closely matched to the correct fix, but the annotation tokens (which name the
failure reason) break the tie reliably.

**Extraction pattern:**

```
Gap document entry:
  issue: "E18 KNN path unreachable — _trained always false"
  wrong_fix: "added Train() call inside ClassifyBaseDomain()"
  correct_fix: "BuildSeededClassifier() at static init — _trained=true before any call"
  reason: "Train() runs after the _trained guard check; static init runs before it"

Extracted pair:
  A:        developer-language prompt (not the gap tracking label)
             "fix the domain classifier KNN path — it never activates"

  Chosen:   "BuildSeededClassifier() at static construction. Seed with one
             representative predicate profile per domain. _trained=true before
             the first prompt reaches the guard check."

  Rejected: "Call Train() at the top of ClassifyBaseDomain()."

  [WRONG: Train() inside ClassifyBaseDomain() executes after the _trained guard
  check — the guard fires first and falls to ClassifyByKeywords before Train()
  runs. The only call site that executes before the guard is static construction.
  A fix that runs after the guard cannot change what the guard decides.]
```

**False-pass entries are the highest-value form in this document type.**
A false pass is where a gate claimed success but the implementation was wrong.
The false pass becomes the rejected for an ArbiterUnit pair; the correctly-failing
gate output becomes chosen.

```
False-pass entry:
  gate: "portability check P-3"
  false_pass_finding: "no hardcoded tenant identifiers found — pass"
  actual_issue: "tenantId is set from a config constant, not from IExecutionContext"
  correct_finding: "config constant 'acme-corp' violates P-3 — reprocess"

ArbiterUnit pair:
  A: ArbiterReviewRequest for portability P-3 check on this flow

  Chosen (reprocess — correct detection):
    verdict: reprocess
    finding: "tenantId sourced from config constant 'acme-corp' violates P-3.
              Config constants are deployment-time literals that behave identically
              to hardcoded strings under the portability protocol."

  Rejected (false pass — what the gate incorrectly produced):
    verdict: pass
    finding: "no hardcoded tenant identifiers found"
  [WRONG: the false pass searched for string literals but not for config constants
  that resolve to tenant identifiers at startup. P-3 covers both. A config constant
  'acme-corp' fails the same test as a literal — it prevents the flow from serving
  any tenant other than acme-corp.]
```

**Annotation rule for Type G:** The annotation must name the *mechanism of the
wrong fix's failure*, not just "this is wrong". For wrong-fix pairs: name the call
site ordering that makes it fail. For false-pass pairs: name the gap in the gate's
detection logic that caused the miss.

**Overlap ratio:** 0.10–0.25 for wrong-fix pairs. 0.15–0.30 for false-pass pairs.
Wrong fix and correct fix use different call sites (ClassifyBaseDomain vs static init).
False pass and correct detection use different verdict vocabulary (pass vs reprocess).

**Pair kind:** `gap_remediation`

**Excluded use:** Never use gap tracking labels as A-sides. "GAP-007: E18 KNN
unreachable" is provenance, not a developer prompt. Convert to developer-language
before use.

---

## Generated code and debug correction pairs — Type H

Generated code debug histories (U23 — `css-debug-history.json`, `generated_code.txt`,
`CORRECTED-DEFECT-CATALOG`) record the full cycle of a code generation or repair task:
what was generated, what was defective, and what the correct version is. This is
structurally identical to correction events (Type 3) but in the implementation domain
rather than the planning or architecture domain.

**Why Type H pairs are distinct from Type D (decision) and Type E (contract):**
Type D and E pairs are about architectural choices and contracts. Type H pairs are
about implementation correctness — the wrong code does something the correct code
does not, at the method or expression level.

**Extraction pattern:**

```
Debug history entry:
  task: "fix the tenant isolation check in JWT middleware"
  generated: "if (token.TenantId == request.TenantId)"
  defect: "case-sensitive comparison fails for mixed-case tenant IDs"
  corrected: "if (token.TenantId.ToLower() == request.TenantId.ToLower())"
  defect_class: "reasoning error — missed normalisation requirement"

Extracted pair:
  A:        "fix the tenant isolation check in the JWT middleware"

  Chosen:   "Normalise both sides before comparison:
             token.TenantId.ToLower() == request.TenantId.ToLower()
             DevelopmentPromptCompiler normalises tenant IDs to lowercase.
             Auth providers may return mixed case. Case-sensitive comparison
             silently passes for exact-match tenants and silently fails for
             tenants whose IDs differ only in case."

  Rejected: "if (token.TenantId == request.TenantId) return true;"

  [WRONG: case-sensitive comparison appears correct in isolation but fails when
  the auth provider returns a differently-cased tenant ID from the one stored in
  the compiled prompt. The defect is silent — no exception, no log entry,
  just a false negative for tenants whose IDs differ only by case.]
```

**For CSS and UI generation debug histories:**

```
Task:       "render the tenant dashboard header with the correct font weight"
Generated:  "font-weight: bold;" (resolved to 700 by browser)
Defect:     "design token specifies --font-weight-heading: 600; bold resolves to 700"
Corrected:  "font-weight: var(--font-weight-heading);"

A:        "render the tenant dashboard header with the correct font weight"
Chosen:   "font-weight: var(--font-weight-heading); — uses the design token
           defined as 600. Hard-coding bold resolves to 700 which diverges
           from the design system."
Rejected: "font-weight: bold;"
[WRONG: bold resolves to 700 in browsers. The design system token is 600.
Using the design token is mandatory — hard-coding values that do not match
the design system creates visual inconsistency that accumulates across components.]
```

**Quality gate for Type H:** The `defect_class` field must indicate a reasoning
error, not a typo or missing import.

```
High (include):   "reasoning error", "wrong layer", "missed normalisation",
                  "wrong call site", "wrong comparison operator"
Medium (include with strong annotation):
                  "incomplete implementation", "missing edge case"
Low (exclude):    "typo", "missing import", "wrong variable name that is otherwise correct"
```

A debug history entry where the correction is `color: red` instead of `color: reds`
is Low — the only difference is a typo, not a reasoning difference. The token
distributions are almost identical.

**Overlap ratio:** 0.10–0.25 for BUG and implementation pairs (correct and incorrect
use different method calls or operators). 0.15–0.30 for CSS/UI pairs (design tokens
vs hard-coded values have moderate overlap from shared property names).

**The annotation for Type H must name the silent failure mode.** The worst
generated-code defects are ones that pass casual inspection. The annotation earns
its reward margin contribution by naming why the wrong code appears correct at first
glance but fails in a specific runtime condition.

**Pair kind:** `correction_event` (same kind as Type 3 — identical extraction pattern,
different domain vocabulary).

**Excluded use:** Do not use the generated code artifact as the A-side prompt.
The generated code is the output of a task, not the input. The task description is
the A-side.

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
| Harness wiring → Teach first row | Teach first row before harness | Taught unit is registered but selector does not exist — verification run calls deterministic unit, not taught unit |
| BuildSeededClassifier at static init → Use classifier | Use new() classifier at runtime | _trained=false permanently — E18 KNN path never reached on any prompt |

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
| 6.4 Code Review and Status Verification | Accurate wiring status, NOT IMPLEMENTED findings | 6.4 overstated status | ~0.10–0.15 — very strong |
| Decision A-series | Current decision with consequence reasoning | Superseded alternative | ~0.10–0.20 — strong |
| Decision D-series | Rule-compliant implementation | Prohibited framework/pattern | ~0.05–0.15 — very strong |
| Contract accuracy (Type E) | Current type name and contract | Superseded type name | ~0.05–0.15 — very strong |

Overlap ratios below 0.30 reliably produce positive reward margin.
Overlap ratios between 0.30 and 0.50 typically produce positive margin with a
strong annotation.
Overlap ratios above 0.50 reliably produce negative margin.

---

## Batch composition requirements

| Pair type | Minimum fraction | Notes |
|---|---|---|
| ARCH: design vs implementation (Type A) | 35% of ARCH pairs | Primary failure mode. Lowest overlap ratio. Reliable positive margin. |
| ARCH: design vs monitoring/tooling (Type A) | 20% of ARCH pairs | 3.5 and 6.1 confusion. Very low overlap ratio. |
| ARCH: design vs review (Type A) | 15% of ARCH pairs | 3.1 confused with 6.1. Moderate overlap — strong annotation required. |
| ARCH: decision trade-off (Type D) | 15% of ARCH pairs | Pre-made from DECISIONS-LOCKED.md. 0.05–0.20 overlap. No construction needed. |
| ARCH: contract accuracy (Type E) | 15% of ARCH pairs | Superseded-vs-current contracts. 0.05–0.15 overlap. Very strong signal. |
| PLAN: correct branches, wrong order (Type B) | 35% of PLAN pairs | Primary failure mode. Moderate overlap — annotation is critical. |
| PLAN: plan vs explanation (Type C) | 25% of PLAN pairs | PLAN vs CODE_Q. Strong distributional difference. |
| PLAN: plan with vs without dependencies (Type B) | 25% of PLAN pairs | Missing `depends_on`. Annotation carries most of the signal. |
| PLAN: structural completeness (Type F) | 15% of PLAN pairs | Gate B present vs absent. Overlap 0.25–0.40 — annotation load-bearing. |
| Gap remediation (Type G) | 10% of any batch | Pre-made from U14. Wrong-fix + false-pass. No construction needed. 0.10–0.30 overlap. |
| Generated code debug (Type H) | 10% of BUG/REFACTOR batches | Pre-made from U23. Natural implementation-vocabulary pairs. 0.10–0.30 overlap. |

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

□ For Type D decision pairs: rejected alternative derived from decision rationale
  or prior session history — not invented; three-check rule applied to confirm
  the decision is still current (not superseded)

□ For Type E contract pairs: three-check rule applied — old contract confirmed
  superseded, new contract confirmed current; rejected uses genuinely old type
  names, not approximations

□ For Type F structural completeness pairs: annotation names the execution failure
  ("no command to execute", "cannot be dispatched") — not just "Gate B is missing"

□ For Type G gap remediation pairs:
  — A-side is developer-language prompt, not gap tracking label
  — Wrong-fix entry: annotation names the call-site or layer ordering that makes
    it fail — not just "this is the wrong approach"
  — False-pass entry: annotation names the detection gap (what the gate missed)
    rather than just stating the verdict should be different
  — Entry rated High (reasoning error) or Medium (missing edge case) before use
  — Low entries (typos, missing imports) excluded

□ For Type H generated code debug pairs:
  — A-side is the task description or design spec, NOT the generated code artifact
  — defect_class confirmed as reasoning error, not typo
  — Annotation names the silent failure mode — why the wrong code appears correct
    at first glance but fails in a specific runtime condition
  — Quality rating applied: High/Medium (reasoning error) included,
    Low (typo/wrong-variable-name) excluded

□ For 6.4 review pairs: chosen uses NOT IMPLEMENTED + evidence (file, grep result)
  — never claims implemented without evidence path

□ For correction events: quality rated High or Medium
  (Low = phrasing-only correction, exclude)

□ TrainingText will use compiledText as promptSide — not the raw prompt string
  (verify PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md is implemented)

□ The prompt appears exactly once in this batch as chosen
```

---

## Last Updated

2026-05-08.
