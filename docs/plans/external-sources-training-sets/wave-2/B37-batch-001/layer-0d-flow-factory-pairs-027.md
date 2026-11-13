# Layer 0-D — Flow Factory Pairs (027)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-027-001 — Round 3a (037-048) factory map

**Round 3a factory registry:**

| Input pattern | Predicates | Factory |
|--------------|-----------|---------|
| escalation process alone | [] | no factory |
| communication plan alone | [] | no factory |
| project kickoff alone | [] | no factory |
| steering committee alone | [] | no factory |
| change control board alone | [] | no factory |
| scope creep alone | [] | no factory |
| risk mitigation alone | [] | no factory |
| quality gate alone | [] | no factory |
| sign-off process alone | [] | no factory |
| handover alone | [] | no factory |
| post-mortem alone | [] | no factory |
| + document/stakeholder/define | [DOCUMENT] | DocumentationFlow (2.1) |
| + design/structure/architecture | [ARCHITECTURE] | ArchitectureFlow31 (3.1) |
| + build/create/service/module | [IMPLEMENT] | BackendServiceFlow41 (4.1) |
| + review/validate | [QA] | QA sub-phase |
| handover document | [DOCUMENT] | DocumentationFlow (2.1) |

---

## B37-0D-027-002 — Round 4b (049-060) factory map

**Round 4b factory registry:**

| Input pattern | Predicates | Factory |
|--------------|-----------|---------|
| waterfall alone | [] | no factory |
| agile transformation alone | [] | no factory |
| SAFe alone | [] | no factory |
| program increment alone | [] | no factory |
| portfolio management alone | [] | no factory |
| value stream alone | [] | no factory |
| flow state alone | [] | no factory |
| WBS abbreviation alone | [] | no factory |
| project closure alone | [] | no factory |
| benefits realization alone | [] | no factory |
| KPI monitoring alone | [] | no factory |
| work breakdown structure | [ARCHITECTURE] | ArchitectureFlow31 (3.1) |
| + document/stakeholder | [DOCUMENT] | DocumentationFlow (2.1) |
| + build/service | [IMPLEMENT] | BackendServiceFlow41 (4.1) |

---

## B37-0D-027-003 — Factory selection is lexically determined

**Rule confirmed for all rounds (002-060):**

Factory selection depends ONLY on predicates from lexical scan.
The factory does NOT:
- Consider PM managing intent
- Override predicate result based on sentence context
- Select a "PM-specific" factory (none exist)

The factory ALWAYS:
- Selects based on predicate → predicate-appropriate factory
- Forwards gap warnings alongside
- Returns knowledge_edge (no factory) when zero predicates

---

## B37-0D-027-004 — Complete absent factory list (all rounds 002-060)

PM-specific factories that do NOT exist:
EscalationFlow, RiskMitigationFlow, ScopeCreepFlow, QualityGateFlow, SignOffFlow, PostMortemFlow, CommunicationPlanFlow, ProjectKickoffFlow, SteeringCommitteeFlow, ChangeControlBoardFlow, WaterfallFlow, AgileTransformationFlow, SAFeFlow, ProgramIncrementFlow, PortfolioManagementFlow, ValueStreamFlow, FlowStateFlow, ProjectClosureFlow, BenefitsRealizationFlow, KPIFlow, GanttChartFlow, MilestoneFlow, ResourceAllocationFlow, CapacityPlanningFlow, CriticalPathFlow, BudgetTrackingFlow, VendorManagementFlow, SLATrackingFlow

No PM vocabulary has a dedicated factory.

---

## B37-0D-027-005 — Parallel factory eligibility (all rounds)

**Parallel eligible:**
[DOCUMENT, ARCHITECTURE] → 2.1 + 3.1
[DOCUMENT, IMPLEMENT] → 2.1 + 4.1
[ARCHITECTURE, IMPLEMENT] → 3.1 + 4.1
[API_CONTRACT, IMPLEMENT] → 3.3 + 4.1
[DOCUMENT, ARCHITECTURE, IMPLEMENT] → 2.1 + 3.1 + 4.1

**Ordered:**
[QA, DOCUMENT] → ordered
[QA, ARCHITECTURE] → ordered
Any combination with TEST → TEST typically last
