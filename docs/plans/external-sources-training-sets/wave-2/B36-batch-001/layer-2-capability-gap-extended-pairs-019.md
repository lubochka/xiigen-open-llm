# Layer 2 — Capability Gap Extended Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-019-001 — "user story points" — DOCUMENT fires on "story" + gap warning

A: "Assign user story points to all backlog items for the sprint."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "story" in "user story points" fires DOCUMENT (literal lexicon match).
    "backlog" and "sprint" are gap vocabulary. DomainId: software-development.
    Gap warning: story-point estimation intent not captured by DOCUMENT routing.
    DOCUMENT fires → SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-L2-019-002 — "story points" alone fires DOCUMENT

A: "Estimate story points for each backlog ticket before the sprint starts."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "story" in "story points" fires DOCUMENT. "backlog", "ticket", "sprint"
    are gap. DOCUMENT fires via "story" substring. Gap warning emitted.

---

## B36-L2-019-003 — "stories" fires DOCUMENT (plural)

A: "Review backlog stories and assign story points for the upcoming sprint."

B:
  Predicates: [QA, DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "review" fires QA. "stories" contains "story" → DOCUMENT fires. "story points"
    also fires DOCUMENT via "story". "backlog" and "sprint" are gap. Two predicates fire.

---

## B36-L2-019-004 — "story" in documentation context: true positive

A: "Write the user story for the payment flow and document the acceptance criteria."

B:
  Predicates: [DOCUMENT]
  GapId: null (no planning gap vocabulary — no sprint/backlog/velocity etc.)
  DomainId: software-development
  Analysis: "story" fires DOCUMENT (true positive: user story authoring). "document"
    fires DOCUMENT. "criteria" fires DOCUMENT. No planning vocabulary present
    → no gap warnings. DomainId: software-development.

---

## B36-L2-019-005 — "story points" false positive: model must not suppress DOCUMENT

A: "We're doing sprint planning — estimate story points for these backlog items."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "story" fires DOCUMENT (false positive: intent is effort estimation, not
    user story authoring). The compiler is literal. DOCUMENT fires unconditionally.
    The model MUST NOT suppress DOCUMENT to preserve knowledge-edge. Gap warning emitted.
  Hard rule: False positives are correct outputs. Do not suppress predicates.
