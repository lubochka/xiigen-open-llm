# Held-Out and Corruption Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Complete held-out and corruption summary for O1

---

## Summary 012-S001 — Held-out pairs inventory (O1 expansion wave-2)

| File | Pair ID | Prompt summary | Expected predicates | TransitionType |
|---|---|---|---|---|
| 002 | H001 | tenant-scoped JWT validation | IMPLEMENT, SECURITY, AUTHORIZATION | feature_addition |
| 002 | H002 | tenant-aware JWT role binding | IMPLEMENT, SECURITY, AUTHORIZATION | feature_addition |
| 003 | H001 | OAuth2 client credentials flow | IMPLEMENT, SECURITY, AUTHORIZATION, API_CONTRACT | feature_addition |
| 003 | H002 | OIDC discovery endpoint | IMPLEMENT, SECURITY, AUTHORIZATION, API_CONTRACT | feature_addition |
| 004 | H001 | unit tests for token expiry | TEST, SECURITY | documentation |
| 004 | H002 | assert refresh token rotation | TEST, SECURITY | documentation |
| 005 | H001 | document TLS security handshake | DOCUMENT, SECURITY | documentation |
| 005 | H002 | write OAuth2 PKCE spec | DOCUMENT, SECURITY | documentation |
| 006 | H001 | build permission assignment service | IMPLEMENT, AUTHORIZATION | feature_addition |
| 006 | H002 | build entitlement provisioning workflow | IMPLEMENT, AUTHORIZATION | feature_addition |
| 007 | H001 | secure API key rotation endpoint | IMPLEMENT, SECURITY, API_CONTRACT | feature_addition |
| 007 | H002 | add TLS certificate pinning to webhook | IMPLEMENT, SECURITY, AUTHORIZATION, API_CONTRACT | feature_addition |
| 008 | H001 | add MFA to admin workflow | IMPLEMENT, SECURITY, AUTHORIZATION | feature_addition |
| 008 | H002 | enforce MFA on privilege escalation | IMPLEMENT, SECURITY, AUTHORIZATION | feature_addition |
| 009 | H001 | write API contract for tenant management | DOCUMENT, API_CONTRACT, AUTHORIZATION | documentation |
| 009 | H002 | specify permission API schema | DOCUMENT, API_CONTRACT, AUTHORIZATION | documentation |
| 010 | H001 | refactor token refresh service | SECURITY | feature_addition |
| 010 | H002 | refactor claims extraction middleware | SECURITY, AUTHORIZATION | feature_addition |
| 011 | H001 | validate session tokens against blacklist | IMPLEMENT, SECURITY | feature_addition |
| 011 | H002 | check token against tenant revocation list | SECURITY, AUTHORIZATION | feature_addition |

Total held-out pairs: 20

---

## Summary 012-S002 — Corruption pairs inventory (O1 expansion wave-2)

| File | Pair ID | Error type | Corrupt behavior | Correct behavior |
|---|---|---|---|---|
| 002 | C001 | v6-01 | invents JWT_AUTH, TENANT_ISOLATION | IMPLEMENT, SECURITY, AUTHORIZATION |
| 002 | C002 | v6-01 | invents TenantJwtFlowAdapter factory | SecurityVerification34FlowAdapter |
| 003 | C001 | v6-02 | suppresses SECURITY on "login reset" + credential | IMPLEMENT, SECURITY |
| 003 | C002 | v6-02 | fires SECURITY on "login" alone | no predicate → knowledge_edge |
| 004 | C001 | v6-03 | "test isolation" routed to 3.4 only | 3.4 + 5.1 (TEST → 5.1) |
| 004 | C002 | v6-03 | TEST branch before SECURITY in orchestration | IMPL → SEC → TEST sequence |
| 005 | C001 | v6-01 | invents TENANT_ISOLATION | DOCUMENT, AUTHORIZATION |
| 005 | C002 | v6-01 | invents SECURITY_SPEC, JWT_DOC | DOCUMENT, SECURITY |
| 006 | C001 | v6-02 | fires SECURITY on "access denied" | no predicate → knowledge_edge |
| 006 | C002 | v6-02 | suppresses AUTHORIZATION on "permission denied" | AUTHORIZATION fires on "permission" |
| 007 | C001 | v6-03 | SECURITY+AUTHORIZATION not deduplicated (4 decisions) | 3 decisions (deduplicated) |
| 007 | C002 | v6-03 | AUTHORIZATION → 4.5 (wrong sub-phase) | AUTHORIZATION → 3.4 |
| 008 | C001 | v6-01 | invents MULTI_FACTOR | IMPLEMENT, SECURITY, AUTHORIZATION |
| 008 | C002 | v6-01 | invents MFA_ENFORCEMENT, PRIVILEGE_ESCALATION_GUARD | IMPLEMENT, SECURITY, AUTHORIZATION |
| 009 | C001 | v6-02 | suppresses API_CONTRACT when AUTHORIZATION present | API_CONTRACT fires independently |
| 009 | C002 | v6-02 | "contract" not fired as API_CONTRACT | "contract" fires API_CONTRACT |
| 010 | C001 | v6-03 | refactor → documentation TransitionType | refactor → feature_addition |
| 010 | C002 | v6-03 | IMPLEMENT+TEST → documentation | IMPLEMENT+TEST → feature_addition |
| 011 | C001 | v6-01 | invents SESSION_BLACKLIST | IMPLEMENT, SECURITY |
| 011 | C002 | v6-01 | invents TOKEN_REVOCATION | SECURITY (canonical) |

Total corruption pairs: 20
v6-01 coverage: 8 corruption pairs
v6-02 coverage: 7 corruption pairs
v6-03 coverage: 5 corruption pairs
All three v6 corruption types: COVERED
