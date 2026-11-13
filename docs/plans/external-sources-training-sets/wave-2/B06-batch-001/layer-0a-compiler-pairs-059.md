# Layer 0-A — Compiler Pairs (059)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-059-001 — VIPAccess: DOCUMENT alone

```
source_id: xiigen-b06-0a-059-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the VIP (Very Important Person) access requirements including early sale access windows, exclusive product unlocks, and dedicated support queue routing."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "VIP access", "early sale access windows", "exclusive product unlocks", "dedicated support queue" — domain nouns
  Confidence: 0.92

---

## B06-0A-059-002 — VIPAccess: IMPLEMENT alone

```
source_id: xiigen-b06-0a-059-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the VIP access gate with tier-based feature unlock and early-window sale activation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "VIP access gate", "tier-based feature unlock", "early-window sale activation"
  Confidence: 0.93

---

## B06-0A-059-003 — VIPAccess: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-059-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the VIP access specification and develop the access control service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-059-004 — VIPAccess: knowledge-edge

```
source_id: xiigen-b06-0a-059-004
source_license_state: first_party
pair_kind: compiler
```

A: "Grant VIP customers access to the flash sale 24 hours before general release."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Grant" is operational. "VIP customers", "flash sale", "general release" are domain nouns.
  Confidence: 0.95

---

## B06-0A-059-005 — VIPAccess: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-059-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the VIP access requirements and design the access gate service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
