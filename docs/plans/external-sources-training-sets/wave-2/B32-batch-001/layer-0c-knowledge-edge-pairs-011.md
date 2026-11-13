# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-011-001 — Mixed-signal handling: style framing never changes routing

**Knowledge edge:** When a request contains both registered predicates and B32 gap vocabulary, the output is always: [registered predicates] + gap warnings. The gap vocabulary is never allowed to change the predicate set or routing.

**The mixed-signal principle:**

```
Output.Predicates = {p | p fires on registered action vocabulary}
Output.GapWarnings = {g | g fires on gap vocabulary}
These two sets are computed independently and then combined.
```

---

## B32-0C-011-002 — Mixed-signal examples

### Example 1: IMPLEMENT + vibe gap

**Request:** "Vibe code a quick service."
- Predicates: IMPLEMENT (from "code" root)
- GapWarnings: gap-vibe-coding-style-001
- Mixed-signal: IMPLEMENT fires; vibe is gap warning
- Output: [IMPLEMENT] + gap warning

### Example 2: ARCHITECTURE + rough-cut gap

**Request:** "Rough-cut architecture design."
- Predicates: ARCHITECTURE (from "architecture" noun + "design" verb)
- GapWarnings: gap-vibe-coding-rough-cut-001
- Mixed-signal: ARCHITECTURE fires; rough-cut is gap warning
- Output: [ARCHITECTURE] + gap warning

### Example 3: UI + hack gap

**Request:** "Hack together a component for the settings."
- Predicates: UI (from "component")
- GapWarnings: gap-vibe-coding-hack-001
- Mixed-signal: UI fires; hack together is gap warning
- Output: [UI] + gap warning

### Example 4: TEST + quick-and-dirty gap

**Request:** "Quick and dirty unit tests for the payment module."
- Predicates: TEST (from "unit tests")
- GapWarnings: gap-vibe-coding-quick-dirty-001
- Mixed-signal: TEST fires; quick-and-dirty is gap warning
- Output: [TEST] + gap warning

---

## B32-0C-011-003 — Why mixed signals are not ambiguous

**Potential objection:** "Quick and dirty unit tests" might mean the developer doesn't want comprehensive tests. Should TEST be suppressed?

**Answer:** No. The routing system does not interpret quality intent. It detects action predicates. "Unit tests" fires TEST. What the developer means by "quick and dirty" is captured in the gap warning — it signals that the developer is operating in an informal mode and the test coverage may be incomplete. This is information for the learning system, not a routing modifier.

The routing system routes to the TEST flow. The TEST flow can receive the gap metadata and potentially adjust its behavior (e.g., generate fewer test cases, note that tests are provisional). But the routing decision itself is unchanged.

---

## B32-0C-011-004 — B32 knowledge-edge summary table

| Situation | Mixed signal? | Output |
|---|---|---|
| Only gap vocabulary | No mix | [] + gaps → knowledge_edge |
| Only action vocabulary | No mix | [predicates] → flow(s) |
| Action + gap vocabulary | Mixed | [predicates] + gap warnings → flow(s) |
| Gap vocabulary + "dashboard" | No mix (dashboard is gap too) | [] → knowledge_edge |
| "coding" (gerund) + "exploratory" | Mixed | [IMPLEMENT] + gap warning |

---
