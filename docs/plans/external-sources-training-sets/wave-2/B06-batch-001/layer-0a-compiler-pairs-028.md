# Layer 0-A — Compiler Pairs (028)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-028-001 — BundlePricing: DOCUMENT alone

```
source_id: xiigen-b06-0a-028-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the bundle pricing rules including fixed-price bundles, percentage-off bundles, and buy-one-get-one scenarios."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "rules" context
  Non-terms: "bundle", "pricing", "fixed-price", "percentage-off", "buy-one-get-one"
  Confidence: 0.92

---

## B06-0A-028-002 — BundlePricing: IMPLEMENT alone

```
source_id: xiigen-b06-0a-028-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the bundle price calculator with support for nested discounts and SKU (Stock Keeping Unit) group matching."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "bundle", "calculator", "nested discounts", "SKU group matching"
  Confidence: 0.93

---

## B06-0A-028-003 — BundlePricing: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-028-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Document the bundle pricing specification and develop the discount computation engine."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "specification" (→ specification)
    IMPLEMENT fires on: "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-028-004 — BundlePricing: knowledge-edge

```
source_id: xiigen-b06-0a-028-004
source_license_state: first_party
pair_kind: compiler
```

A: "Apply bundle discounts to cart items when customers add qualifying SKU combinations."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Apply" is operational. "bundle discounts", "cart items", "SKU combinations" are domain nouns.
  Confidence: 0.95

---

## B06-0A-028-005 — BundlePricing: "narrative" fires DOCUMENT

```
source_id: xiigen-b06-0a-028-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write a narrative description of bundle pricing rules for the product team."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "narrative" (→ narrative), "description" context
    IMPLEMENT fires on: "Write" (→ write)
  Teaching: "narrative" fires DOCUMENT via Rule 13; "Write" fires IMPLEMENT simultaneously.
  TransitionType: feature_addition
  Confidence: 0.91
