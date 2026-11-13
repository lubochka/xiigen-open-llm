# Layer 0-A — Compiler Pairs (027)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-027-001 — FlashSaleEngine: DOCUMENT alone

```
source_id: xiigen-b06-0a-027-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the requirements for a flash sale engine including time-boxed discounts, inventory reservation, and concurrent purchase limits."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
  Non-terms: "flash sale", "time-boxed", "discounts", "inventory reservation", "concurrent purchase limits"
  Confidence: 0.92

---

## B06-0A-027-002 — FlashSaleEngine: IMPLEMENT alone

```
source_id: xiigen-b06-0a-027-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Implement the flash sale countdown timer with auto-rollback when the sale window closes."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Implement" (→ implement)
  Non-terms: "flash sale", "countdown timer", "auto-rollback", "sale window"
  Confidence: 0.94

---

## B06-0A-027-003 — FlashSaleEngine: DOCUMENT + IMPLEMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-027-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the flash sale specification, design the scheduling architecture, and build the inventory hold service."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.91

---

## B06-0A-027-004 — FlashSaleEngine: knowledge-edge

```
source_id: xiigen-b06-0a-027-004
source_license_state: first_party
pair_kind: compiler
```

A: "Run a flash sale for electronics at 50% off for two hours."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Run" is operational. "flash sale", "electronics", "50% off", "two hours" are all domain nouns.
  Confidence: 0.95

---

## B06-0A-027-005 — FlashSaleEngine: "functional" fires DOCUMENT

```
source_id: xiigen-b06-0a-027-005
source_license_state: first_party
pair_kind: compiler
```

A: "Specify the functional requirements for the flash sale engine discount calculation."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "functional" (→ functional), "requirements" (→ requirement)
  Teaching: three DOCUMENT signals — "specify", "functional", "requirements" — all fire; one predicate emitted.
  Confidence: 0.93
