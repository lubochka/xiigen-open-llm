# R19-SIM-GAP-CATALOG.md
## the business onboarding flow Simulation Session — Round 19: Complete Gap Catalog + a planning skill Convergence
## Date: 2026-03-27
## Skills applied: a planning skill (simulation-protocol), a planning skill (root-cause-ladder)
## Source rounds: R13 (baseline), R14 (Phase A), R15 (a task type), R16 (a task type), R17 (Phase E), R18 (Phase F)

---

## COMPLETE GAP INVENTORY — 16 Gaps

| Gap ID | Class | Round | Description | Blocking? |
|--------|-------|-------|-------------|-----------|
| gap number 1 | H | R13 | xiigen-decision-graph the search index index absent — no fixture definition, no bootstrapper registration | YES — blocks SESSION-TEACH-A0 Step 0; all Phase B/F planning graph writes |
| gap number 2 | H | R13 | xiigen-planning-decisions the search index index absent — same absence pattern | YES — blocks all planning triple writes (ROUTING, CALIBRATION, GENESIS_PATCH, ARCH_SUMMARY) |
| gap number 3 | C | R13 | a task type/a task type/a task type NOT in engine-bootstrapper ENGINE_CONTRACTS seed list — executor falls back to defaultNodes() generic 5-node pipeline | NO — generation proceeds with generic pipeline; FAN_IN/CONVERGENCE topology absent |
| gap number 4 | C | R13 | ContractArchetype enum missing FAN_IN/CONVERGENCE/BROADCAST; contracts use type coercions ('fan_in' as ContractArchetype); session files use uppercase ("FAN_IN") | NO — low-impact case mismatch; DifficultyPredictorService uses free-form string |
| gap number 5 | B | R13 | a task type contract emits "OnboardingBroadcastSent"; SESSION-F Step F2 registers gateEvent as "OnboardingCompleted" | NO (as isolated) — description error only at source |
| gap number 6 | D | R14 | Named check graph edge keys ("allSettled_check", "entry_guard_check", "degraded_path_check") don't match contract QG checkType IDs ("fan_in_pattern", "machine_constant_guard", "degraded_terminal") — B-L2-2 creates duplicate discovered edges instead of updating seeded edges | NO — creates spurious duplicate edges; seeded edges stay at confidence 0.10 |
| gap number 7 | D | R15 | quality gate check 02 part 10 (fan_in_pattern / allSettled) not enforced by quality scorer — RealCodeQualityScorer has no fan_in_pattern checker; allSettled violation passes undetected | NO — SILENT_FAILURE; wrong pattern scores same as correct |
| gap number 8 | D | R16 | RouteHandler (entry_guard) exists and explicitly references the business onboarding flow CONVERGENCE, but a task type has no registered topology; defaultNodes() has no route node at entry point | NO — SILENT_FAILURE; wrong event type reaches a task type without guard |
| gap number 9 | D | R16 | NAMED_CHECK_CONVERGENCE_THRESHOLD exported from contracts file but never imported/registered in validate.handler NAMED_CHECKS — score-0 severity gate unenforceable | NO — SILENT_FAILURE; hardcoded threshold passes undetected |
| gap number 10 | D | R16 | quality gate check 02 part 21 (degraded_terminal) not enforced by quality scorer — DataProcessResult.failure() on low score passes the standardized result wrapper pattern check; wrong iron rule violation undetected | NO — SILENT_FAILURE; Wave 2 break risk undetected |
| gap number 11 | C | R17 | resolveCurriculumTier() tierMap missing 'fan_in' and 'convergence' keys; a task type/a task type get tier=3 (default) instead of tier=4; distillationReadiness mislabeled for ORCHESTRATION-class patterns | NO — SILENT_FAILURE; wrong OSS teaching signal; INVALID_TIER gate not triggered |
| gap number 12 | B | R17 | SESSION-E Step E3 gap item 08 gate queries "chosen.code" and "rejected.code" (nested); DpoTriple schema stores "chosen: string" and "rejected: string" (top-level). the search index exists check always returns 0 | YES — Phase E gate perpetually blocked |
| gap number 13 | C | R17 | Z-1.4 prompt.system fetch queries xiigen-prompts by {taskTypeId, promptType: 'genesis'}; SESSION-A may seed genesis prompts with promptKey field only (no taskTypeId field); search returns empty → systemPrompt=null → Step E2 blocks Phase E | YES — blocks Phase E the preference learning signal capture if seeding format misaligned |
| simulation gap fourteen | B | R18 | gap number 5 is systemic across 3 locations: SESSION-F Step F2 cross-flow doc, SESSION-TEACH-A0 Step 5 DOWNSTREAM_DEPENDENCY, SESSION-TEACH-WHY-BROADCAST D-02-broad 01. All use "OnboardingCompleted"; contract emits "OnboardingBroadcastSent". Wave 2 flows configured against wrong gate event. | YES — wave trigger chain broken; the event creation flow..07 would never fire |
| gap number 15 | D | R18 | F3-L2-7 FALSE POSITIVE: when xiigen-planning-decisions is empty, OUTCOME_PENDING count=0 → step reports "✅ 0 OUTCOME_PENDING remain" — success signal with zero evidence | NO — false green only; session continues |
| gap number 16 | C | R18 | F-WAVE-GATE Check 2 (PLANNING_TOTAL ≥ 12) requires the user registration flow + the business onboarding flow combined planning triples. the user registration flow predates planning layer; has 0 planning triples. the business onboarding flow produces 6–9 max. Check 2 will always soft-fail. | NO — soft check; Wave 2 proceeds regardless |

