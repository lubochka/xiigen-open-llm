# Layer 0-C — Knowledge Edge Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-024-001 — Complete B33 gap vocabulary reference

Full list of B33 gap trigger terms (none fire predicates when used as primary verbs/nouns without code verbs):

| Term | Gap category | Gap ID |
|------|-------------|--------|
| adapt the model / adapt the agent | Agent adaptation | gap-ai-agent-adapt-001 |
| fine-tune | LLM quality | gap-ai-agent-llm-001 |
| evolve the system | Agent adaptation | gap-ai-agent-adapt-001 |
| prompt engineering | LLM quality | gap-ai-agent-llm-001 |
| self-healing (AI behavior) | Agent adaptation | gap-ai-agent-adapt-001 |
| reinforcement / RLHF | LLM quality | gap-ai-agent-llm-001 |
| few-shot learning | LLM quality | gap-ai-agent-llm-001 |
| chain-of-thought / CoT | LLM quality | gap-ai-agent-llm-001 |
| orchestrate (AI context) | Agent adaptation | gap-ai-agent-adapt-001 |
| skill (standalone AI capability) | Agent skill | gap-ai-agent-skill-001 |
| agent (standalone, no code verb) | Agent adaptation | gap-ai-agent-adapt-001 |

---

## KE-024-002 — Code verbs that override all AI objects

Code verbs that fire predicates regardless of AI vocabulary in the object:

| Verb | Predicate | Note |
|------|-----------|------|
| build, implement, create, develop, write code, refactor | IMPLEMENT | Code-action verbs |
| design, architect, plan, spec + structural noun | ARCHITECTURE | Structural noun required |
| integrate, connect, wire, interface with | API_CONTRACT | Always fires; no restriction |
| secure, harden, prevent prompt injection | SECURITY | "prompt injection" is sufficient |
| write tests, create tests, add tests | TEST | Test-writing form required |
| document, write docs | DOCUMENT | Documentation verbs |
| QA (abbreviation) | QA | Abbreviation only; must be last |

---

## KE-024-003 — Gap suppression rule for mixed requests

Rule: When any code predicate fires in a mixed gap+code request, gap vocabulary is silently skipped.
Example: "orchestrate the agent and integrate with the CRM API" → ["API_CONTRACT"]
"orchestrate" (gap) is silently skipped; "integrate" (API_CONTRACT) fires. Result contains only predicates, never both gap and predicates.

---

## KE-024-004 — DomainId is always software-development for all B33 gaps

All B33 gap IDs use DomainId: software-development.
There is no "ai-development", "ml-development", or "llm-development" domain in the registry.
This rule applies to: adapt / fine-tune / orchestrate / RAG / chain-of-thought / persona / tool-calling / embedding / reinforcement / self-healing / model evolution gaps.

---

## KE-024-005 — One SCHEDULE_CAPABILITY_LEARNING per gap request

Rule: Exactly one SCHEDULE_CAPABILITY_LEARNING event is emitted per gap request, regardless of how many gap vocabulary terms appear.
Example: "fine-tune, few-shot, chain-of-thought, self-healing, reinforcement" in one request → one SCHEDULE_CAPABILITY_LEARNING, one Gap ID (most specific applicable).
Never emit multiple SCHEDULE_CAPABILITY_LEARNING events for a single request.
