# Layer 0-D — Flow Factory Pairs (038)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-038-001 — Factory: "waterfall" + "agile transformation" combined

A: "Transition from waterfall to agile transformation." → [] → no factory
B: "Document waterfall-to-agile transition for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design waterfall-to-agile transformation architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build waterfall and agile tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-038-002 — Factory: "SAFe" + "program increment" combined

A: "Coordinate SAFe PI ceremonies." → [] → no factory
B: "Define SAFe PI acceptance criteria." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design SAFe PI governance structure." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build SAFe PI tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-038-003 — Factory: "portfolio management" + "value stream" combined

A: "Manage portfolio and map value streams." → [] → no factory
B: "Document portfolio and value stream KPIs for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design portfolio and value stream architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build portfolio and value stream dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-038-004 — Factory: "flow state" + "KPI" + "benefits realization" combined

A: "Monitor flow state, track KPI, manage benefits realization." → [] → no factory
B: "Build flow state KPI and benefits dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
C: "Document flow state, KPI, and benefits for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)

---

## B37-0D-038-005 — Factory: "project closure" + "post-mortem" + "benefits realization" combined

A: "Run project closure, post-mortem, track benefits realization." → [] → no factory
B: "Document project closure, post-mortem, and benefits for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build project closure and benefits tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
D: "Review project closure and post-mortem outcomes." → [QA] → QA sub-phase
