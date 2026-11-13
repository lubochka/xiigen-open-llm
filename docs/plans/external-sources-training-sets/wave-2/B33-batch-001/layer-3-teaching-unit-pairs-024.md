# Layer 3 — Teaching Unit Pairs (024)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-024 — Complete B33 teaching reference

**Teaching goal**: Provide a comprehensive single-file reference for all B33 vocabulary rules introduced in files 013–024.

**Complete B33 gap vocabulary** (no code verb → gap):

| Vocabulary | Gap ID | Notes |
|-----------|--------|-------|
| fine-tune | gap-ai-agent-llm-001 | AI training verb |
| few-shot learning | gap-ai-agent-llm-001 | AI strategy; "add to prompt" is also gap |
| chain-of-thought / CoT | gap-ai-agent-llm-001 | AI reasoning method |
| prompt engineering | gap-ai-agent-llm-001 | AI design methodology; ≠ prompt injection |
| self-healing (AI) | gap-ai-agent-adapt-001 | AI behavioral property |
| reinforcement / RLHF | gap-ai-agent-llm-001 | AI training methodology |
| orchestrate (AI context) | gap-ai-agent-adapt-001 | AI operational verb; ≠ architect |
| adapt the agent/model | gap-ai-agent-adapt-001 | AI adaptation verb |
| evolve the system | gap-ai-agent-adapt-001 | AI evolution verb |
| skill (standalone AI) | gap-ai-agent-skill-001 | AI capability vocabulary |

**Override rules** (code verbs override all gap vocabulary):

| Code verb | Predicate | Always fires on |
|-----------|-----------|----------------|
| build, implement, create, develop | IMPLEMENT | Code artifacts |
| architect, design + structural noun | ARCHITECTURE | Structural design |
| integrate, connect, wire | API_CONTRACT | All contexts |
| prevent/secure + prompt injection | SECURITY | Security context |
| write/create tests | TEST | Test-writing form |
| document | DOCUMENT | Documentation verbs |
| QA (abbreviation) | QA | Always last |

**DomainId**: software-development for all B33 gaps. No other domain applies.
**SCHEDULE_CAPABILITY_LEARNING**: Exactly one event per gap request regardless of vocabulary count.
**Gap suppression**: When any predicate fires, all gap vocabulary in the request is silently skipped.
