# Layer 0-D — Flow Factory Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-010-001 — v6-01 corruption: inventing "JwtAuthFlow" (not in registry)

Predicates: ["SECURITY", "AUTHORIZATION"]
Sub-phase: 3.4

v6-01 corrupt factory selection:
  Wrong: Factory: JwtAuthFlow | TrustState: trusted_learned

Correct factory selection:
  Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned

Error: v6-01 corrupt model invents a flow name not in the factory registry.
  "JwtAuthFlow" does not exist. The factory registry for 3.4 contains only:
  SecurityVerification34FlowAdapter (for O1 scenario).
  Any other name is an invented predicate / invented factory (v6-01 error).

---

## FF-010-002 — v6-01 corruption: inventing "TenantIsolationFlow" (not in registry)

Predicates: ["AUTHORIZATION"]
Sub-phase: 3.4

v6-01 corrupt factory selection:
  Wrong: Factory: TenantIsolationFlow | TrustState: trusted_learned

Correct factory selection:
  Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned

Error: "TenantIsolationFlow" is invented. The 3.4 factory for O1 is always
  SecurityVerification34FlowAdapter. The name is not "TenantFlow", "AuthFlow",
  "TenantIsolationFlow", or any other variation.

---

## FF-010-003 — v6-01 corruption: inventing "MultiFactorFlow"

Predicates: ["SECURITY"]
Sub-phase: 3.4

v6-01 corrupt factory selection:
  Wrong: Factory: MultiFactorFlow | TrustState: trusted_learned
  (corrupt model creates a flow specifically for MFA signal)

Correct factory selection:
  Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned

Error: The factory for 3.4 is SecurityVerification34FlowAdapter for all SECURITY-predicate
  prompts, regardless of which SECURITY token fired (jwt, mfa, oauth, etc.).
  MFA = Multi-Factor Authentication — the specific token does not change the factory name.

---

## FF-010-004 — v6-01 corruption: inventing predicates in the factory selection step

Source: "Build JWT tenant-scoped authentication."
Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] (correct)

v6-01 corrupt compilation:
  Wrong predicates: ["IMPLEMENT", "JWT_AUTH", "TENANT_ISOLATION"] (invented predicates)
  Wrong factories: JwtAuthFlowAdapter (for JWT_AUTH), TenantIsolationFlowAdapter (for TENANT_ISOLATION)

Correct:
  Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"]
  Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
  Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned

Error: v6-01 invents both the predicate names AND corresponding factory names. Neither
  JWT_AUTH nor TENANT_ISOLATION are valid predicates in the O1 predicate lexicon.

---

## FF-010-005 — v6-01 detection: valid factory names for O1

Reference pair — complete list of valid factory names for O1.

Valid factory names in O1 scenario:
  3.4: SecurityVerification34FlowAdapter (trusted_learned)
  3.3: ApiContract33FlowAdapter (probationary_accepted or deterministic_generic)
  5.1: UnitTestDesign51FlowAdapter (probationary_rejected — generic fallback used)
  2.1: RequirementsAuthoringFlow21 (deterministic_generic)
  4.1: ImplementationFlow41 (deterministic_generic)
  7.x: QAGatingFlow73 (deterministic_generic)
  3.x: ArchitectureFlow (deterministic_generic, scenario-specific name)

Any other factory name appearing in O1 output is a v6-01 invented factory error.
