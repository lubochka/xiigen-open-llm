# Layer 0-D — Flow Factory Pairs (031)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-031-001 — Factory: "value stream" cases

A: "Map value streams." → [] → no factory
B: "Document value stream flows for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design value stream architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build value stream visualisation service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review value stream performance." → [QA] → QA sub-phase

---

## B37-0D-031-002 — Factory: "flow state" cases

A: "Monitor team flow state." → [] → no factory
B: "Document flow state metrics for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design flow state monitoring architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build flow state monitoring service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review flow state improvements." → [QA] → QA sub-phase

---

## B37-0D-031-003 — Factory: "work breakdown structure" always → ArchitectureFlow31

A: "Maintain work breakdown structure." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
B: "Create work breakdown structure." → [IMPLEMENT, ARCHITECTURE] → BackendServiceFlow41 (4.1) + ArchitectureFlow31 (3.1)
C: "Document work breakdown structure for stakeholders." → [DOCUMENT, ARCHITECTURE] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
D: "Review work breakdown structure." → [QA, ARCHITECTURE] → QA sub-phase + ArchitectureFlow31 (3.1)
E: "Maintain WBS." → [] → no factory (abbreviation = gap)

---

## B37-0D-031-004 — Factory: "project closure" cases

A: "Manage project closure." → [] → no factory
B: "Document project closure for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design project closure structure." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build project closure dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
E: "Review project closure outcomes." → [QA] → QA sub-phase

---

## B37-0D-031-005 — Factory: "benefits realization" + "KPI" cases

Benefits realization:
A: "Track benefits realization." → [] → no factory
B: "Document benefits for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build benefits tracker service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

KPI:
D: "Monitor KPI metrics." → [] → no factory
E: "Build KPI dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
F: "Design KPI dashboard architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
