# Layer 0-B — Router Pairs (057)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-057-001 — "project closure" + "waterfall" long sentence routing

**Input:** "Manage waterfall project closure, run post-mortem, track benefits realization, and coordinate sign-off with programme board."

Token scan: all gap vocabulary.
Route: knowledge_edge
DomainId: "project-management"
SCL: emitted

---

## B37-0B-057-002 — Same sentence + "document for stakeholders"

**Input:** Same as 057-001, add "and document findings for stakeholders."

"document" → DOCUMENT fires; "stakeholders" → DOCUMENT fires (already fired).
Route: DocumentationFlow (2.1)
DomainId: "software-development"
SCL: NOT emitted
Gap warnings forwarded.

---

## B37-0B-057-003 — "SAFe" + "benefits realization" + "KPI dashboard" combined

**Input:** "Coordinate SAFe, track benefits realization, and build the KPI dashboard."

"build" → IMPLEMENT fires.
Route: BackendServiceFlow41 (4.1)
DomainId: "software-development"
SCL: NOT emitted
GapIds: coordinate-001, manage-delivery-001, track-001 forwarded.

---

## B37-0B-057-004 — "flow state" + "work breakdown structure" edge case

**Input:** "Monitor flow state and maintain work breakdown structure."

"work breakdown structure" → "structure" fires ARCHITECTURE.
Route: ArchitectureFlow31 (3.1)
DomainId: "software-development"
SCL: NOT emitted
GapId: manage-delivery-001 forwarded (from "flow state").

---

## B37-0B-057-005 — "waterfall" + "agile transformation" + "WBS" combined

**Input:** "Transition from waterfall to agile transformation and create work breakdown structure."

"create" → IMPLEMENT fires; "work breakdown structure" → "structure" → ARCHITECTURE fires.
Predicates: ["IMPLEMENT", "ARCHITECTURE"]
Routes: BackendServiceFlow41 (4.1) + ArchitectureFlow31 (3.1) — parallel eligible
DomainId: "software-development"
GapIds: manage-delivery-001 forwarded (waterfall + agile transformation).
