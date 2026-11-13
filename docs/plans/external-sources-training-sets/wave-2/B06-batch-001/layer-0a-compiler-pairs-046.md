# Layer 0-A — Compiler Pairs (046)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-046-001 — PreorderRules: DOCUMENT alone

```
source_id: xiigen-b06-0a-046-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the preorder requirements including deposit capture rules, release date charge triggering, and preorder cancellation policies."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "preorder", "deposit capture", "release date charge", "cancellation policies" — domain nouns
  Confidence: 0.92

---

## B06-0A-046-002 — PreorderRules: IMPLEMENT alone

```
source_id: xiigen-b06-0a-046-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the preorder lifecycle manager with deposit authorization, scheduled charge, and release-day fulfillment trigger."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "preorder lifecycle manager", "deposit authorization", "scheduled charge", "release-day fulfillment trigger"
  Confidence: 0.93

---

## B06-0A-046-003 — PreorderRules: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-046-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the preorder specification and develop the release-date charge service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-046-004 — PreorderRules: knowledge-edge

```
source_id: xiigen-b06-0a-046-004
source_license_state: first_party
pair_kind: compiler
```

A: "Charge the remaining balance when the preorder item ships."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Charge" is operational. "remaining balance", "preorder item" are domain nouns.
  Confidence: 0.95

---

## B06-0A-046-005 — PreorderRules: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-046-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the preorder rules requirements and design the payment scheduling architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
