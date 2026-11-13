# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-031-001 — Style qualifier taxonomy: Round 2 additions

**Updated B32 style qualifier taxonomy with Round 2 terms:**

### Category 1: Quality-bar framing (low-polish intent)
Gap ID: gap-vibe-coding-style-001
Original: scrappy, rough, low-fi, lo-fi, good enough, quick win, fast and loose, scaffold (alone), creative mode
**Round 2 additions:** good-enough (hyphenated), good enough for now, minimum viable, MVP, bare minimum, just enough

### Category 3: Artifact-type without action (noun-only)
Gap ID: gap-vibe-coding-prototype-001
Original: prototype (alone)
**Round 2 additions:** proof-of-concept, PoC, paper prototype, wireframe, mockup

### Category 4: Technical investigation framing
Gap ID: gap-vibe-coding-spike-001
Original: spike, technical spike, run a spike
**Round 2 additions:** tracer bullet, tracer-bullet, walking skeleton, thin slice, thin-slice, vertical slice

### Category 8: Low-fidelity cut framing
Gap ID: gap-vibe-coding-rough-cut-001
Original: rough cut, rough-cut
**Round 2 addition:** rough draft

---

## B32-0C-031-002 — Categories with NO Round 2 additions

**Categories unchanged in Round 2:**

- Category 2 (exploratory): gap-vibe-coding-exploratory-001
- Category 5 (casual-assembly): gap-vibe-coding-hack-001
- Category 6 (demonstration): gap-vibe-coding-demo-001
- Category 7 (non-UI display): gap-vibe-coding-dashboard-001
- Category 9 (vibe/intuition): gap-vibe-coding-intuition-001
- Category 10 (disposability): gap-vibe-coding-throwaway-001
- Category 11 (compound quality-bar): gap-vibe-coding-quick-dirty-001
- Category 12 (minimal-correctness): gap-vibe-coding-just-works-001
- Category 13 (low-ambition): gap-vibe-coding-nothing-fancy-001

Total categories in B32 taxonomy: 13 (unchanged).

---

## B32-0C-031-003 — "proof-of-concept" + "spike" co-occurrence

**Edge case: two different gap families in one request**

"PoC spike for the auth integration":
- "PoC" → gap-vibe-coding-prototype-001
- "spike" → gap-vibe-coding-spike-001
- No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-prototype-001, gap-vibe-coding-spike-001]
SCHEDULE_CAPABILITY_LEARNING: emitted once (Rule 6: once regardless of gap count)
DomainId: "software-development"

---

## B32-0C-031-004 — "thin slice" + "paper prototype" co-occurrence

**Edge case: spike-family + prototype-family together**

"Thin slice paper prototype for the checkout screen":
- "thin slice" → gap-vibe-coding-spike-001
- "paper prototype" → gap-vibe-coding-prototype-001
- "screen" → UI fires ✓

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001]
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (UI fires)

---

## B32-0C-031-005 — "minimum viable" + "walking skeleton": same-family co-occurrence

**Edge case: style-001 + spike-001 together**

"Minimum viable walking skeleton of the payment service":
- "minimum viable" → gap-vibe-coding-style-001
- "walking skeleton" → gap-vibe-coding-spike-001
- No action verb

Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001]
SCHEDULE_CAPABILITY_LEARNING: emitted once
DomainId: "software-development"

---
