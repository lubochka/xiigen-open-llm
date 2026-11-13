# Layer 0-A — Compiler Pairs (034)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-034-001 — SeasonalPromotion: DOCUMENT alone

```
source_id: xiigen-b06-0a-034-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the seasonal promotion rules including campaign scheduling, category-level discounts, and blackout period enforcement."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "rules" context
  Non-terms: "seasonal promotion", "campaign scheduling", "category-level discounts", "blackout period"
  Confidence: 0.92

---

## B06-0A-034-002 — SeasonalPromotion: IMPLEMENT alone

```
source_id: xiigen-b06-0a-034-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Develop the seasonal campaign manager with bulk discount rule upload and activation scheduling."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Develop" (→ develop)
  Non-terms: "seasonal campaign", "bulk discount rule", "activation scheduling"
  Confidence: 0.93

---

## B06-0A-034-003 — SeasonalPromotion: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-034-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Define the seasonal promotion requirements and build the campaign scheduling engine."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.92

---

## B06-0A-034-004 — SeasonalPromotion: knowledge-edge

```
source_id: xiigen-b06-0a-034-004
source_license_state: first_party
pair_kind: compiler
```

A: "Activate the Black Friday campaign discounts across all product categories."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Activate" is operational. "Black Friday campaign", "product categories" are domain nouns.
  Confidence: 0.95

---

## B06-0A-034-005 — SeasonalPromotion: DOCUMENT complete reference

```
source_id: xiigen-b06-0a-034-005
source_license_state: first_party
pair_kind: compiler
```

DOCUMENT lexicon terms confirmed for seasonal promotion domain:
  define → Define the seasonal rules
  describe → Describe the campaign behavior
  specify → Specify the blackout criteria
  requirements → Seasonal promotion requirements
  use-case → Use case for holiday discounting
  behavior → Promotion behavior on checkout
  document → Document the campaign specification
  narrative → A narrative for the seasonal schedule
  specification → Seasonal specification document
  functional → Functional requirements for promotion engine
  business → Business rules for seasonal campaigns
  criteria → Eligibility criteria for promotion codes
  stakeholder → Stakeholder requirements for campaign timing
  feature-brief → Feature-brief for seasonal promotions
  epics → Campaign feature epics
  capability → Capability requirements for bulk promotion
  story → User story for seasonal promotion experience

All 17 DOCUMENT terms demonstrated in seasonal promotion context.
