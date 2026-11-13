# Layer 0-A — Compiler Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-011-001 — v6 Corruption 1: VIBE_CODING predicate invented (WRONG)

**Request:** "Vibe code the user dashboard — nothing fancy."

**CORRUPTED output (v6-01):**
```
Predicates: ["VIBE_CODING"]
RouteTarget: VibeCodingFlow
GapWarnings: none
```

**WHY THIS IS WRONG:**
- "VIBE_CODING" is NOT a registered predicate. There is no VIBE_CODING predicate in the system.
- Inventing a predicate violates the closed-world assumption: only registered predicates can appear in results.
- "vibe code" is gap vocabulary (gap-vibe-coding-style-001), not a predicate.
- "dashboard" is NOT in the UI lexicon → UI does not fire.
- "nothing fancy" → gap-vibe-coding-nothing-fancy-001.
- No action verbs present → zero predicates.

**CORRECT output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-nothing-fancy-001]
OutputDecision: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

---

## B32-0A-011-002 — v6 Corruption 2: IMPLEMENT suppressed by "just make it work" (WRONG)

**Request:** "Just make it work — implement a basic session handler."

**CORRUPTED output (v6-02):**
```
Predicates: []
Reason: "just make it work" signals informal/vibe request; predicates suppressed
GapWarnings: [gap-vibe-coding-just-works-001]
```

**WHY THIS IS WRONG:**
- Rule 2 (B32 Core Rules): Style qualifiers NEVER suppress action predicates.
- "implement" is a direct IMPLEMENT lexicon match → IMPLEMENT fires unconditionally.
- "just make it work" emits a gap warning but has ZERO effect on predicate detection.
- Suppressing IMPLEMENT because of surrounding style framing is the v6-02 corruption pattern.

**CORRECT output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-just-works-001]
OutputDecision: route to IMPLEMENT flow
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-0A-011-003 — v6 Corruption 3: DomainId set to "project-management" (WRONG)

**Request:** "Vibe code a dashboard prototype — just throw it together."

**CORRUPTED output (v6-03):**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
OutputDecision: knowledge_edge
DomainId: "project-management"
```

**WHY THIS IS WRONG:**
- B32 (vibe coding) uses DomainId: "software-development" — NOT "project-management".
- "project-management" is the DomainId for B36 (project planning) and B37 (sprint/agile).
- Vibe coding is a software development activity performed with an unrecognized style vocabulary.
- Setting DomainId to "project-management" misroutes the SCHEDULE_CAPABILITY_LEARNING signal.

**CORRECT output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-prototype-001, gap-vibe-coding-hack-001]
OutputDecision: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

---

## B32-0A-011-004 — All three v6 corruptions: summary comparison table

| Corruption | Wrong Behavior | Correct Behavior |
|---|---|---|
| v6-01 | Invents VIBE_CODING predicate | "vibe" is gap; no predicate invented |
| v6-02 | Suppresses IMPLEMENT due to vibe framing | Style qualifiers never suppress predicates |
| v6-03 | Uses DomainId "project-management" | B32 always uses DomainId "software-development" |

---
