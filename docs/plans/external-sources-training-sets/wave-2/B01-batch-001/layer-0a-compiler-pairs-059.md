# Layer 0-A — Compiler Pairs (059)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-059-001 — SocialShare: "build a social share component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-059-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a social share component with Twitter, LinkedIn, and copy-to-clipboard button variants."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "button"
  IMPLEMENT fires on: "build"
  "social" NOT in UI lexicon; "share" NOT in UI lexicon — fires via "component" and "button".
  TransitionType: feature_addition.

---

## B01-0A-059-002 — SocialShare: "create a share widget with OG preview" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-059-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a share widget with Open Graph preview, custom message input, and platform icon buttons."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "widget", "input", "button"
  IMPLEMENT fires on: "create"
  Three UI lexicon hits; high confidence.
  TransitionType: feature_addition.

---

## B01-0A-059-003 — SocialShare: "document sharing API integration" fires ARCHITECTURE+DOCUMENT

```
source_id: xiigen-b01-0a-059-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the social sharing API integration including authentication tokens, rate limits, and error codes."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Confidence: 0.90

Predicate evidence:
  ARCHITECTURE fires on: "API", "authentication", "integration"
  DOCUMENT fires on: "document"
  No IMPLEMENT verb present.
  TransitionType: documentation.

---

## B01-0A-059-004 — SocialShare: "sharing button went viral and broke our API" fires nothing

```
source_id: xiigen-b01-0a-059-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "The sharing button went viral and broke our API rate limit — scale the service to handle the spike."
  CompiledPredicateIds: []
  Confidence: 0.44

Predicate evidence:
  "went viral" NOT in any predicate lexicon.
  "broke" NOT in any predicate lexicon.
  "rate limit" NOT in any predicate lexicon.
  "scale" NOT in any predicate lexicon.
  "spike" NOT in any predicate lexicon.
  Note: "API" IS in ARCHITECTURE lexicon, BUT "scale" is not an ARCHITECTURE verb — ARCHITECTURE requires structural terms. "broke our API" is incident language, not architecture language.
  CompiledPredicateIds: [] → knowledge-edge path.
