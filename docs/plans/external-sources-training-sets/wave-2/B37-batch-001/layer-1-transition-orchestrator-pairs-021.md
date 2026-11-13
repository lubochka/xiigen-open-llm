# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-021-001 — API dependencies: api_contract + feature_addition

**Input:** "Map all API dependencies between the payment service and the authentication endpoint."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: api_contract + feature_addition (two predicates)
- Next states: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: none

---

## B37-L1-021-002 — team dependencies: TransitionType unknown

**Input:** "Map all team dependencies for the delivery programme."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: dependencies-team-001, manage-delivery-001, track-001, blockers-001

---

## B37-L1-021-003 — integration dependencies: api_contract + feature_addition

**Input:** "Identify integration dependencies between the notification service and messaging module."

**Compiler output:** ["API_CONTRACT", "IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: api_contract + feature_addition
- Next states: APIContractFlow33 (3.3) + BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Parallel eligible: Yes
- Gap IDs: none

---

## B37-L1-021-004 — cross-workstream dependencies: TransitionType unknown

**Input:** "Resolve cross-workstream dependencies before the delivery milestone."

**Compiler output:** []

**Transition orchestrator:**
- TransitionType: unknown
- Next state: knowledge_edge
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING: emitted
- Gap IDs: dependencies-team-001, work-stream-001, manage-delivery-001

---

## B37-L1-021-005 — "service dependencies" boundary: feature_addition only (not api_contract)

**Input:** "Resolve service dependencies between the auth and payment modules."

**Compiler output:** ["IMPLEMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition (IMPLEMENT from "service" + "modules")
- Next state: BackendServiceFlow41 (4.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-dependencies-team-001

**Orchestrator note:** "service" fires IMPLEMENT. TransitionType is feature_addition. APIContractFlow33 is NOT selected because "service" is NOT an API co-occurrence token. The dependencies rule requires API co-occurrence; "service" doesn't qualify.
