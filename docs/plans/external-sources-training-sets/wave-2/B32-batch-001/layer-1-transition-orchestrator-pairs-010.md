# Layer 1 — Transition Orchestrator Pairs (010)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-010-001 — Canonical B32 request patterns: orchestrator perspective

**Purpose:** Reference for how the orchestrator processes the most common B32 request patterns.

### Pattern 1: Zero-predicate vibe request

**Request:** "Run a spike on the caching solution."
**Orchestrator:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
Action: SCHEDULE_CAPABILITY_LEARNING
```

### Pattern 2: IMPLEMENT with vibe framing

**Request:** "Build something quick and dirty for the cart."
**Orchestrator:**
```
TransitionType: feature_addition
FlowOrder: [BackendServiceFlow41]
GapMetadata: [quick-dirty, just-works]
DomainId: "software-development"
```

### Pattern 3: UI with vibe framing

**Request:** "Vibe out a settings screen."
**Orchestrator:**
```
TransitionType: feature_addition
FlowOrder: [UIFlow]
GapMetadata: [vibe-style]
DomainId: "software-development"
```

---

## B32-L1-010-002 — Canonical B32 patterns continued

### Pattern 4: dashboard (zero predicates, vibe)

**Request:** "Throw together a dashboard — nothing fancy."
**Orchestrator:**
```
TransitionType: unknown
SessionState: knowledge_edge
DomainId: "software-development"
Action: SCHEDULE_CAPABILITY_LEARNING
GapIds: [hack-001, dashboard-001, nothing-fancy-001]
```

### Pattern 5: prototype + action verb

**Request:** "Build a rough prototype of the checkout flow."
**Orchestrator:**
```
TransitionType: feature_addition
FlowOrder: [BackendServiceFlow41]
GapMetadata: [prototype-001, rough-cut-001]
DomainId: "software-development"
```

### Pattern 6: exploratory coding

**Request:** "Exploratory coding — hack together the notification module."
**Orchestrator:**
```
TransitionType: feature_addition
FlowOrder: [BackendServiceFlow41]
GapMetadata: [exploratory-001, hack-001]
DomainId: "software-development"
```

---

## B32-L1-010-003 — Canonical patterns: ARCHITECTURE

### Pattern 7: rough-cut architecture

**Request:** "Design a rough-cut architecture — nothing fancy."
**Orchestrator:**
```
TransitionType: documentation
FlowOrder: [ArchitectureFlow31]
GapMetadata: [rough-cut-001, nothing-fancy-001]
DomainId: "software-development"
```

### Pattern 8: Full vibe session (all 7 predicates)

**Request:** "Quick vibe session: architect, integrate, build, secure, test, document, QA."
**Orchestrator:**
```
TransitionType: feature_addition
FlowOrder: [Phase1: ARCH+API, Phase2: IMPL, Phase3: SEC+TEST+DOC, Phase4: QA]
GapMetadata: [vibe-style-001]
DomainId: "software-development"
```

---
