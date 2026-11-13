# Layer 0-A — Compiler Pairs (030)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-030-001 — DropshippingRules: DOCUMENT alone

```
source_id: xiigen-b06-0a-030-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the dropshipping fulfillment rules including supplier routing, split shipment policies, and out-of-stock fallback."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "rules" context, "policies" context
  Non-terms: "dropshipping", "supplier routing", "split shipment", "out-of-stock fallback"
  Confidence: 0.91

---

## B06-0A-030-002 — DropshippingRules: IMPLEMENT alone

```
source_id: xiigen-b06-0a-030-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the supplier order routing service with automatic failover when primary supplier is unavailable."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "supplier", "order routing", "automatic failover", "primary supplier"
  Confidence: 0.93

---

## B06-0A-030-003 — DropshippingRules: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-030-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the dropshipping rules specification and create the supplier selection module."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "create" (→ create)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-030-004 — DropshippingRules: knowledge-edge

```
source_id: xiigen-b06-0a-030-004
source_license_state: first_party
pair_kind: compiler
```

A: "Route orders to the nearest available dropship supplier."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Route" is operational. "orders", "nearest", "dropship supplier" are domain nouns.
  Confidence: 0.95

---

## B06-0A-030-005 — DropshippingRules: "capability" fires DOCUMENT

```
source_id: xiigen-b06-0a-030-005
source_license_state: first_party
pair_kind: compiler
```

A: "Document the capability requirements for multi-supplier dropshipping order fulfillment."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "capability" (→ capability), "requirements" (→ requirement)
  Teaching: "capability" fires DOCUMENT via Rule 16. Three DOCUMENT signals; one predicate.
  Confidence: 0.93
