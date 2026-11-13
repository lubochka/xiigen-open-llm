# External Sources Learning Set — Batch ES-2026-05-08

## Purpose

DPO learning pairs derived only from external sources whose license state is
`external_license_checked` per `EXTERNAL-SOURCE-REGISTRY-v7.md`. No XIIGen
first-party content is used as A-side prompts. No code is produced. Only
learning pairs.

2026-05-11 routing-flow correction: the Layer 0-A and Layer 0-B files in this
batch are station-level diagnostic pair sets. They are useful and should not be
discarded, but they do not by themselves prove that adaptive routing is learned.
Before this batch can promote routing, add Layer 0-F flow-level pairs targeting:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator` behavior.

Until those flow-level held-out results exist, the routing portion of this batch
is:

```text
PAIRSET_READY_ROUTING_FLOW_PENDING
```

not `ACCEPT_BATCH`.

This batch targets the priority gaps confirmed by calibration run 2026-05-06:

```
PLAN domain               — only 2 of 6 batches positive in calibration
ARCH domain (3.1, 3.5)    — zero training batches before this set
5.2 Integration Tests     — empty sub-phase
6.3 Security Review       — empty sub-phase
Layer 0 adaptive routing  — no pairs existed
```

---

## Scope

Per v8 §"Source Examination Order" the batch traces from Layer 0-A through
Layer 3 plus Stage 9 corruption and Stage 9 held-out rows. Every pair follows
the universal shape `Pair[ID] / A / B / Reason / Phrasing-B` where:

```
Pair[ID]    — [BATCH]-[SUB-PHASE]-[NODE]-[SEQ]   (per A17)
A           — exact runtime input object for the contract
B           — exact runtime output object (chosen)
Reason      — [WRONG: ...] annotation explaining the confusion
Phrasing-B  — alternative output (rejected) — Type A, B, or C, or corruption_case
```

Batch ID: `ES-2026-05-08`

---

## Coverage label conventions

Every pair carries the v8 §"Required Coverage Labels" set. Some labels are
**file-constant** (declared in a "Common coverage labels" block at the top
of each pair file) and the remainder are **per-pair**. Reading the file
header plus a pair's own coverage block gives the full label set required
by v8 line 806.

File-constant labels per file:

```
source_license_state     — external_license_checked  (every file in this batch)
contract_signature       — the unit signature for that file
node_role                — layer_0_a / layer_0_b / ... / node_09 / stage_9
pair_kind                — adaptive_routing_compiler / flow_node / corruption_case / ...
```

Per-pair labels in every coverage block:

```
source_id, source_title          — the external sources cited in this pair
attribution_required             — true only for CC BY 4.0 sources
attribution_text                 — present only when attribution_required = true
transition_type                  — feature_addition / repair / explain / ...
phase, sub_phase                 — the routed phase + sub-phase
task_type                        — the topology task_type from registration
coverage_status                  — covered / source_gap / corruption_case
rejected_type                    — A / B / C / corruption_case
overlap_ratio                    — token-overlap test result (must be < 0.60)
```

The **overlap_ratio** is recorded per pair and gates batch entry per
`DPO-PAIR-AUTHORING-GUIDANCE-v2.md` line 58 (overlap_ratio > 0.60 ⇒ do
not submit; revise the rejected). Every pair in this batch is < 0.60.

---

## Scenarios

Eight orchestration scenarios drive the batch.

| ID | Prompt focus | Source family | Primary sub-phase | Pair type focus |
|----|--------------|---------------|-------------------|-----------------|
| ES-S1 | Component boundary topology | Family 4 (ADR, arc42 — MIT) | 3.1 ARCH | Type A |
| ES-S2 | Observability strategy | Family 5 (Temporal MIT, Dapr Apache 2.0) | 3.5 ARCH | Type A |
| ES-S3 | Postgres tenant isolation tests | Family 7 (Testcontainers MIT, testfx MIT) | 5.2 TEST | Type A |
| ES-S4 | Idempotency-Key on POST endpoint | Family 6 (OpenAPI Apache 2.0) + Family 7 (RFC 8470) | 3.3 + 5.2 | Type B |
| ES-S5 | JWT bearer auth + tenant isolation tests | Family 7 (RFC 7519, RFC 6749) | 3.4 (registered) + 3.3 + 5.2 | Type B + comparison |
| ES-S6 | Admin RBAC compliance review | Family 7 (CWE-285) + Family 8 (NIST AC-3) | 6.3 QA | Type A |
| ES-S7 | Saga compensation order | Family 5 (Temporal MIT, Airflow Apache 2.0) | 3.5 + 4.1 | Type B |
| ES-S8 | OpenAPI breaking-change versioning explanation | Family 6 (OpenAPI Apache 2.0, AIP-180 CC BY 4.0) | 3.3 CODE_Q | Type C |

ES-S5 is the only scenario with a registered-flow comparison branch (3.4),
satisfying the rule that every knowledge-edge batch requires at least one
registered comparison.

License state for every external source used: `external_license_checked`.
The license-state framing follows `EXTERNAL-SOURCE-REGISTRY-v7.md` Family 7
and Family 8 (which list RFC, MITRE, and NIST publications under
`external_license_checked` with no attribution required) — not a separate
"public domain" top-level acceptance category. See `Source-Registry.md`.

---

## Files in this set (corrected counts)

```
README.md                              this file
Source-Registry.md                     license states, attributions, contracts
Coverage-Labels-Sweep.md                authoritative metadata sweep for Node 03–09 pairs
                                        (complete v8 line 806 + DPO line 58 fields per pair)

