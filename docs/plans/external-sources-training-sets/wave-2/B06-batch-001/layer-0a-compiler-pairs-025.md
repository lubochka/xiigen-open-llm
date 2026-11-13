# Layer 0-A — Compiler Pairs (025)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-025-001 — GiftCardRedemption: DOCUMENT alone

```
source_id: xiigen-b06-0a-025-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the requirements and behavior for gift card redemption at checkout, including balance checks, partial redemption, and expiry validation."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement), "behavior" (→ behavior)
  Non-terms: "gift card", "redemption", "balance checks", "partial", "expiry" — domain nouns, no predicate
  Confidence: 0.92

---

## B06-0A-025-002 — GiftCardRedemption: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-025-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Write the specification for gift card redemption and build the balance validation service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "specification" (→ specification)
    IMPLEMENT fires on: "Write" (→ write), "build" (→ build)
  TransitionType: feature_addition (IMPLEMENT present)
  Confidence: 0.91

---

## B06-0A-025-003 — GiftCardRedemption: IMPLEMENT alone

```
source_id: xiigen-b06-0a-025-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the gift card redemption service with balance lookup, apply-to-order, and reissue on cancellation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "gift card", "balance lookup", "apply-to-order", "reissue", "cancellation"
  Confidence: 0.93

---

## B06-0A-025-004 — GiftCardRedemption: knowledge-edge (no predicate)

```
source_id: xiigen-b06-0a-025-004
source_license_state: first_party
pair_kind: compiler
```

A: "Handle gift card redemption at checkout."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "handle" is not in any predicate lexicon. "gift card" and "redemption" are domain nouns.
  Confidence: 0.95

---

## B06-0A-025-005 — GiftCardRedemption: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-025-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Document the gift card redemption requirements and design the service architecture for balance management."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Document" (→ document), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (ARCHITECTURE does not upgrade without IMPLEMENT)
  Confidence: 0.91
