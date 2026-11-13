# Layer 0-A — Compiler Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-011-001 — "lunch and learn" → gap

A: "Organize a lunch and learn session on GraphQL."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "lunch and learn" = informal training event vocabulary → gap.

---

## B34-compiler-011-002 — "create" + README → DOCUMENT

A: "Create a README for the new developer setup process."
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Evidence: "create a README" → DOCUMENT (create + documentation artifact type).

---

## B34-compiler-011-003 — "five-predicate" guidance + code request

A: "Mentor the team, build the skill tracker, integrate with HR API, write tests, and document the assessment criteria."
Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
Evidence: mentor→gap; build→IMPLEMENT; integrate→API_CONTRACT; write tests→TEST; document→DOCUMENT.
TransitionType: feature_addition (IMPLEMENT present). GapHandler not invoked.

---

## B34-compiler-011-004 — "facilitate retrospective" → gap

A: "Facilitate a retrospective for the team to improve their development process."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "facilitate retrospective" = process improvement vocabulary → gap. No code-action verb present.

---

## B34-compiler-011-005 — "train" (human sense) → gap; "train" (ML sense) → gap

A: "Train the junior developers on our testing conventions."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "train" (human learning) = guidance vocabulary → gap. Identical gap for ML training ("train the model") — both senses yield gap.
