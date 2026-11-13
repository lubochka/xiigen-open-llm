# Layer 0-A — Compiler Pairs (017)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 017 covers TaxCalculation requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on tax-domain vocabulary.

---

## B06-0A-017-001 — TaxCalculation: "define tax requirements"

```
source_id: xiigen-b06-0a-017-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the tax calculation requirements for checkout, covering VAT (Value Added Tax), sales tax by jurisdiction, and tax-exempt customer handling."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-017-002 — TaxCalculation: "specify tax functional behavior"

```
source_id: xiigen-b06-0a-017-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the functional behavior for real-time tax rate lookup and calculation at each checkout step, including rounding rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "functional" (→ functional),
      "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-017-003 — TaxCalculation: "describe tax use cases"

```
source_id: xiigen-b06-0a-017-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases for tax calculation during checkout, including digital goods tax, physical product tax, and mixed-cart scenarios."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-017-004 — TaxCalculation: "create tax spec" (dual)

```
source_id: xiigen-b06-0a-017-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create a specification for the tax calculation engine, covering jurisdiction determination, rate lookup, and line-item application."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "create" (→ create)
  Note: "create" fires IMPLEMENT — dual-predicate result even in specification context.
  Confidence: 0.92

---

## B06-0A-017-005 — TaxCalculation: "document tax compliance narrative"

```
source_id: xiigen-b06-0a-017-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the narrative for tax compliance in international checkout, covering EU (European Union) VAT, US (United States) sales tax nexus, and Canadian GST (Goods and Services Tax) rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "narrative" (→ narrative)
  Confidence: 0.92
