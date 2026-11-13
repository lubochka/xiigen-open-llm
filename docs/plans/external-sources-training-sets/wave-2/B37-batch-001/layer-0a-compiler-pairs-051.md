# Layer 0-A — Compiler Pairs (051)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-051-001 — "SAFe" (Scaled Agile Framework) full vocabulary boundary

**Input A:** "Coordinate SAFe ceremonies." → Predicates: []
**Input B:** "Track SAFe metrics." → Predicates: [] — "track" → NOT TEST
**Input C:** "Run SAFe PI planning." → Predicates: [] — "PI planning" → gap
**Input D:** "Implement SAFe across the enterprise." → ["IMPLEMENT"]
**Input E:** "Review SAFe adoption outcomes." → ["QA"]

"SAFe" = zero-predicate gap vocabulary. It does not self-trigger any predicate.

---

## B37-0A-051-002 — "program increment" (PI) boundary tests

**Input A:** "Run the PI planning ceremony." → Predicates: []
**Input B:** "Track PI objectives." → Predicates: []
**Input C:** "Review PI outcomes." → ["QA"]
**Input D:** "Define PI goals." → ["DOCUMENT"]
**Input E:** "Build the PI tracking dashboard." → ["IMPLEMENT"]

"PI" and "program increment" = zero-predicate vocabulary. Predicates from co-occurring tokens only.

---

## B37-0A-051-003 — "portfolio management" boundary tests

**Input A:** "Coordinate portfolio management." → Predicates: []
**Input B:** "Track portfolio performance." → Predicates: []
**Input C:** "Design the portfolio management structure." → ["ARCHITECTURE"]
**Input D:** "Document portfolio KPIs for stakeholders." → ["DOCUMENT"]
**Input E:** "Build the portfolio dashboard." → ["IMPLEMENT"]

All confirm: "portfolio management" = gap. Predicates from co-occurring tokens.

---

## B37-0A-051-004 — "value stream mapping" vocabulary

**Input A:** "Map the value stream." → Predicates: []
**Input B:** "Map value stream flows." → Predicates: []
**Input C:** "Design the value stream architecture." → ["ARCHITECTURE"]
**Input D:** "Document the value stream map for stakeholders." → ["DOCUMENT"]
**Input E:** "Build the value stream visualisation service." → ["IMPLEMENT"]

"value stream" and "map" (as verb) = gap. "architecture" fires ARCHITECTURE; "document" fires DOCUMENT.

---

## B37-0A-051-005 — Comprehensive Round 4 predicate boundary table

| Input pattern | Predicates | DomainId |
|--------------|-----------|---------|
| waterfall alone | [] | project-management |
| agile transformation alone | [] | project-management |
| SAFe alone | [] | project-management |
| program increment alone | [] | project-management |
| portfolio management alone | [] | project-management |
| value stream alone | [] | project-management |
| flow state alone | [] | project-management |
| work breakdown structure | [ARCHITECTURE] | software-development |
| project closure alone | [] | project-management |
| benefits realization alone | [] | project-management |
| KPI tracking alone | [] | project-management |
| build KPI dashboard | [IMPLEMENT] | software-development |
