# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-015-001 — v6 corruption: "REASONING" invented predicate

v6 corrupt output: "Enable chain of thought reasoning" → Predicates: ["REASONING"]
Correct output: "Enable chain of thought reasoning" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention)
Fix: "chain of thought" is AI vocabulary → gap. Gap ID: gap-ai-agent-llm-001.

---

## TO-015-002 — v6 corruption: chain-of-thought suppressed to IMPLEMENT

v6 corrupt output: "Enable chain-of-thought reasoning" → Predicates: ["IMPLEMENT"]
Correct output: "Enable chain-of-thought reasoning" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression)
Fix: "enable" in AI behavior context does not fire IMPLEMENT. Gap.

---

## TO-015-003 — ARCHITECTURE fires for chain-of-thought layer design

Request: "Design the chain-of-thought reasoning layer for the agent pipeline."
Predicates: ["ARCHITECTURE"]
TransitionType: feature_addition
Orchestration: ArchitectureFlow31 (3.1)
Teaching: "design" + "layer" (structural noun) → ARCHITECTURE. "chain-of-thought" is AI context only.

---

## TO-015-004 — Ordering rule: ARCHITECTURE before IMPLEMENT for chain-of-thought system

Request: "Design the chain-of-thought architecture and implement the prompt renderer."
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Ordering: ArchitectureFlow31 → ImplementationFlow41
Teaching: Standard ARCHITECTURE-before-IMPLEMENT ordering applies in all contexts.

---

## TO-015-005 — DomainId for chain-of-thought gap

All "chain of thought" / "CoT (chain-of-thought)" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-llm-001
SCHEDULE_CAPABILITY_LEARNING emitted once.
