# Layer 0-A — Compiler Pairs (014)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 014 covers InventoryManagement requirements — five compiler pairs
demonstrating DOCUMENT predicate firing on inventory-domain vocabulary.

---

## B06-0A-014-001 — InventoryManagement: "define inventory requirements"

```
source_id: xiigen-b06-0a-014-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the inventory management requirements for the checkout flow, covering stock reservation, release on timeout, and out-of-stock handling."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-014-002 — InventoryManagement: "describe inventory behavior"

```
source_id: xiigen-b06-0a-014-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the functional behavior of inventory reservation during checkout, including concurrency rules and timeout policies."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "functional" (→ functional),
      "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-014-003 — InventoryManagement: "document inventory use cases"

```
source_id: xiigen-b06-0a-014-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the use cases for inventory check and reservation during the cart and checkout stages."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-014-004 — InventoryManagement: "write inventory spec" (dual)

```
source_id: xiigen-b06-0a-014-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the inventory reservation spec, including the business rules for hold duration, release triggers, and backorder eligibility."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "spec" (→ specification), "business rules" (→ business, requirement)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92

---

## B06-0A-014-005 — InventoryManagement: "specify inventory epics"

```
source_id: xiigen-b06-0a-014-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the epics and capability requirements for real-time inventory tracking across the checkout and fulfilment domains."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "epics" (→ epics),
      "capability" (→ capability), "requirements" (→ requirement)
  Confidence: 0.92
