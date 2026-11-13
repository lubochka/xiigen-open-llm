# Layer 0-B — Router Pairs (053) — CurrencyFormatter: routing decisions for TEST and zero-predicate cases
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: router

---

## R053-001 — TEST+IMPLEMENT: "create CurrencyFormatter and write test suite" routes to 5.1 + 4.1

Request: "Create the CurrencyFormatter and write a test suite covering GBP (Great British Pound), EUR (Euro), and USD (US Dollar) formatting."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"]
  RoutingDecisions: [TEST → 5.1, IMPLEMENT → 4.1] | TransitionType: feature_addition

---

## R053-002 — TEST alone: "assert" on currency symbol placement routes to 5.1, documentation

Request: "Assert that the CurrencyFormatter places the currency symbol before the amount for GBP (Great British Pound)."
  CompiledPredicateIds: ["TEST"]
  RoutingDecisions: [TEST → 5.1] | TransitionType: documentation | SourceStatus: routable

---

## R053-003 — Zero predicate: "format the currency value" routes to nowhere

Request: "Format the currency value to two decimal places using the CurrencyFormatter."
  "format" → domain operation verb; zero predicates.
  CompiledPredicateIds: []
  RoutingDecisions: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## R053-004 — Routing minimal pair: "validate" (TEST → 5.1) vs "evaluate" (nowhere)

A-validate: "Validate that the CurrencyFormatter displays negative amounts correctly." → [TEST → 5.1] | documentation
A-evaluate: "Evaluate the output format produced by the CurrencyFormatter." → [] | unknown | knowledge_edge
Teaching: "validate" is in the TEST lexicon; "evaluate" is a domain computation verb.
