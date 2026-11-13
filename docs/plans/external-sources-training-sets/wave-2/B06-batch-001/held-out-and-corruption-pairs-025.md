# Held-Out and Corruption Pairs (025)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-50 — GiftCardRedemption: DOCUMENT predicate fires

```
source_id: xiigen-b06-held-out-50
pair_kind: held_out
```

A:
  run_id: "B06-ho-50"
  scenario: "DocumentPredicateCheck"
  user_request: "Define the requirements for the giftcardredemption feature."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionType: "documentation"
  DomainId: "software-development"

Generalization test: "define" fires DOCUMENT; DomainId is software-development not giftcardredemption.

---

## Held-Out Pair B06B1-WDA-HO-51 — GiftCardRedemption: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-held-out-51
pair_kind: held_out
```

A:
  run_id: "B06-ho-51"
  scenario: "DualPredicateCheck"
  user_request: "Write the specification for giftcardredemption and build the service."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]
  TransitionType: "feature_addition" (IMPLEMENT present — overrides documentation)
  CanonicalOrdering: "2.1 before 4.1"

Teaching: "write" fires DOCUMENT; "build" fires IMPLEMENT; IMPLEMENT upgrades all to feature_addition.

---

## Corruption Pair B06B1-WDA-COR-27 — v6-01: DOCUMENT missed for GiftCardRedemption

```
source_id: xiigen-b06-corruption-27
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  user_request: "Specify the giftcardredemption behavior and rules."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "specify" is a DOCUMENT lexicon term → DOCUMENT must fire.

B-corrupt:
  CompiledPredicateIds: []
  Note: Predicate list empty — DOCUMENT missed.

Corruption: "specify" fires DOCUMENT; missing it is v6-01.
Fix: Confirm "specify" appears in DOCUMENT lexicon (17 terms). Always emit DOCUMENT when any lexicon term present.

---

## Corruption Pair B06B1-WDA-COR-27b — v6-03: TransitionType wrong for GiftCardRedemption

```
source_id: xiigen-b06-corruption-27b
pair_kind: corruption
corruption_type: Corruption-v6-03-transitiontype-wrong
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Context: "GiftCardRedemption: document requirements and implement service"

B-correct:
  TransitionType: "feature_addition"
  Note: IMPLEMENT present → feature_addition on ALL transitions.

B-corrupt:
  TransitionType: "documentation"
  Note: Incorrectly applying documentation despite IMPLEMENT present.

Corruption: IMPLEMENT always upgrades TransitionType to feature_addition. documentation is wrong here.
Fix: Check PredicateIds — if IMPLEMENT present, all TransitionType values become feature_addition.
