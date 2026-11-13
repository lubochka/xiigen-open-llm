# Testing Pairs — Sub-phase 5.1 Unit Test Design
# Batch: dpo-pairs-testing-batch-002
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: isolated test cases, local behavior checks, component-level verification
# Pair count: 5

---

## Pair AP2-5.1-001 — pure-logic unit tests confused with database integration tests

```
source_id: xiigen-testing-batch-002-AP2-5.1-001
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.1
rejected_sub_phase: 5.2
```

Prompt: `"design unit tests for the discount calculation rules — bulk-tier, loyalty-tier, and seasonal-promotion stack"`

```
CHOSEN — 5.1 Unit Test Design:
DiscountCalculator is a pure function. Every test runs in-memory with
constructed inputs and asserted outputs. No I/O, no fakes, no fixtures.

Test catalogue (12 cases):

Bulk tier alone:
  T1 — quantity 1 → no bulk discount.
  T2 — quantity 9 → no bulk discount (under threshold).
  T3 — quantity 10 → 5% discount (lower threshold).
  T4 — quantity 99 → 10% discount (middle tier).
  T5 — quantity 100 → 15% discount (upper threshold; the boundary case).

Loyalty tier alone:
  T6 — silver loyalty → 3% discount.
  T7 — gold loyalty → 7% discount.
  T8 — no loyalty → no loyalty discount.

Stacking rules:
  T9 — bulk 10% + silver 3% → 13% combined (additive on this product).
  T10 — bulk 15% + seasonal 10% → 22.5% combined (multiplicative when
        seasonal is present; the rule the calculator must encode).
  T11 — bulk 0% + loyalty 7% + seasonal 10% → 16.3% combined (loyalty
        and seasonal multiply when stacked).

Edge case:
  T12 — combined discount > 50% → capped at 50% with a CapApplied flag.

Each test:
  Arrange — construct the calculator and the input bundle.
  Act — call Calculate(bundle).
  Assert — Equal expected_total, Equal expected_discount_breakdown.

Isolation: no DbContext, no HTTP client, no clock, no logger. The calculator
takes inputs and returns outputs.

REJECTED — 5.2 Integration Tests:
Spin up a Testcontainers SQL Server instance for each test class. Seed it
with a Products table, a LoyaltyTiers table, and a SeasonalPromotions table.
For each scenario, insert the test data, call DiscountCalculator (which now
queries the database for the current rules), and assert the result. Use
Respawn to reset the database between tests. Configure xUnit's IClassFixture
to share the container across tests in a class.
[WRONG: discount calculation is pure logic over input data — no database is
required to test it. Spinning up a SQL container per class adds 30 seconds
of startup, requires Docker available in CI, and tests the same logic with
extra failure modes (container start fails, seed query fails, network glitch).
The unit tests must run in-memory and complete in milliseconds. Integration
tests with Testcontainers are appropriate when the code being tested actually
queries the database; this calculator does not, and a unit test is the
correct shape.]
```

---

## Pair AP2-5.1-002 — function-level test confused with end-to-end coverage

```
source_id: xiigen-testing-batch-002-AP2-5.1-002
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.1
rejected_sub_phase: 5.3
```

Prompt: `"write unit tests for the email-template rendering function — substitution, missing-variable handling, and HTML escape behavior"`

```
CHOSEN — 5.1 Unit Test Design:
EmailTemplateRenderer.Render(template, model) is a pure function from
(string, IDictionary<string, object>) to string. Tests construct templates
and models, call Render, and assert the output string.

Test catalogue:

Substitution:
  T1 — template "Hi {{name}}" + model { name = "Alice" } → "Hi Alice".
  T2 — template "Hi {{name}}, welcome to {{product}}" + model with both →
        all variables substituted in order.
  T3 — template with the same variable twice → both occurrences substituted.

Missing-variable behaviour:
  T4 — template "Hi {{name}}" + empty model → throws
        TemplateRenderException with the missing variable name in the
        Errors property.
  T5 — template with a typo'd variable name → throws and identifies the
        unrecognised variable.
  T6 — template with a variable in a comment block "{{!-- {{secret}}
        --}}" → secret is not flagged as missing because comment blocks
        are not substituted.

HTML escape behaviour:
  T7 — model value "Alice & Bob" rendered into an HTML template → output
        contains "Alice &amp; Bob" (escape applied).
  T8 — model value "<script>" rendered into an HTML template → output
        contains "&lt;script&gt;" (escape applied).
  T9 — model value containing "<" rendered into a plain-text template →
        output contains "<" (no escape applied; the template's content type
        controls escaping).

Each test runs in-memory in under 5 ms. The assertion is on the rendered
string, not on the rendering process.

REJECTED — 5.3 End-to-End Scenario Testing:
Use Playwright to drive the full registration flow that triggers a welcome
email. After registration, poll the Mailtrap API for the email. Read the
HTML body and assert that it contains the expected name. Repeat for missing-
variable and HTML-escape cases by triggering registration scenarios that
exercise the template renderer. Run the suite as part of the nightly
end-to-end pipeline.
[WRONG: testing the renderer's behaviour through a full registration flow
adds many failure modes (registration breaks, email queue stalls, Mailtrap
returns a stale message) that have nothing to do with the renderer. A unit
test on the pure Render function isolates the behaviour under test and
runs in milliseconds. End-to-end tests verify that the integrated system
delivers the right email; they are the wrong scale for verifying the
substitution and escape rules of a pure function.]
```

