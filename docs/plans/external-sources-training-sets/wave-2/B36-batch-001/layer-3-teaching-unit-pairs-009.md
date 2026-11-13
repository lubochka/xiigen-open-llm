# Layer 3 — Teaching Unit Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B36-L3-009-001 — "update" / "generate" / "capture" / "run" / "plan": when they do NOT fire

**Teaching objective:** Model learns the complete list of commonly confused neutral tokens in B36 planning sentences.

**The neutral token table:**

| Token | Fires? | Rule | Common mistake |
|-------|--------|------|----------------|
| update | Only with code-level co-occurrence | Rule 12 | Treating "update the roadmap" as IMPLEMENT |
| generate | Never | Rule 13 | Treating "generate report" as IMPLEMENT |
| capture | Never | Rule 14 | Treating "capture requirements" as DOCUMENT |
| run | Never (ceremony context) | Rule 18 | Treating "run the standup" as IMPLEMENT |
| plan | Never | Rule 17 | Treating "plan the sprint" as IMPLEMENT or DOCUMENT |
| facilitate | Never | Rule 1 | Treating "facilitate the retrospective" as QA |
| track | Never | Rule 1 | Treating "track the velocity" as IMPLEMENT |
| schedule | Never | Rule 1 | Treating "schedule the retrospective" as QA |

---

## B36-L3-009-002 — "update" special case

"update" fires IMPLEMENT ONLY when code-level entity is co-present:
- code-level entities: function, class, module, logic, code, service, method, interface

**"update" + code-level entity → IMPLEMENT:**
- "update the authentication function" → IMPLEMENT
- "update the service logic" → IMPLEMENT

**"update" without code-level entity → NO predicate:**
- "update the roadmap" → []
- "update the backlog" → []
- "update the sprint goals" → []

---

## B36-L3-009-003 — Teaching pairs for neutral tokens

**Pair A — "run" does not fire:**

```
Input: "Run the daily standup."
Expected: predicates: [], KNOWLEDGE_EDGE
Wrong: predicates: ["IMPLEMENT"] (treating "run" as code execution)
```

**Pair B — "generate" does not fire:**

```
Input: "Generate a velocity report."
Expected: predicates: [], gap: velocity-001
Wrong: predicates: ["IMPLEMENT"] (treating "generate" like "create")
```

**Pair C — "capture" does not fire DOCUMENT:**

```
Input: "Capture the sprint retrospective notes."
Expected: predicates: [], gap: sprint-001, retrospective-001
Wrong: predicates: ["DOCUMENT"] (treating "capture" like "document")
```

---

## B36-L3-009-004 — Why "plan" does not fire any predicate

"plan" is one of the most intuitive false expectations. Users say "plan the sprint" and the model might expect "plan" to trigger IMPLEMENT or DOCUMENT. It does not.

"plan" is not in any lexicon. No predicate fires on "plan." This applies even in:
- "plan the sprint" → []
- "plan the milestone" → []
- "plan and implement the sprint feature" → ["IMPLEMENT"] (implement fires, plan does not add)
- "plan and document the sprint goals" → ["DOCUMENT"] (document fires, plan does not add)

"plan" is entirely neutral to the predicate system.
