# Held-Out and Corruption Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-004-A — Held-out: "design training program"

A: "Design a training program for the backend engineering team."
B: Predicates: [] | GapHandler → gap-skills-guidance-train-001
Reason: "design a training program" = curriculum design → gap. ARCHITECTURE not fired (training program ≠ structural artifact).
Phrasing-B: Gap — training program design is educational content, not software architecture.

---

## HC-004-B — Held-out: "design the training platform architecture"

A: "Design the training platform architecture."
B: Predicates: ["ARCHITECTURE"] | ArchitectureFlow31 (3.1) | TransitionType: documentation
Reason: "design" + "training platform architecture" = structural noun → ARCHITECTURE fires.
Phrasing-B: ARCHITECTURE — "platform architecture" is a structural artifact even in training context.

---

## HC-004-C — Corruption v6-01: TRAINING predicate invented

v6 corrupt: "Train the team on Go" → Predicates: ["TRAINING"] → TrainingFlow selected
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-01 — TRAINING predicate does not exist in the nine-predicate lexicon. No TrainingFlow.
Fix: Guidance vocabulary → GapHandler. Zero predicates for pure training/mentoring requests.
