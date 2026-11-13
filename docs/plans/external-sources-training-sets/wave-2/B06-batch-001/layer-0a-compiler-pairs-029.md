# Layer 0-A — Compiler Pairs (029)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-029-001 — WholesaleDiscount: DOCUMENT alone

```
source_id: xiigen-b06-0a-029-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the wholesale discount tiers, minimum order quantities, and account-level pricing agreements."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe)
  Non-terms: "wholesale discount", "tiers", "minimum order quantities", "account-level pricing agreements"
  Confidence: 0.92

---

## B06-0A-029-002 — WholesaleDiscount: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-029-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify wholesale discount requirements and build the tiered pricing engine for B2B (Business-to-Business) accounts."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "requirements" (→ requirement)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-029-003 — WholesaleDiscount: ARCHITECTURE alone

```
source_id: xiigen-b06-0a-029-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Design the pricing service architecture to support wholesale account segmentation and contract-based discounts."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  Predicate evidence:
    ARCHITECTURE fires on: "Design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (ARCHITECTURE alone → documentation)
  Confidence: 0.93

---

## B06-0A-029-004 — WholesaleDiscount: knowledge-edge

```
source_id: xiigen-b06-0a-029-004
source_license_state: first_party
pair_kind: compiler
```

A: "Calculate the wholesale price for bulk orders over 500 units."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Calculate" is operational. "wholesale price", "bulk orders", "500 units" are domain nouns.
  Confidence: 0.95

---

## B06-0A-029-005 — WholesaleDiscount: "epics" fires DOCUMENT

```
source_id: xiigen-b06-0a-029-005
source_license_state: first_party
pair_kind: compiler
```

A: "Document the product epics for the wholesale discount configuration feature."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "epics" (→ epics)
  Teaching: "epics" fires DOCUMENT via Rule 15. Two DOCUMENT signals; one predicate emitted.
  Confidence: 0.93
