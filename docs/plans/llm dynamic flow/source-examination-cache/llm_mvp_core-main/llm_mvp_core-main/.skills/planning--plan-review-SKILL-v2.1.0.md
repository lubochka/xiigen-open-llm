# Plan Review Skill v2.1 — 17 Failure Classes + 3-Gate Approval

A plan with count drift costs more to fix than to prevent. A plan that misses the
user's goals costs even more. A plan that ships unguarded controllers or uncertified
flows costs more still. This skill prevents all three.

## Origin

v1.0 extracted from the XIIGen skill migration planning session (March 2026).
v2.0 extended after the governance authoring session round 1 (2026-04-16): the goal delivery check/15 added
after User Journey Reconnection v27 shipped with 3 of 4 goals unmapped.
v2.1 adds the authentication requirements check/20/21 after AUTH-ARBITER analysis (2026-04-24) confirmed that
no FC gate checked whether a plan declared auth requirements, declared NDJSON
type coverage, or referenced the portability/auth certification protocol.

## When to Invoke

- BEFORE handing any plan to Claude Code
- BEFORE declaring a plan "ready for approval"
- AFTER any plan edit that touches a number, a phase, or a skill list
- AFTER any plan edit that touches a goal, a turn, or a referenced artifact
- **(NEW v2.1)** AFTER any plan edit that touches a controller, a distribution
 tier claim, or an arbiter fixture

---

## PRECEDENCE RULE (v2.0, extended v2.1)

the goal delivery check, the design artifact content check, the authentication requirements check, the rule file type coverage check, and the definition-of-done check run FIRST. If any returns BLOCK,
the count accuracy check..the principles compliance check are not evaluated.

```
Order of evaluation:
 the goal delivery check (Goal Delivery Completeness — the goal delivery check skill)
 the design artifact content check (Design Artifact Populated — the design artifact completeness check)
 the authentication requirements check (Auth Requirement Declaration — NEW v2.1)
 the rule file type coverage check (Arbiter NDJSON Type Coverage — the arbiter type requirements skill — NEW v2.1)
 the definition-of-done check (Definition of Done Protocol Reference — NEW v2.1)
 ↓ (only if all PASS or CHALLENGE)
 the count accuracy check through the principles compliance check (internal consistency)
 the architect behavior check (Architect Habits Discipline — the architect habits guide v1.2.0)
 the response construction check (Response Construction Protocol Compliance)
 the interface quality check (UI/UX Compliance — the interface quality check gate — if React pages)
```

Rationale: a plan with perfect internal consistency that ships unguarded controllers
or omits the portability protocol is still the wrong plan. the authentication requirements check/20/21 audit whether
the plan's security and certification posture is sound before correctness is checked.

---

## The 17 Failure Classes

### the goal delivery check: Goal Delivery Completeness (v2.0 — RUNS FIRST)

**Governance:** the goal delivery check skill Goal Delivery Completeness Arbiter

Decompose goal statement into discrete elements. Each element mapped to ≥1 plan turn
with a verification step. Verdict per element: APPROVED | rejected — goal element not covered |
rejected — no way to verify | CHALLENGE. Any BLOCK → plan rejected before the count accuracy check..the principles compliance check execute.

---

### the design artifact content check: Design Artifact Populated (v2.0 — RUNS FIRST)

**Governance:** the design artifact completeness check Design Artifact Completeness

For every artifact referenced: Checks 1-2 (exist + fields populated) must pass.
Any artifact failing Checks 1-2 → plan rejected or must add enrichment task.

---

### the authentication requirements check: Auth Requirement Declaration — NEW v2.1

**Runs FIRST alongside the goal delivery check and the design artifact content check.**

A plan that generates HTTP controllers must declare what authentication and
authorization those controllers require. Without this declaration, every generated
controller ships unguarded and every generated route is anonymous.

**When the authentication requirements check applies:**
- Plan produces any file matching `*.controller.ts`
- Plan produces any HTTP endpoint (`@Get`, `@Post`, `@Put`, `@Delete`, `@Patch`)
- Plan describes "REST API", "endpoints", "routes", or "HTTP interface"

**Detection protocol:**

```bash
# Step 1: Identify if the plan produces controllers
CTRL_COUNT=$(grep -cE "controller\.ts|@Controller|REST|endpoint|HTTP route" PLAN.md \
 2>/dev/null || echo 0)
echo "Controller-producing plan: $CTRL_COUNT references"

# Step 2: Check for auth declaration section
AUTH_SECTION=$(grep -cE "auth.*require|@UseGuards|@Roles|@Public|bypass.*path|BYPASS_PATH|\
 authentication deferred (not yet deployed)|auth.*declaration|route.*protected|protected.*route" PLAN.md \
 2>/dev/null || echo 0)
echo "Auth declaration present: $AUTH_SECTION references"
```

