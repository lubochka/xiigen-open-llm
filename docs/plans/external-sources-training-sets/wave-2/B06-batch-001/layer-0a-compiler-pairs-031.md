# Layer 0-A — Compiler Pairs (031)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-031-001 — AbandonedCartRecovery: DOCUMENT alone

```
source_id: xiigen-b06-0a-031-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Specify the requirements for abandoned cart recovery including email timing windows, discount incentives, and re-engagement conditions."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "requirements" (→ requirement)
  Non-terms: "abandoned cart", "recovery", "email timing windows", "discount incentives", "re-engagement conditions"
  Confidence: 0.92

---

## B06-0A-031-002 — AbandonedCartRecovery: IMPLEMENT alone

```
source_id: xiigen-b06-0a-031-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Build the abandoned cart email trigger with configurable delay intervals and A/B test variant support."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Build" (→ build)
  Non-terms: "abandoned cart", "email trigger", "delay intervals", "A/B test variant"
  Confidence: 0.93

---

## B06-0A-031-003 — AbandonedCartRecovery: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-031-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the functional requirements for abandoned cart recovery and implement the notification scheduler."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "functional" (→ functional), "requirements" (→ requirement)
    IMPLEMENT fires on: "Write" (→ write), "implement" (→ implement)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-031-004 — AbandonedCartRecovery: knowledge-edge

```
source_id: xiigen-b06-0a-031-004
source_license_state: first_party
pair_kind: compiler
```

A: "Send a recovery email two hours after cart abandonment."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Send" is operational. "recovery email", "two hours", "cart abandonment" are domain nouns.
  Confidence: 0.95

---

## B06-0A-031-005 — AbandonedCartRecovery: "story" fires DOCUMENT

```
source_id: xiigen-b06-0a-031-005
source_license_state: first_party
pair_kind: compiler
```

A: "Write a user story for abandoned cart recovery from the perspective of a returning customer."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "story" (→ story)
    IMPLEMENT fires on: "Write" (→ write)
  Teaching: "story" fires DOCUMENT; "Write" fires IMPLEMENT simultaneously.
  TransitionType: feature_addition
  Confidence: 0.91
