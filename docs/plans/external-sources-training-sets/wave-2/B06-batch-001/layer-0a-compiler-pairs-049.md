# Layer 0-A — Compiler Pairs (049)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-049-001 — ReturnWindowPolicy: DOCUMENT alone

```
source_id: xiigen-b06-0a-049-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the return window policy requirements including standard 30-day window, extended holiday returns, and category-specific exclusions."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "return window policy", "30-day window", "extended holiday returns", "category-specific exclusions" — domain nouns
  Confidence: 0.92

---

## B06-0A-049-002 — ReturnWindowPolicy: IMPLEMENT alone

```
source_id: xiigen-b06-0a-049-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the return window evaluator with category overrides and holiday extension logic."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "return window evaluator", "category overrides", "holiday extension logic"
  Confidence: 0.93

---

## B06-0A-049-003 — ReturnWindowPolicy: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-049-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the return window policy specification and build the eligibility checker service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-049-004 — ReturnWindowPolicy: knowledge-edge

```
source_id: xiigen-b06-0a-049-004
source_license_state: first_party
pair_kind: compiler
```

A: "Check if the order is still within the return window."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Check" is operational. "order", "return window" are domain nouns.
  Confidence: 0.95

---

## B06-0A-049-005 — ReturnWindowPolicy: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-049-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the return window policy requirements and design the returns eligibility service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
