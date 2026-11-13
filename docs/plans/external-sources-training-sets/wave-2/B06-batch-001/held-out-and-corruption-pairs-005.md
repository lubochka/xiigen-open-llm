# Held-Out and Corruption Pairs (005)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 005 covers HO-010 (stakeholder→2.1+2.4), HO-011 (no predicate match), COR-007 (v6-03 routing collapse from 2.1+2.4 to 2.1 only).

---

## Held-Out Pair B06B1-WDA-HO-010 — stakeholder requirements → 2.1 + 2.4

```
source_id: xiigen-b06-held-out-010
pair_kind: held_out
```

A:
  run_id: "B06-ho-010"
  user_request: "Document the business requirements and stakeholder expectations for the new checkout payment flow."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["document", "business", "stakeholder", "requirements"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "2.4"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "document" → DOCUMENT. "business" → DOCUMENT (Rule 7). "stakeholder" → DOCUMENT (Rule 8).
  "requirements" → DOCUMENT. Four signals → one predicate, confidence 0.92.
  "stakeholder" triggers 2.4 sub-phase in addition to 2.1.
  TransitionType: documentation (no IMPLEMENT, no UI).
  The model must produce exactly two routing decisions: 2.1 and 2.4.

---

## Held-Out Pair B06B1-WDA-HO-011 — no predicate: "improve the checkout flow"

```
source_id: xiigen-b06-held-out-011
pair_kind: held_out
```

A:
  run_id: "B06-ho-011"
  user_request: "We need to improve the checkout flow to reduce cart abandonment."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: []
  Confidence: 0.70
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Expected routing decisions:
  - PredictedSubPhaseId: null; KnowledgeEdge: true; FailureClassification: "no_predicate_match"

Generalization test:
  "improve" — not in any predicate lexicon.
  "checkout flow" — "checkout" and "flow" are not predicate lexicon terms.
  "cart abandonment" — not in any predicate lexicon.
  No lexicon terms present → knowledge-edge.
  The model must NOT infer DOCUMENT from business intent. Only lexicon terms fire predicates.

---

## Corruption Pair B06B1-WDA-COR-007 — v6-03: stakeholder 2.4 routing dropped

```
source_id: xiigen-b06-corruption-007
pair_kind: corruption
corruption_type: Corruption-v6-03-routing-collapsed
```

A:
  DocumentId: "B06-ho-010-prompt"
  CompiledText: "Document the business requirements and stakeholder expectations for the new checkout payment flow."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B-correct:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
    - PredictedSubPhaseId: "2.4"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  DecisionCount: 2
  Warnings: []

B-corrupt:
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  DecisionCount: 1
  Warnings: []

Corruption type: v6-03 — routing collapsed; 2.4 routing decision dropped despite "stakeholder"
signal being present in compiled terms.
Why harmful: StakeholderRequirementsFlow24 never executes. External payment partner and
marketplace vendor requirements are never documented. The stakeholder perspective — partner
SLAs (Service-Level Agreements), integration contracts, external facing requirements — is
entirely absent from the output. Downstream: external partners receive no formal requirements
document. The model must emit two routing decisions whenever "stakeholder" is a compiled term.
