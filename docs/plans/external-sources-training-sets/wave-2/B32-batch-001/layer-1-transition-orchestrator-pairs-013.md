# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-013-001 — "scrappy" alone: knowledge_edge; TransitionType unknown

**Predicates:** `[]`

**Gap context:** gap-vibe-coding-style-001 ("scrappy")

**Transition orchestrator decision:**

- Predicates empty → no transition to orchestrate
- TransitionType: unknown
- DomainId: "software-development"
- SCHEDULE_CAPABILITY_LEARNING emitted

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

## B32-L1-013-002 — "scrappy build": TransitionType feature_addition

**Predicates:** `["IMPLEMENT"]`

**Gap context:** gap-vibe-coding-style-001

**Transition orchestrator decision:**

- IMPLEMENT → TransitionType: feature_addition
- SessionState: active
- DomainId: "software-development"

**Output:**
```
TransitionType: feature_addition
SessionState: active
DomainId: "software-development"
CapabilityAction: none
GapIds: ["gap-vibe-coding-style-001"]
ContributionFlag: true
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-L1-013-003 — "scrappy design": TransitionType documentation

**Predicates:** `["ARCHITECTURE"]`

**Gap context:** gap-vibe-coding-style-001

**Transition orchestrator decision:**

- ARCHITECTURE → TransitionType: documentation

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

## B32-L1-013-004 — "scrappy build + test": TransitionType feature_addition (IMPLEMENT + TEST)

**Predicates:** `["IMPLEMENT", "TEST"]`

**Gap context:** gap-vibe-coding-style-001

**Transition orchestrator decision:**

- IMPLEMENT + TEST → TransitionType: feature_addition (IMPLEMENT dominates for transition type)

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
