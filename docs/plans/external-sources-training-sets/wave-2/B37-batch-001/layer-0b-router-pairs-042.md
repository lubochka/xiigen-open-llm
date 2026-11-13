# Layer 0-B — Router Pairs (042)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-042-001 — "value stream" routing cases

**Case A:** "Map value streams." → knowledge_edge | project-management | SCL
**Case B:** "Design value stream architecture." → ArchitectureFlow31 (3.1)
**Case C:** "Document value stream flows for stakeholders." → DocumentationFlow (2.1)
**Case D:** "Build value stream visualisation service." → BackendServiceFlow41 (4.1)
**Case E:** "Review value stream mapping." → QA sub-phase

---

## B37-0B-042-002 — "flow state" routing cases

**Case A:** "Monitor team flow state." → knowledge_edge | project-management | SCL
**Case B:** "Build flow state dashboard service." → BackendServiceFlow41 (4.1)
**Case C:** "Design flow state monitoring architecture." → ArchitectureFlow31 (3.1)
**Case D:** "Document flow state metrics for stakeholders." → DocumentationFlow (2.1)
**Case E:** "Review flow state improvements." → QA sub-phase

---

## B37-0B-042-003 — "work breakdown structure" (WBS) routing — ARCHITECTURE always fires

**"work breakdown structure" → ARCHITECTURE fires on "structure" token (any context):**

**Case A:** "Maintain work breakdown structure." → ArchitectureFlow31 (3.1) | software-development
**Case B:** "Create work breakdown structure." → ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1) — ARCHITECTURE + IMPLEMENT
**Case C:** "Document work breakdown structure for stakeholders." → ArchitectureFlow31 (3.1) + DocumentationFlow (2.1) — ARCHITECTURE + DOCUMENT
**Case D:** "Review work breakdown structure." → ArchitectureFlow31 (3.1) + QA sub-phase — ARCHITECTURE + QA

WBS is the ONLY Round 4 term that ALWAYS fires a predicate (ARCHITECTURE) — even without additional predicate tokens.

---

## B37-0B-042-004 — "project closure" routing cases

**Case A:** "Manage project closure." → knowledge_edge | project-management | SCL
**Case B:** "Document project closure for stakeholders." → DocumentationFlow (2.1)
**Case C:** "Review project closure outcomes." → QA sub-phase
**Case D:** "Build project closure dashboard." → BackendServiceFlow41 (4.1)

---

## B37-0B-042-005 — "benefits realization" + "KPI" routing cases

Benefits realization:
**Case A:** "Track benefits realization." → knowledge_edge | SCL
**Case B:** "Build benefits realization tracker." → BackendServiceFlow41 (4.1)
**Case C:** "Document benefits realization plan for stakeholders." → DocumentationFlow (2.1)

KPI:
**Case D:** "Monitor KPI metrics." → knowledge_edge | SCL
**Case E:** "Build KPI dashboard." → BackendServiceFlow41 (4.1)
**Case F:** "Review KPI performance." → QA sub-phase
