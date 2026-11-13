# GUIDE-B21 — How to Produce `FLOW-XX-step 1-INVARIANTS.md`
## Library guidance file — FLOW-PREP-LIBRARY-MASTER-PLAN v6.1-FINAL
## Produced: Round 31 | Phase 4 (Guidance File Authoring)
## Date: 2026-04-20

---

## FINAL GOAL (re-read before authoring any FLOW-XX-step 1-INVARIANTS.md):
> "Taking the produced library as a result of this plan run — same size as list B —
> and applying on new flow specs, we will get proper list B for this flow."

This file is the step 1-INVARIANTS guidance: one of the 50 guidance files that
together constitute the library. When Claude Code applies this guidance to a new
flow's spec, it will produce a correct step 1-INVARIANTS.md that anchors the
entire simulation pipeline with non-negotiable constraints.

---

## WHAT THIS FILE IS

`FLOW-XX-step 1-INVARIANTS.md` is **Step 1 of the 10-step simulation pipeline** —
the document that extracts every constraint that must hold for the flow regardless
of what any AI model decides. These constraints anchor all nine subsequent steps.
Cycles 1, 2, and 3 cannot operate correctly unless Step 1 has declared what is
MACHINE (cannot change) versus FREEDOM (tenant-configurable).

**Position in the 10-step sequence:**

```
Step 1:  FLOW-XX-step 1-INVARIANTS.md    ← this file (anchors everything)
Step 2:  FLOW-XX-step 2-CYCLE1-CONTEXT.md
...
Step 10: FLOW-XX-step 10-CHAIN-REVIEW.md
```

**Why it comes first:** The design simulation generates code. Generated code can
pass all tests and still violate a non-negotiable architectural constraint — this
is the SILENT_FAILURE class (a locked architecture decision). Step 1 forces every constraint to be
stated as a verifiable condition before any generation begins, so that Cycle 1-3
context packages and arbiter panels can encode them correctly.

---

## LIST A SOURCES FOR THIS GUIDANCE FILE

| ZIP | Role | Specific files |
|-----|------|----------------|
| archive batch 11 | PRIMARY | `the booking flow-step 1-INVARIANTS.md` — compressed mature format: flow identity, user intent, 9 DNA rules table, flow-specific constraints, Freedom/Machine classification, success criteria, state write |
| archive batch 11 | PRIMARY | `the user registration flow-step 1-INVARIANTS.md` — richest version: full 4-phase execution (scope gate, DNA extraction, the business flow arbiter rules, iron rules from prior runs, FREEDOM items separate), expected results, bad results, issue inventory |
| archive batch 11 | COMPARISON | `the event creation flow-step 1-INVARIANTS.md`, `the friend request flow-step 1-INVARIANTS.md` — show how invariants compress for v2 guide format |
| archive batch 15 | ROLE REGISTRY | `XIIGEN-ROLE-SCREEN-ARCHITECTURE-GUIDE.md` §1 — 9 role families, 143 entries across 5 layers: Layer 0 (SYSTEM), Layer 1 (Platform), Layer 2 (Context/ownership), Layer 3 (Tenant-configured), Layer 4 (Non-human). Step 1 must declare which roles from this registry are used by the flow |
| archive batch 11 | D-HIST LOCKED | `DECISIONS-LOCKED.md` §D-HIST Group — 8 locked architectural decisions (a locked architecture decision..a locked architecture decision): Fabric-First, the atomic first-writer-wins lock at ORDER 1, V39 (4 artifacts per external system), broken object-level authorization/the request-scoped context storage tenant scoping, REGISTRATION archetype atomic, best-effort observer, SILENT_FAILURE score-0, read-path extension. These constraints apply to EVERY flow and must appear in every Step 1 alongside the 9 DNA rules. |

**The D-HIST difference from the user registration flow era:** the user registration flow through the marketplace flow were written
before D-HIST decisions were locked. Their Step 1 files contain only the 9 DNA rules
as non-negotiables. For new flows (the booking flow+) using guide v2, D-HIST constraints are
an additional source of MACHINE constraints that must be represented.

---

## OUTPUT FILE SPECIFICATION

**Target path:** `docs/sessions/FLOW-XX/FLOW-XX-step 1-INVARIANTS.md`

