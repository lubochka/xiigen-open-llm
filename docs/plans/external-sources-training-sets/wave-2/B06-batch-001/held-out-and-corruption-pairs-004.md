# Held-Out and Corruption Pairs (004)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 004 covers HO-008 (feature-brief→DOCUMENT not IMPLEMENT), HO-009 (capability→DOCUMENT), COR-006 (feature-brief treated as IMPLEMENT only, v6-01).

---

## Held-Out Pair B06B1-WDA-HO-008 — feature-brief fires DOCUMENT (Rule 14), not IMPLEMENT

```
source_id: xiigen-b06-held-out-008
pair_kind: held_out
```

A:
  run_id: "B06-ho-008"
  user_request: "Author the feature brief for the cart abandonment recovery capability."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["feature-brief", "capability"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "feature brief" → DOCUMENT (Rule 14). "capability" → DOCUMENT (Rule 16).
  "author" → NOT an IMPLEMENT verb (author is a documentation verb, not in IMPLEMENT lexicon).
  Two DOCUMENT signals → one predicate, confidence 0.92.
  TransitionType: documentation (no IMPLEMENT verb, no UI signal).
  The model must NOT fire IMPLEMENT from "feature brief" alone — only "feature" alone fires IMPLEMENT.

---

## Held-Out Pair B06B1-WDA-HO-009 — capability fires DOCUMENT (Rule 16)

```
source_id: xiigen-b06-held-out-009
pair_kind: held_out
```

A:
  run_id: "B06-ho-009"
  user_request: "Describe the capability requirements for the checkout service — covering what the system must do, not how it does it."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms_DOCUMENT: ["describe", "capability", "requirements"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false

Generalization test:
  "describe" → DOCUMENT. "capability" → DOCUMENT (Rule 16). "requirements" → DOCUMENT.
  Three DOCUMENT signals → one predicate, maximum confidence 0.92.
  "checkout service" — 'service' is not in any predicate lexicon.
  "what the system must do" — DOCUMENT intent confirmed.
  TransitionType: documentation.

---

## Corruption Pair B06B1-WDA-COR-006 — v6-01: feature-brief treated as IMPLEMENT, DOCUMENT missed

```
source_id: xiigen-b06-corruption-006
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  DocumentId: "B06-ho-008-prompt"
  CompiledText: "Author the feature brief for the cart abandonment recovery capability."
  SourceStatus: "compiler output pending"

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  CompiledTerms: ["feature-brief", "capability"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
  TransitionType: "documentation"

B-corrupt:
  CompiledPredicateIds: ["IMPLEMENT"]
  CompiledTerms: ["feature"]
  RoutingDecisions:
    - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]
  TransitionType: "feature_addition"

Corruption type: v6-01 — DOCUMENT missed; "feature brief" broken into "feature" (IMPLEMENT)
and "brief" (ignored), losing the compound DOCUMENT signal.
Why harmful: Feature briefs are requirements documents, not code. Routing to sub-phase 4.1
(Backend Services) triggers an implementation flow that produces code artifacts, not a scope
document. The feature brief — problem statement, use cases, success criteria, out-of-scope —
is never authored. Also: TransitionType incorrectly set to feature_addition.
The model must recognize "feature brief" as the compound DOCUMENT term (Rule 14).
