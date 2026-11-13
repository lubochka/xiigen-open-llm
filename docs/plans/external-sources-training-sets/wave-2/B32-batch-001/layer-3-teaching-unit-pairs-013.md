# Layer 3 — Teaching Unit Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-013-001 — "scrappy" is a quality modifier, not an action: complete rule

**Teaching unit:** "Scrappy" tells the system how the developer wants to work (low-polish, fast, rough). It does not tell the system what technical action to perform. The system routes on action verbs, not on quality preferences.

**Core rule:**
```
IF "scrappy" appears WITHOUT an action verb
THEN predicates = [] AND gap-vibe-coding-style-001 is emitted

IF "scrappy" appears WITH an action verb
THEN predicate fires on the action verb AND gap-vibe-coding-style-001 is a warning
```

---

## B32-L3-013-002 — Examples: scrappy alone vs scrappy + action verb

**Zero-predicate examples:**

| Request | Why zero predicates |
|---|---|
| "Scrappy solution" | No action verb |
| "Scrappy prototype" | "prototype" alone; no action verb |
| "Something scrappy" | Style descriptor only |
| "Keep it scrappy" | "keep" is not IMPLEMENT |

**Predicate-firing examples:**

| Request | Action verb | Predicate |
|---|---|---|
| "Scrappy build" | build | IMPLEMENT |
| "Scrappy implementation" | implementation (root) | IMPLEMENT |
| "Scrappy design" | design | ARCHITECTURE |
| "Scrappy code" | code | IMPLEMENT |

---

## B32-L3-013-003 — Style qualifiers at sentence level: compound request teaching

**Request:** "Scrappy but functional — build the service, test it, and document the API."

All of these predicates fire despite "scrappy" framing:
- "build" → IMPLEMENT
- "test" → TEST
- "document" → DOCUMENT
- "API" → API_CONTRACT

**Result:** [IMPLEMENT, TEST, DOCUMENT, API_CONTRACT] + gap-vibe-coding-style-001

Style qualifier at sentence level is a single warning for the whole request, not per-predicate.

---
