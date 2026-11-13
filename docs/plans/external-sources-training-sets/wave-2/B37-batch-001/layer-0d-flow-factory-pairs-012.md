# Layer 0-D — Flow Factory Pairs (012)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-012-001 — Complete B37 factory map: summary reference

**B37 flow factory registry (all relevant factories):**

| Factory | Trigger predicate | Sub-phase | Notes |
|---------|-----------------|---------|-------|
| DocumentationFlow | DOCUMENT | 2.1 | Fires on stakeholder, define, document, criteria, requirements |
| ArchitectureFlow31 | ARCHITECTURE | 3.1 | Fires on structure, architecture, design |
| APIContractFlow33 | API_CONTRACT | 3.3 | Fires on dependencies + API co-occurrence |
| BackendServiceFlow41 | IMPLEMENT | 4.1 | Fires on build, service, module, create, implement, write |
| TestFlow5x | TEST | 5.x | Fires on test, verify, assert, coverage, acceptance, unit, regression, e2e |
| ReviewFlow | QA (review) | QA sub-phase | Fires on review |
| QAGatingFlow | QA (validate) | QA sub-phase | Fires on validate |
| knowledge_edge | [] | N/A | Zero predicates; B37 managing vocabulary |

---

## B37-0D-012-002 — B37 factory absence list

**Factories that do NOT exist (cannot be selected):**

- ManagementFlow — does not exist
- CoordinationFlow — does not exist
- TrackingFlow — does not exist
- EscalationFlow — does not exist
- BlockerResolutionFlow — does not exist
- StatusReportingFlow — does not exist
- RAGStatusFlow — does not exist
- RAIDLogFlow — does not exist
- ProjectManagingFlow — does not exist
- StakeholderNotificationFlow — does not exist
- ActionItemsFlow — does not exist
- FollowUpFlow — does not exist
- DeliveryManagementFlow — does not exist

Any factory name from this list appearing in model output = v6-01 corruption.

---

## B37-0D-012-003 — Factory selection completeness

**A complete factory selection output for B37 includes:**

1. Predicate list (or empty list)
2. Factory name(s) (or "None — knowledge_edge")
3. Sub-phase(s) (or "N/A — knowledge_edge")
4. DomainId ("project-management" or "software-development")
5. Parallel eligibility (Yes/No/Ordered)
6. Gap warnings list (if any gap vocabulary present)
7. SCHEDULE_CAPABILITY_LEARNING (emitted/not emitted)

---

## B37-0D-012-004 — Factory output for the five canonical B37 patterns

**Pattern 1: Pure managing (most common)**
Input: "Track delivery blockers and coordinate escalations."
Factory output: None, knowledge_edge, DomainId "project-management", SCL emitted

**Pattern 2: Stakeholder mention**
Input: "Update stakeholders on current status."
Factory output: DocumentationFlow, 2.1, DomainId "software-development"

**Pattern 3: Governance structure**
Input: "Define the accountability structure."
Factory output: ArchitectureFlow31 + DocumentationFlow, 3.1+2.1, DomainId "software-development"

**Pattern 4: Build in managing sentence**
Input: "Build the escalation tracking module."
Factory output: BackendServiceFlow41, 4.1, DomainId "software-development"

**Pattern 5: Review in managing sentence**
Input: "Review the RAID log entries."
Factory output: ReviewFlow, QA sub-phase, DomainId "software-development"

---

## B37-0D-012-005 — Factory summary: B37 is fundamentally knowledge_edge

The dominant output for B37 is knowledge_edge with no factory selected.

This reflects the capability gap design of B37: the system currently has no registered predicates for project managing vocabulary. Every pure managing request routes to knowledge_edge and emits SCHEDULE_CAPABILITY_LEARNING.

Factories are only invoked when managing sentences accidentally contain predicate-eligible tokens (stakeholder, structure, review, test, build, etc.). This is by design — these are false positives from the managing perspective, correctly handled as predicate fires with gap warnings.
