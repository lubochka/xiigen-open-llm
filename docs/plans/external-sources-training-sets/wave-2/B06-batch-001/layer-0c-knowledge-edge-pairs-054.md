# Layer 0-C — Knowledge-Edge Pairs (054)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-054-001 — WishlistSharing: gap verbs

```
source_id: xiigen-b06-0c-054-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-054-001-kedge"
  CompiledPredicateIds: []

B:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Gap examples for WishlistSharing:
    "Share the customer's wishlist via a link with friends." → KNOWLEDGE_EDGE (share = operational)
    "Generate a shareable wishlist link." → KNOWLEDGE_EDGE (generate = operational)
    "Toggle the wishlist visibility between public and private." → KNOWLEDGE_EDGE (toggle = operational)
    "Add a collaborator to the shared wishlist." → KNOWLEDGE_EDGE (add = operational)
  Teaching: "customer's wishlist", "shareable link", "wishlist visibility", "collaborator" are domain nouns.

---

## B06-0C-054-002 — WishlistSharing: boundary cases

```
source_id: xiigen-b06-0c-054-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-054-002-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Boundary cases for WishlistSharing:
    "Define the wishlist sharing requirements." → NOT knowledge-edge (define, requirements fire DOCUMENT)
    "Share wishlists per the specification." → NOT knowledge-edge (specification fires DOCUMENT)
    "Create the sharing service." → NOT knowledge-edge (create fires IMPLEMENT)
    "Share wishlists with friends." → KNOWLEDGE_EDGE (no predicate term present)

---

## B06-0C-054-003 — WishlistSharing: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-054-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-054-003-kedge"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Gap vs spec vocabulary for WishlistSharing:
    gap: "share/generate/toggle/add/remove wishlist"
    spec: "define/describe/specify/document wishlist sharing requirements"
  Teaching: "wishlist", "shareable link", "collaborative wishlist", "visibility" are domain nouns — never fire predicates alone.
