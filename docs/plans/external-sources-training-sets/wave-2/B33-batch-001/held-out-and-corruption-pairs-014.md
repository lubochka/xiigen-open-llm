# Held-Out and Corruption Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-014-A — Held-out: "few-shot" boundary cases

A (gap): "Use few-shot learning to help the agent adapt." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (gap): "Add few-shot examples to the system prompt." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (IMPLEMENT): "Build the few-shot example retrieval service." → Predicates: ["IMPLEMENT"] → ImplementationFlow41
Distinction: "use/add to prompt" = AI vocabulary → gap. "build service" = code verb → IMPLEMENT.

---

## HC-014-B — Held-out: "few-shot" + code predicate mixed

A: "Add few-shot examples to the prompt and integrate the prompt template with the recommendation API."
B: Predicates: ["API_CONTRACT"] | GapHandler suppressed; gap-ai-agent-llm-001 silently skipped.
Reason: "add to prompt" = gap; "integrate with API" → API_CONTRACT fires. Mixed: only predicate returned.

---

## HC-014-C — Corruption v6-02: "few-shot" suppressed to IMPLEMENT

v6 corrupt: "Use few-shot learning for the agent" → Predicates: ["IMPLEMENT"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-llm-001
Error: v6-02 gap suppression. "use" in AI strategy context does not fire IMPLEMENT. Only code-action verbs fire IMPLEMENT.

---

## HC-014-D — Held-out: "few-shot" + "evaluate" double AI vocabulary

A: "Evaluate whether few-shot prompting improves accuracy for our classification task."
B: Predicates: [] | GapHandler → gap-ai-agent-llm-001
Reason: "evaluate" = AI quality assessment + "few-shot prompting" = AI strategy. Both gap vocabulary. No code verb.
Phrasing-B: Gap — multiple AI assessment terms in one request still yield one gap ID and one event.