---

## GAP CLASS DISTRIBUTION

| Class | Count | Gap IDs | Definition |
|-------|-------|---------|------------|
| H — Hard infrastructure | 2 | gap number 1, gap number 2 | the search index index absent — must create before any session proceeds |
| B — Naming/content error | 3 | gap number 5, gap number 12, simulation gap fourteen | Wrong names in session files or planning docs |
| C — Wired but not registered | 5 | gap number 3, gap number 4, gap number 11, gap number 13, gap number 16 | Defined/exists but not connected to runtime path |
| D — Quality gate without enforcement | 6 | gap number 6, gap number 7, gap number 8, gap number 9, gap number 10, gap number 15 | Gate declared; no implementation in pipeline |

**Blocking gaps:** 4 (gap number 1, gap number 2, gap number 12, gap number 13, simulation gap fourteen)
→ These prevent Phase A, Phase E, and Wave 2 from completing correctly.

---

## a planning skill ROOT-CAUSE LADDER CONVERGENCE

Applying a planning skill (root-cause-ladder): trace each gap backward to find minimal set of root causes. 16 gaps → 7 root causes.

### rc 1 — Layer 2 Index Definitions Absent

**Gaps driven:** gap number 1, gap number 2

**Root:** The XIIGen planning layer (Layer 2) was designed in the SESSION-TEACH files
but the search index index infrastructure to support it — `xiigen-decision-graph` and
`xiigen-planning-decisions` — was never created. No fixture JSON files exist for these
indices. The `EngineBootstrapper` has no instruction to create them. The `GraphRAGTwoLayerSeeder`
(the system bootstrap flow scoped) shows the infrastructure pattern exists but has not been generalized.

**Fix path:** Create two fixture files:
- `server/fixtures/indices/xiigen-decision-graph.json` — mappings for ARCH_NODE, REQUIRES_CHECK, DOWNSTREAM_DEPENDENCY, OPTIONAL_ARBITER, ROUTES_TO, SCORE_BRACKET_OVERRIDE, PREDICTS_CYCLE_COUNT entity types
- `server/fixtures/indices/xiigen-planning-decisions.json` — mappings for ROUTING_DECISION, CALIBRATION_COMPARISON, GENESIS_PATCH_EFFECTIVENESS, ARCH_SUMMARY document types

Register both in EngineBootstrapper index creation list.

---

### rc 2 — the business onboarding flow Task Types Not Registered in Engine Runtime

**Gaps driven:** gap number 3, gap number 4, gap number 8

**Root:** a task type/a task type/a task type contracts are fully defined in `profile-enrichment-matching-contracts.ts`
but were never added to `engine-bootstrapper.ts` ENGINE_CONTRACTS seed list. The
`ContractArchetype` enum was not extended with FAN_IN/CONVERGENCE/BROADCAST values.
As a result: (a) GenericNodeExecutor falls back to `defaultNodes()` for a task type/a task type/a task type,
losing archetype-specific execution topology; (b) RouteHandler (entry_guard) cannot be
wired into a task type's execution topology since no topology is registered; (c) any enum-based
archetype routing silently misses a task type/a task type/a task type.

**Fix path:**
1. Add a task type/a task type/a task type to ENGINE_CONTRACTS with topology definitions including RouteHandler for a task type
2. Add `FAN_IN = 'fan_in'`, `CONVERGENCE = 'convergence'`, `BROADCAST = 'broadcast'` to ContractArchetype enum (replace type coercions)
3. Register a task type topology with `route` node as entry point (wire RouteHandler for entry_guard)

