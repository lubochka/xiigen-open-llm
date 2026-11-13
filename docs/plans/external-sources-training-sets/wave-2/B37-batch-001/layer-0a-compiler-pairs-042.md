# Layer 0-A — Compiler Pairs (042)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-042-001 — "value stream" is zero-predicate vocabulary

**Input A:** "Map the value stream for the delivery pipeline."
- "Map" → not in predicate lexicon → gap
- "value stream" → zero predicates → gap
- "delivery pipeline" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Design the value stream architecture."
- "Design" → ARCHITECTURE fires
- "value stream" → gap
- "architecture" → ARCHITECTURE fires

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-042-002 — "flow state" is zero-predicate vocabulary

**Input A:** "Monitor team flow state across the sprint."
- "Monitor" → gap
- "flow state" → zero predicates → gap
- "sprint" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Build flow state monitoring into the delivery dashboard."
- "Build" → IMPLEMENT fires
- "flow state" → gap
- "monitoring" → gap
- "delivery dashboard" → gap; "dashboard" alone → gap (not UI token without "UI" explicit)

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

---

## B37-0A-042-003 — "work breakdown structure" fires DOCUMENT

**Input A:** "Create a work breakdown structure for the programme."

Token scan:
- "Create" → IMPLEMENT fires
- "work breakdown structure" → "structure" fires ARCHITECTURE (Rule 7)
- "programme" → gap

Predicates: ["IMPLEMENT", "ARCHITECTURE"]
DomainId: "software-development"

**Input B:** "Maintain the work breakdown structure."
- "Maintain" → gap
- "work breakdown structure" → "structure" fires ARCHITECTURE

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"

**Rule:** "work breakdown structure" always fires ARCHITECTURE on "structure" token.

---

## B37-0A-042-004 — "project closure" is zero-predicate vocabulary

**Input A:** "Manage project closure activities."
- "Manage" → gap
- "project closure" → zero predicates → gap
- "activities" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Document project closure for stakeholders."
- "Document" → DOCUMENT fires
- "project closure" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-042-005 — "benefits realization" is zero-predicate vocabulary

**Input:** "Track benefits realization post-delivery."

Token scan:
- "Track" → NOT TEST (Rule 2) → gap
- "benefits realization" → zero predicates → gap
- "post-delivery" → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-track-001, gap-project-managing-manage-delivery-001
SCL: emitted
