# Layer 0-D — Flow Factory Pairs (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-004-001 — SECURITY + AUTHORIZATION → two separate flows both in sub-phase 3.4

**Scenario:** OAuthIdentityConnector (Auth0/Okta) — four predicates including SECURITY and AUTHORIZATION.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
ConnectorName: OAuthIdentityConnector
```

**Factory selection for 3.4 predicates:**
- SECURITY → SecurityAuditFlow34 (sub-phase 3.4)
- AUTHORIZATION → AuthorizationFlow34 (sub-phase 3.4)

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { subPhase: "3.3", flow: "APIContractFlow33" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34" },
  { subPhase: "3.4", flow: "AuthorizationFlow34" }
]
DecisionCount: 4
TransitionType: feature_addition
```

---

## B17-0D-004-002 — SecurityAuditFlow34 vs AuthorizationFlow34

**SecurityAuditFlow34:**
- Handles: token security, credential protection, signature validation
- Typical output: security review checklist, token storage guidance, encryption recommendations
- Triggered by: SECURITY predicate

**AuthorizationFlow34:**
- Handles: OAuth 2.0 protocol implementation, code exchange, PKCE, scope management
- Typical output: OAuth flow diagram, token endpoint configuration, scope definitions
- Triggered by: AUTHORIZATION predicate

**Both are sub-phase 3.4 flows, but they are separate flows with separate outputs.**

---

## B17-0D-004-003 — Factory selects both 3.4 flows when both predicates fire

**Factory rule for 3.4:** When both SECURITY and AUTHORIZATION predicates are in CompiledPredicateIds, the factory selects BOTH SecurityAuditFlow34 AND AuthorizationFlow34.

**This is not a conflict.** The factory treats each predicate as an independent flow selection instruction.

**Factory ordering within 3.4:**
- SecurityAuditFlow34 typically runs first (baseline security requirements)
- AuthorizationFlow34 runs after (authorization layer sits on top of security layer)

---

## B17-0D-004-004 — Minimal factory for SECURITY only vs AUTHORIZATION only

**SECURITY only:**
```
{ subPhase: "3.4", flow: "SecurityAuditFlow34" }
```
DecisionCount contribution: 1 (for SECURITY)

**AUTHORIZATION only:**
```
{ subPhase: "3.4", flow: "AuthorizationFlow34" }
```
DecisionCount contribution: 1 (for AUTHORIZATION)

**Both together:**
```
{ subPhase: "3.4", flow: "SecurityAuditFlow34" },
{ subPhase: "3.4", flow: "AuthorizationFlow34" }
```
DecisionCount contribution: 2
