# Layer 0-B — Router Pairs (040)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-040-001 — Round 4 (049-060) routing table opening

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| waterfall alone | [] | knowledge_edge | project-management |
| waterfall + design/structure | [ARCHITECTURE] | 3.1 | software-development |
| agile transformation alone | [] | knowledge_edge | project-management |
| agile transformation + define | [DOCUMENT] | 2.1 | software-development |
| SAFe alone | [] | knowledge_edge | project-management |
| SAFe + implement | [IMPLEMENT] | 4.1 | software-development |
| program increment alone | [] | knowledge_edge | project-management |
| program increment + define/criteria | [DOCUMENT] | 2.1 | software-development |

---

## B37-0B-040-002 — Round 4 routing table continued

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| portfolio management alone | [] | knowledge_edge | project-management |
| portfolio management + design/structure | [ARCHITECTURE] | 3.1 | software-development |
| value stream alone | [] | knowledge_edge | project-management |
| value stream + design/architecture | [ARCHITECTURE] | 3.1 | software-development |
| flow state alone | [] | knowledge_edge | project-management |
| flow state + build/service | [IMPLEMENT] | 4.1 | software-development |
| work breakdown structure | [ARCHITECTURE] | 3.1 | software-development |
| WBS abbreviation alone | [] | knowledge_edge | project-management |

---

## B37-0B-040-003 — Round 4 routing table: closure and benefits rows

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| project closure alone | [] | knowledge_edge | project-management |
| project closure + document | [DOCUMENT] | 2.1 | software-development |
| benefits realization alone | [] | knowledge_edge | project-management |
| benefits realization + build | [IMPLEMENT] | 4.1 | software-development |
| KPI monitoring alone | [] | knowledge_edge | project-management |
| KPI dashboard + build | [IMPLEMENT] | 4.1 | software-development |
| KPI + review | [QA] | QA sub-phase | software-development |

---

## B37-0B-040-004 — SCL emission for Round 4 vocabulary

**SCHEDULE_CAPABILITY_LEARNING emitted for all Round 4 zero-predicate requests:**
- waterfall alone → SCL
- agile transformation alone → SCL
- SAFe alone → SCL
- program increment alone → SCL
- portfolio management alone → SCL
- value stream alone → SCL
- flow state alone → SCL
- project closure alone → SCL
- benefits realization alone → SCL
- KPI monitoring alone → SCL
- WBS abbreviation alone → SCL

**SCL NOT emitted:** work breakdown structure (ARCHITECTURE fires on "structure")

---

## B37-0B-040-005 — Round 4 absent factories confirmation

**All non-existent factories for Round 4 vocabulary:**
- WaterfallFlow, WaterfallDeliveryFlow
- AgileTransformationFlow, AgileFlow
- SAFeFlow, ScaledAgileFlow
- ProgramIncrementFlow, PIFlow
- PortfolioManagementFlow, PortfolioFlow
- ValueStreamFlow, ValueStreamMappingFlow
- FlowStateFlow, TeamFlowFlow
- ProjectClosureFlow, ClosureFlow
- BenefitsRealizationFlow, BenefitsFlow
- KPIDashboardFlow, KPIFlow

None exist. Any output claiming one = v6-01 corruption.
