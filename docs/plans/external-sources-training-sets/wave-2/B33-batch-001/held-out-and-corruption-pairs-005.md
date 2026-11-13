# Held-Out and Corruption Pairs (005)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## HC-005-A — Held-out: "train the model" gap

A: "Train the model on our domain-specific dataset."
B: Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | gap-ai-agent-llm-001
Reason: "train" (ML training) = AI vocabulary → gap. "train" in ML context ≠ code-action verb.
Phrasing-B: Gap — ML training vocabulary triggers GapHandler. "Build a training pipeline" would fire IMPLEMENT.

---

## HC-005-B — Held-out: "Build the training data pipeline"

A: "Build the training data ingestion pipeline."
B: Predicates: ["IMPLEMENT"] | ImplementationFlow41 | TransitionType: feature_addition
Reason: "build" = code verb; "training data ingestion pipeline" = software component (pipeline).
Phrasing-B: IMPLEMENT — "build the [noun] pipeline" is a code-action regardless of ML context.

---

## HC-005-C — Corruption v6-03: Multi-predicate collapse

v6 corrupt: "Integrate CRM, build the agent, secure inputs, write tests" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST"]
Error type: v6-03 — collapsed four distinct predicates to one.
Fix: Each action verb fires independently. integrate→API_CONTRACT; build→IMPLEMENT; secure→SECURITY; write tests→TEST. All four must appear.
