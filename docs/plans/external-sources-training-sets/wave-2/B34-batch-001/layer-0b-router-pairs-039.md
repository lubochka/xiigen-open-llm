# Layer 0-B — Router Pairs (039)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-039-001 — "review with" routing = GapHandler

A: "Review the onboarding documentation with the new hires."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "review [X] with [person]" = mentoring vocabulary → GapHandler. No code action by requester.

---

## B34-router-039-002 — "present" routing = GapHandler

A: "Present the Q2 technical roadmap to the engineering all-hands."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: present = guidance vocabulary → GapHandler invoked. gap-skills-guidance-train-001 selected.

---

## B34-router-039-003 — "discuss" routing = GapHandler

A: "Discuss the observability strategy with the SRE (Site Reliability Engineering) team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: discuss = collaborative guidance → GapHandler. No code action.

---

## B34-router-039-004 — "present" + ARCHITECTURE routing

A: "Present the proposed architecture and architect the final component boundaries."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "present the proposed architecture" = gap warning; "architect the final component boundaries" = requester's ARCHITECTURE action → ARCHITECTURE routes to ArchitectureFlow31.

---

## B34-router-039-005 — "discuss" + API_CONTRACT routing

A: "Discuss the integration patterns with the team and integrate the logging service with the audit trail."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "discuss integration patterns" = gap warning; "integrate logging service with audit trail" = requester's direct integrate action → API_CONTRACT routes to APIContractFlow33.
