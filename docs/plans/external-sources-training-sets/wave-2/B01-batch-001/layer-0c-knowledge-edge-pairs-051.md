# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B01-0C-051-001 — RichTextEditor: "ux" fires UI even in casual phrasing

```
source_id: xiigen-b01-0c-051-001
source_license_state: first_party
pair_kind: knowledge_edge
```

Boundary case from file 051:
  "The editor feels clunky — fix the UX" → CompiledPredicateIds: ["UI"]
  Reason: "ux" (case-insensitive) IS in UI lexicon — fires UI.
  "fix" NOT in IMPLEMENT lexicon (gap verb) → IMPLEMENT does NOT fire.
  This is NOT a knowledge-edge case — it routes to 1.1, TransitionType: documentation.

Contrast with true knowledge-edge:
  "The editor is laggy — profile and tune the keystroke handler" → KNOWLEDGE_EDGE.
  No UI lexicon term; "profile" and "tune" are gap verbs.

Key rule: "ux" anywhere in request (even casual complaint) fires UI predicate.
"fix" does NOT fire IMPLEMENT. "fix the UX" → UI only.

---

## B01-0C-051-002 — "write" as IMPLEMENT verb — context dependency

```
source_id: xiigen-b01-0c-051-002
source_license_state: first_party
pair_kind: knowledge_edge
```

"write" as IMPLEMENT verb parsing:
  "write code for the component" → IMPLEMENT fires (code artifact context).
  "write unit tests" → IMPLEMENT + TEST both fire.
  "write the feature specification" → DOCUMENT fires on "specification"; IMPLEMENT fires on "write".
  "write a design document" → DOCUMENT fires on "document"; IMPLEMENT fires on "write".
  
Ambiguous "write" (DOCUMENT+IMPLEMENT both fire):
  "write user stories" → DOCUMENT + IMPLEMENT (special rule, confirmed).
  "write requirements" → DOCUMENT fires on "requirements"; IMPLEMENT fires on "write".
  
Gap "write" (DOCUMENT fires, IMPLEMENT does NOT):
  Does NOT exist — "write" always fires IMPLEMENT when present.
  
Rule: "write" ALWAYS fires IMPLEMENT when it appears as a verb.
Co-predicates (DOCUMENT, TEST) may also fire based on the object of "write".
