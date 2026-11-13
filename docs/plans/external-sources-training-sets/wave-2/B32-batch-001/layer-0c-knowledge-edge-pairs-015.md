# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-015-001 — "quick win" with action verb: IMPLEMENT fires; style is warning

**Knowledge edge:** "quick win" alone → gap. "quick win" + action verb → predicate fires.

**Rule:**
```
IF "quick win" appears WITHOUT an action verb
THEN predicates = [] AND gap-vibe-coding-style-001 emitted

IF "quick win" appears WITH an action verb
THEN predicate fires AND gap-vibe-coding-style-001 is a warning
```

**Examples:**

| Request | Predicates | Notes |
|---|---|---|
| "A quick win" | [] | No action verb |
| "Quick win for the team" | [] | No action verb; "for the team" is context |
| "Build a quick win" | [IMPLEMENT] | "build" fires |
| "Implement a quick win" | [IMPLEMENT] | "implement" fires |
| "Document the quick win" | [DOCUMENT] | "document" fires |
| "Test this quick win" | [TEST] | "test" fires |

---

## B32-0C-015-002 — "quick win" is not in IMPLEMENT lexicon

**Why "quick win" does not fire IMPLEMENT:**

"Quick win" describes the scope or value of the outcome (small, fast, high-value). It does not describe what to do with the code. "Win" is not in the IMPLEMENT lexicon (implement, build, create, write, code).

Analogy: "A good feature" does not fire IMPLEMENT. "Build a good feature" fires IMPLEMENT. Similarly, "quick win" alone does not fire, but "build a quick win" fires on "build".

---

## B32-0C-015-003 — "quick" alone: not a gap term; not a predicate trigger

**Distinction:**
- "quick win" → gap-vibe-coding-style-001 (compound phrase, registered)
- "quick" alone (adjective) → no gap ID; no predicate
- "quick prototype" → "prototype" alone → gap-vibe-coding-prototype-001 only
- "quickly implement" → "implement" fires IMPLEMENT; "quickly" is an adverb modifier

**Teaching:** Gap vocabulary sensitivity is at the phrase level. "quick" alone is not registered. "quick win" as a compound phrase is registered.

---
