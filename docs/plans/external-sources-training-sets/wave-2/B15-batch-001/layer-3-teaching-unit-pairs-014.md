# Layer 3 — Teaching Unit Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-014-A — "chargeback" is not a gap

A: "Implement the refund and chargeback service." Does the compiler return [] because "chargeback" is unknown domain terminology?
B: No. "implement" fires IMPLEMENT unconditionally. Domain nouns ("chargeback") never suppress predicate detection. The v6-01 corruption is exactly this mistake. Correct: CompiledPredicateIds: ["IMPLEMENT"].

## TU-014-B — "payment reversal" alone does not fire API_CONTRACT

A: "Implement the refund service with payment reversal logic." How many predicates fire?
B: One: IMPLEMENT. "payment reversal" is a business process description within IMPLEMENT scope. API_CONTRACT fires only on: integrate/wire up/connect (verbs) or "contract" (keyword). "reversal" is not in the API_CONTRACT lexicon.

## TU-014-C — "define payment reversal contracts" fires API_CONTRACT

A: "Implement the refund service and define the payment reversal contracts." How many predicates?
B: Two: ["IMPLEMENT", "API_CONTRACT"]. "define" fires DOCUMENT — wait. "define" fires DOCUMENT. "contracts" fires API_CONTRACT. Three predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]. Teaching point: "define the contracts" fires both API_CONTRACT (on "contracts") and DOCUMENT (on "define").

## TU-014-D — partial refund vs full refund: same predicate

A: "Implement partial refund and implement full refund logic." How many predicates?
B: One: ["IMPLEMENT"] — after deduplication. "implement" fires twice → single IMPLEMENT predicate. TransitionType: feature_addition.

## TU-014-E — chargeback status machine: is ARCHITECTURE fired?

A: "Implement the chargeback handling service and define the domain model for chargeback states." How many predicates?
B: Two: ["IMPLEMENT", "ARCHITECTURE"]. "domain model" fires ARCHITECTURE. "chargeback states" is the domain noun following "model" — irrelevant to the trigger. "model" is the keyword.
