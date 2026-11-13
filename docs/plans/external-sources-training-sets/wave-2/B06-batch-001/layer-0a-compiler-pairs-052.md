# Layer 0-A — Compiler Pairs (052)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-052-001 — InstallmentPlan: DOCUMENT alone

```
source_id: xiigen-b06-0a-052-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the installment plan requirements including BNPL (Buy Now Pay Later) eligibility, interest-free period rules, and missed payment penalties."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "installment plan", "BNPL", "interest-free period", "missed payment penalties" — domain nouns
  Confidence: 0.92

---

## B06-0A-052-002 — InstallmentPlan: IMPLEMENT alone

```
source_id: xiigen-b06-0a-052-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the installment plan scheduler with instalment due-date calculation and missed-payment detection."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "installment plan scheduler", "due-date calculation", "missed-payment detection"
  Confidence: 0.93

---

## B06-0A-052-003 — InstallmentPlan: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-052-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the installment plan specification and build the payment schedule service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-052-004 — InstallmentPlan: knowledge-edge

```
source_id: xiigen-b06-0a-052-004
source_license_state: first_party
pair_kind: compiler
```

A: "Schedule monthly installment charges for the customer."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Schedule" is operational. "monthly installment charges", "customer" are domain nouns.
  Confidence: 0.95

---

## B06-0A-052-005 — InstallmentPlan: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-052-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the installment plan requirements and design the payment scheduling architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
