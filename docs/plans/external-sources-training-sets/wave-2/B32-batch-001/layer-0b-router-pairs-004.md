# Layer 0-B — Router Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-004-001 — [UI] → UIFlow routing (screen in vibe context)

**Predicates received:** `["UI"]`

**Source request:** "Vibe code a quick screen for the product listing — make it feel intuitive."

**Gap warnings present:** gap-vibe-coding-style-001, gap-vibe-coding-intuition-001

**Router decision:**

Step 1 — predicates non-empty: proceed
Step 2 — UI present → route to UIFlow (node 1.1 or 4.2 depending on system)
Step 3 — no IMPLEMENT separately? Note: "code" root from "vibe code" fires IMPLEMENT
  → Predicates are actually ["UI", "IMPLEMENT"] per file 004 analysis
  → This pair illustrates UI-only routing when IMPLEMENT is not co-present

**Corrected pair (UI only, no IMPLEMENT):**
**Request:** "Vibe out a screen for the settings panel."

**Output:**
```
RouteTarget: UIFlow
FlowSelected: [UIFlow]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
```

---

## B32-0B-004-002 — [UI] routing: "page" in vibe context

**Predicates received:** `["UI"]`

**Source request:** "Vibe out a settings page — nothing fancy."

**Gap warnings present:** gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001

**Router output:**
```
RouteTarget: UIFlow
FlowSelected: [UIFlow]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001]
DomainId: "software-development"
```

---

## B32-0B-004-003 — [UI] routing: "component" in vibe context

**Predicates received:** `["UI"]`

**Source request:** "Rough cut — a notification component for the sidebar."

**Gap warnings present:** gap-vibe-coding-rough-cut-001

**Router output:**
```
RouteTarget: UIFlow
FlowSelected: [UIFlow]
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-rough-cut-001]
DomainId: "software-development"
```

---

## B32-0B-004-004 — UI routing: same node regardless of vibe framing

**Rule:** UIFlow is selected for UI predicates in all B32 contexts. Style framing ("vibe", "rough cut", "nothing fancy") does not change the routing destination.

| Request | Predicates | RouteTarget |
|---|---|---|
| "Build a settings screen" | [UI] | UIFlow |
| "Vibe code a settings screen" | [UI, IMPLEMENT] | UIFlow + BackendServiceFlow41 |
| "Rough cut settings screen" | [UI] | UIFlow |
| "Nothing fancy — a settings component" | [UI] | UIFlow |

---