**Also initializes:** `docs/sessions/FLOW-XX/FLOW-XX-PLAN-STATE.json` (if not yet created)

---

## THE TWO FORMATS: v1 (the user registration flow) AND v2 (the booking flow+)

the user registration flow uses a rich 4-phase execution format with separate phases for scope gate,
DNA extraction, the business flow arbiter extraction, and iron rule extraction. the business onboarding flow through the booking flow+
use guide v2 — a compressed format with the same logical content but presented as
a single scan rather than four labeled phases.

**For new flows: use the v2 compressed format** (the booking flow style). The v1 format is
preserved in this guidance for reference. Use v1 only when a new flow has unusual
scope or the business flow arbiter rule complexity that warrants explicit phase documentation.

---

## UNIVERSAL CONSTRAINTS TO ALWAYS INCLUDE

Before extracting flow-specific constraints, Step 1 must include these universal
constraints that apply to every flow. They come from two sources.

### Source 1: The 9 DNA Rules (always MACHINE)

| # | Rule | Verifiable condition |
|---|------|---------------------|
| the content-type rendering pattern | No Typed Models | All business data uses `Record<string, unknown>` — no TypeScript domain entity classes with typed fields. Violation: any `class User { name: string }` pattern in service code. |
| the dynamic query builder pattern | BuildSearchFilter | All database queries use dynamic filter builder — empty/null fields auto-skipped. Violation: any literal `{ must: [{ term: ... }] }` in service code. |
| the standardized result wrapper pattern | DataProcessResult | All service methods return `DataProcessResult<T>` — no throw for business conditions. Violation: any `throw new NotFoundException()` in service code. |
| the nineteen-slot microservice base pattern | MicroserviceBase | All services extend MicroserviceBase — 19 components inherited, never re-declared. Violation: any service that does not extend MicroserviceBase. |
| the tenant scope isolation pattern | Scope Isolation | Tenant context reads from AsyncLocalStorage automatically — tenantId is never passed as a parameter to fabric methods. Violation: any method signature with tenantId as parameter. |
| the dynamic controller pattern | DynamicController | No entity-specific controllers. Violation: any `UsersController`, `RegistrationController`, etc. |
| the seventh architecture DNA pattern | Idempotency | All queue consumers deduplicate via idempotency keys before any processing. Violation: any queue consumer that processes without idempotency check. |
| the eighth architecture DNA pattern | Outbox Pattern | `storeDocument()` completes before `enqueue()` is called. Violation: `enqueue()` called before or without `storeDocument()`. |
| the ninth architecture DNA pattern | CloudEvents | All inter-service events use CloudEvents envelope via `createCloudEvent()`. Violation: any event published without CloudEvents envelope. |

**Classification:** All 9 DNA rules are MACHINE. A tenant changing any of these
breaks system guarantees that apply to all tenants.

### Source 2: D-HIST Locked Decisions (new flows, guide v2+)

These 8 decisions from `DECISIONS-LOCKED.md` §D-HIST are also MACHINE constraints
that must appear in Step 1 for all new flows. They do not replace the DNA rules —
they are additive:

| ID | Decision | Verifiable condition for Step 1 |
|----|----------|--------------------------------|
| a locked architecture decision | Fabric-First | Services import only interface tokens (`DATABASE_SERVICE`, etc.) via `@Inject()`. No SDK imports (`@elastic/elasticsearch`, `@anthropic-ai/sdk`) in service code. |
| a locked architecture decision | the atomic first-writer-wins lock at ORDER 1 | Queue consumers: `setIfAbsent()` idempotency check at ORDER 1 before any business logic. The idempotency key format is fixed. |
| a locked architecture decision | V39 (4 artifacts per external system) | Every external system dependency produces exactly: Interface + Factory + Base Skeleton + Generated Implementation. No shortcuts. |
| a locked architecture decision | broken object-level authorization/the request-scoped context storage tenant scoping | Tenant scoping via AsyncLocalStorage. `tenantId` never passed as parameter to fabric methods. `TenantContextMiddleware` sets context automatically. |
| a locked architecture decision | REGISTRATION archetype atomic | If the flow has REGISTRATION archetypes: `registerAtomically()` — no separate check-then-write sequence. `cycleBudget=3` for REGISTRATION tasks. |
| a locked architecture decision | Best-effort observer | Observer task types (analytics, notifications, audit logging) wrap entire handler in try/catch and return `DataProcessResult.success()` from catch — never `failure()`. |
| a locked architecture decision | SILENT_FAILURE → score-0 | Named checks that prevent SILENT_FAILURE violations use `score-0` severity, not `BUILD_FAILURE`. `machineConstants[]` values with `neverFromConfig: true` may never use `config.get()`. |
| a locked architecture decision | Read-path extension | For guarded services: read-path extension (not dual-write) when extending. Writer guard invariants stay single-sourced. |

