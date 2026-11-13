# Layer 0-B — Router Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-008-001 — Gap route: "present" → gap

A: "Present the new architecture to the engineering team."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "present" = presentation vocabulary; "new architecture" as noun object → gap (no design verb fired).

---

## B34-router-008-002 — DOCUMENT route: "create spec"

A: "Create a technical specification for the new engineer onboarding checklist."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "create a technical specification" → DOCUMENT (create + specification artifact).

---

## B34-router-008-003 — IMPLEMENT+TEST route: build + write tests

A: "Build the quiz module for the training platform and write tests for the quiz logic."
Router decision: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 → TestingFlow51
TransitionType: feature_addition

---

## B34-router-008-004 — Gap route: "train to use" → gap

A: "Train the operations team to use the new deployment pipeline."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-train-001
Evidence: "train to use" = user training vocabulary → gap. "deployment pipeline" ≠ predicate without code-action verb.

---

## B34-router-008-005 — AUTHORIZATION route in guidance context

A: "Add role-based access control to the training portal so only approved users can access content."
Router decision: Predicates = ["AUTHORIZATION"] → AuthorizationFlow
Evidence: "add RBAC (Role-Based Access Control)" → AUTHORIZATION (access control + role vocabulary).
