# Layer 0-A — Compiler Pairs (054)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: compiler

---

## B06-0A-054-001 — WishlistSharing: DOCUMENT alone

```
source_id: xiigen-b06-0a-054-001
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Define the wishlist sharing requirements including public/private toggle, shareable link generation, and collaborative wishlist permissions."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Define" (→ define), "requirements" (→ requirement)
  Non-terms: "wishlist sharing", "public/private toggle", "shareable link generation", "collaborative wishlist permissions" — domain nouns
  Confidence: 0.92

---

## B06-0A-054-002 — WishlistSharing: IMPLEMENT alone

```
source_id: xiigen-b06-0a-054-002
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Create the wishlist sharing service with visibility controls and invite-link generation."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Predicate evidence:
    IMPLEMENT fires on: "Create" (→ create)
  Non-terms: "wishlist sharing service", "visibility controls", "invite-link generation"
  Confidence: 0.93

---

## B06-0A-054-003 — WishlistSharing: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-0a-054-003
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
pair_kind: compiler
```

A: "Specify the wishlist sharing specification and build the permission management service."

B:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Predicate evidence:
    DOCUMENT fires on: "Specify" (→ specify), "specification" (→ specification)
    IMPLEMENT fires on: "build" (→ build)
  TransitionType: feature_addition
  Confidence: 0.91

---

## B06-0A-054-004 — WishlistSharing: knowledge-edge

```
source_id: xiigen-b06-0a-054-004
source_license_state: first_party
pair_kind: compiler
```

A: "Share the customer's wishlist via a link with friends."

B:
  CompiledPredicateIds: []
  KnowledgeEdge: true
  Reason: "Share" is operational. "customer's wishlist", "link", "friends" are domain nouns.
  Confidence: 0.95

---

## B06-0A-054-005 — WishlistSharing: DOCUMENT + ARCHITECTURE

```
source_id: xiigen-b06-0a-054-005
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: documentation
pair_kind: compiler
```

A: "Describe the wishlist sharing requirements and design the sharing permission architecture."

B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Predicate evidence:
    DOCUMENT fires on: "Describe" (→ describe), "requirements" (→ requirement)
    ARCHITECTURE fires on: "design" (→ design), "architecture" (→ architecture)
  TransitionType: documentation (no IMPLEMENT present)
  Confidence: 0.91
