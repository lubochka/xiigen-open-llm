# Layer 0-A — Compiler Pairs (042)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-042-001 — CouponStackingRules: DOCUMENT alone

```
source_id: xiigen-b06-0a-042-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the coupon stacking requirements including which coupon types can combine, priority order of application, and maximum total discount caps."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "coupon stacking", "coupon types", "priority order", "maximum total discount caps" — domain nouns
  Confidence: 0.92

---

## B06-0A-042-002 — CouponStackingRules: IMPLEMENT alone

```
source_id: xiigen-b06-0a-042-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the coupon stacking resolver with conflict detection and ordered discount application."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "coupon stacking resolver", "conflict detection", "ordered discount application"
  Confidence: 0.93

---

## B06-0A-042-003 — CouponStackingRules: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-042-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the coupon stacking business rules specification and develop the discount combination service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "business" (→ business, Rule 7), "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-042-004 — CouponStackingRules: knowledge-edge

```
source_id: xiigen-b06-0a-042-004
source_license_state: first_party
pair_kind: compiler
```

A: "Apply all active coupons to the cart in sequence."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Apply" is operational. "active coupons", "cart" are domain nouns.
  Confidence: 0.95

---

## B06-0A-042-005 — CouponStackingRules: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-042-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the coupon stacking requirements and design the promotion engine architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
