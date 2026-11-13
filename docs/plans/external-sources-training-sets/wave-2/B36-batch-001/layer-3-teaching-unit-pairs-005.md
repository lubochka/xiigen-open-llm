# Layer 3 — Teaching Unit Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-005-001 — DomainId inference: "project-management" (zero predicates) vs "software-development" (any predicate)

**Teaching objective:** Model learns the binary DomainId rule.

**Rule statement:**
- predicates: [] → DomainId: "project-management"
- predicates: [anything] → DomainId: "software-development"

No other factors influence DomainId. Not the balance of gap vocabulary. Not the subject matter. Not the user's role. Only the predicate list.

---

## B36-L3-005-002 — Teaching pairs for DomainId

**Pair A — zero predicates:**

```
Input: "Plan the sprint roadmap."
Predicates: []
Expected DomainId: "project-management"
```

**Pair B — one predicate:**

```
Input: "Review the sprint roadmap."
Predicates: ["QA"] (review fires)
Expected DomainId: "software-development"
```

**Pair C — many gap terms, one predicate:**

```
Input: "Build the sprint backlog tracker with velocity and burndown features."
Predicates: ["IMPLEMENT"] (build, features fire)
Gap warnings: sprint-001, backlog-001, velocity-001, burndown-001
Expected DomainId: "software-development"
```

---

## B36-L3-005-003 — Anti-pattern: DomainId "project-management" when predicate fired

**Wrong:**

```
Input: "Review the sprint backlog."
Predicates: ["QA"]
Wrong output: DomainId: "project-management"
  Reasoning (wrong): "This is clearly a project management sentence."
```

**Correct:**

```
Predicates: ["QA"]
Correct output: DomainId: "software-development"
  Reasoning (correct): Any predicate fires → software-development. Period.
```

This is v6-03 corruption. The sentence content is irrelevant to DomainId. The predicate list is the only DomainId determinant.

---

## B36-L3-005-004 — DomainId "project-management" use cases in B36

DomainId "project-management" only appears in B36 for KNOWLEDGE_EDGE requests:

1. "Plan the sprint roadmap" → []
2. "Groom the backlog" → []
3. "Run the standup" → []
4. "Track the burndown" → []
5. "Estimate the story points" → [] (wait: "story" fires DOCUMENT! → wrong example)

**Correction:** "Estimate the effort for the backlog items" → [] (DomainId: "project-management")

The correct test: a request where NO predicate token appears — only gap vocabulary. That is the only condition for DomainId "project-management."