Include all D-HIST entries in Step 1 even if the flow doesn't yet have task types
that exercise them — the goal is to make every subsequent cycle aware of them so
the context packages encode them into genesis prompts.

---

## HOW TO PRODUCE THE FILE (v2 FORMAT)

### Step 1 — Read the flow spec and PLAN-STATE.json

```bash
# If PLAN-STATE.json already exists, read it
cat FLOW-XX-PLAN-STATE.json 2>/dev/null | python3 -m json.tool | head -20

# Extract from flow spec:
# - flow_id, title, task_range (T[NNN]-T[NNN+M])
# - domain
# - focus areas (1-3 phrases)
# - user intent (verbatim — one paragraph)
```

### Step 2 — Write the file header

```markdown
# FLOW-XX — STEP 1: INVARIANTS
## Status: COMPLETE
## Skills loaded: planning--system-intake-SKILL.md (a planning skill), planning--freedom-machine-classification-SKILL.md (a planning skill)
## Guide version: v2
```

### Step 3 — Write the TASK section

```markdown
---

## TASK

Extract the non-negotiable facts about FLOW-XX that every cycle must honour.
These are hard constraints derived from the flow's domain, DNA rules, and the business flow arbiter contracts.
```

### Step 4 — Write FLOW IDENTITY

Derive from the flow's business spec or master plan:

```markdown
---

## FLOW IDENTITY

```
flow_id:    FLOW-XX
title:      [Flow human name]
task_range: T[NNN]-T[NNN+M]
domain:     [Domain family — e.g., XIIGen Community Platform — Social]
focus:      [1-3 keywords from spec — e.g., friend request, social feed, experiment routing]
```
```

### Step 5 — Write USER INTENT (verbatim)

Extract verbatim from the flow spec. This is the user's original goal statement
for this flow — not rephrased. Format: a single "When ... [trigger], [action]."
sentence or short paragraph:

```markdown
---

## USER INTENT (verbatim)

> [Verbatim user intent from flow spec]
```

**For standard XIIGen flows, the pattern is:**
> "When [actor] [trigger event] on the XIIGen [platform area], [what the engine does] [for what purpose]."

### Step 6 — Write NON-NEGOTIABLES (9 DNA Rules table)

Copy the standard 9-rule table (shown in the universal constraints section above).
For v2 format, use the condensed form (name + short constraint, not full verifiable
condition). For v1 format, use full verifiable conditions.

```markdown
---

## NON-NEGOTIABLES (9 DNA Rules — apply to every executor)

| # | Rule | Constraint |
|---|------|-----------|
| the content-type rendering pattern | No Typed Models | All business data uses `Record<string, unknown>` — no entity classes |
| the dynamic query builder pattern | BuildSearchFilter | All queries use dynamic filter builder — no hand-built query DSL |
| the standardized result wrapper pattern | DataProcessResult | All service methods return `DataProcessResult<T>` — no throw |
| the nineteen-slot microservice base pattern | MicroserviceBase | All services extend MicroserviceBase — 19 components inherited |
| the tenant scope isolation pattern | Scope Isolation | Tenant context via AsyncLocalStorage — no tenantId parameter |
| the dynamic controller pattern | DynamicController | No entity-specific controllers — one DynamicController for all CRUD |
| the seventh architecture DNA pattern | Idempotency | All queue consumers deduplicate via idempotency keys |
| the eighth architecture DNA pattern | Outbox Pattern | storeDocument() BEFORE enqueue() — always |
| the ninth architecture DNA pattern | CloudEvents | All inter-service events use CloudEvents envelope |
```

### Step 7 — Write FLOW-SPECIFIC CONSTRAINTS