---

## Pair AP2-5.1-003 — pure-logic test confused with manual review

```
source_id: xiigen-testing-batch-002-AP2-5.1-003
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.1
rejected_sub_phase: 6.1
```

Prompt: `"design unit tests for the password-policy validator — minimum length, character classes, and breach-list rejection"`

```
CHOSEN — 5.1 Unit Test Design:
PasswordPolicyValidator.Validate(password) returns a ValidationResult with
IsValid and a list of failed rules. Tests are pure-input pure-output.

Test catalogue (per rule, with edge cases):

Minimum length (12 characters):
  T1 — 11 chars → IsValid=false, FailedRules contains "min_length".
  T2 — 12 chars → "min_length" not in FailedRules.
  T3 — 200 chars → "min_length" not in FailedRules; no upper-bound rule.

Character classes (must contain ≥3 of: lowercase, uppercase, digit, symbol):
  T4 — only lowercase → "char_class_min" in FailedRules.
  T5 — lowercase + uppercase only → "char_class_min" in FailedRules.
  T6 — lowercase + uppercase + digit → "char_class_min" not in FailedRules.
  T7 — all four classes → "char_class_min" not in FailedRules.

Breach-list rejection (constructor injects an in-memory breach set):
  T8 — password "Password123!" with the breach set containing
        "password123" → "breach_match" in FailedRules
        (case-insensitive match).
  T9 — password "T7&fmKn4ZpQ!" with empty breach set → "breach_match"
        not in FailedRules.
  T10 — null breach set → constructor argument null check; no test
        against the validate method itself.

Composite cases:
  T11 — short password that also fails breach → both "min_length" and
        "breach_match" appear; the validator does not short-circuit on
        the first failure.

The breach set is an in-memory ISet<string> injected through the
constructor. No database, no HTTP call to HaveIBeenPwned; that adapter is
tested in its own integration test (5.2 territory).

REJECTED — 6.1 Manual QA and Acceptance Review:
The QA reviewer creates a test plan covering common weak passwords
("password", "12345678", "qwerty"), strong passwords (T7&fmKn4ZpQ!),
edge cases (exactly 12 chars, 11 chars, 200 chars), and breach-list cases.
For each, the reviewer manually enters the password in the registration
form and records whether the validator accepts or rejects it. The reviewer
documents findings and signs off if all expected outcomes match.
[WRONG: password validation rules are pure logic — they take an input and
return an output deterministically. Unit tests for this can run thousands
of cases in milliseconds. A manual QA reviewer who hand-enters 11 cases
in the registration form takes hours, exercises only a few cases, and
catches none of the edge cases that pure-logic unit tests cover trivially.
Manual review is the gate for human-judgment concerns (visual rendering,
real email delivery, exploratory testing); it is not the gate for
deterministic-rule verification.]
```

---

## Pair AP2-5.1-004 — value-object test confused with full repository test

```
source_id: xiigen-testing-batch-002-AP2-5.1-004
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.1
rejected_sub_phase: 5.2
```

Prompt: `"design unit tests for the Money value type — addition, subtraction, currency mismatch, and rounding behavior"`

