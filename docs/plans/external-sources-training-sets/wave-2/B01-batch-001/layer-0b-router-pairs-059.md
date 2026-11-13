# Layer 0-B — Router Pairs (059)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-059-001 — SocialShare: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-059-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a social share component with Twitter, LinkedIn, and copy-to-clipboard button variants."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-059-002 — SocialShare: UI+IMPLEMENT (triple UI hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-059-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a share widget with Open Graph preview, custom message input, and platform icon buttons."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-059-003 — SocialShare: ARCHITECTURE+DOCUMENT routes to 3.1 + 2.1

```
source_id: xiigen-b01-0b-059-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
   User request: "Document the social sharing API integration including authentication tokens, rate limits, and error codes."
B: DecisionCount: 2 | SubPhaseIds: ["3.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-059-004 — SocialShare: knowledge-edge (viral incident gap)

```
source_id: xiigen-b01-0b-059-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "The sharing button went viral and broke our API rate limit — scale the service to handle the spike."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"

Note: "API" is in ARCHITECTURE lexicon but incident/operations framing (scale, spike, broke) has no structural ARCHITECTURE verb — no predicate fires.
