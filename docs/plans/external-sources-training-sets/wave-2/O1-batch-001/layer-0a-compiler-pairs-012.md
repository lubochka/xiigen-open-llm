# Layer 0-A — Compiler Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-012-001 — Complete SECURITY predicate lexicon

A: "What tokens fire the SECURITY predicate in the O1 scenario?"
B: Predicates: ["SECURITY"] (reference pair — not a routing pair) | TransitionType: none

SECURITY lexicon (complete):
  jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac (security context),
  security, secret, certificate, tls, harden, secure, encrypt, hash, credential,
  sanitize, validate (security context)

  NOT in SECURITY lexicon: login, password, user, account, sign-in, sign-on, logout
  Boundary: "login" colloquial = no predicate; "authentication" = SECURITY
  Boundary: "token invalid" = SECURITY (token); "login failed" = no predicate (login)

---

## O1-compiler-012-002 — Complete AUTHORIZATION predicate lexicon

A: "What tokens fire the AUTHORIZATION predicate in the O1 scenario?"
B: Predicates: ["AUTHORIZATION"] (reference pair) | TransitionType: none

AUTHORIZATION lexicon (complete):
  tenant, role, permission, access control (compound), multi-tenant, rbac (authorization context),
  authorize, privilege, entitlement, scope, claims

  NOT in AUTHORIZATION lexicon: access (alone), user, group, team, workspace
  Boundary: "access control" (compound) = AUTHORIZATION; "access denied" (colloquial) = no predicate
  Boundary: "rbac" fires both SECURITY (security posture) and AUTHORIZATION (entitlement control)

---

## O1-compiler-012-003 — v6 corruption types summary for O1

A: "What are the three v6 corruption types and how do they manifest in O1?"
B: Reference pair — no predicate output | TransitionType: none

v6-01 — Predicate invention:
  Error: model invents predicates not in the lexicon
  O1 examples: inventing JWT_AUTH, TENANT_ISOLATION, MULTI_FACTOR, SESSION_BLACKLIST
  Correct behavior: only the 9 canonical predicates exist: SECURITY, AUTHORIZATION,
    API_CONTRACT, IMPLEMENT, TEST, DOCUMENT, ARCHITECTURE, QA, UI

v6-02 — Gap suppression:
  Error A: "login" → model fires SECURITY (wrong — login not in lexicon)
  Error B: "tenant" → model returns no predicate (wrong — AUTHORIZATION must fire)
  Error C: "access denied" → model fires AUTHORIZATION (wrong — colloquial, not in lexicon)
  Correct behavior: strict lexicon matching, no interpolation

v6-03 — Wrong routing:
  Error A: "test isolation" → model routes to 3.4 (wrong — should route to 5.1 via TEST)
  Error B: SECURITY+AUTHORIZATION → model produces two separate 3.4 routing decisions
    (wrong — both predicates fold into one routing decision to 3.4)
  Correct behavior: TEST routes to 5.1; SECURITY+AUTHORIZATION = single 3.4 decision

---

## O1-compiler-012-004 — TransitionType selection rules

A: "How is TransitionType selected in the O1 scenario compiler?"
B: Reference pair — no predicate output | TransitionType: none

Selection rules (priority order):
  1. feature_addition: IMPLEMENT fires (build, implement, create, scaffold, write [code], add [code])
  2. documentation: TEST or DOCUMENT fires without IMPLEMENT
  3. none: no predicate fires (knowledge_edge result)

  Note: "write tests" → TEST fires → documentation (not feature_addition)
  Note: "write [the service]" → IMPLEMENT fires → feature_addition
  Note: "refactoring" fires neither IMPLEMENT nor DOCUMENT in standard lexicon —
    routing context determines TransitionType in refactor scenarios

---

## O1-compiler-012-005 — Canonical predicate ordering rule

A: "What is the canonical predicate ordering in the O1 scenario?"
B: Reference pair — no predicate output | TransitionType: none

Canonical ordering (for output and routing):
  ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST ‖ DOCUMENT → QA

  Notes:
  - TEST and DOCUMENT are parallel (‖) — neither has priority over the other
  - QA is always absolute last
  - SECURITY and AUTHORIZATION both map to sub-phase 3.4 (same destination)
  - Ordering governs: predicate list output, routing decision order, factory selection order,
    orchestrator scheduling order
  - Predicates not present in a given prompt are simply omitted — ordering applies
    only to the predicates that did fire
