# Layer 0-A — Compiler Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: compiler

---

## B36-0A-011-001 — v6-01 corruption: PROJECT_PLANNING predicate invented

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Run the sprint standup and groom the backlog."
v6-01 error: Compiler emits ["PROJECT_PLANNING"] for sprint/standup/backlog vocabulary.
```

**Why this is wrong:**
PROJECT_PLANNING is not a registered predicate in any lexicon. There are exactly nine registered predicates: IMPLEMENT, DOCUMENT, QA, ARCHITECTURE, TEST, SECURITY, PERFORMANCE, REFACTOR, DEPLOY. PROJECT_PLANNING does not exist. Inventing a tenth predicate corrupts the routing table and causes the factory to fail with an unknown flow selector.

**Correct output:**
- Input: "Run the sprint standup and groom the backlog."
- Predicate output: `[]` (zero predicates)
- KNOWLEDGE_EDGE state: true
- DomainId: "project-management"
- recommended_action: SCHEDULE_CAPABILITY_LEARNING

---

## B36-0A-011-002 — v6-02 corruption: "review" QA suppressed in planning context

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Review the sprint velocity and retrospective outcomes."
v6-02 error: Compiler suppresses QA because the sentence is about project planning.
Output: []
DomainId: "project-management"
```

**Why this is wrong:**
"review" always fires QA. The subject matter being project planning does not suppress the predicate. The compiler is literal — it fires on tokens regardless of semantic context. Suppressing QA because the user seems to be talking about planning violates the lexicon contract.

**Correct output:**
- Predicate output: `["QA"]`
- CapabilityGapWarnings: sprint, velocity, retrospective
- DomainId: "software-development"
- TransitionType: no_contribution

---

## B36-0A-011-003 — v6-03 corruption: DomainId wrongly "project-management" when IMPLEMENT fired

**Corrupt v6 output (DO NOT REPRODUCE):**

```
Input: "Implement the sprint tracking feature and close the backlog tickets."
v6-03 error: DomainId set to "project-management" because majority of vocabulary is planning-related.
Output: DomainId: "project-management", predicates: ["IMPLEMENT"]
```

**Why this is wrong:**
DomainId is "project-management" ONLY when zero predicates fire. When any predicate fires — even one — DomainId must be "software-development" regardless of how much gap vocabulary is present in the request. The gap vocabulary majority does not override the predicate firing rule.

**Correct output:**
- Predicate output: `["IMPLEMENT"]`
- DomainId: "software-development" (IMPLEMENT fired)
- CapabilityGapWarnings: sprint, backlog, ticket

---

## B36-0A-011-004 — all three v6 corruptions in one summary table

| v6 Code | Corruption Description | Effect | Correct Behavior |
|---------|----------------------|--------|-----------------|
| v6-01 | Invents PROJECT_PLANNING predicate | Router crashes — unknown flow | Output `[]`; KNOWLEDGE_EDGE |
| v6-02 | Suppresses QA because context is planning | Wrong routing — misses QA gate | "review" always fires QA |
| v6-03 | Sets DomainId "project-management" when predicate fired | Downstream domain mismatch | Any predicate → "software-development" |

**The three v6 corruptions are distinct failures.** v6-01 is a predicate invention error. v6-02 is a context-override suppression error. v6-03 is a DomainId bleeding error. A model can exhibit any one, two, or all three simultaneously. Each is independently detectable and independently fixable.
