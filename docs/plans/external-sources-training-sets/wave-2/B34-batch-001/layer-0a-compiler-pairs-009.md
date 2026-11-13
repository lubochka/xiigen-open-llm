# Layer 0-A — Compiler Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: compiler

---

## B34-compiler-009-001 — "assess" developer skills → gap

A: "Assess the team's proficiency in Go and identify skill gaps."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "assess proficiency" = evaluation vocabulary; "identify skill gaps" = HR/training vocabulary → gap.

---

## B34-compiler-009-002 — "integrate" + training context → API_CONTRACT

A: "Integrate the internal training platform with the HR management system."
B: Predicates: ["API_CONTRACT"] | TransitionType: documentation
Evidence: "integrate" always fires API_CONTRACT, even with training/HR context objects.

---

## B34-compiler-009-003 — Full gap: pure mentoring request

A: "Coach, mentor, and guide the junior engineers through the full SDLC (Software Development Lifecycle)."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: coach/mentor/guide = all guidance vocabulary → gap. SDLC abbreviation = gap (no predicate). Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B34-compiler-009-004 — "implement" + DOCUMENT: training content system

A: "Implement the internal wiki for developer knowledge sharing and document the architecture guidelines."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: "implement the wiki" → IMPLEMENT (software system); "document the guidelines" → DOCUMENT.

---

## B34-compiler-009-005 — "design" + curriculum → gap

A: "Design a curriculum for the frontend engineering bootcamp."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "design a curriculum" = educational design vocabulary → gap. ARCHITECTURE requires structural system design, not educational content.
