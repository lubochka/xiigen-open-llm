# Layer 0-A — Compiler Pairs (035)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-035-001 — DigitalProductDelivery: DOCUMENT alone

```
source_id: xiigen-b06-0a-035-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the digital product delivery requirements including download link generation, license key issuance, and access revocation on refund."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "digital product", "download link", "license key issuance", "access revocation"
  Confidence: 0.92

---

## B06-0A-035-002 — DigitalProductDelivery: IMPLEMENT alone

```
source_id: xiigen-b06-0a-035-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the digital fulfillment service with time-limited download URLs and concurrent download throttling."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "digital fulfillment", "time-limited download URLs", "concurrent download throttling"
  Confidence: 0.93

---

## B06-0A-035-003 — DigitalProductDelivery: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-035-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the digital delivery specification and develop the license key generation service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "develop" (→ develop)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-035-004 — DigitalProductDelivery: knowledge-edge

```
source_id: xiigen-b06-0a-035-004
source_license_state: first_party
pair_kind: compiler
```

A: "Email the download link to the customer after payment confirmation."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Email" is operational. "download link", "customer", "payment confirmation" are domain nouns.
  Confidence: 0.95

---

## B06-0A-035-005 — DigitalProductDelivery: DOCUMENT + ARCHITECTURE + IMPLEMENT

```
source_id: xiigen-b06-0a-035-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write the digital delivery requirements, design the CDN (Content Delivery Network) architecture, and build the download token service."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91
