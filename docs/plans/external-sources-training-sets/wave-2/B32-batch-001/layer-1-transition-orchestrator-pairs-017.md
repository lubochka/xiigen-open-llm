# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-017-001 — "low-fi" alone: knowledge_edge

**Predicates:** `[]`

**Gap context:** gap-vibe-coding-style-001 ("low-fi")

**Output:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: ["gap-vibe-coding-style-001"]
ContributionFlag: false
```

---

## B32-L1-017-002 — "low-fi + prototype" → knowledge_edge; both gap

**Predicates:** `[]`

**Gap context:** gap-vibe-coding-style-001, gap-vibe-coding-prototype-001

**Output:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: ["gap-vibe-coding-style-001", "gap-vibe-coding-prototype-001"]
ContributionFlag: false
```

---

## B32-L1-017-003 — "low-fi build + page" → TransitionType feature_addition

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-style-001

**Output:**
```
TransitionType: feature_addition
SessionState: active
DomainId: "software-development"
GapIds: ["gap-vibe-coding-style-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-017-004 — "low-fi design + interface" → TransitionType documentation (ARCHITECTURE)

**Predicates:** `["ARCHITECTURE", "UI"]`

**Gap context:** gap-vibe-coding-style-001

**Transition decision:** ARCHITECTURE present → TransitionType: documentation

**Output:**
```
TransitionType: documentation
SessionState: active
DomainId: "software-development"
GapIds: ["gap-vibe-coding-style-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---
