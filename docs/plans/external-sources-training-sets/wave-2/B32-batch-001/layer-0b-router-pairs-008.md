# Layer 0-B — Router Pairs (008)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-008-001 — [UI, IMPLEMENT] → UIFlow + BackendServiceFlow41

**Predicates received:** `["UI", "IMPLEMENT"]`

**Source request:** "Vibe code the user profile screen and implement the avatar upload component."

**Gap warnings present:** gap-vibe-coding-style-001

**Router decision:**

Step 1 — predicates non-empty: proceed
Step 2 — UI present → UIFlow selected
Step 3 — IMPLEMENT present → BackendServiceFlow41 selected
Step 4 — both flows selected → parallel-eligible routing
Step 5 — gap warnings carried through; do not affect selection

**Output:**
```
RouteTarget: [UIFlow, BackendServiceFlow41]
FlowSelected: [UIFlow, BackendServiceFlow41]
ParallelEligible: true
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
```

---

## B32-0B-008-002 — UI + IMPLEMENT parallel routing rule

**Rule:** When both UI and IMPLEMENT predicates are present:
- Both UIFlow and BackendServiceFlow41 are selected
- ParallelEligible: true (UI work and backend work can proceed concurrently)
- TransitionType: feature_addition (IMPLEMENT present)
- This applies regardless of vibe framing

---

## B32-0B-008-003 — [UI, IMPLEMENT] routing: "vibe code a page" context

**Predicates received:** `["UI", "IMPLEMENT"]`

**Source request:** "Vibe code a landing page for the product."

**Router output:**
```
RouteTarget: [UIFlow, BackendServiceFlow41]
FlowSelected: [UIFlow, BackendServiceFlow41]
ParallelEligible: true
TransitionType: feature_addition
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
```

---

## B32-0B-008-004 — Routing order: UI before IMPLEMENT in multi-predicate results

**Convention:** When multiple flows are selected, the ordering in the routing output follows predicate priority:
1. ARCHITECTURE (if present)
2. UI
3. IMPLEMENT
4. API_CONTRACT
5. SECURITY
6. TEST
7. DOCUMENT
8. QA

This ordering is consistent across all B32 multi-predicate cases.

---
