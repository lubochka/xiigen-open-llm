# Layer 1 — Transition Orchestrator Pairs (048)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-048-001 — Orchestration: ARCHITECTURE requires structural noun with design/architect verb

Rule: "design the LLM serving layer" (no structural noun) → may not fire ARCHITECTURE.
"design the LLM serving architecture" → ARCHITECTURE fires ("architecture" = structural noun).
"design the LLM serving platform" → ARCHITECTURE fires ("platform" = structural noun).

---

## TO-048-002 — Orchestration: TEST requires test-writing form

Rule: "test the semantic search" ≠ TEST predicate.
"write tests for the semantic search" = TEST predicate.
"create tests for the semantic search" = TEST predicate.
"add unit tests for the semantic search" = TEST predicate.
Only test-writing verb forms fire TEST.

---

## TO-048-003 — Orchestration: QA abbreviation required for QA predicate

Rule: "quality assurance the platform" → does NOT fire QA.
"QA the platform" (abbreviation) → fires QA.
Only the abbreviation "QA" fires QAGatingFlow73.

---

## TO-048-004 — Orchestration: implement + QA (two-predicate: IMPLEMENT then QA)

Predicates: ["IMPLEMENT", "QA"]
Request: "Implement the vector database indexing service and QA the query accuracy."
Flow order: ImplementationFlow41 → QAGatingFlow73 | TransitionType: feature_addition
Rule: QA always last.

---

## TO-048-005 — Orchestration: all five canonical ordering rules applied simultaneously

Request with all five rules triggered:
"Design architecture, integrate API, implement service, write tests, secure endpoint, document, QA."
→ ARCHITECTURE → API_CONTRACT → IMPLEMENT → TEST / SECURITY → DOCUMENT → QA
(TEST and SECURITY both after IMPLEMENT; DOCUMENT can be last before QA; QA absolute last.)
