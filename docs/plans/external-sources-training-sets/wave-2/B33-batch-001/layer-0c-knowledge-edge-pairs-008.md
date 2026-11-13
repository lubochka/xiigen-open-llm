# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-008-001 — Observation statements are always gap

Patterns → always gap regardless of AI vocabulary:
"[subject] is/are [adjective/noun]" (description statement)
"[subject] does not [verb]" (problem statement)
"[subject] needs [noun]" (requirement without action verb)
"[subject] should [verb]" (requirement without code verb)

Examples:
"The agent is not performing well" → gap
"The LLM responses are inconsistent" → gap
"The agent needs better memory" → gap
"The system should be more autonomous" → gap

---

## KE-008-002 — Strategic/business statements are gap

"We need an AI-first approach" → gap
"Our AI strategy needs updating" → gap
"The agent should handle more use cases" → gap
"We want to leverage LLMs in our product" → gap

---

## KE-008-003 — Comparative statements are gap

"Agent A is better than Agent B" → gap
"GPT-4 outperforms GPT-3.5 on our task" → gap
"Our RAG accuracy is 72%, need 85%" → gap

---

## KE-008-004 — "evaluate" in ML context is always gap

"evaluate the model performance" → gap
"evaluate agent responses" → gap
"run evaluation benchmarks" → gap
"assess LLM accuracy" → gap

Contrast — code-related evaluation:
"write evaluation tests" → TEST (test-writing verb)
"build an evaluation harness" → IMPLEMENT (build is code verb)

---

## KE-008-005 — DomainId and gap categorization for B33

All B33 gaps:
- DomainId: "software-development" (not "ai-development" — no such domain)
- GapIds: gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001
- SCHEDULE_CAPABILITY_LEARNING event → subproject 16 (AutonomousLearningPipeline)
