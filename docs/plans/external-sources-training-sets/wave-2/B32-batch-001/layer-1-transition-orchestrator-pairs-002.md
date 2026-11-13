# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-002-001 — knowledge_edge: TransitionType unknown; DomainId: software-development

**Predicates:** `[]`

**Gap context:** gap-vibe-coding-spike-001 (source: "Run a spike to explore the best approach.")

**Transition orchestrator decision:**

Step 1 — predicates empty → no transition to orchestrate
Step 2 — TransitionType: unknown (cannot determine without predicates)
Step 3 — DomainId: "software-development"
Step 4 — emit SCHEDULE_CAPABILITY_LEARNING
Step 5 — session state: awaiting_capability_expansion

**Output:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapIds: ["gap-vibe-coding-spike-001", "gap-vibe-coding-exploratory-001"]
ContributionFlag: false
```

---

## B32-L1-002-002 — knowledge_edge: TransitionType always unknown when predicates empty

**Rule:** TransitionType is derived from the predicate set. When predicates = [], there is no basis for determining TransitionType. The value is always "unknown" for knowledge_edge outcomes.

| Predicates | TransitionType |
|---|---|
| [] | unknown |
| [IMPLEMENT] | feature_addition |
| [UI] | feature_addition |
| [ARCHITECTURE] | documentation |
| [QA] | no contribution |

---

## B32-L1-002-003 — knowledge_edge session state: what happens next

**When the orchestrator sets SessionState: knowledge_edge:**

1. The current request is logged with gap metadata
2. SCHEDULE_CAPABILITY_LEARNING event is dispatched to the learning pipeline
3. The session waits (no flow is active)
4. The user may refine their request with action vocabulary
5. If user refines: new request processed fresh from compiler stage
6. If user does not refine: session ends in knowledge_edge state

---

## B32-L1-002-004 — DomainId "software-development" in orchestrator

**The orchestrator uses DomainId to:**
1. Route SCHEDULE_CAPABILITY_LEARNING to the correct learning pipeline
2. Tag the knowledge_edge event for analytics
3. Determine which capability expansion dataset to update

DomainId "software-development" must be set at this layer even if the compiler and router already set it. Each layer is responsible for maintaining the correct DomainId in its output.

---
