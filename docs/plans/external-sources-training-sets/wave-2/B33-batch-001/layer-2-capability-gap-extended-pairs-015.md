# Layer 2 — Capability Gap Extended Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-015 — Mixed: "build agent + fine-tune" — IMPLEMENT fires + gap warning

Request: "Build the agent and fine-tune it on customer data to improve domain accuracy."

### Flow: Mixed Predicate + Gap

**Node 1**: Parse request. Two actions: "build the agent" (code verb), "fine-tune it on customer data" (AI vocabulary).
**Node 2**: Predicate scan: "build" → IMPLEMENT fires. "fine-tune" = AI vocabulary → gap (silently skipped when predicate fires).
**Node 3**: Predicate set = [IMPLEMENT]. GapHandler NOT invoked (predicate fired).
**Node 4**: Gap warning logged internally: gap-ai-agent-llm-001 detected but suppressed.
**Node 5**: DomainId: software-development
**Node 6**: Flow: ImplementationFlow41
**Node 7**: GapWarning: {suppressedGap: gap-ai-agent-llm-001, reason: "IMPLEMENT fired; gap silently skipped"}
**Node 8**: Response: IMPLEMENT approved. Fine-tuning capability gap noted but not scheduled (predicate took priority).
**Node 9**: Verdict: IMPLEMENT_APPROVED | GapWarning: gap-ai-agent-llm-001 suppressed

---

## B33-gap-015b — Gap analysis pairs: "build + fine-tune" boundary

A: "Build the agent and fine-tune it on customer data."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-llm-001 suppressed (IMPLEMENT fired)

A: "Fine-tune the agent on customer data."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development | Note: No code verb. Pure gap.

A: "Implement the fine-tuning job and fine-tune the model."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-llm-001 suppressed

A: "Fine-tune the model and integrate with the training API."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | Note: "integrate" fires API_CONTRACT; "fine-tune" gap suppressed.

A: "Just fine-tune the model."
Predicates: [] | GapId: gap-ai-agent-llm-001 | DomainId: software-development
