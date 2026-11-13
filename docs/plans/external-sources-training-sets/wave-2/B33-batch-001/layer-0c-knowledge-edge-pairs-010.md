# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-010-001 — Complete gap vocabulary list for B33

AI/Agent: adapt (AI) | agent | agentic | autonomous (AI) | chain-of-thought | context window | embedding | evaluate (ML) | few-shot | fine-tune | foundation model | grounding | hallucination | hyperparameter | inference | knowledge cutoff | LLM | model (ML) | multi-agent | orchestrate (AI) | persona (AI) | prompt | prompt engineering | RAG | reasoning (AI) | RLHF | self-adapting | self-correcting | self-improving | skill (AI) | system prompt | temperature (ML) | token | tool-calling | tool-use | train (ML) | vector | vector database | vector embedding

---

## KE-010-002 — Exception: prompt injection → SECURITY

The ONLY AI vocabulary that independently fires a predicate:
"prompt injection" / "jailbreak" / "adversarial prompt" → SECURITY

Rule: Prompt injection is treated as a security vulnerability class (like SQL injection, XSS). All other AI vocabulary → gap.

---

## KE-010-003 — "pipeline" edge in AI context

"AI pipeline" / "LLM pipeline" → gap without code verb
"ML pipeline" → gap without code verb
BUT: "build the inference pipeline" → IMPLEMENT
"implement the data pipeline" → IMPLEMENT
"design the ML pipeline architecture" → ARCHITECTURE

---

## KE-010-004 — "agent" in code artifact naming does not fire predicate

"AgentService" class (code name) → does not independently fire IMPLEMENT if no code verb
"AgentRouter" interface → does not independently fire ARCHITECTURE if no design verb
BUT: "implement the AgentService" → IMPLEMENT
"design the AgentRouter architecture" → ARCHITECTURE

Rule: Code artifact names containing "agent" do not fire predicates on their own.

---

## KE-010-005 — "benchmark" edge

"benchmark the agent" → gap (evaluation activity)
"run benchmarks" → gap (operational)
"create a benchmark suite" → IMPLEMENT ("create" is code verb; benchmark suite is software artifact)
"write benchmark tests" → TEST ("write tests")
