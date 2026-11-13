# Layer 1 — Transition Orchestrator Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-004-001 — UI in vibe context: feature_addition TransitionType

**Predicates:** `["UI"]`

**Source request:** "Vibe out a settings page — nothing fancy."

**Gap warnings:** gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001

**Transition orchestrator output:**
```
TransitionType: feature_addition
FlowOrder: [UIFlow]
DomainId: "software-development"
GapMetadata: [gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001]
ContributionFlag: true
SessionState: active
```

**Note:** UI predicate results in feature_addition (new UI features are being added). The "nothing fancy" gap warning does not change the TransitionType.

---

## B32-L1-004-002 — UI feature_addition: all UI cases

**Rule:** TransitionType = feature_addition when UI is present.

| Request | TransitionType |
|---|---|
| "Build a settings screen" | feature_addition |
| "Vibe code a screen" | feature_addition |
| "Nothing fancy settings page" | feature_addition |
| "Rough-cut notification component" | feature_addition |

---

## B32-L1-004-003 — UI alone vs UI + IMPLEMENT: both feature_addition

**UI alone:**
```
TransitionType: feature_addition
FlowOrder: [UIFlow]
```

**UI + IMPLEMENT:**
```
TransitionType: feature_addition
FlowOrder: [UIFlow, BackendServiceFlow41]
ParallelEligible: true
```

Both result in feature_addition. The difference is in the FlowOrder and ParallelEligible properties.

---

## B32-L1-004-004 — Transition metadata for UI flows in vibe context

**The vibe gap metadata in UI transitions communicates:**

1. The UI work may be prototype/exploratory in nature
2. The implementation may not follow production standards
3. The session is flagged for potential review before merging

This is metadata — it does not change the routing. But downstream consumers (QA, review systems) can use it to apply appropriate scrutiny.

---
