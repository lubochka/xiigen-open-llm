# Layer 0-A — Compiler Pairs (056)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-056-001 — PriceMatchPolicy: DOCUMENT alone

```
source_id: xiigen-b06-0a-056-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the price match policy requirements including eligible competitor sources, verification window, and one-time-per-order limit."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "price match policy", "eligible competitor sources", "verification window", "one-time-per-order limit" — domain nouns
  Confidence: 0.92

---

## B06-0A-056-002 — PriceMatchPolicy: IMPLEMENT alone

```
source_id: xiigen-b06-0a-056-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the price match request handler with competitor price lookup and adjustment approval workflow."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "price match request handler", "competitor price lookup", "adjustment approval workflow"
  Confidence: 0.93

---

## B06-0A-056-003 — PriceMatchPolicy: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-056-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the price match policy specification and build the price adjustment service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-056-004 — PriceMatchPolicy: knowledge-edge

```
source_id: xiigen-b06-0a-056-004
source_license_state: first_party
pair_kind: compiler
```

A: "Match the competitor price when the customer submits a valid price match request."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Match" is operational. "competitor price", "customer", "price match request" are domain nouns.
  Confidence: 0.95

---

## B06-0A-056-005 — PriceMatchPolicy: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-056-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the price match policy requirements and design the competitor price verification architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
