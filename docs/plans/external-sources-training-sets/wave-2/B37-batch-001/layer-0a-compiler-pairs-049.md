# Layer 0-A — Compiler Pairs (049)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-049-001 — Long PM sentence with only Round 4 vocabulary (049-060)

**Input:** "Coordinate waterfall delivery, manage the agile transformation, track SAFe adoption, oversee program increment planning, and monitor portfolio management."

Token scan:
- All tokens = gap vocabulary
- "Coordinate" → gap; "waterfall" → gap; "manage" → gap; "agile transformation" → gap
- "track" → NOT TEST → gap; "SAFe adoption" → gap; "oversee" → gap
- "program increment" → gap; "monitor" → gap; "portfolio management" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-049-002 — Round 4 single-predicate triggers

**Input A:** "Design the waterfall phase structure." → ["ARCHITECTURE"]
**Input B:** "Define the agile transformation roadmap." → ["DOCUMENT"]
**Input C:** "Implement SAFe tooling." → ["IMPLEMENT"]
**Input D:** "Define program increment acceptance criteria." → ["DOCUMENT"]
**Input E:** "Review portfolio management outcomes." → ["QA"]

All five: one predicate fires; DomainId: "software-development".

---

## B37-0A-049-003 — "value stream" + "flow state" vocabulary with predicate

**Input A:** "Map value streams and monitor flow state." → Predicates: []
**Input B:** "Design the value stream architecture." → ["ARCHITECTURE"]
**Input C:** "Build the flow state dashboard service." → ["IMPLEMENT"]
**Input D:** "Document value stream flows for stakeholders." → ["DOCUMENT"]

"value stream" and "flow state" = gap. Predicates from co-occurring tokens.

---

## B37-0A-049-004 — "project closure" + "benefits realization" combined

**Input A:** "Manage project closure and track benefits realization." → Predicates: []
**Input B:** "Document project closure and benefits realization for stakeholders." → ["DOCUMENT"]
**Input C:** "Review project closure outcomes and validate benefits realization." → ["QA"]

"project closure" and "benefits realization" = gap vocabulary in all inputs.

---

## B37-0A-049-005 — Complete Round 4 (049-060) vocabulary list

**All zero-predicate when used alone:**
waterfall, agile transformation, SAFe, program increment, portfolio management,
value stream, flow state, work breakdown structure (WBS abbreviation only),
project closure, benefits realization, KPI tracking/monitoring

**"structure" exception:**
- "work breakdown structure" → ARCHITECTURE fires (on "structure")
- "waterfall phase structure" → ARCHITECTURE fires (on "structure")
- Any PM noun + "structure" → ARCHITECTURE fires

**DomainId rule unchanged:** zero predicates → "project-management"; any predicate → "software-development"
