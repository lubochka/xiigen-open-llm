# Held-Out and Corruption Pairs (013)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-013-A — Held-out: "shadow" → gap

A: "Shadow the lead engineer on the deployment pipeline configuration."
B: Predicates: [] | GapHandler → gap-skills-guidance-mentor-001
Reason: "shadow" = observation vocabulary → gap. Requester observes, does not act.

---

## HC-013-B — Held-out: "shadow and build" → IMPLEMENT

A: "Shadow the infrastructure team and implement the auto-scaling rules for the new cluster."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "shadow" = gap warning; "implement" = requester's direct code action → IMPLEMENT.
Phrasing-B: GapWarning: gap-skills-guidance-mentor-001 | IMPLEMENT fires.

---

## HC-013-C — Corruption v6-02: "shadow while she builds" fires IMPLEMENT

v6 corrupt: "Shadow the architect while she builds the new service" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). "she builds" = third-party action. Requester's action = shadow = gap. Only requester's actions fire predicates.

---

## HC-013-D — Corruption v6-02: "shadow on the deployment" fires IMPLEMENT

v6 corrupt: "Shadow me on the production deployment" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02. "shadow on deployment" = observation vocabulary → gap. Deployment as topic ≠ IMPLEMENT trigger; shadow is the action verb.
