# Layer 0-B — Router Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-009-001 — [QA] → QA sub-phase (sprint burndown review)

**Input predicate list:** `["QA"]`
**Request origin:** "Review the sprint burndown report and approve the release readiness."

**Router decision tree:**

```
predicates contains QA? YES → QA sub-phase
predicates contains IMPLEMENT? NO
predicates contains DOCUMENT? NO
→ Single path: QA → QA sub-phase
→ TransitionType: no_contribution
→ DomainId: "software-development"
→ CapabilityGapWarnings: sprint-001, burndown-001, release-cycle-001
```

**Router output:**
- Path: QA sub-phase
- TransitionType: no_contribution
- CapabilityGapWarnings: sprint-001, burndown-001, release-cycle-001

---

## B36-0B-009-002 — QA TransitionType is no_contribution

**Why QA → no_contribution:**
QA does not add new functionality (that would be feature_addition) nor does it produce documentation (that would be documentation). QA validates existing work. Its contribution type is no_contribution because it validates rather than creates.

This holds even in sprint context: reviewing a burndown report is QA validation; it does not contribute new artifacts to the codebase or documentation.

---

## B36-0B-009-003 — [QA] → QA: "approve" in milestone context

**Input predicate list:** `["QA"]`
**Request origin:** "Approve the sprint completion checklist before the milestone release date."

**Router decision:**
- QA → QA sub-phase
- TransitionType: no_contribution
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, milestone-001, release-cycle-001

---

## B36-0B-009-004 — QA routing does not create a nine-node flow

**Important boundary:**
QA routing enters the QA sub-phase — a gating check, not a full nine-node flow. The nine-node flow (phases 1 through 9) is associated with IMPLEMENT as the primary predicate. When only QA fires, the QA sub-phase executes in isolation.

When IMPLEMENT + QA both fire, the nine-node flow executes (driven by IMPLEMENT) and QA runs as the final gate. The QA sub-phase ordering: IMPLEMENT → DOCUMENT → QA, with QA always last.

| Predicate combination | Execution model |
|-----------------------|----------------|
| [QA] only | QA sub-phase only — no nine-node flow |
| [IMPLEMENT] only | Nine-node flow (4.1) |
| [IMPLEMENT, QA] | Nine-node flow (4.1) → QA gate at end |
| [IMPLEMENT, DOCUMENT, QA] | Nine-node (4.1 + 2.1) → QA gate at end |
