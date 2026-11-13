# Layer 2 — Capability Gap Extended Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-013 — GapHandler nine-node flow: "fine-tune the model"

Request: "We need to fine-tune the model on our internal support ticket data."

### Flow: GapHandler Nine-Node

**Node 1**: Parse request. "fine-tune" = AI training verb. "model" = AI artifact. "support ticket data" = domain-specific training corpus.
**Node 2**: Predicate scan: "fine-tune" does not fire any of the nine canonical predicates. Predicate set = [].
**Node 3**: Gap classification: LLM quality vocabulary. Request concerns ML training methodology.
**Node 4**: Gap ID: gap-ai-agent-llm-001
**Node 5**: DomainId: software-development
**Node 6**: Emit SCHEDULE_CAPABILITY_LEARNING → AutonomousLearningPipeline
**Node 7**: Log: {id: gap-ai-agent-llm-001, request: "fine-tune on support ticket data", domainId: software-development}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED

---

## B33-gap-013b — Gap analysis pairs: "fine-tune" vocabulary

A: "Fine-tune the model on customer service conversations."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development

A: "Build a fine-tuning pipeline that ingests customer conversations."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development | Note: IMPLEMENT fires on "build"; gap not logged.

A: "Adapt the model by fine-tuning it on proprietary legal documents."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development | Note: Both "adapt" and "fine-tune" are gap vocabulary.

A: "Integrate the fine-tuning service with the model registry API."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | Note: "integrate" fires API_CONTRACT regardless of fine-tune context.
