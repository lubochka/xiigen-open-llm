# Layer 0-B — Router Pairs (054)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: router

---

## B34-router-054-001 — nested guidance frame routing = single GapHandler

A: "Coach the tech leads on how to mentor the junior engineers."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: coach [nested] mentor = guidance vocabulary nesting → GapHandler. Only one gap. No predicate in "mentor junior engineers" (third-party action).

---

## B34-router-054-002 — "walk through onboarding" routing = GapHandler

A: "Walk the new employees through the onboarding checklist."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-onboard-001
Evidence: walk-through = explanation vocabulary; onboarding context → gap-skills-guidance-onboard-001 selected (most specific).

---

## B34-router-054-003 — "shadow and review" stacked guidance routing

A: "Shadow the senior engineer and review the codebase together."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: shadow + review with = both guidance vocabulary → GapHandler. "Review codebase together" = collaborative review (guidance), not code action by requester alone.

---

## B34-router-054-004 — conditional guidance frame routing

A: "If the team has questions, be available to explain the authentication flow."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: conditional form of "explain" = still guidance vocabulary → GapHandler. Conditional framing does not change routing.

---

## B34-router-054-005 — imperative "go consult" routing

A: "Go consult with the security team on the encryption key management approach."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: imperative "go consult" = advisory vocabulary → GapHandler. Encryption/key management topic does not fire SECURITY.
