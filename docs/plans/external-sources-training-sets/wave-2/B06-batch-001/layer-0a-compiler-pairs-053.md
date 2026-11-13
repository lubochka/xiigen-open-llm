# Layer 0-A — Compiler Pairs (053)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-053-001 — StoreCreditManagement: DOCUMENT alone

```
source_id: xiigen-b06-0a-053-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the store credit management requirements including issuance on returns, partial spend rules, and expiry enforcement."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "store credit management", "issuance on returns", "partial spend rules", "expiry enforcement" — domain nouns
  Confidence: 0.92

---

## B06-0A-053-002 — StoreCreditManagement: IMPLEMENT alone

```
source_id: xiigen-b06-0a-053-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the store credit ledger with balance tracking, spend deduction, and automatic expiry."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "store credit ledger", "balance tracking", "spend deduction", "automatic expiry"
  Confidence: 0.93

---

## B06-0A-053-003 — StoreCreditManagement: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-053-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the store credit specification and develop the credit ledger service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-053-004 — StoreCreditManagement: knowledge-edge

```
source_id: xiigen-b06-0a-053-004
source_license_state: first_party
pair_kind: compiler
```

A: "Issue store credit to the customer after a successful return."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Issue" is operational. "store credit", "customer", "return" are domain nouns.
  Confidence: 0.95

---

## B06-0A-053-005 — StoreCreditManagement: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-053-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the store credit requirements and design the credit ledger architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