Add constraints from the D-HIST locked decisions that are relevant to this flow,
plus any domain-specific the business flow arbiter rules:

```markdown
---

## FLOW-SPECIFIC CONSTRAINTS

[From D-HIST locked decisions:]
- a locked architecture decision: Fabric-first — all service code imports only interface tokens, never SDK
- a locked architecture decision: Queue consumers use the atomic first-writer-wins lock idempotency at ORDER 1 before any processing
- a locked architecture decision: Tenant context via AsyncLocalStorage — tenantId never a method parameter
- a locked architecture decision: Named checks for SILENT_FAILURE violations use score-0, not BUILD_FAILURE
[Add a locked architecture decision if flow has REGISTRATION archetypes]
[Add a locked architecture decision if flow has observer/analytics/notification task types]
[Add a locked architecture decision if flow introduces new external system dependencies]

[From flow's the business flow arbiter rules and domain:]
- the business flow arbiter cross-flow validation required before deployment
- All task types in range T[NNN]-T[NNN+M] must be represented
- [Any domain-specific the business flow arbiter rules — e.g., an iron rule email uniqueness for registration flows]

[Role constraints from archive batch 15 §1 (if applicable):]
[If this flow introduces actors beyond role 0 / role 1 / ROLE-TENANT-ADMIN:]
- Role: [ROLE-ID] ([role string]) — [description from archive batch 15 §1]
[Otherwise:]
- No new roles introduced — role 0 (unauthenticated) and role 1 (authenticated) are the primary actors
```

**How to determine which roles to declare:**
Read the flow spec and identify every human or system actor. Cross-reference against
archive batch 15 §1 to find the canonical Role ID and role string. If the flow introduces a
new actor not in archive batch 15 §1, that role must be proposed before Step 1 completes.

### Step 8 — Write FREEDOM / MACHINE CLASSIFICATION

```markdown
---

## FREEDOM / MACHINE CLASSIFICATION

| Concern | Classification | Reason |
|---------|---------------|--------|
| Business logic thresholds | FREEDOM | Operators may tune confidence levels |
| Infrastructure routing | MACHINE | Fixed fabric interface pattern |
| DNA rule enforcement | MACHINE | Non-negotiable — code constraint |
| Task type archetypes | MACHINE | Fixed per the business flow arbiter contract |
| Tenant scope isolation | MACHINE | AsyncLocalStorage — not configurable |
[Add flow-specific rows:]
| [Flow-specific concern] | [FREEDOM/MACHINE] | [Why] |
```

**Classification test (a planning skill):** Ask "Does a tenant changing this value change what
the system guarantees?" If YES → MACHINE. If NO → FREEDOM (goes to freedom_notes).

### Step 9 — Write SUCCESS CRITERIA

Criteria that a Cycle 1 plan must satisfy to be accepted:

```markdown
---

## SUCCESS CRITERIA

A plan produced by Cycle 1 for FLOW-XX is successful when:

1. Every task type in T[NNN]-T[NNN+M] has at least one plan step
2. Every plan step is scoped to a single responsibility (single task type)
3. No step imports provider SDKs directly (fabric-first)
4. No step creates entity-specific controllers
5. All steps return DataProcessResult<T>
6. Focus areas covered: [list 1-3 focus areas from flow spec]
```

### Step 10 — Write STATE WRITE

```markdown
---

## STATE WRITE

```
cycle1.status          → "INVARIANTS_EXTRACTED"
step_status            → "COMPLETE"
user_intent            → "[verbatim user intent string]"
non_negotiables        → [the content-type rendering pattern through the ninth architecture DNA pattern]
success_criteria_count → 6
```
```

Apply to FLOW-XX-PLAN-STATE.json:

```bash
node -e "
const fs = require('fs');
let s = {};
try { s = JSON.parse(fs.readFileSync('FLOW-XX-PLAN-STATE.json')); } catch(e) { /* new */ }

s.flowId = 'FLOW-XX';
s.flow_title = '[Flow human name]';
s.task_range = 'T[NNN]-T[NNN+M]';
s.domain = '[domain]';
s.guide_version = 'v2';
s.cycle1 = s.cycle1 || {};
s.cycle1.status = 'INVARIANTS_EXTRACTED';
s.step_status = 'COMPLETE';
s.current_step = 1;
s.user_intent = '[verbatim user intent]';
s.non_negotiables = ['the content-type rendering pattern','the dynamic query builder pattern','the standardized result wrapper pattern','the nineteen-slot microservice base pattern','the tenant scope isolation pattern','the dynamic controller pattern','the seventh architecture DNA pattern','the eighth architecture DNA pattern','the ninth architecture DNA pattern'];
s.success_criteria_count = 6;

fs.writeFileSync('FLOW-XX-PLAN-STATE.json', JSON.stringify(s, null, 2));
console.log('STATE written. cycle1.status:', s.cycle1.status);
"
```

