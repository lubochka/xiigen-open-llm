# Layer 0-D — Flow Factory Pairs (033)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-033-001 — Factory selection: combined Round 3a + 4b zero-predicate

"Manage escalation process, coordinate CCB, monitor scope creep, run risk mitigation, manage waterfall delivery, coordinate agile transformation, track SAFe, run PI planning."
→ [] → no factory → knowledge_edge
SCL: emitted. DomainId: "project-management".

---

## B37-0D-033-002 — Factory selection: combined + one DOCUMENT trigger

"Manage escalation, coordinate CCB, monitor waterfall, coordinate SAFe, document findings for stakeholders."
→ [DOCUMENT] → DocumentationFlow (2.1)
SCL: NOT emitted. DomainId: "software-development".
All managing vocabulary → CapabilityGapWarnings forwarded.

---

## B37-0D-033-003 — Factory selection: combined + one IMPLEMENT trigger

"Manage escalation, coordinate CCB, monitor waterfall, coordinate SAFe, build the tracking service."
→ [IMPLEMENT] → BackendServiceFlow41 (4.1)
SCL: NOT emitted. DomainId: "software-development".
All managing vocabulary → CapabilityGapWarnings forwarded.

---

## B37-0D-033-004 — Factory selection: combined + one ARCHITECTURE trigger

"Manage escalation, coordinate CCB, monitor waterfall, coordinate SAFe, design the governance structure."
→ [ARCHITECTURE] → ArchitectureFlow31 (3.1)
SCL: NOT emitted. DomainId: "software-development".

---

## B37-0D-033-005 — Factory selection: combined + one QA trigger

"Manage escalation, coordinate CCB, monitor waterfall, coordinate SAFe, review the quality gate."
→ [QA] → QA sub-phase
SCL: NOT emitted. DomainId: "software-development".
All managing vocabulary → CapabilityGapWarnings forwarded.

Pattern confirmed: one predicate token changes routing from knowledge_edge to predicate-appropriate factory.
