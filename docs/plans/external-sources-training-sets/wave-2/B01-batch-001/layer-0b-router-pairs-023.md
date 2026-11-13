# Layer 0-B — Router Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## Method note (023)

v6-03 wrong routing examples. Corruption-v6-03 is the multi-branch result collapse:
  The corrupted output merges multiple predicates into a single RoutingDecision,
  assigns the wrong sub-phase to a predicate, or fabricates decisions for missing predicates.

Each pair shows:
  A: correct compiler output
  B-correct: correct routing (one entry per predicate)
  B-corrupt: v6-03 collapse

---

## B01-0B-023-001 — v6-03: collapsing UI+DOCUMENT into single 1.1 decision

```
source_id: xiigen-b01-0b-023-001
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-03-multi-branch-collapse
```

A:
  DocumentId: "B01-0b-023-a-prompt"
  CompiledText: "Describe the responsive layout and define the specification for the wishlist page."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 2

B-corrupt (v6-03):
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI", "DOCUMENT"], KnowledgeEdge: false
  DecisionCount: 1

Corruption analysis:
  B-corrupt collapses both predicates into a single 1.1 entry.
  DOCUMENT is silently assigned to 1.1 — which does not own the DOCUMENT predicate.
  Sub-phase 2.1 (BusinessRequirementsFlow21) is never selected.
  The specification artifact is never produced.
  Correct output: always 2 entries when 2 predicates fire.

---

## B01-0B-023-002 — v6-03: collapsing three predicates into one decision

```
source_id: xiigen-b01-0b-023-002
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-03-multi-branch-collapse
```

A:
  DocumentId: "B01-0b-023-b-prompt"
  CompiledText: "Build and document the checkout form component, specifying all input interaction criteria."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["IMPLEMENT"]
  DecisionCount: 3

B-corrupt (v6-03):
  RoutingDecisions:
    - PredictedSubPhaseId: "4.1", EvidencePredicates: ["UI", "DOCUMENT", "IMPLEMENT"]
  DecisionCount: 1

Corruption analysis:
  B-corrupt puts all predicates into the IMPLEMENT sub-phase 4.1 entry.
  1.1 and 2.1 are never populated. Screen inventory and business requirements never execute.
  Correct output: 3 independent entries.

---

## B01-0B-023-003 — v6-03: inventing a 1.1 decision from a no-predicate input

```
source_id: xiigen-b01-0b-023-003
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-03-fabricated-routing
```

A:
  DocumentId: "B01-kedge-nav-001-prompt"
  CompiledText: "Redesign the main navigation menu to reduce cognitive load."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: null, KnowledgeEdge: true, FailureClassification: "no_predicate_match"
  DecisionCount: 1

B-corrupt (v6-03 fabrication):
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"], KnowledgeEdge: false
  DecisionCount: 1

Corruption analysis:
  B-corrupt fabricates a UI predicate from the word "navigation" — which is NOT in the UI lexicon.
  The correct output is KnowledgeEdge: true with no sub-phase.
  Guessing a sub-phase from gap vocabulary is Corruption-v6-03 (fabricated routing).

---

## B01-0B-023-004 — v6-03: assigning DOCUMENT evidence to 1.1 instead of 2.1

```
source_id: xiigen-b01-0b-023-004
source_license_state: first_party
pair_kind: router
corruption_type: Corruption-v6-03-wrong-sub-phase
```

A:
  DocumentId: "B01-0b-023-d-prompt"
  CompiledText: "Describe the functional behavior narrative for the settings dashboard."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
    - PredictedSubPhaseId: "2.1", EvidencePredicates: ["DOCUMENT"]
  DecisionCount: 2

B-corrupt (v6-03 wrong sub-phase):
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["UI"]
    - PredictedSubPhaseId: "1.1", EvidencePredicates: ["DOCUMENT"]
  DecisionCount: 2

Corruption analysis:
  B-corrupt routes DOCUMENT to 1.1 instead of 2.1.
  DOCUMENT's owner is sub-phase 2.1, not 1.1.
  Two entries are produced but both point to the wrong sub-phase for DOCUMENT.
  This is Corruption-v6-03: wrong sub-phase assignment for a correctly identified predicate.
