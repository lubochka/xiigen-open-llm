# Layer 0-A — Compiler Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-006-001 — "bootcamp" → gap

A: "Run a two-week coding bootcamp for new hires."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Evidence: "bootcamp" = intensive training vocabulary; "run a bootcamp" = facilitation verb → gap.

---

## B34-compiler-006-002 — "cross-train" → gap

A: "Cross-train the frontend team on backend APIs."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "cross-train" = skill-building/training vocabulary → gap.

---

## B34-compiler-006-003 — "build + training portal" → IMPLEMENT

A: "Build a self-service training portal for the engineering team."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "build" = code verb; "self-service training portal" = software application (portal). Code verb overrides.
Note: "portal" is a software artifact noun. "build a training portal" → IMPLEMENT even with guidance context.

---

## B34-compiler-006-004 — IMPLEMENT+DOCUMENT: build and document

A: "Build the onboarding checklist system and document the process flow."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: "build" → IMPLEMENT; "document" → DOCUMENT. Both fire.

---

## B34-compiler-006-005 — "show how" → gap

A: "Show the team how to set up a CI pipeline."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "show how" = instructional vocabulary → gap. No predicate fires.
Note: "Set up a CI pipeline" alone also does not fire predicates ("set up" = configuration, not code-action).
