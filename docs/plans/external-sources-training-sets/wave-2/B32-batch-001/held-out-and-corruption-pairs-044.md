# Held-Out and Corruption Pairs (044)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-044-001 — HELD-OUT: "quick implementation — build and test" → IMPLEMENT + TEST

**Request:** "Quick implementation — build the handler and write tests."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-044-002 — HELD-OUT: all six gap IDs, no predicates → SCL once

**Request:** "Lean spike prototype hack-together rough-cut dashboard concept."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001, gap-vibe-coding-rough-cut-001, gap-vibe-coding-dashboard-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
TransitionType: unknown
```

---

## B32-HO-044-003 — CORRUPTION: unknown vocabulary → VIBE_CODING predicate (WRONG)

**Corrupted output:**
```
Request: "Just wing it — implement the feature."
Predicates: ["IMPLEMENT", "VIBE_CODING"]
Reason: "wing it" is vibe-coding behavior
```

**Why this is wrong:**
- VIBE_CODING is not a registered predicate (v6-01 corruption)
- "wing it" → gap-vibe-coding-style-001 (gap warning only; never a predicate)

**Correct predicates:** ["IMPLEMENT"]

---

## B32-HO-044-004 — CORRUPTION: all nine v6 corruption types combined scenario

**Scenario demonstrating all corruption risks:**

Request: "Lean pragmatic sprint — implement handler, skip tests."

**All-correct output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
SessionState: active
ContributionFlag: true
```

**Common corruption failures for this request:**
- v6-01: Adding VIBE_CODING predicate
- v6-02: Suppressing IMPLEMENT because of "lean pragmatic" framing
- v6-03: DomainId = "project-management" (because "sprint")
- v6-04: Emitting SCL despite predicates being non-empty
- v6-05: SCL emitted twice (once per qualifier)
- v6-06: TEST fires from "skip tests"
- v6-07: wrong gap ID (hack-001 for lean/pragmatic)
- v6-08: (not triggered here; no dashboard)
- v6-09: DomainId omitted

---
