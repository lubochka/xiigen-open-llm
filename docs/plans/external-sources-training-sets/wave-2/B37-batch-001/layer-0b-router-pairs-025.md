# Layer 0-B — Router Pairs (025)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-025-001 — Round 2 routing table: status report and progress update vocabulary

**Routing reference for Round 2 vocabulary (025-036) — part 1:**

| Input pattern | Predicates | Routes | DomainId |
|--------------|-----------|--------|---------|
| status report maintenance | [] | knowledge_edge | project-management |
| document the status report | [DOCUMENT] | 2.1 | software-development |
| status report + stakeholders | [DOCUMENT] | 2.1 | software-development |
| progress update coordination | [] | knowledge_edge | project-management |
| progress update + stakeholders | [DOCUMENT] | 2.1 | software-development |
| document progress update | [DOCUMENT] | 2.1 | software-development |
| milestone tracking | [] | knowledge_edge | project-management |
| review milestone tracking | [QA] | QA sub-phase | software-development |
| define milestone criteria | [DOCUMENT] | 2.1 | software-development |

---

## B37-0B-025-002 — Round 2 routing table: Gantt chart and resource allocation

**Routing reference for Round 2 vocabulary — part 2:**

| Input pattern | Predicates | Routes | DomainId |
|--------------|-----------|--------|---------|
| maintain Gantt chart | [] | knowledge_edge | project-management |
| create Gantt chart | [IMPLEMENT] | 4.1 | software-development |
| review Gantt chart | [QA] | QA sub-phase | software-development |
| create Gantt chart document | [IMPLEMENT, DOCUMENT] | 4.1 + 2.1 | software-development |
| resource allocation coordination | [] | knowledge_edge | project-management |
| review resource allocation | [QA] | QA sub-phase | software-development |
| design resource allocation structure | [ARCHITECTURE] | 3.1 | software-development |
| implement resource allocation module | [IMPLEMENT] | 4.1 | software-development |

---

## B37-0B-025-003 — Round 2 routing table: capacity planning and dependency mapping

| Input pattern | Predicates | Routes | DomainId |
|--------------|-----------|--------|---------|
| capacity planning session | [] | knowledge_edge | project-management |
| define capacity planning structure | [DOCUMENT, ARCHITECTURE] | 2.1 + 3.1 | software-development |
| design capacity planning structure | [ARCHITECTURE] | 3.1 | software-development |
| dependency mapping (no API) | [] | knowledge_edge | project-management |
| dependency mapping + API co-occurrence | [API_CONTRACT] | 3.3 | software-development |
| dependency mapping + API + service | [API_CONTRACT, IMPLEMENT] | 3.3 + 4.1 | software-development |
| design dependency map structure | [ARCHITECTURE] | 3.1 | software-development |

---

## B37-0B-025-004 — Round 2 routing table: critical path, budget, vendor, SLA

| Input pattern | Predicates | Routes | DomainId |
|--------------|-----------|--------|---------|
| critical path analysis | [] | knowledge_edge | project-management |
| define critical path criteria | [DOCUMENT] | 2.1 | software-development |
| document critical path + acceptance | [DOCUMENT, TEST] | 2.1 + TEST | software-development |
| budget tracking | [] | knowledge_edge | project-management |
| define budget tracking criteria | [DOCUMENT] | 2.1 | software-development |
| build budget tracking service | [IMPLEMENT] | 4.1 | software-development |
| vendor management | [] | knowledge_edge | project-management |
| review vendor performance | [QA] | QA sub-phase | software-development |
| SLA tracking | [] | knowledge_edge | project-management |
| implement SLA tracking service | [IMPLEMENT] | 4.1 | software-development |

---

## B37-0B-025-005 — SCL emission rule for Round 2 vocabulary

**SCHEDULE_CAPABILITY_LEARNING emitted for all zero-predicate Round 2 requests:**
- status report maintenance → SCL emitted
- progress update coordination → SCL emitted
- milestone tracking → SCL emitted
- Gantt chart maintenance → SCL emitted
- resource allocation coordination → SCL emitted
- capacity planning sessions → SCL emitted
- dependency mapping (no API) → SCL emitted
- critical path analysis → SCL emitted
- budget tracking → SCL emitted
- vendor management → SCL emitted
- SLA tracking → SCL emitted

**SCL NOT emitted** when any predicate fires (review, define, document, stakeholder, structure, design, create, implement, build, write, service, module, API + dependencies).
