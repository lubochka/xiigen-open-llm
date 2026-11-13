# Layer 1 — Transition Orchestrator Pairs (050) — "configure" vs "implement" boundary: utility domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T050-001 — "configure" → unknown; "implement" → feature_addition: NumberFormatter example

Request A-configure: "Configure the NumberFormatter to use two decimal places."
  CompiledPredicateIds: [] | TransitionType: unknown
  Evidence: "configure" → NOT in IMPLEMENT lexicon; zero predicates.

Request A-implement: "Implement the NumberFormatter with two decimal place precision."
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT fires; TransitionType: feature_addition.

---

## T050-002 — "set up" → context verb (does not fire IMPLEMENT); "build" → IMPLEMENT

Request A-setup: "Set up the RandomGenerator with a fixed seed."
  CompiledPredicateIds: [] | TransitionType: unknown
  Evidence: "set up" → context verb; does NOT fire IMPLEMENT or any predicate.

Request A-build: "Build the RandomGenerator with a fixed seed parameter."
  CompiledPredicateIds: ["IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "build" → IMPLEMENT fires; TransitionType: feature_addition.

---

## T050-003 — "initialise" → context verb (does not fire IMPLEMENT): universal rule

"Initialise the GeoCalculator with the WGS-84 reference ellipsoid." → CompiledPredicateIds: []
"initialise" → context/setup verb; NOT in IMPLEMENT lexicon.
Confirmed: setup verbs (initialise, configure, set up, register, instantiate) are universally zero predicates.
Only: write, create, build, develop, implement, add, extend, generate fire IMPLEMENT.
