# Held-Out and Corruption Pairs (031)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Held-Out Pair B06B1-WDA-HO-62 — AbandonedCartRecovery: DOCUMENT predicate fires

```
source_id: xiigen-b06-held-out-62
pair_kind: held_out
```

A:
  run_id: "B06-ho-62"
  scenario: "DocumentPredicateCheck"
  user_request: "Define the requirements for the abandonedcartrecovery feature."

Expected B:
  CompiledPredicateIds: ["DOCUMENT"]
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TransitionType: "documentation"
  DomainId: "software-development"

Generalization test: "define" fires DOCUMENT; DomainId is software-development not abandonedcartrecovery.

---

## Held-Out Pair B06B1-WDA-HO-63 — AbandonedCartRecovery: DOCUMENT+IMPLEMENT → feature_addition

```
source_id: xiigen-b06-held-out-63
pair_kind: held_out
```

A:
  run_id: "B06-ho-63"
  scenario: "DualPredicateCheck"
  user_request: "Write the specification for abandonedcartrecovery and build the service."

Expected B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]
  TransitionType: "feature_addition" (IMPLEMENT present — overrides documentation)
  CanonicalOrdering: "2.1 before 4.1"

Teaching: "write" fires DOCUMENT; "build" fires IMPLEMENT; IMPLEMENT upgrades all to feature_addition.

---

## Corruption Pair B06B1-WDA-COR-33 — v6-01: DOCUMENT missed for AbandonedCartRecovery

```
source_id: xiigen-b06-corruption-33
pair_kind: corruption
corruption_type: Corruption-v6-01-document-missed
```

A:
  user_request: "Specify the abandonedcartrecovery behavior and rules."

B-correct:
  CompiledPredicateIds: ["DOCUMENT"]
  Note: "specify" is a DOCUMENT lexicon term → DOCUMENT must fire.

B-corrupt:
  CompiledPredicateIds: []
  Note: Predicate list empty — DOCUMENT missed.

Corruption: "specify" fires DOCUMENT; missing it is v6-01.
Fix: Confirm "specify" appears in DOCUMENT lexicon (17 terms). Always emit DOCUMENT when any lexicon term present.

---

## Corruption Pair B06B1-WDA-COR-33b — v6-03: TransitionType wrong for AbandonedCartRecovery

```
source_id: xiigen-b06-corruption-33b
pair_kind: corruption
corruption_type: Corruption-v6-03-transitiontype-wrong
```

A:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Context: "AbandonedCartRecovery: document requirements and implement service"

B-correct:
  TransitionType: "feature_addition"
  Note: IMPLEMENT present → feature_addition on ALL transitions.

B-corrupt:
  TransitionType: "documentation"
  Note: Incorrectly applying documentation despite IMPLEMENT present.

Corruption: IMPLEMENT always upgrades TransitionType to feature_addition. documentation is wrong here.
Fix: Check PredicateIds — if IMPLEMENT present, all TransitionType values become feature_addition.
