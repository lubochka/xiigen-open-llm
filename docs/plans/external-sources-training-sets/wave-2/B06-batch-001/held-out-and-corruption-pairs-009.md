# Held-Out and Corruption Pairs (009)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 009 covers HO-018 (DomainId always software-development, Rule 17), COR-011 (v6-03: wrong DomainId).

---

## Held-Out Pair B06B1-WDA-HO-018 — DomainId always software-development (Rule 17)

```
source_id: xiigen-b06-held-out-018
pair_kind: held_out
```

A:
  run_id: "B06-ho-018"
  user_request: "Define the checkout flow business requirements and stakeholder expectations."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["define", "business", "stakeholder", "requirements"]
  Confidence: 0.92
  Warnings: []

Expected transition:
  TransitionId: "transition-B06-ho-018"
  TransitionType: "documentation"
  DomainId: "software-development"
  SubPhaseId: "2.1"
  MetaData:
    SelectedFlowId: "requirements-authoring-flow-2.1"
    TrustState: "trusted_learned"

Generalization test:
  "DomainId: software-development" must be set for ALL B06 transitions.
  Rule 17 is unconditional for this batch.
  Second routing decision to 2.4 also expected because "stakeholder" is compiled.
  The model must set DomainId: software-development on every emitted transition,
  regardless of the request content.

---

## Held-Out Pair B06B1-WDA-HO-019 — probationary flow rejected, fallback used

```
source_id: xiigen-b06-held-out-019
pair_kind: held_out
```

A:
  run_id: "B06-ho-019"
  sub_phase_id: "2.1"
  flow_candidate_state:
    registered_flow_id: "requirements-authoring-flow-2.1-v2-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "node_coverage: 7/9 (N04, N05 missing)"

Expected B (factory output):
  SelectedFlowId: "requirements-authoring-flow-2.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-v2-candidate"
  SelectionOutcome: "PRODUCTION_FLOW_SELECTED"
  Warnings: ["probationary_candidate_rejected: requirements-authoring-flow-2.1-v2-candidate (7/9 nodes)"]

Note: "requirements-authoring-flow-2.1" (original) is trusted_learned. The v2 candidate
has not yet reached trusted_learned. The factory selects the trusted_learned production flow
and rejects the probationary candidate, with a warning indicating candidate rejection.

---

## Corruption Pair B06B1-WDA-COR-011 — v6-03: wrong DomainId

```
source_id: xiigen-b06-corruption-011
pair_kind: corruption
corruption_type: Corruption-v6-03-wrong-domain-id
```

A:
  DocumentId: "B06-ho-018-prompt"
  CompiledText: "Define the checkout flow business requirements and stakeholder expectations."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  TransitionId: "transition-B06-ho-018"
  TransitionType: "documentation"
  DomainId: "software-development"
  SubPhaseId: "2.1"

B-corrupt:
  TransitionId: "transition-B06-ho-018"
  TransitionType: "documentation"
  DomainId: "business-analysis"
  SubPhaseId: "2.1"

Corruption type: v6-03 — wrong DomainId; "business requirements" context causes the model
to emit DomainId: business-analysis instead of software-development.
Why harmful: Rule 17 states DomainId is always software-development for B06 (Checkout flow
business requirements and use cases). The domain is the software system being built, not
the business domain of the requirement. A wrong DomainId routes the transition to the wrong
domain processor, which may have different flow registries and arbiter configurations.
The model must apply Rule 17 unconditionally: DomainId = software-development for all B06 transitions.
