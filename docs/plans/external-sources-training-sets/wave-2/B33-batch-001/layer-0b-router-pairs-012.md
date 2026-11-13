# Layer 0-B — Router Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-012-001 — IMPLEMENT+TEST+DOCUMENT+QA: full AI feature cycle

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] | "Build intent module, write tests, document taxonomy, QA sign-off"
B: IMPLEMENT → 4.1; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73. Four flows. QA last.

---

## C0B-012-002 — Gap → GapHandler: pure AI vocabulary sentence

A: [] | "The agent should use few-shot learning to adapt to new query types"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "few-shot learning", "adapt" (AI sense) → gap. No code verb.

---

## C0B-012-003 — ARCHITECTURE+IMPLEMENT: design + build agent system

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the multi-agent system architecture and implement the routing service"
B: ARCHITECTURE → 3.5 or 3.1; IMPLEMENT → 4.1. TransitionType: feature_addition.

---

## C0B-012-004 — Gap disambiguation: "orchestrate" vs "architect"

"orchestrate the agents" → GapHandler (operational AI verb, not ARCHITECTURE)
"architect the agent orchestration layer" → ARCHITECTURE (architecture verb fires)
Teaching: "orchestrate" (AI operational) ≠ "architect" (architecture verb). Only design verbs fire ARCHITECTURE.

---

## C0B-012-005 — Gap: strategic AI request

A: [] | "We need to build an AI-first customer experience platform"
B: Actually — "build" fires IMPLEMENT. The AI context is object.
Teaching: "need to build" → IMPLEMENT fires on "build". Requirement framing ("we need to") does not suppress code verbs.
CompiledPredicateIds: ["IMPLEMENT"]
