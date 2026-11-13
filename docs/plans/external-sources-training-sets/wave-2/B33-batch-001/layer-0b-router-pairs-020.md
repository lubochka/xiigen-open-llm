# Layer 0-B — Router Pairs (020)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-020-001 — Gap → GapHandler: "orchestrate agent workflows" — AI context

A: [] | "Orchestrate the agent workflows so tasks are handed off between specialist agents."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "orchestrate" in AI context = operational AI vocabulary → gap. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-020-002 — API_CONTRACT fires: "orchestrate via API" boundary

A: ["API_CONTRACT"] | "Orchestrate the agent calls via the orchestration API to manage task routing."
B: API_CONTRACT → APIContractFlow33.
Reason: "via API" pattern triggers API_CONTRACT. "orchestrate" context does not suppress API_CONTRACT.

---

## C0B-020-003 — Gap → GapHandler: "orchestrate" standalone

A: [] | "The platform should orchestrate multi-step reasoning across agents."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "orchestrate" standalone in AI context → gap. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-020-004 — ARCHITECTURE fires: "architect the orchestration layer"

A: ["ARCHITECTURE"] | "Architect the orchestration layer that coordinates tasks across specialist agents."
B: ARCHITECTURE → ArchitectureFlow31 (3.1).
Reason: "architect" = design verb + "layer" = structural noun → ARCHITECTURE. "orchestration" is noun context only.

---

## C0B-020-005 — IMPLEMENT fires: "build orchestration service" boundary

A: ["IMPLEMENT"] | "Build the agent orchestration service that routes tasks to the appropriate specialist."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "orchestrate" (AI verb) → gap. "build orchestration service" (code verb + software artifact) → IMPLEMENT. Noun form of "orchestration" does not gap-ify "build".
