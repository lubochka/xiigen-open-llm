# Layer 0-A — Compiler Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1b-0A-012-001 — Predicate rule summary: SECURITY lexicon

A: "Which terms fire the SECURITY predicate for O1b?"

B: SECURITY lexicon (O1b relevant subset): jwt, jws, jwe, authentication, auth, oauth, oidc, openid, token, session, asvs, mfa, 2fa, rbac, security, credential, password, hash, encrypt, tls, ssl, certificate, signing, signature, secret, key, cipher. NOT in lexicon: "login", "access denied", "portal is down".

---

## O1b-0A-012-002 — Predicate rule summary: AUTHORIZATION lexicon

A: "Which terms fire the AUTHORIZATION predicate for O1b?"

B: AUTHORIZATION lexicon (O1b relevant subset): authorize, authorization, tenant, permission, role, access-control, scope, privilege, claim, policy, acl, entitlement, rbac, principal, grant, revoke. Note: "service account" → AUTHORIZATION (principal/account with scope). "access denied" → colloquial, NOT AUTHORIZATION.

---

## O1b-0A-012-003 — Predicate rule summary: v6 corruption types

A: "What are the three v6 corruption types for O1b?"

B: v6-01: Predicate invention — inventing predicates like OAUTH_FLOW, CLIENT_CREDS, SERVICE_ACCOUNT that do not exist in the lexicon. Correction: use SECURITY or AUTHORIZATION. v6-02: Gap suppression — failing to detect a valid lexicon keyword (e.g., treating "scope" as no predicate, or treating "client portal" as SECURITY). v6-03: Wrong routing — routing "service account" to 4.1 instead of 3.4, or producing two 3.4 decisions when SECURITY+AUTHORIZATION should dedup to one.

---

## O1b-0A-012-004 — Predicate rule summary: TransitionType rules

A: "How is TransitionType determined?"

B: Rules: (1) IMPLEMENT present → feature_addition. (2) "refactor" verb present → refactoring (overrides feature_addition). (3) TEST or DOCUMENT present WITHOUT IMPLEMENT → documentation. (4) QA alone → no contribution (QA does not set TransitionType). (5) No predicates → knowledge_edge.

---

## O1b-0A-012-005 — Predicate rule summary: canonical ordering

A: "What is the canonical predicate ordering for O1b?"

B: Canonical ordering: ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST ‖ DOCUMENT (parallel, either order) → QA (absolute last). This ordering determines the sequence in which predicates are listed in B outputs, not the priority of routing decisions.
