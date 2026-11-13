# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-013-001 — "scrappy" is a style qualifier: does not fire any predicate

**Knowledge edge:** "scrappy" communicates low-polish intent. The system has no SCRAPPY predicate and cannot route on style preference.

**Rule:**
```
IF "scrappy" appears WITHOUT an action verb
THEN predicates = [] AND gap-vibe-coding-style-001 emitted

IF "scrappy" appears WITH an action verb
THEN predicate fires on the action verb AND gap-vibe-coding-style-001 is a warning
```

**Examples:**

| Request | Predicates | Reason |
|---|---|---|
| "Scrappy solution" | [] | No action verb |
| "Scrappy prototype" | [] | "prototype" alone + style qualifier |
| "Scrappy build" | [IMPLEMENT] | "build" fires IMPLEMENT |
| "Scrappy implementation" | [IMPLEMENT] | "implementation" root fires IMPLEMENT |
| "Scrappy design" | [ARCHITECTURE] | "design" fires ARCHITECTURE |

---

## B32-0C-013-002 — Why "scrappy" is gap vocabulary

**Reasoning:**

"Scrappy" communicates the developer's quality expectation, not the technical action to be performed. The system routes on what to do, not on how polished the result should be.

- In "scrappy build": "build" = action → IMPLEMENT fires; "scrappy" = quality modifier → gap warning
- In "scrappy prototype": no action verb → zero predicates; "scrappy" and "prototype" both produce gap IDs
- In "scrappy design": "design" = action → ARCHITECTURE fires; "scrappy" = quality modifier

**Implication:** Any response that treats "scrappy" as a synonym for any action predicate is incorrect.

---

## B32-0C-013-003 — "scrappy" in compound requests: each action verb fires its predicate

**Request:** "Scrappy but functional — build the service, test it, and document the API."

- "scrappy" → gap-vibe-coding-style-001
- "build" → IMPLEMENT fires
- "test" → TEST fires
- "document" → DOCUMENT fires
- "API" → API_CONTRACT fires

**Result:** `["IMPLEMENT", "TEST", "DOCUMENT", "API_CONTRACT"]`

**GapWarnings:** gap-vibe-coding-style-001

**Teaching:** Style qualifiers at the sentence level apply as warnings across the whole request. They do not suppress any of the individual action predicates.

---