---

### rc 3 — a task type CloudEvent Name Inconsistency (Systemic)

**Gaps driven:** gap number 5, simulation gap fourteen

**Root:** The a task type contract specifies `events: ['user.onboarding.broadcast.sent']` and
`purpose: 'Emit OnboardingBroadcastSent'`. However, all three session teaching documents
that reference the Wave 2 gate event use "OnboardingCompleted" — a name that may have
been used during early design and was superseded by the contract. The error was introduced
into SESSION-TEACH-WHY-BROADCAST (D-02-broad 01), SESSION-TEACH-A0 (Step 5 DOWNSTREAM_DEPENDENCY),
and SESSION-F (Step F2 cross-flow registration) independently. At Wave 2 execution time,
the event creation flow..07 planners reading any of these three documents would configure the wrong trigger.

**Fix path:** Three-location correction:
1. `SESSION-TEACH-WHY-BROADCAST.md` — D-02-broad 01: change "OnboardingCompleted" → "OnboardingBroadcastSent" in REQUIRED_EMIT edge description, teaching narrative, and planning triple
2. `SESSION-TEACH-A0.md` — Step 5 DOWNSTREAM_DEPENDENCY: change event name in description field
3. `SESSION-F.md` (the business onboarding flow-SESSION-F.md) — Step F2: change gateEvent for a task type→WAVE2 document from "OnboardingCompleted" → "OnboardingBroadcastSent"

---

### rc 4 — Archetype-Specific Quality Enforcement Not Implemented

**Gaps driven:** gap number 6, gap number 7, gap number 9, gap number 10

**Root:** The quality enforcement pipeline was built for generic DNA patterns (the content-type rendering pattern
through the ninth architecture DNA pattern). When the business onboarding flow contracts defined archetype-specific quality gates
(quality gate check 02 part 10 fan_in_pattern, quality gate check 02 part 21 degraded_terminal, quality gate check 02 part 23 machine_constant_guard)
and a named check (`NAMED_CHECK_CONVERGENCE_THRESHOLD`), none of these were wired into
the enforcement infrastructure:
- `RealCodeQualityScorer` has no archetype-specific gate enforcement (only 5 generic dimensions)
- `validate.handler.ts` NAMED_CHECKS registry has only throttle + idempotency
- `NAMED_CHECK_CONVERGENCE_THRESHOLD` is exported from contracts but never imported or registered
- Named check edge keys in SESSION-TEACH-A0 ("allSettled_check") don't match the contract QG checkType IDs ("fan_in_pattern") that the pipeline would report

This is the most severe cluster — it means the contracts' score-0 severity gates and iron
rule violations can never be detected at runtime.

**Fix path:**
1. Extend `RealCodeQualityScorer.spec_adherence` with an archetype dispatcher that checks `fan_in_pattern`, `degraded_terminal`, `machine_constant_guard` based on `contract.archetype`
2. Register `NAMED_CHECK_CONVERGENCE_THRESHOLD` in `validate.handler.ts` NAMED_CHECKS
3. Align SESSION-TEACH-A0 Step 3 graph edge key names with contract QG checkType IDs:
   - `allSettled_check` → `fan_in_pattern`
   - `entry_guard_check` → keep (not in contract QG; keep as planning-level concept)
   - `degraded_path_check` → `degraded_terminal`

---

### rc 5 — P18 Teaching Fields Not Extended for the business onboarding flow Archetypes

**Gaps driven:** gap number 11, gap number 13

**Root:** Two P18 wiring points assume the existing archetype vocabulary:
1. `feedback.handler.ts` `resolveCurriculumTier()` tierMap was written for pre-the business onboarding flow
   archetypes. 'fan_in' and 'convergence' are not in the map → default tier=3. The
   SESSION-TEACH-A0 seeds curriculum tiers correctly in the graph (tier 4 for FAN_IN and
   CONVERGENCE) but feedback.handler.ts doesn't read from the graph — it uses its own map.
2. Z-1.4 prompt.system fetch uses `{taskTypeId, promptType: 'genesis'}` but SESSION-A
   genesis prompt fixtures may be indexed by `promptKey` field only, without `taskTypeId`.
   The fetch and the seeder are not aligned on which field identifies genesis prompts.

**Fix path:**
1. Add to `resolveCurriculumTier()` tierMap: `fan_in: 4`, `convergence: 4`, `broadcast: 3`
2. Verify SESSION-A genesis prompt fixture documents contain `taskTypeId: 'a task type'/'a task type'/'a task type'` field alongside `promptKey` field; if absent, add to fixture seeding

