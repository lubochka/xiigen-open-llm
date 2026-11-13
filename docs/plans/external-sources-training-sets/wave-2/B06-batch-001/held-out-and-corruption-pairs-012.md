# Held-Out and Corruption Pairs (012)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 012 covers HO-024 (cross-layer chain integrity), COR-014 (v6-03: chain DocumentId broken), and the held-out evaluation summary.

---

## Held-Out Pair B06B1-WDA-HO-024 — chain integrity: DocumentId propagation

```
source_id: xiigen-b06-held-out-024
pair_kind: held_out
```

A:
  run_id: "B06-ho-024"
  scenario: "ChainIntegrityTest"
  sub_phase_id: "2.1"
  input_document_id: "B06-transition-ho-024"

Expected chain:
  Layer_0A:
    Input: "UserRequest (raw text)"
    Output: "CompiledPredicateIds with DocumentId: B06-transition-ho-024"

  Layer_0B:
    Input: "DocumentId: B06-transition-ho-024"
    Output: "RoutingDecisions referencing DocumentId: B06-transition-ho-024"

  Layer_0D:
    Input: "RoutingDecisions with DocumentId: B06-transition-ho-024"
    Output: "SelectedFlowId with SourceDocumentId: B06-transition-ho-024"

  Layer_1:
    Input: "SelectedFlowId with SourceDocumentId: B06-transition-ho-024"
    Output: "TransitionId: transition-B06-ho-024 with DocumentId: B06-transition-ho-024"

  Layer_2:
    Input: "TransitionId: transition-B06-ho-024"
    Output: "N01 through N09 all reference TransitionId: transition-B06-ho-024"

Expected chain integrity:
  DocumentId_continuity: "B06-transition-ho-024 must appear in every layer's output"
  Missing_DocumentId: "FAIL — chain broken"

Generalization test:
  The DocumentId is the chain integrity anchor. Every layer must pass it forward.
  If any layer drops the DocumentId, the chain is broken and the nine-node flow cannot
  be attributed back to its source request.

---

## Held-Out Pair B06B1-WDA-HO-025 — confidence calibration for single term

```
source_id: xiigen-b06-held-out-025
pair_kind: held_out
```

A:
  run_id: "B06-ho-025"
  user_request: "Write a spec."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (compiler output):
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  CompiledTerms_DOCUMENT: ["spec"]
  CompiledTerms_IMPLEMENT: ["write"]
  Confidence: 0.86
  Warnings: []

Expected routing decisions:
  - PredictedSubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]; KnowledgeEdge: false
  - PredictedSubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]; KnowledgeEdge: false

Generalization test:
  Minimal request: "Write a spec" — one DOCUMENT term, one IMPLEMENT term.
  Confidence 0.86 (two terms: one per predicate, less context).
  TransitionType: feature_addition.
  The model must fire both predicates even from a terse request.

---

## Corruption Pair B06B1-WDA-COR-014 — v6-03: DocumentId dropped, chain broken

```
source_id: xiigen-b06-corruption-014
pair_kind: corruption
corruption_type: Corruption-v6-03-chain-integrity-broken
```

A:
  SourceDocumentId: "B06-transition-ho-024"
  Layer_0B_output:
    RoutingDecisions:
      - PredictedSubPhaseId: "2.1"; DocumentId: "B06-transition-ho-024"

B-correct (Layer_1 Transition):
  TransitionId: "transition-B06-ho-024"
  DocumentId: "B06-transition-ho-024"
  TransitionType: "documentation"
  DomainId: "software-development"
  SubPhaseId: "2.1"
  SelectedFlowId: "requirements-authoring-flow-2.1"

B-corrupt (Layer_1 Transition):
  TransitionId: "transition-B06-ho-024"
  DocumentId: null
  TransitionType: "documentation"
  DomainId: "software-development"
  SubPhaseId: "2.1"
  SelectedFlowId: "requirements-authoring-flow-2.1"

Corruption type: v6-03 — DocumentId dropped at Layer_1; chain integrity broken.
Why harmful: Downstream Layer_2 nine-node flows receive a transition with null DocumentId.
They cannot attribute their output back to the originating request. QA (Quality Assurance)
cannot verify which request produced which use case document. Debugging a production
failure is impossible without the DocumentId chain.
The model must always propagate DocumentId from each layer's input to each layer's output.
DocumentId is never generated fresh inside a layer — it always comes from the layer above.

---

## Held-Out Evaluation Summary

```
source_id: xiigen-b06-held-out-summary
pair_kind: held_out_summary
```

Total held-out pairs: 25 (HO-001 through HO-025)
Total corruption pairs: 14 (COR-001 through COR-014)

Held-out categories:
  - Predicate firing accuracy: HO-001, HO-002, HO-004, HO-005, HO-006, HO-007, HO-008, HO-009, HO-010, HO-011, HO-012, HO-013, HO-017, HO-023, HO-025
  - Routing accuracy (sub-phase decisions): HO-003, HO-010, HO-014, HO-015, HO-016, HO-018
  - Output structure (use case completeness): HO-020, HO-021
  - Arbiter behavior: HO-022
  - Chain integrity: HO-024
  - Trust state / flow factory: HO-019

Corruption categories covered:
  - v6-01 (DOCUMENT missed): COR-004, COR-005, COR-006, COR-008, COR-010, COR-012
  - v6-02 (IMPLEMENT suppressed / false positive / arbiter suppressed): COR-008, COR-013
  - v6-03 (routing collapsed / TransitionType wrong / chain broken): COR-007, COR-009, COR-011, COR-014

Promotion gate:
  Required: Pass all 25 held-out pairs without v6 corruption in output.
  Pass condition: All 25 HO outputs match Expected B with zero corruption type matches.
