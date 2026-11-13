# Layer 0-B — Router Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-003-001 — [UI] → 1.1: checkout form interactions

```
source_id: xiigen-b01-0b-003-001
source_title: XIIGen B01 router-003 — [UI] routes to 1.1 for checkout form interaction mapping
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-003-001]
  DocumentId: "B01-0a-003-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-003-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout form interaction and input validation mapping"
  CompiledText: "Map the checkout form interactions and identify each input field's validation behavior."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-003-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI fires on form, interaction, input. All three are in the UI lexicon. Sub-phase
  1.1 owns UI-predicate routing for Phase 1. "Validation behavior" — "behavior" is
  in the DOCUMENT lexicon, but in this request it describes a form field property
  rather than a specification intent. The compiler correctly produced only ["UI"].
  Single routing decision: 1.1.

---

## B01-0B-003-002 — [UI] → 1.1: checkout screen flow with multiple UI terms

```
source_id: xiigen-b01-0b-003-002
source_title: XIIGen B01 router-003 — [UI] for checkout screen with button + input + form
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-003-002]
  DocumentId: "B01-0a-003-002-prompt"
  CompiledText: "Create the checkout screen flow showing each form step, button states, and input field interactions from cart review to order confirmation."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-003-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  Five UI terms fire (screen, form, button, input, interaction). Only UI fires;
  no DOCUMENT or IMPLEMENT terms. Sub-phase 1.1 is the sole routing target.
  NearestWrongSubPhaseScore 0.43 is slightly higher because "checkout screen flow"
  could resemble 1.2 (Wireframes) or 1.5 (UI Testing) surface topics, but 1.1
  is definitively correct for screen flow and screen inventory work.

---

## B01-0B-003-003 — [UI, DOCUMENT, IMPLEMENT] → three routing decisions

```
source_id: xiigen-b01-0b-003-003
source_title: XIIGen B01 router-003 — three routing decisions from three predicates
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-003-003 revised]
  DocumentId: "B01-0a-003-003-prompt"
  CompiledText: "Describe the functional behavior of each checkout form screen so developers can implement the validation rules."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-003-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-003-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.33
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-003-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.29
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale:
  Three independent predicates → three independent routing decisions, each with
  only its own EvidencePredicates. UI → 1.1 (Screen Flow and UX Definition);
  DOCUMENT → 2.1 (Business Requirements); IMPLEMENT → 4.1 (Implementation).
  The router must produce exactly DecisionCount: 3 entries. Merging any two
  predicates into a single decision is Corruption-v6-03.
