# Layer 0-B — Router Pairs (037)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-037-001 — Round 3 (037-048) routing table opening

**Master routing table for Round 3a vocabulary (037-048):**

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| escalation process alone | [] | knowledge_edge | project-management |
| escalation process + document | [DOCUMENT] | 2.1 | software-development |
| escalation process + stakeholders | [DOCUMENT] | 2.1 | software-development |
| communication plan alone | [] | knowledge_edge | project-management |
| communication plan + document | [DOCUMENT] | 2.1 | software-development |
| project kickoff alone | [] | knowledge_edge | project-management |
| steering committee alone | [] | knowledge_edge | project-management |
| change control board alone | [] | knowledge_edge | project-management |

---

## B37-0B-037-002 — Round 3a routing table continued

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| scope creep alone | [] | knowledge_edge | project-management |
| scope creep + define | [DOCUMENT] | 2.1 | software-development |
| risk mitigation alone | [] | knowledge_edge | project-management |
| risk mitigation + implement | [IMPLEMENT] | 4.1 | software-development |
| quality gate alone | [] | knowledge_edge | project-management |
| quality gate + review | [QA] | QA sub-phase | software-development |
| sign-off process alone | [] | knowledge_edge | project-management |
| handover alone | [] | knowledge_edge | project-management |
| handover document | [DOCUMENT] | 2.1 | software-development |

---

## B37-0B-037-003 — Round 3a routing table: post-mortem row

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| post-mortem alone | [] | knowledge_edge | project-management |
| post-mortem + document | [DOCUMENT] | 2.1 | software-development |
| post-mortem + review | [QA] | QA sub-phase | software-development |
| post-mortem + build | [IMPLEMENT] | 4.1 | software-development |
| PM combo (all above, no predicate token) | [] | knowledge_edge | project-management |

---

## B37-0B-037-004 — SCL emission rule for Round 3a vocabulary

**SCHEDULE_CAPABILITY_LEARNING emitted for all Round 3a zero-predicate requests:**
- escalation process → SCL emitted
- communication plan alone → SCL emitted
- project kickoff → SCL emitted
- steering committee alone → SCL emitted
- change control board alone → SCL emitted
- scope creep alone → SCL emitted
- risk mitigation alone → SCL emitted
- quality gate alone → SCL emitted
- sign-off process alone → SCL emitted
- post-mortem alone → SCL emitted

**SCL NOT emitted when any predicate fires.**

---

## B37-0B-037-005 — Round 3a absent factories confirmation

**All non-existent factories for Round 3a vocabulary:**
- EscalationFlow, EscalationProcessFlow
- CommunicationPlanFlow, CommunicationFlow
- ProjectKickoffFlow, KickoffFlow
- SteeringCommitteeFlow, GovernanceFlow
- ChangeControlFlow, ChangeControlBoardFlow
- ScopeCreepFlow, ScopeManagementFlow
- RiskMitigationFlow, MitigationFlow
- QualityGateFlow, GateFlow
- SignOffFlow, ApprovalFlow
- HandoverFlow, TransitionFlow
- PostMortemFlow, RetrospectiveFlow (not registered)

None of these exist. Any output claiming one = v6-01 corruption.
