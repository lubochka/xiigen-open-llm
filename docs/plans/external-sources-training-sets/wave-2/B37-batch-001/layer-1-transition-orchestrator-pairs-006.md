# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-006-001 — QA in managing context: no_contribution TransitionType

**Input:** "Review the escalation log and follow up with the engineering team on open action items."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: no_contribution (QA in project managing context — review of managing artifacts)
- Note: QA does not contribute a formal TransitionType to the main flow; it routes to a QA sub-phase
- ContextCarryForward: false
- Next state: QA sub-phase (ReviewFlow)
- DomainId: "software-development"

**Gap IDs forwarded:**
- gap-project-managing-escalation-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001

**Orchestrator note:** "no_contribution" indicates that QA fires but does not establish a primary feature/documentation/architecture flow. QA sub-phase handles the review independently.

---

## B37-L1-006-002 — QA: "validate" in managing context

**Input:** "Validate the delivery pace and confirm all blockers are resolved."

**Compiler output:** ["QA"]

**Transition orchestrator:**
- TransitionType: no_contribution (QA sub-phase only)
- Next state: QAGatingFlow (validate context)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-blockers-001

---

## B37-L1-006-003 — QA + DOCUMENT: no_contribution + documentation

**Input:** "Review the escalation criteria and document the approval requirements."

**Compiler output:** ["QA", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: no_contribution (QA) + documentation (DOCUMENT)
- Compound: QA sub-phase + DocumentationFlow
- Next states: ReviewFlow + DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-escalation-001

---

## B37-L1-006-004 — QA + IMPLEMENT: no_contribution + feature_addition

**Input:** "Review the tracking service requirements and build the module."

**Compiler output:** ["QA", "IMPLEMENT", "DOCUMENT"]

Analysis:
- "Review" → QA
- "tracking" → gap (track family)
- "service" → IMPLEMENT
- "requirements" → DOCUMENT
- "build" → IMPLEMENT
- "module" → IMPLEMENT

**Transition orchestrator:**
- TransitionType: no_contribution (QA) + feature_addition (IMPLEMENT) + documentation (DOCUMENT)
- Next states: QAGatingFlow + BackendServiceFlow41 + DocumentationFlow
- DomainId: "software-development"
- Parallel: BackendServiceFlow41 + DocumentationFlow parallel; QA after IMPLEMENT

---

## B37-L1-006-005 — QA TransitionType rules in B37

**QA predicate transition rules:**

| QA context | TransitionType | Next state |
|-----------|--------------|-----------|
| review of code | no_contribution | ReviewFlow |
| review of documents | no_contribution | ReviewFlow |
| review of managing artifacts (logs, RAID) | no_contribution | ReviewFlow |
| validate outputs | no_contribution | QAGatingFlow |
| QA + DOCUMENT | no_contribution + documentation | ReviewFlow + DocumentationFlow |
| QA + IMPLEMENT | no_contribution + feature_addition | QAGatingFlow + BackendServiceFlow41 |

**TransitionType "no_contribution"** means QA does not initiate a primary flow; it gates or reviews the output of other flows or standalone artifacts.
