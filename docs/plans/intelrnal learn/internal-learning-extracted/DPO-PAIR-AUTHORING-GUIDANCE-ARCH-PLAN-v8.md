# DPO Pair Authoring Guidance — ARCH and PLAN Domains
# Version: 8.0
# Date: 2026-05-08
# Supersedes: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v7.0

---

## What changed from v7

**2026-05-08 DTO expansion added** for the full additional registry addendum:
A01-A51, B13-B43, C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13,
H01-H08, I01-I14, J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19,
P01-P12, Q01-Q05, R01-R13, S01-S14, T01-T10, U01-U20, V01-V08,
W01-W16, X01-X16, Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08,
AE01-AE10, AF01-AF13, and AG01-AG06. These families are
now handled by a DTO-first gate: no pair may be authored from a newly added family
until `source_family`, canonicality, source locator fields, zone mapping, and
family-specific required fields are recorded. This is intentionally stricter than
filename-based classification.

**A25–A36 families added** from XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md.

**Type L added** (infrastructure as code + DB schema pairs). A25 IaC files (Terraform,
HCL) and A26 database migration files are `full_type_candidate` sources with stable
pair construction rules. L1 pairs teach resource graph and deployment dependency
ordering; L2 pairs teach migration sequence and schema constraint conformance. Both
require a security gate before use (no credentials, no production data). Pair kinds:
`flow_node` (ordering), `arbiter` (constraint conformance).

**A21 status corrected** from `full_type` to `limited_authority` per addendum ruling.
Synthetic catalogs now inherit A07 self-training risk warning.

**New excluded families** A27, A29, A30, A34 added with explicit rules. A28 and A35
added as `preprocess_then_reclassify`. A31, A33, A36 added as `limited_authority`.

**Checklist extended** with Type L, A25–A36 exclusion, and security gate checks.

**Batch composition updated:** Type L added (10% of ARCH/PLAN batches where infra/DB
is in scope).

---

## What changed from v6

**B-series pending note added.** B01–B12 families (from registry v11, not yet
published) are not yet in the decision table. A navigation note and provisional
classification mapping were added: pairs from B-series documents should use
`pending_b_series: true` and be classified under the closest U/A-series family
until registry v11 is available.

**Batch composition cross-reference updated** to reference examination method v14
(was v13). Subdivision language clarified: v6 fractions apply *within* the primary
quotas set in v14.

**Type K added** (config schema + source contract pairs from Types 25/26). Two
subtypes: K1 (schema enum violation — reprocess, not concern) and K2 (interface
boundary — exact type owner from source contract). Added to confusion table,
batch composition, and checklist.

**Naming translation pair_kind added** to confusion table and navigation table.
Lowest overlap class in the corpus (0.05–0.15). Not to be labelled `migration_plan`
or `correction_event`.

**ARCH confusion table extended** with Type K1, K2, Type J topology, and naming
translation rows.

**Canonicality gate made explicit stop rule** — now reads "STOP HERE IF FAILED,
do not proceed to any other check" with explicit record-as-canonicality-failure
instruction to enable accurate batch diagnostics.

**Missing `## Correction events as pre-made pairs` section header restored.**

**Navigation table added at document start** mapping pair types to sections in
both this document and the examination method.

---

## What changed from v5
contain labeled positive/negative examples are the lowest-effort, highest-trust
new source: chosen and rejected are already identified. Three mandatory gates before
extraction: Gate 1 label verification against current decisions (stale positives
become Type 16 rejected-vocabulary only), Gate 2 provenance check (synthetic examples
flagged), Gate 3 overlap test. Pair kind is content-dependent.

**Diagram and topology pairs added (Type J).** A13 diagram source files (`.mmd`,
`.mermaid`, `.puml`) encode dependency directions precisely — a directed edge is
unambiguous in ways that prose is not. Type J pairs are strongest for the sub-class
of ARCH pairs where the question is about a specific dependency direction. Preprocessing
rule: parse as graph, preserve nodes and edges, never flatten to prose first.

**Batch composition updated:** Type I (10% of any batch), Type J (10% of ARCH
batches) added.

**Checklist extended** with Type I and J specific checks.

---

## What changed from v4

**Canonicality gate added to pre-submission checklist.** Registry v7 introduced
the canonicality framework. Every pair in this document must now record
`source_canonicality` (canonical or extracted_copy only) and
`canonical_confirmed_via`. mirror / stale / archive_container = gate failure.

**Type G (gap remediation) extraction rule upgraded to v7 precision.** v4 described
the three-part structure but did not state the fallback rule: when a gap document
has no wrong-fix attempt recorded (Part 2 absent), use the ARCH or BUG confusion
tables to construct rejected — do not invent a wrong fix. Correct fix must have
attached evidence; a fix without evidence is a claim, not ground truth.

**Type H (generated code debug) quality gate upgraded to v7 precision.** The
overlap threshold is now explicit: if overlap_ratio > 0.45, the annotation must
introduce ≥ 5 non-overlapping tokens describing why the wrong code fails
functionally. The correct code must have a verification signal in the debug history
(build passed, test passed, visual confirmed). Pairs where the "wrong" code is a
deliberate placeholder are excluded.

**Canonicality entries added to confusion tables and batch composition notes.**

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

## Document navigation

This document covers pair construction — worked examples, confusion tables, annotation
rules, and quality gates. For layer structure, required coverage fields, source family
rules, and batch acceptance gates, use the examination method document alongside this one.

| If you are working on… | Primary section here | Also read in examination method |
|---|---|---|
| ARCH boundary pairs | Sub-phase 3.1, 3.2, 3.5, 6.1, 6.4 | ARCH confusion reference table |
| PLAN ordering pairs | PLAN domain, dependency confusion table | Node 03 SchemePlannerUnit pairs |
| Decision + contract pairs | Types D, E | Types 9, 16 in source registry |
| Gap remediation pairs | Type G | U14 source family rules |
| Generated code pairs | Type H | U23 source family rules |
| Catalog pairs | Type I | A21 source family rules + three gates |
| Diagram topology pairs | Type J | A13 source family rules |
| Config schema + interface pairs | Type K | Types 25, 26 in source registry |
| IaC + DB schema pairs | Type L | A25, A26 source family rules |
| Naming translation pairs | `naming_translation` pair_kind | B05 pending (use naming_translation kind now) |
| Coverage labels, required fields | Pre-submission checklist | Required Coverage Labels section |
| Batch quotas (primary) | Batch composition (subdivision only) | Batch composition minimums |
| Source canonicality | Pre-submission checklist (gate 1) | Canonicality Framework section |

