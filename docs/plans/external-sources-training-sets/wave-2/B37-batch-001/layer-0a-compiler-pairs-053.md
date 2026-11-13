# Layer 0-A — Compiler Pairs (053)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-053-001 — "agile transformation" with all predicate types

**Input A:** "Manage agile transformation." → []
**Input B:** "Define agile transformation scope." → ["DOCUMENT"] — "define" fires
**Input C:** "Design agile transformation architecture." → ["ARCHITECTURE"]
**Input D:** "Implement agile transformation tooling." → ["IMPLEMENT"]
**Input E:** "Review agile transformation outcomes." → ["QA"]

Five predicate types tested. "agile transformation" contributes gap only.

---

## B37-0A-053-002 — "flow state" + "value stream" long managing sentence

**Input:** "Monitor flow state, map value streams, track portfolio management, and coordinate program increment planning."

All tokens: gap vocabulary.
Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, track-001, coordinate-001
SCL: emitted

---

## B37-0A-053-003 — "project closure" full predicate boundary

**Input A:** "Manage project closure." → []
**Input B:** "Review project closure." → ["QA"]
**Input C:** "Document project closure for stakeholders." → ["DOCUMENT"]
**Input D:** "Define project closure criteria." → ["DOCUMENT"]
**Input E:** "Build the project closure dashboard." → ["IMPLEMENT"]

"project closure" = gap. Five predicate types all fire from co-occurring token.

---

## B37-0A-053-004 — "SAFe" + "program increment" combined

**Input A:** "Coordinate SAFe program increment planning." → []
**Input B:** "Run SAFe PI ceremonies." → []
**Input C:** "Implement SAFe PI tooling." → ["IMPLEMENT"]
**Input D:** "Define SAFe PI acceptance criteria." → ["DOCUMENT"]

Both "SAFe" and "program increment" = gap. Only co-occurring predicate tokens fire.

---

## B37-0A-053-005 — "portfolio management" + "benefits realization" combined

**Input:** "Track portfolio management outcomes and benefits realization metrics."

Token scan:
- "Track" → NOT TEST → gap
- "portfolio management" → gap
- "outcomes" → gap
- "benefits realization" → gap
- "metrics" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted
