# Layer 2 — Capability Gap Extended Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-024 — Complete B33 capability-gap summary

### B33 Gap ID Reference

| Gap ID | Category | Triggers | DomainId |
|--------|----------|----------|----------|
| gap-ai-agent-adapt-001 | Agent adaptation | adapt, orchestrate (standalone), self-healing (AI), evolve the system, persona, domain shift, tone shift | software-development |
| gap-ai-agent-skill-001 | Agent skill/capability | tool-calling, skill (standalone), capability (AI), teach (AI), enable (AI behavior), expose as API | software-development |
| gap-ai-agent-llm-001 | LLM quality | fine-tune, chain-of-thought, few-shot, prompt engineering, reinforcement, RLHF, grounding, hallucination, temperature, context window | software-development |

---

## B33-gap-024b — Summary: Nine teaching points for B33 extended vocabulary

**1. fine-tune** → gap-ai-agent-llm-001 always. Code verb applied to fine-tune artifact → IMPLEMENT.
**2. few-shot** → gap-ai-agent-llm-001. "add to prompt" = gap. "build loader" = IMPLEMENT.
**3. chain-of-thought** → gap-ai-agent-llm-001. "implement renderer" = IMPLEMENT. "design layer" = ARCHITECTURE.
**4. prompt engineering** → gap-ai-agent-llm-001. "prompt injection" = SECURITY (exception).
**5. self-healing** → gap-ai-agent-adapt-001. "implement retry logic" = IMPLEMENT.
**6. reinforcement / RLHF** → gap-ai-agent-llm-001. "build feedback collector" = IMPLEMENT.
**7. orchestrate** → gap-ai-agent-adapt-001. "via API" → API_CONTRACT. "architect layer" → ARCHITECTURE.
**8. adapt** → gap-ai-agent-adapt-001. "build the agent" = IMPLEMENT. Primary verb wins.
**9. evolve the system** → gap-ai-agent-adapt-001. "build versioning system" = IMPLEMENT.

---

## B33-gap-024c — Gap analysis pairs: final summary cases

A: "Fine-tune, orchestrate, self-healing, reinforcement — no code verb."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development | Note: One event, one gap ID.

A: "Architect the agent platform, integrate the API, implement the scheduler, secure against injection, write tests, document, QA."
Predicates: [ARCHITECTURE, API_CONTRACT, IMPLEMENT, SECURITY, TEST, DOCUMENT, QA] | GapId: N/A | DomainId: software-development | Note: All seven predicates fire; all AI vocabulary in object position suppressed.

A: "Orchestrate and fine-tune the agent system."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development | Note: Both gap vocabulary terms; one event.

A: "Orchestrate via API and fine-tune the model."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-llm-001 suppressed (API_CONTRACT fired on "via API").
