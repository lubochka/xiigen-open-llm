# Layer 0-A — Compiler Pairs (032)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-032-001 — ProductRecommendation: DOCUMENT alone

```
source_id: xiigen-b06-0a-032-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the requirements for the product recommendation engine including collaborative filtering, purchase history analysis, and real-time personalization."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "product recommendation", "collaborative filtering", "purchase history", "real-time personalization"
  Confidence: 0.92

---

## B06-0A-032-002 — ProductRecommendation: IMPLEMENT alone

```
source_id: xiigen-b06-0a-032-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the recommendation scoring service with configurable algorithm weights and result caching."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "recommendation scoring", "algorithm weights", "result caching"
  Confidence: 0.93

---

## B06-0A-032-003 — ProductRecommendation: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-032-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the recommendation system requirements and design the ML (Machine Learning) pipeline architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT)
  Confidence: 0.91

---

## B06-0A-032-004 — ProductRecommendation: knowledge-edge

```
source_id: xiigen-b06-0a-032-004
source_license_state: first_party
pair_kind: compiler
```

A: "Show related products based on the current item category."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Show" is operational. "related products", "current item category" are domain nouns.
  Confidence: 0.95

---

## B06-0A-032-005 — ProductRecommendation: "feature-brief" fires DOCUMENT

```
source_id: xiigen-b06-0a-032-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write a feature-brief for the product recommendation engine for stakeholder review."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "feature-brief" (→ feature-brief), "stakeholder" (→ stakeholder)
    IMPLEMENT fires on: "Write" (→ write)
  Teaching: "feature-brief" fires DOCUMENT via Rule 14; "stakeholder" fires DOCUMENT via Rule 8.
  TransitionType: feature_addition. Also note: "stakeholder" triggers 2.4 routing.
  Confidence: 0.91
