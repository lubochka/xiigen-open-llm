# Held-Out and Corruption Pairs (008)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 008 covers HO-016 (four predicates, feature_addition), HO-017 (pure DOCUMENT, no co-predicate), COR-010 (v6-01: DOCUMENT missed in four-predicate request).

---

## Held-Out Pair B06B1-WDA-HO-016 — four predicates: feature_addition, four flows

```
source_id: xiigen-b06-held-out-016
pair_kind: held_out
```

A:
  run_id: "B06-ho-016"
  user_request: "Design the architecture, write the requirements, implement the checkout service, and document the UI screens for the complete checkout redesign."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  CompiledTerms_DOCUMENT: ["requirements", "document"]
  CompiledTerms_ARCHITECTURE: ["design", "architecture"]
  CompiledTerms_IMPLEMENT: ["write", "implement"]
  CompiledTerms_UI: ["UI", "screens"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "1.1"; EvidencePredicates: ["UI"]; KnowledgeEdge: false

Expected TransitionType: feature_addition (IMPLEMENT + UI both present)
Expected canonical ordering: 2.1 → 3.1 → 4.1 → 1.1

Generalization test:
  All four predicates fired from independent signals.
  Four routing decisions → four concurrent flows.
  The model must produce all four routing decisions without collapsing any.

---

## Held-Out Pair B06B1-WDA-HO-017 — pure DOCUMENT: "describe the behavior"

```
source_id: xiigen-b06-held-out-017
pair_kind: held_out
```

A:
  run_id: "B06-ho-017"
  user_request: "Describe the expected behavior of the checkout service when a payment fails — covering all retry scenarios and fallback options."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["describe", "behavior"]
  Confidence: 0.91
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Expected TransitionType: documentation

Generalization test:
  "describe" → DOCUMENT (Rule 5). "behavior" → DOCUMENT. Two signals → confidence 0.91.
  "checkout service", "payment fails", "retry scenarios" — not in any predicate lexicon.
  No IMPLEMENT verb present.
  TransitionType: documentation.

---

## Corruption Pair B06B1-WDA-COR-010 — v6-01: DOCUMENT missed in four-predicate request

```
source_id: xiigen-b06-corruption-010
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  DocumentId: "B06-ho-016-prompt"
  CompiledText: "Design the architecture, write the requirements, implement the checkout service, and document the UI screens."
  SourceStatus: "compiler output pending"

B-correct:
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]
    - PredictedSubPhaseId: "1.1"; EvidencePredicates: ["UI"]
  DecisionCount: 4

B-corrupt:
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "UI"]
  RoutingDecisions:
    - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]
    - PredictedSubPhaseId: "1.1"; EvidencePredicates: ["UI"]
  DecisionCount: 3

Corruption type: v6-01 — DOCUMENT missed in a multi-predicate context; "requirements" and
"document" not recognized as DOCUMENT signals while the model was focused on the stronger
construction signals (implement, write, UI, screens).
Why harmful: Sub-phase 2.1 (RequirementsAuthoringFlow21) never executes. No business
requirements, use cases, or acceptance criteria are produced. The checkout service is
implemented without a requirements specification — a direct quality and traceability gap.
The model must scan for ALL lexicon signals independently regardless of how many other
strong signals are present.
