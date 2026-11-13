# Held-Out and Corruption Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-005-A — Held-out: "quality assurance" spelled out → gap

A: "Ensure quality assurance of the developer onboarding program."
B: Predicates: [] | GapHandler → gap-skills-guidance-onboard-001
Reason: "quality assurance" spelled out → gap. QA abbreviation required for QA predicate.
Phrasing-B: Gap — "quality assurance" spelled out never fires QA predicate.

---

## HC-005-B — Held-out: "QA" abbreviation → QA fires

A: "QA the developer onboarding workflow before the first cohort starts."
B: Predicates: ["QA"] | QAGatingFlow73 (7.3) | TransitionType: no contribution
Reason: "QA" abbreviation → QA fires. "onboarding workflow" = feature acceptance gate context → 7.3.
Phrasing-B: QA fires — abbreviation is the trigger. Same predicate regardless of context.

---

## HC-005-C — Corruption v6-02: "ensure quality" fires QA

v6 corrupt: "Ensure the quality of our training content" → Predicates: ["QA"] → QAGatingFlow73
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-02. "quality" spelled out → gap. QA fires only on the two-letter abbreviation "QA."

---

## HC-005-D — Held-out: mixed guidance + one predicate

A: "Coach the team and write the technical onboarding guide."
B: Predicates: ["DOCUMENT"] | DocumentationFlow | TransitionType: documentation
"coach" → gap warning; "write the onboarding guide" → DOCUMENT.
GapHandler NOT invoked (DOCUMENT fires).
