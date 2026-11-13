# Layer 0-B — Router Pairs (016)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-016-001 — Gap → GapHandler: "prompt engineering" strategy

A: [] | "We need better prompt engineering to reduce hallucinations."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "prompt engineering" = AI vocabulary. "need" is not a code verb. Gap ID: gap-ai-agent-llm-001.

---

## C0B-016-002 — IMPLEMENT: "build prompt engineering toolkit"

A: ["IMPLEMENT"] | "Build a prompt engineering toolkit with a template manager and A/B test harness."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "build" fires IMPLEMENT. "toolkit" is the software artifact.

---

## C0B-016-003 — IMPLEMENT+TEST: create prompt template library + tests

A: ["IMPLEMENT", "TEST"] | "Create the prompt template library and write regression tests for template outputs."
B: IMPLEMENT → 4.1; TEST → E2ETestingFlow53 (5.1). TransitionType: feature_addition.

---

## C0B-016-004 — Gap disambiguation: "prompt engineering" vs "build prompt system"

"prompt engineering for the agent" → GapHandler (AI vocabulary → gap)
"build the prompt management system" → ImplementationFlow41 (code verb fires)
Teaching: "prompt engineering" is always AI vocabulary. Only code verbs applied to software systems fire IMPLEMENT.

---

## C0B-016-005 — Gap: "prompt engineering" observation

A: [] | "The current prompt engineering approach is producing inconsistent agent responses."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Observation. No action verb. "prompt engineering" + "agent responses" → gap. Gap ID: gap-ai-agent-llm-001.
