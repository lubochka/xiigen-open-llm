# Layer 0-B — Router Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## Method note (022)

Routing order rules. This file teaches:
  1. Sub-phase hierarchy for primary SubPhaseId selection: UI > ARCHITECTURE > IMPLEMENT > DOCUMENT
  2. All predicates always produce independent RoutingDecision entries
  3. The order of decisions in the array does not affect execution
  4. EM-9 guard: no dependency_order, parallel_group, domain_label, or is_primary fields

---

## B01-0B-022-001 — Order rule: UI is primary when UI and DOCUMENT both present

```
source_id: xiigen-b01-0b-022-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0b-022-order-001-prompt"
  CompiledText: "Define the wireframe for the registration page and document the behavioral specification for each input field."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0b-022-order-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0b-022-order-001-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Order rule applied:
  Primary sub-phase for SelectedLogicalTransition: 1.1 (UI ranks above DOCUMENT).
  Router produces both entries; orchestrator selects the primary based on hierarchy.
  EM-9: no is_primary, dependency_order, parallel_group, or domain_label fields in entries.

---

## B01-0B-022-002 — Order rule: IMPLEMENT wins over UI and DOCUMENT for TransitionType

```
source_id: xiigen-b01-0b-022-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0b-022-order-002-prompt"
  CompiledText: "Implement the wishlist screen and write the feature specification for each component's behavior."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0b-022-order-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0b-022-order-002-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0b-022-order-002-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Order rule applied:
  All three predicates produce independent entries.
  TransitionType resolved by orchestrator: IMPLEMENT present → feature_addition (unconditional).
  Router itself does not set TransitionType — that is the orchestrator's responsibility.

---

## B01-0B-022-003 — EM-9 guard demonstration: forbidden fields must not appear

```
source_id: xiigen-b01-0b-022-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0b-022-em9-prompt"
  CompiledText: "Inventory the color tokens and responsive layout of the address book screen."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B (correct — no forbidden fields):
  RoutingDecisions:
    - DocumentId: "B01-0b-022-em9-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

EM-9 guard:
  Correct output does NOT contain: dependency_order, parallel_group, domain_label, is_primary.
  If any of these four fields appear in a RoutingDecision, the output is corrupt (v6-03).

---

## B01-0B-022-004 — Routing symmetry: DOCUMENT-only → 2.1 (no UI decision)

```
source_id: xiigen-b01-0b-022-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0b-022-doc-only-prompt"
  CompiledText: "Write the functional specification for the delivery options use-case."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0b-022-doc-only-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0b-022-doc-only-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Symmetry rule:
  No UI predicate → no 1.1 decision. The model must NOT add a 1.1 decision
  just because the topic is somewhat UI-adjacent. Predicates must explicitly fire.
