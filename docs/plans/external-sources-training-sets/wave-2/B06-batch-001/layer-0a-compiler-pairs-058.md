# Layer 0-A — Compiler Pairs (058)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-058-001 — AutoReplenishment: DOCUMENT alone

```
source_id: xiigen-b06-0a-058-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the auto-replenishment requirements including subscription frequency options, skip-delivery rules, and payment method update policies."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "auto-replenishment", "subscription frequency options", "skip-delivery rules", "payment method update policies" — domain nouns
  Confidence: 0.92

---

## B06-0A-058-002 — AutoReplenishment: IMPLEMENT alone

```
source_id: xiigen-b06-0a-058-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the auto-replenishment scheduler with recurring order creation and payment retry logic."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "auto-replenishment scheduler", "recurring order creation", "payment retry logic"
  Confidence: 0.93

---

## B06-0A-058-003 — AutoReplenishment: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-058-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the auto-replenishment specification and build the subscription order service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-058-004 — AutoReplenishment: knowledge-edge

```
source_id: xiigen-b06-0a-058-004
source_license_state: first_party
pair_kind: compiler
```

A: "Automatically reorder the product every 30 days for the subscribed customer."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Reorder" is operational. "product", "subscribed customer" are domain nouns.
  Confidence: 0.95

---

## B06-0A-058-005 — AutoReplenishment: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-058-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the auto-replenishment requirements and design the recurring order scheduler architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
