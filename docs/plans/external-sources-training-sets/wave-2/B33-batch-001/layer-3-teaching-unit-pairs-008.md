# Layer 3 — Teaching Unit Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-008 — GapHandler: one SCHEDULE_CAPABILITY_LEARNING per request

**Teaching goal**: One gap → one event emission, one gap ID, one log entry — regardless of how many gap verbs appear.

**Rule**: GapHandler emits exactly one SCHEDULE_CAPABILITY_LEARNING per request. Multiple gap verbs in one request do not produce multiple events.

**Gap ID selection priority** (when multiple gap categories present):
1. If adaptation vocabulary present → gap-ai-agent-adapt-001 (highest priority)
2. If skill/capability vocabulary present (no adaptation) → gap-ai-agent-skill-001
3. If LLM/model/reasoning vocabulary only → gap-ai-agent-llm-001

**Examples**:

| Request | Gap verbs | Gap ID selected |
|---------|-----------|-----------------|
| "Adapt and fine-tune the agent" | adapt + fine-tune | gap-ai-agent-adapt-001 (adaptation wins) |
| "Enable tool-calling and chain-of-thought" | enable tool-calling + chain-of-thought | gap-ai-agent-skill-001 (skill over LLM) |
| "Improve reasoning and grounding" | improve reasoning + grounding | gap-ai-agent-llm-001 (LLM quality only) |

**Response is always identical**:
"Capability not available. Request logged for learning pipeline."
No variation based on gap ID or domain.
