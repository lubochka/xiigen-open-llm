# Held-Out and Corruption Pairs (005)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-005-001 — HELD-OUT: "vibe code a dashboard" → knowledge_edge (dashboard not in UI)

**Request:** "Vibe code a dashboard for the analytics."

**Analysis:**
- "vibe code" → gap-vibe-coding-style-001 + "code" root → IMPLEMENT fires ✓
- "dashboard" → NOT in UI lexicon → gap-vibe-coding-dashboard-001; UI does not fire

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "software-development"
```

**Note:** This is NOT knowledge_edge — IMPLEMENT fires from "code" root. The request routes to BackendServiceFlow41 despite "dashboard" not being in UI lexicon.

---

## B32-HO-005-002 — CORRUPTION: dashboard fires UI (WRONG)

**Corrupted output:**
```
Predicates: ["UI", "IMPLEMENT"]
Reason: "dashboard" is a UI element → UI fires
```

**Why this is wrong:**
- "dashboard" is NOT in the UI lexicon
- UI does not fire on "dashboard"
- The correct predicates are ["IMPLEMENT"] only (from "code" root)
- gap-vibe-coding-dashboard-001 is emitted as a warning, not as a UI predicate trigger

**Detection:** Any output where "dashboard" causes UI to fire is a corruption.

---

## B32-HO-005-003 — Pure "vibe code a dashboard" routing clarification

**Request variation:** "Vibe code a dashboard."

**Disambiguation:**

The "code" root in "vibe code" fires IMPLEMENT. Therefore this is NOT knowledge_edge.

| Request | Code root? | UI term? | Result |
|---|---|---|---|
| "Vibe code a dashboard" | YES (code) | NO (dashboard) | [IMPLEMENT] |
| "Vibe out a dashboard" | NO | NO | [] → knowledge_edge |
| "Build a dashboard" | YES (build) | NO | [IMPLEMENT] |

"Vibe out a dashboard" (without "code" root) IS knowledge_edge.
"Vibe code a dashboard" (with "code" root) is NOT knowledge_edge.

---

## B32-HO-005-004 — Key teaching: "vibe code" always fires IMPLEMENT; "dashboard" never fires UI

| Component | Result |
|---|---|
| "vibe" | gap-vibe-coding-style-001 |
| "code" (root in "vibe code") | IMPLEMENT fires |
| "dashboard" | gap-vibe-coding-dashboard-001 (NOT UI) |

These three effects are independent.

---
