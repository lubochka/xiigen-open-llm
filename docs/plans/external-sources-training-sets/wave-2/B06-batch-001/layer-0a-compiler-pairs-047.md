# Layer 0-A — Compiler Pairs (047)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## Method note

File 047 is the IMPLEMENT+DOCUMENT combo sweep for files 037-048.
Demonstrates all dual-predicate IMPLEMENT+DOCUMENT patterns across Round 3 domains.

---

## B06-0A-047-001 — IMPLEMENT+DOCUMENT combo: CustomerSegmentation + DynamicPricing

```
source_id: xiigen-b06-0a-047-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the segmentation and dynamic pricing requirements and develop both services."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.91

---

## B06-0A-047-002 — IMPLEMENT+DOCUMENT combo: VolumeDiscount + MembershipTiers

```
source_id: xiigen-b06-0a-047-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the volume discount and membership tier services and document their business rules."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "business" (→ business, Rule 7)
    IMPLEMENT fires on: "Create" (→ create)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-047-003 — IMPLEMENT+DOCUMENT combo: CouponStacking + FreeShipping

```
source_id: xiigen-b06-0a-047-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the coupon stacking resolver and free shipping checker and specify the stacking rules."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specify)
    IMPLEMENT fires on: "Build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-047-004 — IMPLEMENT+DOCUMENT combo: BackorderManagement + PreorderRules

```
source_id: xiigen-b06-0a-047-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the backorder and preorder services and write their functional specifications."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "functional" (→ functional, Rule 11), "specifications" (→ specification)
    IMPLEMENT fires on: "Develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-047-005 — IMPLEMENT+DOCUMENT combo: all Round 3 domains sweep

```
source_id: xiigen-b06-0a-047-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write the requirements for segmentation, dynamic pricing, geofenced pricing, volume discounts, membership tiers, coupon stacking, free shipping, minimum order, backorders, and preorders — and build all corresponding services."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  Domain nouns add zero predicates regardless of count.
  TransitionType: feature_addition
  Confidence: 0.90
