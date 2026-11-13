# Layer 0-B — Router Pairs (059)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-059-001 — "explain and integrate" routing = API_CONTRACT fires

A: "Explain the webhook patterns to the team and integrate the partner webhook endpoint."
Router decision: Predicates = ["API_CONTRACT"] → APIContractFlow33 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "explain to team" = gap warning; "integrate the partner webhook endpoint" = requester's direct integrate → API_CONTRACT fires.

---

## B34-router-059-002 — "show how to" + "then implement" routing = IMPLEMENT fires

A: "Show the team how to implement feature flags and then implement the dark launch mechanism."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "show how to implement" = gap warning (infinitive); "implement dark launch mechanism" = requester's direct IMPLEMENT → IMPLEMENT fires.

---

## B34-router-059-003 — "demonstrate" + "then architect" routing = ARCHITECTURE fires

A: "Demonstrate the event storming technique to the team, then architect the bounded contexts."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "demonstrate to team" = gap warning; "architect bounded contexts" = requester's ARCHITECTURE action → ArchitectureFlow31.

---

## B34-router-059-004 — advisory + QA routing

A: "Seek advice on the QA (Quality Assurance) process and then QA the release build."
Router decision: Predicates = ["QA"] → QAGatingFlow73 | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "seek advice on QA process" = gap warning; "QA the release build" = QA abbreviation direct action → QA fires → QAGatingFlow73.

---

## B34-router-059-005 — guidance vocabulary at request end routing = same result

A: "Architect the new service topology and present it to the engineering chapter."
Router decision: Predicates = ["ARCHITECTURE"] → ArchitectureFlow31 | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: "architect new topology" = ARCHITECTURE fires first; "present to chapter" (at end) = gap warning. Position does not affect routing.
