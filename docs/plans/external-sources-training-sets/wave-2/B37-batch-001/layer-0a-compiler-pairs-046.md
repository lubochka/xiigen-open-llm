# Layer 0-A — Compiler Pairs (046)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-046-001 — "waterfall" in delivery context — no predicate

**Input A:** "Track waterfall delivery phases." → Predicates: []
**Input B:** "Coordinate waterfall and agile workstreams." → Predicates: []
**Input C:** "Design the waterfall phase structure." → ["ARCHITECTURE"] — "design" + "structure"
**Input D:** "Document the waterfall methodology for stakeholders." → ["DOCUMENT"]

"waterfall" = gap. Predicates from co-occurring tokens only.

---

## B37-0A-046-002 — "agile transformation" vocabulary combinations

**Input A:** "Manage the agile transformation." → Predicates: []
**Input B:** "Define the agile transformation roadmap for stakeholders." → ["DOCUMENT"] — "define" + "stakeholders"
**Input C:** "Design the agile transformation architecture." → ["ARCHITECTURE"]
**Input D:** "Implement the agile transformation platform." → ["IMPLEMENT"]

"agile transformation" = gap vocabulary. DomainId "project-management" only for Input A.

---

## B37-0A-046-003 — "program increment" with ceremony vocabulary

**Input A:** "Facilitate program increment planning." → Predicates: []
**Input B:** "Facilitate program increment review." → ["QA"] — "review" fires
**Input C:** "Document program increment outcomes." → ["DOCUMENT"]
**Input D:** "Define program increment acceptance criteria." → ["DOCUMENT"] — "define" + "criteria"

"program increment" = gap. Program increment review → QA fires on "review" (same rule as all reviewing).

---

## B37-0A-046-004 — "SAFe" vocabulary: coordination gap vs implementation predicate

**Input A:** "Coordinate SAFe across the enterprise." → Predicates: []
**Input B:** "Roll out SAFe to new teams." → Predicates: [] — "roll out" → gap
**Input C:** "Implement SAFe tooling." → ["IMPLEMENT"]
**Input D:** "Define SAFe governance structure." → ["DOCUMENT", "ARCHITECTURE"]

SAFe = gap. "Implement" + SAFe → IMPLEMENT fires on "Implement", not on "SAFe".

---

## B37-0A-046-005 — "value stream" + "flow state" combined vocabulary

**Input:** "Monitor value stream flow state and manage team throughput."

Token scan:
- "Monitor" → gap
- "value stream" → gap
- "flow state" → gap
- "manage" → gap
- "team throughput" → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-manage-delivery-001, gap-project-managing-track-001
SCL: emitted
