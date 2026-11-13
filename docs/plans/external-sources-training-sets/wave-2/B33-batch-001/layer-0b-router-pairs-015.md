# Layer 0-B — Router Pairs (015)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-015-001 — Gap → GapHandler: "chain of thought" standalone

A: [] | "Enable chain of thought reasoning in the agent's response pipeline."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "chain of thought" + "enable" (AI context) = AI reasoning vocabulary. No code verb. Gap ID: gap-ai-agent-llm-001.

---

## C0B-015-002 — IMPLEMENT: "implement chain-of-thought renderer"

A: ["IMPLEMENT"] | "Implement a chain-of-thought prompt renderer that formats reasoning steps."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "implement" fires IMPLEMENT. "renderer" is the software artifact.

---

## C0B-015-003 — ARCHITECTURE: "design chain-of-thought layer"

A: ["ARCHITECTURE"] | "Design the chain-of-thought reasoning layer for the agent pipeline."
B: ARCHITECTURE → ArchitectureFlow31 (3.1 or 3.5). TransitionType: feature_addition.
Reason: "design" + "layer" (structural noun) → ARCHITECTURE. "chain-of-thought" is AI context only.

---

## C0B-015-004 — IMPLEMENT fires when "build" + AI object: boundary case

A: ["IMPLEMENT"] | "Build a chain-of-thought agent that reasons step-by-step before answering."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build a chain-of-thought agent" → IMPLEMENT fires on "build". The agent is a software artifact; "chain-of-thought" is the AI design pattern applied to it.

---

## C0B-015-005 — Gap: "chain of thought" evaluation

A: [] | "Does chain of thought reasoning reduce hallucinations in the production agent?"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Question about AI behavior quality. No code verb. Gap ID: gap-ai-agent-llm-001.
