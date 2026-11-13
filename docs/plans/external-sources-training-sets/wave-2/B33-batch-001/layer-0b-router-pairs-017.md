# Layer 0-B — Router Pairs (017)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-017-001 — Gap → GapHandler: "self-healing" agent behavior

A: [] | "The agent should be self-healing and automatically correct reasoning errors."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "self-healing" = AI agent behavior vocabulary. No code verb. Gap ID: gap-ai-agent-adapt-001.

---

## C0B-017-002 — IMPLEMENT: "implement self-healing retry logic"

A: ["IMPLEMENT"] | "Implement self-healing retry logic that re-prompts the agent when validation fails."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "implement" fires IMPLEMENT. "retry logic" is the software artifact. "self-healing" is AI-adjacent adjective.

---

## C0B-017-003 — ARCHITECTURE+IMPLEMENT: "design + build self-healing system"

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the self-healing architecture for failure recovery and implement the fallback routing service."
B: ARCHITECTURE → ArchitectureFlow31; IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.

---

## C0B-017-004 — Gap disambiguation: "self-healing" vs "implement self-healing"

"self-healing agent" → GapHandler (AI behavior vocabulary → gap)
"implement the self-healing retry service" → ImplementationFlow41 (code verb fires)
Teaching: "self-healing" as an AI agent property → gap. Code verb applied to a software retry service → IMPLEMENT.

---

## C0B-017-005 — Gap: "self-healing" evaluation question

A: [] | "How do self-healing agents decide when to retry versus escalate?"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Question about AI agent strategy. No code verb. Gap ID: gap-ai-agent-adapt-001.
