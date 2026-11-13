# Held-Out and Corruption Pairs (002)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 002 covers held-out pairs: HO-004 (criteria→DOCUMENT) and HO-005 (spec→DOCUMENT+IMPLEMENT).
Corruption pairs: COR-004 (criteria routed to QA instead of DOCUMENT).

---

## Held-Out Pair B06B1-WDA-HO-004 — acceptance criteria fires DOCUMENT

```
source_id: xiigen-b06-held-out-004
pair_kind: held_out
```

A:
  run_id: "B06-ho-004"
  user_request: "Document the acceptance criteria for the guest checkout flow, covering all error states and edge cases."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["document", "criteria"]
  Confidence: 0.91
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"
  - EvidencePredicates: ["DOCUMENT"]
  - KnowledgeEdge: false

Generalization test:
  "document" → DOCUMENT. "criteria" (acceptance criteria) → DOCUMENT.
  Two DOCUMENT signals → single predicate, higher confidence (0.91).
  "guest checkout flow" — no IMPLEMENT verb present. TransitionType: documentation.
  The model must fire DOCUMENT from two independent lexicon terms without treating
  them as separate predicate instances.

---

## Held-Out Pair B06B1-WDA-HO-005 — specification fires DOCUMENT; write adds IMPLEMENT

```
source_id: xiigen-b06-held-out-005
pair_kind: held_out
```

A:
  run_id: "B06-ho-005"
  user_request: "Write the technical specification for the checkout payment integration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["specification"]
  CompiledTerms_IMPLEMENT: ["write"]
  Confidence: 0.90
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]; KnowledgeEdge: false

Generalization test:
  "specification" → DOCUMENT. "write" → IMPLEMENT.
  "technical specification" — 'technical' is a qualifier, not a separate predicate signal.
  TransitionType: feature_addition (IMPLEMENT present).
  The model must fire both predicates and produce two routing decisions (2.1 + 4.1).

---

## Corruption Pair B06B1-WDA-COR-004 — v6-01: criteria routed to QA instead of DOCUMENT

```
source_id: xiigen-b06-corruption-004
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  DocumentId: "B06-ho-004-prompt"
  CompiledText: "Document the acceptance criteria for the guest checkout flow."
  SourceStatus: "compiler output pending"

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["document", "criteria"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false

B-corrupt:
  CompiledPredicateIds: ["QA"]
  CompiledTerms: ["criteria"]
  RoutingDecisions:
    - PredictedSubPhaseId: "5.1"
      EvidencePredicates: ["QA"]
      KnowledgeEdge: false

Corruption type: v6-01 — DOCUMENT missed; criteria misclassified as QA signal.
Why harmful: "acceptance criteria" is a DOCUMENT artifact (requirements documentation).
Routing to QA sub-phase 5.1 skips the requirements authoring entirely and triggers a
test plan authoring flow instead. No use case document or requirements spec is produced.
The model must recognize "criteria" as a DOCUMENT lexicon term, not a QA trigger.
