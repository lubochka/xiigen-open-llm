# Layer 0-A — Compiler Pairs (015)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note

File 015 covers LoyaltyProgram requirements — five compiler pairs demonstrating
DOCUMENT predicate firing on loyalty-domain vocabulary.

---

## B06-0A-015-001 — LoyaltyProgram: "define loyalty requirements"

```
source_id: xiigen-b06-0a-015-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the loyalty program requirements for checkout, covering points accrual, redemption rules, and tier-based discount application."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "define" (→ define), "requirements" (→ requirement)
  Confidence: 0.92

---

## B06-0A-015-002 — LoyaltyProgram: "document loyalty narrative"

```
source_id: xiigen-b06-0a-015-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the narrative for loyalty points checkout integration, describing how members see and apply rewards at each checkout step."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "document" (→ document), "narrative" (→ narrative),
      "describing" (→ describe)
  Confidence: 0.92

---

## B06-0A-015-003 — LoyaltyProgram: "specify loyalty feature-brief"

```
source_id: xiigen-b06-0a-015-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the feature-brief for loyalty rewards at checkout, including the acceptance criteria for points balance display and redemption confirmation."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specify" (→ specification), "feature-brief" (→ feature-brief),
      "criteria" (→ criteria)
  Confidence: 0.92

---

## B06-0A-015-004 — LoyaltyProgram: "write loyalty stories" (dual)

```
source_id: xiigen-b06-0a-015-004
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the user stories for loyalty program integration, covering points display, partial redemption, and insufficient-points scenarios."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "stories" (→ story)
    IMPLEMENT fires on: "write" (→ write)
  Note: "write" fires IMPLEMENT — dual-predicate rule applies even in story-authoring context.
  Confidence: 0.92

---

## B06-0A-015-005 — LoyaltyProgram: "describe loyalty stakeholder behavior"

```
source_id: xiigen-b06-0a-015-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the stakeholder behavior expectations for loyalty program checkout, including marketing, product, and customer-service perspectives."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "describe" (→ describe), "stakeholder" (→ stakeholder),
      "behavior" (→ behavior)
  Note: "stakeholder" fires DOCUMENT and also triggers 2.4 routing at Layer 0-B.
  Confidence: 0.92
