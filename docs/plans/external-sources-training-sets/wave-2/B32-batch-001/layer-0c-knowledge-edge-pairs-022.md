# Layer 0-C — Knowledge-Edge Pairs (022)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0c-knowledge-edge-pairs

---

## B32-0C-022-001 — Dashboard vs screen: the UI boundary in full

**Knowledge edge:** The single most important B32 boundary: "dashboard" is NOT in the UI lexicon; "screen" IS.

**Full UI lexicon (complete list):**

screen | page | interface | component | form | layout | button | input | widget | modal

**Words that are NOT in UI lexicon (common confusions):**

dashboard | panel | sidebar | toolbar | navbar | header | footer | chart | graph | report | view | tab

---

## B32-0C-022-002 — Why "dashboard" is not in the UI lexicon

**Reasoning:** The UI predicate was designed to fire on structural UI primitives (screens, pages, components) rather than on business intelligence (BI) or data visualization concepts. "Dashboard" typically refers to a business-level view aggregating multiple data sources. It was excluded because:

1. Dashboard requests often require IMPLEMENT (data logic) and ARCHITECTURE (data aggregation design) without necessarily requiring the UI predicate to separately fire
2. "Dashboard" is ambiguous between a UI artifact and a BI product concept
3. Including "dashboard" in the UI lexicon would over-fire UI on data/analytics requests

**The gap ID gap-vibe-coding-dashboard-001 exists specifically to teach the system this boundary.**

---

## B32-0C-022-003 — "dashboard + screen" co-occurrence: both effects apply

**Request:** "Build a dashboard screen for the analytics."

- "dashboard" → gap-vibe-coding-dashboard-001
- "screen" → UI fires
- "build" → IMPLEMENT fires

**Result:** `["IMPLEMENT", "UI"]` + gap-dashboard-001

**Teaching:** The gap warning for "dashboard" and the UI predicate from "screen" are independent. Both apply. The presence of "screen" does not cause "dashboard" to be absorbed into the UI predicate.

---
