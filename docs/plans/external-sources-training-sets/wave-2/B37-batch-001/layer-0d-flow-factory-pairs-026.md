# Layer 0-D — Flow Factory Pairs (026)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-026-001 — Round 2 factory selection: QA fires → QA sub-phase

"Review the Gantt chart." → ["QA"] → QA sub-phase
"Validate milestone progress." → ["QA"] → QA sub-phase
"Review SLA compliance." → ["QA"] → QA sub-phase

QA sub-phase is the factory-equivalent for QA predicate. Not a named flow — a sub-phase of the current flow.

---

## B37-0D-026-002 — Round 2 factory selection: API_CONTRACT fires → APIContractFlow33

"Map API dependencies between services." → ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
"Resolve API integration dependencies." → ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)

"API dependencies" (with API co-occurrence) → API_CONTRACT. "service" co-occurrence → IMPLEMENT also fires.

---

## B37-0D-026-003 — Round 2: dependency mapping boundary

"Map team dependencies." → [] → knowledge_edge → NO factory
"Map service API dependencies." → ["API_CONTRACT", "IMPLEMENT"] → APIContractFlow33 + BackendServiceFlow41
"Map project dependencies." → [] → knowledge_edge → NO factory
"Resolve API contract dependencies between services." → ["API_CONTRACT", "IMPLEMENT"] → 3.3 + 4.1

Boundary: "dependencies" alone → gap. "dependencies" + API token → API_CONTRACT fires.

---

## B37-0D-026-004 — Round 2 factory map complete

| Input pattern | Predicates | Factory |
|--------------|-----------|---------|
| status report alone | [] | no factory |
| progress update alone | [] | no factory |
| milestone tracking alone | [] | no factory |
| Gantt chart alone | [] | no factory |
| resource allocation alone | [] | no factory |
| capacity planning alone | [] | no factory |
| dependency mapping (no API) | [] | no factory |
| critical path alone | [] | no factory |
| budget tracking alone | [] | no factory |
| vendor management alone | [] | no factory |
| SLA tracking alone | [] | no factory |
| + stakeholder/document/define | [DOCUMENT] | DocumentationFlow (2.1) |
| + design/structure/architecture | [ARCHITECTURE] | ArchitectureFlow31 (3.1) |
| + build/create/service/module | [IMPLEMENT] | BackendServiceFlow41 (4.1) |
| + review/validate | [QA] | QA sub-phase |
| API + dependencies | [API_CONTRACT] | APIContractFlow33 (3.3) |

---

## B37-0D-026-005 — SCL emission rule for factory selection

**SCHEDULE_CAPABILITY_LEARNING emitted → NO factory selected:**
All Round 2 zero-predicate requests → SCL + knowledge_edge + NO factory.

**SCHEDULE_CAPABILITY_LEARNING NOT emitted → factory IS selected:**
Any predicate fires → factory selected based on predicate → SCL NOT emitted.

This rule is identical for all B37 rounds (002-060).
