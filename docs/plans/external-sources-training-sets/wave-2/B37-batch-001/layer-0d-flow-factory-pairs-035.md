# Layer 0-D — Flow Factory Pairs (035)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-035-001 — Factory: complete B37 factory registry (only four real factories)

**Registered factories:**
1. DocumentationFlow — Phase 2.1 — triggered by: DOCUMENT predicate
2. ArchitectureFlow31 — Phase 3.1 — triggered by: ARCHITECTURE predicate
3. APIContractFlow33 — Phase 3.3 — triggered by: API_CONTRACT predicate
4. BackendServiceFlow41 — Phase 4.1 — triggered by: IMPLEMENT predicate
5. QA sub-phase — triggered by: QA predicate (not a named flow, a sub-phase)

All other flow names = v6-01 corruption.

---

## B37-0D-035-002 — Factory: "project closure" + "benefits realization" + "KPI" combined

A: "Run project closure, track benefits realization, monitor KPI." → [] → no factory
B: "Document project closure, benefits, and KPIs for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build project closure KPI dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
D: "Design project closure and benefits architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)

---

## B37-0D-035-003 — Factory: "agile transformation" + "SAFe" + "portfolio" combined

A: "Manage agile transformation, SAFe, and portfolio." → [] → no factory
B: "Document agile, SAFe, and portfolio for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Design agile, SAFe, and portfolio governance structure." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
D: "Build agile, SAFe, and portfolio tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-035-004 — Factory: "waterfall" + "project closure" + "post-mortem"

A: "Manage waterfall project closure and run post-mortem." → [] → no factory
B: "Document waterfall closure and post-mortem for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Review waterfall closure and post-mortem." → [QA] → QA sub-phase
D: "Build waterfall closure tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-035-005 — Factory: "value stream" + "flow state" + "program increment"

A: "Map value streams, monitor flow state, run PI planning." → [] → no factory
B: "Document value stream, flow state, and PI metrics for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
C: "Build value stream, flow state, and PI tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
D: "Design value stream and PI architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
