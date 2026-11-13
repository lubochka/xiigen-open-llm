# Layer 0-A — Compiler Pairs (036)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-036-001 — Complete IMPLEMENT reference: Round 2 services

IMPLEMENT fires on: build, implement, create, scaffold, bootstrap, add (code artifact), write (code), extend, develop, code, generate, refactor, process.
Round 2 services confirmed: PaymentGatewayService, SubscriptionService, InvoiceService, TaxCalculationService, CurrencyConversionService, ExchangeRateService, WalletService, CreditService, BillingCycleService, ProrationService, ChargebackService all fire IMPLEMENT when any IMPLEMENT trigger verb is present.

---

## B15-compiler-036-002 — TransitionType rules: Round 2 confirmation

| Service | Verb | TransitionType |
|---------|------|---------------|
| PaymentGatewayService | build | feature_addition |
| SubscriptionService | refactor | refactoring |
| ExchangeRateService | extend | feature_addition |
| BillingCycleService | refactor + extend | refactoring (refactor wins) |
| ChargebackService | implement | feature_addition |

---

## B15-compiler-036-003 — "process" fires IMPLEMENT

"process" as a verb describing code artifact creation fires IMPLEMENT. Example: "Process chargeback disputes" → IMPLEMENT. Teaching: "process" in the context of building functionality = IMPLEMENT trigger.

---

## B15-compiler-036-004 — Deduplication across Round 2

All deduplication rules from 001-024 apply to Round 2 services. Examples:
- "build and scaffold PaymentGatewayService" → ["IMPLEMENT"] (not two IMPLEMENT entries)
- "implement and refactor ChargebackService" → ["IMPLEMENT"] | refactoring TransitionType
- "integrate and wire up exchange rate provider" → ["API_CONTRACT"] (not two API_CONTRACT entries)

---

## B15-compiler-036-005 — "payment" / "billing" / "invoice" do NOT fire predicates

"payment", "billing", "invoice", "chargeback", "proration", "subscription", "wallet" are domain nouns.
None fire any predicate. IMPLEMENT fires only from action verbs (build/create/implement etc.).
v6-01 risk: treating domain nouns as unknown predicates and returning [] is incorrect.
