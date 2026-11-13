# Layer 0-A — Compiler Pairs (041)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-041-001 — "waterfall" is zero-predicate vocabulary

**Input A:** "Run the project using a waterfall methodology."

Token scan:
- "Run" → gap
- "waterfall methodology" → zero predicates → gap
- "project" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Design the waterfall delivery process."
- "Design" → ARCHITECTURE fires
- "waterfall" → gap
- "delivery process" → gap

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-041-002 — "agile transformation" is zero-predicate vocabulary

**Input A:** "Lead the agile transformation programme."
- "Lead" → gap
- "agile transformation" → zero predicates → gap
- "programme" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Define the agile transformation roadmap."
- "Define" → DOCUMENT fires
- "agile transformation" → gap
- "roadmap" → alone → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-041-003 — "SAFe" (Scaled Agile Framework) is zero-predicate vocabulary

**Input A:** "Implement SAFe across the portfolio."

Token scan:
- "Implement" → IMPLEMENT fires
- "SAFe" → zero predicates → gap
- "portfolio" → zero predicates → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

**Input B:** "Coordinate SAFe adoption across teams."
- "Coordinate" → gap
- "SAFe adoption" → gap
- "teams" → gap

Predicates: []
DomainId: "project-management"

**Rule:** "SAFe" alone → gap. "Implement SAFe" → IMPLEMENT fires on "Implement".

---

## B37-0A-041-004 — "program increment" is zero-predicate vocabulary

**Input A:** "Plan the program increment for Q3."
- "Plan" → not in predicate lexicon → gap
- "program increment" → zero predicates → gap
- Predicates: []

**Input B:** "Define program increment objectives."
- "Define" → DOCUMENT fires
- "program increment" → gap
- "objectives" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-041-005 — "portfolio management" is zero-predicate vocabulary

**Input:** "Coordinate portfolio management across all programmes."

Token scan:
- "Coordinate" → gap
- "portfolio management" → zero predicates → gap
- "programmes" → zero predicates → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-coordinate-001, gap-project-managing-manage-delivery-001
SCL: emitted
