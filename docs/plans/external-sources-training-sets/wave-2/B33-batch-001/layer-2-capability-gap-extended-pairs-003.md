# Layer 2 — Capability Gap Extended Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B33-gap-003 — GapHandler nine-node flow: "Fine-tune the model on proprietary data"

Request: "Fine-tune the model on our proprietary customer support data."

### Flow: GapHandler Nine-Node

**Node 1**: Parse request. Action verb: "fine-tune". Lexicon check: fine-tune → ML training vocabulary → gap candidate.
**Node 2**: Predicate scan: no code-action verb found ("fine-tune" is not build/implement/create). Predicate set = [].
**Node 3**: Gap classification: model training/optimization request. Gap category: LLM quality.
**Node 4**: Gap ID assignment: gap-ai-agent-llm-001
**Node 5**: DomainId assignment: software-development
**Node 6**: Emit SCHEDULE_CAPABILITY_LEARNING → AutonomousLearningPipeline
**Node 7**: Log to gap registry: {id: gap-ai-agent-llm-001, request: "fine-tune on proprietary data", domainId: software-development}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED — ML training request queued; no predicate flow executed.

---

**Key distinction**: "fine-tune" ≠ IMPLEMENT. Even though it modifies model behavior, fine-tuning is ML vocabulary, not code-action vocabulary. "Build a fine-tuning pipeline" → IMPLEMENT (code verb overrides).
