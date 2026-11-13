# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-024-001 — Complete B33 orchestration ordering reference

Five canonical flow ordering rules (same in all batches including AI context):
1. ARCHITECTURE first — must complete before IMPLEMENT starts
2. API_CONTRACT before IMPLEMENT — contract spec informs implementation
3. IMPLEMENT before TEST — tests run on implemented code
4. IMPLEMENT before SECURITY — security audits the built artifact
5. QA always last — absolute; validates all preceding flows

None of these rules change based on AI vocabulary in the request.

---

## TO-024-002 — v6 corruption catalogue for B33 extended vocabulary

| v6 corrupt input | Error type | Correct output |
|-----------------|------------|---------------|
| fine-tune → FINE_TUNE | v6-01 | [] → GapHandler |
| few-shot → FEW_SHOT | v6-01 | [] → GapHandler |
| chain-of-thought → REASONING | v6-01 | [] → GapHandler |
| self-healing → SELF_HEAL | v6-01 | [] → GapHandler |
| reinforcement → REINFORCE | v6-01 | [] → GapHandler |
| adapt → IMPLEMENT | v6-02 | [] → GapHandler |
| orchestrate → ARCHITECTURE | v6-02 | [] → GapHandler |
| build + integrate → [IMPLEMENT] only | v6-03 | ["IMPLEMENT", "API_CONTRACT"] |

---

## TO-024-003 — TransitionType determination is independent of AI vocabulary

TransitionType hierarchy (same for all batches):
- IMPLEMENT present → feature_addition (unless refactor context)
- refactor without IMPLEMENT → refactoring
- documentation-only → documentation
- QA-only → N/A (QA is sub-phase, not TransitionType)
AI vocabulary (fine-tune, adapt, orchestrate, reinforcement) does not alter TransitionType determination.

---

## TO-024-004 — One gap ID per request — selection priority

When multiple gap vocabulary terms appear in one request, select the most specific gap ID:
1. gap-ai-agent-llm-001 — LLM quality vocabulary dominant: fine-tune, chain-of-thought, reinforcement, few-shot
2. gap-ai-agent-adapt-001 — Agent adaptation dominant: adapt, orchestrate, self-healing, evolve
3. gap-ai-agent-skill-001 — Skill vocabulary dominant: skill, tool-calling, capability, teach
Apply first matching in order of specificity for the request intent.

---

## TO-024-005 — DomainId consistency rule for all B33 gaps

All B33 gaps regardless of vocabulary: DomainId: software-development
Rationale: No "ai-development", "ml-development", "llm-development" domain exists in the registry.
This rule is absolute and applies to every gap vocabulary term in B33.
