# Layer 0-B — Router Pairs (047)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-047-001 — "flow state" routing stress test

**Five inputs, five routes:**
A: "Monitor flow state." → knowledge_edge | PM | SCL
B: "Define flow state metrics." → DocumentationFlow (2.1) | SD
C: "Design flow state monitoring architecture." → ArchitectureFlow31 (3.1) | SD
D: "Build flow state monitoring service." → BackendServiceFlow41 (4.1) | SD
E: "Review flow state improvements." → QA sub-phase | SD

---

## B37-0B-047-002 — "benefits realization" routing stress test

**Five inputs:**
A: "Track benefits realization." → knowledge_edge | PM | SCL
B: "Document benefits realization for stakeholders." → DocumentationFlow (2.1) | SD
C: "Design benefits realization framework architecture." → ArchitectureFlow31 (3.1) | SD
D: "Build benefits realization tracker service." → BackendServiceFlow41 (4.1) | SD
E: "Review benefits realization outcomes." → QA sub-phase | SD

---

## B37-0B-047-003 — "KPI dashboard" routing stress test

**Five inputs:**
A: "Monitor KPI dashboard." → knowledge_edge | PM | SCL
B: "Document KPI dashboard requirements for stakeholders." → DocumentationFlow (2.1) | SD
C: "Design KPI dashboard architecture." → ArchitectureFlow31 (3.1) | SD
D: "Build KPI dashboard service." → BackendServiceFlow41 (4.1) | SD
E: "Review KPI dashboard performance." → QA sub-phase | SD

---

## B37-0B-047-004 — Parallel routing eligibility for Round 3+4 combos

**Parallel eligible combinations:**
- [DOCUMENT, ARCHITECTURE] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) — parallel eligible
- [DOCUMENT, IMPLEMENT] → DocumentationFlow (2.1) + BackendServiceFlow41 (4.1) — parallel eligible
- [ARCHITECTURE, IMPLEMENT] → ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1) — parallel eligible
- [DOCUMENT, ARCHITECTURE, IMPLEMENT] → all three — parallel eligible

**Ordered:**
- [QA, DOCUMENT] → ordered (QA before or after documentation)
- Any with TEST → TEST last

---

## B37-0B-047-005 — Routing from "create" vs "maintain" on WBS

"Maintain work breakdown structure" → ["ARCHITECTURE"] → ArchitectureFlow31 only
"Create work breakdown structure" → ["IMPLEMENT", "ARCHITECTURE"] → BackendServiceFlow41 + ArchitectureFlow31 (parallel)

Key: "create" fires IMPLEMENT in addition to ARCHITECTURE from "structure". "maintain" does not fire IMPLEMENT.
