# Held-Out and Corruption Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-010-A — Held-out: knowledge transfer → gap

A: "Organize a knowledge transfer from the senior team to the new hires."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "knowledge transfer" = guidance/HR vocabulary → gap.

---

## HC-010-B — Held-out: IMPLEMENT+TEST+DOCUMENT+QA four-predicate mixed

A: "Build the certification module, write tests, document the criteria, and QA the workflow before launch."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
Orchestration: ImplementationFlow41 → TestingFlow ‖ DocumentationFlow → QAGatingFlow73
TransitionType: feature_addition | QA last

---

## HC-010-C — Held-out: "advise" → gap even with technical object

A: "Advise the architecture team on microservices decomposition strategy."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "advise" = advisory vocabulary → gap regardless of technical subject matter.

---

## HC-010-D — Corruption v6-02: "advise on architecture" fires ARCHITECTURE

v6 corrupt: "Advise the team on the system architecture" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "advise" = guidance vocabulary → gap. The architectural object does not fire ARCHITECTURE. Design/architect verbs are required.
