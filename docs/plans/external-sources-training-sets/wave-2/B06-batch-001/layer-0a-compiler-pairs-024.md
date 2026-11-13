# Layer 0-A — Compiler Pairs (024)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 024 is the multi-predicate sweep and complete rule reference for files 013-024.
Covers all new domains (shipping, inventory, loyalty, return/refund, tax, multi-currency,
bundle/discount, subscription, fraud, data-privacy) with a consolidated rule table.

---

## B06-0A-024-001 — Multi-predicate: all new domains, DOCUMENT sweep

```
source_id: xiigen-b06-0a-024-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define, describe, and specify the requirements, use cases, and behavior for shipping, inventory, loyalty, return and refund, tax, multi-currency, bundle and discount, subscription billing, fraud detection, and data privacy within the checkout flow."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "describe" (→ describe), "specify" (→ specification),
      "requirements" (→ requirement), "use cases" (→ use-case), "behavior" (→ behavior)
  Note: "define", "describe", "specify" are all DOCUMENT verbs — not IMPLEMENT.
  All ten domain words (shipping, inventory, loyalty, etc.) contain no predicate terms alone.
  Confidence: 0.92

---

## B06-0A-024-002 — Multi-predicate: DOCUMENT+IMPLEMENT sweep with new domains

```
source_id: xiigen-b06-0a-024-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the requirements and specification for shipping, loyalty program, tax calculation, subscription billing, and fraud detection in the checkout flow."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement), "specification" (→ specification)
    IMPLEMENT fires on: "write" (→ write)
  TransitionType: feature_addition.
  Confidence: 0.92

---

## B06-0A-024-003 — Complete DOCUMENT predicate rule reference for files 013-024

```
source_id: xiigen-b06-0a-024-003
source_license_state: first_party
pair_kind: compiler
```

Extended B06 DOCUMENT term coverage across files 013-024:

  File | Domain              | DOCUMENT terms demonstrated
  -----|---------------------|-----------------------------
  013  | ShippingRequirements    | define, requirements, behavior, specification, use-case, functional, criteria
  014  | InventoryManagement     | define, requirements, describe, functional, behavior, use-case, spec, epics, capability
  015  | LoyaltyProgram          | define, requirements, document, narrative, describe, specify, feature-brief, criteria, story, stakeholder, behavior
  016  | ReturnAndRefund         | define, requirements, describe, functional, behavior, use-case, specification, business, epics, capability
  017  | TaxCalculation          | define, requirements, specify, functional, behavior, describe, use-case, document, narrative
  018  | MultiCurrency           | define, requirements, document, behavior, specify, specification, describe, functional, criteria
  019  | BundleAndDiscount       | define, requirements, document, behavior, specify, feature-brief, describe, use-case, narrative
  020  | SubscriptionBilling     | define, requirements, document, functional, behavior, describe, use-case, specify, epics, capability
  021  | FraudDetection          | define, requirements, document, behavior, criteria, describe, use-case, specify, specification
  022  | DataPrivacy             | define, requirements, document, behavior, describe, use-case, specify, stakeholder, specification
  023  | DOCUMENT+IMPLEMENT dual | All IMPLEMENT verbs: write, create, build, develop
  024  | Multi-domain sweep      | define, describe, specify, requirements, use-case, behavior, specification

  All 17 DOCUMENT lexicon terms covered across files 013-024.

---

## B06-0A-024-004 — Predicate non-terms in new domains (024)

```
source_id: xiigen-b06-0a-024-004
source_license_state: first_party
pair_kind: compiler
```

Domain words that do NOT fire any predicate:
  shipping, carrier, delivery, inventory, stock, reservation, loyalty, points, accrual,
  redemption, refund, return, tax, VAT (Value Added Tax), GST (Goods and Services Tax),
  currency, FX (Foreign Exchange), exchange, bundle, coupon, discount, subscription,
  recurring, fraud, chargeback, privacy, GDPR (General Data Protection Regulation),
  PII (Personally Identifiable Information), consent, erasure

Teaching point: Domain nouns never fire predicates alone. Only the 17 DOCUMENT terms,
and the IMPLEMENT/ARCHITECTURE/UI lexicon terms fire predicates. Domain nouns provide
context but do not change the predicate result.

---

## B06-0A-024-005 — TransitionType rule reference for files 013-024

```
source_id: xiigen-b06-0a-024-005
source_license_state: first_party
pair_kind: compiler
```

TransitionType determination across all files 013-024:

  DOCUMENT alone → TransitionType: documentation
  DOCUMENT + IMPLEMENT → TransitionType: feature_addition
  DOCUMENT + ARCHITECTURE → TransitionType: documentation (ARCHITECTURE alone does not trigger feature_addition)
  DOCUMENT + IMPLEMENT + ARCHITECTURE → TransitionType: feature_addition

  Files 013-024 TransitionType summary:
    013: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    014: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    015: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    016: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    017: pairs 1-3,5 = documentation; pair 4 = feature_addition (create)
    018: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    019: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    020: pairs 1-3,5 = documentation; pair 4 = feature_addition (write)
    021: pairs 1-4 = documentation; pair 5 = feature_addition (create)
    022: pairs 1-4 = documentation; pair 5 = feature_addition (write)
    023: all 5 = feature_addition (all pairs have IMPLEMENT verb)
    024: pair 1 = documentation; pair 2 = feature_addition