**B-series families (B01–B12) are pending registry v11.** Until that registry is
published, pairs from B-series documents should be filed provisionally under the
closest U/A-series family with `pending_b_series: true` in the pair file. See
examination method v14 decision table for provisional classification mapping.

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

**Three-part extraction rule — mandatory before building any Type G pair:**

```
Part 1 — gap description:   context for the A-side. Never the A-side itself.
Part 2 — wrong-fix attempt: what was tried and failed → becomes rejected.
Part 3 — correct fix + evidence → becomes chosen.
```

**When Part 2 is absent** (document has only gap + correct fix): construct rejected
using the ARCH or BUG confusion tables for the relevant sub-phase. Do not invent a
wrong fix. Do not leave the pair without a rejected.

**Correct fix must have evidence.** A test result, run output, or verification
statement must be attached to Part 3. A fix without evidence is a claim, not ground
truth. Do not use it as chosen content.

**Annotation rule:** The annotation must name the *mechanism of the wrong fix's
failure* — which call site ordering prevents it, which layer it targets incorrectly,
which detection gap caused the miss. Not just "this is wrong".

**Overlap ratio:** 0.15–0.30 for wrong-fix pairs (wrong and correct call sites use
different vocabulary). 0.10–0.20 for false-pass pairs (pass vs reprocess verdict
vocabulary).

**Pair kind:** `gap_remediation`

**Excluded use:**
```
Never use gap tracking labels ("GAP-007") as A-sides — convert to developer language.
Never invent a wrong-fix attempt if the document does not record one.
Never use a correct fix without attached evidence as chosen content.
```

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

**Quality gate for Type H — apply before including any pair:**

```
□ Overlap ratio computed:
  — If overlap_ratio > 0.45: annotation must introduce ≥ 5 non-overlapping
    tokens describing why the wrong code fails functionally (not just which
    lines differ)
  — If overlap_ratio > 0.60: exclude regardless of annotation quality

□ Debug history confirms the wrong code was actually generated (not constructed)
  — rejected must come from a real generation event, not be written to look wrong

□ Correct code has a verification signal in the debug history:
  build passed, test passed, or visual confirmed
  — a corrected version without verification is a claim, not ground truth

□ defect class:
  High (include unconditionally): "reasoning error", "wrong layer",
    "missed normalisation", "wrong call site", "wrong comparison operator"
  Medium (include with strong annotation): "incomplete implementation",
    "missing edge case"
  Low (exclude): "typo", "missing import", "wrong variable name that is
    otherwise correct", "placeholder not replaced"

□ Rejected is genuinely wrong — not a deliberate placeholder or stub.
  Include only defects that represent real generation failures.
```

**Typical overlap:** 0.25–0.50. Generated code corrections share more structure
than conversation corrections (Type 3). Annotation weight is higher.

**The annotation must name the silent failure mode.** The worst generated-code
defects pass casual inspection. The annotation earns its reward margin contribution
by naming why the wrong code appears correct at first glance but fails in a
specific runtime condition.

**Pair kind:** `correction_event` (same kind as Type 3 — identical extraction
pattern, different domain vocabulary).

**Excluded use:**
```
Do not use generated code artifact as A-side — it is the output, not the input.
Do not include pairs where the "wrong" version is a deliberate placeholder.
Do not include pairs without a verification signal on the correct version.
```

## Positive/negative example catalog pairs — Type I

Positive/negative example catalogs (A21 — `POSITIVE-NEGATIVE-EXAMPLES.md`,
`CORRECTED-DEFECT-CATALOG`, example banks, failure/success lists) contain
labeled examples where chosen and rejected are already identified. No rejected
construction is required — extraction, verification, and quality rating are the
only work.

**Why Type I pairs are the lowest-effort, highest-trust new source:** The catalog
author already identified the contrast. The chosen and rejected were written to
demonstrate a meaningful difference, so overlap is typically < 0.30 — stronger than
most constructed pairs. The risk is that the label was correct when written but a
later decision superseded it.

**Three mandatory gates before extraction:**

```
Gate 1 — Label verification (most important):
  Check every "positive" / "correct" / "pass" example against:
    — current Type 9 locked decisions (A-series and D-series)
    — document state register (current versions of all referenced contracts)
  If a positive example is superseded by a later decision: it becomes Type 16
  rejected-vocabulary — use only as rejected, never as chosen.

Gate 2 — Provenance:
  Confirm the example came from a real session or real artifact.
  Synthetic examples (written to fill a coverage gap) must be marked:
    source_canonicality: synthetic
  Apply extra scrutiny: the example was never tested against the actual system.

Gate 3 — Overlap test:
  Compute overlap_ratio for each pair.
  If > 0.45: annotation must introduce ≥ 5 non-overlapping tokens.
  If > 0.60: exclude regardless of annotation.
```

**Extraction example:**

```
Catalog entry:
  correct: "CompiledPredicateIds: ['TEST', 'SECURITY'] —
            'test coverage' triggers TEST as the primary verb;
            'auth module' is the object, not the action"
  wrong:   "CompiledPredicateIds: ['SECURITY', 'AUTHORIZATION'] —
            misidentifies the object as the primary predicate"

After Gate 1 (label verified against current predicate lexicon — no changes):
After Gate 2 (came from calibration run analysis — real session, not synthetic):
After Gate 3 (overlap ratio 0.15 — passes):

Extracted pair:
  A: "test coverage shows 42% on the authentication module"
  Chosen: CompiledPredicateIds: ["TEST", "SECURITY"] — confirmed correct
  Rejected: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  [WRONG: 'test coverage' is the primary verb signal — the action is measurement
  of coverage. 'authentication module' is the object being measured. TEST takes
  priority. AUTHORIZATION has no activation term in this prompt.]
```

**Annotation for Type I pairs:** The catalog often provides the annotation source
directly (the "why it's wrong" description next to the negative example). Use it.
If no annotation is present, apply the standard ARCH/BUG/PLAN annotation rules.

**Pair kind:** depends on content — `adaptive_routing_compiler`, `arbiter`,
`flow_node`, or `correction_event` based on what the examples demonstrate.

**Overlap ratio:** typically < 0.30 — example catalogs are designed to contrast.

**Excluded use:**
```
Never skip Gate 1. A stale positive example as chosen trains the model on
a decision that has been superseded.
Never use synthetic examples without the synthetic flag.
```

---

## Diagram and topology pairs — Type J

Diagram source files (A13 — `.mmd`, `.mermaid`, `.puml`, `.drawio`,
`DEPENDENCY-GRAPH.mmd`) encode the same component relationships as prose
architecture documents but more precisely: nodes and edges are explicit, and
wrong directions are objectively distinguishable from correct ones.

