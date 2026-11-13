# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-008-001 — knowledge_edge (no predicates) → no transition registered

Request: "The thing on the login page is broken again."
Predicates: [] | Orchestration: none
TransitionType: none | KnowledgeEdge: true
Action: knowledge_edge handler invoked; no flow branches instantiated; no SelectedLogicalTransition produced
Note: The orchestrator's TransitionOrchestratorUnit produces a null/empty SelectedLogicalTransition
  result with KnowledgeEdge: true. The nine-node chain is not entered.

---

## TO-008-002 — knowledge_edge from login-screen prompt

Request: "Login screen keeps showing a spinner indefinitely."
Predicates: [] | Orchestration: none
TransitionType: none | KnowledgeEdge: true
Note: "login screen", "spinner" — no predicate in any lexicon; knowledge_edge

---

## TO-008-003 — knowledge_edge does not produce a SelectedLogicalTransition

Reference pair — what the orchestrator output looks like for knowledge_edge.

Normal output (when predicates fire):
  SelectedLogicalTransition:
    Flows: [SecurityVerification34Flow, ...]
    TransitionType: feature_addition
    KnowledgeEdge: false

knowledge_edge output:
  SelectedLogicalTransition:
    Flows: []
    TransitionType: none
    KnowledgeEdge: true
    FailureClassification: no_predicate_match

---

## TO-008-004 — knowledge_edge from colloquial error description

Request: "Users are saying they get kicked out randomly."
Predicates: [] | Orchestration: none
TransitionType: none | KnowledgeEdge: true
Note: "kicked out" is colloquial for session expiry/termination; "session" not present
  If prompt had been "users' sessions are being terminated randomly":
    "session" fires SECURITY → routes to 3.4 → NOT knowledge_edge
  But "kicked out" = no predicate → knowledge_edge

---

## TO-008-005 — knowledge_edge with a QA-only prompt (QA not sufficient without routing context)

Request: "Can you QA the authentication changes from last sprint?"
Predicates: ["SECURITY", "QA"] (SECURITY fires on "authentication")
Correction: Not a knowledge_edge — SECURITY fires on "authentication"
Revised Orchestration: SecurityVerification34Flow → QAGatingFlow73
TransitionType: documentation (no IMPLEMENT)
Branch 1: SecurityVerification34Flow (3.4) | sequential-first
Branch 2: QAGatingFlow73 (7.x) | sequential-last (QA always last)
Note: "QA" alone would not produce a standalone transition; it requires other predicates.
  SECURITY fires here (authentication), so this is NOT a knowledge_edge case.
