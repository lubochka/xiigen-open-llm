# Layer 0-A — Compiler Pairs (057)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-057-001 — StockAlertSystem: DOCUMENT alone

```
source_id: xiigen-b06-0a-057-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the stock alert system requirements including back-in-stock notification subscriptions, low-stock threshold warnings, and alert delivery channel preferences."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "stock alert system", "back-in-stock notification", "low-stock threshold warnings", "alert delivery channel" — domain nouns
  Confidence: 0.92

---

## B06-0A-057-002 — StockAlertSystem: IMPLEMENT alone

```
source_id: xiigen-b06-0a-057-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the stock alert notification service with threshold monitor and multi-channel delivery dispatcher."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "stock alert notification service", "threshold monitor", "multi-channel delivery dispatcher"
  Confidence: 0.93

---

## B06-0A-057-003 — StockAlertSystem: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-057-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the stock alert specification and build the subscription management service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-057-004 — StockAlertSystem: knowledge-edge

```
source_id: xiigen-b06-0a-057-004
source_license_state: first_party
pair_kind: compiler
```

A: "Notify subscribed customers when out-of-stock items are replenished."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Notify" is operational. "subscribed customers", "out-of-stock items" are domain nouns.
  Confidence: 0.95

---

## B06-0A-057-005 — StockAlertSystem: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-057-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the stock alert requirements and design the notification service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
