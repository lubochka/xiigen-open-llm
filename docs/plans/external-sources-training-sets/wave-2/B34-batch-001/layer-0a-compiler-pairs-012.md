# Layer 0-A — Compiler Pairs (012)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-012-001 — "share knowledge" → gap

A: "Share knowledge with the team about our database migration strategy."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "share knowledge" = knowledge-sharing vocabulary → gap. "database migration strategy" = not a predicate without implement verb.

---

## B34-compiler-012-002 — "architect" inside guidance → ARCHITECTURE fires

A: "Walk the team through how to architect a resilient microservices system."
Analysis: "walk through" = guidance frame; "architect a resilient microservices system" = embedded ARCHITECTURE action.
B: Predicates: ["ARCHITECTURE"] | TransitionType: documentation
Evidence: "architect" = design verb embedded inside guidance frame; fires ARCHITECTURE.
Key rule: Embedded architecture verbs inside guidance frames DO fire ARCHITECTURE.

---

## B34-compiler-012-003 — "build" + certification program → gap or IMPLEMENT?

A: "Build a certification program for senior engineer promotion."
Resolution: "certification program" = HR/content artifact → DOCUMENT (content creation intent).
B: Predicates: ["DOCUMENT"] | TransitionType: documentation
Note: "build a certification program tracker" → IMPLEMENT; "build a certification program" → DOCUMENT (content artifact default).

---

## B34-compiler-012-004 — v6 corruption: "coach" incorrectly fires DOCUMENT

v6 corrupt: "Coach the team on writing docs" → Predicates: ["DOCUMENT"]
Correct: "coach" → gap; "writing docs" is embedded but "coach on writing docs" is guidance frame, not a DOCUMENT action.
Analysis: Unlike "guide through writing the API documentation" (which embeds DOCUMENT action), "coach on writing" = mentoring about writing → gap.
Error type: v6-02 (gap suppression). Correct output: Predicates: [] → GapHandler.

---

## B34-compiler-012-005 — "consult" → gap

A: "Consult with the architecture team on the best approach for the new system."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "consult" = advisory/consultation vocabulary → gap. "best approach" = advisory intent, not design action.