**Required auth declaration content (at least one of):**
```
Option A — Full auth plan:
 Routes protected: [list route paths]
 Guards: @UseGuards(JwtAuthGuard, RolesGuard)
 Roles per route: [table of route → @Roles values]
 Public routes (if any): [list + BYPASS_PATHS update step]

Option B — authentication deferred (not yet deployed) declaration:
 authentication status: authentication deferred (not yet deployed)
 Reason: AUTH-ROLES-GROUPS-PLAN-v3.0 Phases 1-4 not yet deployed
 Phase H: [named follow-up session or plan phase for decoration]

Option C — N/A declaration:
 No HTTP controllers in this plan (service-only flow)
```

**the authentication requirements check verdict rules:**
- `CTRL_COUNT === 0` → **PASS (N/A)** — no controllers, no auth requirement needed
- Auth declaration present (Option A, B, or C) → **PASS**
- `CTRL_COUNT > 0 AND AUTH_SECTION === 0` → **BLOCK** — add auth declaration before the count accuracy check..the principles compliance check

**Detection command (for plan review session):**
```bash
# Check if plan mentions controllers AND has auth section
HAS_CTRL=$(grep -cE "\.controller\.ts|@Controller\(" PLAN.md || echo 0)
HAS_AUTH=$(grep -cE "@UseGuards|authentication deferred (not yet deployed)|auth.*declaration|bypass-paths" PLAN.md || echo 0)
echo "the authentication requirements check: controllers=$HAS_CTRL auth_declared=$HAS_AUTH"
# BLOCK if HAS_CTRL > 0 AND HAS_AUTH == 0
```

---

### the rule file type coverage check: Arbiter NDJSON Type Coverage — NEW v2.1

**Runs FIRST alongside the goal delivery check and the design artifact content check.**

**Governance:** the arbiter type requirements skill Arbiter NDJSON Requirements

A plan that creates or modifies `fixtures/arbiters/{slug}-arbiters.bulk.ndjson`
must declare that arbiter type coverage meets the the arbiter type requirements skill minimum type matrix.
Without this check, NDJSON files can ship with zero typed records and no
`scope_isolation` coverage.

**When the rule file type coverage check applies:**
- Plan produces or modifies `fixtures/arbiters/` files
- Plan describes "arbiter fixture", "NDJSON", or "bulk.ndjson"
- Plan produces a new flow (all new flows require NDJSON)

**Detection protocol:**

```bash
# Step 1: Does the plan touch arbiter fixtures?
NDJSON_REF=$(grep -cE "arbiters/|bulk\.ndjson|arbiter.*fixture|fixture.*arbiter" PLAN.md \
 2>/dev/null || echo 0)
echo "Arbiter NDJSON references: $NDJSON_REF"

# Step 2: Does the plan declare type coverage?
TYPE_COVERAGE=$(grep -cE "scope_isolation|arbiterType|the arbiter type requirements skill|type.*coverage|NDJSON.*type" \
 PLAN.md 2>/dev/null || echo 0)
echo "Arbiter type coverage declared: $TYPE_COVERAGE references"
```

**Required NDJSON coverage declaration (at least one of):**
```
Option A — Coverage assertion:
 NDJSON type coverage: scope_isolation ≥1, domain ≥3, security ≥1 (PII flow)
 Verified by: jq '[.[] | select(.arbiterType=="scope_isolation")] | length' ≥ 1

Option B — New flow declaration:
 New NDJSON to be created following the arbiter type requirements skill minimum type matrix
 scope_isolation: ≥1 per tenant-data service (mandatory)
 [other types per the arbiter type requirements skill matrix for this flow's characteristics]

Option C — N/A declaration:
 No arbiter fixtures produced or modified in this plan
```

**the rule file type coverage check verdict rules:**
- `NDJSON_REF === 0` → **PASS (N/A)** — no arbiter fixtures in scope
- Coverage declaration present (Option A, B, or C) → **PASS**
- `NDJSON_REF > 0 AND TYPE_COVERAGE === 0` → **BLOCK** — add the arbiter type requirements skill type coverage declaration

---

### the definition-of-done check: Definition of Done Protocol Reference — NEW v2.1

