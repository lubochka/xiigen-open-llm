# Layer 0-B — Router Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-014-001 — Gap → GapHandler: "few-shot learning" strategy

A: [] | "Use few-shot learning to help the agent handle new query categories."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "few-shot learning" = AI training strategy. No code verb. Gap ID: gap-ai-agent-llm-001.

---

## C0B-014-002 — Gap → GapHandler: "few-shot examples in prompt"

A: [] | "Add a few-shot example block to the system prompt."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "add to system prompt" = AI prompt configuration. Not a code artifact. Gap ID: gap-ai-agent-llm-001.

---

## C0B-014-003 — IMPLEMENT: "build few-shot example loader"

A: ["IMPLEMENT"] | "Build a few-shot example loader that reads from the database."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "build" fires IMPLEMENT. Software artifact is the loader service.

---

## C0B-014-004 — IMPLEMENT+TEST: build + test few-shot retrieval

A: ["IMPLEMENT", "TEST"] | "Implement the few-shot retrieval service and write unit tests for edge cases."
B: IMPLEMENT → 4.1; TEST → E2ETestingFlow53 (5.1 unit). TransitionType: feature_addition.

---

## C0B-014-005 — Gap disambiguation: "few-shot" vs "build few-shot"

"few-shot learning for the agent" → GapHandler (AI vocabulary; no code verb)
"build the few-shot example database" → ImplementationFlow41 (code verb fires)
Teaching: "few-shot" is gap vocabulary. Only code verbs applied to software artifacts (loader, database, service) fire IMPLEMENT.
