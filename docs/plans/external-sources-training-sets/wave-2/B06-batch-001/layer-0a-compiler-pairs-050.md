# Layer 0-A — Compiler Pairs (050)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-050-001 — WarrantyTracking: DOCUMENT alone

```
source_id: xiigen-b06-0a-050-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the warranty tracking requirements including manufacturer warranty registration, extended warranty upsell, and expiry notification rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
  Non-terms: "warranty tracking", "manufacturer warranty registration", "extended warranty upsell", "expiry notification" — domain nouns
  Confidence: 0.92

---

## B06-0A-050-002 — WarrantyTracking: IMPLEMENT alone

```
source_id: xiigen-b06-0a-050-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the warranty ledger service with purchase-linked registration and expiry scheduler."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "warranty ledger service", "purchase-linked registration", "expiry scheduler"
  Confidence: 0.93

---

## B06-0A-050-003 — WarrantyTracking: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-050-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the warranty tracking specification and build the warranty registration service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-050-004 — WarrantyTracking: knowledge-edge

```
source_id: xiigen-b06-0a-050-004
source_license_state: first_party
pair_kind: compiler
```

A: "Track the warranty expiry date for each purchased product."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Track" is operational. "warranty expiry date", "purchased product" are domain nouns.
  Confidence: 0.95

---

## B06-0A-050-005 — WarrantyTracking: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-050-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the warranty tracking requirements and design the warranty ledger service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