**Runs FIRST alongside the goal delivery check and the design artifact content check.**

A plan that claims to deliver a flow to ACTIVE, portable, or TIER-X status must
reference the portability protocol (the flow portability test protocol skill) and auth protocol (authentication check) as delivery
gates, or explicitly defer them with a named follow-up session. Without this
check, plans can claim completion without any protocol having been run.

This gap was identified across all 49 flows: zero plans referenced the flow portability test protocol skill unit test layer
or the Tier C certification checklist in their delivery criteria.

**When the definition-of-done check applies:**
- Plan claims to deliver a flow to ACTIVE, portable, Tier A, Tier B, Tier C, or Tier D
- Plan uses language like "flow complete", "ready for distribution", "done"
- Plan produces a PHASE-COMPLETE document

**Detection protocol:**

```bash
# Step 1: Does the plan claim flow completion?
COMPLETION_CLAIM=$(grep -cE "\bACTIVE\b|\bMOBILE\b|\bTIER[_-][ABCD]\b|\bPHASE-COMPLETE\b|\
 flow.*complete|complete.*flow|done.*flow|distribution.*ready" PLAN.md \
 2>/dev/null || echo 0)
echo "Flow completion claims: $COMPLETION_CLAIM"

# Step 2: Does the plan reference the protocol?
PROTOCOL_REF=$(grep -cE "the flow portability test protocol skill|portability.*test|portability check.*gate|authentication check.*gate|unit test layer|authentication deferred (not yet deployed)|\
 authentication status|portability status|tenant certification tier|the authentication deployment prerequisite check" PLAN.md \
 2>/dev/null || echo 0)
echo "Protocol references: $PROTOCOL_REF"

# Step 3: For Tier C claims specifically, check the authentication deployment prerequisite check
Tier C_CLAIM=$(grep -cE "TIER[_-]C|Tier C" PLAN.md 2>/dev/null || echo 0)
GUARD_14_REF=$(grep -cE "the authentication deployment prerequisite check|AUTH-PLAN.*Phase.*[1-4]|auth.*infrastructure.*deploy" \
 PLAN.md 2>/dev/null || echo 0)
echo "Tier C claims: $Tier C_CLAIM | the authentication deployment prerequisite check references: $GUARD_14_REF"
```

**Required protocol reference (at least one of):**
```
Option A — Full protocol references:
 portability status target: MOBILE (portability check gate)
 authentication status target: authentication in place or authentication deferred (not yet deployed) (authentication check gate)
 the flow portability test protocol skill unit test layer: included as delivery gate OR explicitly deferred
 tenant certification tier: Tier A minimum declared
 [For Tier C]: the authentication deployment prerequisite check explicitly checked

Option B — Explicit deferral:
 the flow portability test protocol skill unit test layer: NOT_RUN — deferred to [named session or plan phase]
 authentication status: authentication deferred (not yet deployed) — deferred to Phase H
 tenant certification tier: NONE — to be upgraded in [named session]

Option C — N/A:
 This plan does not complete a flow (infrastructure only, or partial phase)
```

**the definition-of-done check verdict rules:**
- `COMPLETION_CLAIM === 0` → **PASS (N/A)** — no completion claim, no protocol required
- Protocol reference present (Option A, B, or C) → **PASS**
- `COMPLETION_CLAIM > 0 AND PROTOCOL_REF === 0` → **BLOCK** — add protocol reference
- `Tier C_CLAIM > 0 AND GUARD_14_REF === 0` → **BLOCK** — the authentication deployment prerequisite check must be explicitly addressed for Tier C

---

### the count accuracy check through the principles compliance check (v1.0 — unchanged)

| FC | Name | What it catches |
|----|------|----------------|
| the count accuracy check | Count Drift | Number updated in one place, not all places |
| the path verification check | Path Errors | File paths don't match codebase convention |
| the skill order check | Phantom Skills | Skill in load order but no phase creates it |
| the pipeline contract check | Duplicate Numbers | Same position number appears twice |
| the naming convention check | Missing Items in Lists | Skill absent from one required list |
| the locked rules check | Stale Numbers | Number references older version of live data |
| the code quality check | Phase Placement Errors | Skill in different phase across plan/deliverable/session |
| the test gate integrity check | Format Violations | Plan formatted for human reading, not Claude Code execution |
| the issue inventory check | Requirement Ambiguity | Delivery requirement has no project-specific "done" definition |
| the cross-document propagation check | Cross-Document Propagation | Fix applied to one doc, stale in others |
| the overview-detail consistency check | Overview-Detail Phase Mismatch | Phase header lists different skills than deliverable block |
| the principles compliance check | Foundational Principles Compliance | Plan doesn't address P1-P8 gate questions |

