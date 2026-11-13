# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-036-001 — v6 corruption catalogue for round-3 vocabulary

| v6 corrupt input | Error type | Correct output |
|-----------------|------------|---------------|
| LLM → LLM_CALL | v6-01 | [] → GapHandler → gap-ai-ml-model-001 |
| GPT → GPT_GENERATE | v6-01 | [] → GapHandler → gap-ai-ml-model-001 |
| BERT → BERT_ENCODE | v6-01 | [] → GapHandler → gap-ai-ml-model-001 |
| semantic-search → IMPLEMENT | v6-02 | [] → GapHandler → gap-ai-ml-model-001 |
| knowledge-graph → ARCHITECTURE | v6-02 | [] → GapHandler → gap-ai-ml-model-001 |
| build LLM + integrate API → [IMPLEMENT] only | v6-03 | ["IMPLEMENT", "API_CONTRACT"] |

---

## TO-036-002 — TransitionType: independent of AI vocabulary (round-3)

Rule: TransitionType hierarchy is unchanged by round-3 AI vocabulary.
- IMPLEMENT present (new feature) → feature_addition
- "refactor" present → refactoring
- documentation-only → documentation
"AI agent", "LLM", "vector database" in the request do not alter TransitionType.

---

## TO-036-003 — One SCHEDULE_CAPABILITY_LEARNING regardless of round count (final rule)

Rule: Even when gap terms from rounds 1, 2, and 3 appear in a single gap request, exactly one SCHEDULE_CAPABILITY_LEARNING event is emitted.
Gap ID: gap-ai-ml-model-001 (model terms dominate when present).

---

## TO-036-004 — Orchestration: build LLM orchestration service

Predicates: ["IMPLEMENT"]
Request: "Build an LLM orchestration service that manages AI agent tool calls and memory."
Flow: ImplementationFlow41 | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "LLM orchestration service" = software artifact.

---

## TO-036-005 — Complete B33 orchestration ordering reference (all rounds)

Five canonical flow ordering rules (unchanged across all B33 rounds):
1. ARCHITECTURE first — must complete before IMPLEMENT starts
2. API_CONTRACT before IMPLEMENT — contract spec informs implementation
3. IMPLEMENT before TEST — tests run on implemented code
4. IMPLEMENT before SECURITY — security audits the built artifact
5. QA always last — absolute; validates all preceding flows

None of these rules change based on AI vocabulary (rounds 1, 2, or 3) in the request.
