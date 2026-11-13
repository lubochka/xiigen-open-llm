# Layer 1 — Transition Orchestrator Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B17B1-WDA-T1-001 — feature_addition: integration build

```
source_id: xiigen-b17-t1-001
transition_type: feature_addition
phase: 4
sub_phase: 4.3
pair_kind: transition_orchestrator
```

A:
  user_request: "Build the Stripe payment integration connector — implement the charge, refund, and webhook handlers."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  PredictedSubPhaseId: "4.3"

B:
  TransitionId: "transition-B17-t1-001-4.3-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.3"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "IMPLEMENT (build, implement) + API_CONTRACT (connector, webhook) → 4.3 integration; new connector being created = feature_addition"
    primary_intent: "create"
    domain_signal: "external_service_integration"

---

## Pair B17B1-WDA-T1-002 — feature_addition: OAuth connector

```
source_id: xiigen-b17-t1-002
transition_type: feature_addition
phase: 4
sub_phase: 4.3
pair_kind: transition_orchestrator
```

A:
  user_request: "Build the OAuth 2.0 connector for the third-party identity provider and implement the token exchange flow."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
  PredictedSubPhaseId: "4.3"

B:
  TransitionId: "transition-B17-t1-002-4.3-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.3"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "IMPLEMENT (build, implement) dominant for 4.3; API_CONTRACT (connector) confirms integration context; SECURITY (token) and AUTHORIZATION (oauth) route to additional sub-phases (3.4)"
    primary_intent: "create"
    domain_signal: "external_service_integration"