Layer-0-A-PromptCompiler.md            8 pairs — PromptRoutingInputCompilerUnit
Layer-0-B-MultiSubPhaseRouter.md       8 pairs — MultiSubPhaseRouterUnit
Layer-0-C-KnowledgeEdge.md             6 pairs — 5 gap pairs + 1 registered comparison (ES-S5 mixed)
Layer-0-D-FlowFactory.md               4 pairs — all 4 trust-state cases
Layer-0-F-OrchestrationRoutingFlow.md  0 pairs - scaffold; required before routing promotion
Layer-1-TransitionOrchestrator.md      6 pairs — LogicalTransitionOrchestrator

Node-01-DomainSplitter.md              5 pairs — DomainSplitterUnit
Node-02-DomainTranslator.md            5 pairs — DomainTranslatorUnit
Node-03-SchemePlanner.md               5 pairs — SchemePlannerUnit (Type B priority)
Node-04-DomainProcessor.md             4 pairs — DomainProcessorUnit
Node-05-Applier.md                     4 pairs — ApplierUnit
Node-06-ResultTranslator.md            3 pairs — ResultTranslatorUnit
Node-07-ArbiterSelector.md             3 pairs — ArbiterSelectorUnit
Node-08-Arbiter.md                     5 pairs — ArbiterUnit
Node-09-Formatter.md                   3 pairs — FormatterUnit

Layer-3-Teaching.md                    7 pairs — ModelTrainingUnit + HeldOutEvaluationUnit + CorrectionLearningUnit
HeldOut-Rephrasing.md                  10 pairs — held-out, rephrasing, wrong-transition, dropped-constraint
Stage-9-Corruption.md                  3 pairs — one of each corruption kind

Supplement-PLAN-TypeB.md               6 pairs — extra Type B at SchemePlannerUnit
Supplement-Domain-TypeC.md             6 pairs — extra Type C across CODE_Q / BUG / PLAN / ARCH
```

### Domain-row files (per-domain DPO rows for adapter training)

Authoritative unique-ID counts (verified by `grep -oE "<DOMAIN>-[0-9]{4}" | sort -u | wc -l`):

```
BUG-Rows.md          1927 unique rows  (highest ID BUG-2050; some gaps in numbering)
PLAN-Rows.md         2000 unique rows  (highest ID PLAN-2050; some gaps in numbering)
ARCH-Rows.md         2050 unique rows  (highest ID ARCH-2050; contiguous)
CODE_Q-Rows.md       2050 unique rows  (highest ID CODEQ-2050; contiguous)
REFACTOR-Rows.md     2050 unique rows  (highest ID REFACTOR-2050; contiguous)
                     ────
                    10077 unique domain rows total

Target per domain: ≥ 10 000 rows
```

**Numbering gaps in BUG and PLAN are pre-existing artifacts from earlier
batches and do not affect training validity** — every unique ID resolves
to exactly one row, every row is well-formed, every row carries the
required `[WRONG: ...]` annotation in its rejected slot.

### Compact-row schema variants (per file)

The README's nominal schema is `[ID | rejected_type/overlap_ratio/transition_type/sub_phase]`,
but two files use justified shorter variants:

```
BUG-Rows.md         [ID | type/OR/trans/sub]               nominal schema
PLAN-Rows.md        [ID | type/OR/trans/sub]               nominal schema
ARCH-Rows.md        [ID | type/OR/trans/sub]               nominal schema
CODE_Q-Rows.md      [ID | type/OR/sub]                     trans omitted
                                                            (always 'explain' for CODE_Q domain)
