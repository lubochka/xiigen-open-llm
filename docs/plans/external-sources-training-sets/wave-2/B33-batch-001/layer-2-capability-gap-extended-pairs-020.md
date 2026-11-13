# Layer 2 — Capability Gap Extended Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B33-gap-020 — API_CONTRACT fires: "orchestrate AI workflows" — gap warning

Request: "Orchestrate the AI workflows via the platform API to manage task routing across agents."

### Flow: API_CONTRACT fires + gap warning

**Node 1**: Parse request. "orchestrate" = AI operational verb (gap); "via the platform API" = integration pattern.
**Node 2**: Predicate scan: "orchestrate" = gap (AI vocabulary); "via API" pattern → API_CONTRACT fires.
**Node 3**: Predicate set = [API_CONTRACT]. Gap (from "orchestrate AI workflows") silently skipped.
**Node 4**: GapWarning: {suppressedGap: gap-ai-agent-adapt-001, reason: "API_CONTRACT fired; orchestrate gap suppressed"}
**Node 5**: DomainId: software-development
**Node 6**: Flow: APIContractFlow33
**Node 7**: Ordering: APIContractFlow33 (only predicate present)
**Node 8**: TransitionType: feature_addition
**Node 9**: Verdict: API_CONTRACT_APPROVED | GapWarning: gap-ai-agent-adapt-001 suppressed

---

## B33-gap-020b — Gap analysis pairs: "orchestrate AI workflows" vocabulary

A: "Orchestrate the agent workflows."
Predicates: [] | GapId: gap-ai-agent-adapt-001 | DomainId: software-development | Note: "orchestrate" standalone = gap.

A: "Orchestrate AI workflows via the orchestration API."
Predicates: [API_CONTRACT] | GapId: N/A | DomainId: software-development | GapWarning: gap-ai-agent-adapt-001 suppressed

A: "Architect the orchestration layer for AI workflows."
Predicates: [ARCHITECTURE] | GapId: N/A | DomainId: software-development | Note: "architect" + "layer" = ARCHITECTURE.

A: "Build the AI workflow orchestration engine."
Predicates: [IMPLEMENT] | GapId: N/A | DomainId: software-development | Note: "build" fires IMPLEMENT.

A: "The platform should orchestrate multi-step reasoning."
Predicates: [] | GapId: gap-ai-agent-adapt-001 | DomainId: software-development | Note: "should orchestrate" = strategic/AI vocabulary → gap.
