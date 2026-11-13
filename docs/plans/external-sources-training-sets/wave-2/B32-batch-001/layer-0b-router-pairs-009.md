# Layer 0-B — Router Pairs (009)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-009-001 — [ARCHITECTURE] → ArchitectureFlow31 (rough-cut architecture)

**Predicates received:** `["ARCHITECTURE"]`

**Source request:** "Design a rough-cut architecture for the feature flag system — nothing fancy."

**Gap warnings present:** gap-vibe-coding-rough-cut-001, gap-vibe-coding-nothing-fancy-001

**Router output:**
```
RouteTarget: ArchitectureFlow31
FlowSelected: [ArchitectureFlow31]
TransitionType: documentation
GapWarnings: [gap-vibe-coding-rough-cut-001, gap-vibe-coding-nothing-fancy-001]
DomainId: "software-development"
```

**Note:** TransitionType: documentation when ARCHITECTURE fires without IMPLEMENT co-occurring.

---

## B32-0B-009-002 — ARCHITECTURE routing: vibe framing carries through as metadata

**Rule:** ArchitectureFlow31 is selected for ARCHITECTURE predicates regardless of vibe framing. "Rough-cut", "nothing fancy", "exploratory design" are all gap warnings only.

| Request | Predicates | RouteTarget |
|---|---|---|
| "Design the system architecture" | [ARCHITECTURE] | ArchitectureFlow31 |
| "Rough-cut architecture design" | [ARCHITECTURE] | ArchitectureFlow31 |
| "Nothing fancy — design the architecture" | [ARCHITECTURE] | ArchitectureFlow31 |
| "Spike the architecture design" | [ARCHITECTURE] | ArchitectureFlow31 |

All route identically to ArchitectureFlow31.

---

## B32-0B-009-003 — [ARCHITECTURE] routing: "architect" trigger

**Predicates received:** `["ARCHITECTURE"]`

**Source request:** "Rough cut — architect the microservices layout."

**Gap warnings present:** gap-vibe-coding-rough-cut-001

**Note on "layout":** UI fires on "layout" → predicates are actually ["ARCHITECTURE", "UI"]

**Corrected output:**
```
RouteTarget: [ArchitectureFlow31, UIFlow]
FlowSelected: [ArchitectureFlow31, UIFlow]
TransitionType: documentation (ARCHITECTURE) + feature_addition (UI)
GapWarnings: [gap-vibe-coding-rough-cut-001]
DomainId: "software-development"
```

---

## B32-0B-009-004 — ARCHITECTURE + IMPLEMENT: documentation + feature_addition

**Predicates received:** `["ARCHITECTURE", "IMPLEMENT"]`

**Source request:** "Design and build the feature flag system."

**Router output:**
```
RouteTarget: [ArchitectureFlow31, BackendServiceFlow41]
FlowSelected: [ArchitectureFlow31, BackendServiceFlow41]
TransitionType: feature_addition (IMPLEMENT present)
GapWarnings: none
DomainId: "software-development"
```

**Rule:** When IMPLEMENT co-occurs with ARCHITECTURE, TransitionType = feature_addition (IMPLEMENT dominates for TransitionType determination).

---
