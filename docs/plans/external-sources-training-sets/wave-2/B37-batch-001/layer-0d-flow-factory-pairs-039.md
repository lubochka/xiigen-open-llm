# Layer 0-D — Flow Factory Pairs (039)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-039-001 — Factory: all B37 rounds combined stress test → no factory

"Track milestones, coordinate blockers, manage risk log, run escalation process, coordinate CCB, monitor scope creep, manage waterfall, coordinate agile transformation, track SAFe, run PI, manage portfolio, map value streams, monitor flow state, manage project closure, track benefits realization, monitor KPI."

All gap vocabulary → [] → no factory → knowledge_edge
SCL: emitted. DomainId: "project-management".

---

## B37-0D-039-002 — Factory: all rounds + stakeholders → DocumentationFlow

Same as 039-001 + "and report to stakeholders."
"stakeholders" → [DOCUMENT] → DocumentationFlow (2.1)
SCL: NOT emitted. DomainId: "software-development".

---

## B37-0D-039-003 — Factory: all rounds + service → BackendServiceFlow41

Same as 039-001 + "and build the tracking service."
"service" → [IMPLEMENT] → BackendServiceFlow41 (4.1)
SCL: NOT emitted. DomainId: "software-development".

---

## B37-0D-039-004 — Factory: all rounds + work breakdown structure → ArchitectureFlow31

Same as 039-001 + "and maintain the work breakdown structure."
"structure" → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
SCL: NOT emitted. DomainId: "software-development".

---

## B37-0D-039-005 — Factory: all rounds + review → QA sub-phase

Same as 039-001 + "and review quality gate effectiveness."
"review" → [QA] → QA sub-phase
SCL: NOT emitted. DomainId: "software-development".

Pattern: ONE additional predicate token changes routing from knowledge_edge to predicate-appropriate factory.
