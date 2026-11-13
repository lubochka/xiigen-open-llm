# Held-Out and Corruption Pairs (003)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 003 covers HO-006 (narrative→DOCUMENT) and COR-005 (narrative suppressed, v6-01).

---

## Held-Out Pair B06B1-WDA-HO-006 — narrative fires DOCUMENT (Rule 13)

```
source_id: xiigen-b06-held-out-006
pair_kind: held_out
```

A:
  run_id: "B06-ho-006"
  user_request: "Create the product narrative for the new checkout experience — what problem it solves and why customers will love it."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["narrative"]
  CompiledTerms_IMPLEMENT: ["create"]
  Confidence: 0.89
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]; KnowledgeEdge: false

Generalization test:
  "narrative" → DOCUMENT (Rule 13). "create" → IMPLEMENT.
  "checkout experience" — no additional predicate signals.
  TransitionType: feature_addition (IMPLEMENT present).
  The model must recognize "narrative" as a DOCUMENT term (Rule 13) and "create"
  as an IMPLEMENT verb, producing dual-predicate output.

---

## Held-Out Pair B06B1-WDA-HO-007 — epics fires DOCUMENT (Rule 15)

```
source_id: xiigen-b06-held-out-007
pair_kind: held_out
```

A:
  run_id: "B06-ho-007"
  user_request: "Define the epics for the checkout redesign — covering the payment, cart, and confirmation flow epics."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["define", "epics"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "define" → DOCUMENT. "epics" → DOCUMENT (Rule 15).
  Two DOCUMENT signals → one predicate, confidence 0.92.
  No IMPLEMENT verb present → TransitionType: documentation.
  "redesign" is not in any predicate lexicon.
  The model must fire DOCUMENT from "define" + "epics" and set documentation TransitionType.

---

## Corruption Pair B06B1-WDA-COR-005 — v6-01: narrative missed; treated as UI-only

```
source_id: xiigen-b06-corruption-005
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  DocumentId: "B06-ho-006-prompt"
  CompiledText: "Create the product narrative for the new checkout experience."
  SourceStatus: "compiler output pending"

B-correct:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["narrative"]
  CompiledTerms_IMPLEMENT: ["create"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

B-corrupt:
  CompiledPredicateIds: ["UI"]
  CompiledTerms: ["experience"]
  RoutingDecisions:
    - PredictedSubPhaseId: "1.1"; EvidencePredicates: ["UI"]

Corruption type: v6-01 — DOCUMENT missed; "narrative" misclassified as UI signal because
"checkout experience" context primed the model for UI predicate.
Why harmful: Product narrative is a DOCUMENT artifact — a high-level requirements and
vision document. Routing to UI sub-phase 1.1 skips requirements documentation entirely.
No business requirements or use cases are produced. The model must scan for "narrative"
as a DOCUMENT lexicon term (Rule 13) independent of surrounding context.
