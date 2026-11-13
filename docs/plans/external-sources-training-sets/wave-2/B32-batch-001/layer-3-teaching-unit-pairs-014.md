# Layer 3 — Teaching Unit Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-014-001 — "rough cut" is a compound gap term: both forms registered

**Teaching unit:** The compound phrase "rough cut" (and its hyphenated form "rough-cut") is registered as gap vocabulary. The standalone adjective "rough" is NOT registered as a gap term.

**Rule:**
```
"rough cut" or "rough-cut" → gap-vibe-coding-rough-cut-001
"rough" alone → general style adjective; no registered gap ID in B32
```

---

## B32-L3-014-002 — Examples: rough cut alone vs rough cut + action verb

**Zero-predicate (rough cut alone, no action):**

| Request | Predicates | Notes |
|---|---|---|
| "A rough cut" | [] | No action verb |
| "Rough cut pipeline" | [] | "pipeline" is a domain noun |
| "Rough cut of the API" | [] | No action verb; API is noun here |

**Predicate-firing (rough cut + action verb):**

| Request | Action verb | Predicate |
|---|---|---|
| "Build a rough cut" | build | IMPLEMENT |
| "Rough cut implementation" | implementation (root) | IMPLEMENT |
| "Rough-cut design" | design | ARCHITECTURE |
| "Rough cut + screen" (no verb) | screen fires UI | UI only |

---

## B32-L3-014-003 — Why rough cut is treated as gap rather than "close enough to implement"

**Reasoning:**

"Rough cut" describes the quality bar of the expected output — it does not specify what technical action to perform. Without an action verb:
- The system cannot determine whether to implement, design, test, or document
- "Rough cut" alone is analogous to "a prototype" — a quality modifier on an implied but unstated artifact
- Adding an action verb ("build a rough cut") immediately resolves the ambiguity

The gap warning alerts downstream learning that the user is in a low-formality mode.

---
