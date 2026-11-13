# Layer 0-B — Router Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-003-001 — [IMPLEMENT] → 4.1 routing (quick and dirty build)

**Predicates received:** `["IMPLEMENT"]`

**Source request:** "Build something quick and dirty for the shopping cart — just make it work."

**Gap warnings present:** gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001

**Router decision:**

Step 1 — predicates non-empty: proceed to flow selection
Step 2 — IMPLEMENT present → route to BackendServiceFlow (node 4.1)
Step 3 — no UI present → UIFlow not selected
Step 4 — gap warnings present → append to routing output; do NOT alter flow selection
Step 5 — DomainId: "software-development"

**Output:**
```
RouteTarget: BackendServiceFlow41
FlowSelected: [BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001]
DomainId: "software-development"
```

**Key rule:** Gap warnings are carried through routing as metadata. They do not affect which flow is selected.

---

## B32-0B-003-002 — [IMPLEMENT] → 4.1: exploratory coding context

**Predicates received:** `["IMPLEMENT"]`

**Source request:** "Exploratory coding session — hack together the notification service module."

**Gap warnings present:** gap-vibe-coding-exploratory-001, gap-vibe-coding-hack-001

**Router output:**
```
RouteTarget: BackendServiceFlow41
FlowSelected: [BackendServiceFlow41]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-exploratory-001, gap-vibe-coding-hack-001]
DomainId: "software-development"
```

---

## B32-0B-003-003 — [IMPLEMENT] → 4.1: prototype + build context

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

---

## B32-0B-003-004 — IMPLEMENT routing: same node 4.1 regardless of vibe framing

**Rule:** BackendServiceFlow41 (node 4.1) is selected for IMPLEMENT predicates in all B32 contexts. The style framing (vibe, rough, throwaway, etc.) does not change the routing destination. A vibe-framed IMPLEMENT request routes to exactly the same node as a clean IMPLEMENT request.

**Comparison:**

| Request | Predicates | RouteTarget |
|---|---|---|
| "Build a service" | [IMPLEMENT] | BackendServiceFlow41 |
| "Quick and dirty — build a service" | [IMPLEMENT] | BackendServiceFlow41 |
| "Throwaway — build a service" | [IMPLEMENT] | BackendServiceFlow41 |
| "Vibe code — build a service" | [IMPLEMENT] | BackendServiceFlow41 |

All four route identically. Only the GapWarnings field differs.

---
