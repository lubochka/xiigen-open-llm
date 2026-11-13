# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-011-001 — v6-02 orchestration: "login issues" → knowledge_edge (not SECURITY routing)

Request: "We're having login issues since the JWT update."
Predicates: Analysis required

Correct analysis:
  "login" — NOT in SECURITY lexicon → no SECURITY predicate
  "jwt" — IS in SECURITY lexicon → SECURITY fires
  "update" — not in any predicate lexicon

Result: SECURITY fires (on "jwt") → NOT knowledge_edge → routes to 3.4
Orchestration: SecurityVerification34Flow
TransitionType: feature_addition

v6-02 error (what the corrupt model would do):
  Wrong: sees "login issues" as the main signal; ignores "jwt"; returns knowledge_edge
  Correct: "jwt" fires SECURITY; "login" is irrelevant noise; routes to 3.4

Teaching: always scan the entire prompt before concluding no predicate fires.
  A v6-02 corrupt model stops at "login" and short-circuits to knowledge_edge.

---

## TO-011-002 — v6-02 orchestration: "login page auth error" — auth fires, login is noise

Request: "The login page is throwing auth errors after the session token change."
Predicates (correct): ["SECURITY"] (auth + session + token all fire SECURITY; login is noise)
Orchestration: SecurityVerification34Flow
TransitionType: feature_addition

v6-02 error: model suppresses SECURITY because "login page" appears to dominate context;
  treats the auth/session/token signals as noise. Returns knowledge_edge instead of SECURITY.

---

## TO-011-003 — v6-02 orchestration: "tenant list" → AUTHORIZATION fires (not knowledge_edge)

Request: "The tenant list is not filtering by the current user's scope correctly."
Predicates (correct): ["AUTHORIZATION"] (tenant + scope fire AUTHORIZATION)
Orchestration: SecurityVerification34Flow (3.4)
TransitionType: feature_addition

v6-02 error: model sees "filtering" and "list" as the main signals; suppresses AUTHORIZATION;
  returns knowledge_edge. Correct behavior: "tenant" and "scope" fire AUTHORIZATION.

---

## TO-011-004 — Orchestration summary: complete rules for O1

Reference pair — orchestration rules summary.

Rule 1: Single sub-phase predicate(s) → single flow branch
Rule 2: Multiple predicates, same sub-phase (SECURITY+AUTHORIZATION) → single flow branch
Rule 3: Multiple predicates, different sub-phases → fan-out (multiple branches)
Rule 4: Parallel execution when branches are independent of each other's output
Rule 5: Sequential execution when one branch produces output that another branch consumes
Rule 6: TEST branches always sequential after IMPLEMENT/SECURITY branches
Rule 7: QA branch always last, sequential after all other branches
Rule 8: TransitionType: IMPLEMENT → feature_addition; TEST/DOCUMENT without IMPLEMENT → documentation
Rule 9: knowledge_edge → no flow branches; knowledge_edge handler invoked
Rule 10: DOCUMENT branches are parallel with other non-dependent branches

---

## TO-011-005 — Orchestration completeness: all branches must be registered in SelectedLogicalTransition

Reference pair — completeness requirement.

A valid SelectedLogicalTransition contains:
  Flows: all flow branches (in canonical order), each with:
    - FlowName (exact adapter name from registry)
    - SubPhase
    - TrustState
    - EvidencePredicates
    - ExecutionMode (parallel or sequential)
    - ExecutionPhase (1, 2, 3, ...)
  TransitionType: feature_addition | documentation | none
  KnowledgeEdge: true | false

Completeness rule: every predicate that fired must have a corresponding flow entry in Flows[].
  If TEST fired but generic-5.1 is not in Flows[], the transition is incomplete.
  The orchestrator validates completeness before submitting to the nine-node chain.
