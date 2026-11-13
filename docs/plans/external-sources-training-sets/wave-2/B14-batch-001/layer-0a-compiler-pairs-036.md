# Layer 0-A — Compiler Pairs (036)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-036-001 — Complete predicate reference: AUTHORIZATION triggers

AUTHORIZATION fires on ALL of the following vocabulary:
  Action verbs: authorize, grant, revoke, permit (in access policy context), restrict (in access policy context)
  Role/policy nouns: IAM role, RBAC role, ClusterRole, RoleBinding, ClusterRoleBinding, service account (in binding context), trust policy, permission boundary, access policy, policy document
  Compound phrases: "assume role", "role binding", "service account binding", "IAM trust policy", "permission grant", "access grant"

---

## B14-compiler-036-002 — Complete predicate reference: API_CONTRACT triggers

API_CONTRACT fires on ALL of the following vocabulary:
  Action verbs: define (in contract/endpoint context), specify (API spec context), contract, expose (service endpoint), publish (API)
  Contract nouns: API contract, endpoint, interface (API), schema (API), OpenAPI, Swagger, contract, service interface, API spec, proto (in gRPC context), IDL (Interface Definition Language)
  Compound phrases: "API contract", "service contract", "endpoint definition", "interface specification", "breaking change policy"

---

## B14-compiler-036-003 — Complete predicate reference: DOCUMENT triggers

DOCUMENT fires on ALL of the following vocabulary:
  Action verbs: document, record, describe (in documentation context), write up, write (in documentation context), capture (in doc context)
  Documentary nouns: ADR (Architecture Decision Record), runbook, playbook (documentation context), documentation, README, wiki, guide, reference, changelog
  Compound phrases: "write the ADR", "document the architecture", "record the decision", "write the runbook"

---

## B14-compiler-036-004 — Disambiguation: IMPLEMENT vs DOCUMENT for "write"

"write" verb: fires IMPLEMENT when followed by a code artifact noun (module, Dockerfile, script, chart, playbook in IaC context, configuration file, manifest, template)
"write" verb: fires DOCUMENT when followed by a documentary artifact noun (ADR, runbook, documentation, guide, README)
Neither: "write" alone without a following noun is ambiguous → resolved by the noun that follows.
  "Write the Terraform module" → IMPLEMENT | "Write the ADR" → DOCUMENT | "Write the playbook" → context-dependent

---

## B14-compiler-036-005 — Complete rule: multi-predicate flow with all 6 B14 predicates

When a request fires all 6 B14 predicates (ARCHITECTURE, IMPLEMENT, SECURITY, AUTHORIZATION, API_CONTRACT, DOCUMENT):
  FlowCount: 6 concurrent flow instances
  TransitionType: feature_addition (IMPLEMENT present)
  Sequential order: ARCHITECTURE (3.5) → API_CONTRACT (2.1) → SECURITY (6.3) → AUTHORIZATION (phase TBD) → IMPLEMENT (4.1) → DOCUMENT (6.2)
  All 6 flows: TrustState deterministic_generic
