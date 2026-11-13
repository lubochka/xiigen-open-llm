# Layer 0-C — Knowledge-Edge Pairs (025)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-025-001 — GiftCardRedemption: gap verbs

```
source_id: xiigen-b06-0c-025-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-025-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_gift_card_domain:
      "process gift card payment" → KNOWLEDGE_EDGE (process = operational verb)
      "validate gift card balance" → KNOWLEDGE_EDGE (validate = operational verb)
      "apply gift card to order" → KNOWLEDGE_EDGE (apply = operational verb)
      "check gift card expiry" → KNOWLEDGE_EDGE (check = operational verb)
      "handle partial redemption" → KNOWLEDGE_EDGE (handle = operational verb)
    boundary_cases:
      "define gift card validation requirements" → NOT knowledge-edge (define fires DOCUMENT)
      "specify gift card balance check behavior" → NOT knowledge-edge (specify fires DOCUMENT)

---

## B06-0C-025-002 — GiftCardRedemption: partial predicate in gap context

```
source_id: xiigen-b06-0c-025-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-025-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  partial_predicate_boundary:
    "Handle the gift card requirements at checkout" → NOT knowledge-edge
      Reason: "requirements" fires DOCUMENT despite "handle" being a gap verb.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78 (reduced for gap-verb context)
    "Process requirements for gift card redemption" → NOT knowledge-edge
      Reason: "requirements" fires DOCUMENT despite "Process" being gap verb.
    Teaching: A single DOCUMENT term anywhere in the sentence prevents knowledge-edge.

---

## B06-0C-025-003 — GiftCardRedemption: complete gap vocabulary

```
source_id: xiigen-b06-0c-025-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-025-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vocabulary_gift_card:
    operational_verbs: ["process", "validate", "apply", "check", "handle", "redeem", "expire", "reissue", "void"]
    domain_nouns: ["gift card", "balance", "redemption", "partial redemption", "expiry date",
                   "reissue", "void", "activation", "denomination"]
  Teaching: None of these words fire any predicate. Only DOCUMENT/IMPLEMENT/ARCHITECTURE/UI
    lexicon terms fire predicates. Domain knowledge does not alter predicate logic.
