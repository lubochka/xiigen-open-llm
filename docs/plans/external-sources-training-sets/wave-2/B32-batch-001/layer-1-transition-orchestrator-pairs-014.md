# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-014-001 — "rough cut" alone: knowledge_edge; TransitionType unknown

**Predicates:** `[]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Output:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: ["gap-vibe-coding-rough-cut-001"]
ContributionFlag: false
```

---

## B32-L1-014-002 — "rough cut build": TransitionType feature_addition

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Output:**
```
TransitionType: feature_addition
SessionState: active
DomainId: "software-development"
GapIds: ["gap-vibe-coding-rough-cut-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-014-003 — "rough cut design": TransitionType documentation

**Predicates:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Output:**
```
TransitionType: documentation
SessionState: active
DomainId: "software-development"
GapIds: ["gap-vibe-coding-rough-cut-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-014-004 — "rough cut + screen build": TransitionType feature_addition (IMPLEMENT + UI)

**Predicates:** `["IMPLEMENT", "UI"]`

**Gap context:** gap-vibe-coding-rough-cut-001

**Output:**
```
TransitionType: feature_addition
SessionState: active
DomainId: "software-development"
GapIds: ["gap-vibe-coding-rough-cut-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---
