# Layer 0-D — Flow Factory Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: flow-factory-pairs

---

## B37-0D-024-001 — Complete extended B37 factory map (013-024 vocabulary)

**Extended B37 flow factory registry:**

| Vocabulary pattern | Predicates | Factories | Sub-phase |
|-------------------|-----------|---------|---------|
| risk log alone | [] | knowledge_edge | N/A |
| review risk log | [QA] | ReviewFlow | QA |
| document risk log | [DOCUMENT] | DocumentationFlow | 2.1 |
| change request alone | [] | knowledge_edge | N/A |
| document change request | [DOCUMENT] | DocumentationFlow | 2.1 |
| project charter (circulate) | [] | knowledge_edge | N/A |
| create project charter | [IMPLEMENT] | BackendServiceFlow41 | 4.1 |
| decision log alone | [] | knowledge_edge | N/A |
| document architectural decisions | [DOCUMENT, ARCHITECTURE] | DocumentationFlow + ArchitectureFlow31 | 2.1+3.1 |
| lessons-learned session | [] | knowledge_edge | N/A |
| accountability alone | [] | knowledge_edge | N/A |
| define accountability structure | [DOCUMENT, ARCHITECTURE] | DocumentationFlow + ArchitectureFlow31 | 2.1+3.1 |
| team dependencies | [] | knowledge_edge | N/A |
| API dependencies + service | [API_CONTRACT, IMPLEMENT] | APIContractFlow33 + BackendServiceFlow41 | 3.3+4.1 |
| managing + stakeholder | [DOCUMENT] | DocumentationFlow | 2.1 |
| managing + structure | [ARCHITECTURE] | ArchitectureFlow31 | 3.1 |

---

## B37-0D-024-002 — Absent factories: extended list for 013-024 vocabulary

**All non-existent factories for extended B37 vocabulary:**
- RiskLogFlow, RiskTrackingFlow, RiskManagementFlow
- ChangeRequestFlow, ChangeManagementFlow, ChangeControlFlow
- ProjectCharterFlow, ProgrammeGovernanceFlow, ProjectInitiationFlow
- DecisionLogFlow, DecisionManagementFlow
- LessonsLearnedFlow, RetrospectiveFlow, PostImplementationFlow
- AccountabilityFlow, OwnershipFlow, RACIFlow, GovernanceFlow
- StakeholderNotificationFlow, StakeholderUpdateFlow, CommunicationFlow
- TeamDependencyFlow, DependencyManagementFlow

None of these exist. Any output claiming one of these = v6-01 corruption.

---

## B37-0D-024-003 — SCL emission: extended vocabulary confirmation

**SCHEDULE_CAPABILITY_LEARNING emitted for all zero-predicate extended B37 requests:**
- risk log maintenance → SCL emitted
- change request processing → SCL emitted
- project charter circulation → SCL emitted
- decision log updates → SCL emitted
- lessons-learned sessions → SCL emitted
- accountability coordination → SCL emitted
- team dependency resolution → SCL emitted

**SCL NOT emitted when any predicate fires** (even if most vocabulary is gap).

---

## B37-0D-024-004 — Parallel factory eligibility for extended vocabulary

**Parallel eligible factory pairs in extended B37:**

1. [DOCUMENT, ARCHITECTURE] → DocumentationFlow (2.1) + ArchitectureFlow31 (3.1) — parallel eligible
2. [API_CONTRACT, IMPLEMENT] → APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1) — parallel eligible
3. [DOCUMENT, IMPLEMENT] → DocumentationFlow (2.1) + BackendServiceFlow41 (4.1) — parallel eligible
4. [DOCUMENT, ARCHITECTURE, IMPLEMENT] → all three — parallel eligible

**Ordered (not parallel):**
- [QA, DOCUMENT] → QA typically precedes or follows documentation — ordered
- Any combination with TEST → TEST typically last in flow

---

## B37-0D-024-005 — Factory selection is lexically determined, not intent-determined

**Final rule for extended B37 factory selection:**

The flow factory is selected EXCLUSIVELY based on predicates from the lexical scan.

The factory does NOT:
- Consider whether the managing intent would be better served by a different factory
- Override the predicate result based on sentence framing
- Select a "project management" factory that doesn't exist

The factory ALWAYS:
- Selects the predicate-appropriate flow
- Forwards gap warnings alongside the selected flow
- Returns knowledge_edge when zero predicates fire

This principle applies to all B37 vocabulary — base (002-012) and extended (013-024).
