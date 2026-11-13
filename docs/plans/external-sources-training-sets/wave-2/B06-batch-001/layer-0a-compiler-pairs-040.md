# Layer 0-A — Compiler Pairs (040)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-040-001 — VolumeDiscount: DOCUMENT alone

```
source_id: xiigen-b06-0a-040-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the volume discount requirements including quantity break points, tiered percentage off, and SKU (Stock Keeping Unit) eligibility rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
  Non-terms: "volume discount", "quantity break points", "tiered percentage off", "SKU eligibility" — domain nouns
  Confidence: 0.92

---

## B06-0A-040-002 — VolumeDiscount: IMPLEMENT alone

```
source_id: xiigen-b06-0a-040-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the volume discount calculator with break-point evaluation and cart-level discount aggregation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "volume discount calculator", "break-point evaluation", "cart-level discount aggregation"
  Confidence: 0.93

---

## B06-0A-040-003 — VolumeDiscount: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-040-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the volume discount specification and develop the quantity-tier pricing service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-040-004 — VolumeDiscount: knowledge-edge

```
source_id: xiigen-b06-0a-040-004
source_license_state: first_party
pair_kind: compiler
```

A: "Calculate the discount when a customer buys more than 10 units."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Calculate" is operational. "discount", "customer", "units" are domain nouns.
  Confidence: 0.95

---

## B06-0A-040-005 — VolumeDiscount: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-040-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the volume discount requirements and design the pricing rule engine architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
