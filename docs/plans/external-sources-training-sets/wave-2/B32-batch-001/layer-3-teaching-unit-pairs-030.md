# Layer 3 — Teaching Unit Pairs (030)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-030-001 — Teaching: gap + ARCHITECTURE

**Teaching unit:** gap vocabulary with ARCHITECTURE predicate

ARCHITECTURE fires from action verbs: "architect", "design", "structure", "plan the architecture".

ARCHITECTURE does NOT fire from the noun "architecture" alone.

**Gap co-occurrence rule:** Gap qualifier + ARCHITECTURE verb → ARCHITECTURE fires + gap warning.

---

## B32-L3-030-002 — Teaching: "design" verb as ARCHITECTURE trigger

"Design" is in the ARCHITECTURE lexicon when used with architecture-scope objects (modules, layers, services, system, infrastructure).

**Boundary:** "design a logo" or "design the UI" routes differently (UI for screen/interface contexts).

In B32 requests with gap qualifiers, "design" almost always indicates ARCHITECTURE unless "UI" or "screen" is the object.

---

## B32-L3-030-003 — Teaching: ARCHITECTURE + API_CONTRACT → documentation TransitionType

When both ARCHITECTURE and API_CONTRACT are present (no IMPLEMENT):
- TransitionType = documentation
- Neither overrides the other; both produce documentation
- IMPLEMENT overrides both to feature_addition if present

---

## B32-L3-030-004 — Teaching: three qualifiers same gap ID → one GapWarnings entry

When multiple qualifiers all map to gap-vibe-coding-style-001:
- GapWarnings = [gap-vibe-coding-style-001] (deduplicated, one entry)
- SCHEDULE_CAPABILITY_LEARNING emitted once
- CapabilityGapWarning = "style_qualifier_noted" (not multiplied)

This holds for any number of qualifiers sharing the same gap ID.

---
