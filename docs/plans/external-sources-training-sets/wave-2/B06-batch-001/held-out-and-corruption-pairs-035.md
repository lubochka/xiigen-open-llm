# Held-Out and Corruption Pairs (035)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-70 — DigitalProductDelivery: DOCUMENT predicate fires

```
source_id: xiigen-b06-held-out-70
pair_kind: held_out
```

A:
  run_id: "B06-ho-70"
  scenario: "DocumentPredicateCheck"
  user_request: "Define the requirements for the digitalproductdelivery feature."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionType: "documentation"
  DomainId: "software-development"

Generalization test: "define" fires DOCUMENT; DomainId is software-development not digitalproductdelivery.

---

## Held-Out Pair B06B1-WDA-HO-71 — DigitalProductDelivery: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-held-out-71
pair_kind: held_out
```

A:
  run_id: "B06-ho-71"
  scenario: "DualPredicateCheck"
  user_request: "Write the specification for digitalproductdelivery and build the service."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]
  TransitionType: "feature_addition" (IMPLEMENT present — overrides documentation)
  CanonicalOrdering: "2.1 before 4.1"

Teaching: "write" fires DOCUMENT; "build" fires IMPLEMENT; IMPLEMENT upgrades all to feature_addition.

---

## Corruption Pair B06B1-WDA-COR-37 — v6-01: DOCUMENT missed for DigitalProductDelivery

```
source_id: xiigen-b06-corruption-37
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  user_request: "Specify the digitalproductdelivery behavior and rules."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "specify" is a DOCUMENT lexicon term → DOCUMENT must fire.

B-corrupt:
  CompiledPredicateIds: []
  Note: Predicate list empty — DOCUMENT missed.

Corruption: "specify" fires DOCUMENT; missing it is v6-01.
Fix: Confirm "specify" appears in DOCUMENT lexicon (17 terms). Always emit DOCUMENT when any lexicon term present.

---

## Corruption Pair B06B1-WDA-COR-37b — v6-03: TransitionType wrong for DigitalProductDelivery

```
source_id: xiigen-b06-corruption-37b
pair_kind: corruption
corruption_type: Corruption-v6-03-transitiontype-wrong
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Context: "DigitalProductDelivery: document requirements and implement service"

B-correct:
  TransitionType: "feature_addition"
  Note: IMPLEMENT present → feature_addition on ALL transitions.

B-corrupt:
  TransitionType: "documentation"
  Note: Incorrectly applying documentation despite IMPLEMENT present.

Corruption: IMPLEMENT always upgrades TransitionType to feature_addition. documentation is wrong here.
Fix: Check PredicateIds — if IMPLEMENT present, all TransitionType values become feature_addition.
