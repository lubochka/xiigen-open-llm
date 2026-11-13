# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-004-001 — IMPLEMENT in managing context: feature_addition TransitionType

**Input:** "Manage the delivery of the authentication service and build the access control module."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition (IMPLEMENT predicate → feature/service addition)
- ContextCarryForward: false
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs forwarded: gap-project-managing-manage-delivery-001

**Orchestrator note:** TransitionType "feature_addition" is set because IMPLEMENT fired. The "manage the delivery" framing is managing vocabulary that generates a gap warning. The transition type reflects what the predicate indicates (feature/service build), not the managing framing.

---

## B37-L1-004-002 — IMPLEMENT: "create module" → feature_addition

**Input:** "Create the project status tracking module for the delivery portal."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition
- Next state: BackendServiceFlow41
- DomainId: "software-development"
- Gap IDs: gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001

---

## B37-L1-004-003 — IMPLEMENT + DOCUMENT: feature_addition + documentation, parallel eligible

**Input:** "Build the escalation tracking service and document the API criteria."

**Compiler output:** ["IMPLEMENT", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition + documentation (parallel)
- Next states: BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes (independent flows)
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-track-status-001

---

## B37-L1-004-004 — IMPLEMENT + ARCHITECTURE: feature_addition + architecture_design

**Input:** "Build the service module following the governance structure design."

**Compiler output:** ["IMPLEMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: feature_addition + architecture_design (parallel)
- Next states: BackendServiceFlow41 (4.1) + ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Parallel eligible: Yes (architecture may inform build, but both can proceed)
- Gap IDs: gap-project-managing-accountability-001 (governance)

---

## B37-L1-004-005 — feature_addition TransitionType rules

**TransitionType = "feature_addition" when:**
- IMPLEMENT predicate fired
- Trigger tokens: build, implement, create, write (code), service, module
- Prior context: any
- Next state: BackendServiceFlow41

**feature_addition + documentation (parallel) when:**
- IMPLEMENT + DOCUMENT both fired
- Next states: BackendServiceFlow41 + DocumentationFlow in parallel

**feature_addition + architecture_design when:**
- IMPLEMENT + ARCHITECTURE both fired
- Often architecture is defined first, then build follows

**DomainId invariant:** Always "software-development" when feature_addition TransitionType
