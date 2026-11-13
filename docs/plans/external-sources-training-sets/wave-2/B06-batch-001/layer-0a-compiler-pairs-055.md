# Layer 0-A — Compiler Pairs (055)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-055-001 — ProductBundleCreator: DOCUMENT alone

```
source_id: xiigen-b06-0a-055-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the product bundle creator requirements including component selection rules, bundle pricing override, and individual item sale restrictions."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "product bundle creator", "component selection rules", "bundle pricing override", "item sale restrictions" — domain nouns
  Confidence: 0.92

---

## B06-0A-055-002 — ProductBundleCreator: IMPLEMENT alone

```
source_id: xiigen-b06-0a-055-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the product bundle builder with component inventory reservation and bundle price computation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "product bundle builder", "component inventory reservation", "bundle price computation"
  Confidence: 0.93

---

## B06-0A-055-003 — ProductBundleCreator: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-055-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the product bundle creator specification and develop the bundle pricing engine."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-055-004 — ProductBundleCreator: knowledge-edge

```
source_id: xiigen-b06-0a-055-004
source_license_state: first_party
pair_kind: compiler
```

A: "Combine three products into a bundle at a discounted price."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Combine" is operational. "products", "bundle", "discounted price" are domain nouns.
  Confidence: 0.95

---

## B06-0A-055-005 — ProductBundleCreator: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-055-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the product bundle requirements and design the bundle pricing engine architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
