# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-023-001 — v6 corruption: "evolve the system" suppressed to IMPLEMENT

v6 corrupt output: "Evolve the system through orchestrator adaptation" → Predicates: ["IMPLEMENT"]
Correct output: Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "evolve the system" = AI adaptation vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.

---

## TO-023-002 — ARCHITECTURE+IMPLEMENT correctly fires for model lifecycle

Request: "Design the model lifecycle architecture and implement the checkpoint promotion pipeline."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-023-003 — API_CONTRACT fires for model evolution tracker integration

Request: "Integrate the model evolution tracker with the registry API."
Predicates: ["API_CONTRACT"]
Orchestration: APIContractFlow33
Teaching: "integrate" fires API_CONTRACT. "model evolution tracker" is AI context only.

---

## TO-023-004 — GapHandler: "skill registry update" is gap not IMPLEMENT

Request: "Update the orchestrator's skill registry as new capabilities emerge."
Predicates: []
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING → gap-ai-agent-adapt-001
Teaching: "update" applied to AI skill registry = AI configuration → gap. Not IMPLEMENT.

---

## TO-023-005 — DomainId for model evolution gap

All "evolve the system" / "model evolution" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-adapt-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
