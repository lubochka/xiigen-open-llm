# Layer 0-B — Router Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: router

---

## B34-router-009-001 — Gap route: "hand off" → gap

A: "Hand off the codebase knowledge to the new team lead."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "hand off" = knowledge transfer vocabulary → gap.

---

## B34-router-009-002 — IMPLEMENT route: "build" + LMS feature

A: "Build the progress badge system for the internal learning management system."
Router decision: Predicates = ["IMPLEMENT"] → ImplementationFlow41
Evidence: "build" = code verb; "progress badge system" = software component.

---

## B34-router-009-003 — DOCUMENT route: "write" + style guide

A: "Write a code style guide for all new developers joining the team."
Router decision: Predicates = ["DOCUMENT"] → DocumentationFlow
Evidence: "write a code style guide" → DOCUMENT.

---

## B34-router-009-004 — Gap route: "observe" → gap

A: "Observe how the senior developers handle production incidents."
Router decision: Predicates = [] → GapHandler → gap-skills-guidance-mentor-001
Evidence: "observe" = observation/shadowing vocabulary → gap.

---

## B34-router-009-005 — v6 corruption: "training program" incorrectly fires IMPLEMENT

v6 corrupt: "Design a training program for backend engineers" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02 (gap suppression). "design a training program" = curriculum design → gap; ARCHITECTURE not fired (no structural noun); IMPLEMENT not fired (no code artifact noun).
