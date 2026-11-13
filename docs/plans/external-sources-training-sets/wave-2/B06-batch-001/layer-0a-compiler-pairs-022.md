# Layer 0-A — Compiler Pairs (022)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 022 covers DataPrivacy requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on data-privacy domain vocabulary.

---

## B06-0A-022-001 — DataPrivacy: "define data privacy requirements"

```
source_id: xiigen-b06-0a-022-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the data privacy requirements for the checkout flow, covering GDPR (General Data Protection Regulation) consent, PII (Personally Identifiable Information) handling, and data minimization policies."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-022-002 — DataPrivacy: "document data privacy behavior"

```
source_id: xiigen-b06-0a-022-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the behavior for customer data retention and deletion at checkout, including right-to-erasure triggers and anonymization rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-022-003 — DataPrivacy: "describe privacy use cases"

```
source_id: xiigen-b06-0a-022-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases for privacy consent collection at checkout, including first-time consent, re-consent on policy change, and consent withdrawal."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-022-004 — DataPrivacy: "specify privacy stakeholder requirements"

```
source_id: xiigen-b06-0a-022-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the stakeholder requirements for data privacy in checkout, covering legal, security, and product owner perspectives."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "stakeholder" (→ stakeholder),
      "requirements" (→ requirement)
  Note: "stakeholder" fires DOCUMENT and also triggers 2.4 routing at Layer 0-B.
  Confidence: 0.92

---

## B06-0A-022-005 — DataPrivacy: "write privacy spec" (dual)

```
source_id: xiigen-b06-0a-022-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the data privacy specification for checkout event logging, including which fields must be masked, which must be omitted, and audit trail requirements."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification), "requirements" (→ requirement)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92
