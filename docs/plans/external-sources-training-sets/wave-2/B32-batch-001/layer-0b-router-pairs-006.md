# Layer 0-B — Router Pairs (006)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-006-001 — [IMPLEMENT] → 4.1: prototype + build routing

**Predicates received:** `["IMPLEMENT"]`

**Source request:** "Build a rough prototype of the checkout flow — throwaway code is fine."

**Gap warnings present:** gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001

**Router output:**
```
RouteTarget: BackendServiceFlow41
FlowSelected: [BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-prototype-001, gap-vibe-coding-throwaway-001]
DomainId: "software-development"
```

**Note:** "prototype" in the gap warnings signals to downstream consumers that the resulting artifact may be throwaway/exploratory. The routing itself is identical to any other IMPLEMENT request. The orchestrator passes gap metadata to the flow factory for potential flagging.

---

## B32-0B-006-002 — Gap metadata propagation: prototype flag downstream

**When gap-vibe-coding-prototype-001 is present with IMPLEMENT routing:**

The flow factory (layer 0-D) and transition orchestrator (layer 1) receive the gap metadata. This allows downstream components to:
- Flag the output as prototype/exploratory
- Set lower confidence thresholds
- Note in session metadata that this is throwaway work

However, the ROUTING DECISION (which flow) is unchanged. BackendServiceFlow41 is still selected.

---

## B32-0B-006-003 — [IMPLEMENT] → 4.1: "create a prototype" routing

**Predicates received:** `["IMPLEMENT"]`

**Source request:** "Create a quick prototype of the payment flow."

**Gap warnings present:** gap-vibe-coding-prototype-001

**Router output:**
```
RouteTarget: BackendServiceFlow41
FlowSelected: [BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-prototype-001]
DomainId: "software-development"
```

---

## B32-0B-006-004 — Prototype routing: comparison table

| Request pattern | Action verb | Predicates | RouteTarget |
|---|---|---|---|
| "A rough prototype" | none | [] | knowledge_edge |
| "Build a rough prototype" | build | [IMPLEMENT] | BackendServiceFlow41 |
| "Create a prototype" | create | [IMPLEMENT] | BackendServiceFlow41 |
| "Write a prototype" | write | [IMPLEMENT] | BackendServiceFlow41 |
| "Implement a prototype" | implement | [IMPLEMENT] | BackendServiceFlow41 |

The presence of an action verb is the sole determinant of whether knowledge_edge or BackendServiceFlow41 is selected.

---
