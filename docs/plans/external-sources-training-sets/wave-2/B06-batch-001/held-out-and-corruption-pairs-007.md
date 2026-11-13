# Held-Out and Corruption Pairs (007)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 007 covers HO-014 (DOCUMENT+ARCHITECTURE: documentation TransitionType), COR-009 (v6-03: ARCHITECTURE presence wrongly triggers feature_addition).

---

## Held-Out Pair B06B1-WDA-HO-014 — DOCUMENT + ARCHITECTURE stays documentation

```
source_id: xiigen-b06-held-out-014
pair_kind: held_out
```

A:
  run_id: "B06-ho-014"
  user_request: "Design the architecture and write the requirements spec for the checkout service redesign."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["requirements", "spec"]
  CompiledTerms_ARCHITECTURE: ["architecture", "design"]
  CompiledTerms_IMPLEMENT: ["write"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]; KnowledgeEdge: false

Expected TransitionType: feature_addition (IMPLEMENT present from "write")

Generalization test:
  "design" → ARCHITECTURE. "architecture" → ARCHITECTURE. "requirements" → DOCUMENT.
  "spec" → DOCUMENT. "write" → IMPLEMENT.
  Three predicates fired → three routing decisions (2.1 + 3.1 + 4.1).
  TransitionType: feature_addition because IMPLEMENT is present.
  The model must fire all three predicates independently and produce three routing decisions.

---

## Held-Out Pair B06B1-WDA-HO-015 — DOCUMENT + ARCHITECTURE only: documentation TransitionType

```
source_id: xiigen-b06-held-out-015
pair_kind: held_out
```

A:
  run_id: "B06-ho-015"
  user_request: "Design the system architecture and document the functional requirements for the checkout service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  CompiledTerms_DOCUMENT: ["document", "functional", "requirements"]
  CompiledTerms_ARCHITECTURE: ["design", "architecture"]
  Confidence: 0.92
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]; KnowledgeEdge: false

Expected TransitionType: documentation (no IMPLEMENT, no UI — ARCHITECTURE alone does not force feature_addition)

Generalization test:
  "document" → DOCUMENT. "functional" → DOCUMENT. "requirements" → DOCUMENT.
  "design" → ARCHITECTURE. "architecture" → ARCHITECTURE.
  No IMPLEMENT verb (document, design are not IMPLEMENT verbs).
  TransitionType: documentation.

---

## Corruption Pair B06B1-WDA-COR-009 — v6-03: ARCHITECTURE triggers feature_addition incorrectly

```
source_id: xiigen-b06-corruption-009
pair_kind: corruption
corruption_type: Corruption-v6-03-transition-type-wrong
```

A:
  DocumentId: "B06-ho-015-prompt"
  CompiledText: "Design the system architecture and document the functional requirements for the checkout service."
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  SourceStatus: "ready"

B-correct:
  TransitionType: "documentation"
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
  ArbiterType: "documentation completeness and accuracy arbiter"
  VersionBump: false
  DeploymentPipelineActivated: false

B-corrupt:
  TransitionType: "feature_addition"
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - PredictedSubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
  ArbiterType: "code quality and testability arbiter"
  VersionBump: true
  DeploymentPipelineActivated: true

Corruption type: v6-03 — TransitionType wrong; ARCHITECTURE presence treated as equivalent
to IMPLEMENT for TransitionType determination.
Why harmful: Documentation activities trigger deployment pipeline and version bump. Code
quality arbiter is selected instead of documentation completeness arbiter. QA (Quality
Assurance) regression gates are activated for what is purely a design and documentation task.
The model must apply the rule: ARCHITECTURE alone → documentation; only IMPLEMENT or UI
→ feature_addition.
