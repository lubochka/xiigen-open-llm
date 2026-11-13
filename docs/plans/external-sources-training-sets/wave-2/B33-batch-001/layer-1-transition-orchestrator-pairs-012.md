# Layer 1 — Transition Orchestrator Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-012-001 — v6 corruption: invented AI_AGENT predicate

v6 corrupt output: Predicates: ["IMPLEMENT", "AI_AGENT"] | Orchestration: ImplementationFlow41 → AgentAdaptFlow
Correct output: Predicates: ["IMPLEMENT"] | Orchestration: ImplementationFlow41
Error type: v6-01 (predicate invention — AI_AGENT does not exist in the nine-predicate lexicon)
Fix: AI context in object position does not create new predicates. Only the nine canonical predicates exist.

---

## TO-012-002 — v6 corruption: gap suppressed to IMPLEMENT

v6 corrupt output: "Adapt the agent for e-commerce" → Predicates: ["IMPLEMENT"]
Correct output: "Adapt the agent for e-commerce" → Predicates: [] → GapHandler
Error type: v6-02 (gap suppression — AI vocabulary gap incorrectly mapped to IMPLEMENT)
Fix: "adapt" is AI vocabulary → gap. Only code-action verbs (build, implement, create code artifact) fire IMPLEMENT.

---

## TO-012-003 — v6 corruption: multi-predicate collapse in AI context

v6 corrupt output: "Build agent, integrate CRM, secure gateway, QA" → Predicates: ["IMPLEMENT"] (collapsed)
Correct output: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "QA"]
Error type: v6-03 (multi-predicate collapse — multiple flows suppressed to single IMPLEMENT)
Fix: Each action verb fires its own predicate independently. No collapsing permitted.

---

## TO-012-004 — Orchestration ordering summary for AI context

Five canonical ordering rules:
1. ARCHITECTURE first (if present) — must complete before IMPLEMENT starts
2. API_CONTRACT before IMPLEMENT (contract spec informs implementation)
3. IMPLEMENT before TEST (tests run on implemented code)
4. SECURITY after IMPLEMENT (audits the built artifact)
5. QA always last — absolute; QA validates all preceding flow outputs

---

## TO-012-005 — DomainId for all B33 gaps

All AI agent adaptation gap requests use DomainId: software-development
Reason: There is no "ai-development" or "ml-development" domain in the registry.
AI capability gaps are classified under software-development regardless of AI specificity.
This applies to: adapt/fine-tune/orchestrate/RAG/chain-of-thought/persona/tool-calling/embedding gaps.
