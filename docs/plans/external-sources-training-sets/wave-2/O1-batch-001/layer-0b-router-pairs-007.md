# Layer 0-B — Router Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-007-001 — knowledge_edge (no predicates) → no routing decision

Predicates: [] | Routes to: none
Routing decisions: 0 (one knowledge_edge record produced with KnowledgeEdge: true)
Source: "The thing on the login page is broken again."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Note: The router still produces one array entry; it is the synthetic knowledge_edge record.
  The orchestrator reads this record and routes to the knowledge-edge synthetic flow path.

---

## O1-router-007-002 — knowledge_edge from "login page error"

Predicates: [] | Routes to: none
Routing decisions: 0
Source: "The login page keeps showing an error for existing users."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Explanation: "login" + "page" + "error" — none of these tokens are in any predicate lexicon

---

## O1-router-007-003 — knowledge_edge from "access denied" colloquial

Predicates: [] | Routes to: none
Routing decisions: 0
Source: "Some users are getting access denied errors in production."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Explanation: "access denied" is a colloquial runtime error message, not "access control"
  "access" alone is not in the AUTHORIZATION lexicon

---

## O1-router-007-004 — knowledge_edge from "login reset workflow"

Predicates: [] | Routes to: none
Routing decisions: 0
Source: "Users need a login reset workflow for forgotten passwords."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Explanation: "login" not in SECURITY; "reset" not in SECURITY; "passwords" ≠ "credential"
  ("credential" IS in SECURITY; "passwords" alone is NOT the lexicon token)

---

## O1-router-007-005 — knowledge_edge from short unroutable prompt

Predicates: [] | Routes to: none
Routing decisions: 0
Source: "It stopped working after the last deploy."
KnowledgeEdge: true | FailureClassification: "no_predicate_match"
Explanation: Zero tokens match any predicate lexicon
  "working", "deploy", "stopped" are operational/devops terms — no predicate coverage
  in the O1 scenario predicate set
