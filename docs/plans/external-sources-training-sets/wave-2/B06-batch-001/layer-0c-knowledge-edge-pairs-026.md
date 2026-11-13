# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B06-0C-026-001 — ReferralProgram: gap verbs

```
source_id: xiigen-b06-0c-026-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-026-001-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gap_verbs_in_referral_domain:
      "track referral invites" → KNOWLEDGE_EDGE (track = operational verb)
      "assign reward credits to referrer" → KNOWLEDGE_EDGE (assign = operational verb)
      "monitor referral conversion rates" → KNOWLEDGE_EDGE (monitor = operational verb)
      "expire referral codes after 30 days" → KNOWLEDGE_EDGE (expire = operational verb)
      "prevent duplicate referral fraud" → KNOWLEDGE_EDGE (prevent = operational verb)

---

## B06-0C-026-002 — ReferralProgram: gap vs spec vocabulary

```
source_id: xiigen-b06-0c-026-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-026-002-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  gap_vs_spec_table:
    "gap: track referrals" vs "spec: define referral tracking requirements" → gap no predicate; spec fires DOCUMENT
    "gap: assign rewards" vs "spec: specify reward assignment behavior" → gap no predicate; spec fires DOCUMENT
    "gap: monitor conversions" vs "spec: document conversion monitoring criteria" → gap no predicate; spec fires DOCUMENT
    "gap: prevent fraud" vs "spec: describe fraud prevention requirements" → gap no predicate; spec fires DOCUMENT

---

## B06-0C-026-003 — ReferralProgram: "business" fires DOCUMENT in gap context

```
source_id: xiigen-b06-0c-026-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-026-003-kedge"
  CompiledPredicateIds: ["META"]

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  boundary_case:
    "Track business referrals for the loyalty program" → NOT knowledge-edge
      Reason: "business" fires DOCUMENT via Rule 7.
      CompiledPredicateIds: ["DOCUMENT"]; Confidence: 0.78
    Teaching: Rule 7 ("business" fires DOCUMENT) applies even when the primary verb is a gap verb.
    The lexicon scan is full-text, not position-dependent. "business" anywhere in the sentence fires DOCUMENT.
