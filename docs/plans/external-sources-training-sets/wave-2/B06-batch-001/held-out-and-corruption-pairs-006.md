# Held-Out and Corruption Pairs (006)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 006 covers HO-012 (define→DOCUMENT not IMPLEMENT), COR-008 (v6-02: IMPLEMENT fires from "define", suppressing DOCUMENT).

---

## Held-Out Pair B06B1-WDA-HO-012 — "define" fires DOCUMENT (Rule 6), not IMPLEMENT

```
source_id: xiigen-b06-held-out-012
pair_kind: held_out
```

A:
  run_id: "B06-ho-012"
  user_request: "Define the business rules for the payment processing and order confirmation flows."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["define", "business", "rules"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "define" → DOCUMENT (Rule 6). "business" → DOCUMENT (Rule 7). "rules" is close but
  not directly in DOCUMENT lexicon — however "business" and "define" are sufficient.
  "payment processing" — not in any predicate lexicon.
  TransitionType: documentation (no IMPLEMENT verb, no UI).
  The model must recognize "define" as a DOCUMENT lexicon verb (Rule 6), NOT an IMPLEMENT signal.

---

## Held-Out Pair B06B1-WDA-HO-013 — functional requirements fires DOCUMENT

```
source_id: xiigen-b06-held-out-013
pair_kind: held_out
```

A:
  run_id: "B06-ho-013"
  user_request: "Specify the functional requirements for the checkout service, including all edge cases and error handling paths."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["functional", "requirements"]
  Confidence: 0.91
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "functional" → DOCUMENT. "requirements" → DOCUMENT. Two signals → confidence 0.91.
  "specify" — not in IMPLEMENT lexicon (specify is a neutral verb).
  TransitionType: documentation.
  "checkout service", "edge cases", "error handling" — none in any predicate lexicon.

---

## Corruption Pair B06B1-WDA-COR-008 — v6-02: "define" fires IMPLEMENT incorrectly

```
source_id: xiigen-b06-corruption-008
pair_kind: corruption
corruption_type: Corruption-v6-02-implement-false-positive
```

A:
  DocumentId: "B06-ho-012-prompt"
  CompiledText: "Define the business rules for the payment processing and order confirmation flows."
  SourceStatus: "compiler output pending"

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["define", "business"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
  TransitionType: "documentation"

B-corrupt:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["business"]
  CompiledTerms_IMPLEMENT: ["define"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]
  TransitionType: "feature_addition"

Corruption type: v6-02 — IMPLEMENT false positive; "define" treated as IMPLEMENT construction
verb when it is a DOCUMENT lexicon verb (Rule 6).
Why harmful: Sub-phase 4.1 (Backend Services) is activated for a pure requirements definition
task. An implementation flow runs, producing code artifacts where only a business rules
document was requested. TransitionType is incorrectly set to feature_addition, triggering
version bumps and deployment pipelines for a documentation task.
The model must check "define" against the DOCUMENT verb lexicon before checking it against
the IMPLEMENT lexicon. Rule 6 is unconditional: "define" → DOCUMENT.
