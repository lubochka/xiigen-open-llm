# Layer 0-A — Compiler Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-004-001 — "skill-building" → gap

A: "Run a skill-building session on TypeScript for the team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001 | DomainId: software-development
Evidence: "skill-building" = training vocabulary; "session" = event noun; "TypeScript" alone is not a predicate.

---

## B34-compiler-004-002 — "training program" → gap

A: "Design a training program for the engineering team."
Analysis: "design" could fire ARCHITECTURE, but "a training program" is curriculum content, not a structural artifact.
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "design a training program" = curriculum design vocabulary → gap. ARCHITECTURE requires structural/system nouns.
Key rule: "design a [training/learning/onboarding] program" → gap. "design a [system/architecture/platform]" → ARCHITECTURE.

---

## B34-compiler-004-003 — "TDD" does not fire TEST

A: "Introduce TDD (Test-Driven Development) practices to the team."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "TDD" = abbreviated technique name; "introduce" = guidance vocabulary; neither fires TEST.
Key rule: TDD abbreviation alone does not fire TEST. TEST fires on test-writing verbs (write/create tests), not methodology vocabulary.

---

## B34-compiler-004-004 — "knowledge transfer" → gap

A: "Set up a knowledge transfer session for the backend team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "knowledge transfer" = organizational learning vocabulary → gap. "set up" ≠ code-action verb.

---

## B34-compiler-004-005 — "pair programming" → gap

A: "Facilitate pair programming sessions with junior developers."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "pair programming" = mentoring practice vocabulary; "facilitate" = facilitation verb → gap.
