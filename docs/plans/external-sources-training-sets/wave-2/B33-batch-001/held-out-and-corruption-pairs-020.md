# Held-Out and Corruption Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-020-A — Held-out: "orchestrate" boundary cases

A (gap): "Orchestrate the agent workflows." → Predicates: [] → GapHandler → gap-ai-agent-adapt-001
A (ARCHITECTURE): "Architect the orchestration layer." → Predicates: ["ARCHITECTURE"]
A (IMPLEMENT): "Build the orchestration service." → Predicates: ["IMPLEMENT"]
A (API_CONTRACT): "Orchestrate via the orchestration API." → Predicates: ["API_CONTRACT"]
Key distinction: "orchestrate" (AI verb) → gap. Design verb + structural noun → ARCHITECTURE. Code verb + artifact → IMPLEMENT. "via API" → API_CONTRACT.

---

## HC-020-B — Corruption v6-02: "orchestrate" incorrectly mapped to ARCHITECTURE

v6 corrupt: "Orchestrate the AI agent workflows" → Predicates: ["ARCHITECTURE"]
Correct: Predicates: [] → GapHandler → gap-ai-agent-adapt-001
Error: v6-02. "orchestrate" is AI operational vocabulary → gap. It is NOT a design verb and does not fire ARCHITECTURE.

---

## HC-020-C — Held-out: "agent workflow" noun phrase without verb

A: "The agent workflow orchestration needs improvement."
B: Predicates: [] | GapHandler → gap-ai-agent-adapt-001
Reason: No action verb. Noun phrase describing AI system state → gap. "needs improvement" is not a code action verb.
Phrasing-B: Gap — observation about AI agent coordination without code action verb.

---

## HC-020-D — Held-out: "orchestrate" + "architect" in same request

A: "Orchestrate the workflows and architect the orchestration platform for scalability."
B: Predicates: ["ARCHITECTURE"] | "orchestrate" gap suppressed (ARCHITECTURE fired); gap-ai-agent-adapt-001 silently skipped.
Reason: "orchestrate" = gap (skipped); "architect" + "platform" (structural noun) → ARCHITECTURE fires.
Phrasing-B: ARCHITECTURE fires; "orchestrate" gap is suppressed when predicate fires.
