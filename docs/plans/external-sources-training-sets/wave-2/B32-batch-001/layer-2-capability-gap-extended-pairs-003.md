# Layer 2 — Capability Gap Extended Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-2-capability-gap-extended-pairs

---

## B32-L2-003-001 — "prototype alone" vs "build prototype": side-by-side comparison trace

**Case A: "prototype" alone (zero predicates)**

**Request A:** "A rough prototype of the checkout flow."

**Full trace A:**

Layer 0-A:
```
Predicates: []
Reason: "prototype" = gap vocab; no action verb present
GapWarnings: [gap-vibe-coding-prototype-001]
```

Layer 0-B:
```
RouteTarget: knowledge_edge
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: emit
```

Layer 2:
```
CapabilityGapType: vocabulary_gap
UnrecognizedTerms: ["prototype"]
Gap: "prototype" alone is unrecognized artifact type (not an action)
LearningSignal: true
```

**Result A:** knowledge_edge

---

**Case B: "build a prototype" (IMPLEMENT fires)**

**Request B:** "Build a rough prototype of the checkout flow."

**Full trace B:**

Layer 0-A:
```
Predicates: ["IMPLEMENT"]
Reason: "build" fires IMPLEMENT; "prototype" is gap qualifier
GapWarnings: [gap-vibe-coding-prototype-001, gap-vibe-coding-rough-cut-001]
```

Layer 0-B:
```
RouteTarget: BackendServiceFlow41
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
```

Layer 1:
```
TransitionType: feature_addition
```

**Result B:** [IMPLEMENT] → BackendServiceFlow41 + gap warnings

---

## B32-L2-003-002 — Side-by-side comparison

| Property | Case A (prototype alone) | Case B (build prototype) |
|---|---|---|
| Predicates | [] | ["IMPLEMENT"] |
| Route | knowledge_edge | BackendServiceFlow41 |
| SCHEDULE_CAPABILITY_LEARNING | Yes | No |
| TransitionType | unknown | feature_addition |
| Gap warnings | [prototype-001] | [prototype-001, rough-cut-001] |
| DomainId | software-development | software-development |
| Difference | No action verb | "build" is the action verb |

**The single token "build" changes the entire routing outcome.** Without an action verb, the system cannot determine what to do. With "build", IMPLEMENT fires and the request routes to a productive flow.

---
