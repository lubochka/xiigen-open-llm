# Layer 0-A — Compiler Pairs (033)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-033-001 — CrossSellUpsell: DOCUMENT alone

```
source_id: xiigen-b06-0a-033-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the cross-sell and upsell trigger rules including placement positions, eligibility conditions, and display limits."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "rules" context
  Non-terms: "cross-sell", "upsell", "trigger", "placement positions", "eligibility conditions"
  Confidence: 0.92

---

## B06-0A-033-002 — CrossSellUpsell: IMPLEMENT alone

```
source_id: xiigen-b06-0a-033-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the upsell widget with real-time price delta display and one-click upgrade flow."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "upsell widget", "price delta", "one-click upgrade flow"
  Confidence: 0.93

---

## B06-0A-033-003 — CrossSellUpsell: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-033-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the cross-sell requirements and create the product affinity scoring module."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "requirements" (→ requirement)
    IMPLEMENT fires on: "create" (→ create)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-033-004 — CrossSellUpsell: knowledge-edge

```
source_id: xiigen-b06-0a-033-004
source_license_state: first_party
pair_kind: compiler
```

A: "Show premium product alternatives to the selected item."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Show" is operational. "premium product alternatives", "selected item" are domain nouns.
  Confidence: 0.95

---

## B06-0A-033-005 — CrossSellUpsell: multi-predicate sweep

```
source_id: xiigen-b06-0a-033-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write the cross-sell specification, design the recommendation architecture, and implement the affinity scoring API (Application Programming Interface)."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
    IMPLEMENT fires on: "Write" (→ write), "implement" (→ implement)
  TransitionType: feature_addition
  Confidence: 0.91