### Step 11 — Close with STEP COMPLETE

```markdown
---

**STEP 1 COMPLETE**
```

---

## WHEN TO USE v1 FORMAT (the user registration flow STYLE)

The v1 format adds these sections not present in v2:

**Phase A — Verify flow scope (a planning skill):**
Explicitly runs the scope ladder (CONVENTION → ADAPTATION → EXTENSION → NEW FLOW →
NEW INFRA) and answers Q1-Q4. Required when there is genuine ambiguity about whether
this flow is correctly classified as NEW FLOW vs EXTENSION.

**Phase B — Full DNA extraction with verifiable conditions:**
Full verifiable condition for each rule with detection criterion and violation example.
Required for flows in new domains where the pattern of DNA violations is not yet
established.

**Phase C — the business flow arbiter conflict rules:**
Explicitly extracts domain-specific the business flow arbiter rules (CF-N format) that apply to this flow.
Required for flows that introduce new conflict patterns not covered by existing CF rules.

**Phase D — Iron rules from prior runs:**
Extracts lessons from architecture decisions (D-XX-N) and prior simulation findings.
Required for flows where prior architectural decisions exist in `FLOW-XX-ARCHITECTURE-DECISIONS.json`.

**Phase E — MACHINE constraints:**
Additional MACHINE items beyond DNA rules (score thresholds, panel sizes, key formats).
Required for flows with unusual panel size requirements or score thresholds.

**Phase F — FREEDOM items (separately recorded):**
All tenant-configurable items: rate limit windows, model keys, content thresholds.
Required when FREEDOM items must be explicitly distinguished from MACHINE constraints
to avoid confusion in subsequent cycle context packages.

Use v1 when: new flow domain, unusual the business flow arbiter rules, prior architectural decisions exist,
explicit scope ambiguity.
Use v2 when: flow follows established pattern, all DNA rules apply normally, no unusual
the business flow arbiter or architectural decision history.

---

## ROLE REGISTRY REFERENCE (archive batch 15 §1)

When a flow introduces human or system actors beyond the default role 0/role 1/
ROLE-TENANT-ADMIN, declare them in FLOW-SPECIFIC CONSTRAINTS. Cross-reference
these canonical role IDs:

**Platform actors (apply to all flows):**
- role 0 — anonymous/unauthenticated (no JWT)
- role 1 — authenticated user (base platform access)
- ROLE-TENANT-ADMIN — `"tenant_admin"` (configures tenant settings)
- ROLE-PLATFORM-ADMIN — `"platform_admin"` (cross-tenant super-admin)

**Context roles (ownership-derived — select as applicable):**
- role 1-ORGANIZER — `"organizer"` (owns event — the event creation flow, 09)
- role 1-ATTENDEE — `"attendee"` (registered for event)
- role 1-AUTHOR — `"author"` (owns content)
- role 1-REQUESTER/RECEIVER/CONNECTED — social connection roles (the friend request flow)
- ROLE-CLIENT/FREELANCER/AGENCY — marketplace roles (the freelancer marketplace flow)
- ROLE-APP-DEVELOPER — OAuth app developer (the sponsored content flow)
- ROLE-FORM-SUBMITTER — form-filling end user (the dynamic forms flow)
- ROLE-HUMAN-DECISION-MAKER — human gate role (the cross-flow governance engine, 26, 27)

**Non-human actors (select as applicable):**
- ROLE-the business flow arbiter-REVIEWER — Business Flow Arbiter (AI)
- ROLE-BUILD-AGENT — CI/CD pipeline runner
- ROLE-PROMPT-REVIEWER — automated prompt judge (AI)

If none of these apply (engine-internal flow with no UI actors): declare
"No human actors — engine-internal flow. role 0 and role 1 are not applicable."

