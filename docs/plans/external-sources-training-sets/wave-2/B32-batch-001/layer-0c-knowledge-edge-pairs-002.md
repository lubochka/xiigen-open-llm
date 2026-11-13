# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-002-001 — "prototype" vs "build a prototype": action verb required for IMPLEMENT

**Knowledge edge:** The word "prototype" alone does not fire IMPLEMENT. An action verb must co-occur.

**Rule:**
- "prototype" alone → gap-vibe-coding-prototype-001 → zero predicates → knowledge_edge
- "prototype" + action verb → IMPLEMENT fires on the action verb; "prototype" → gap warning

**Examples:**

| Request | Predicates | Reason |
|---|---|---|
| "A prototype of the service" | [] | "prototype" alone; no action verb |
| "Build a prototype of the service" | [IMPLEMENT] | "build" fires IMPLEMENT |
| "Create a quick prototype" | [IMPLEMENT] | "create" fires IMPLEMENT |
| "Write a prototype" | [IMPLEMENT] | "write" fires IMPLEMENT |
| "Implement a prototype" | [IMPLEMENT] | "implement" fires IMPLEMENT |
| "Rough prototype — nothing fancy" | [] | "prototype" alone; adjectives are not action verbs |

---

## B32-0C-002-002 — Why "prototype" alone does not fire IMPLEMENT

**Reasoning:**

"Prototype" is a noun describing the artifact type, not the action to be performed.

- In "Build a prototype": "build" is the action → IMPLEMENT fires on "build"
- In "A prototype": there is no action stated → the system cannot determine what to do
- In "I need a prototype": "need" is not in IMPLEMENT lexicon → no predicate fires

This mirrors how other artifact nouns work:
- "A service" → no predicate (no action)
- "Build a service" → IMPLEMENT (action present)
- "A prototype" → no predicate
- "Build a prototype" → IMPLEMENT

---

## B32-0C-002-003 — "prototype" as style qualifier in action-verb sentences

**When "prototype" accompanies an action verb, it functions as a style qualifier:**

"Build a rough prototype of the checkout flow — throwaway code is fine."

In this sentence:
- "build" = action verb → fires IMPLEMENT
- "prototype" = style/type qualifier → gap-vibe-coding-prototype-001 warning
- "rough" = additional style qualifier
- "throwaway code" = additional gap marker → gap-vibe-coding-throwaway-001

The sentence structure: [action verb] + [artifact type with style qualifier]
IMPLEMENT fires on the action verb. Style qualifiers (including "prototype") emit warnings.

---
