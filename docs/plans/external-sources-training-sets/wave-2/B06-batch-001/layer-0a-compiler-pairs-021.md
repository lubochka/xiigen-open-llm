# Layer 0-A — Compiler Pairs (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 021 covers FraudDetection requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on fraud-detection domain vocabulary.

---

## B06-0A-021-001 — FraudDetection: "define fraud detection requirements"

```
source_id: xiigen-b06-0a-021-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the fraud detection requirements for the checkout flow, covering velocity checks, device fingerprinting, and 3DS (3-D Secure) challenge triggers."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-021-002 — FraudDetection: "document fraud detection behavior"

```
source_id: xiigen-b06-0a-021-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the fraud scoring behavior during checkout, including risk threshold definitions, automatic decline rules, and manual review queuing criteria."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "behavior" (→ behavior),
      "criteria" (→ criteria)
  Confidence: 0.92

---

## B06-0A-021-003 — FraudDetection: "describe fraud use cases"

```
source_id: xiigen-b06-0a-021-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases for fraud challenge flows at checkout, covering SMS OTP (One-Time Password), biometric challenge, and fallback decline paths."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-021-004 — FraudDetection: "specify fraud spec" (DOCUMENT alone)

```
source_id: xiigen-b06-0a-021-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the fraud detection spec for chargebacks and dispute handling, including evidence collection requirements and response timeline criteria."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "spec" (→ specification),
      "requirements" (→ requirement), "criteria" (→ criteria)
  Note: "specify" is in DOCUMENT lexicon; it does NOT fire IMPLEMENT.
  Confidence: 0.92

---

## B06-0A-021-005 — FraudDetection: "create fraud requirements document" (dual)

```
source_id: xiigen-b06-0a-021-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the fraud detection requirements document for the checkout team, covering all risk scoring rules and escalation paths."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement), "document" (→ document)
    IMPLEMENT fires on: "create" (→ create)
  Confidence: 0.92
