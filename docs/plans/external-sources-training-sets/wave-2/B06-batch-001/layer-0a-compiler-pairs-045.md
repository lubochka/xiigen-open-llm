# Layer 0-A — Compiler Pairs (045)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-045-001 — BackorderManagement: DOCUMENT alone

```
source_id: xiigen-b06-0a-045-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the backorder management requirements including customer notification rules, estimated availability display, and automatic fulfillment on restock."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "backorder management", "customer notification", "estimated availability", "automatic fulfillment", "restock" — domain nouns
  Confidence: 0.92

---

## B06-0A-045-002 — BackorderManagement: IMPLEMENT alone

```
source_id: xiigen-b06-0a-045-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the backorder queue manager with restock event listener and customer notification dispatcher."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "backorder queue manager", "restock event listener", "customer notification dispatcher"
  Confidence: 0.93

---

## B06-0A-045-003 — BackorderManagement: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-045-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the backorder management specification and build the fulfillment queue service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-045-004 — BackorderManagement: knowledge-edge

```
source_id: xiigen-b06-0a-045-004
source_license_state: first_party
pair_kind: compiler
```

A: "Queue the customer order when inventory is exhausted."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Queue" is operational. "customer order", "inventory" are domain nouns.
  Confidence: 0.95

---

## B06-0A-045-005 — BackorderManagement: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-045-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the backorder management requirements and design the fulfillment queue architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
