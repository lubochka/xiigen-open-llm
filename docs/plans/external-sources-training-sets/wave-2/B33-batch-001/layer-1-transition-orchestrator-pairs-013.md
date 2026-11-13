# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-013-001 — v6 corruption: "FINE_TUNE" invented predicate

v6 corrupt output: "Fine-tune the model on customer data" → Predicates: ["FINE_TUNE"]
Correct output: "Fine-tune the model on customer data" → Predicates: [] → GapHandler
Error type: v6-01 (predicate invention — FINE_TUNE does not exist in the nine-predicate lexicon)
Fix: "fine-tune" is AI vocabulary → gap. No new predicate FINE_TUNE exists. Gap ID: gap-ai-agent-llm-001.

---

## TO-013-002 — v6 corruption: gap suppressed for "fine-tune"

v6 corrupt output: "Fine-tune the model" → Predicates: ["IMPLEMENT"]
Correct output: "Fine-tune the model" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression — AI vocabulary gap incorrectly mapped to IMPLEMENT)
Fix: "fine-tune" is AI training vocabulary → gap. Only code-action verbs (build, implement) fire IMPLEMENT.

---

## TO-013-003 — TransitionType for fine-tuning infrastructure

Request: "Build the fine-tuning pipeline and document the configuration schema."
Predicates: ["IMPLEMENT", "DOCUMENT"]
TransitionType: feature_addition (IMPLEMENT present → always feature_addition)
Orchestration: ImplementationFlow41 → DocumentationFlow62
Note: "fine-tuning" AI context does not alter TransitionType determination.

---

## TO-013-004 — Ordering rule: API_CONTRACT before IMPLEMENT in fine-tune context

Request: "Integrate the fine-tuning service with the model registry and implement the job scheduler."
Predicates: ["API_CONTRACT", "IMPLEMENT"]
Ordering: APIContractFlow33 → ImplementationFlow41
Teaching: Standard ordering rule applies regardless of AI (fine-tuning) context.

---

## TO-013-005 — DomainId for fine-tune gap

All "fine-tune" gap requests: DomainId: software-development
Gap ID: gap-ai-agent-llm-001
SCHEDULE_CAPABILITY_LEARNING emitted once per request.
No "ai-development" or "llm-development" DomainId exists.
