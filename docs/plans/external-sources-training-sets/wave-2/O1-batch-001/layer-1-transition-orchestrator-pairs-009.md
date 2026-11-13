# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-009-001 — TransitionType selection: feature_addition wins over documentation

Request: "Build the JWT auth module and write the security specification."
Predicates: ["IMPLEMENT", "SECURITY", "DOCUMENT"]
TransitionType: feature_addition (IMPLEMENT fires — overrides DOCUMENT's documentation)
Note: IMPLEMENT + DOCUMENT → feature_addition wins (IMPLEMENT takes priority over DOCUMENT)
  If IMPLEMENT were absent: DOCUMENT alone → documentation TransitionType

---

## TO-009-002 — TransitionType selection: documentation when only DOCUMENT+TEST fire

Request: "Write tests for the auth flow and document the security handshake."
Predicates: ["SECURITY", "TEST", "DOCUMENT"]
TransitionType: documentation (TEST + DOCUMENT fire; no IMPLEMENT)
Note: No IMPLEMENT predicate → TransitionType = documentation
  "write tests" fires TEST (not IMPLEMENT); "document" fires DOCUMENT (not IMPLEMENT)

---

## TO-009-003 — TransitionType selection: none when no predicate fires

Request: "The dashboard looks strange."
Predicates: []
TransitionType: none | KnowledgeEdge: true
Note: Zero predicates → TransitionType none → no transition registered

---

## TO-009-004 — TransitionType priority table

Reference pair — TransitionType selection priority.

Priority (highest to lowest):
  1. feature_addition: IMPLEMENT fires (regardless of other predicates)
  2. documentation: TEST or DOCUMENT fires (no IMPLEMENT present)
  3. none: no predicate fires (knowledge_edge)

Examples:
  IMPLEMENT+SECURITY → feature_addition
  IMPLEMENT+TEST → feature_addition (not documentation)
  IMPLEMENT+DOCUMENT → feature_addition (not documentation)
  TEST alone → documentation
  DOCUMENT alone → documentation
  TEST+DOCUMENT → documentation
  SECURITY alone → feature_addition? (no: SECURITY alone does not fire IMPLEMENT)
    Correction: SECURITY alone without IMPLEMENT → TransitionType is feature_addition
    only if "harden"/"secure"/"add"/"build" etc. also appear as IMPLEMENT triggers
    Pure "fix the token expiry" → SECURITY fires (token); IMPLEMENT fires ("fix" as structural modification)
    → feature_addition

---

## TO-009-005 — TransitionType: refactoring detection (edge case)

Request: "Refactor the JWT token validation service to use the new OIDC (OpenID Connect) library."
Predicates: ["SECURITY"]
TransitionType consideration: "refactor" is a structural modification verb
  Standard rule: "refactor" does not appear in the IMPLEMENT lexicon explicitly
  Practical result: SECURITY fires; TransitionType is feature_addition (structural modification)
  Note: Some implementations may detect "refactor" as a distinct TransitionType;
    in O1 standard processing, feature_addition is the result when a modification verb
    co-occurs with a domain predicate
  OIDC expansion: OIDC = OpenID Connect
