# Layer 0-A — Compiler Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: compiler

---

## B01-0A-012-001 — B01 compiler pattern summary: complete predicate rule table

```
source_id: xiigen-b01-0a-012-summary
source_title: XIIGen B01 compiler-012 — complete predicate rule table and pattern summary
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

### UI predicate: complete lexicon (21 terms)

| Term | Notes |
|---|---|
| ui | always fires UI |
| interface | always fires UI |
| ux | always fires UI |
| screen | always fires UI |
| page | always fires UI |
| modal | always fires UI |
| form | always fires UI |
| layout | always fires UI |
| component | always fires UI |
| responsive | always fires UI |
| accessibility | always fires UI (B01 rule 11) |
| visual | always fires UI |
| wireframe | always fires UI |
| prototype | always fires UI in B01 (NOT gap vocabulary in B01; contrast with B32) |
| interaction | always fires UI |
| button | always fires UI |
| input | always fires UI |
| typography | always fires UI |
| color | always fires UI |
| spacing | always fires UI |
| design-system | fires UI as compound term; "design system" (two words) also fires UI |
| theme | always fires UI |

Total: 22 entries (design-system counts as one term regardless of hyphenation).

### DOCUMENT predicate: lexicon

| Term | Notes |
|---|---|
| document | always fires DOCUMENT |
| spec | fires DOCUMENT |
| specification | fires DOCUMENT |
| requirement | fires DOCUMENT (singular or plural) |
| story | fires DOCUMENT |
| criteria | fires DOCUMENT |
| describe | fires DOCUMENT |
| define | fires DOCUMENT |
| functional | fires DOCUMENT |
| behavior | fires DOCUMENT |
| use-case | fires DOCUMENT (hyphenated or "use case") |
| narrative | fires DOCUMENT |
| feature-brief | fires DOCUMENT |
| capability | fires DOCUMENT |
| epics | fires DOCUMENT |

### IMPLEMENT predicate: lexicon (partial, relevant terms)

| Term | Notes |
|---|---|
| implement | always fires IMPLEMENT |
| code | fires IMPLEMENT |
| build | fires IMPLEMENT |
| develop | fires IMPLEMENT |
| write | fires IMPLEMENT (regardless of what is being written) |
| create | fires IMPLEMENT |
| function | fires IMPLEMENT |
| class | fires IMPLEMENT |
| logic | fires IMPLEMENT |
| library | fires IMPLEMENT |
| handler | fires IMPLEMENT |
| middleware | fires IMPLEMENT |
| controller | fires IMPLEMENT |
| backend | fires IMPLEMENT |
| feature | fires IMPLEMENT |
| method | fires IMPLEMENT |
| refactor | fires IMPLEMENT |

### Terms that do NOT fire any predicate in B01

| Term | Common misconception |
|---|---|
| navigation | looks like UI but is NOT in the UI lexicon |
| onboarding | context word only |
| signup | context word only |
| email activation | context word only |
| first-run | context word only |
| journey | context word only |
| redesign | not in IMPLEMENT or UI lexicons |
| modern | not in any lexicon |
| engaging | not in any lexicon |
| flow | not in any lexicon |
| audit | not in any lexicon |
| review | not in any lexicon |

---

## B01-0A-012-002 — Dual-predicate rule summary

```
source_id: xiigen-b01-0a-012-dual
source_title: XIIGen B01 compiler-012 — dual-predicate firing rules
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

Dual-predicate rule: when a single request contains terms from two different
predicate lexicons, BOTH predicates must appear in CompiledPredicateIds. Neither
suppresses the other.

Key dual-predicate examples:
  "write" + "story" → IMPLEMENT (write) + DOCUMENT (story)
  "document" + "screen" → DOCUMENT (document) + UI (screen)
  "define" + "interface" → DOCUMENT (define) + UI (interface)
  "implement" + "component" → IMPLEMENT (implement) + UI (component)
  "design" + "architecture" → ARCHITECTURE fires; if "screen" also present → UI fires too
  "create" + "specification" → IMPLEMENT (create) + DOCUMENT (specification)

The correct compiler output always includes all fired predicates. The corrupt
output suppresses one or more predicates (Corruption-v6-02).

---

## B01-0A-012-003 — TransitionType determination summary

```
source_id: xiigen-b01-0a-012-transition
source_title: XIIGen B01 compiler-012 — TransitionType rules
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

TransitionType selection rules for B01:
  feature_addition — IMPLEMENT predicate present (any other predicates may also be present)
  documentation    — DOCUMENT present without IMPLEMENT
  feature_addition — UI present without DOCUMENT and without IMPLEMENT
    (creating new UI artifacts = feature_addition)
  documentation    — ARCHITECTURE present without IMPLEMENT
    (architecture diagrams and specs = documentation)
  feature_addition — ARCHITECTURE present with IMPLEMENT

Summary table:

| Predicates | TransitionType |
|---|---|
| [UI] | feature_addition |
| [DOCUMENT] | documentation |
| [UI, DOCUMENT] | feature_addition (UI drives it) |
| [UI, IMPLEMENT] | feature_addition |
| [DOCUMENT, IMPLEMENT] | feature_addition |
| [UI, DOCUMENT, IMPLEMENT] | feature_addition |
| [ARCHITECTURE] | documentation |
| [ARCHITECTURE, UI] | documentation (no IMPLEMENT) |
| [ARCHITECTURE, IMPLEMENT] | feature_addition |
| [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] | feature_addition |