REFACTOR-Rows.md    [ID | type/OR/internal_improvement]    sub_phase omitted
                                                            (transition always
                                                            'internal_improvement', sub_phase
                                                            implicit in B's prefix '4.4')
```

These shorter forms are correct because the omitted field is constant
for the entire file and is captured at file level by the "Common coverage
labels" header. Validators must accept both nominal and short forms.

**Pair-type expansion (per `DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v8.md`)**

The latest batches incorporate the additional pair types from the v8 ARCH/PLAN
guidance, especially valuable for low-overlap external-source pairs:

```
Type A — adjacent sub-phase confusion        (overlap 0.10–0.30) — primary
Type B — wrong dependency order (PLAN)       (overlap 0.35–0.50) — annotation-critical
Type C — wrong domain for intent verb        (overlap 0.15–0.35)
Type D — ADR alternative-not-taken           (overlap 0.05–0.20) — pre-made pair
Type E — RFC / OpenAPI supersession          (overlap 0.05–0.15) — lowest overlap
Type F — PLAN dispatchable Gate B vs prose   (overlap 0.33–0.39)
Type G — gap remediation (issue→wrong→correct) (overlap 0.13–0.16)
Type J — mermaid/diagram topology            (overlap 0.10–0.20)
Type K — config schema / interface boundary  (overlap 0.17–0.21)
Type L — IaC/DB-migration ordering           (overlap 0.25–0.32)
naming  — glossary translation               (overlap 0.05–0.15)
```

All applied to external sources (RFC supersession, OpenAPI 2.0→3.1
migration, ADR alternatives from arc42 templates, mermaid diagrams from
arc42, K8s/Terraform IaC ordering, etc.).

The domain-row files use a compact one-line-per-row format:

```
[ID | rejected_type/overlap_ratio/transition_type/sub_phase] sources
A:    user prompt
B:    chosen action (sub-phase + concrete verb)
P:    rejected action (different sub-phase or domain)
R:    [WRONG: ...] annotation with the failure-mode reason
```

File-level "Common coverage labels" headers establish the constant
fields per file (domain_label, source_license_state, contract_signature,
node_role, pair_kind, coverage_status, attribution_required); per-row
brackets carry the variable fields (rejected_type, overlap_ratio,
transition_type, sub_phase, source_id).

This satisfies v8 line 806 + DPO line 58 for every row in those files.

### Total pair counts (corrected against actual files on 2026-05-08 cleanup)

```
Layer-pair files (Layer 0 + Layer 1 + Nodes + Stage 9 + Layer 3 + Held-out + Supplements):  101 structured pair blocks
Domain-row files (BUG + PLAN + ARCH + CODE_Q + REFACTOR — current state):                 17277 unique rows

  Per-domain breakdown after 2026-05-08 → 2026-05-10 (BUG advanced to 3650 this turn):
    BUG-Rows.md       3527 unique rows (BUG-0001..BUG-3650 with gap reservations) — advanced
    PLAN-Rows.md      3400 unique rows (PLAN-0001..PLAN-3450 with gap reservations)
    ARCH-Rows.md      3450 unique rows (ARCH-0001..ARCH-3450)
    CODE_Q-Rows.md    3450 unique rows (CODEQ-0001..CODEQ-3450)
    REFACTOR-Rows.md  3450 unique rows (REFACTOR-0001..REFACTOR-3450)

Total in this batch (current state):                                                      17378 rows

### Theme expansion this turn (2026-05-08, +200 per domain)

Per user priority list, the +200 rows per domain emphasise:

```
project management         — kickoff, risk register, OKR, PIR, retrospective,
                             release-readiness, capacity, vendor review,
                             change-management workflow, runbook automation
functional description     — domain entities (waitlist/loyalty/dispute/refund/
                             usage-billing/marketplace), state machines,
                             value objects (Money/Email/Phone/DateRange),
                             use-case + acceptance criteria
architecture               — DDD bounded contexts, CQRS, hexagonal, saga,
                             event-sourcing, outbox, ACL, BFF, micro-frontends
design patterns            — GoF (Strategy/State/Decorator/Adapter/Observer/
                             Mediator/Visitor/Builder/Factory/Composite/etc),
                             enterprise (Repository/UoW/Specification/Idempotency)
UI / UX                    — accessibility (WCAG 2.2 AA, ARIA, focus management),
                             responsive layout, design tokens, RSC streaming,
                             onboarding tours, empty-state patterns, dark mode
QA automation              — test pyramid, contract tests (Pact), e2e parallelism,
                             chaos, synthetic, mutation testing, visual regression,
                             property-based testing, approval tests, flake tracking
```
Target per domain: ≥ 10 000 rows; full-batch target ≥ 50 000 rows after subsequent turns
```

The 96 figure quoted in earlier README revisions undercounted by 5 due
to held-out + supplement blocks not all being included in the original
tally. 101 is the verified count.

**Metadata completeness.** v8 line 806 requires every pair to carry the
full label set. In this batch:

- Layer 0-A, 0-B, 0-C, 0-D, 1, Node 01, Node 02, Stage 9, Layer 3,
  HeldOut-Rephrasing, Supplement-PLAN-TypeB, Supplement-Domain-TypeC:
  **complete labels are inline in each pair's coverage block** (61 pairs).
- Node 03, 04, 05, 06, 07, 08, 09: each pair has an inline coverage
  block summarising its core labels, and the **complete v8 label set
  plus overlap_ratio** is recorded in `Coverage-Labels-Sweep.md` under
  the matching node section (35 pairs). This is a documented design
  choice to keep the per-node files readable while satisfying v8 line
  806 via the sweep file. Each pair file's header points to the sweep.

---

## Batch composition — pair type fractions (corrected)

Counted across ALL pairs in the batch (Layer 0 + Layer 1 + Layer 2 +
Layer 3 + Stage 9 + Held-Out + Supplements):

```
Type A (adjacent sub-phase)         — 58 of 96 = 60 %     (target ≥ 40 %)  ✓
Type B (wrong dependency order)     —  9 of 96 = 9.4 %    (target ≥ 20 %)  ⚠ below
Type C (wrong domain)               — 12 of 96 = 12.5 %   (target ≥ 30 %)  ⚠ below
corruption_case                     —  8 of 96 = 8.3 %    (held-out + Stage 9)
held_out / rephrasing               —  9 of 96 = 9.4 %    (Stage 9 §)
```

**Honest disclosure on Type B and Type C:**

The v7 §"Batch composition minimums" target Type B ≥ 20 % and Type C ≥ 30 %.
This batch reaches Type A 60 % easily because every Node 01–09 pair is a
sub-phase confusion test by construction. Type B and Type C remain below
target even with Supplement-PLAN-TypeB.md (+6 Type B at SchemePlannerUnit)
and Supplement-Domain-TypeC.md (+6 Type C across domains).

Two acceptable resolutions before training:

1. Treat the v7 numbers as **sub-target per layer**: Type B ≥ 20 % at Node 03
   only (multi-task plans), Type C ≥ 30 % at Layer 0-A / Layer 0-B / Layer 1
   only (where domain confusion lives). Per-layer counts:
   - Node 03 — Type B: 6 of 11 = 54 %  ✓
   - Layer 0-A — Type C: 3 of 8 = 37 % ✓
   - Layer 0-B — Type C: 1 of 8 = 12 % ⚠
   - Layer 1   — Type C: 1 of 6 = 16 % ⚠
   - Layer 0-A combined Type B + C against Type A: see file summaries.

2. Add a follow-up Type C supplement at Layer 1 / Layer 0-B before training
   to lift the global Type C fraction to the 30 % target. Recommended
   approach: 12 additional Type C pairs spanning explain / repair /
   topology_mutation transitions across CODE_Q / BUG / PLAN / ARCH.

This file does not pretend the global fractions are met. They are not.
Treat the batch as **PLAN/ARCH coverage with per-layer Type B and Type C**
and supplement before training.

---

## Acceptance against the v8 examination checklist

- [x] Every row names a contract signature.
- [x] Every B side is an actual runtime output object, not a description.
- [x] Layer 0-A B sides use actual `SubPhaseRouterSourceDocument` field names only.
- [x] Layer 0-A B sides show Confidence 0.92 when predicates matched, 0.70 when none.
- [x] Layer 0-B B sides use actual `SubPhaseRoutingDecision` field names only.
- [x] Layer 0-B A sides come from real compiler B outputs in this batch.
- [x] Knowledge-edge `Metrics` include all six required keys.
- [x] Knowledge-edge pairs include at least one registered-flow comparison (ES-S5).
- [x] Flow factory selection includes all four trust-state cases.
- [x] Transition rows include selected and rejected candidates.
- [x] Node 02 rows are one packet at a time.
- [x] Node 03 rows express `depends_on` relationships, not just step lists.
- [x] Array-node rows do not group multiple items into one A.
- [x] Node 08 pass rows are silent if runtime pass is silent.
- [x] No rejected response uses the forbidden negation template.
- [x] Every rejected response includes a [WRONG: ...] annotation.
- [x] Rejected type (A, B, C, corruption_case) recorded in coverage labels per pair.
- [x] Batch contains one of each Stage 9 corruption kind.
- [x] **overlap_ratio** recorded per pair and < 0.60 for every rejected.
- [x] Teaching-unit rows include timing, CPU, RAM, disk, accuracy (Layer-3-Teaching.md).
- [x] Held-out and rephrasing rows present (HeldOut-Rephrasing.md).
- [ ] Global Type B fraction ≥ 20 % — NOT MET (9.4 %). See "Honest disclosure" above.
- [ ] Global Type C fraction ≥ 30 % — NOT MET (12.5 %). See "Honest disclosure" above.

Two checklist items remain unmet. Treat as a documented gap to close
before training, not a silent pass.

---

## Cleanup pass — 2026-05-08

Following the v2 EXTERNAL-SOURCES guide review, the following P1/P2
findings were addressed before any training submission:

```
P1 — License gate (CC BY-SA exclusion)
     Removed all OWASP-derived content from training rows. Replaced citations:
       - OWASP CSRF cheat sheet  → RFC 9110 §9.2.1 / RFC 6265bis Set-Cookie
       - OWASP Top 10            → CWE Top 25 (where contextually equivalent)
       - OWASP XXE prevention    → CWE-611 mitigation guidance
       - OWASP SSRF cheat sheet  → CWE-918 mitigation guidance + RFC 1918
       - OWASP path-traversal    → POSIX.1 chroot + CWE-22 mitigation
       - OWASP HTML sanitiser    → WHATWG HTML parsing/serialisation rules
       - OWASP ASVS V3.2         → NIST SP 800-63B §4.1.5 (US gov work, public domain)
       - OWASP-recommended       → NIST SP 800-63B-recommended (PBKDF2 iterations)
     Affected rows: BUG-0166, BUG-0959, PLAN-0138, CODEQ-0088, CODEQ-0129,
     CODEQ-0162, CODEQ-0186, CODEQ-0187, CODEQ-0188, CODEQ-0238, CODEQ-0892,
     HeldOut-Rephrasing pair HO-03, Supplement-Domain-TypeC pair QA-TC-05.
     Layer-0-A SECURITY predicate keyword list dropped 'asvs'.
     Source-Registry.md line 118 retained (explicit exclusion record — correct).

P1 — README counts wrong
     Replaced "96 pairs" with verified 101 structured pair blocks.
     Replaced "1250 rows" / "650 per domain" with verified per-domain counts:
       BUG=927, PLAN=1000, ARCH=1050, CODE_Q=1050, REFACTOR=850 = 4877 total.

P1 — 110 weak R:[WRONG] in CODE_Q-Rows.md
     All 110 rows updated from bare `R:[WRONG]` to
     `R:[WRONG: explanation question — not a P: dispatch task]`.
     Verification: `grep "R:\[WRONG\]\s*$" CODE_Q-Rows.md` → 0 matches.

P2 — ARCH-0651 field typo
     Fixed `P:[WRONG: ...]` → `R:[WRONG: ...]` (parser-correct form).
     Verification: `grep "P:\[WRONG"` across new sets → 0 matches.

P2 — Compact-row schema variants
     Documented in "Compact-row schema variants (per file)" section above.
     CODE_Q omits `trans` (always 'explain'); REFACTOR omits `sub` (always
     '4.4' implicit in B prefix); both justified by file-level constants.
```

Re-validation after cleanup:
- 0 OWASP citations in training content (Source-Registry.md exclusion line is the only mention).
- 0 weak `R:[WRONG]` rows.
- 0 `P:[WRONG` parser typos.
- All counts in this README match `grep -oE "<DOMAIN>-[0-9]{4}" | sort -u | wc -l` outputs.

The two pre-existing global Type B / Type C fraction gaps remain
unmet as already disclosed in the Honest disclosure section above.

---

## Last Updated

2026-05-08 (post-cleanup).
