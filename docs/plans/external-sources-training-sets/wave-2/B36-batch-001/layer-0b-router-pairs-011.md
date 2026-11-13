# Layer 0-B — Router Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-011-001 — v6 router corruption: DomainId wrong when predicate fired

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Implement the sprint features and close the epic tickets."
Predicates: ["IMPLEMENT"]
v6-03 error: Router sets DomainId: "project-management" because sprint/epic/tickets
  are the dominant vocabulary.
```

**Why this is wrong:**
DomainId is determined by the predicate firing rule, not by vocabulary proportion. When IMPLEMENT fires, DomainId is "software-development." Period. The proportion of gap vocabulary in the request is irrelevant.

**Correct output:**
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Path: Phase 4.1
- CapabilityGapWarnings: sprint-001, epic-001, ticket-001

---

## B36-0B-011-002 — v6 router corruption: routing to non-existent PROJECT_PLANNING path

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Run the sprint standup and groom the backlog."
Predicates: ["PROJECT_PLANNING"] (invented)
v6-01 error: Router looks for PROJECT_PLANNING flow — crashes with unknown flow error.
```

**Why this is wrong:**
PROJECT_PLANNING is not a registered predicate. The router has no path for it. The correct output is predicates: `[]` → KNOWLEDGE_EDGE. Inventing a predicate at the compiler stage propagates as a router crash.

**Correct output:**
- Predicates: []
- Path: KNOWLEDGE_EDGE
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

---

## B36-0B-011-003 — v6 router corruption: QA suppressed in planning context

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Review the sprint velocity."
Predicates: [] (QA suppressed because "this is a planning sentence")
v6-02 error: Router diverts to KNOWLEDGE_EDGE instead of QA sub-phase.
DomainId: "project-management" (wrong — QA had fired before suppression)
```

**Why this is wrong:**
"review" fires QA — this happens at the compiler, before the router sees the predicate list. The router receives ["QA"] and routes to QA sub-phase. If v6-02 suppression happened at the compiler stage, the router sees [] — but that [] is corrupt because the compiler should have emitted ["QA"].

The correct behavior at the compiler: ["QA"]
The correct behavior at the router: QA sub-phase, DomainId: "software-development"

---

## B36-0B-011-004 — three v6 corruptions mapped to router effects

| v6 Code | Corruption at stage | Router sees | Router effect |
|---------|--------------------|-----------  |--------------|
| v6-01 | Compiler invents PROJECT_PLANNING | ["PROJECT_PLANNING"] | Unknown flow crash |
| v6-02 | Compiler suppresses QA | [] (wrong) | KNOWLEDGE_EDGE (wrong) |
| v6-03 | Router misassigns DomainId | ["IMPLEMENT"] | Wrong DomainId downstream |

Each corruption affects a different stage and has a different downstream effect. Testing for all three requires separate test cases.