---

## ACCEPTANCE CRITERIA FOR step 1-INVARIANTS

Before Step 1 is considered complete:

- [ ] File header declares Status: COMPLETE and skills loaded (a planning skill + a planning skill)
- [ ] FLOW IDENTITY block is populated from flow spec (not placeholders)
- [ ] USER INTENT is verbatim from flow spec (not paraphrased)
- [ ] All 9 DNA rules are present in NON-NEGOTIABLES table
- [ ] D-HIST constraints included in FLOW-SPECIFIC CONSTRAINTS (guide v2+)
- [ ] Role actors from archive batch 15 §1 declared if flow has non-default actors
- [ ] FREEDOM / MACHINE classification table has at least 5 rows
- [ ] Success criteria list has exactly 6 items, item 6 references flow focus areas
- [ ] STATE WRITE updates PLAN-STATE.json with `cycle1.status = "INVARIANTS_EXTRACTED"`
- [ ] No technology provider names in MACHINE constraints (Redis, Elasticsearch, NestJS,
      Anthropic must not appear — use interface names: `IScopedMemoryService`, `IDatabase`, etc.)
- [ ] FREEDOM items are NOT in the constraints table — they are in freedom_notes in state

---

## KEY RULES

**1. User intent is verbatim, never paraphrased.**
The user_intent in state is the verbatim string from the flow spec. Cycle 1 context
packages reference this string directly. Paraphrasing changes the intent that Cycle 1
is asked to plan for — causing specification drift.

**2. No technology names in MACHINE constraints.**
MACHINE constraints state verifiable conditions using interface names, not provider
names. "Redis" does not appear — `IScopedMemoryService` does. "Elasticsearch" does
not appear — `IDatabase` does. "NestJS" and "Anthropic" do not appear anywhere.
Technology names in MACHINE constraints break provider migration (a locked architecture decision).

**3. D-HIST constraints are additive to DNA rules, not replacements.**
Both must appear. a locked architecture decision through a locked architecture decision apply to every new flow alongside
the 9 DNA rules. They represent lessons learned across the user registration flow..the booking flow+ that were
elevated to permanent lock status.

**4. FREEDOM items are recorded in state, not in the Step 1 document.**
FREEDOM items (rate limit windows, model keys, threshold values) go into
`freedom_notes` in PLAN-STATE.json. They are NOT included in the Step 1 invariants
document — mixing them with MACHINE constraints confuses the Cycle 1 context package.

**5. Success criteria item 6 must reference the specific focus areas.**
Success criteria items 1-5 are standard (one step per task type, single responsibility,
no SDK imports, no entity controllers, DataProcessResult). Item 6 is flow-specific:
"Focus areas covered: [flow's 2-3 focus keywords from FLOW IDENTITY block]."
Generic item 6 text ("business logic implemented") is a red flag — it means the
Step 1 author didn't read the flow spec carefully enough.

---

## RELATIONSHIP TO SUBSEQUENT STEPS

Step 1's output anchors the entire simulation pipeline:

- **Step 2 (CYCLE1-CONTEXT):** Reads `user_intent` from state and builds the Cycle 1
  context package around it. The 9 DNA rules and D-HIST constraints feed into the
  context package's "non-negotiables" section.

- **Step 3 (CYCLE1-TEST):** The success criteria from Step 1 become the pass/fail
  conditions for Cycle 1 verification.

- **Step 4-8:** The MACHINE/FREEDOM classification informs which items appear in
  genesis prompts as iron rules (MACHINE) versus configurable options (FREEDOM).

- **Step 10 (CHAIN-REVIEW):** Reads `cycle1.status = "INVARIANTS_EXTRACTED"` from
  state to verify Step 1 was completed. Missing state write blocks the chain review.

---

*End of GUIDE-B21 — FLOW-XX-step 1-INVARIANTS.md*
*List A sources: archive batch 11 (the user registration flow/03/07/09 step 1-INVARIANTS examples),*
*archive batch 15 §1 (role registry — 9 families, 143 entries),*
*archive batch 11 DECISIONS-LOCKED.md §D-HIST (8 locked architectural decisions)*
*Target B-type: B-21 — FLOW-XX-step 1-INVARIANTS.md*
*Round: 31 of 72*
