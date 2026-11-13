# Layer 0-B — Router Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-006-001 — Router gap: "LLM" keyword alone

A: [] | "The LLM is outputting inconsistent responses"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "LLM" + observation → gap. No predicate.

---

## C0B-006-002 — Router gap: "agentic" keyword alone

A: [] | "Make the system more agentic"
B: GapHandler
Reason: "agentic" → AI vocabulary gap. "more agentic" is vague.

---

## C0B-006-003 — Router gap: "prompt" keyword alone

A: [] | "Improve the prompt for the classifier"
B: GapHandler
Reason: "improve the prompt" → prompt engineering gap. No code verb.

---

## C0B-006-004 — Router gap: "RAG" keyword + no code verb

A: [] | "Our RAG system is returning irrelevant documents"
B: GapHandler
Reason: Observation about RAG behavior → gap. No predicate.

---

## C0B-006-005 — v6-01 corruption: AI_AGENT invented

A: ["IMPLEMENT"] | "Implement the AI agent routing service"
Correct: [ImplementationFlow41]
Corrupt: ["IMPLEMENT", "AI_AGENT"] ← v6-01 (AI_AGENT is not a predicate)
Teaching: AI_AGENT is not a predicate. "agent" in any form is not a predicate trigger.
