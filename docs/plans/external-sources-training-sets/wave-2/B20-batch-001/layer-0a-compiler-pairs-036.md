# Layer 0-A — Compiler Pairs (036) — Complete TEST reference: all financial calculator predicates summarised
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P036-001 — Full reference: financial calculator verbs that fire TEST

TEST-firing verbs confirmed across all Round 2 financial calculators:
  validate, assert, verify, spec, mock (test-adjacent), stub (test-adjacent), test (verb/noun/gerund), test-driven
  Compounds: unit tests, test cases, test suite, test harness, test fixture, test double, test coverage, regression tests, test scenarios
  All morphological forms: test / tests / testing / tested / test-driven

---

## P036-002 — Full reference: financial domain verbs that do NOT fire any predicate

Domain verbs in financial calculator context — all produce zero predicates:
  calculate, compute, accrue, pay, apply (standalone), evaluate, determine, process, run, execute, trigger, launch, invoke, start
  "make sure", "check", "confirm", "ensure" — informal QA; all zero predicates

---

## P036-003 — Minimal pair summary: "validate" (TEST) vs "confirm" (zero)

A-validate: "Validate the InterestCalculator returns 5% for a £1,000 principal." → CompiledPredicateIds: ["TEST"]
A-confirm: "Confirm the InterestCalculator returns 5% for a £1,000 principal." → CompiledPredicateIds: []
Difference: "validate" is in the TEST predicate lexicon; "confirm" is informal QA (zero predicate).
This is the canonical "validate ≠ confirm" boundary pair for financial domain.

---

## P036-004 — TEST alone TransitionType reference for financial calculators

When only TEST fires (no IMPLEMENT construction verb):
  TransitionType: documentation — test specification without construction.
  All financial domain verbs (calculate, compute, accrue, apply, evaluate) do NOT count as IMPLEMENT.
  Only: write, create, build, develop, implement, add, extend, generate fire IMPLEMENT.

---

## P036-005 — Financial abbreviation gap reference

None of these abbreviations fire predicates:
  ROI (Return on Investment), NPV (Net Present Value), IRR (Internal Rate of Return),
  COGS (Cost of Goods Sold), EBITDA (Earnings Before Interest Tax Depreciation Amortisation),
  P&L (Profit and Loss), HR (Human Resources), Q4 (Quarter 4), SKU (Stock Keeping Unit)
  All are domain vocabulary. Predicates fire from action verbs and test-infrastructure vocabulary only.
