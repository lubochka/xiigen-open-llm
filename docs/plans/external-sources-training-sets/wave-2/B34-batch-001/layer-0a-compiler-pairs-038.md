# Layer 0-A — Compiler Pairs (038)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-038-001 — "demonstrate" = always gap

A: "Demonstrate the CI/CD (Continuous Integration/Continuous Deployment) pipeline workflow to the new developers."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: demonstrate = teaching vocabulary → gap always. CI/CD pipeline is the learning topic, not a predicate trigger.

---

## B34-compiler-038-002 — "demonstrate and implement" = IMPLEMENT fires

A: "Demonstrate the deployment pattern to the team, then implement it in the staging environment."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-train-001
Rule: "demonstrate to team" = gap warning; "implement in staging" = requester's direct IMPLEMENT action → IMPLEMENT fires.

---

## B34-compiler-038-003 — "demonstrate how to" = gap (parallel to "show how to")

A: "Demonstrate how to configure the Kubernetes (K8s) ingress controller."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "demonstrate how to" = teaching frame + learning target infinitive → gap. Topic is learning content.

---

## B34-compiler-038-004 — "demonstrate how to integrate" = gap

A: "Demonstrate how to integrate the payment gateway with the checkout service."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Rule: "demonstrate how to integrate" = learning target infinitive → gap. Integrate exception does NOT apply here.

---

## B34-compiler-038-005 — "demonstrate" + security action embedded

A: "Demonstrate the secure coding guidelines to the team and harden the authentication module."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-train-001
Rule: "demonstrate guidelines to team" = gap warning; "harden the authentication module" = requester's security action → SECURITY fires.
