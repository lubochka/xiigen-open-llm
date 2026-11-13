# Layer 2 — Nine-Node Flow Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: BddCheckoutScenarios | Predicate: TEST alone | TransitionType: documentation

---

## B39-nine-016 — BDD checkout scenarios — PASS

Request: "Write BDD scenarios in Gherkin for the checkout flow: Given-When-Then for each payment method."
Predicate set: ["TEST"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 (TEST alone — no UI component vocabulary fired)

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Checkout flow — credit card payment, PayPal payment, bank transfer payment.
Domains: credit_card_scenario, paypal_scenario, bank_transfer_scenario.
Note: TEST alone fires (no UI vocabulary in the request). UIInteractionFlow15 not instantiated.

**Node 2 — DomainTranslator**:
  credit_card_scenario: Given valid card details, When user submits checkout, Then order confirmation shown.
  paypal_scenario: Given user clicks PayPal button, When redirect returns with approval, Then order confirmation shown.
  bank_transfer_scenario: Given user selects bank transfer, When user submits, Then "pending payment" status shown.

**Node 3 — SchemePlanner**:
  Format: Gherkin (.feature files) | Runner: Cucumber / SpecFlow
  Scenarios: 3 payment methods × 2 paths (happy path + failure path) = 6 scenarios.

**Node 4 — DomainProcessor**:
  Scenario 1: "Successful credit card payment"
    Given the user has items in cart and valid card details
    When the user clicks "Place Order"
    Then the order confirmation page is shown
    And the order ID is displayed
  Scenario 2: "Failed credit card payment (declined)"
    Given the user has items in cart and a declined card
    When the user clicks "Place Order"
    Then the error message "Payment declined" is displayed
  Scenario 3: "PayPal payment approved"
    Given the user selects PayPal as payment method
    When PayPal returns approval_status=approved
    Then the order confirmation page is shown
  [3 more scenarios for PayPal failure and bank transfer paths]

**Node 5 — Applier**: Gherkin format; Given-When-Then; one .feature file per payment method; step definitions separate.

**Node 6 — ResultTranslator**: 6 scenarios in 3 feature files. Shared step definitions: 8 steps reused across scenarios.

**Node 7 — ArbiterSelector**: BDDScenarioCoverageArbiter — each payment method needs happy path + failure path.

**Node 8 — Arbiter**:
  credit_card: happy ✓, failure ✓. PASS.
  paypal: happy ✓, failure ✓. PASS.
  bank_transfer: happy ✓; failure (bank rejected) — missing.
  OverallVerdict: PASS_WITH_WARNINGS — bank transfer failure scenario missing.

**Node 9 — Formatter**: Output: checkout-credit-card.feature, checkout-paypal.feature, checkout-bank-transfer.feature. 6 scenarios. Gap: bank_transfer_failure. Status: ready_with_gaps.
