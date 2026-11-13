# Layer 0-A — Compiler Pairs (038)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-038-001 — DynamicPricing: DOCUMENT alone

```
source_id: xiigen-b06-0a-038-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the dynamic pricing requirements including demand-based adjustments, competitor price tracking, and price floor and ceiling rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
  Non-terms: "dynamic pricing", "demand-based adjustments", "competitor price tracking", "price floor" — domain nouns
  Confidence: 0.92

---

## B06-0A-038-002 — DynamicPricing: IMPLEMENT alone

```
source_id: xiigen-b06-0a-038-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the dynamic pricing engine with real-time competitor feed ingestion and margin-aware price adjustment."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "dynamic pricing engine", "competitor feed ingestion", "margin-aware"
  Confidence: 0.93

---

## B06-0A-038-003 — DynamicPricing: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-038-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the dynamic pricing specification and build the price-adjustment service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-038-004 — DynamicPricing: knowledge-edge

```
source_id: xiigen-b06-0a-038-004
source_license_state: first_party
pair_kind: compiler
```

A: "Recalculate product prices every hour based on demand signals."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Recalculate" is operational. "product prices", "demand signals" are domain nouns.
  Confidence: 0.95

---

## B06-0A-038-005 — DynamicPricing: DOCUMENT + ARCHITECTURE + IMPLEMENT

```
source_id: xiigen-b06-0a-038-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the dynamic pricing requirements, design the pricing engine architecture, and implement the demand-based adjustment module."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
    IMPLEMENT fires on: "Write" (→ write), "implement" (→ implement)
  TransitionType: feature_addition
  Confidence: 0.91
