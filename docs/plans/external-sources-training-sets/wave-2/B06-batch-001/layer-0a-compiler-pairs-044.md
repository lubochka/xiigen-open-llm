# Layer 0-A — Compiler Pairs (044)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-044-001 — MinimumOrderValue: DOCUMENT alone

```
source_id: xiigen-b06-0a-044-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the minimum order value requirements including per-channel thresholds, digital vs physical product rules, and cart rejection behavior."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement), "behavior" (→ behavior)
  Non-terms: "minimum order value", "per-channel thresholds", "digital vs physical product", "cart rejection" — domain nouns
  Confidence: 0.92

---

## B06-0A-044-002 — MinimumOrderValue: IMPLEMENT alone

```
source_id: xiigen-b06-0a-044-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the minimum order guard with channel-aware threshold lookup and cart validation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "minimum order guard", "channel-aware threshold lookup", "cart validation"
  Confidence: 0.93

---

## B06-0A-044-003 — MinimumOrderValue: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-044-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the minimum order value specification and build the cart validation service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-044-004 — MinimumOrderValue: knowledge-edge

```
source_id: xiigen-b06-0a-044-004
source_license_state: first_party
pair_kind: compiler
```

A: "Block checkout when the cart total is below the minimum."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Block" is operational. "checkout", "cart total", "minimum" are domain nouns.
  Confidence: 0.95

---

## B06-0A-044-005 — MinimumOrderValue: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-044-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the minimum order value requirements and design the cart validation service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
