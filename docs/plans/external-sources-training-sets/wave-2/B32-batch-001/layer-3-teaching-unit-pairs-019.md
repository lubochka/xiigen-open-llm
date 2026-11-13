# Layer 3 — Teaching Unit Pairs (019)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-019-001 — "fast and loose" describes work style, not action: complete rule

**Teaching unit:** "Fast and loose" communicates a low-rigor, informal working style. It has no action verb embedded in it.

**Core rule:**
```
"fast and loose" alone → gap-vibe-coding-style-001 → zero predicates
"fast and loose" + action verb → predicate fires + gap-style-001 warning
```

---

## B32-L3-019-002 — Examples: fast and loose alone vs with action verb

**Zero-predicate:**

| Request | Predicates | Notes |
|---|---|---|
| "Fast and loose approach" | [] | "approach" is not action verb |
| "Take it fast and loose" | [] | "take" is not IMPLEMENT |
| "Fast and loose methodology" | [] | "methodology" is domain noun |

**Predicate-firing:**

| Request | Predicate | Notes |
|---|---|---|
| "Fast and loose + build" | IMPLEMENT | "build" fires |
| "Fast and loose + design" | ARCHITECTURE | "design" fires |
| "Fast and loose coding" | IMPLEMENT | "coding" root fires IMPLEMENT |
| "Fast and loose + implementation" | IMPLEMENT | root fires |

---

## B32-L3-019-003 — "fast and loose + implementation" — the root match case

**Request:** "Play it fast and loose with the implementation details for now."

- "fast and loose" → gap-vibe-coding-style-001
- "implementation" → root "implement" → IMPLEMENT fires

**Teaching:** The root matching rule applies even inside a noun phrase ("implementation details"). "Implementation" contains "implement" root. IMPLEMENT fires.

**Common mistake to avoid:** Treating "implementation details" as pure domain noun when "implementation" roots to "implement". Always apply root matching.

---
