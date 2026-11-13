# Layer 2 — Capability Gap Extended Pairs (021)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-021 — "teach the team to QA" — gap (QA abbreviation in teach-to pattern)

A: "Teach the QA (Quality Assurance) team to QA the new release pipeline effectively."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "teach to QA" = "teach to X" pattern → gap. The QA abbreviation in infinitive position ("to QA") = what team will learn. Requester's action = teach = gap-skills-guidance-train-001.

A: "Train the team to QA each feature before merging."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "train to QA" = "teach to X" pattern → gap. Train = guidance vocabulary; "to QA" = infinitive learning target → gap.

A: "QA the new developer onboarding portal before the first cohort launches."
B: Predicates: ["QA"] | DomainId: software-development
Analysis: "QA [the portal]" = QA abbreviation as direct action verb → QA fires. Requester performs QA directly.

A: "Teach the engineers about quality assurance processes and then QA the current release."
B: Predicates: ["QA"] | GapWarning: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "teach about quality assurance" = gap (quality assurance spelled out = gap); "QA the current release" = QA abbreviation as direct action → QA fires. Gap warning for teach.

A: "Ensure the quality of training materials meets our standards for the new onboarding program."
B: Predicates: [] | GapId: gap-skills-guidance-train-001 | DomainId: software-development
Analysis: "ensure quality of training" = quality assurance spelled out in training context → gap. QA fires only on "QA" abbreviation; "ensure quality" and "quality assurance" → gap.
