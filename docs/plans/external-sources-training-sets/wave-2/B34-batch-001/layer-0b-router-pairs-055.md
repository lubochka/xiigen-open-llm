# Layer 0-B — Router Pairs (055)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-055-001 — "present" in meeting context routing = GapHandler

A: "Present the sprint retrospective findings to the engineering chapter."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: present = guidance vocabulary → GapHandler. Meeting/retrospective context does not change routing.

---

## B34-router-055-002 — "discuss" in meeting context routing = GapHandler

A: "Discuss the backend migration strategy in the architecture review meeting."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: discuss = collaborative guidance → GapHandler. Architecture topic in discussion frame does not route to ArchitectureFlow31.

---

## B34-router-055-003 — "brief" + "present" + "discuss" all in one request = GapHandler once

A: "Brief the leadership team, present the roadmap, and discuss the trade-offs."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: brief + present + discuss = all guidance vocabulary → GapHandler once. SCHEDULE_CAPABILITY_LEARNING emitted once.

---

## B34-router-055-004 — guidance vocabulary in "we need to" framing routing = GapHandler

A: "We need to brief and familiarize the new team members with our engineering practices."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: brief + familiarize = guidance vocabulary; "new team members" context → onboard-001. Framing ("we need to") does not change routing.

---

## B34-router-055-005 — "I want to consult" framing routing = GapHandler

A: "I want to consult with the API (Application Programming Interface) governance team on the new versioning scheme."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "I want to consult with" = advisory vocabulary → GapHandler. Intent framing ("I want to") does not change routing.
