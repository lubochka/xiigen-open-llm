# Layer 3 — Teaching Unit Pairs (041)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-041-001 — Teaching: TransitionType precedence (complete rule)

**Teaching unit:** TransitionType selection

Priority order (highest to lowest):
1. IMPLEMENT or UI present → feature_addition
2. ARCHITECTURE, API_CONTRACT, or DOCUMENT present (no IMPLEMENT/UI) → documentation
3. Only TEST, QA, SECURITY present → no_contribution
4. Predicates = [] → unknown (knowledge_edge)

**Example:** ["TEST", "DOCUMENT"] → documentation (rule 2; IMPLEMENT absent).

---

## B32-L3-041-002 — Teaching: gap qualifier with multi-predicate requests

**Teaching unit:** Gap qualifier never prevents multi-predicate firing

When a gap qualifier appears with multiple action verbs:
- Each action verb fires its predicate independently
- The gap qualifier adds to GapWarnings once (deduplicated)
- CapabilityGapWarning = "style_qualifier_noted" appears once in metadata

A request with 8 action verbs and one gap qualifier fires 8 predicates + one gap warning.

---

## B32-L3-041-003 — Teaching: DOCUMENT predicate trigger summary

**Teaching unit:** DOCUMENT fires from explicit document verbs

| Expression | DOCUMENT fires? |
|---|---|
| "document X" | YES |
| "write docs" | YES |
| "create documentation" | YES |
| "add docs" | YES |
| "document the contract" | YES (DOCUMENT + API_CONTRACT both fire) |
| "docs" as noun only | NO |
| "no docs needed" | NO (negation) |
| "we need docs" | NO (desire, not imperative) |

---

## B32-L3-041-004 — Teaching: GapMetadata applied to every flow

**Teaching unit:** GapMetadata propagation

When multiple factories are invoked:
- The same GapMetadata object is applied to every flow created
- No factory can discard or modify GapMetadata
- GapMetadata is not split or averaged across factories

All flows carry identical gap information.

---
