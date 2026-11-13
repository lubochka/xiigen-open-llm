# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 016 covers ReturnAndRefund transition orchestration — four pairs.

---

## B06-T1-016-001 — ReturnAndRefund: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-016-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}]

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1

---

## B06-T1-016-002 — ReturnAndRefund: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-016-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "4.1"}]

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2; Ordering: 2.1 → 4.1

---

## B06-T1-016-003 — ReturnAndRefund: DOCUMENT+ARCHITECTURE → documentation

```
source_id: xiigen-b06-t1-016-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}, {PredictedSubPhaseId: "3.1"}]

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    - SubPhaseId: "3.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: documentation (ARCHITECTURE alone does not trigger feature_addition)
  Ordering: 2.1 before 3.1

---

## B06-T1-016-004 — ReturnAndRefund: transition ordering canonical reference

```
source_id: xiigen-b06-t1-016-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

Canonical transition ordering (repeated from 012 for new domain reference):

  When multiple sub-phases are active, canonical order is:
    1. 2.1 (Business Requirements) — always first
    2. 3.1 (Architecture) — after requirements
    3. 2.4 (Stakeholder Requirements) — after 2.1
    4. 1.1 (Screen/UX) — after architecture
    5. 4.1 (Implementation) — last

  Examples for return/refund domain:
    [2.1, 4.1] → 2.1 before 4.1
    [2.1, 3.1] → 2.1 before 3.1
    [2.1, 2.4] → 2.1 before 2.4
    [2.1, 3.1, 4.1] → 2.1, 3.1, 4.1
