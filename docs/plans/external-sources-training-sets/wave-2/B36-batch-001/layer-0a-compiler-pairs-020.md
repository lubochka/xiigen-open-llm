# Layer 0-A — Compiler Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-020-001 — Sprint planning + IMPLEMENT: "build the sprint features"

A:
  run_id: "B36-0A-020-001"
  user_request: "Plan the sprint and build the authentication features scheduled for this iteration."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" — fires IMPLEMENT
  "features" — fires IMPLEMENT
  "sprint" — not in any predicate lexicon (gap)
  "plan" — not in any predicate lexicon
  "iteration" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT fires; sprint and iteration are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-sprint-001", "gap-project-planning-iteration-001"]

---

## B36-0A-020-002 — Sprint planning + IMPLEMENT: "implement the roadmap items"

A:
  run_id: "B36-0A-020-002"
  user_request: "Implement the first quarter roadmap items: build the API service and create the database module."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" — fires IMPLEMENT
  "build" — fires IMPLEMENT
  "API service" — "API" is context; "service" co-occurs with "build" reinforcing IMPLEMENT
  "create" — fires IMPLEMENT (co-occurring with "module")
  "module" — IMPLEMENT co-occurrence
  "roadmap" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates on multiple tokens; roadmap is gap warning
  CapabilityGapWarnings: ["gap-project-planning-roadmap-001"]

---

## B36-0A-020-003 — Backlog + IMPLEMENT: "write the code for backlog items"

A:
  run_id: "B36-0A-020-003"
  user_request: "Write the code for the top five backlog items and deploy the updated service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "write" — fires IMPLEMENT (write + code → IMPLEMENT; "write the code")
  "code" — IMPLEMENT co-occurrence; reinforces "write" → IMPLEMENT
  "deploy" — may fire IMPLEMENT in deployment context
  "service" — IMPLEMENT co-occurrence
  "backlog" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT fires; backlog is gap warning
  CapabilityGapWarnings: ["gap-project-planning-backlog-001"]

---

## B36-0A-020-004 — Epic + IMPLEMENT: "implement the epic features"

A:
  run_id: "B36-0A-020-004"
  user_request: "In this sprint, implement the user authentication epic: build the login module and create the token service."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "implement" — fires IMPLEMENT
  "build" — fires IMPLEMENT
  "create" — fires IMPLEMENT (co-occurring with "service")
  "module" — IMPLEMENT co-occurrence
  "service" — IMPLEMENT co-occurrence
  "epic" — not in any predicate lexicon (gap)
  "sprint" — not in any predicate lexicon (gap)
  Mixed: IMPLEMENT dominates; epic and sprint are gap warnings
  CapabilityGapWarnings: ["gap-project-planning-epic-001", "gap-project-planning-sprint-001"]

---

## B36-0A-020-005 — Sprint + IMPLEMENT + DOCUMENT: "build features, write requirements"

A:
  run_id: "B36-0A-020-005"
  user_request: "For this sprint: build the notification service, implement the email feature, and document the API requirements."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  "build" — fires IMPLEMENT
  "service" — IMPLEMENT co-occurrence
  "implement" — fires IMPLEMENT
  "feature" — fires IMPLEMENT
  "document" — fires DOCUMENT
  "requirements" — fires DOCUMENT
  "sprint" — not in any predicate lexicon (gap)
  Mixed: both IMPLEMENT and DOCUMENT fire; sprint is gap warning
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
