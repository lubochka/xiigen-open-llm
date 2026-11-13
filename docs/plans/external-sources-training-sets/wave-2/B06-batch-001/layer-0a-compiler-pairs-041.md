# Layer 0-A — Compiler Pairs (041)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-041-001 — MembershipTiers: DOCUMENT alone

```
source_id: xiigen-b06-0a-041-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the membership tier requirements including Bronze, Silver, Gold, and Platinum tiers, point accumulation rules, and tier-specific benefit eligibility."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "membership tier", "Bronze", "Silver", "Gold", "Platinum", "point accumulation", "benefit eligibility" — domain nouns
  Confidence: 0.92

---

## B06-0A-041-002 — MembershipTiers: IMPLEMENT alone

```
source_id: xiigen-b06-0a-041-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the membership tier service with tier evaluation, automatic upgrade, and downgrade scheduling."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "membership tier service", "tier evaluation", "automatic upgrade", "downgrade scheduling"
  Confidence: 0.93

---

## B06-0A-041-003 — MembershipTiers: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-041-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the membership tier specification and build the tier upgrade evaluation engine."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-041-004 — MembershipTiers: knowledge-edge

```
source_id: xiigen-b06-0a-041-004
source_license_state: first_party
pair_kind: compiler
```

A: "Upgrade a customer to Gold tier when they reach 500 points."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Upgrade" is operational. "customer", "Gold tier", "points" are domain nouns.
  Confidence: 0.95

---

## B06-0A-041-005 — MembershipTiers: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-041-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the membership tier requirements and design the tier state machine architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
