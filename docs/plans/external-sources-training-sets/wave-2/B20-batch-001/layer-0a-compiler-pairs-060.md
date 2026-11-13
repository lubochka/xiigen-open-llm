# Layer 0-A — Compiler Pairs (060) — Complete factory reference: all utility domains predicate summary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P060-001 — Full reference: utility-domain verbs that fire TEST

TEST-firing verbs confirmed across all utility-domain classes (StringParser, RegexValidator, DateTimeCalc, TimeZoneConverter, CurrencyFormatter, NumberFormatter, UnitConverter, GeoCalculator, StatisticsCalc, ProbabilityEngine, RandomGenerator):
  test, verify, assert, validate, spec, mock (test-adjacent), stub (test-adjacent), test-driven
  Compounds: unit tests, test cases, test suite, test harness, test fixture, test double, test coverage, regression tests
  Domain operation verbs do NOT fire any predicate: parse, convert, format, round, split, match, calculate, compute, determine, evaluate.

---

## P060-002 — Full reference: utility-domain verbs that do NOT fire any predicate

Domain operation verbs — all produce zero predicates:
  parse, tokenise, split, match, format, round, convert, transform, evaluate, calculate, compute
  determine, process, produce, apply (standalone), generate (boundary: "generate" DOES fire IMPLEMENT)
  generate is the only utility operation verb that fires IMPLEMENT.

---

## P060-003 — Boundary case: "generate" fires IMPLEMENT; all other utility operation verbs do not

A-generate: "Generate the RandomGenerator output sequence." → CompiledPredicateIds: ["IMPLEMENT"]
A-calculate: "Calculate the GeoCalculator distance." → CompiledPredicateIds: []
"generate" is uniquely in the IMPLEMENT predicate lexicon among utility operation verbs.

---

## P060-004 — TEST alone TransitionType reference for utility-domain classes

When only TEST fires (assert/verify/validate/spec on utility domain):
  TransitionType: documentation
  Domain operation verbs (parse, convert, format, round, calculate, compute) do NOT count as IMPLEMENT.

---

## P060-005 — Utility-domain abbreviation gap reference

None of these utility abbreviations fire predicates:
  UTC (Coordinated Universal Time), DST (Daylight Saving Time), IANA (Internet Assigned Numbers Authority),
  GPS (Global Positioning System), ISO (International Organisation for Standardisation),
  GBP (Great British Pound), EUR (Euro), USD (US Dollar), CSV (Comma-Separated Values)
  All are domain vocabulary. Predicates fire from action verbs and test-infrastructure vocabulary only.
