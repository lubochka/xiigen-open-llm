# Layer 3 — Teaching Unit Pairs (020)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-020 — Guidance + IMPLEMENT embedded teaching unit

**Core rule**: IMPLEMENT fires when "build/implement/create + software noun" appears as an embedded action in the requester's participial (active) form. Does NOT fire in infinitive learning-target form ("coach on how to build").

**IMPLEMENT fires inside guidance frames (participial form)**:
| Request | IMPLEMENT fires? | Form |
|---------|-----------------|------|
| "Coach through building the service" | yes | building (participial) |
| "Mentor through implementing the pipeline" | yes | implementing (participial) |
| "Guide through refactoring the module" | yes | refactoring (participial) |
| "Walk through building + implement it" | yes | implement (direct verb) |

**IMPLEMENT does NOT fire (infinitive learning-target form)**:
| Request | IMPLEMENT fires? | Reason |
|---------|-----------------|--------|
| "Coach on how to build APIs" | no | "teach to X" pattern |
| "Advise on building best practices" | no | advisory + noun |
| "Help understand how to implement" | no | learning intent |
| "Facilitate team building the service" | no | third-party action |

**TransitionType**:
IMPLEMENT in guidance context → feature_addition (default)
"refactoring" intent inside guidance frame → refactoring

**Gap ID**: gap-skills-guidance-mentor-001 (coach/mentor/guide); gap-skills-guidance-train-001 (show/demonstrate).
