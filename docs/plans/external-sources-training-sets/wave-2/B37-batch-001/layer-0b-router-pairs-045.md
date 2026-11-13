# Layer 0-B — Router Pairs (045)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-045-001 — "waterfall" + "agile transformation" combined routing

**Case A:** "Transition from waterfall to agile transformation." → knowledge_edge | PM | SCL
**Case B:** "Define the waterfall-to-agile transformation roadmap." → DocumentationFlow (2.1) | SD
**Case C:** "Design the waterfall-to-agile transformation architecture." → ArchitectureFlow31 (3.1) | SD
**Case D:** "Build the waterfall tracking and agile transformation platform." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-045-002 — "SAFe" + "program increment" combined routing

**Case A:** "Coordinate SAFe program increment ceremonies." → knowledge_edge | PM | SCL
**Case B:** "Define SAFe PI governance criteria." → DocumentationFlow (2.1) | SD
**Case C:** "Design SAFe PI governance structure." → ArchitectureFlow31 (3.1) | SD
**Case D:** "Implement SAFe PI tracking service." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-045-003 — "portfolio management" + "value stream" combined routing

**Case A:** "Manage portfolio and map value streams." → knowledge_edge | PM | SCL
**Case B:** "Document portfolio and value stream metrics for stakeholders." → DocumentationFlow (2.1) | SD
**Case C:** "Design portfolio and value stream architecture." → ArchitectureFlow31 (3.1) | SD
**Case D:** "Build portfolio and value stream dashboard service." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-045-004 — "project closure" + "benefits realization" + "KPI" combined routing

**Case A:** "Manage project closure, track benefits realization, monitor KPI." → knowledge_edge | PM | SCL
**Case B:** "Document project closure and benefits realization KPIs for stakeholders." → DocumentationFlow (2.1) | SD
**Case C:** "Build project closure and KPI dashboard service." → BackendServiceFlow41 (4.1) | SD

---

## B37-0B-045-005 — "work breakdown structure" routing in all contexts

"work breakdown structure" ALWAYS fires ARCHITECTURE on "structure" token:

**Case A:** "Create WBS" (abbreviation) → knowledge_edge (WBS abbreviation = gap)
**Case B:** "Create work breakdown structure." → ArchitectureFlow31 + BackendServiceFlow41 (ARCHITECTURE + IMPLEMENT)
**Case C:** "Maintain work breakdown structure." → ArchitectureFlow31 (ARCHITECTURE only)
**Case D:** "Document work breakdown structure for stakeholders." → ArchitectureFlow31 + DocumentationFlow (ARCHITECTURE + DOCUMENT)
**Case E:** "Review work breakdown structure." → ArchitectureFlow31 + QA sub-phase (ARCHITECTURE + QA)
