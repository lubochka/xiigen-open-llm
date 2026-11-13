# Layer 0-B — Router Pairs (055)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0b-router-pairs

---

## B32-0B-055-001 — complete routing decision tree

**Router decision logic for B32:**

```
Step 1: Scan for gap vocabulary → build GapWarnings list
Step 2: Scan for action verbs → build Predicates list (negations reduce this list)
Step 3: Route:
  - If Predicates = [] AND GapWarnings non-empty → knowledge_edge + SCL
  - If Predicates = [] AND GapWarnings = [] → normal empty (no SCL, no gap warning)
  - If Predicates non-empty → invoke factories for each predicate
    - Attach GapMetadata to each flow if GapWarnings non-empty
Step 4: Always set DomainId = "software-development"
```

---

## B32-0B-055-002 — routing with negations: negations reduce predicate list

**Negation effect on routing:**

Negations are processed before routing. The Predicates list is the result AFTER negations are applied.

"Lean — implement, skip tests":
- Before negation processing: candidates = [IMPLEMENT, TEST(negated)]
- After negation processing: Predicates = ["IMPLEMENT"]
- Router receives Predicates = ["IMPLEMENT"]

---

## B32-0B-055-003 — routing with mixed gap IDs

**Multiple gap IDs do not affect routing decision:**

Whether GapWarnings has 1 or 6 entries, the routing decision is the same:
- Predicates = [] → knowledge_edge + SCL (once)
- Predicates non-empty → active flows + gap metadata

---
