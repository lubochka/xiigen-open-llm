# Held-Out and Corruption Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-048 — Nine-node N01 receives Layer 1 output (new domain)

```
source_id: xiigen-b06-held-out-048
pair_kind: held_out
```

A:
  run_id: "B06-ho-048"
  scenario: "N01InputChainCheck"
  user_request: "Document the loyalty program requirements."

Expected N01 A input:
  Type: "SelectedLogicalTransition"
  TransitionId: "transition-loyalty-ho-048"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  DomainId: "software-development"

Generalization test: N01 always receives SelectedLogicalTransition from Layer 1.
  N01 does NOT receive the raw user request directly.

---

## Held-Out Pair B06B1-WDA-HO-049 — "specify" alone is DOCUMENT; "build" alone is IMPLEMENT

```
source_id: xiigen-b06-held-out-049
pair_kind: held_out
```

A:
  run_id: "B06-ho-049"
  user_request: "Specify the subscription billing requirements."

Expected B (pair 1):
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "specify" fires DOCUMENT only.

Contrast case B (pair 2 — hypothetical):
  user_request: "Build the subscription billing service."
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "build" fires IMPLEMENT only (no DOCUMENT terms in request).

Teaching: verb determines predicate; "specify" is DOCUMENT; "build" is IMPLEMENT.

---

## Corruption Pair B06B1-WDA-COR-026 — v6-03: ordering violation new domain

```
source_id: xiigen-b06-corruption-026
pair_kind: corruption
corruption_type: Corruption-v6-03-ordering-violation
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions:
    - PredictedSubPhaseId: "2.1"
    - PredictedSubPhaseId: "4.1"
  Context: "FraudDetection: write fraud detection requirements and implement the risk engine"

B-correct:
  Transitions:
    1. SubPhaseId: "2.1"; TransitionType: "feature_addition"
    2. SubPhaseId: "4.1"; TransitionType: "feature_addition"

B-corrupt:
  Transitions:
    1. SubPhaseId: "4.1"; TransitionType: "feature_addition"
    2. SubPhaseId: "2.1"; TransitionType: "feature_addition"

Corruption: 4.1 before 2.1 — fraud detection implementation before requirements.
Fix: 2.1 always precedes 4.1 in canonical ordering. Requirements before implementation.

---

## Extended Held-Out Evaluation Summary (HO-026 through HO-049)

Total new held-out pairs: 24 (HO-026 through HO-049)
Total new corruption pairs: 12 (COR-015 through COR-026)

New held-out categories added:
  - New domain DOCUMENT: HO-026, HO-028, HO-029, HO-031, HO-032, HO-034, HO-040, HO-041, HO-042, HO-044, HO-045
  - Dual predicate (DOCUMENT+IMPLEMENT): HO-027, HO-030, HO-033, HO-039, HO-043
  - Stakeholder dual route (2.1+2.4): HO-028, HO-035
  - Knowledge-edge: HO-037
  - Partial predicate: HO-036
  - DOCUMENT+ARCHITECTURE: HO-038
  - Trust state (probationary_rejected): HO-046
  - Chain integrity (N01 input): HO-048
  - Verb disambiguation: HO-047, HO-049

New corruption categories:
  - v6-01 (DOCUMENT missed): COR-015, COR-019, COR-024
  - v6-02 (IMPLEMENT suppressed/false-positive): COR-017, COR-021, COR-022, COR-025
  - v6-03 (routing collapsed/TransitionType/ordering/extra-subphase/DomainId): COR-016, COR-018, COR-020, COR-023, COR-026

Promotion gate extension:
  Required: Pass all 49 held-out pairs (HO-001 through HO-049) without v6 corruption.
  Existing 25 HO pairs remain in effect. New 24 HO pairs added.
