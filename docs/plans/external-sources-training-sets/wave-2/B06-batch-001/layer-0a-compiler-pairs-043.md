# Layer 0-A — Compiler Pairs (043)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-043-001 — FreeShippingThreshold: DOCUMENT alone

```
source_id: xiigen-b06-0a-043-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the free shipping threshold requirements including minimum order value, eligible product categories, and geographic zone restrictions."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "free shipping threshold", "minimum order value", "eligible product categories", "geographic zone" — domain nouns
  Confidence: 0.92

---

## B06-0A-043-002 — FreeShippingThreshold: IMPLEMENT alone

```
source_id: xiigen-b06-0a-043-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the free shipping eligibility checker with cart total evaluation and zone lookup."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "free shipping eligibility checker", "cart total evaluation", "zone lookup"
  Confidence: 0.93

---

## B06-0A-043-003 — FreeShippingThreshold: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-043-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the free shipping threshold specification and build the cart-total validation service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-043-004 — FreeShippingThreshold: knowledge-edge

```
source_id: xiigen-b06-0a-043-004
source_license_state: first_party
pair_kind: compiler
```

A: "Waive shipping costs when the cart reaches the threshold."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Waive" is operational. "shipping costs", "cart", "threshold" are domain nouns.
  Confidence: 0.95

---

## B06-0A-043-005 — FreeShippingThreshold: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-043-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the free shipping threshold requirements and design the shipping eligibility service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
