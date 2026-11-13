# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-004-001 — Style qualifiers never suppress action predicates: rule with examples

**Knowledge edge:** In B32, style qualifiers (vibe vocabulary, framing words) are ALWAYS gap warnings. They NEVER suppress registered action predicates.

**The rule:**
```
For every style qualifier Q and every registered predicate P:
  If P fires in sentence S, then P fires in sentence S + Q.
  Q cannot prevent P from firing.
  Q emits a gap warning alongside the routing.
```

---

## B32-0C-004-002 — Examples showing style qualifiers do not suppress predicates

| Request | Style qualifier | Action predicate | Result |
|---|---|---|---|
| "implement a session handler" | none | implement → IMPLEMENT | [IMPLEMENT] |
| "just make it work — implement a session handler" | just make it work | implement → IMPLEMENT | [IMPLEMENT] + gap |
| "quick and dirty implement" | quick and dirty | implement → IMPLEMENT | [IMPLEMENT] + gap |
| "vibe code a screen" | vibe code | code → IMPLEMENT; screen → UI | [UI, IMPLEMENT] + gap |
| "throwaway — build the service" | throwaway | build → IMPLEMENT | [IMPLEMENT] + gap |
| "rough prototype — build it" | rough prototype | build → IMPLEMENT | [IMPLEMENT] + gap |
| "hack together the component" | hack together | component → UI | [UI] + gap |
| "nothing fancy — design the architecture" | nothing fancy | design → ARCHITECTURE | [ARCHITECTURE] + gap |

In every case, the style qualifier becomes a gap warning and the action predicate fires normally.

---

## B32-0C-004-003 — Why style qualifiers cannot suppress predicates (reasoning)

**Design rationale:**

The routing system is action-based. It responds to what the user wants to DO (build, implement, design, test), not how they want to do it (vibe, quick, rough, throwaway). Style qualifiers describe the quality or methodology — they belong to the gap/learning system, not the predicate system.

Allowing style qualifiers to suppress predicates would create:
1. Inconsistent routing: same action (implement) routes differently based on framing
2. Learned bad behavior: users discover they can suppress routing by adding framing words
3. Lost learning signals: gap vocabulary should generate learning events, not routing exceptions

Therefore: style qualifiers → gap warnings only. Predicates fire on action vocabulary unconditionally.

---

## B32-0C-004-004 — Contrast with B36/B37: style qualifiers in project management

**B36/B37 context:** In project management scenarios (B36/B37), "agile", "scrum", "sprint" are also gap vocabulary — unrecognized methodology terms. The same rule applies: they emit gap warnings but do not suppress PLAN, SCHEDULE, ASSIGN, or other project management predicates.

This rule is cross-domain: style qualifiers (methodology, quality, urgency) NEVER suppress predicates in any scenario.

---
