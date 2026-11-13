# Held-Out and Corruption Pairs (023)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-023-A — Held-out: "model evolution" boundary cases

A (gap): "Evolve the system through orchestrator adaptation." → Predicates: [] → GapHandler → gap-ai-agent-adapt-001
A (IMPLEMENT): "Build the model versioning system." → Predicates: ["IMPLEMENT"]
A (ARCHITECTURE): "Design the model lifecycle architecture." → Predicates: ["ARCHITECTURE"]
Key distinction: AI evolution vocabulary → gap. Code verb + artifact → IMPLEMENT. Design verb + structural noun → ARCHITECTURE.

---

## HC-023-B — Held-out: "update skill registry" is gap not IMPLEMENT

A: "Update the orchestrator's skill registry to add new agent capabilities."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: "update" applied to AI skill registry = AI configuration change → gap. Not a software version update.
Phrasing-B: Gap — "update" in AI skill/capability context is AI vocabulary, not IMPLEMENT.

---

## HC-023-C — Corruption v6-02: "evolve the system" mapped to IMPLEMENT

v6 corrupt: "Evolve the system through orchestrator adaptation" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-adapt-001
Error: v6-02. "evolve" is AI adaptation vocabulary → gap. No code artifact implied.

---

## HC-023-D — Held-out: "model evolution" + "integrate" mixed

A: "Evolve the model and integrate the evolution tracker with the checkpoint API."
B: Predicates: ["API_CONTRACT"] | GapHandler suppressed; gap-ai-agent-adapt-001 silently skipped.
Reason: "evolve" = gap (skipped); "integrate with API" → API_CONTRACT fires.
Phrasing-B: API_CONTRACT — "integrate" fires regardless of "evolve" gap vocabulary.
