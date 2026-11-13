# Layer 0-C — Knowledge Edge Pairs (054)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-054-001 — OPAGatekeeper: gap vocabulary — OPA runtime operations

Confirmed gap vocabulary for OPA (Open Policy Agent) runtime operations (zero predicates):
  Action verbs: install, upgrade, enable, disable, audit (OPA audit — operational), check (policy result), query (Rego query — operational), test (Rego unit test — could fire TEST)
  Exception: "write Rego unit tests" → fires TEST (test code artifact being created)
  Runtime nouns: Rego query result, OPA bundle, audit log (OPA), violation count

---

## B14-edge-054-002 — OPAGatekeeper: disambiguation — "policy" noun routes differently

"policy" noun disambiguation in OPA context:
  "implement the OPA policy module" → IMPLEMENT (policy module = IaC code artifact)
  "enforce OPA security policies" → SECURITY (enforce + security context)
  "restrict namespace access via OPA policy" → AUTHORIZATION (restrict in access context)
  "design the OPA policy framework" → ARCHITECTURE (design + framework)
  "document the OPA policy ADR" → DOCUMENT (document + ADR)
  Same "policy" noun → 5 different predicates depending on verb and context.

---

## B14-edge-054-003 — OPAGatekeeper: SECURITY boundary — constraint enforcement compound phrases

SECURITY fires for:
  "enforce OPA Gatekeeper security constraints" → SECURITY ("enforce" + security context)
  "harden cluster policies with deny-all baseline via OPA" → SECURITY ("harden")
  "mTLS between OPA webhook and API server" → SECURITY ("mTLS")
  "audit OPA violations" → gap (audit = operational review)

---

## B14-edge-054-004 — OPAGatekeeper: IMPLEMENT boundary — Rego code is an IaC artifact

Rego policy modules and ConstraintTemplates are IaC code artifacts:
  "write the Rego policy module" → IMPLEMENT ("write" + "module")
  "scaffold the Gatekeeper ConstraintTemplate" → IMPLEMENT ("scaffold" + "ConstraintTemplate")
  "implement the OPA constraint policy file" → IMPLEMENT ("implement" + "policy file" = IaC artifact)
  "query the Rego engine" → gap (runtime query, not code artifact creation)
