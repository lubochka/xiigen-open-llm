# Layer 0-A — Compiler Pairs (023)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 023 covers DOCUMENT+IMPLEMENT dual cases — five compiler pairs demonstrating
the dual-predicate rule when "write" or "create" appears alongside DOCUMENT vocabulary.
All five pairs produce TransitionType: feature_addition.

---

## B06-0A-023-001 — DOCUMENT+IMPLEMENT: "write the requirements document"

```
source_id: xiigen-b06-0a-023-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the requirements document for the checkout payment error recovery flow."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement), "document" (→ document)
    IMPLEMENT fires on: "write" (→ write)
  Rule: "write" + requirements noun → DOCUMENT + IMPLEMENT (both fire — Rule 9 applies).
  TransitionType: feature_addition (IMPLEMENT present).
  Confidence: 0.92

---

## B06-0A-023-002 — DOCUMENT+IMPLEMENT: "create the user story"

```
source_id: xiigen-b06-0a-023-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the user story for guest checkout address entry, including acceptance criteria for address validation and delivery confirmation."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "story" (→ story), "criteria" (→ criteria)
    IMPLEMENT fires on: "create" (→ create)
  Rule: "create" + story noun → DOCUMENT + IMPLEMENT.
  TransitionType: feature_addition.
  Confidence: 0.92

---

## B06-0A-023-003 — DOCUMENT+IMPLEMENT: "build the spec" (rare)

```
source_id: xiigen-b06-0a-023-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the spec for the order-summary checkout module, covering itemization, tax display, and discount breakdown."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "spec" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  Rule: "build" + spec noun → DOCUMENT + IMPLEMENT. "build" is in IMPLEMENT lexicon.
  TransitionType: feature_addition.
  Confidence: 0.92

---

## B06-0A-023-004 — DOCUMENT+IMPLEMENT: "develop the feature-brief"

```
source_id: xiigen-b06-0a-023-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the feature-brief for address book checkout integration, covering default address selection and new address entry flows."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "feature-brief" (→ feature-brief)
    IMPLEMENT fires on: "develop" (→ develop)
  Rule: "develop" is in IMPLEMENT lexicon — fires IMPLEMENT even in authoring context.
  TransitionType: feature_addition.
  Confidence: 0.92

---

## B06-0A-023-005 — DOCUMENT+IMPLEMENT: "write the epics" (canonical dual)

```
source_id: xiigen-b06-0a-023-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the epics for the checkout redesign initiative, covering guest flow, registered flow, and payment processing domains."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "epics" (→ epics)
    IMPLEMENT fires on: "write" (→ write)
  Rule: "write" fires IMPLEMENT; "epics" fires DOCUMENT. Both predicates fire independently.
  Anti-pattern avoided: Do NOT suppress IMPLEMENT because the object is documentation-like.
  TransitionType: feature_addition.
  Confidence: 0.92
