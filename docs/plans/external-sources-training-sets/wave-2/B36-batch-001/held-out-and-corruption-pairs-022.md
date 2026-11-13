# Held-Out and Corruption Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-022-001 — "prioritize" + "build the feature" (IMPLEMENT fires)

```
held_out_reason: "Tests IMPLEMENT fires when 'build' co-occurs with code entity in prioritize context"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "Prioritize the backlog and build the feature toggle service for the upcoming sprint."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  Teaching: "build" + "service" fire IMPLEMENT. "prioritize", "backlog", "sprint" are gap.
    IMPLEMENT fires unconditionally once code entity co-occurrence confirmed.

---

### Held-Out B36B1-HO-022-002 — "triage" + "review the code" (QA fires)

```
held_out_reason: "Tests QA fires in triage context via 'review the code'"
expected_outcome: QA fires
```

A:
  user_request: "Triage the open bugs and review the code for the payment service."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-triage-001"
  Teaching: "review" fires QA. "triage" is gap. QA fires from "review" unconditionally.

---

### Corruption B36B1-COR-022-001 — v6-02: IMPLEMENT suppressed in "prioritize" context

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Prioritize epics and implement the core authentication module this sprint."
  (Expected: IMPLEMENT fires on "implement" + "module")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed IMPLEMENT because 'the sentence is primarily about prioritization'"

Why this is corrupt:
  "'implement' + 'module' fires IMPLEMENT unconditionally regardless of surrounding planning
   vocabulary. The prioritization framing does not suppress IMPLEMENT. This is v6-02."

Correct B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-022-002 — v6-03: DomainId project-management when QA fired in triage context

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Triage the incident and review the error logs from the production service."
  CompiledPredicateIds: ["QA"]

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model set project-management because triage and incident feel like operations management"

Why this is corrupt:
  "QA fired. Any predicate → DomainId: 'software-development'. The triage and incident
   vocabulary does not override the predicate-driven DomainId. This is v6-03."

Correct B:
  DomainId: "software-development"
