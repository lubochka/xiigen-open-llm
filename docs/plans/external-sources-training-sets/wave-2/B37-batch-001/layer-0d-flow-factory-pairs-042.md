# Layer 0-D — Flow Factory Pairs (042)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-042-001 — Factory: "steering committee" + "CCB" + "programme board" combined

A: "Coordinate steering committee, CCB, and programme board." → [] → no factory
B: "Escalate to steering committee, CCB, and programme board." → [] → no factory
C: "Escalate to steering committee, CCB, and stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)
D: "Build steering committee and CCB reporting service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-042-002 — Factory: "portfolio" + "SAFe" + "PI" + "value stream"

A: "Manage portfolio, coordinate SAFe, run PI, map value streams." → [] → no factory
B: "Design portfolio and SAFe PI value stream architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
C: "Document portfolio, SAFe, PI, and value stream for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)

---

## B37-0D-042-003 — Factory: "waterfall" + "agile" + "SAFe" + "PI" full methodology chain

A: "Transition waterfall to agile SAFe with PI planning." → [] → no factory
B: "Design waterfall-to-SAFe agile PI architecture." → [ARCHITECTURE] → ArchitectureFlow31 (3.1)
C: "Build waterfall-to-agile SAFe PI tracking service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)

---

## B37-0D-042-004 — Factory: "project closure" + "KPI" + "flow state" + "benefits"

A: "Manage project closure, monitor KPI, track flow state, manage benefits." → [] → no factory
B: "Build project closure KPI and benefits dashboard service." → [IMPLEMENT] → BackendServiceFlow41 (4.1)
C: "Document project closure KPI and benefits for stakeholders." → [DOCUMENT] → DocumentationFlow (2.1)

---

## B37-0D-042-005 — Factory: final rule statement (Layer 0-D)

**The factory selection rule for B37 (all rounds 002-060):**

Factory = f(Predicates)
- [] → no factory (knowledge_edge)
- [DOCUMENT] → DocumentationFlow (2.1)
- [ARCHITECTURE] → ArchitectureFlow31 (3.1)
- [IMPLEMENT] → BackendServiceFlow41 (4.1)
- [QA] → QA sub-phase
- [API_CONTRACT] → APIContractFlow33 (3.3)
- Multiple → all corresponding factories (parallel eligible)

The PM managing vocabulary is input to gap ID extraction only. It never influences factory selection.
