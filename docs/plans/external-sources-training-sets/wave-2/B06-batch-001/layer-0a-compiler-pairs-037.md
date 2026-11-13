# Layer 0-A — Compiler Pairs (037)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-037-001 — CustomerSegmentation: DOCUMENT alone

```
source_id: xiigen-b06-0a-037-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the requirements and behavior for customer segmentation including RFM (Recency, Frequency, Monetary) scoring, cohort assignment, and segment-based pricing eligibility."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement), "behavior" (→ behavior)
  Non-terms: "customer segmentation", "RFM", "cohort", "pricing eligibility" — domain nouns, no predicate
  Confidence: 0.92

---

## B06-0A-037-002 — CustomerSegmentation: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-037-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the specification for customer segmentation and build the cohort classification service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.91

---

## B06-0A-037-003 — CustomerSegmentation: IMPLEMENT alone

```
source_id: xiigen-b06-0a-037-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the customer segmentation engine with RFM scoring, cohort assignment, and segment refresh scheduling."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "customer segmentation engine", "RFM scoring", "cohort assignment", "segment refresh"
  Confidence: 0.93

---

## B06-0A-037-004 — CustomerSegmentation: knowledge-edge (no predicate)

```
source_id: xiigen-b06-0a-037-004
source_license_state: first_party
pair_kind: compiler
```

A: "Assign customers to segments based on purchase history."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Assign" is operational. "customers", "segments", "purchase history" are domain nouns.
  Confidence: 0.95

---

## B06-0A-037-005 — CustomerSegmentation: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-037-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the customer segmentation requirements and design the data pipeline architecture for cohort computation."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (ARCHITECTURE does not upgrade without IMPLEMENT)
  Confidence: 0.91
