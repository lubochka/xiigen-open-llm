# Layer 0-A — Compiler Pairs (013)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 013 covers ShippingRequirements DOCUMENT cases — five pairs demonstrating
the DOCUMENT predicate firing on shipping-domain requirement vocabulary.

---

## B06-0A-013-001 — ShippingRequirements: "define shipping requirements"

```
source_id: xiigen-b06-0a-013-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the shipping requirements for the checkout flow, covering standard, express, and overnight delivery options."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-013-002 — ShippingRequirements: "document shipping behavior"

```
source_id: xiigen-b06-0a-013-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the shipping behavior and delivery window specifications for domestic and international orders."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "behavior" (→ behavior),
      "specifications" (→ specification)
  Confidence: 0.92

---

## B06-0A-013-003 — ShippingRequirements: "describe shipping use cases"

```
source_id: xiigen-b06-0a-013-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases for shipping address validation and carrier selection during the checkout flow."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-013-004 — ShippingRequirements: "write shipping spec" (dual)

```
source_id: xiigen-b06-0a-013-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the shipping specification for the carrier API (Application Programming Interface) integration, including retry and fallback behavior."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification), "behavior" (→ behavior)
    IMPLEMENT fires on: "write" (→ write)
  Note: "write" fires IMPLEMENT unconditionally — compiler is lexicon-based, not intent-based.
  Confidence: 0.92

---

## B06-0A-013-005 — ShippingRequirements: "specify shipping functional criteria"

```
source_id: xiigen-b06-0a-013-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the functional criteria for shipping cost calculation, including weight tiers, distance zones, and surcharge rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "functional" (→ functional),
      "criteria" (→ criteria)
  Confidence: 0.92