**Why Type J pairs differ from Type 2 (formal architecture documents):**
Type 2 uses prose to describe boundaries — "the compiler depends on the router"
may be ambiguous about direction. A diagram shows `CompilerUnit → RouterUnit`
as a directed edge — the direction is unambiguous. This makes Type J pairs
stronger for the sub-class of ARCH pairs where the question is about a specific
dependency direction.

**How to use for ARCH dependency direction pairs:**

```
A: "which unit receives the compiler's output — the router or the planner?"

Chosen (from DEPENDENCY-GRAPH.mmd — edge: CompilerUnit → RouterUnit):
  "MultiSubPhaseRouterUnit receives SubPhaseRouterSourceDocument from
   PromptRoutingInputCompilerUnit. The dependency graph shows a directed
   edge CompilerUnit → RouterUnit. SchemePlannerUnit is downstream of the
   router — it receives translated domain statements, not the compiled prompt."

Rejected (wrong dependency direction):
  "SchemePlannerUnit receives the compiled output directly — it reads the
   predicate list and plans the branches."
[WRONG: DEPENDENCY-GRAPH.mmd shows no direct edge from CompilerUnit to
SchemePlannerUnit. The compiler's output goes to the router first; the
router routes each predicate to sub-phases; SchemePlannerUnit receives
translated domain statements from Node 02 (DomainTranslatorUnit), not
the compiled predicates directly.]
```

**How to use for architecture review pairs (circular dependency detection):**

```
A (ArbiterReviewRequest):
  artifact_type: "dependency_graph"
  artifact_content: "ProjectA references ProjectB; ProjectB references ProjectA"
  review_criteria: "verify no circular dependencies per architecture contract"

Chosen (reprocess — circular dependency detected):
  verdict: reprocess
  finding: "DEPENDENCY-GRAPH.mmd shows ProjectA → ProjectB → ProjectA.
            This is a circular dependency. The architecture contract prohibits
            circular references between projects. Neither project can be built
            without the other, creating a build graph cycle."

Rejected (concern — wrong severity):
  verdict: concern
  finding: "ProjectA and ProjectB have mutual references that may create
            some coupling concerns."
[WRONG: a circular dependency is a build failure, not a coupling concern.
'May create coupling' understates a defect that prevents compilation.
The correct verdict is reprocess — the circular reference must be broken
before the architecture is acceptable.]
```

**Preprocessing rule:** Parse the diagram as a graph before extracting pairs.
Preserve nodes and edges. Do not flatten to prose before the extraction — flattening
loses edge direction, which is the primary pair signal.

**Three-check rule applies:** Confirm the diagram is the current version via the
document state register before using as chosen content.

**Overlap ratio:** 0.10–0.20 for dependency direction pairs (correct and wrong
directions use different node names in the dependent position).

**Pair kind:** `topology_graph` (topology pairs), `arbiter` (review pairs).

**Excluded use:**
```
Do not include raw diagram DSL syntax (Mermaid, PlantUML) as A-side.
A-side is a developer-language question about the architecture.
Diagram text is B-side evidence — authority for what the correct answer is.
```

---

## Configuration schema and source contract pairs — Type K

Configuration schema files (Type 25 — `*.schema.json`) and source code contract
files (Type 26 — interface definitions, DTO declarations, enum types) are the
strongest available sources for field-level accuracy pairs. Schema conformance is
binary; interface method signatures are exact. These are stronger than Type 2 prose
for any pair where the correct answer depends on an exact field name, enum value,
or type signature.

**Why Type K is distinct from Type E (contract accuracy):** Type E pairs teach the
model to prefer the *current version* over a *superseded version* of the same
contract. Type K pairs teach the model to prefer the *schema-grounded* answer over
an *inferred* answer for the same current contract. The failure mode is different:
Type E rejected = old vocabulary; Type K rejected = vocabulary inferred from behavior
rather than read from the source file.

**Subtype K1 — Configuration schema violation pairs (from Type 25):**

```
A: "what values does checkoutBehavior accept in the availability rules config?"

Chosen (K1 — schema-grounded):
  "checkoutBehavior accepts exactly three values per availability_rules.schema.json:
   'inline', 'modal', and 'redirect_internal'. Any other value fails schema validation."

Rejected (K1 — inferred without checking schema):
  "checkoutBehavior accepts 'inline', 'popup', 'modal', or 'redirect' based on
   how the checkout needs to behave in the UI."
[WRONG: 'popup' and 'redirect' are not in the schema enum. Inferring allowed values
from behavior vocabulary without reading the schema produces answers that cause
config validation failures at runtime.]
```

**ArbiterUnit pair from schema violation (K1):**

```
A (ArbiterReviewRequest):
  artifact_type: "config_object"
  artifact_content: { "maxCartItems": -1, "checkoutBehavior": "redirect_external" }
  review_criteria: "validate against availability_rules.schema.json"

Chosen (reprocess — schema violation is binary, not a concern):
  verdict: reprocess
  finding: "maxCartItems must be >= 0 per schema. checkoutBehavior 'redirect_external'
            is not in enum ['inline', 'modal', 'redirect_internal']. Two violations.
            Schema conformance is binary — a value either satisfies the schema or it does not."

Rejected (concern — wrong severity):
  verdict: concern
  finding: "maxCartItems is -1 which seems low."
[WRONG: schema violation is not opinion — a value outside the enum is objectively
invalid. "Seems low" replaces a binary schema check with subjective assessment.
When a schema violation exists, the correct verdict is reprocess, not concern.]
```

**Subtype K2 — Source code interface boundary pairs (from Type 26):**

```
A: "who owns the routing decision — the compiler or the router?"

Chosen (K2 — interface-grounded):
  "The router owns the routing decision. The compiler produces
   SubPhaseRouterSourceDocument (its output type). The router
   consumes that and produces SubPhaseRoutingDecision[] (its output type).
   Ownership is defined by which unit produces the decision object —
   that is the router."

Rejected (K2 — boundary confused):
  "The compiler handles routing — it compiles the prompt and decides which
   sub-phases are relevant. The router just formats the output."
[WRONG: interface contracts show the split precisely. The compiler's output type
is SubPhaseRouterSourceDocument — it produces input for the router. The router's
output type is SubPhaseRoutingDecision[] — it makes the decisions. Calling the
compiler's work "routing" misidentifies which interface owns the decision.]
```

**Three-check rule applies to Type K:** Confirm the schema file and source contract
are the current versions via the document state register before using as chosen content.
A schema that was updated since the pair was written may have added or removed enum values.

