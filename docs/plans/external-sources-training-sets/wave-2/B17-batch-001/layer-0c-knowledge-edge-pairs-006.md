# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-006-001 — SECURITY + AUTHORIZATION both route to 3.4: not a conflict

**Rule 9:** SECURITY and AUTHORIZATION both route to sub-phase 3.4. This is not an error or conflict — it is the correct behavior.

**Sub-phase 3.4** (Security and Compliance) handles both security audit work and authorization protocol work. These are conceptually related but operationally distinct flows within the same sub-phase zone.

**Two predicates, one sub-phase, two routing decisions:**
- SECURITY → 3.4 via SecurityAuditFlow34
- AUTHORIZATION → 3.4 via AuthorizationFlow34
- DecisionCount += 2 (one for each predicate)
- Sub-phase 3.4 appears once in the routing output (destination, not counted twice)

---

## B17-0C-006-002 — DecisionCount counts predicates fired, NOT unique sub-phases

**Scenario:** ["SECURITY", "AUTHORIZATION"]
- DecisionCount: 2
- Unique sub-phases: 1 (3.4)

**Scenario:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
- DecisionCount: 4
- Unique sub-phases: 3 (4.3, 3.3, 3.4)

**Formula:** DecisionCount = |CompiledPredicateIds| always.
**Formula:** UniqueSubPhases = |{distinct sub-phase values in routing}| always.

These are reported separately and may differ. Neither is wrong when they differ.

---

## B17-0C-006-003 — Why SecurityAuditFlow34 and AuthorizationFlow34 are separate

**SecurityAuditFlow34 handles:**
- Token security (secure token storage, token validation)
- Webhook signature verification
- Credential security
- Encryption at rest/transit

**AuthorizationFlow34 handles:**
- OAuth (Open Authorization) protocol implementation
- OAuth 2.0 code exchange
- PKCE (Proof Key for Code Exchange) flow
- JWT (JSON Web Token) verification
- Scope and permission management

**They share sub-phase 3.4 because both are security-domain work, but they address different technical concerns.**

---

## B17-0C-006-004 — Common input patterns that fire both SECURITY and AUTHORIZATION

| Input fragment | SECURITY | AUTHORIZATION |
|---|---|---|
| "OAuth token refresh" | Yes (token) | Yes (OAuth) |
| "secure OAuth client credentials" | Yes (secure) | Yes (OAuth) |
| "implement OAuth and secure the token store" | Yes (secure, token) | Yes (OAuth) |
| "OAuth authentication flow" | Yes (authentication) | Yes (OAuth) |
| "implement token refresh after OAuth exchange" | Yes (token) | Yes (OAuth) |

**Pattern:** Any OAuth implementation that also mentions tokens or security terms will fire both SECURITY and AUTHORIZATION.
