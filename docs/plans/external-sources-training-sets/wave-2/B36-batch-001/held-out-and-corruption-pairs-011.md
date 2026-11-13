# Held-Out and Corruption Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-011-001 — Three v6 corruptions: v6-01 (PROJECT_PLANNING invented)

**Detection input:** "Run the sprint planning and groom the backlog."

**v6-01 corrupt output:**
- predicates: ["PROJECT_PLANNING"]
- Routing: crash / undefined

**Correct output:**
- predicates: []
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted: sprint-001, backlog-001

**Test assertion for v6-01:**
```
assert "PROJECT_PLANNING" NOT IN predicates
assert predicates == []
assert knowledgeEdge == true
```

---

## B36-HO-011-002 — Three v6 corruptions: v6-02 (QA suppressed)

**Detection input:** "Review the sprint planning."

**v6-02 corrupt output:**
- predicates: [] (QA suppressed — "review" treated as planning activity)
- DomainId: "project-management" (wrong)
- KNOWLEDGE_EDGE: true (wrong)

**Correct output:**
- predicates: ["QA"]
- DomainId: "software-development"
- KNOWLEDGE_EDGE: false
- CapabilityGapWarnings: sprint-001

**Test assertion for v6-02:**
```
assert "QA" IN predicates when "review" in request
assert domainId == "software-development"
assert knowledgeEdge == false
```

---

## B36-HO-011-003 — Three v6 corruptions: v6-03 (DomainId wrong when predicate fired)

**Detection input:** "Implement the sprint backlog features."

**v6-03 corrupt output:**
- predicates: ["IMPLEMENT"] (correct predicate)
- DomainId: "project-management" (WRONG)

**Correct output:**
- predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, backlog-001

**Test assertion for v6-03:**
```
assert domainId == "software-development" when predicates != []
assert domainId != "project-management" when predicates != []
```

---

## B36-HO-011-004 — Three v6 corruptions in one test suite

A complete B36 v6 corruption test suite requires three test inputs and three assertions:

| Test | Input | Key assertion |
|------|-------|--------------|
| v6-01 | Planning-only request | predicates NOT contains "PROJECT_PLANNING" |
| v6-02 | "review" in planning sentence | predicates contains "QA" |
| v6-03 | Predicate fires in planning sentence | domainId == "software-development" |

All three should pass independently. A model may pass two of three and fail one — each failure mode is independently detectable.
