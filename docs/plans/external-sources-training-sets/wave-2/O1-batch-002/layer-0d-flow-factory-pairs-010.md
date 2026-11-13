# Layer 0-D — Flow Factory Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-010-001 — v6-01: inventing OAuthFlow factory (wrong)

A: RoutingDecision: { sub_phase: "3.4", predicates: ["SECURITY"] }

CORRUPTED FACTORY (v6-01): FlowSelection: { adapter: "OAuthFlowAdapter", state: "trusted_learned" }
   ERROR: "OAuthFlowAdapter" is an invented adapter name. There is no OAuthFlowAdapter in the system. The correct adapter for 3.4 is SecurityVerification34FlowAdapter.

CORRECT FACTORY: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned", flow: "SecurityVerification34Flow" }

---

## O1b-0D-010-002 — v6-01: inventing ClientCredentialsFlowAdapter (wrong)

A: RoutingDecision: { sub_phase: "3.4", predicates: ["SECURITY", "AUTHORIZATION"] }

CORRUPTED FACTORY (v6-01): FlowSelection: { adapter: "ClientCredentialsFlowAdapter", state: "trusted_learned" }
   ERROR: Invented adapter. "ClientCredentials" is a scenario detail, not an adapter name.

CORRECT FACTORY: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }

---

## O1b-0D-010-003 — v6-01: inventing TenantScopeFlowAdapter (wrong)

A: RoutingDecision: { sub_phase: "3.4", predicates: ["AUTHORIZATION"] }

CORRUPTED FACTORY (v6-01): FlowSelection: { adapter: "TenantScopeFlowAdapter", state: "trusted_learned" }
   ERROR: Invented adapter. Scenario-specific terms must never become adapter names.

CORRECT FACTORY: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }

---

## O1b-0D-010-004 — Canonical adapter names for O1b

A: What are the canonical adapter names for all sub-phases in O1b?

B: CANONICAL LIST (must use exactly):
   - 3.4: SecurityVerification34FlowAdapter
   - 3.3: ApiContract33FlowAdapter
   - 4.1: UniversalNineNodeDevelopmentFlow
   - 5.1: UnitTestingFlow51 (with fallback: GenericFallbackFlow)
   - 2.1: DocumentationFlow21
   Any other name is a v6-01 corruption.

---

## O1b-0D-010-005 — v6-01 detection: how to spot invented adapters

A: How to detect v6-01 adapter invention

B: Indicators of v6-01: adapter name contains scenario-specific terms (OAuth, ClientCreds, Tenant, Scope). Correct adapters use generic sub-phase numbered names (SecurityVerification34, ApiContract33, UniversalNineNodeDevelopment). If the adapter name describes the scenario rather than the sub-phase, it is invented.
