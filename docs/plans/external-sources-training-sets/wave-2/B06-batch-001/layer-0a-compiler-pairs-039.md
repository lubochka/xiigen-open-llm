# Layer 0-A — Compiler Pairs (039)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-039-001 — GeofencedPricing: DOCUMENT alone

```
source_id: xiigen-b06-0a-039-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the geofenced pricing requirements including region detection, country-level price overrides, and currency localization rules."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "geofenced pricing", "region detection", "country-level price overrides", "currency localization" — domain nouns
  Confidence: 0.92

---

## B06-0A-039-002 — GeofencedPricing: IMPLEMENT alone

```
source_id: xiigen-b06-0a-039-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the geofence pricing resolver with IP (Internet Protocol) geolocation lookup and region price table application."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "geofence pricing resolver", "IP geolocation", "region price table"
  Confidence: 0.93

---

## B06-0A-039-003 — GeofencedPricing: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-039-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Document the geofenced pricing specification and develop the region resolver service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "specification" (→ specification)
    IMPLEMENT fires on: "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-039-004 — GeofencedPricing: knowledge-edge

```
source_id: xiigen-b06-0a-039-004
source_license_state: first_party
pair_kind: compiler
```

A: "Apply the correct price for each customer based on their detected region."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Apply" is operational. "price", "customer", "detected region" are domain nouns.
  Confidence: 0.95

---

## B06-0A-039-005 — GeofencedPricing: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-039-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the geofenced pricing requirements and design the geolocation service architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
