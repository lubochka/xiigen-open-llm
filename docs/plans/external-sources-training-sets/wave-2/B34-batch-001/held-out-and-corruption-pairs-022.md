# Held-Out and Corruption Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-022-A — Held-out: "explain" → always gap

A: "Explain the token-based authentication flow and RBAC (Role-Based Access Control) model to the frontend team."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "explain" = always gap. Security/auth topic does not fire SECURITY or AUTHORIZATION. Explain = teaching = gap.

---

## HC-022-B — Held-out: "explain and document" → DOCUMENT (explain is gap warning)

A: "Explain the API contract design and document the endpoint specifications in the developer portal."
B: Predicates: ["DOCUMENT"] | DocumentationFlow | TransitionType: documentation
Reason: "explain" = gap warning; "document the endpoint specifications" → DOCUMENT fires. Explain does not contribute DOCUMENT predicate.

---

## HC-022-C — Corruption v6-02: "explain the architecture" fires ARCHITECTURE

v6 corrupt: "Explain the microservices architecture to the new team" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "explain" = always gap regardless of topic. ARCHITECTURE fires on design/architect verbs, not on explain.

---

## HC-022-D — Corruption v6-02: "explain" fires DOCUMENT

v6 corrupt: "Explain the onboarding process to new engineers" → Predicates: ["DOCUMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "explain" ≠ DOCUMENT. This is a v6-02 corruption pattern documented in Key Teaching Point 12. Explain = teaching → always gap. DOCUMENT requires explicit write/create/produce verbs.