```
CHOSEN — 5.1 Unit Test Design:
Money is an immutable value type carrying (Amount, Currency, Scale). All
operations return a new Money or throw. Tests construct Money instances,
perform operations, and assert the result.

Test catalogue:

Construction:
  T1 — new Money(100, "USD") → Amount=100, Currency=USD, Scale=2 (default).
  T2 — new Money(100, "JPY") → Scale=0 (Japanese yen has no minor units).
  T3 — new Money(-50, "USD") → constructed without exception (overdraft
        amounts are valid Money).
  T4 — new Money(decimal.MaxValue, "USD") → constructed without exception.

Equality:
  T5 — new Money(100, "USD").Equals(new Money(100, "USD")) → true.
  T6 — new Money(100, "USD").Equals(new Money(100, "EUR")) → false.
  T7 — new Money(1.00m, "USD").Equals(new Money(1, "USD")) → true (scale
        normalisation on equality).

Addition:
  T8 — Money(100, USD) + Money(50, USD) → Money(150, USD).
  T9 — Money(100, USD) + Money(50, EUR) → throws CurrencyMismatchException.
  T10 — Money(0.10m, USD) + Money(0.20m, USD) → Money(0.30m, USD); no
        floating-point drift because Money uses decimal.

Subtraction:
  T11 — Money(100, USD) - Money(150, USD) → Money(-50, USD); no
        unsigned-only constraint.
  T12 — Money(100, USD) - Money(50, EUR) → throws CurrencyMismatchException.

Rounding:
  T13 — Money(1.005m, USD).Round() → Money(1.01m, USD); banker's rounding
        on 0.005 boundaries (the rule the type encodes).
  T14 — Money(1.005m, JPY).Round() → Money(1, JPY); JPY scale is 0.

Each test runs in microseconds. No fixtures, no setup, no DbContext.

REJECTED — 5.2 Integration Tests:
Set up a Testcontainers SQL Server instance with an Orders table and an
OrderLines table. Insert orders with various money amounts and currencies.
For each test scenario, insert the data, query it back through the EF Core
DbContext, perform the Money operation, and assert the persisted result.
Use Respawn to reset between tests. Verify that EF Core's value converter
correctly serialises and deserialises Money values.
[WRONG: testing a value type's arithmetic and equality is pure-logic
verification. The integration test setup adds 30+ seconds per class, requires
Docker in CI, and tests EF Core's value converter at the same time as the
arithmetic — meaning a failure cannot be attributed cleanly to the
arithmetic or to the persistence. Unit tests in milliseconds prove the type's
contract; integration tests for the EF Core converter live in their own
test class with their own purpose.]
```

---

## Pair AP2-5.1-005 — algorithm test confused with backend service implementation

```
source_id: xiigen-testing-batch-002-AP2-5.1-005
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.1
rejected_sub_phase: 4.1
```

Prompt: `"design unit tests for the rate-limiter token-bucket algorithm — refill rate, burst capacity, and clock-skew tolerance"`

```
CHOSEN — 5.1 Unit Test Design:
TokenBucket is a state machine over (capacity, refillPerSecond, clock).
Tests inject a controllable IClock to advance time deterministically.

Test catalogue:

Initial state:
  T1 — fresh bucket(capacity=100, refill=10/s) → TryConsume(50) succeeds,
        Tokens after = 50.
  T2 — fresh bucket → TryConsume(100) succeeds, Tokens after = 0.
  T3 — fresh bucket → TryConsume(101) fails, Tokens unchanged.

Refill:
  T4 — bucket emptied at t=0; clock advances to t=1s → Tokens = 10.
  T5 — bucket emptied at t=0; clock advances to t=10s → Tokens = 100
        (refill capped at capacity).
  T6 — bucket emptied at t=0; clock advances to t=20s → Tokens = 100
        (still capped; no overflow).

Burst:
  T7 — full bucket; rapid TryConsume(1) × 100 in 0 ms simulated → all
        100 succeed (burst supports the full capacity).
  T8 — full bucket; rapid TryConsume(1) × 101 in 0 ms simulated → first
        100 succeed, the 101st fails.

Clock skew:
  T9 — clock returns the same value twice in a row → no negative
        elapsed; refill is 0, not negative.
  T10 — clock returns a value earlier than the previous call → refill
        is 0; the bucket does not lose tokens to a backwards-moving clock.

Concurrency:
  T11 — two threads each call TryConsume(50) on a 100-token bucket → both
        succeed (sum to 100); two TryConsume(60) calls → exactly one
        succeeds (the algorithm's guarantee under concurrent access).

The IClock is a fake that returns whatever the test sets. No real clock,
no Thread.Sleep, no time-of-day in any assertion. Concurrency tests use
synchronised barriers, not Task.Delay.

REJECTED — 4.1 Backend Services:
Implement RateLimiterService using IDistributedCache as the token-bucket
store. Inject IConnectionMultiplexer for Redis. Implement the bucket
operations with Lua scripts to avoid race conditions across instances.
Add Polly retry for transient cache failures. Register as singleton.
Document the configuration for capacity and refill in appsettings.json.
[WRONG: "design unit tests" asks for the test cases that prove the
algorithm's correctness. Implementing the production service is a different
sub-phase (4.1). The unit tests for the algorithm's pure logic must exist
independently of any specific persistence backend. Tests against a Redis-
backed implementation conflate two failure modes — the algorithm and the
distributed-cache infrastructure — and cannot run in milliseconds. The
algorithm needs unit tests; the Redis-backed service needs integration
tests in its own sub-phase.]
```
