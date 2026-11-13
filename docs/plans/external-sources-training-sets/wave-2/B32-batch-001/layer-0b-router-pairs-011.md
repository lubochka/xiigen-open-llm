# Layer 0-B — Router Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0b-router-pairs

---

## B32-0B-011-001 — v6 Corruption: DomainId "project-management" for vibe gap routing

**Predicates received:** `[]`

**Gap context:** gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-prototype-001

**CORRUPTED router output (v6-03):**
```
RouteTarget: knowledge_edge
DomainId: "project-management"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

**WHY THIS IS WRONG:**
- B32 vibe coding is a software-development domain. DomainId must be "software-development".
- "project-management" is the DomainId for B36 (project planning vocabulary) and B37 (sprint/agile vocabulary).
- Using "project-management" for vibe coding would send SCHEDULE_CAPABILITY_LEARNING to the wrong learning pipeline.

**CORRECT router output:**
```
RouteTarget: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-dashboard-001, gap-vibe-coding-prototype-001]
```

---

## B32-0B-011-002 — DomainId comparison: B32 vs B36 vs B37

| Scenario | DomainId | Example Gap Vocabulary |
|---|---|---|
| B32 — Vibe coding | software-development | spike, vibe, prototype, hack, throwaway |
| B36 — Project planning | project-management | milestone, roadmap, backlog, sprint planning |
| B37 — Agile/sprint | project-management | velocity, burndown, retrospective, standup |

These are distinct domains. A vibe coding request (B32) must never receive DomainId "project-management" and a project planning request (B36) must never receive DomainId "software-development".

---

## B32-0B-011-003 — v6 corruption in router: wrong flow for vibe predicates

**CORRUPTED router output (v6-01 variant):**
```
Predicates: ["VIBE_CODING"]
RouteTarget: VibeCodingFlow
```

**WHY THIS IS WRONG:**
- No VIBE_CODING predicate exists in the system
- No VibeCodingFlow exists in the flow registry
- The router must work with registered predicates and registered flows only
- If the compiler emits an unregistered predicate, the router must reject it or log an error — it must not create a new flow

---

## B32-0B-011-004 — Router validation: unregistered predicates

**Router behavior when it receives an unregistered predicate:**

If compiler emits `["VIBE_CODING"]`:
```
RouterAction: log_error("unregistered predicate: VIBE_CODING")
RouteTarget: knowledge_edge (fallback for unroutable state)
ErrorCode: PREDICATE_NOT_REGISTERED
```

The router never invents flows. knowledge_edge is the safe fallback for unregistered predicates.

---