---

### rc 6 — SESSION-E Authored Against Outdated DpoTriple Schema

**Gaps driven:** gap number 12

**Root:** SESSION-E Step E3 gap item 08 gate was authored when DpoTriple had nested
`chosen: { code: string }` and `rejected: { code: string }` subfields. The schema was
subsequently refactored to top-level `chosen: string` and `rejected: string` (simpler
representation). SESSION-E was not updated to reflect this schema change. The the search index exists
query against `chosen.code` and `rejected.code` will always return 0, causing Phase E
to report perpetual gap item 08 non-compliance.

**Fix path:** Update SESSION-E Step E3 gap item 08 the search index query:
- Replace `{"exists": {"field": "chosen.code"}}` → `{"exists": {"field": "chosen"}}`
- Replace `{"exists": {"field": "rejected.code"}}` → `{"exists": {"field": "rejected"}}`

---

### rc 7 — Planning Layer Designed Assuming the user registration flow Historical Signal

**Gaps driven:** gap number 15, gap number 16

**Root:** F-WAVE-GATE and F3-L2-7 were designed assuming the planning layer accumulates
signal from the user registration flow through the business onboarding flow (combined). the user registration flow predates the planning layer by
design — it has no `xiigen-planning-decisions` entries. The F-WAVE-GATE Check 2 threshold
of ≥12 VALIDATED planning triples is only achievable if the user registration flow contributes ~3–6 triples.
Similarly, F3-L2-7's success condition (count=0 OUTCOME_PENDING) produces a false positive
when the index is empty — the guard doesn't distinguish "nothing is pending" from "nothing
was ever written".

**Fix path:**
1. F-WAVE-GATE Check 2: lower threshold to ≥6 (achievable from the business onboarding flow alone) or document
   that this check requires the user registration flow planning backfill as a separate prerequisite task
2. F3-L2-7: add pre-check guard — if `TOTAL_FLOW02_TRIPLES=0` before update_by_query, print
   WARN and skip the "✅" success message; only report success if updated > 0

---

## CONVERGENCE SUMMARY — 16 Gaps → 7 Root Causes

```
16 SIMULATION GAPS
├── rc 1: Layer 2 index definitions absent ──────────────── gap number 1, gap number 2
│   Fix: 2 fixture files + bootstrapper entries
│
├── rc 2: the business onboarding flow task types not registered in runtime ──── gap number 3, gap number 4, gap number 8
│   Fix: ENGINE_CONTRACTS + ContractArchetype enum + a task type topology
│
├── rc 3: a task type CloudEvent name inconsistency (systemic) ──── gap number 5, simulation gap fourteen
│   Fix: 3-location rename in session files
│
├── rc 4: Archetype-specific quality enforcement absent ──── gap number 6, gap number 7, gap number 9, gap number 10
│   Fix: quality scorer + validate.handler + graph edge key alignment
│
├── rc 5: P18 fields not extended for the business onboarding flow archetypes ── gap number 11, gap number 13
│   Fix: tierMap + SESSION-A prompt seeding alignment
│
├── rc 6: SESSION-E vs DpoTriple schema drift ────────────── gap number 12
│   Fix: 2 field name corrections in SESSION-E Step E3
│
└── rc 7: Planning layer assumes the user registration flow baseline ─────────  gap number 15, gap number 16
    Fix: F-WAVE-GATE threshold + F3-L2-7 empty-index guard
```

---

## FIX PRIORITY MATRIX

| Priority | Root Cause | Blocking? | Effort | Gaps Resolved |
|----------|-----------|-----------|--------|---------------|
| P1 | rc 1: Create Layer 2 index fixtures | YES (H) | LOW — 2 JSON files | gap number 1, gap number 2 |
| P1 | rc 3: Rename a task type event in 3 session files | YES (B) | LOW — 3 text edits | gap number 5, simulation gap fourteen |
| P1 | rc 6: Fix SESSION-E gap item 08 field names | YES (B) | LOW — 2 text edits | gap number 12 |
| P2 | rc 2: Register a task type/a task type/a task type in engine | NO (C) | MEDIUM — engine-bootstrapper + enum + topology | gap number 3, gap number 4, gap number 8 |
| P2 | rc 5: Extend P18 tierMap + prompt seeding | NO→YES (C) | LOW-MEDIUM — 3 map entries + fixture check | gap number 11, gap number 13 |
| P3 | rc 4: Archetype quality enforcement | NO (D) | HIGH — quality scorer + validate.handler + graph key alignment | gap number 6, gap number 7, gap number 9, gap number 10 |
| P3 | rc 7: Planning layer threshold calibration | NO (D) | LOW — threshold + empty-index guard | gap number 15, gap number 16 |

