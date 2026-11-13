# Layer 2 — Capability Gap Extended Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-022 — Multiple AI gap vocabulary in one request

Request: "The agent needs fine-tuning, better prompt engineering, self-healing capabilities, and reinforcement learning to evolve the system."

### Flow: GapHandler Nine-Node

**Node 1**: Parse request. Multiple AI vocabulary terms: "fine-tuning", "prompt engineering", "self-healing", "reinforcement learning", "evolve the system". No code verb present.
**Node 2**: Predicate scan: None of the gap vocabulary terms fire a canonical predicate. Predicate set = [].
**Node 3**: Gap classification: Multiple LLM quality and agent adaptation terms. Dominant category: LLM quality (fine-tune, prompt engineering, reinforcement learning).
**Node 4**: Gap ID: gap-ai-agent-llm-001 (one gap ID despite multiple vocabulary terms)
**Node 5**: DomainId: software-development
**Node 6**: Emit exactly one SCHEDULE_CAPABILITY_LEARNING → AutonomousLearningPipeline
**Node 7**: Log: {id: gap-ai-agent-llm-001, request: "multiple AI capability gaps", domainId: software-development}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED | Note: One event emitted regardless of vocabulary count.

---

## B33-gap-022b — Gap analysis pairs: multiple AI vocabulary in one request

A: "Fine-tune, few-shot, chain-of-thought, self-healing, reinforcement for the agent."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development | Note: One gap ID; one event.

A: "Build an agent with fine-tuning, few-shot, chain-of-thought capabilities."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development | Note: "build" fires IMPLEMENT; all AI vocabulary suppressed.

A: "The agent needs chain-of-thought and self-healing, and we should integrate it with the feedback API."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-llm-001 and gap-ai-agent-adapt-001 suppressed (API_CONTRACT fired).

A: "Agent needs reinforcement, and prevent prompt injection attacks."
Predicates: [SECURITY] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-llm-001 suppressed (SECURITY fired).