**Overlap ratio:** 0.10–0.20 for K1 schema pairs (schema field vocabulary vs inferred
behavior vocabulary). 0.10–0.20 for K2 interface boundary pairs (exact type names vs
prose descriptions).

**Pair kind:** `arbiter` (K1 schema-conformance pairs), `flow_node` (K2 interface
boundary pairs).

**Excluded use:**
```
Do not derive architecture decisions from config schemas — schemas define valid values,
not why those values were chosen. Use Type 9 for rationale.
Do not use schema file text or .cs interface text as A-side input.
Do not invent enum values not present in the actual schema file.
Three-check rule is mandatory — do not assume schema is current without verification.
```

---

## Infrastructure and schema contract pairs — Type L

Infrastructure as code files (A25 — `*.tf`, `*.hcl`) and database schema / migration
files (A26 — `*.sql`, `supabase/migrations/*.sql`) are `full_type_candidate` sources
with stable pair construction rules. Both define structural contracts that are stronger
than prose descriptions for pairs where the correct answer depends on a declared
resource, dependency relationship, column constraint, or migration order.

**Why Type L is distinct from Type K (config schema / source contract):** Type K covers
application-layer contracts (JSON schema validation, C# interface boundaries). Type L
covers infrastructure and storage-layer contracts — IaC resource graphs and database
schema constraints. The authority layer is different: Type K governs runtime behavior;
Type L governs deployment topology and persistence structure.

**Subtype L1 — Infrastructure as code (A25):**

```
A: "which services must be deployed before the API gateway starts?"

Chosen (L1 — from IaC resource graph):
  "The database module and identity service must both be deployed first.
   The gateway resource declares depends_on = [module.database, module.identity].
   These dependencies are explicit in the IaC file — the gateway health check
   will fail if either prerequisite is absent."

Rejected (L1 — deployment order ignored):
  "The API gateway can start in any order; other services register after startup."
[WRONG: depends_on in IaC is a structural declaration, not a soft recommendation.
The provider enforces this order — the gateway resource is not created until both
declared dependencies succeed. Ignoring depends_on produces infrastructure state
that the IaC file explicitly prevents.]
```

**Security gate — mandatory before any L1 pair:**
```
□ No provider credentials, API tokens, or access keys in the file
□ No terraform.tfstate or .tfvars files with real values
□ State files → treat as secure_operational, exclude entirely
```

**Subtype L2 — Database schema and migration files (A26):**

```
A: "can the add_tenant_constraints migration run before create_tenants?"

Chosen (L2 — from migration SQL dependency):
  "No. add_tenant_constraints declares a foreign key on cart_items.tenant_id
   referencing tenants(id). The tenants table does not exist until
   create_tenants runs. Reversing the order causes the ALTER TABLE to fail
   at the foreign key reference."

Rejected (L2 — migration order treated as optional):
  "Migration order is configurable — they can run in parallel."
[WRONG: a foreign key cannot reference a table that does not yet exist.
create_tenants is a structural prerequisite. The database engine enforces
this — the migration will error before any rows are inserted.]
```

**Excluded use for both subtypes:**
```
Do not use IaC alternatives as chosen without a Type 9 adoption decision.
Do not expose provider credentials, state files, or customer data.
Do not train on production data dumps or seed rows with real tenant data.
Do not use IaC or schema facts as product rationale — they record decisions,
not the reasoning behind them. Use Type 9 for rationale.
```

**Typical overlap:** L1: 0.15–0.30 (resource vocabulary vs deployment-order prose).
L2: 0.10–0.25 (SQL vocabulary vs application-layer prose).

**Pair kind:** `flow_node` (deployment/migration ordering), `arbiter` (constraint
conformance and unsafe-default detection).

---



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
| Schema conformance (Type K1) | Schema-valid config / exact enum | Inferred or invalid enum value | ~0.10–0.20 — very strong |
| Interface boundary (Type K2) | Exact interface owner / DTO fields | Prose-inferred boundary | ~0.10–0.20 — very strong |
| Topology direction (Type J) | Correct directed graph edge | Wrong edge direction | ~0.10–0.20 — very strong |
| Naming translation | Current term / canonical name | Deprecated or renamed term | ~0.05–0.15 — lowest overlap class |

Overlap ratios below 0.30 reliably produce positive reward margin.
Overlap ratios between 0.30 and 0.50 typically produce positive margin with a
strong annotation.
Overlap ratios above 0.50 reliably produce negative margin.

---

## Additional Registry Family DTO Gate

The additional registry companion covers `A01-A51`, `B13-B43`, `C01-C12`,
`Z41-Z56`, `D01-D19`, `E01-E16`, `F01-F17`, `G01-G13`, `H01-H08`,
`I01-I14`, `J01-J09`, `K01-K13`, `L01-L08`, `M01-M21`, `N01-N09`,
`O01-O19`, `P01-P12`, `Q01-Q05`, `R01-R13`, `S01-S14`, `T01-T10`,
`U01-U20`, `V01-V08`, `W01-W16`, `X01-X16`, `Y01-Y10`, `AA01-AA17`,
`AB01-AB07`, `AC01-AC20`, `AD01-AD08`, `AE01-AE10`, `AF01-AF13`,
and `AG01-AG06`. These families are too varied
to classify by filename alone. A row from any additional family is invalid unless
the pair DTO records the source role, exact locator, and family-specific fields
before the chosen/rejected text is written.

Minimum DTO block for every additional-family pair:

```json
{
  "source_family": "E01",
  "source_document_family": "FLOW-DESIGN-SIM-Rx",
  "source_role": "b_side_chosen",
  "source_path": "...",
  "source_locator": {
    "section_id": "...",
    "line_start": null,
    "json_pointer": null,
    "archive_member_path": null
  },
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "...",
  "target_registry_type": "Type 2",
  "zone_id": null,
  "pair_kind": "design_simulation",
  "rejected_type": "A",
  "compiled_prompt_in_training": true
}
```

Family-specific DTO fields:

| Family group | Required DTO fields | Hard failure |
|---|---|---|
| `D01-D04`, `E01-E06`, `F11-F17` session-round artifacts | `session_id`, `flow_id`, `phase_id`, `round_id`, `artifact_id`, `verdict_or_gate_id` | merged rounds or missing round id |
| `D06`, `E16`, `F05` patch/delta/replan docs | `target_document_id`, `base_version`, `delta_id`, `before_state`, `after_state`, `supersedes` | delta used without base document |
| `D09-D10`, `E07`, `E13`, `F04`, `F07-F08` state artifacts | `state_scope`, `state_schema_version`, `state_status`, `flow_id_or_phase_id`, `completed_or_reconciled_at` | `in_progress` state used as chosen completion |
| `E02`, `E05`, `F02`, `F09` design/topology authorities | `flow_id`, `subflow_id`, `decision_id`, `topology_node_id`, `edge_id`, `arbiter_id` as applicable | decision or edge applied to wrong flow |
| `E14`, `E15`, `B26-B28`, `F10`, `F12-F14` maps/digests/reviews | `map_id`, `lookup_key`, `target_path`, `evidence_locator`, `source_backed=true` | digest or map used as canonical source |
| `Z41-Z49` Older directory archive-content families | `archive_path`, `archive_member_path`, `snapshot_id`, `snapshot_canonicality`, `reported_id_alias` | archive filename used as document authority |
| `Z50-Z56` external nested-archive families | `source_canonicality=external`, `external_source_family`, `structural_pattern_only=true`, `archive_member_path` | external content used as XIIGen-specific B-side |
| `F01` missing skill files | `skill_id`, `skill_source_path`, `recovered_skill_version`, `activation_rule_id`, `skill_boundary_quote` | skill behavior inferred from name only |
| `G01-G05` runtime and validation artifacts | `phase_id`, `flow_id`, `runtime_source_id`, `command_id` or `matrix_row_id`, `evidence_locator` | DPO metrics or plan prose used as runtime evidence |
| `G06-G11` archive bundles | `archive_path`, `archive_kind`, `content_manifest_path`, `extraction_path`, `archive_member_path` | archive filename used as pair authority |
| `G12-G13` source contracts | `flow_id`, `contract_file_path`, `export_name`, `field_names`, `seed_record_ids` or `topology_source_ids` | fields inferred from prose |
| `H01-H08` named session docs | `named_artifact_kind`, artifact-specific ids, `target_type`, `evidence_locator`, version/scope fields | broad type label without named DTO shape |
| `I01-I14` session-message outputs | `message_source_kind`, `session_type`, artifact-specific ids, version/rule ids, `thinking_or_tool_locator` when applicable | thinking/tool-call output treated as unlocated source |
| `J01-J09` active-project/training packages | `project_id`, `archive_path`, `archive_member_path`, experiment/training DTO fields, `cycle_id` when applicable | active artifact treated as stale archive or raw dump |
| `K01-K13` bash/view path families | exact loaded/opened path, runtime use, family-specific ids, parsed graph/debug fields when applicable | path mention treated as authority without DTO |
| `L01-L08` Older repository reference families | repository-era canonicality, target tool/stack/pattern id, prompt/template/config-specific fields | historical repo file used without era/tool/pattern DTO |
| `M01-M21` training/examination/live-run artifacts | JSON/YAML/JSONL parsed DTO fields, flow/cycle/run ids, verdict/metric/evidence locators | machine-readable artifact flattened into prose or filename authority |
| `N01-N09` tool-result reference families | tool_result/view/create_file locator, reference family id, protocol/template/interface/registry-specific fields | reference doc inferred from path only |
| `O01-O19` live docs-folder examination/UX families | flow id, step/phase/tenant/role/screenshot locators, paired MD/JSON or PNG/review links | live docs artifact treated as old archive or screenshot filename used as evidence |
| `P01-P12` session-statistics families | session type, matrix/tool_result origin, interface/constant/round-output-specific DTO fields | statistical count treated as source authority without concrete artifact |
| `Q01-Q05` tool-result header families | parent document id, H1/header locator, full recovered skill/remediation/query DTO fields | H1 text treated as full source or skill inferred from name |
| `R01-R13` docs.zip session process families | ZIP member path, flow id, round/session id, template-vs-instance role, gate/action ids | template instance confused with canonical template or round prompt used as output |
| `S01-S14` llm_mvp_core document families | `project_id=llm_mvp_core`, subproject/experiment/capability ids, project adoption status for skills | llm_mvp_core source used as XIIGen authority without adoption |
| `T01-T10` live governance/evidence families | version, flow id, machine-readable evidence fields, latest-version status | stale governance/report version used as current |
| `U01-U20` detailed llm_mvp_core skill families | exact skill id plus skill-specific DTO fields from the U family | broad "skill file" label used without skill-specific rule fields |
| `V01-V08` xiigen-COMPLETE canonical families | `archive_id=xiigen-COMPLETE.zip`, `canonical_nestjs=true`, module/page/test-specific fields | older Python/FLOW33 pattern used as chosen when NestJS conflicts |
| `W01-W16` llm_mvp_core experiment/data refinements | experiment id, subtype, mandatory plan/schema/result/progress fields | broad "experiment plan" label used without subtype DTO |
| `X01-X16` Genie skill library package families | `project_id=genie_skill_library`, archive/member path, stack/tool/template/recipe/pattern DTO fields | Genie source used as XIIGen authority or stack/template family collapsed |
| `Y01-Y10` module-architecture and phases_done families | archive id, phase, module/site/skill/config/deployment or translation state DTO fields | composition matrix, translated parts, and final declaration collapsed into one architecture label |
| `AA01-AA17` xiigen_v18_complete composition families | V18 law/module/matrix/manifest/site-config/flow-definition/skill-implementation DTO fields | V18 composition-layer source collapsed into generic Type 20 or filename-only skill label |
| `AB01-AB07` canonical V18 migration and architect families | v4 FINAL migration, architect skill package/reference, schema/config/manifest role fields | stale migration version or schema/config/manifest roles swapped |
| `AC01-AC20` compliance/flow-extension/plugin-factory families | requirement, flow, renumbering, append-to, validation, example polarity, or source-code mapping DTO fields | broad "flow doc", "state", "test", or "skill index" label without AC-specific DTO |
| `AD01-AD08` web-session/flow-state/consolidation families | handoff consumer, per-flow metrics, SDK audit, artifact range, validation matrix, or before/after consolidation DTO fields | analysis doc as execution, fabricated metrics, draft numbers, or false all-pass |
| `AE01-AE10` canonical extension and V37 planning families | flow boundary, append target, citation type, V37 subphase, approach library, or decision-log DTO fields | snapshot ZIP as authority, merge plan as append content, cited research/Q&A swap, or RAG index as implementation |
| `AF01-AF13` flow planning/examination/artifact verdict families | supreme skill load-order, governance version, skeleton state, transcript zone, goal anchor, simulation/TEACH-QA round, HTML verdict extraction, or confirmation target fields | raw transcript/HTML used whole, TEACH/design/planning simulation swapped, or existing family duplicated |
| `AG01-AG06` flow input/renumbering/merge-completion families | standalone ID map, applied merge offset, raw intent/response boundary, citation type, PromptOps schema/loop, or early engine extension fields | draft IDs used despite renumber map, plan used as completion, or raw user intent separated from design response |

New pair kinds permitted by these families:

```text
design_simulation
session_round_contract
patch_addendum
state_reconciliation
package_verification
requirements_traceability
skill_recovery
external_structural_pattern
runtime_evidence
archive_manifest
named_session_artifact
data_catalog_evolution
rule_stress_test
merged_canonical_document
session_type_taxonomy
role_execution_plan
visual_proof_plan
run_instruction_input
active_experiment_result
teaching_cycle_package
bash_view_path_source
drawio_topology_graph
component_debug_history
repository_reference_authority
training_run_telemetry
dpo_checkpoint_metric
training_pair_yaml
holdout_corruption_eval
holdout_rephrase_eval
teaching_ledger_audit
flow_examination_result
arbiter_analysis_pass
retroactive_hypothesis_analysis
flow_live_run_evidence
goal_gap_report
guide_b_document_production
plain_english_transform
flow_registry_reference
engine_contract_template
canonical_merge_protocol
wave_assignment
fabric_interface_contract
known_violation_registry
foundational_principle_gate
fleet_raw_input_jsonl
code_derived_state
step_teaching_contract
adaptation_surface
tenant_profile_instance
portability_audit_trail
visual_screenshot_evidence
ux_review_finding
fleet_validation
typescript_service_interface
machine_constant_rule
round_gap_matrix
round_root_cause_map
historical_rag_batch
gap_closure_plan
skill_registration_manifest
skill_gap_remediation
planning_startup_skill
architectural_decision_testing
claude_code_integration_instruction
verification_query_section
session_process_template
per_task_why_teaching
cross_flow_edge_spec
llm_subproject_registry
llm_experiment_plan
llm_capability_plan
live_portability_evidence
nestjs_canonical_source
experiment_runtime_state
gate_d_capability_format
tech_alternative_implementation
full_code_reference_extract
stack_integration_recipe
ai_tool_project_config
decision_philosophy_ladder
stack_migration_methodology
design_pattern_catalog
ml_simulation_blueprint
scale_evolution_map
rag_reference_index
use_case_template_guide
boilerplate_starter_template
iac_template
unified_library_entrypoint
skill_library_master_plan
genie_package_boundary
capability_matrix_analysis
module_architecture_reference
ai_agent_composition_guide
consolidation_prompt
prompt_usage_guide
v18_architectural_law
v18_site_config_instance
v18_flow_definition_json
v18_skill_implementation_prompt
v18_config_schema
architect_skill_package
architect_reference_protocol
versioned_stack_migration_correction
flow_compliance_requirement
flow_renumber_map
canonical_validation_report
web_session_handoff_governance
sdk_audit_provider_plan
document_consolidation_record
canonical_flow_extension
cited_domain_research
approach_library_rag_index
session_decision_rag_index
versioned_adaptive_plan
supreme_skill_load_order
flow_examination_governance
skeleton_fix_plan
session_transcript_source
goal_anchoring
planning_simulation_training
deep_design_simulation
teach_qa_bootstrap
teach_qa_correction
autonomous_execution_plan
simulation_patch
design_review_html_verdict
standalone_renumber_map
merge_completion_evidence
flow_initiation_intent_response
prompt_improvement_genesis
early_engine_extension
```

The pair kind must name the reason the source is useful. For example, `Z52` GSD
runtime state uses `external_structural_pattern`; it does not use `flow_node`
because the external todo-app content is not XIIGen flow authority.

---

## Batch composition requirements

**Relationship with examination method:** The examination method (v14) sets the
primary batch quotas: Type A ≥ 40%, Type B ≥ 20%, Type C ≥ 30%, Stage 9 corruption
rows in every batch. The subdivisions below apply *within* those quotas — they tell
you how to distribute ARCH-specific pairs inside the 40% Type A allocation, and
PLAN-specific pairs inside the 20–30% Type B/C allocation.

| Pair type | Minimum fraction | Notes |
|---|---|---|
| ARCH: design vs implementation (Type A) | 35% of ARCH-type-A pairs | Primary failure mode. Lowest overlap ratio. Reliable positive margin. |
| ARCH: design vs monitoring/tooling (Type A) | 20% of ARCH-type-A pairs | 3.5 and 6.1 confusion. Very low overlap ratio. |
| ARCH: design vs review (Type A) | 15% of ARCH-type-A pairs | 3.1 confused with 6.1. Moderate overlap — strong annotation required. |
| ARCH: decision trade-off (Type D) | 15% of ARCH pairs | Pre-made from DECISIONS-LOCKED.md. 0.05–0.20 overlap. No construction needed. |
| ARCH: contract accuracy (Type E) | 15% of ARCH pairs | Superseded-vs-current contracts. 0.05–0.15 overlap. Very strong signal. |
| PLAN: correct branches, wrong order (Type B) | 35% of PLAN-type-B pairs | Primary failure mode. Moderate overlap — annotation is critical. |
| PLAN: plan vs explanation (Type C) | 25% of PLAN-type-C pairs | PLAN vs CODE_Q. Strong distributional difference. |
| PLAN: plan with vs without dependencies (Type B) | 25% of PLAN-type-B pairs | Missing `depends_on`. Annotation carries most of the signal. |
| PLAN: structural completeness (Type F) | 15% of PLAN pairs | Gate B present vs absent. Overlap 0.25–0.40 — annotation load-bearing. |
| Gap remediation (Type G) | 10% of any batch | Pre-made from U14. Wrong-fix + false-pass. No construction needed. 0.10–0.30 overlap. |
| Generated code debug (Type H) | 10% of BUG/REFACTOR batches | Pre-made from U23. Natural implementation-vocabulary pairs. 0.10–0.30 overlap. |
| Positive/negative catalogs (Type I) | 10% of any batch | Pre-made from A21. Lowest construction effort. Three gates required. < 0.30 overlap. |
| Diagram/topology pairs (Type J, `topology_graph`) | 10% of ARCH batches | From A13 diagram files. Strongest direction-clarity pairs. 0.10–0.20 overlap. |
| Config schema + source contract pairs (Type K) | 10% of CODE_Q/ARCH batches | From Types 25/26. Schema enum violations and interface boundary pairs. 0.10–0.25 overlap. |
| IaC and schema contract pairs (Type L) | 10% of ARCH/PLAN batches where infra/DB in scope | From A25/A26. Resource graph and migration order pairs. 0.10–0.30 overlap. |

---

## Pre-submission checklist

Before adding any pair to a batch:

```
□ CANONICALITY GATE — STOP HERE IF FAILED. Do not proceed to any other check.
  source_canonicality recorded: canonical or extracted_copy only
  — mirror / stale / archive_container / secure_operational → STOP. Exclude pair entirely.
    Record rejection reason as "canonicality: {class}" not "failed overlap" or other.
  — external → STOP. Must go in a separate external-only batch file; mixed_with_internal: false.
  Failure here means the source is invalid. No amount of overlap quality makes it valid.

□ canonical_confirmed_via recorded: names the document state register or
  "What changed" header that confirms this is the current version

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
  — Three-part structure confirmed: gap (context) / wrong-fix (rejected) / correct-fix+evidence (chosen)
  — If wrong-fix absent in document: use ARCH/BUG confusion tables for rejected; do not invent
  — Correct fix has attached evidence (test result, run output, verification); claim without evidence excluded
  — A-side is developer-language prompt, not gap tracking label
  — Annotation names the mechanism: call-site ordering, layer, detection gap — not just "this is wrong"

□ For Type H generated code debug pairs:
  — A-side is the task description / design spec — NOT the generated code artifact
  — overlap_ratio computed before submission:
      if > 0.45: annotation must introduce ≥ 5 non-overlapping tokens on why wrong code fails functionally
      if > 0.60: exclude regardless of annotation
  — Correct code has verification signal (build passed / test passed / visual confirmed)
  — Rejected is from a real generation event — deliberate placeholders excluded
  — defect_class: High/Medium (reasoning error) included; Low (typo/missing-import) excluded

□ For Type I positive/negative catalog pairs:
  — Gate 1: every "positive" example verified against current Type 9 decisions and
    document state register — stale positives become Type 16 rejected-vocabulary only
  — Gate 2: provenance confirmed — synthetic examples flagged source_canonicality: synthetic
  — Gate 3: overlap_ratio computed — same thresholds as Type H apply
  — Annotation sourced from catalog's own "why wrong" description if present

□ For Type J diagram/topology pairs:
  — pair_kind must be `topology_graph` (not `flow_node`) for topology evidence pairs
  — Diagram parsed as graph (nodes + edges preserved) before extraction
  — Raw DSL syntax (Mermaid, PlantUML) never appears in A-side
  — Three-check rule applied: diagram version confirmed current
  — ArbiterUnit pairs: verdict severity matches defect — circular dependency = reprocess

□ For Type K config schema and source contract pairs:
  — K1 (schema): enum values in chosen are present verbatim in the schema file —
    never inferred from behavior vocabulary
  — K2 (interface): interface/DTO fields in chosen match the source contract exactly
  — Three-check rule mandatory for both subtypes — schema and contract versions confirmed current
  — Schema violation pairs: verdict is reprocess not concern — schema conformance is binary

□ For Type L infrastructure and schema pairs:
  — L1 (IaC): security gate applied — no credentials, state files, or real .tfvars values
  — L1: resource/dependency graph derived from actual depends_on declarations, not assumed
  — L2 (DB migration): migration order derived from actual FK/constraint dependency chain,
    not from assumed execution order
  — L2: no production data or customer seed rows in pair content
  — Both subtypes: IaC alternatives and schema variants not used as chosen without a Type 9
    adoption decision naming that configuration

□ For A25–A36 families generally:
  — A27 (publish profiles): confirmed as sanitized sample with placeholder values only —
    any real credential → exclude as secure_operational
  — A29/A30/A34: excluded entirely — not inspected as text sources
  — A28/A35: preprocessing confirmed complete before classification
  — A31 (web server configs): application API authority from Type 26 / Type 2, not web.config
  — A33 (IDE metadata): AGENTS.md and Type 14 take precedence over IDE settings

□ For additional registry families A01-A51 / B13-B43 / C01-C12 / Z41-Z56 / D01-D19 /
  E01-E16 / F01-F17 / G01-G13 / H01-H08 / I01-I14 / J01-J09 / K01-K13 /
  L01-L08 / M01-M21 / N01-N09 / O01-O19 / P01-P12 / Q01-Q05 /
  R01-R13 / S01-S14 / T01-T10 / U01-U20 / V01-V08 / W01-W16 /
  X01-X16 / Y01-Y10 / AA01-AA17 / AB01-AB07 / AC01-AC20 / AD01-AD08 /
  AE01-AE10 / AF01-AF13 / AG01-AG06:
  — source_family and source_document_family recorded exactly
  — source_locator present (section, line, json_pointer, or archive_member_path)
  — target_registry_type recorded when the family maps into Type 2/4/8/9/13/16/19/22/23/29/32/34
  — family-specific DTO fields from "Additional Registry Family DTO Gate" present
  — no filename-only classification; the row cites the field/section that made the type true

□ For Z50-Z56 external nested-archive families:
  — source_canonicality = external
  — structural_pattern_only = true
  — no XIIGen-specific B-side authority unless a Type 9 adoption decision is cited

□ For F01 missing skill files:
  — recovered SKILL.md content present before Type 20 B-side pair construction
  — activation rule and skill boundary quoted from recovered skill, not inferred from session mention

□ For G01-G13 runtime/archive/contract artifacts:
  — G01-G04 runtime artifacts record flow_id/phase_id and runtime source id
  — G05 validation matrix uses matrix row id and Covered By artifact, not plan title
  — G06-G11 archives have manifest + member-level classification before pair construction
  — G12-G13 contract fields extracted from TypeScript source, not prose

□ For H01-H08 named session documents:
  — named_artifact_kind recorded
  — target_type recorded only after subtype identification
  — certificates/comparisons/DoD/catalogs use scope/version fields, not broad document title

□ For I01-I14 session-message outputs:
  — thinking/tool-call/message locator recorded when source was discovered inside session content
  — I01 task catalog version and task_type_id recorded
  — I02/I03 rule_family recorded as CF or ST; never mixed
  — I04 merged document records source_versions and continuity_check_passed
  — I13/I14 session type label used only to route produced artifacts to concrete families

□ For J01-J09 active-project/training artifacts:
  — active project id and exact archive/member path recorded
  — J05 experiment result JSON includes baseline/enhanced/improvement/next_action fields
  — J09 teaching cycle rows include cycle_id, layer_id, ledger row, checkpoint path

□ For K01-K13 bash/view path families:
  — exact path observed in bash/view layer recorded
  — K01 input instruction is marked source_prompt_role=input_instruction
  — K07 DrawIO parsed graph fields present before topology pair construction
  — K08 component CSS debug history includes component_id and verification signal

□ For L01-L08 repository reference families:
  — repository-era canonicality and target tool/stack/pattern identity recorded
  — L07 secret templates contain only sanitized `$secret:` / `$env:` references

□ For M01-M21 training/examination/live-run artifacts:
  — JSON/YAML/JSONL DTO fields parsed directly, not summarized from filename
  — M05-M09 distinguish training rows, eval holdouts, ledgers, and cycle summaries
  — M13-M18 record flow id, hypothesis/goal/verdict id, and evidence locator

□ For N01-N09 tool-result reference families:
  — tool_result/view/create_file locator recorded
  — N02/N03 template/protocol artifacts are used as procedure authority only
  — N05/N09 interface or module-map graph edges extracted explicitly

□ For O01-O19 live docs-folder families:
  — live docs are marked current working docs, not historical archive
  — O07/O08 adaptation surface and tenant profile are not swapped
  — O13/O14 screenshot evidence is linked to UX review finding row

□ For P01-P12 session-statistics families:
  — source artifact path or tool_result locator exists; count alone is not evidence
  — P01 uses TypeScript interface DTO fields, not C# interface defaults
  — P02 records constant_id plus rule text
  — P03/P04 distinguish round output artifacts from round plans

□ For Q01-Q05 tool-result header families:
  — parent document id and H1/header locator recorded
  — Q01 records gap ids, skills_to_edit, new_skill_ids, testing timeframe, and verification script locator
  — Q02/Q03 recovered full SKILL.md content before Type 20 pair use
  — Q05 records executable query body, target index/source, and expected result before Type 28 pair use

□ For R01-R13 docs.zip session process families:
  — ZIP member path, flow id, round/session id, and template-vs-instance role recorded
  — R09 includes no-code teaching boundary, task_type_ids, DPO triple count, and RAG seed ids
  — R10 includes both flow ids plus emitted/consumed event contract

□ For S01-S14 llm_mvp_core families:
  — project_id=llm_mvp_core recorded
  — skills are not used as XIIGen authority unless adoption_status permits it
  — experiment/subproject/capability ids recorded where applicable

□ For T01-T10 live governance/evidence families:
  — latest version wins for versioned governance/report docs
  — machine-readable evidence fields are parsed directly
  — T07 includes phaseCloseScan and exit codes for portability verdicts

□ For U01-U20 detailed llm_mvp_core skills:
  — exact skill id and skill-specific DTO fields recorded
  — broad "skill file" classification is not enough
  — U15 records the three required pre-code documents and approval status

□ For V01-V08 xiigen-COMPLETE canonical families:
  — archive_id=xiigen-COMPLETE.zip and canonical_nestjs=true recorded
  — older Python/FLOW33 snapshots are stale when they conflict with V families
  — V07 records all fabric ids and cross-tenant assertions

□ For W01-W16 llm_mvp_core experiment/data refinements:
  — broad "experiment plan" label is not enough
  — W01/W02 include hypothesis, baseline, enhanced, metric, tier, and scale-up/failure rules
  — W07-W09 distinguish session state, result schema, and runtime result/progress
  — W16 includes Gate D mandatory sections and Gate B checks

□ For X01-X16 Genie skill library families:
  — project_id=genie_skill_library and archive/member path recorded
  — X01/X03/X12/X13 record stack_id, recipe combo or template kind, and placeholders/runtime
  — X04 records tool_name; Cursor/Cline/Copilot/Claude config formats are not swapped
  — X10/X14 index/entrypoint summaries point to detailed target sources instead of replacing them

□ For Y01-Y10 module-architecture and phases_done families:
  — archive_id=module-architecture.zip or phases_done.zip recorded
  — Y01 records site_type/module_id/config knobs from the 20x15 matrix
  — Y02/Y03 record module_id, skill ids, config_index, and deployment_manifest_id
  — Y06/Y07 record stack_target, source line range, output part, and verification command
  — Y08-Y10 distinguish sub-phase state, master state, and final declaration

□ For AA01-AA17 xiigen_v18_complete families:
  — archive_id=xiigen_v18_complete.zip recorded
  — AA01-AA05 record law/module/matrix/composition/manifest ids
  — AA07/AA10 parse populated config JSON and flow definition JSON
  — AA08/AA09/AA16 record skill_id plus stack/provider and implementation/test fields
  — AA17 records alias target instead of creating duplicate family

□ For AB01-AB07 canonical V18 migration and architect families:
  — canonical anchor recorded: v4 FINAL, xiigen-architect-skill, or xiigen_v18_complete
  — AB01 records correction ids; v1-v3 migration plans are stale when v4 FINAL conflicts
  — AB02/AB03 distinguish SKILL.md package from named reference protocol
  — AB05-AB07 distinguish schema, config instance, and deployment manifest roles

□ For AC01-AC20 compliance/flow-extension/plugin-factory families:
  — requirement_id, flow_id, renumbering offsets, append_to target, validation row, example polarity, or source-code mapping variant recorded as applicable
  — AC13/AC14 state and renumber map fields are used for artifact ID pairs; prose ranges alone are insufficient
  — AC19 positive/negative examples keep polarity; rejected examples are not treated as chosen

□ For AD01-AD08 web-session/flow-state/consolidation families:
  — AD01 records consumer type and executable-vs-reference split
  — AD02/AD05 distinguish per-flow final state from per-phase state
  — AD06/AD07 require renumbered ranges and validation matrix evidence before canonical completion claims
  — AD08 keeps execution plan and consolidation report paired

□ For AE01-AE10 canonical extension/V37 planning families:
  — AE01 archive bundles are provenance only; extracted members supply authority
  — AE02 records append_after_anchor, target_canonical_file, save_point_checksum, and content block
  — AE04-AE06 distinguish cited deep research, engine-specific research, and Q&A chat logs
  — AE09/AE10 RAG indexes point to source libraries/decisions and do not replace implementation sources

□ For AF01-AF13 flow planning/examination/artifact verdict families:
  — AF01 records SUPREME priority and load-before constraint before skill pairs
  — AF04 and AF12 are zone-extracted before any pair text is written
  — AF06-AF09 distinguish planning simulation, deep design simulation, TEACH-QA-R0, and TEACH-QA-R1
  — AF13 records the existing family being confirmed and does not create duplicate training rows

□ For AG01-AG06 flow input/renumbering/merge-completion families:
  — AG01/AG02 use explicit maps and applied offsets, never draft ranges
  — AG03 preserves both raw user intent and bounded AI design response locators
  — AG04 stays research support unless corroborated by implemented flow docs
  — AG06 is early engine-extension authority, not a later merge-plan substitute

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

2026-05-08. Version 7.0.
