# Layer 3 — Teaching Unit Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-002-001 — "prototype" requires action verb: complete rule with examples

**Teaching unit:** The word "prototype" is a NOUN describing an artifact type, not an action verb. The system routes on actions, not artifact types. Therefore "prototype" alone does not fire any predicate.

**Core rule:**
```
IF "prototype" appears WITHOUT an action verb (build/create/write/implement/code)
THEN predicates = [] AND gap-vibe-coding-prototype-001 is emitted

IF "prototype" appears WITH an action verb
THEN IMPLEMENT fires on the action verb AND gap-vibe-coding-prototype-001 is emitted
```

---

## B32-L3-002-002 — Examples: prototype alone vs prototype + action verb

**Zero-predicate examples (prototype alone):**

| Request | Why zero predicates |
|---|---|
| "A rough prototype" | No action verb |
| "Prototype for the checkout flow" | No action verb |
| "Quick prototype — nothing fancy" | "quick" and "nothing fancy" are not action verbs |
| "Just a prototype" | "just" is not an action verb |
| "Rough prototype, throwaway" | No action verb |

**IMPLEMENT-firing examples (prototype + action verb):**

| Request | Action verb | Result |
|---|---|---|
| "Build a rough prototype" | build | [IMPLEMENT] + gap-prototype-001 |
| "Create a quick prototype" | create | [IMPLEMENT] + gap-prototype-001 |
| "Write a prototype" | write | [IMPLEMENT] + gap-prototype-001 |
| "Implement a prototype" | implement | [IMPLEMENT] + gap-prototype-001 |
| "Code a prototype" | code | [IMPLEMENT] + gap-prototype-001 |

---

## B32-L3-002-003 — Teaching point: why noun artifact types don't fire predicates

**Analogy:** "A car" does not tell a mechanic what to do. "Build a car" tells the mechanic to build. "Fix a car" tells the mechanic to fix. The noun alone is insufficient.

Similarly: "A prototype" does not tell the system what to do with it. "Build a prototype" tells the system to build. "Test a prototype" (if TEST lexicon included "prototype") would tell the system to test.

**Application to B32:** Always check for action verbs FIRST. If present, predicates fire. If absent, check for gap vocabulary. "Prototype" alone → gap → knowledge_edge.

---
