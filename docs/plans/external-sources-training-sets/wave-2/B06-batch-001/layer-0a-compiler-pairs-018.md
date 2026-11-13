# Layer 0-A — Compiler Pairs (018)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 018 covers MultiCurrency requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on multi-currency domain vocabulary.

---

## B06-0A-018-001 — MultiCurrency: "define multi-currency requirements"

```
source_id: xiigen-b06-0a-018-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the multi-currency requirements for the checkout flow, covering display currency, settlement currency, and exchange rate update policies."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-018-002 — MultiCurrency: "document currency behavior"

```
source_id: xiigen-b06-0a-018-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the currency conversion behavior during checkout, including rounding policies, FX (Foreign Exchange) markup rules, and displayed-vs-settled discrepancy handling."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-018-003 — MultiCurrency: "specify multi-currency spec"

```
source_id: xiigen-b06-0a-018-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the multi-currency spec for the payment gateway integration, including supported currencies, precision rules, and refund currency handling."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "spec" (→ specification)
  Confidence: 0.92

---

## B06-0A-018-004 — MultiCurrency: "write currency use cases" (dual)

```
source_id: xiigen-b06-0a-018-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the use cases for multi-currency checkout, covering currency selection, price display, payment processing, and receipt generation."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "use cases" (→ use-case)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92

---

## B06-0A-018-005 — MultiCurrency: "describe currency functional criteria"

```
source_id: xiigen-b06-0a-018-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the functional criteria for currency auto-detection at checkout, using IP (Internet Protocol) geolocation and account preference signals."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "functional" (→ functional),
      "criteria" (→ criteria)
  Confidence: 0.92
