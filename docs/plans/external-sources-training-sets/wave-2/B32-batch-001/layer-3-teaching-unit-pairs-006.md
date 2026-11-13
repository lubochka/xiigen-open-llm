# Layer 3 — Teaching Unit Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-006-001 — "spike": technical spike vocabulary in gap; complete list

**Teaching unit:** "Spike" is technical slang for a time-boxed investigation or proof-of-concept exercise in software development. It is not in any predicate lexicon. It is recognized as B32 gap vocabulary.

**Core rule:**
```
"spike" → gap-vibe-coding-spike-001
Not in: IMPLEMENT lexicon, ARCHITECTURE lexicon, or any other lexicon
Always produces a gap warning
Zero predicates when "spike" is the primary verb
```

---

## B32-L3-006-002 — Complete spike vocabulary forms

| Form | Gap ID | Example |
|---|---|---|
| spike (verb) | gap-vibe-coding-spike-001 | "spike the solution" |
| spike (noun) | gap-vibe-coding-spike-001 | "run a spike" |
| "run a spike" | gap-vibe-coding-spike-001 | "run a spike on the API" |
| "spike on X" | gap-vibe-coding-spike-001 | "spike on the best approach" |
| "do a spike" | gap-vibe-coding-spike-001 | "do a spike for the auth module" |
| "technical spike" | gap-vibe-coding-spike-001 | "technical spike for the recommendation engine" |

All forms produce gap-vibe-coding-spike-001 regardless of surrounding context.

---

## B32-L3-006-003 — Spike with co-present action verbs

**When "spike" appears with a registered action verb, the predicate fires on the action verb:**

| Request | Predicate | Gap |
|---|---|---|
| "Spike the design" | ARCHITECTURE (from "design") | gap-spike-001 |
| "Run a spike and implement the solution" | IMPLEMENT (from "implement") | gap-spike-001 |
| "Spike and then build the service" | IMPLEMENT (from "build") | gap-spike-001 |

"Spike" never contributes to predicates. The action verb carries the predicate; "spike" contributes the gap warning.

---

## B32-L3-006-004 — Why "spike" is gap: predicate design rationale

**Spikes are investigations, not deliverables.** A spike produces knowledge (understanding of an approach) but not code. The system routes on what is to be built/implemented/designed. A spike request says "investigate" but the system has no INVESTIGATE predicate.

**Learning signal:** gap-vibe-coding-spike-001 signals that the system needs to learn how to handle investigation requests. Possible future predicate: INVESTIGATE or RESEARCH.

Until such a predicate is added, "spike" alone → knowledge_edge, which is the correct safe fallback.

---
