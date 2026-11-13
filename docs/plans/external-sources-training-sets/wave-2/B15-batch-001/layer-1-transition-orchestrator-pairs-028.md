# Layer 1 — Transition Orchestrator Pairs (028)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-028-001 — TaxCalculationService: IMPLEMENT single transition

A: "Build a TaxCalculationService to calculate applicable taxes for orders."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1"] | ContextCarryForward: true

---

## B15-t1-028-002 — TaxCalculationService: IMPLEMENT+ARCHITECTURE transition

A: "Build TaxCalculationService with a tax jurisdiction domain model."
B: TransitionType: feature_addition | SubPhaseIds: ["3.1", "4.1"] | ContextCarryForward: true

---

## B15-t1-028-003 — TaxCalculationService: IMPLEMENT+SECURITY transition

A: "Build TaxCalculationService and secure the tax rate configuration endpoint."
B: TransitionType: feature_addition | SubPhaseIds: ["security", "4.1"] | ContextCarryForward: true

---

## B15-t1-028-004 — TaxCalculationService: IMPLEMENT+TEST+DOCUMENT transition

A: "Build TaxCalculationService, write unit tests for all jurisdiction rules, and document the tax calculation formula."
B: TransitionType: feature_addition | SubPhaseIds: ["4.1", "5.1", "2.1"] | ContextCarryForward: true
DOCUMENT (2.1) is parallel-eligible with IMPLEMENT (4.1).

---

## B15-t1-028-005 — SubPhaseIds ordering rule confirmed for Round 1 services

TaxCalculationService, CurrencyConversionService, ExchangeRateService, WalletService, CreditService — all Round 1 services follow the same SubPhaseIds ordering:
["3.1"] → ["3.3"] → ["security"] → ["4.1"] → ["5.1"] or ["5.3"] → ["2.1"] → ["7.1"] or ["7.3"]
