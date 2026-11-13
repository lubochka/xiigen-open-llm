# Layer 0-A — Compiler Pairs (055)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-055-001 — All-rounds zero-predicate stress test #1

**Input:** "Track milestones, coordinate blockers, run escalation, facilitate project kickoff, monitor waterfall delivery, coordinate agile transformation, track SAFe adoption, and manage portfolio."

All tokens: gap vocabulary across all B37 rounds.
Predicates: []
DomainId: "project-management"
SCL: emitted (once)

---

## B37-0A-055-002 — All-rounds zero-predicate stress test #2

**Input:** "Manage risk log, process change requests, circulate project charter, run post-mortem, manage project closure, and track benefits realization."

All tokens: gap vocabulary.
Predicates: []
DomainId: "project-management"
SCL: emitted (once)

---

## B37-0A-055-003 — One DOCUMENT trigger in all-rounds sentence

**Input:** "Track milestones, coordinate blockers, run escalation, manage agile transformation, and send the communication plan to stakeholders."

"stakeholders" → DOCUMENT fires.
Predicates: ["DOCUMENT"]
DomainId: "software-development"
SCL: NOT emitted
All other tokens → gap warnings forwarded

---

## B37-0A-055-004 — One IMPLEMENT trigger in all-rounds sentence

**Input:** "Track milestones, manage SAFe adoption, coordinate change control board, and build the benefits realization dashboard."

"build" → IMPLEMENT fires.
Predicates: ["IMPLEMENT"]
DomainId: "software-development"
SCL: NOT emitted

---

## B37-0A-055-005 — v6 corruption types for Round 4 vocabulary (049-060)

**v6-01 examples:**
- "Coordinate agile transformation" → ["AGILE"] — invented
- "Run project closure" → ["CLOSURE"] — invented
- "Monitor KPI dashboard" → ["KPI_TRACKING"] — invented
Correct for all three: Predicates: []

**v6-02 examples:**
- "Send waterfall status to stakeholders" → DOCUMENT suppressed — v6-02
- "Portfolio update for stakeholders" → DOCUMENT suppressed — v6-02
Correct: DOCUMENT fires on "stakeholders" — no suppression.

**v6-03 examples:**
- "Design agile structure" → DomainId "project-management" despite ARCHITECTURE — v6-03
- "Review portfolio outcomes" → DomainId "project-management" despite QA — v6-03
Correct: DomainId "software-development" when any predicate fires.
