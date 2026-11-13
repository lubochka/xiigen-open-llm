# Layer 3 — Teaching Unit Pairs (024) — Complete teaching summary for B20 batch extension
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-024-001 — Master predicate lexicon for sub-phase 5.1 (Unit Tests)

TEST fires on (verbs): test, verify, assert, validate, spec
TEST fires on (compounds): unit test(s), integration test(s), test suite, test case(s), test harness, test fixture, test coverage, regression test(s), test double(s), mock (in test context), stub (in test context)
TEST fires on (morphological forms): test / tests / testing / tested / test-driven — all fire TEST
TEST does NOT fire on: run, execute, trigger, launch, invoke, start (execution verbs); check, confirm, ensure, make sure (informal QA); testable (adjective)

IMPLEMENT fires on: write, create, build, develop, implement, add, extend, generate
IMPLEMENT does NOT fire on: "run", "execute" (execution); "refactor", "extract" (fires but classifies as refactoring)

SECURITY fires on: security, authentication, JWT (JSON Web Token), token, auth, encryption, authorization, signing, hashing, tampered, vulnerability, revocation

---

## Teach-024-002 — "run" ≠ TEST: the definitive boundary rule

"run [any test noun]" → ALWAYS zero predicates → ALWAYS knowledge_edge.
This rule is absolute. No surrounding context changes it:
  - "run tests before release" → zero predicates
  - "run unit tests after refactor" → zero predicates
  - "run the regression suite nightly" → zero predicates
  - "run security tests on the JWT module" → zero predicates (but SECURITY may still fire from domain vocabulary: ["SECURITY"])

---

## Teach-024-003 — TransitionType belongs to IMPLEMENT; TEST alone → documentation

When TEST is the only predicate (via assert/verify/validate/spec):
  TransitionType: documentation — specifying expected behaviour
When TEST + IMPLEMENT fire together:
  TransitionType: feature_addition — constructing new test artifacts
When TEST + SECURITY fire (no IMPLEMENT):
  TransitionType: feature_addition — multi-predicate requests default to feature_addition

---

## Teach-024-004 — Domain service vocabulary: none of the billing domains fire predicates

None of these domain terms fire any predicate:
  CartCalculation, InventoryAllocation, DiscountEngine, TaxCalculation, ShippingCost,
  LoyaltyPoints, FraudScore, PasswordValidator, JwtTokenBuilder, PriceFormatter, CouponApplicator.
  VAT (Value Added Tax), BOGO (Buy One Get One), SKU (Stock Keeping Unit), GST (Goods and Services Tax), GBP (Great British Pound), EUR (Euro), USD (US Dollar).
  rollback, reserve, release, allocate, accrue, redeem, expire — domain verbs, not predicate-firing.
Predicates fire from action verbs and test-infrastructure vocabulary only.
