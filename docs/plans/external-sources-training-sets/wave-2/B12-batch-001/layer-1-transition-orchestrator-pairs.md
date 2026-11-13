# Layer 1 — Transition Orchestrator Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B12B1-WDA-T1-001 — feature_addition: API contract documentation

```
source_id: xiigen-b12-t1-001
transition_type: feature_addition
phase: 3
sub_phase: 3.3
pair_kind: transition_orchestrator
```

A:
  run_id: "B12-t1-001"
  user_request: "Document the REST API endpoints for the order service including request/response schemas and authentication requirements."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  PredictedSubPhaseId: "3.3"

B:
  TransitionId: "transition-B12-t1-001-3.3-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "3"
  SubPhaseId: "3.3"
  DomainId: "software-development"
  PredicateIds: ["API_CONTRACT", "DOCUMENT", "SECURITY"]
  ContextCarryForward: true
  EvidenceIds: ["B12-compiler-001-prompt"]
  Metadata:
    transition_classification_basis: "API_CONTRACT primary (REST, API, endpoint, schema) → 3.3; feature_addition because new documentation being created"
    primary_intent: "create"
    domain_signal: "api_contract_design"

---

## Pair B12B1-WDA-T1-002 — documentation (not feature_addition): existing API doc update

```
source_id: xiigen-b12-t1-002
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: transition_orchestrator
```

A:
  run_id: "B12-t1-002"
  user_request: "Update the OpenAPI specification to reflect the new pagination parameters added to the search endpoint."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  PredictedSubPhaseId: "3.3"

B:
  TransitionId: "transition-B12-t1-002-3.3-documentation"
  TransitionType: "documentation"
  PhaseId: "3"
  SubPhaseId: "3.3"
  DomainId: "software-development"
  PredicateIds: ["API_CONTRACT", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B12-t1-002-prompt"]
  Metadata:
    transition_classification_basis: "API_CONTRACT + DOCUMENT; 'update' on an existing spec classifies as documentation (existing artifact updated, not new feature created)"
    primary_intent: "update"
    domain_signal: "api_contract_maintenance"

Teaching: TransitionType "documentation" applies when updating an existing contract/spec,
not when creating new functionality. "feature_addition" applies when creating new API
contracts or new endpoints. "update the OpenAPI spec" → documentation.
"Write the OpenAPI spec" → feature_addition.
