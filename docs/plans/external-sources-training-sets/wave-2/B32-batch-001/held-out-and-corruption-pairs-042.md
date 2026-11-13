# Held-Out and Corruption Pairs (042)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-042-001 — HELD-OUT: "expedient spike — implement the dashboard" → IMPLEMENT + three gap IDs

**Request:** "Expedient spike — implement the dashboard."

**Expected output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
RouteTarget: [ImplementationFlow]
DomainId: "software-development"
CapabilityGapWarning: style_qualifier_noted
TransitionType: feature_addition
```

---

## B32-HO-042-002 — HELD-OUT: "lean spike — just be a spike" (no implement) → three-gap knowledge_edge

**Request:** "Lean spike on the dashboard concept."

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-spike-001, gap-vibe-coding-dashboard-001]
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
DomainId: "software-development"
TransitionType: unknown
```

---

## B32-HO-042-003 — CORRUPTION: DomainId = "project-management" (WRONG)

**Corrupted output:**
```
Request: "Lean sprint — implement the feature."
DomainId: "project-management"
Reason: "sprint" is a project management term
```

**Why this is wrong:**
- DomainId = "software-development" for all B32 outputs (v6-03 corruption)
- "sprint" is a planning term but B32 DomainId is fixed

**Correct DomainId:** "software-development"

---

## B32-HO-042-004 — CORRUPTION: capabilityGapWarning varies by gap ID type (WRONG)

**Corrupted output:**
```
Request: "Hack together a module."
capabilityGapWarning: "hack_approach_noted"
Reason: hack vocabulary gets a different warning label
```

**Why this is wrong:**
- capabilityGapWarning = "style_qualifier_noted" is constant for all B32 gap scenarios
- The warning label does not vary by gap ID type

**Correct capabilityGapWarning:** "style_qualifier_noted"

---