Full detection protocols for the count accuracy check..the principles compliance check: see plan-review-skill v1.0.0.

---

### the architect behavior check: Architect Habits Discipline (v2.0 — the architect habits guide v1.2.0)

Verifies the plan author ran the three-step doc-first loop at authoring time.
PASS if authoring-time evidence present; CONCERN if Tier 1 Gate 0i passed but
authoring evidence missing; BLOCK if both absent.

---

### the response construction check: Response Construction Protocol Compliance (v2.0)

For every substantive response in the plan-authoring session: Step 1 decomposition,
Step 2 absorption (full mode), Step 3 prior-correction thread, Step 5 source-layer
tags, Step 6 feedback recheck. All must be present for the response to be compliant.

---

### the interface quality check: UI/UX Compliance (v2.0 — if React pages)

Fires only if the plan produces `*.tsx` files in `client/src/pages/`. Verifies
Phase 7 is declared, the interface quality check Audit Trail path is specified, grammar type declared
for tenant-facing pages. Does not fire for server-only plans.

---

## Updated Review Protocol (12 steps)

**Steps 1-3 (new in v2.1 — run FIRST):**

**Step 0a** — the authentication requirements check Auth Requirement Declaration: does the plan produce controllers?
If yes: is auth declared (Option A/B/C)?
```bash
HAS_CTRL=$(grep -cE "\.controller\.ts|@Controller\(" PLAN.md || echo 0)
HAS_AUTH=$(grep -cE "@UseGuards|authentication deferred (not yet deployed)|auth.*declaration|bypass-paths" PLAN.md || echo 0)
echo "the authentication requirements check: $([[ $HAS_CTRL -eq 0 || $HAS_AUTH -gt 0 ]] && echo 'PASS' || echo 'BLOCK')"
```

**Step 0b** — the rule file type coverage check NDJSON Type Coverage: does the plan touch arbiters?
If yes: is the arbiter type requirements skill type coverage declared?
```bash
HAS_NDJSON=$(grep -cE "arbiters/|bulk\.ndjson" PLAN.md || echo 0)
HAS_TYPES=$(grep -cE "scope_isolation|arbiterType|the arbiter type requirements skill" PLAN.md || echo 0)
echo "the rule file type coverage check: $([[ $HAS_NDJSON -eq 0 || $HAS_TYPES -gt 0 ]] && echo 'PASS' || echo 'BLOCK')"
```

**Step 0c** — the definition-of-done check Definition of Done Protocol Reference: does the plan claim completion?
If yes: is protocol referenced? For Tier C: is the authentication deployment prerequisite check addressed?
```bash
HAS_DONE=$(grep -cE "ACTIVE|MOBILE|TIER[_-][ABCD]|PHASE-COMPLETE" PLAN.md || echo 0)
HAS_PROTO=$(grep -cE "the flow portability test protocol skill|portability check|authentication check|authentication deferred (not yet deployed)|portability status|tenant certification tier" PLAN.md || echo 0)
HAS_G14=$(grep -cE "the authentication deployment prerequisite check" PLAN.md || echo 0)
Tier C=$(grep -cE "TIER[_-]C" PLAN.md || echo 0)
echo "the definition-of-done check done: $([[ $HAS_DONE -eq 0 || $HAS_PROTO -gt 0 ]] && echo 'PASS' || echo 'BLOCK')"
echo "the definition-of-done check Guard14: $([[ $Tier C -eq 0 || $HAS_G14 -gt 0 ]] && echo 'PASS' || echo 'BLOCK')"
```

**Steps 1-10 (original — unchanged):**
Step 1: Build Skill Presence Matrix (the naming convention check). Step 2: Verify Phase Assignments (the code quality check).
Step 3: Count All Numbers (the count accuracy check). Step 4: Verify Paths (the path verification check). Step 5: Check Load
Order (the skill order check/the pipeline contract check). Step 6: Verify Source of Numbers (the locked rules check). Step 7: Format Check
(the test gate integrity check). Step 8: Requirement Definitions (the issue inventory check). Step 9: Cross-Document Sweep (the cross-document propagation check).
Step 10: Overview-Detail Match (the overview-detail consistency check) + Principles Compliance (the principles compliance check).

