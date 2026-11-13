# Layer 0-A — Compiler Pairs (050)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-050-001 — "show how to" + "then build" = IMPLEMENT fires

A: "Show the team how to set up the gRPC (gRPC Remote Procedure Call) service framework, then build the notification service."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "show how to set up" = gap warning; "build the notification service" = requester's IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-050-002 — "demonstrate" + "then test" = TEST fires

A: "Demonstrate the mocking approach to the team and then write the test doubles for the payment service."
B: Predicates: ["TEST"] | GapWarning: gap-skills-guidance-train-001
Rule: "demonstrate approach to team" = gap warning; "write the test doubles" = requester's TEST action → TEST fires.

---

## B34-compiler-050-003 — "introduce" + "then integrate" = API_CONTRACT fires

A: "Introduce the new event bus to the team and integrate it with the order processing service."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-onboard-001
Rule: "introduce event bus to team" = gap warning (onboard-001); "integrate it with order processing" = requester's direct integrate action → API_CONTRACT fires.

---

## B34-compiler-050-004 — "discuss" + "then architect" = ARCHITECTURE fires

A: "Discuss the domain boundaries with the team and architect the bounded contexts."
B: Predicates: ["ARCHITECTURE"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "discuss with team" = gap warning; "architect the bounded contexts" = requester's ARCHITECTURE action → ARCHITECTURE fires.

---

## B34-compiler-050-005 — "consult on" + "then document" = DOCUMENT fires

A: "Consult on the best documentation format and write the runbook for the incident response process."
B: Predicates: ["DOCUMENT"] | GapWarning: gap-skills-guidance-mentor-001
Rule: "consult on format" = gap warning; "write the runbook" = requester's DOCUMENT action → DOCUMENT fires.
