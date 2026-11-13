# Layer 0-B — Router Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-010-001 — Gap route: "sponsor" → gap

A: "Sponsor junior developers to attend conferences and grow their skills."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001

---

## B34-router-010-002 — IMPLEMENT+DOCUMENT+QA route

A: "Build the certification tracking module, document the certification criteria, and QA the sign-off workflow."
Router decision: Predicates = ["IMPLEMENT", "DOCUMENT", "QA"] → ImplementationFlow41 + DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition. QA always last.

---

## B34-router-010-003 — Gap route: "review PR" mentoring → gap

A: "Review pull requests from junior developers to help them improve."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "review PRs" in mentoring context = guidance vocabulary → gap.

---

## B34-router-010-004 — DOCUMENT route: "create playbook"

A: "Create a deployment playbook for new DevOps engineers."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "create a playbook" → DOCUMENT (create + operational documentation artifact).

---

## B34-router-010-005 — Gap route: "instruct" → gap

A: "Instruct the team on how to use our infrastructure tooling."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "instruct" = instructional vocabulary → gap. "infrastructure tooling" without build verb → gap.
