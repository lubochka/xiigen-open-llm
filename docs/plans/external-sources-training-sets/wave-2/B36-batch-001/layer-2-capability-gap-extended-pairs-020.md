# Layer 2 — Capability Gap Extended Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-020-001 — Sprint + IMPLEMENT: "build the sprint features"

A: "Plan the sprint and build the authentication features for this iteration."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "build" fires IMPLEMENT. "features" fires IMPLEMENT. "sprint" and "iteration"
    are gap. IMPLEMENT dominates. DomainId: software-development.
    Gap warnings for sprint-001, iteration-001.

---

## B36-L2-020-002 — Roadmap + IMPLEMENT: "implement roadmap items"

A: "Implement the Q1 roadmap items: build the API service and create the database module."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-roadmap-001
  DomainId: software-development
  Analysis: "implement", "build", "create" fire IMPLEMENT. "roadmap" is gap.
    DomainId: software-development. Gap warning for roadmap-001.

---

## B36-L2-020-003 — Epic + IMPLEMENT: "implement the epic"

A: "Implement the user authentication epic: build the login module and token service."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-epic-001
  DomainId: software-development
  Analysis: "implement", "build" fire IMPLEMENT. "module" and "service" reinforce IMPLEMENT.
    "epic" is gap. DomainId: software-development. Gap warning for epic-001.

---

## B36-L2-020-004 — Sprint + IMPLEMENT + DOCUMENT: mixed three signals

A: "This sprint: build the notification service, implement the email feature, document the API requirements."

B:
  Predicates: [IMPLEMENT, DOCUMENT]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "build", "implement", "feature" fire IMPLEMENT. "document", "requirements"
    fire DOCUMENT. "sprint" is gap. Two predicates fire. DomainId: software-development.
    Gap warning for sprint-001.

---

## B36-L2-020-005 — Backlog + IMPLEMENT: "write the code for backlog items"

A: "Write the code for the top five backlog items and deploy the updated service."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "write" + "code" fires IMPLEMENT. "service" reinforces IMPLEMENT.
    "backlog" is gap. DomainId: software-development. Gap warning for backlog-001.