**Steps 11-12 (v2.0 — unchanged):**
Step 11: the goal delivery check Goal Delivery + the design artifact content check Design Artifact Completeness.
Step 12: the architect behavior check Architect Habits + the response construction check Response Construction + the interface quality check UI/UX.

---

## Passing Criteria — Three Gates Required (updated v2.1)

**Gate A — FC Checks (Claude Code runs all steps)**

```
✅ the authentication requirements check: Auth declaration present (or N/A for no-controller plans)
✅ the rule file type coverage check: NDJSON type coverage declared per the arbiter type requirements skill (or N/A)
✅ the definition-of-done check: Protocol reference present for completion claims (or N/A)
✅ the definition-of-done check: the authentication deployment prerequisite check addressed for Tier C claims (or N/A)
✅ the goal delivery check: All goal elements mapped + verification specified
✅ the design artifact content check: All referenced artifacts pass Checks 1-2
✅ the count accuracy check..the principles compliance check: All internal consistency checks pass
✅ the architect behavior check: Architect habits doc-first loop evidence present
✅ the response construction check: Response construction protocol compliant
✅ the interface quality check: UI/UX compliance (if React pages produced)
```

**Gate B — AI Cross-Review** (2 independent models, different from plan author)

**Gate C — Luba Written Approval** — explicit written approval before execution.

---

## Anti-Patterns Table (updated v2.1)

| Anti-pattern | FC | Real cost |
|---|---|---|
| "I updated the count" (in one place) | the count accuracy check | 4 re-review rounds |
| Copying a path from memory | the path verification check | 42 files in wrong location |
| Conceptual placeholder in load order | the skill order check | Claude Code loads non-existent skill |
| Duplicate phase numbers | the pipeline contract check | Load order wrong |
| Adding skill without updating all lists | the naming convention check | Count drift |
| Using plan-time estimates for live counts | the locked rules check | Gate passes on wrong baseline |
| Moving skill without updating all locations | the code quality check | SESSION builds in wrong phase |
| Sending merged doc to Claude Code | the test gate integrity check | Claude Code commits as reference |
| Generic requirement ("UI e2e") | the issue inventory check | Wrong definition invented |
| Fix in one doc, stale in others | the cross-document propagation check | Most frequent failure (30% of issues) |
| Phase header vs deliverable mismatch | the overview-detail consistency check | Count drift + phantom |
| P1-P8 not answered | the principles compliance check | Principles violations shipped |
| Plan produces controllers, no auth section | the authentication requirements check | Unguarded controllers shipped |
| Plan touches NDJSON, no type coverage check | the rule file type coverage check | Zero arbiterType records shipped |
| Plan claims ACTIVE/portable, no protocol ref | the definition-of-done check | Flow certified without protocol run |
| Plan claims Tier C, no the authentication deployment prerequisite check mention | the definition-of-done check | Tier C claimed without auth infra |

## Changelog

- **v1.0.0** — initial. the count accuracy check..the principles compliance check, 3-gate approval, 10-step protocol.
- **v2.0.0** — the goal delivery check (Goal Delivery Completeness, the goal delivery check skill) + the design artifact content check (Design Artifact
 Populated, the design artifact completeness check) run FIRST. Precedence rule. the architect behavior check (Architect Habits), the response construction check
 (Response Construction), the interface quality check (UI/UX). 12-step protocol.
- **v2.1.0** — the authentication requirements check (Auth Requirement Declaration), the rule file type coverage check (Arbiter NDJSON Type
 Coverage per the arbiter type requirements skill), the definition-of-done check (Definition of Done Protocol Reference including the authentication deployment prerequisite check
 check for Tier C). All three run FIRST alongside the goal delivery check/the design artifact content check. Steps 0a/0b/0c added
 to review protocol. Anti-patterns table extended. Closes AUTH-ARBITER-SKILLS-
 REMEDIATION-plan version 3.0 Phase 10.

---

## HUMAN LANGUAGE GATE CHECK

This check runs on every plan output before it is considered complete.

**Verification:** Scan the entire plan document for machine shortcodes outside code fences.

Detection pattern: any token matching `[A-Z]+-[0-9]+` or bare letter+number identifiers (H0, M1, V10, Q4, etc.) or ALL_CAPS_UNDERSCORE status strings used as prose (SILENT_FAILURE, BUILD_FAILURE, etc.).

**Failure:** Plan is not complete until all shortcodes are translated to human language using the map in `code-execution--human-language-gate-SKILL.md`.

**This check is absolute** — it applies to every plan regardless of session type, urgency, or whether the audience is considered "technical."

