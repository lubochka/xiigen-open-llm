# Layer 0-A — Compiler Pairs (016)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 016 covers ReturnAndRefund requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on returns-domain vocabulary.

---

## B06-0A-016-001 — ReturnAndRefund: "define return policy requirements"

```
source_id: xiigen-b06-0a-016-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the return and refund policy requirements for the checkout and post-purchase flow, covering 30-day, 60-day, and final-sale scenarios."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-016-002 — ReturnAndRefund: "describe refund behavior"

```
source_id: xiigen-b06-0a-016-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the functional behavior for refund processing, including eligibility checks, partial refund rules, and timeline requirements."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "functional" (→ functional),
      "behavior" (→ behavior), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-016-003 — ReturnAndRefund: "document return use cases"

```
source_id: xiigen-b06-0a-016-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the use cases for self-service return initiation, including label generation and item condition assessment flows."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-016-004 — ReturnAndRefund: "write refund spec" (dual)

```
source_id: xiigen-b06-0a-016-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the specification for refund eligibility calculation, including business rules for promotional items, bundled products, and partial returns."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification), "business rules" (→ business, requirement)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92

---

## B06-0A-016-005 — ReturnAndRefund: "define refund epics and capability"

```
source_id: xiigen-b06-0a-016-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the epics and capability requirements for end-to-end refund processing, from return request to credit issuance."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "epics" (→ epics),
      "capability" (→ capability), "requirements" (→ requirement)
  Confidence: 0.92
