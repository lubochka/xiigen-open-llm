# Layer 0-A — Compiler Pairs (051)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-051-001 — SplitPayment: DOCUMENT alone

```
source_id: xiigen-b06-0a-051-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the split payment requirements including maximum payment method count, partial capture rules, and failure fallback behavior."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement), "behavior" (→ behavior)
  Non-terms: "split payment", "maximum payment method count", "partial capture", "failure fallback" — domain nouns
  Confidence: 0.92

---

## B06-0A-051-002 — SplitPayment: IMPLEMENT alone

```
source_id: xiigen-b06-0a-051-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the split payment coordinator with multi-method authorization and partial capture management."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "split payment coordinator", "multi-method authorization", "partial capture management"
  Confidence: 0.93

---

## B06-0A-051-003 — SplitPayment: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-051-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the split payment specification and develop the payment method coordinator."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-051-004 — SplitPayment: knowledge-edge

```
source_id: xiigen-b06-0a-051-004
source_license_state: first_party
pair_kind: compiler
```

A: "Split the order total across two payment methods at checkout."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Split" is operational. "order total", "payment methods", "checkout" are domain nouns.
  Confidence: 0.95

---

## B06-0A-051-005 — SplitPayment: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-051-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the split payment requirements and design the multi-method payment architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
