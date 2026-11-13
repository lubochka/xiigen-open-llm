# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-059-001 — Knowledge-edge: "scope creep" + "waterfall" + "agile transformation"

1. "Monitor scope creep during waterfall and agile transformation." → knowledge_edge
2. "Coordinate scope creep management across waterfall and agile." → knowledge_edge | coordinate-001
3. "Design scope management architecture for waterfall and agile transformation." → NOT knowledge_edge
4. "Build scope tracking service for waterfall delivery." → NOT knowledge_edge

---

## B37-0C-059-002 — Knowledge-edge: "risk mitigation" + "benefits realization" + "portfolio"

1. "Manage risk mitigation and track benefits realization in portfolio context." → knowledge_edge
2. "Coordinate risk mitigation, benefits, and portfolio activities." → knowledge_edge | coordinate-001
3. "Document risk mitigation and benefits realization for portfolio stakeholders." → NOT knowledge_edge
4. "Review risk mitigation and benefits realization outcomes." → NOT knowledge_edge (QA fires)

---

## B37-0C-059-003 — Knowledge-edge: "SAFe" + "program increment" + "flow state"

1. "Coordinate SAFe PI ceremonies and monitor team flow state." → knowledge_edge
2. "Track SAFe PI metrics and flow state." → knowledge_edge | track-001
3. "Build SAFe PI and flow state monitoring service." → NOT knowledge_edge (IMPLEMENT fires)
4. "Design SAFe PI and flow state architecture." → NOT knowledge_edge (ARCHITECTURE fires)

---

## B37-0C-059-004 — Knowledge-edge: "project kickoff" + "communication plan" + "steering committee"

1. "Facilitate project kickoff, prepare communication plan, coordinate steering committee." → knowledge_edge
2. "Run project kickoff and present to steering committee." → knowledge_edge
3. "Present project kickoff communication plan to steering committee and stakeholders." → NOT knowledge_edge (stakeholders fires DOCUMENT)
4. "Document kickoff and communication plan for steering committee and stakeholders." → NOT knowledge_edge

---

## B37-0C-059-005 — Knowledge-edge: final comprehensive vocabulary test

All B37 gap vocabulary families (all rounds) → knowledge_edge when used alone without predicate tokens.
DomainId: "project-management" for every knowledge_edge response.
SCL: exactly one per request.
Gate B: zero v6 corruptions on knowledge_edge identification.

Test: For each of the 30+ B37 vocabulary families, using only gap verbs → knowledge_edge. Adding any of: stakeholders, structure, design, architecture, build, create, implement, service, module, review, validate, define, document, write, criteria → NOT knowledge_edge.
