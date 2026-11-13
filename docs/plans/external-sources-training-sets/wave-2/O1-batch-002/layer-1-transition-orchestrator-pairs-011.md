# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-011-001 — v6-02: "client portal" → knowledge_edge (not SECURITY)

A: "the client portal is failing for enterprise tenants"

CORRUPTED ORCHESTRATION (v6-02): OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow" }] }
   ERROR: "client portal" is colloquial. "enterprise" is not a predicate keyword. "tenants" in a colloquial context about portal failure does not fire AUTHORIZATION. Zero predicates → knowledge_edge → no flows.

CORRECT ORCHESTRATION: OrchestrationPlan: { flows: [], execution: "none", reason: "knowledge_edge" }

---

## O1b-L1-011-002 — v6-02: "portal issues" suppressing correct detection

A: "portal issues are affecting tenant API access"

CORRUPTED ORCHESTRATION (v6-02): Predicates: [] (misses "tenant" and "API")
   ERROR: "tenant" fires AUTHORIZATION; "API" fires API_CONTRACT. These keywords ARE present and should fire predicates. The v6-02 error suppresses valid predicate detection.

CORRECT ORCHESTRATION: Predicates: ["AUTHORIZATION", "API_CONTRACT"] | RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-L1-011-003 — v6-02: gap suppression of "scope" keyword

A: "scope issues when calling tenant APIs from the service account"

CORRUPTED ORCHESTRATION (v6-02): Predicates: [] (scope not caught)
   ERROR: "scope" fires AUTHORIZATION; "tenant" fires AUTHORIZATION; "API" fires API_CONTRACT; "service account" fires AUTHORIZATION. Multiple predicates present. All suppressed in this v6-02 example.

CORRECT ORCHESTRATION: Predicates: ["AUTHORIZATION", "API_CONTRACT"] | RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

---

## O1b-L1-011-004 — v6-02 detection pattern

A: How to detect v6-02 gap suppression in orchestration output

B: v6-02 indicators: (1) OrchestrationPlan has zero flows for a prompt containing "oauth", "token", "scope", "tenant", "authorization", "authentication" or other lexicon words. (2) knowledge_edge classification for a prompt that contains at least one lexicon keyword. (3) SECURITY or AUTHORIZATION predicates missing when "oauth", "scope", "tenant", "credential" are present.

---

## O1b-L1-011-005 — v6-02 vs correct for "client portal" variants

A: Test "client portal X" against v6-02 detection

B: "client portal is down" → knowledge_edge CORRECT (no lexicon words).
   "client portal oauth is broken" → SECURITY fires ("oauth") → 3.4 routing CORRECT.
   "client portal access token expired" → SECURITY fires ("token") → 3.4 routing CORRECT.
   Rule: add any lexicon word to "client portal" and it becomes routable. The portal phrase itself never fires; the lexicon keyword does.
