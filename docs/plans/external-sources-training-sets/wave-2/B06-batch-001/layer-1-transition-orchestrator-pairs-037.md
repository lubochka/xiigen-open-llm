# Layer 1 — Transition Orchestrator Pairs (037)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B06-T1-037-001 — CustomerSegmentation: DOCUMENT → documentation

```
source_id: xiigen-b06-t1-037-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Define the CustomerSegmentation requirements."
  PredicateIds: ["DOCUMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 1
  TransitionType: "documentation"

---

## B06-T1-037-002 — CustomerSegmentation: DOCUMENT + IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-037-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Write the CustomerSegmentation specification and build the corresponding service."
  PredicateIds: ["DOCUMENT", "IMPLEMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    2. SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: "feature_addition" (IMPLEMENT present)
  CanonicalOrdering: 2.1 → 4.1

---

## B06-T1-037-003 — CustomerSegmentation: DOCUMENT + ARCHITECTURE → documentation

```
source_id: xiigen-b06-t1-037-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Describe the CustomerSegmentation requirements and design the service architecture."
  PredicateIds: ["DOCUMENT", "ARCHITECTURE"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "documentation"; DomainId: "software-development"
    2. SubPhaseId: "3.1"; TransitionType: "documentation"; DomainId: "software-development"
  TransitionCount: 2
  TransitionType: "documentation" (ARCHITECTURE does not upgrade to feature_addition)
  CanonicalOrdering: 2.1 → 3.1

---

## B06-T1-037-004 — CustomerSegmentation: DOCUMENT + ARCHITECTURE + IMPLEMENT → feature_addition

```
source_id: xiigen-b06-t1-037-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  UserRequest: "Write the CustomerSegmentation requirements, design the architecture, and implement the service."
  PredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

B:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    2. SubPhaseId: "3.1"; TransitionType: "feature_addition"; DomainId: "software-development"
    3. SubPhaseId: "4.1"; TransitionType: "feature_addition"; DomainId: "software-development"
  TransitionCount: 3
  TransitionType: "feature_addition" (IMPLEMENT overrides all transitions unconditionally)
  CanonicalOrdering: 2.1 → 3.1 → 4.1
