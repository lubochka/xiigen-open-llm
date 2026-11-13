# Layer 0-B — Router Pairs (060)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-060-001 — Complete B37 router master reference (final)

**Routing map — all B37 predicates:**

| Predicates | Routes | DomainId |
|-----------|--------|---------|
| [] | knowledge_edge | project-management |
| [DOCUMENT] | DocumentationFlow (2.1) | software-development |
| [ARCHITECTURE] | ArchitectureFlow31 (3.1) | software-development |
| [IMPLEMENT] | BackendServiceFlow41 (4.1) | software-development |
| [QA] | QA sub-phase | software-development |
| [API_CONTRACT] | APIContractFlow33 (3.3) | software-development |
| [TEST] | TestFlow (if registered) | software-development |
| [DOCUMENT, ARCHITECTURE] | 2.1 + 3.1 | software-development |
| [DOCUMENT, IMPLEMENT] | 2.1 + 4.1 | software-development |
| [ARCHITECTURE, IMPLEMENT] | 3.1 + 4.1 | software-development |
| [API_CONTRACT, IMPLEMENT] | 3.3 + 4.1 | software-development |
| [DOCUMENT, ARCHITECTURE, IMPLEMENT] | 2.1 + 3.1 + 4.1 | software-development |

---

## B37-0B-060-002 — Complete absent factory list (all rounds 002-060)

**Factories that do NOT exist (v6-01 triggers if claimed):**

Round 1-2 absent: RiskLogFlow, ChangeRequestFlow, LessonsLearnedFlow, AccountabilityFlow, StakeholderUpdateFlow, TeamDependencyFlow, StatusReportFlow, MilestoneTrackingFlow, RAIDLogFlow
Round 3a absent: EscalationFlow, CommunicationPlanFlow, ProjectKickoffFlow, SteeringCommitteeFlow, ChangeControlBoardFlow, ScopeCreepFlow, RiskMitigationFlow, QualityGateFlow, SignOffFlow, PostMortemFlow
Round 4b absent: WaterfallFlow, AgileTransformationFlow, SAFeFlow, ProgramIncrementFlow, PortfolioManagementFlow, ValueStreamFlow, FlowStateFlow, ProjectClosureFlow, BenefitsRealizationFlow, KPIFlow

**Factories that DO exist:** DocumentationFlow (2.1), ArchitectureFlow31 (3.1), APIContractFlow33 (3.3), BackendServiceFlow41 (4.1)

---

## B37-0B-060-003 — B37 router certification exam: final 10 questions

Q1: "Coordinate change control board and steering committee." → knowledge_edge
Q2: "Design change control board governance structure." → ArchitectureFlow31 (3.1)
Q3: "Manage waterfall project closure and post-mortem." → knowledge_edge
Q4: "Build waterfall closure KPI dashboard." → BackendServiceFlow41 (4.1)
Q5: "Review agile transformation benefits and SAFe adoption." → QA sub-phase
Q6: "Document value stream flows for stakeholders." → DocumentationFlow (2.1)
Q7: "Maintain work breakdown structure." → ArchitectureFlow31 (3.1)
Q8: "Monitor flow state and benefits realization." → knowledge_edge
Q9: "Map portfolio API dependencies between services." → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
Q10: "Create work breakdown structure and document it for stakeholders." → ArchitectureFlow31 (3.1) + BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)

---

## B37-0B-060-004 — DomainId binary switch: final confirmation

**Confirmed across all 59 router files (002-060):**
- Zero predicates → ALWAYS "project-management"
- Any predicate fires → ALWAYS "software-development"
- No gradation, no exception, no intent-based override

---

## B37-0B-060-005 — B37 router layer complete

Layer 0-B router training complete (002-060).
59 router files covering all B37 vocabulary rounds.
Core routing principle: predicate-driven, not intent-driven.
No PM managing vocabulary determines routing.
All false positive rules consistent across all rounds.
