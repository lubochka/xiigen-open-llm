# Layer 0-C — Knowledge Edge Pairs (059)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-059-001 — "show the team how to" vs "show the team [that / what]" edge

"Show the team how to write idiomatic error handling" → gap (how to + infinitive = learning target)
"Show the team what idiomatic error handling looks like" → gap (what = display/demonstrate = guidance → gap)
"Show the team that the tests are passing" → neutral (show that = notification act, not guidance)
"Show the team the code review process" → gap (show [person] [process noun] = guidance when process is learning content)
Rule: "show how to" = gap; "show what [X] looks like" = gap; "show that [fact]" = neutral; "show [person] [process]" = gap.

---

## KE-059-002 — "describe" as adjacent guidance vocabulary

"Describe the CI/CD (Continuous Integration/Continuous Deployment) pipeline to the new team" → gap (describe to person = explanation → gap-skills-guidance-train-001)
"Describe how to implement the CI/CD pipeline" → gap (describe how to = learning frame)
"Describe and implement the CI/CD pipeline" → IMPLEMENT fires; describe = gap warning
Rule: "describe [topic] to [person]" = guidance vocabulary → gap. Parallel to explain.

---

## KE-059-003 — "illustrate" as adjacent guidance vocabulary

"Illustrate the hexagonal architecture to the engineering team" → gap (illustrate = explanation vocabulary → gap-skills-guidance-train-001)
"Illustrate how to implement the hexagonal pattern" → gap (illustrate how to = learning frame)
"Illustrate and implement the pattern" → IMPLEMENT fires; illustrate = gap warning
Rule: "illustrate [topic] to [person]" = guidance vocabulary → gap. Parallel to demonstrate.

---

## KE-059-004 — "show [person] the ropes" idiom

"Show the new engineer the ropes" → gap-skills-guidance-onboard-001 (idiomatic onboarding expression → guidance vocabulary)
"Show [person] the ropes and build the service together" → IMPLEMENT fires; show the ropes = gap warning
Rule: idiomatic guidance expressions (show the ropes, walk through the process) = guidance vocabulary → gap.

---

## KE-059-005 — "hand over" / "hand off" as guidance vocabulary

"Hand over the codebase to the new team and walk them through it" → gap (hand over + walk through = guidance vocabulary)
"Hand off the deployment process to the ops team" → gap (hand off = knowledge transfer = guidance → mentor-001)
"Hand off and document the process" → DOCUMENT fires; hand off = gap warning
"Hand over the service and write the migration guide" → DOCUMENT fires; hand over = gap warning
Rule: "hand over/off" as knowledge transfer act = guidance vocabulary → gap when standalone.
