# Layer 0-A — Compiler Pairs (020)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 020 covers SubscriptionBilling requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on subscription-domain vocabulary.

---

## B06-0A-020-001 — SubscriptionBilling: "define subscription requirements"

```
source_id: xiigen-b06-0a-020-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the subscription billing requirements for checkout, covering initial setup, recurring charge schedules, trial periods, and cancellation flows."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-020-002 — SubscriptionBilling: "document subscription behavior"

```
source_id: xiigen-b06-0a-020-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the functional behavior for subscription renewal during checkout, including payment retry logic, dunning policies, and grace period rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "functional" (→ functional),
      "behavior" (→ behavior)
  Confidence: 0.92

---

## B06-0A-020-003 — SubscriptionBilling: "describe subscription use cases"

```
source_id: xiigen-b06-0a-020-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the use cases for subscription plan selection and checkout, including upgrade, downgrade, and add-on scenarios."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "use cases" (→ use-case)
  Confidence: 0.92

---

## B06-0A-020-004 — SubscriptionBilling: "write subscription stories" (dual)

```
source_id: xiigen-b06-0a-020-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the user stories for subscription checkout, covering plan selection, payment method storage, and auto-renewal consent."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "stories" (→ story)
    IMPLEMENT fires on: "write" (→ write)
  Confidence: 0.92

---

## B06-0A-020-005 — SubscriptionBilling: "specify subscription epics and capability"

```
source_id: xiigen-b06-0a-020-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the epics and capability requirements for end-to-end subscription lifecycle management at the checkout layer."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "epics" (→ epics),
      "capability" (→ capability), "requirements" (→ requirement)
  Confidence: 0.92
