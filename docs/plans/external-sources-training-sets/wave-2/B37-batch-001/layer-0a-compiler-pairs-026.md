# Layer 0-A — Compiler Pairs (026)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-026-001 — "capacity planning" gap vs ARCHITECTURE when building plan structure

**Input A:** "Run the capacity planning session for next quarter."

Token scan:
- "Run" → not in predicate lexicon → gap
- "capacity planning" → zero predicates → gap
- "next quarter" → not in predicate lexicon

Predicates: []
DomainId: "project-management"
GapId: gap-project-managing-manage-delivery-001

**Input B:** "Design the capacity planning structure for the programme."

Token scan:
- "Design" → ARCHITECTURE fires (Rule 7)
- "capacity planning" → zero predicates → gap
- "structure" → ARCHITECTURE fires (Rule 7)
- "programme" → gap

Multiple ARCHITECTURE triggers → ONE ARCHITECTURE predicate
Predicates: ["ARCHITECTURE"]
DomainId: "software-development"

---

## B37-0A-026-002 — "dependency mapping" gap vs ARCHITECTURE when building dependency graph

**Input A:** "Map team dependencies for the delivery workstream."

Token scan:
- "Map" → not in predicate lexicon → gap
- "team dependencies" → no API co-occurrence → gap (Rule 5)
- "delivery workstream" → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Design the dependency map architecture for the microservices."

Token scan:
- "Design" → ARCHITECTURE fires (Rule 7)
- "dependency map" → no API co-occurrence on "dependency" alone → gap
- "architecture" → ARCHITECTURE fires (Rule 7)
- "microservices" → IMPLEMENT fires (Rule 9: "service")

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
DomainId: "software-development"

**Key rule:** "dependency mapping" alone → gap. Adding "design/architecture/microservices" → predicates fire.

---

## B37-0A-026-003 — "critical path" is zero-predicate vocabulary

**Input A:** "Identify the critical path for the delivery programme."

Token scan:
- "Identify" → not in predicate lexicon → gap
- "critical path" → zero predicates → gap
- "delivery programme" → gap

Predicates: []
DomainId: "project-management"
GapId: gap-project-managing-manage-delivery-001, gap-project-managing-track-001

**Input B:** "Document the critical path and define acceptance criteria."

Token scan:
- "Document" → DOCUMENT fires
- "critical path" → gap
- "define" → DOCUMENT fires
- "acceptance criteria" → "criteria" fires DOCUMENT; "acceptance" → TEST fires

Predicates: ["DOCUMENT", "TEST"]
DomainId: "software-development"

---

## B37-0A-026-004 — "budget tracking" is gap vocabulary

**Input:** "Track budget utilisation and flag variance to the programme board."

Token scan:
- "Track" → NOT in TEST lexicon (Rule 2) → gap
- "budget utilisation" → zero predicates → gap
- "flag" → not in predicate lexicon → gap
- "variance" → not in predicate lexicon → gap
- "programme board" → zero predicates (confirmed false-positive-free) → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-track-001, gap-project-managing-manage-delivery-001
SCL: emitted

---

## B37-0A-026-005 — "vendor management" is gap vocabulary

**Input A:** "Manage vendor relationships and track SLA compliance."

Token scan:
- "Manage" → not in predicate lexicon → gap
- "vendor relationships" → zero predicates → gap
- "track" → NOT TEST → gap
- "SLA compliance" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Build the vendor management portal and define SLA tracking requirements."

Token scan:
- "Build" → IMPLEMENT fires
- "vendor management portal" → "portal" ambiguous; "build" → IMPLEMENT already
- "define" → DOCUMENT fires
- "SLA tracking requirements" → "requirements" → DOCUMENT fires

Predicates: ["IMPLEMENT", "DOCUMENT"]
DomainId: "software-development"
