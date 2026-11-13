# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 021 covers FraudDetection transition orchestration — four pairs.

---

## B06-T1-021-001 — FraudDetection: DOCUMENT alone → documentation

```
source_id: xiigen-b06-t1-021-001
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

## B06-T1-021-002 — FraudDetection: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-021-002
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

## B06-T1-021-003 — FraudDetection: DataPrivacy DOCUMENT+SECURITY scenario

```
source_id: xiigen-b06-t1-021-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions: [{PredictedSubPhaseId: "2.1"}]
  Context: "DataPrivacyRequirements — privacy and security framed as requirements"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1

Note: "Security" and "privacy" as subject matter areas do not add predicates.
  Only DOCUMENT terms in the predicate lexicon determine routing.
  The nine-node flow handles security-specific details via N02 DomainTranslatorUnit.

---

## B06-T1-021-004 — FraudDetection: orchestration ordering for 3-predicate scenario

```
source_id: xiigen-b06-t1-021-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "3.1"
    - PredictedSubPhaseId: "4.1"

B:
  Transitions:
    - SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "3.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    - SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 3
  TransitionOrdering: 2.1 → 3.1 → 4.1 (requirements → architecture → implementation)
  TransitionType: feature_addition (IMPLEMENT present — applies to all three)