**Total fixes to unblock Phase A through Phase F:** P1 items (3 root causes, 5 gaps) — all text edits or file additions, no engine code changes.

**Total fixes to achieve full SILENT_FAILURE elimination:** P1+P2+P3 (all 7 root causes).

---

## BLOCKING GAP RESOLUTION SEQUENCE

For the business onboarding flow sessions to run end-to-end without blocking:

```
1. Create xiigen-decision-graph fixture + register in bootstrapper  (rc 1, gap number 1)
2. Create xiigen-planning-decisions fixture + register              (rc 1, gap number 2)
3. Fix a task type event name in SESSION-TEACH-WHY-BROADCAST               (rc 3, simulation gap fourteen)
4. Fix a task type event name in SESSION-TEACH-A0 Step 5                   (rc 3, simulation gap fourteen)
5. Fix a task type event name in SESSION-F Step F2                         (rc 3, gap number 5)
6. Fix SESSION-E Step E3 gap item 08 query field names                  (rc 6, gap number 12)
7. Add taskTypeId to SESSION-A genesis prompt fixtures              (rc 5, gap number 13)

After steps 1-7: Phase A, Phase E, Phase F, and Wave 2 trigger chain all unblocked.
Remaining SILENT_FAILUREs (D-class) require engine code changes but don't block execution.
```

---

## SILENT_FAILURE SEVERITY RANKING

| Rank | Gap | Severity reason |
|------|-----|----------------|
| 1 | gap number 9 (convergence_threshold named check) | score-0 severity gate declared in contract; completely unenforceable. a task type code hardcoding confidence threshold passes all gates. |
| 2 | gap number 10 (degraded_terminal) | iron rule "never DataProcessResult.failure on low score" is a task type's most important constraint; scorer cannot detect violation |
| 3 | gap number 7 (fan_in_pattern/allSettled) | allSettled is the ONLY iron rule for a task type; Promise.all() violation passes quality scoring |
| 4 | gap number 11 (curriculumTier wrong for a task type/a task type) | FAN_IN and CONVERGENCE labeled as tier-3 (service) complexity when they are tier-4 (orchestration); OSS distillation signal miscalibrated |
| 5 | gap number 6 (named check key mismatch) | B-L2-2 creates spurious edges; seeded REQUIRES_CHECK edges stay at confidence 0.10 forever — planning graph never learns from evidence |
| 6 | gap number 8 (RouteHandler not wired in a task type) | Entry guard absent from a task type execution; wrong event types processed silently |
| 7 | gap number 15 (F3-L2-7 false positive) | Planning layer appears complete with zero evidence |

---

## ISSUE INVENTORY

| Issue | Source Round | Severity | Status |
|-------|-------------|----------|--------|
| gap number 1: xiigen-decision-graph absent | R13 | BLOCKING | Open — R20 delivery |
| gap number 2: xiigen-planning-decisions absent | R13 | BLOCKING | Open — R20 delivery |
| gap number 3: a task type/a task type/a task type not in bootstrap | R13 | WARNING | Open — R20 delivery |
| gap number 4: ContractArchetype case mismatch | R13 | WARNING | Open — R20 delivery |
| gap number 5: a task type event name (root) | R13 | WARNING | Open — R20 delivery |
| gap number 6: Named check key mismatch | R14 | WARNING | Open — R20 delivery |
| gap number 7: fan_in_pattern not in scorer | R15 | WARNING | Open — R20 delivery |
| gap number 8: RouteHandler not in a task type topology | R16 | WARNING | Open — R20 delivery |
| gap number 9: convergence_threshold not in NAMED_CHECKS | R16 | WARNING (score-0 severity) | Open — R20 delivery |
| gap number 10: degraded_terminal not in scorer | R16 | WARNING | Open — R20 delivery |
| gap number 11: curriculumTier wrong for a task type/a task type | R17 | WARNING | Open — R20 delivery |
| gap number 12: gap item 08 field name mismatch | R17 | BLOCKING | Open — R20 delivery |
| gap number 13: prompt.system taskTypeId field alignment | R17 | BLOCKING (conditional) | Open — R20 delivery |
| simulation gap fourteen: a task type event name systemic (3 locations) | R18 | BLOCKING | Open — R20 delivery |
| gap number 15: F3-L2-7 false positive | R18 | WARNING | Open — R20 delivery |
| gap number 16: F-WAVE-GATE Check 2 threshold | R18 | WARNING | Open — R20 delivery |
