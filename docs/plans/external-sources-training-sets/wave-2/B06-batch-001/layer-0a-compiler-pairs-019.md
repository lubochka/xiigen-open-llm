# Layer 0-A — Compiler Pairs (019)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 019 covers BundleAndDiscount requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on bundle and discount domain vocabulary.

---

## B06-0A-019-001 — BundleAndDiscount: "define bundle discount requirements"

```
source_id: xiigen-b06-0a-019-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the bundle and discount requirements for checkout, covering percentage discounts, fixed-amount coupons, buy-one-get-one rules, and minimum order thresholds."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-019-002 — BundleAndDiscount: "document discount behavior"

```
source_id: xiigen-b06-0a-019-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the discount application behavior during checkout, including stacking rules, priority order, and maximum discount limits."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-019-003 — BundleAndDiscount: "specify bundle feature-brief"

```
source_id: xiigen-b06-0a-019-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the feature-brief for product bundle checkout, covering bundle composition rules, quantity overrides, and partial-bundle pricing."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "feature-brief" (→ feature-brief)
  Confidence: 0.92

---

## B06-0A-019-004 — BundleAndDiscount: "write discount spec" (dual)

```
source_id: xiigen-b06-0a-019-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the discount engine specification, including the business rules for coupon validation, exclusion lists, and real-time price recalculation."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification), "business rules" (→ business, requirement)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92

---

## B06-0A-019-005 — BundleAndDiscount: "describe discount use cases and narrative"

```
source_id: xiigen-b06-0a-019-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases and narrative for discount application, including edge cases for expired coupons, invalid codes, and discount conflicts."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case),
      "narrative" (→ narrative)
  Confidence: 0.92
