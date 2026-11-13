# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-1-transition-orchestrator-pairs

---

## B32-L1-003-001 — IMPLEMENT in vibe context: feature_addition TransitionType

**Predicates:** `["IMPLEMENT"]`

**Source request:** "Build something quick and dirty for the shopping cart — just make it work."

**Gap warnings:** gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001

**Transition orchestrator decision:**

Step 1 — IMPLEMENT present → TransitionType: feature_addition
Step 2 — single flow: BackendServiceFlow41
Step 3 — no ordering constraints (single flow)
Step 4 — DomainId: "software-development"
Step 5 — gap metadata attached to transition record

**Output:**
```
TransitionType: feature_addition
FlowOrder: [BackendServiceFlow41]
DomainId: "software-development"
GapMetadata: [gap-vibe-coding-quick-dirty-001, gap-vibe-coding-just-works-001]
ContributionFlag: true
SessionState: active
```

---

## B32-L1-003-002 — feature_addition: all IMPLEMENT cases in B32

**Rule:** TransitionType = feature_addition whenever IMPLEMENT is in the predicate set, regardless of:
- Style framing (vibe, quick, dirty, rough, throwaway)
- Co-present predicates
- Gap warnings present

| Request | IMPLEMENT? | TransitionType |
|---|---|---|
| "Build a service" | yes | feature_addition |
| "Quick and dirty — build a service" | yes | feature_addition |
| "Throwaway — implement a handler" | yes | feature_addition |
| "Vibe code a module" | yes (via "code" root) | feature_addition |
| "Just make it work — build it" | yes | feature_addition |

---

## B32-L1-003-003 — ContributionFlag: true when IMPLEMENT fires

**When TransitionType = feature_addition:**

ContributionFlag: true
Meaning: This session contributes new code/features to the codebase
The contribution is tracked in session history regardless of the vibe framing

Even "throwaway" or "prototype" IMPLEMENT requests set ContributionFlag: true. The throwaway nature is captured in gap metadata but the session still contributes code.

---

## B32-L1-003-004 — DomainId "software-development" in feature_addition

**For all B32 IMPLEMENT requests:**

```
{
  transitionType: "feature_addition",
  domainId: "software-development",
  gapMetadata: [...],
  contributionFlag: true
}
```

DomainId "software-development" is consistent regardless of the vibe framing. The contribution is a software development contribution.

---
