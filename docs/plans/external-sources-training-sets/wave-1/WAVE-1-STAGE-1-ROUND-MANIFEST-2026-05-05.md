# Round Manifest — Wave 1, Stage 1 Complete
# Date: 2026-05-05
# Pipeline: EXTERNAL-SOURCES-FULL-PIPELINE-PLAN-v1.md
# Stage: 1 of 7 — Source document translation to experiment design

---

## Scope

Wave 1, Sub-phase 3.4 (Security and Authorization Model).
Priority Group 1 sources: OWASP-ASVS, NIST-SSDF, CWE-TOP25, IETF-RFCS.
Batch: CB4 — Pair ID prefix: CB4-3.4-

---

## Stage 0 Gate Status — All Four Sources

| Source | License class | Sub-phase | Stage 0 gate |
|--------|--------------|-----------|-------------|
| OWASP-ASVS | ATTRIBUTION_SHAREALIKE (CC BY-SA 4.0) | 3.4 | PASS |
| NIST-SSDF | PUBLIC_DOMAIN_US | 3.4 | PASS |
| CWE-TOP25 | FREE_COMMERCIAL_USE_WITH_NOTICE | 3.4 | PASS |
| IETF-RFCS | IETF_TRUST_PROVISIONS | 3.4 | PASS |

Note: OWASP-ASVS version used is 5.0.0 (released May 2025), not 4.0.3.
Chapter numbering has changed: V6=Authentication, V7=Session Management,
V8=Authorization, V9=Self-contained Tokens, V10=OAuth and OIDC.

---

## Stage 1 Output — Files Produced

### OWASP-ASVS (5 documents)
- wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-SOURCE-BINDING-v1.md
- wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-BINDING-FAILURE-TAXONOMY-v1.md
- wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-EXPECTED-ORCHESTRATION-TRACE-v1.md
- wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-HUMAN-RESULT-STANDARDS-v1.md
- wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-EXECUTOR-CONTRACT-v1.md

### NIST-SSDF (5 documents)
- wave-1/NIST-SSDF/stage-1/NIST-SSDF-SOURCE-BINDING-v1.md
- wave-1/NIST-SSDF/stage-1/NIST-SSDF-BINDING-FAILURE-TAXONOMY-v1.md
- wave-1/NIST-SSDF/stage-1/NIST-SSDF-EXPECTED-ORCHESTRATION-TRACE-v1.md
- wave-1/NIST-SSDF/stage-1/NIST-SSDF-HUMAN-RESULT-STANDARDS-v1.md
- wave-1/NIST-SSDF/stage-1/NIST-SSDF-EXECUTOR-CONTRACT-v1.md

### CWE-TOP25 (5 documents)
- wave-1/CWE-TOP25/stage-1/CWE-TOP25-SOURCE-BINDING-v1.md
- wave-1/CWE-TOP25/stage-1/CWE-TOP25-BINDING-FAILURE-TAXONOMY-v1.md
- wave-1/CWE-TOP25/stage-1/CWE-TOP25-EXPECTED-ORCHESTRATION-TRACE-v1.md
- wave-1/CWE-TOP25/stage-1/CWE-TOP25-HUMAN-RESULT-STANDARDS-v1.md
- wave-1/CWE-TOP25/stage-1/CWE-TOP25-EXECUTOR-CONTRACT-v1.md

### IETF-RFCS (5 documents)
- wave-1/IETF-RFCS/stage-1/IETF-RFCS-SOURCE-BINDING-v1.md
- wave-1/IETF-RFCS/stage-1/IETF-RFCS-BINDING-FAILURE-TAXONOMY-v1.md
- wave-1/IETF-RFCS/stage-1/IETF-RFCS-EXPECTED-ORCHESTRATION-TRACE-v1.md
- wave-1/IETF-RFCS/stage-1/IETF-RFCS-HUMAN-RESULT-STANDARDS-v1.md
- wave-1/IETF-RFCS/stage-1/IETF-RFCS-EXECUTOR-CONTRACT-v1.md

---

## Stage 1 Gate Verification — All Four Sources

Gate requirements per EXTERNAL-SOURCES-FULL-PIPELINE-PLAN-v1.md:
- All five oracle documents produced .............. PASS (5 × 4 = 20 documents)
- Acceptance criteria measurable .................. PASS (all oracle docs include AC sections)
- Failure taxonomy ≥ 3 distinct gap classifications PASS (each taxonomy has 5 failure modes)
- Central question has positive and negative halves PASS (all four central questions include both)

Stage 1 gate: **CLOSED for all four sources.**

---

## Central Questions Summary

**OWASP-ASVS:** Does the orchestrator correctly enforce ASVS 5.0.0 requirements
as arbiter rules (PKCE, JWT, session) — or produce coverage illusions, wrong
sub-phase assignments, or claim boundary failures?

**NIST-SSDF:** Does the orchestrator correctly distinguish SSDF process evidence
(design artifacts) from ASVS runtime compliance evidence — or accept runtime
test results as proof of design-phase practices?

**CWE-TOP25:** Does the orchestrator correctly identify CWE-862 (absent
authorization check) vs. CWE-269 (present check with wrong scope) — or confuse
the weakness categories and assign wrong severity?

**IETF-RFCS:** Does the orchestrator apply RFC MUST vs. MAY normative keyword
calibration to produce correct verdict severity — or treat all RFC clauses
uniformly or miss trust boundary violations in symmetric key deployments?

---

## Failure Mode Summary Across All Sources

| FM code | Source | Type | Primary node | Primary scenario |
|---------|--------|------|-------------|-----------------|
| FM-1: COVERAGE_ILLUSION | OWASP-ASVS | Judgment | Node 8 | S1 |
| FM-2: WRONG_SUB_PHASE | OWASP-ASVS | Contract | Node 1, 2 | All |
| FM-3: CLAIM_BOUNDARY | OWASP-ASVS | Judgment | Node 8 | S2, S3 |
| FM-4: ARBITER_SELECTION_GAP | OWASP-ASVS | Contract | Node 7 | S2 |
| FM-5: FORMATTER_RESOLUTION | OWASP-ASVS | Contract | Node 9 | S2 |
| FM-SSDF-1: EVIDENCE_TYPE_CONFLATION | NIST-SSDF | Judgment | Node 8 | S2 |
| FM-SSDF-2: PRACTICE_FAMILY_CONFUSION | NIST-SSDF | Contract | Node 4, 8 | All |
| FM-SSDF-3: TIMING_GAP_IGNORED | NIST-SSDF | Judgment | Node 8 | S3 |
| FM-SSDF-4: SSDF_ASVS_BOUNDARY | NIST-SSDF | Judgment | Node 7, 8 | S2 |
| FM-SSDF-5: ARTIFACT_NAME_REQUIRED | NIST-SSDF | Judgment | Node 8 | S2 |
| FM-CWE-1: CWE_ID_ABSENT | CWE-TOP25 | Structural | Node 8 | All |
| FM-CWE-2: CWE_CATEGORY_CONFUSION | CWE-TOP25 | Judgment | Node 8 | S3 |
| FM-CWE-3: SEVERITY_UNDER_ASSIGNMENT | CWE-TOP25 | Judgment | Node 8 | S2 |
| FM-CWE-4: IDOR_PATTERN_MISSED | CWE-TOP25 | Judgment | Node 8 | S2 |
| FM-CWE-5: DESCRIPTION_WITHOUT_CHECK | CWE-TOP25 | Structural | Node 8 | All |
| FM-RFC-1: RFC_SURFACE_CLAIM | IETF-RFCS | Judgment | Node 8 | All |
| FM-RFC-2: RFC_SECTION_ABSENT | IETF-RFCS | Structural | Node 8 | All |
| FM-RFC-3: MUST_MAY_CONFUSION | IETF-RFCS | Judgment | Node 8 | S2, S3 |
| FM-RFC-4: TRUST_BOUNDARY_MISSED | IETF-RFCS | Judgment | Node 8 | S2 |
| FM-RFC-5: PROTOCOL_VERSION | IETF-RFCS | Judgment | Node 8 | All |

Total failure modes defined: 20 across 4 sources.

---

## Stage 2 Status — COMPLETE (2026-05-05)

Evidence files produced for all four sources (all structural gaps, baseline 1160):
- wave-1/OWASP-ASVS/stage-2/OWASP-ASVS-EXPERIMENT-EVIDENCE-v1.md
- wave-1/NIST-SSDF/stage-2/NIST-SSDF-EXPERIMENT-EVIDENCE-v1.md
- wave-1/CWE-TOP25/stage-2/CWE-TOP25-EXPERIMENT-EVIDENCE-v1.md
- wave-1/IETF-RFCS/stage-2/IETF-RFCS-EXPERIMENT-EVIDENCE-v1.md

Finding: no security-verification-3.4 flow registered; all nine nodes produce no output
for all three scenarios across all four sources. All gaps are structural.

## Stage 3 Status — COMPLETE (Action 2) + IN PROGRESS (Action 3, 2026-05-05)

Gap closure pass (Action 2) produced 120 pair instances across 9 node files:
- wave-1/CB4-stage-3-pairs/node-01-domain-splitter-set.md
- wave-1/CB4-stage-3-pairs/node-02-domain-translator-set.md
- wave-1/CB4-stage-3-pairs/node-03-scheme-planner-set.md
- wave-1/CB4-stage-3-pairs/node-04-domain-processor-set.md
- wave-1/CB4-stage-3-pairs/node-05-applier-set.md
- wave-1/CB4-stage-3-pairs/node-06-result-translator-set.md
- wave-1/CB4-stage-3-pairs/node-07-arbiter-selector-set.md
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set.md
- wave-1/CB4-stage-3-pairs/node-09-formatter-set.md

Stage 3 gate document: wave-1/CB4-stage-3-pairs/CB4-STAGE-3-GATE-v1.md

Action 3 (coverage extension) — IN PROGRESS, 2732 additional instances produced:
Sequences CB4-3.4-061 through CB4-3.4-1426 (1366 additional base pairs × 2 phrasings).
Total CB4 dataset: 2852 instances (1426 base pairs).

Action 3 files produced (as of 2026-05-05):
Node extensions:
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext1.md (061–072, 24 instances)
- wave-1/CB4-stage-3-pairs/node-07-arbiter-selector-ext1.md (081–088, 16 instances)
- wave-1/CB4-stage-3-pairs/node-09-formatter-ext1.md (089–096, 16 instances)
- wave-1/CB4-stage-3-pairs/node-04-domain-processor-ext1.md (127–134, 16 instances)
- wave-1/CB4-stage-3-pairs/node-05-applier-ext1.md (135–142, 16 instances)
- wave-1/CB4-stage-3-pairs/node-06-result-translator-ext1.md (143–150, 16 instances)
- wave-1/CB4-stage-3-pairs/node-01-02-03-ext1.md (159–170, 24 instances)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext2.md (107–116, 20 instances — Stratum B)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext3.md (117–126, 20 instances — Stratum D)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext4.md (171–178, 16 instances — new domains)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext5.md (179–188, 20 instances — mixed adv.)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext6.md (199–206, 16 instances — final domains)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext7.md (207–214, 16 instances — Stratum B)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext8.md (223–230, 16 instances — Stratum D)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext9.md (241–248, 16 instances — Stratum D adv.)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext10.md (257–264, 16 instances — new sub-domains)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext11.md (283–290, 16 instances — Stratum D accept/reject)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext12.md (309–316, 16 instances — Stratum B correct scoping)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext13.md (335–342, 16 instances — Stratum B/D new adversarial)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext14.md (361–368, 16 instances — Stratum A/B/D new patterns)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext15.md (387–394, 16 instances — Stratum A/B/D introspection/rotation/MFA)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext16.md (413–420, 16 instances — Stratum A/B/D key size/TLS/fixation)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext17.md (439–446, 16 instances — Stratum A/B/D CORS/implicit/DPoP/scope)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext18.md (465–472, 16 instances — Stratum A/B/D dynamic reg/URI compare/cache/scope)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext19.md (491–498, 16 instances — Stratum A/B/D OIDC nonce/aud/PAR/rate-limit)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext20.md (517–524, 16 instances — Stratum A/B/D hybrid flow/state entropy/JWE/DPoP+aud)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext21.md (543–550, 16 instances — Stratum A/B/D mTLS/UserInfo/PO.1-training/refresh-family)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext22.md (569–576, 16 instances — Stratum A/B/D resource-indicators/PKCE-length/SBOM/client-assertion)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext23.md (595–602, 16 instances — Stratum A/B/D JWKS-rollover/back-channel-logout/prompt-none/token-exchange)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext24.md (621–628, 16 instances — Stratum A/B/D access-token-query-string/error-leakage/device-flow/CORS-introspect)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext25.md (647–654, 16 instances — Stratum A/B/D PAR-request_uri/JWKS-HTTP/PKCE-verifier-storage/mTLS-CA-validation)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext26.md (673–680, 16 instances — Stratum A/B/D native-app-redirect/auth-code-single-use/grant-type-enforcement)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext27.md (699–706, 16 instances — Stratum A/B/D ID-token-API-auth/refresh-absolute-lifetime/revocation-cascade/RS-algorithm-allowlist)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext28.md (725–732, 16 instances — Stratum A/B/D AS-discovery-HTTPS/OIDC-issuer-binding/kid-header/client-auth-enforcement/RS-aud-validation)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext29.md (751–758, 16 instances — Stratum A/B/D RFC-9207-iss/scope-elevation-refresh/credential-logging/PKCE-default-plain/exp-claim-absent)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext30.md (777–784, 16 instances — Stratum A/B/D token-response-cache-control/client-cred-scope-minimization/OIDC-nonce-in-ID-token/ROPC-prohibition)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext31.md (803–810, 16 instances — Stratum A/B/D AS-PKCE-enforcement/HS256-multi-party/token-endpoint-rate-limiting/OIDC-end-session-hint)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext32.md (829–836, 16 instances — Stratum A/B/D JWT-jti/auth-code-lifetime/JWT-sub/PAR-HTTPS)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext33.md (855–862, 16 instances — Stratum A/B/D JWT-iss-validation/DPoP-ath/PKCE-verifier-entropy/redirect-uri-scheme)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext34.md (881–888, 16 instances — Stratum A/B/D state-validation/token-storage/OIDC-acr/UserInfo-scope)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext35.md (907–914, 16 instances — Stratum A/B/D mobile-client-secret/token-type/JWT-PII/confidential-PKCE)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext36.md (933–940, 16 instances — Stratum A/B/D PAR-enforcement/offline_access/JWT-typ/DPoP-cnf-jkt)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext37.md (959–966, 16 instances — Stratum A/B/D client_credentials-refresh_token/OIDC-max_age/nbf-RS/wildcard-aud)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext38.md (985–992, 16 instances — Stratum A/B/D introspection-active-expired/scope-enlargement-code-exchange/iss-absent-RS/access-token-TTL-long)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext39.md (1011–1018, 16 instances — Stratum A/B/D jku-injection/client-enumeration-error/OAuth-audit-logging/exp-not-validated-RS)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext40.md (1037–1044, 16 instances — Stratum A/B/D mtls-cert-binding/oidc-prompt-login/pkce-challenge-storage/state-echo/authz-code-lifetime/id-token-alg-none/jwt-revocation-check/client-secret-url)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext41.md (1063–1070, 16 instances — Stratum A/B/D dpop-htu-htm/par-client-binding/oidc-hybrid-c_hash/dpop-jti-replay/scope-downgrade-response/tls-version/oidc-hybrid-at_hash/expires_in-absent)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext42.md (1089–1096, 16 instances — Stratum A/B/D dpop-cnf-jkt-issuance/oidc-iss-https/jwks-rollover-window/revocation-400-error/redirect-uri-binding/jwks-use-absent/introspection-pii/user-enumeration-login-hint)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext43.md (1115–1122, 16 instances — Stratum A/B/D grant-type-required/jar-alg-none/refresh-client-binding/jwt-iat-absent/userinfo-unauthenticated/auth-method-enforcement/rs-scope-validation/authz-endpoint-http)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext44.md (1141–1148, 16 instances — Stratum A/B/D auth-time-max-age/confidential-client-none/refresh-rotation/jwks-uri-absent/post-logout-redirect/bearer-token-logging/sub-email-pii/jwt-scope-absent)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext45.md (1167–1174, 16 instances — Stratum A/B/D prompt-none-ui/client-cred-scope-elevation/error-stack-trace/jwt-nbf-absent/token-endpoint-get/iss-case-insensitive/nonce-transformed/sub-only-multiissuer)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext46.md (1193–1200, 16 instances — Stratum A/B/D jti-cycling/claims-supported-absent/expires_in-mismatch/login_hint-xss/samesite-absent/auth-code-client-binding/private_key_jwt-alg_none/userinfo-http)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext47.md (1219–1226, 16 instances — Stratum A/B/D response-type-absent/id-token-aud-absent/web-client-http-redirect/client-secret-plaintext/client-cred-scope-default/id-token-azp-absent/userinfo-query-param/as-metadata-issuer-absent)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext48.md (1245–1252, 16 instances — Stratum A/B/D redirect-uri-at-token-exchange/dynamic-reg-client-secret/as-open-redirect/at-revocation-absent/userinfo-sub-mismatch/auth-code-race-condition/totp-no-rate-limit/jar-exp-30-days)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext49.md (1271–1278, 16 instances — Stratum A/B/D rp-auth-time-max-age/revocation-json-only/pkce-verifier-not-enforced/shared-signing-key/prompt-consent-skipped/dpop-mtls-partial/error-account-leakage/sub-case-normalization)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext50.md (1297–1304, 16 instances — Stratum A/B/D rp-aud-truthy-check/rs-jwt-no-sig-verify/multiple-redirect-uri-absent/fragment-in-redirect-uri/jwks-soft-failure/claims-locales-ignored/wwwauthenticate-realm-absent/authz-endpoint-no-rate-limit)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext51.md (1323–1330, 16 instances — Stratum A/B/D multiple-bearer-methods/jar-iss-absent/client-assertion-jti-replay/token-scope-absent-narrowed/par-requesturi-expired/introspection-cache-ttl/claims-parameter-ignored/grant-types-absent)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext52.md (1349–1356, 16 instances — Stratum A/B/D bcl-events-absent/par-client-binding/azp-mismatch/implicit-flow-enabled/refresh-token-expires-absent/userinfo-cors-absent/par-expiry-long/id-token-hint-session)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext53.md (1375–1382, 16 instances — Stratum A/B/D token-exchange-type/par-jar-sig/bcl-nonce-prohibited/client-id-absent/iat-staleness/reg-auth-method/device-interval/acr-validation)
- wave-1/CB4-stage-3-pairs/node-08-arbiter-set-ext54.md (1401–1408, 16 instances — Stratum A/B/D issuer-mismatch/token-type-absent/par-request-uri-absent/userinfo-content-type/userinfo-endpoint-absent/jar-nbf-absent/azp-multi-aud/pkce-public-clients)
Multi-node:
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set.md (073–080, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext1.md (151–158, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext2.md (215–222, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext3.md (249–256, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext4.md (275–282, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext5.md (301–308, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext6.md (317–324, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext7.md (343–350, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext8.md (369–376, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext9.md (395–402, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext10.md (421–428, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext11.md (447–454, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext12.md (473–480, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext13.md (499–506, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext14.md (525–532, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext15.md (551–558, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext16.md (577–584, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext17.md (603–610, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext18.md (629–636, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext19.md (655–662, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext20.md (681–688, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext21.md (707–714, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext22.md (733–740, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext23.md (759–766, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext24.md (785–792, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext25.md (811–818, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext26.md (837–844, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext27.md (863–870, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext28.md (889–896, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext29.md (915–922, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext30.md (941–948, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext31.md (967–974, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext32.md (993–1000, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext33.md (1019–1026, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext34.md (1045–1052, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext35.md (1071–1078, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext36.md (1097–1104, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext37.md (1123–1130, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext38.md (1149–1156, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext39.md (1175–1182, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext40.md (1201–1208, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext41.md (1227–1234, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext42.md (1253–1260, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext43.md (1279–1286, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext44.md (1305–1312, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext45.md (1331–1338, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext46.md (1357–1364, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext47.md (1383–1390, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-pipeline-chain-set-ext48.md (1409–1416, 16 instances — Stratum C)
- wave-1/CB4-stage-3-pairs/node-cross-source-set.md (097–106, 20 instances — Stratum B/C)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext1.md (189–198, 20 instances — new domains)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext2.md (231–240, 20 instances — new domain cross)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext3.md (265–274, 20 instances — sub-domain cross)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext4.md (291–300, 20 instances — sub-domain accept/reject)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext5.md (325–334, 20 instances — mixed PASS/REPROCESS/CONCERN)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext6.md (351–360, 20 instances — ext13 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext7.md (377–386, 20 instances — ext14 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext8.md (403–412, 20 instances — ext15 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext9.md (429–438, 20 instances — ext16 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext10.md (455–464, 20 instances — ext17 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext11.md (481–490, 20 instances — ext18 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext12.md (507–516, 20 instances — ext19 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext13.md (533–542, 20 instances — ext20 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext14.md (559–568, 20 instances — ext21 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext15.md (585–594, 20 instances — ext22 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext16.md (611–620, 20 instances — ext23 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext17.md (637–646, 20 instances — ext24 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext18.md (663–672, 20 instances — ext25 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext19.md (689–698, 20 instances — ext26 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext20.md (715–724, 20 instances — ext27 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext21.md (741–750, 20 instances — ext28 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext22.md (767–776, 20 instances — ext29 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext23.md (793–802, 20 instances — ext30 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext24.md (819–828, 20 instances — ext31 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext25.md (845–854, 20 instances — ext32 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext26.md (871–880, 20 instances — ext33 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext27.md (897–906, 20 instances — ext34 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext28.md (923–932, 20 instances — ext35 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext29.md (949–958, 20 instances — ext36 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext30.md (975–984, 20 instances — ext37 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext31.md (1001–1010, 20 instances — ext38 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext32.md (1027–1036, 20 instances — ext39 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext33.md (1053–1062, 20 instances — ext40 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext34.md (1079–1088, 20 instances — ext41 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext35.md (1105–1114, 20 instances — ext42 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext36.md (1131–1140, 20 instances — ext43 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext37.md (1157–1166, 20 instances — ext44 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext38.md (1183–1192, 20 instances — ext45 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext39.md (1209–1218, 20 instances — ext46 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext40.md (1235–1244, 20 instances — ext47 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext41.md (1261–1270, 20 instances — ext48 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext42.md (1287–1296, 20 instances — ext49 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext43.md (1313–1322, 20 instances — ext50 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext44.md (1339–1348, 20 instances — ext51 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext45.md (1365–1374, 20 instances — ext52 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext46.md (1391–1400, 20 instances — ext53 cross-source combinations)
- wave-1/CB4-stage-3-pairs/node-cross-source-set-ext47.md (1417–1426, 20 instances — ext54 cross-source combinations)

New security domains introduced in Action 3 (beyond original 9-pair scope):
  ASVS: V6.2.4 (current password on change), V6.3 (MFA), V7.2.3 (session fixation),
        V7.2.4 (idle timeout), V8.2.2 (sensitive data in logs), V8.3.2 (admin audit log),
        V9.3.1 (token transport), V10.2.6 (state parameter), V10.3 (token revocation),
        V10.5.1 (OIDC ID token aud claim)
  CWE:  CWE-288 (auth bypass alternate path), CWE-307 (brute force), CWE-352 (CSRF)
  SSDF: PO.1 (org training), PO.3 (SBOM), PW.5 (signed artifacts), PW.9 (pre-integration review)
  IETF: RFC 7636 (PKCE plain vs S256), RFC 8628 (device flow), RFC 8725 (algorithm confusion / JWT best practices)

New sub-domains introduced in ext10 (beyond ext8 scope):
  ASVS: V10.2.1 (redirect URI exact registration), V7.3.1 (server-side session invalidation on logout),
        V10.4.6-plain (PKCE plain method), V8.2.1 (authorization metadata accessible to users),
        V6.2.5-config (bcrypt cost factor appropriateness), V9.1.1-allowlist (algorithm allowlist enforcement)
  CWE:  CWE-798 (use of hard-coded credentials — cwe-authn territory)
  SSDF: PW.1 (hardcoded credentials), PW.3.2 (security-focused code review)

Key adversarial patterns added in ext8–ext10 (Stratum D):
  V10.2.6 state: "RFC 6749 says RECOMMENDED not MUST" — rejected, independent standards
  CWE-288: "secret header = authentication" — rejected, obscurity not identity
  V8.2.2: "encrypted at rest satisfies V8.2.2" — rejected, log pipeline propagation
  CWE-862: "role checked in UI" — rejected, client-side not authorization
  V10.5.1: "single issuer makes aud redundant" — rejected, token confusion attack vector
  PW.2: "core architecture unchanged, features added" — rejected, new components in scope
  V8.1.2: "network IP whitelist = deny-by-default" — rejected, perimeter ≠ application layer
  Algorithm confusion: HS256 with RSA public key as HMAC secret — CRITICAL REPROCESS
  V10.2.1 redirect: "we own all subdomains" — rejected, exact registration required regardless
  V10.4.6 plain PKCE: "RFC 7636 allows plain" — rejected, §4.2 MUST-use-S256-if-capable
  V6.2.5 bcrypt: "cost factor is performance not security" — rejected, cost controls offline attack resistance

Key PASS acceptance cases added in ext11 (Stratum D — correct patterns recognized):
  PW.1: env var from Vault at runtime — PASS (correct credential management pattern)
  V6.2.5: Argon2id m=65536/t=3/p=4 — PASS (appropriate memory-hard configuration)
  CWE-798/PW.1: os.environ read ≠ hardcoded — PASS (env var retrieval is correct)
  V8.2.1: roles in response header + /me/profile — PASS (mechanism equivalence)
  PW.3.2: Security Champion + OWASP checklist — PASS (valid organizational model)

Key correct-scoping decisions added in ext12 (Stratum B):
  V10.4.6: PKCE on authorization code flows only; device flow (RFC 8628) correctly excluded — PASS
  V6.2.5: bcrypt cost 12 currently compliant; planning Argon2id upgrade ≠ current non-compliance — PASS
  V8.2.1: JWT role claims in user-held token satisfy authorization transparency — PASS
  V9.1.1: production RS256-only PASS; dev environment HS256 divergence = CONCERN (not REPROCESS)
  V7.3.1: 5-minute expiry → CONCERN; 12-hour nightly batch → REPROCESS (severity gradient trained)

Key adversarial patterns added in ext13 (Stratum B/D):
  V10.2.1: partial exact + wildcard remainder → REPROCESS (no "mostly exact" exception)
  V7.3.1: SSO session not terminated on app logout → REPROCESS (SSO session in scope)
  PW.1: Kubernetes ConfigMap → REPROCESS (plaintext in etcd, not credential store)
  V9.1.1: blocklist (blocks alg=none + HMAC) → CONCERN (dangerous algs blocked; not allowlist)
  V10.4.6: length-only verifier validation → REPROCESS (format check ≠ SHA256 comparison)
  V8.2.1: 403-only permission disclosure → CONCERN (reactive trial-and-error ≠ proactive access)
  PW.3.2: AI code review only → CONCERN (no human security reviewer designated)
  V6.2.5: scrypt N=1024 → REPROCESS (32× below minimum; not CONCERN — order-of-magnitude gap)

New patterns introduced in ext14 (Stratum A/B/D):
  V9.3.1: JWT at DEBUG level (disabled in prod) → CONCERN (latent risk, meaningful mitigation)
  V10.2.1: prefix matching "we own the domain" → REPROCESS (no domain-ownership exception)
  V7.2.4+V7.3.1: 30-min idle + immediate logout invalidation → PASS (clean baseline)
  V9.1.1: allowlist ["RS256", "RS384", "RS512"] → PASS (multi-algorithm allowlist valid)
  V10.4.6: UUID code_verifier (36 chars) → CONCERN (entropy adequate; length < 43 minimum)
  PW.1: os.environ from CI/CD secrets → PASS (twelve-factor pattern correct)
  CWE-269: Team Lead grants Org Admin role → REPROCESS (privilege escalation, grants not bounded)
  V6.2.5 graduation: bcrypt cost=4 → CONCERN vs scrypt N=1024 → REPROCESS (magnitude matters)
  K8s Secret without etcd encryption → CONCERN (correct resource type; encoding not encryption)

New patterns introduced in ext15 (Stratum A/B/D):
  V10.4.6: PKCE for confidential clients — CONCERN (client_secret mitigates but no explicit exemption)
  V10.3: logout revocation PASS + no refresh token rotation — CONCERN (rotation is defense-in-depth)
  V6.3: optional MFA — CONCERN (Level 1 may PASS; Level 2 requires enforcement for sensitive ops)
  V10.2.1: security design review approval as argument — PASS (exact registration is the evidence)
  RFC 7519 nbf: nbf = iat - 5 clock skew — PASS (5-second backward offset accepted practice)
  RFC 7662 §2.1: open introspection endpoint — REPROCESS (MUST auth requirement violated)
  V9.2: JWT signing key never rotated — CONCERN (no MUST violation; key lifecycle hygiene gap)
  JWT logging severity gradient: DEBUG/prod=CONCERN; DEBUG/staging+prod-data=REPROCESS; INFO/prod=REPROCESS

New patterns introduced in ext16 (Stratum A/B/D):
  RFC 7518 §3.3: 2048-bit RS256 PASS; planning 4096-bit upgrade ≠ non-compliant (mirror bcrypt→Argon2id pattern)
  RFC 6749 §3.2: token endpoint HTTP with perimeter/gateway TLS → REPROCESS (endpoint must use TLS directly)
  V7.2.3 session fixation: same session ID pre/post-login → REPROCESS
  V7.2.3 correct: new session ID generated on login + old invalidated → PASS
  V10.2.6 state: correctly implemented; SPA "no CSRF risk" argument irrelevant → PASS
  RFC 6749 §4.1.2: 10-minute single-use auth code = boundary of RECOMMENDED → PASS
  CWE-384 adversarial: "long session ID entropy substitutes for regeneration" → REJECTED (fixation ≠ guessing)
  RFC 6749 §3.2 adversarial: "API gateway TLS = endpoint TLS" → REJECTED (same as load-balancer pattern)
  RSA key size: 1024-bit RS256 → REPROCESS (RFC 7518 §3.3 MUST ≥ 2048); 2048-bit → PASS; 4096-bit → PASS
  RFC 7662 §2.1 adversarial: internal network restriction → REJECTED (perimeter ≠ application-layer auth)
  V9.2: signing key never rotated → CONCERN; quarterly rotation → PASS
  nbf graduation: 5-second → PASS; 30-minute → CONCERN (deployment concern ≠ clock skew)
  V7.2.3: stateless JWT — fixation surface absent by architecture → PASS
  RFC 6749 §4.1.2: 2-second auth code expiry → PASS (maximum sets ceiling, not floor)
  V9.2 adversarial: "4096-bit key = longer rotation" → REJECTED (size ≠ operational rotation)
  RFC 6749 §3.3: silent scope narrowing without scope response → CONCERN; rejection → PASS; scope response → PASS
  .env file graduated: server reads in prod → CONCERN; CI/CD secrets injection → PASS; platform env → PASS
  Resource server scope non-enforcement → REPROCESS (scope constraint nullified)

New patterns introduced in ext17 (Stratum A/B/D):
  CORS wildcard on token endpoint "preflight blocks it" → REPROCESS (simple POST no preflight)
  CORS: token endpoint restricted; authorization endpoint wildcard → PASS (redirects not tokens)
  response_type=token implicit flow "RFC 6749 supports it" → REPROCESS (deprecated in BCP)
  response_type=code only (implicit rejected) → PASS
  DPoP absent with 15-min tokens + HTTPS → PASS (DPoP optional, not MUST)
  RFC 7009 §2.1: unauthenticated revocation "revocation is safe" → CONCERN
  PKCE downgrade: server accepts plain as fallback → REPROCESS (downgrade attack vector)
  JWT sub claim used but not pre-validated against expected value → PASS (correct behavior)
  JWT claim validation: iss+aud+exp+iat+nbf validated → PASS; sub without pre-validation → PASS
  RFC 6749 §4.1.3: redirect_uri not verified at token exchange → REPROCESS (absent MUST step)
  redirect_uri verified at exchange + all other controls → PASS

New patterns introduced in ext18 (Stratum A/B/D):
  RFC 7591 open dynamic registration "spec supports it" → REPROCESS (unauthenticated, undermines V10.2.1)
  RFC 7591 with initial access token + manual vetting → PASS
  redirect_uri full-URI case-insensitive comparison → CONCERN (path is case-sensitive per RFC 3986)
  redirect_uri RFC 3986-correct comparison: scheme/host lower; path exact → PASS
  Internal service scope=admin for all calls → CONCERN (least-privilege violation; enforcement PASS)
  Introspection cache 5/10/15-min TTL no active invalidation → CONCERN (bounded post-revocation window)
  Introspection cache TTL=token-remaining + pub/sub invalidation ≤100ms → PASS
  Three CONCERNs do NOT escalate to REPROCESS — graduation rule: 0 REPROCESS + N CONCERN = CONCERN
  MD5 with static salt → REPROCESS (wrong algorithm + ineffective salt: doubly REPROCESS)
  "URIs are case-insensitive" → REJECTED (RFC 3986: only scheme+host are case-insensitive; path is not)

New patterns introduced in ext19 (Stratum A/B/D):
  OIDC nonce not validated at client ("nonce present = sufficient") → REPROCESS (match validation absent)
  OIDC nonce fully validated: value match + single-use + invalidation per attempt → PASS
  PAR (RFC 9126) available but not required for public clients → CONCERN (PAR recommended, not MUST)
  PAR required for all clients: authorization endpoint accepts only request_uri → PASS
  Token endpoint no rate limiting: "auth requirement substitutes" → CONCERN (argument rejected)
  OIDC aud claim not validated: "signature+issuer sufficient" → REPROCESS (token confusion attack)
  Complete OIDC ID token validation: sig, iss, exp, nbf, aud, nonce (single-use), at_hash → PASS
  OIDC nonce adversarial: "nonce present in token proves server processed our request" → REJECTED
  OIDC aud adversarial: "signature+issuer validation is sufficient" → REJECTED (aud = intended recipient)
  OIDC nonce server-side absent: client silently passes → REPROCESS (double gap)

New patterns introduced in ext20 (Stratum A/B/D):
  OIDC hybrid flow response_type=code id_token → REPROCESS (ID token in front-channel URL)
  "OIDC Core defines hybrid flow" → REJECTED (BCP restricts; same pattern as implicit flow)
  OIDC code-only flow: ID token only at token endpoint → PASS
  State parameter 6-char alphanumeric (~35 bits entropy) → CONCERN (below 128-bit minimum)
  State parameter 256-bit CSPRNG + session-stored + single-use → PASS
  JWT standard-claim payload (no PII) unencrypted → PASS (JWE not required for standard claims)
  JWT with PII (email/name/phone/address) unencrypted in localStorage → REPROCESS
    "HTTPS protects in transit" → REJECTED (localStorage is separate attack surface from transit)
  DPoP binding + aud validation at resource server → dual-binding PASS
  JWE appropriate when: payload contains PII OR token passes through untrusted intermediaries

New patterns introduced in ext21 (Stratum A/B/D):
  client_credentials with 95-bit entropy secret + bcrypt stored → PASS
  mTLS (RFC 8705) client auth + cnf/x5t#S256 certificate-bound tokens → PASS
  OIDC UserInfo sub cross-check absent — "same IdP = trusted" → CONCERN
  OIDC UserInfo sub cross-check + JWT sig validation → PASS (OIDC Core §5.3.4)
  No formal security training (SSDF PO.1) — "experienced developers = trained" → REJECTED → CONCERN
  Refresh token rotation + family detection + family revocation on replay → PASS (BCP §4.14.1)
  Refresh rotation without family revocation → CONCERN (attacker-first race condition unhandled)
  "Replay detected = attacker blocked" → REJECTED (if attacker uses first, they hold current token)
  No training + production vulnerability traceable to gap → REPROCESS (escalation from CONCERN)
  Access tokens with exp=2099 (non-expiring) → REPROCESS (bounded lifetime required)

New patterns introduced in ext22 (Stratum A/B/D):
  Risk-tiered access token TTL (60/15/5 min by risk) → PASS (bounded; risk-proportionate)
  Single token for multiple resource servers without aud RS restriction → CONCERN
  RFC 8707 resource indicators with per-RS aud binding + RS validates aud → PASS
  PKCE code_verifier 256 chars — exceeds RFC 7636 §4.1 maximum of 128 → CONCERN
    "Longer = more secure" → REJECTED (maximum exists for interoperability)
  No SBOM + 3-week CVE assessment time → CONCERN (PO.3.2; "grep = SBOM" rejected)
  CycloneDX SBOM auto-updated in CI/CD + transitive deps + 1-hour CVE query → PASS
  JWT client assertions (RFC 7523): sig + iss + aud + exp + jti → PASS (eliminates shared secrets)

New patterns introduced in ext23 (Stratum A/B/D):
  JWKS key rollover: quarterly + 24-hour dual-key window (≥ max token TTL) → PASS
  JWKS rollover window < max token TTL → CONCERN (valid tokens rejected after rotation)
  OIDC front-channel logout via iframes → CONCERN (browser reliability: SameSite, ITP, ad-blockers)
  OIDC back-channel logout with signed logout token + jti uniqueness → PASS (reliable, server-to-server)
  OIDC prompt=none with PKCE + exact redirect_uri → PASS (correct use of silent check)
  RFC 8693 token exchange with scope narrowing → PASS (least-privilege delegation)
  RFC 8693 token exchange for scope widening → REPROCESS (privilege escalation; §7.1 violation)
    "read:data authorization extends to admin" → REJECTED (scopes require independent consent)
  JWT client assertion aud validated by substring match → REPROCESS (exact match required by RFC 7523)
  JWT client assertion jti not tracked → REPROCESS ("signed = replay safe" rejected)

New patterns introduced in ext27 (Stratum A/B/D):
  ID token used for API authorization at resource server → REPROCESS (OIDC Core §2 + ASVS V10.5.3)
    ID token: aud = client_id (not RS); no scopes; no RFC 7009 revocation pathway
    "Eliminates need for access token" → conflates authentication assertion with authorization credential
    Correct: ID token for display/id_token_hint only; access token for RS (Bearer header + aud validation)
  Refresh token without absolute maximum lifetime → CONCERN (BCP §4.14 SHOULD)
    Rotation + family revocation present but no absolute max: active attacker with stolen token
    maintains indefinite access if they use it before legitimate user → active use ≠ replay
    Correct: 90-day absolute max + 30-day idle + rotation + family revocation → PASS
  Refresh token revocation no cascade to access tokens (60-min TTL) → CONCERN (RFC 7009 §2 MAY)
    RFC 7009 cascade is MAY not MUST → CONCERN not REPROCESS
    60-minute post-revocation window is meaningful; reduce TTL or implement access token blocklist
    Access token blocklist on revocation with TTL = remaining token lifetime → PASS
  RS JWT algorithm enforcement absent (accepts HS256 + "none") → REPROCESS (V9.1.1 MUST + RFC 8725)
    HS256 confusion: AS public RSA key used as HMAC secret → forged JWT passes RS validation
    "none" attack: unsigned JWT accepted with valid claims → auth bypass
    Allowlist must be checked BEFORE signature verification (not after)
    Correct: allowlist ["RS256"] or ["RS256", "PS256"] checked pre-signature; "none"/"HS*" rejected → PASS

New patterns introduced in ext26 (Stratum A/B/D):
  Native app redirect URI: custom URI scheme (myapp://) → CONCERN (RFC 8252 §7.1; hijacking risk)
    RFC 8252 §7.2 RECOMMENDS claimed HTTPS redirect URIs (Universal Links / App Links)
    Claimed HTTPS: OS-verified; only registered app can handle → PASS
    PKCE S256 REQUIRED for native apps per RFC 8252 §8.1
  Authorization code single-use not enforced → REPROCESS (RFC 6749 §4.1.2 MUST single-use)
    "Short lifetime + PKCE prevents replay" → REJECTED (MUST unconditional; same PAR pattern)
    Without single-use tracking: cannot detect code reuse → cannot fulfill revocation MUST
    Correct: mark used on first exchange; 400 on reuse; revoke ALL tokens on replay → PASS
    RFC 6749 §4.1.2: two MUST conditions — (1) single-use; (2) token revocation on replay detection
  Grant type not enforced per client registration → REPROCESS (RFC 6749 §3.1.1; CWE-269)
    "Authenticated = can use any grant" → REJECTED (authentication ≠ capability authorization)
    client registered for authorization_code using client_credentials = user consent bypass
    Correct: token endpoint validates requested grant_type against registered list → PASS

New patterns introduced in ext25 (Stratum A/B/D):
  PAR (RFC 9126) request_uri one-time-use not enforced → REPROCESS (§4.1 MUST one-time-use)
  Short expiry (30s) as substitute for one-time-use → REJECTED (MUST not conditional on risk)
  Correct PAR: Redis consumed flag + TTL + client_id binding at push and use → PASS
  JWKS endpoint over HTTP → REPROCESS (RFC 8414 §2 MUST HTTPS; RFC 7517 integrity)
    "Internal network = safe" → rejected (on-path MitM on LAN substitutes public keys)
    HTTP JWKS allows key substitution → all JWT signature verifications bypass-able
  Correct JWKS: HTTPS + TLS 1.3 + trusted CA + public-only + rotation-aware caching → PASS
  PKCE code_verifier in sessionStorage → CONCERN (JavaScript-accessible; XSS exfiltrates both code and verifier)
    "PKCE means code alone unusable" → irrelevant when XSS yields both
    "sessionStorage > localStorage" → true for persistence; both XSS-accessible equally
    Better: module-level variable (not storage API), cleared after token exchange
  mTLS: any self-signed certificate accepted without CA validation or pre-registration → REPROCESS (CWE-295)
    "Handshake proves key possession" → partially true; does NOT prove identity without binding
    RFC 8705 §2 requires CA chain validation OR explicit certificate pre-registration
  Correct mTLS: private CA trust store + fingerprint pre-registration → RFC 8705 §2 → PASS

New patterns introduced in ext24 (Stratum A/B/D):
  Access token in URL query parameter (RFC 6750 §2.3 SHOULD NOT) + full-URL DEBUG logging → REPROCESS
    Combined: SHOULD NOT transmission + MUST NOT logging (V9.3.1) = REPROCESS
    "RFC 6750 defines it" → defined as last resort, explicitly SHOULD NOT
    "Internal perimeter" → log aggregators within perimeter are still credential exposure
  OAuth error response enumerating all registered client IDs → REPROCESS (CWE-200)
    Complete client registry disclosed to unauthenticated caller via error_description
    RFC 6749 §5.2 does not require listing valid client IDs; minimum-disclosure rule violated
    error=code-only response → PASS; enumeration → REPROCESS
  Device flow (RFC 8628) no server-side rate limiting → REPROCESS (§6.1 MUST)
    "slow_down = protection" → REJECTED (client-cooperative; malicious clients ignore it)
    user_code 5-char [A-Z0-9] ~60M keyspace + 30-min lifetime + no limit = brute-forceable
    Correct: 8-char filtered-alphabet CSPRNG (~78B or more) + server-side per-device_code limit → PASS
  RFC 8628 §6.1 MUST: both conditions required — sufficient entropy AND server-side rate limiting
  Device flow brute-force argument validation: 34^6 ≈ 1.5B + 50-attempt limit → PASS (math verified)
    RFC 8628 does not mandate a specific attempt limit; entropy × limit combination governs
  CORS wildcard on /introspect: token metadata (sub/scope/exp) readable from any origin → REPROCESS
    Sensitive metadata endpoint with wildcard = V8.2.2 violation + CWE-200
  CORS: /token restricted to registered SPA origin + /introspect no CORS headers (backend-only) → PASS
    No CORS headers on backend-only endpoint is stricter than wildcard → acceptable
    Wildcard on /introspect "backends don't use CORS" argument → CONCERN (hygiene gap, not REPROCESS)
  Error response minimum-disclosure baseline: {"error": "invalid_client"} only → PASS

New patterns introduced in ext39 (Stratum A/B/D):
  JWT jku header parameter followed by RS without allowlist validation → REPROCESS (RFC 8725 §3.3 MUST ×2)
    jku must be explicitly allowed + JWKS fetched only from trusted locations; both MUSTs violated
    Attack: attacker crafts token with jku pointing to attacker-controlled JWKS; RS validates attacker's token
    "Honoring jku is spec-compliant" → rejected; RFC 8725 §3.3 restricts jku to pre-trusted locations
    Correct: RS ignores jku/jwk/x5u headers; uses only deployment-configured JWKS endpoint → PASS
  Token endpoint returns distinct errors for unknown client_id vs wrong secret → REPROCESS (RFC 6749 §5.2 + CWE-204)
    RFC 6749 §5.2: all authentication failures → invalid_client; no distinct unknown_client code
    CWE-204: observable response discrepancy enables client_id enumeration by unauthenticated attacker
    "Helpful error messages" → rejected; registered client IDs must not be discoverable by unauthenticated parties
    Correct: uniform invalid_client response; failure details logged internally only → PASS
  OAuth security events (failed auth, redirect_uri mismatch, state mismatch) not logged → CONCERN (ASVS V8.3.2)
    ASVS V8.3.2: security event log must capture forensic detail; OAuth events are forensically relevant
    Network-layer logs lack POST body content needed for OAuth event correlation
    No RFC MUST for specific OAuth event logging; ASVS guidance gap → CONCERN not REPROCESS
    Correct: application-level structured JSON logging of security events excluding credentials/PII → PASS
  RS does not validate exp claim — expired tokens accepted indefinitely → REPROCESS (RFC 9068 §4 MUST)
    RFC 9068 §4: RS MUST reject JWT when exp < current Unix time
    Stolen token valid forever without exp check; 15-minute TTL from issuance irrelevant at RS
    "UX preference for graceful acceptance" → rejected; MUST is unconditional; client should refresh on 401
    Correct: exp > current_time asserted in validation pipeline; expired token → 401 → client refresh → PASS

New patterns introduced in ext38 (Stratum A/B/D):
  Token introspection returns active=true for expired tokens → REPROCESS (RFC 7662 §2.2 active field semantics)
    active must accurately reflect current status including expiry; expired token → active=false is required
    "RS can check exp themselves" → rejected; introspection is the authoritative status oracle; RFC 7662 purpose
    Correct: introspection evaluates exp + revocation before responding; active=false for expired/revoked → PASS
  Scope enlargement at authorization code exchange → REPROCESS (RFC 6749 §4.1.3 MUST NOT)
    scope in token request exceeds authorization grant scope; user only consented to read:profile
    "Client knows what it needs" → rejected; resource owner consent at auth phase is the ceiling
    RFC 6749 §4.1.3: scope MUST NOT include any scope not originally granted by resource owner
    Correct: token endpoint validates scope against authorization grant; expansion rejected (error=invalid_scope) → PASS
  JWT iss claim not validated at RS — any issuer's token accepted → REPROCESS (RFC 9068 §4 MUST validate iss)
    "Signature proves source" → rejected; iss validation and signature verification are complementary
    Signature verifies integrity; iss verifies provenance (token from expected AS vs any JWKS source)
    Correct: RS validates iss against exact allowlist before key lookup → PASS
  Access token TTL of 24 hours for user-facing flows → CONCERN (ASVS V9.3.1 guidance "usually 15 min to 1 hour")
    24 hours is 16-96× above guidance ceiling; "usually" is guidance language, not MUST → CONCERN not REPROCESS
    24-hour exfiltration exploitation window is meaningful but no RFC provides max TTL as MUST
    Correct: 15-minute access token TTL + refresh token for continuity (offline_access + rotation) → PASS

New patterns introduced in ext37 (Stratum A/B/D):
  client_credentials grant response includes refresh_token → REPROCESS (RFC 6749 §4.4.3 MUST NOT)
    "Long-running services need lifecycle simplification" → rejected; service can re-authenticate with its credentials
    refresh_token creates additional long-lived secret without security benefit in client-credential context
    Correct: client_credentials response contains only access_token, token_type, expires_in, scope → PASS
  OIDC max_age parameter present but ignored by AS → REPROCESS (OIDC Core §3.1.2.2 MUST re-authenticate)
    max_age is a security requirement from RP, not advisory hint; elapsed time > max_age → AS MUST re-authenticate
    "Session continuity priority" → rejected; MUST has no session-continuity exception
    Correct: AS evaluates max_age, triggers re-auth if elapsed > max_age, updates auth_time in ID token → PASS
  RS skips nbf validation, accepts token before nbf time → REPROCESS (RFC 9068 §4 MUST reject if nbf > current time)
    "exp covers validity window; nbf is redundant" → rejected; exp = end bound; nbf = start bound; complementary checks
    Penetration test: token with nbf 1 hour in future accepted immediately
    Correct: RS validates nbf ≤ current_time + 5-second clock skew tolerance → PASS
  JWT aud claim as wildcard pattern (*.example.com) instead of specific RS URI → CONCERN (RFC 8725 §3.9 SHOULD)
    Wildcard aud defeats per-RS audience isolation; any RS on domain accepts any token
    No explicit MUST prohibiting wildcard in RFC 8725; SHOULD-level guidance gap → CONCERN not REPROCESS
    Correct: per-RS aud via RFC 8707 resource indicators; each RS validates its own URI in aud → PASS

New patterns introduced in ext36 (Stratum A/B/D):
  PAR require_pushed_authorization_requests=true with authorization endpoint fallback → REPROCESS (RFC 9126 §4 MUST reject non-PAR requests)
    "Legacy migration path" → rejected; metadata is a binding commitment; migration handled by separate instance with flag=false
    Authorization endpoint correctly enforces PAR-required: 400 error=invalid_request on non-request_uri requests → PASS
  Refresh token issued without offline_access scope in authorization request → REPROCESS (OIDC Core §11 MUST NOT)
    "UX default: refresh tokens always helpful" → rejected; refresh token grants offline access beyond user's consent scope
    OIDC Core §11: MUST NOT issue refresh token unless offline_access was in scope + OP supports it
    Correct: refresh token gated on offline_access scope presence; consent screen names offline capability → PASS
  JWT access token typ header absent (or generic "JWT") → REPROCESS (RFC 9068 §2.1 MUST typ=at+JWT)
    typ=at+JWT is a structural discriminator preventing token confusion attacks (ID token presented as access token)
    Without typ: RS cannot distinguish access tokens from ID tokens or other JWT types; same issuer/alg/kid used for both
    "structurally valid JWT is sufficient" → rejected; RFC 9068 profile-specific MUST overrides base JWT optional typ
    RS validates typ as first check before signature verification → PASS
  DPoP-bound access token (cnf/jkt present) accepted at RS as plain Bearer → REPROCESS (RFC 9449 §7.2 MUST validate DPoP proof)
    Ignoring cnf/jkt converts sender-constrained token to unconstrained Bearer; stolen token usable without DPoP key
    "Signature + claims validation = sufficient" → rejected; cnf/jkt is a binding mandate, not optional metadata
    RS validates full DPoP chain: key match via cnf/jkt, htm/htu, ath, iat, jti replay prevention → PASS

New patterns introduced in ext35 (Stratum A/B/D):
  Hardcoded client_secret in mobile app binary → REPROCESS (ASVS V9.2.1 MUST no hardcoded credentials; CWE-798)
    "Obfuscation provides protection" → rejected; obfuscation is deterrent not security control
    RFC 8252: native apps are public clients; no client_secret required or appropriate
    Correct: register as public client with PKCE S256; no credential to hardcode
  Mobile app as public client with PKCE S256 (no client_secret) → PASS (RFC 8252; ASVS V9.2.1 satisfied by design)
  Token endpoint successful response missing token_type field → REPROCESS (RFC 6749 §5.1 MUST)
    RFC 6749 §5.1: MUST include access_token AND token_type in successful response
    "Documentation tells clients the type" → rejected; §5.1 requires self-describing response
    token_type allows programmatic determination of Authorization header scheme
  Token endpoint response with token_type=Bearer + all §5.1 MUST fields → PASS
  JWT access token containing email in custom claim, unencrypted → CONCERN (ASVS V9.3.1 PII-in-token guidance)
    JWT payload transparent to all bearers; RS logging risk introduces V8.2.2 concern
    "HTTPS protects in transit" → rejected (ext20 pattern); payload exposed in logs/storage/debug
    No unconditional MUST for JWE on access tokens: CONCERN not REPROCESS
  JWT access token with standard non-PII claims only (sub PPID, iss, aud, exp, iat, jti, scope) → PASS
  Confidential client authorization code flow without PKCE → CONCERN (RFC 9700 §2.1.1 SHOULD for confidential clients)
    client_secret authenticates at token endpoint but does not prevent code interception attack
    RFC 9700 §2.1.1 SHOULD (not MUST) for confidential clients: CONCERN not REPROCESS
  Confidential client with authorization code + PKCE S256 + client_secret (layered) → PASS

New patterns introduced in ext34 (Stratum A/B/D):
  OAuth state parameter sent but not validated at callback → REPROCESS (RFC 6749 §10.12 MUST CSRF protection; CWE-352)
    "Sending for logging" rejected: state provides CSRF protection only through validation, not transmission
    Attack: attacker's auth code CSRF-injected into victim's callback; state mismatch not detected; victim bound to attacker's token
  State 256-bit CSPRNG + exact match + single-use + absent-rejected → PASS (RFC 6749 §10.12 CSRF protection satisfied)
  Access tokens stored in browser localStorage → CONCERN (ASVS V9.3.1 SHOULD guidance; XSS exfiltration surface)
    XSS can re-exfiltrate tokens continuously to defeat TTL mitigation; "validate on every API call" = RS-side, orthogonal to storage
    No unconditional MUST excluding localStorage → CONCERN not REPROCESS
  Access tokens in module-level memory; refresh token in HttpOnly Secure SameSite=Strict cookie → PASS
  OIDC acr claim not validated despite acr_values request → CONCERN (OIDC Core §3.1.3.7.9 SHOULD)
    AS returns acr=1 when acr=2 was requested; RP doesn't check acr → MFA requirement silently bypassed
    SHOULD not MUST: CONCERN not REPROCESS
  OIDC acr validated: acr ≥ requested level; insufficient assurance → rejected + re-auth forced → PASS (above SHOULD floor)
  UserInfo endpoint returns all claims regardless of access token scope → REPROCESS (OIDC Core §5.3.3 MUST scope-filtered)
    OIDC scope-claim mapping: openid → sub only; profile → name/etc.; email/phone/address scopes required for respective claims
    "Authenticated = entitled" rejected: authentication proves token validity; scope proves claim entitlement
    Privacy violation: user consented to scope=openid; receiving email/phone is beyond consented disclosure
  UserInfo returns only scope-authorized claims per §5.4 mapping → PASS

New patterns introduced in ext33 (Stratum A/B/D):
  JWT iss validated with substring/contains match → REPROCESS (RFC 7519 §4.1.1 case-sensitive exact string comparison required)
    "contains('as.example.com')" allows "evil-as.example.com" to pass iss validation
    In JWKS-discovery mode: RS fetches keys from attacker-controlled discovery URL → forged tokens pass both iss and sig validation
    "Handles staging + production in one check" → rejected; correct: allowlist of exact issuer strings per environment
  JWT iss validated with exact case-sensitive allowlist → PASS
  DPoP proof at RS without ath (access token hash) claim → REPROCESS (RFC 9449 §4.2 MUST ath at RS)
    ath = BASE64URL(SHA256(ASCII(access_token))); binds proof cryptographically to specific access token
    Without ath: stolen access token + new DPoP proof from attacker's key pair = valid request at RS
    "RS can correlate without ath" → rejected; visual co-presence ≠ cryptographic binding
  DPoP proof with ath + htm + htu + iat + jti + cnf/jkt cross-check → PASS (RFC 9449 §4.2 fully satisfied)
  PKCE code_verifier from Unix timestamp → REPROCESS (RFC 7636 §4.1 MUST high-entropy CSPRNG; also length < 43 chars)
    Timestamp ≈ 10 bits/second entropy; ±5s window → 10,000 enumeration candidates; brute-forceable in 15-min code window
    "Unique per request" → rejected; uniqueness ≠ unpredictability; PKCE requires cryptographic unguessability
  PKCE code_verifier from CSPRNG; 64 chars; 384-bit entropy; unreserved chars; in-memory only → PASS
  Authorization server accepts redirect_uri with javascript: URI scheme → REPROCESS (CWE-79 XSS at AS origin)
    javascript: registration + runtime redirect = stored XSS executed at AS origin
    "Runtime match is the RFC requirement" → rejected; AS must validate scheme at registration not just match at runtime
    Permitted schemes: https (web), http (localhost), custom scheme (native apps per RFC 8252)
  AS enforces scheme allowlist (https/http-localhost/custom) at registration; javascript: rejected with 400 → PASS

New patterns introduced in ext32 (Stratum A/B/D):
  JWT access tokens without jti claim → CONCERN (RFC 7519 §4.1.7 SHOULD; not MUST)
    "15-min TTL substitutes for jti" → partially accepted (limits breach window) but incomplete
    jti enables: (a) per-token precision revocation before TTL; (b) RS-side concurrent-use anomaly detection; (c) per-token audit correlation
    SHOULD-level gap: CONCERN
  JWT with jti (UUID v4) + Redis JTI blocklist with TTL-matched entries → PASS
  Authorization codes with 24-hour lifetime → REPROCESS (RFC 6749 §4.1.2 MUST expire shortly)
    "Single-use enforcement makes lifetime irrelevant" → REJECTED (addresses different threat: replay vs interception-and-use)
    24-hour window: attacker who intercepts code has a full day to execute exchange
    "User flexibility — return next day" → REJECTED (UX preference; new auth request required for next session)
    "Shortly" universally interpreted as minutes; RFC references 10 minutes as reference point
  60-second authorization code + single-use + replay-triggered revocation → PASS
  JWT sub claim absent from access tokens → REPROCESS (RFC 9068 §2.2 MUST sub)
    "Privacy: don't expose user identifiers in tokens" → REJECTED (wrong remediation; opaque PPID satisfies MUST + privacy)
    Without sub: RS cannot apply per-user policies, user-level revocation non-functional, audit logs user-blind
  JWT with opaque UUID v4-based PPID sub → PASS (RFC 9068 §2.2 MUST satisfied; no PII in sub)
  PAR endpoint over plain HTTP → REPROCESS (RFC 9126 §3 MUST TLS; endpoint-specific requirement)
    "Server-to-server over internal network" → REJECTED (same category as JWKS/discovery HTTP: on-path MITM)
    On-path attacker can replace code_challenge before PAR request is registered (parameter injection)
    "TLS on authorization endpoint covers PAR" → REJECTED (each endpoint has independent transport requirement)
  PAR endpoint over HTTPS (TLS 1.3) + client authentication + request_uri single-use + client_id binding → PASS

New patterns introduced in ext31 (Stratum A/B/D):
  AS does not require PKCE for public clients → REPROCESS (RFC 9700 §2.1.1 MUST require PKCE for public clients)
    "Backward compatibility with older clients" → REJECTED (MUST has no compatibility exception; update clients)
    Optional PKCE = no PKCE protection for clients that omit it; code interception viable
  AS enforces PKCE requirement for all public client authorization code requests → PASS
  HS256 (symmetric) signing in multi-party token system → REPROCESS (RFC 8725 §3.6 + V9.1.1)
    Symmetric key distributed to all RSes: any RS compromise → token forgery capability for all RSes
    "HS256 is simpler" → REJECTED (operational simplicity does not satisfy multi-party signing security)
    RFC 8725 §3.6: use asymmetric keys when signer and verifiers are different parties
  RS256 (asymmetric) signing: AS holds private key; RSes use HTTPS JWKS public key only → PASS
  Token endpoint IP-rate-limiting only (no per-client-id throttling) → CONCERN (botnet bypass viable; no MUST for specific mechanism)
    "High-volume legitimate services can't be per-client throttled" → partially accepted as design challenge; CONCERN
  Per-client-id rate limiting with graduated response (counter + 429 + rotation flag) → PASS
  OIDC End Session without id_token_hint requirement → CONCERN (RECOMMENDED not MUST; session forcing attack viable)
    "Logout is harmless" → REJECTED (session forcing: disrupts operations, DoS against specific users, phishing precursor)
  OIDC End Session with id_token_hint required and fully validated (5-step chain) → PASS

New patterns introduced in ext30 (Stratum A/B/D):
  Token response Cache-Control: no-store absent → REPROCESS (RFC 6749 §5.1 MUST on responses containing tokens)
    "Short-lived tokens minimize risk" → REJECTED (refresh tokens long-lived; MUST not conditional on TTL)
    "API gateway handles headers" → REJECTED (MUST applies at AS origin, not downstream component)
  Correct: Cache-Control: no-store + Pragma: no-cache on all token responses → PASS
  Client credentials service account with scope=admin:all (actual need: read:user-activity only) → REPROCESS (CWE-269 + ASVS least-privilege)
    "Internal service, trusted" → REJECTED (internal compromise and lateral movement are real vectors; least-privilege applies to all services)
    Convenience-driven over-privilege is canonical CWE-269 scenario
  Correct: minimal scope registration matching actual service need → PASS; scope enforcement at RS provides defense-in-depth
  OIDC nonce absent from ID token when nonce was in authorization request → REPROCESS (OIDC Core §3.1.3.7.11 MUST)
    "Nonce is an optional claim" → partially accurate (optional when client doesn't provide it); MUST when present in request
    Absent nonce eliminates token-level replay binding → replayed ID tokens from prior sessions undetectable
  Correct: AS echoes nonce in ID token; client validates match + single-use → PASS
  ROPC (Resource Owner Password Credentials) grant type enabled → REPROCESS (RFC 9700 §2.4 MUST NOT)
    "RFC 6749 defines ROPC" → REJECTED (RFC 9700 BCP 240 supersedes; MUST NOT)
    "Users prefer direct credential entry" → REJECTED (direct credential collection by application = OAuth trust boundary elimination)
    ROPC: no MFA enforcement possible; phishing applications indistinguishable; AS trust boundary bypassed
  ROPC disabled; authorization code + PKCE S256 replaces it → PASS

New patterns introduced in ext29 (Stratum A/B/D):
  RFC 9207 iss parameter absent from authorization response → CONCERN (SHOULD not MUST; mix-up attack defense)
  Correct: iss present + client validates against configured issuer → PASS
  Scope elevation at refresh token exchange (AS honors scope beyond original grant) → REPROCESS (RFC 6749 §6 ceiling violation)
    "User authenticated; incremental scopes supported" → REJECTED (new scopes require new authorization request; refresh is server-to-server)
    RFC 6749 §6: scope within original grant (ceiling); AS must reject or narrow; cannot expand
  Correct: refresh scope bounded to original grant; downscoping also correctly honored → PASS
  Token endpoint logs full POST body including client_secret in production → REPROCESS (V8.2.2 MUST NOT log credentials)
    "Access-controlled log store" → REJECTED (MUST NOT is unconditional; ELK multiplies exposure surface)
  Correct token logging: non-credential metadata only + framework-level credential redaction → PASS
  PKCE code_challenge_method absent → AS defaults to plain (RFC 7636 §4.3) → REPROCESS (§4.2 MUST use S256 if capable)
    "Following RFC 7636 default" → partially accurate about §4.3 but does not satisfy §4.2 MUST for S256-capable clients
    Omission-downgrade same risk as explicit plain-method downgrade from S256-capable client
  JWT access tokens without exp claim → REPROCESS (RFC 9068 §2.2 MUST exp; tokens non-expiring at RS)
    "RFC 7519 exp is OPTIONAL" → REJECTED (RFC 9068 §2.2 profile-specific MUST overrides base-spec OPTIONAL for access tokens)
    "Refresh token expiry manages lifecycle" → REJECTED (RS validates access token directly; refresh token state orthogonal)
    "Application session management" → REJECTED (application session ≠ access token; token replayable independent of session)

New patterns introduced in ext28 (Stratum A/B/D):
  AS discovery metadata over HTTP → REPROCESS (RFC 8414 §3 MUST HTTPS; MITM can replace endpoint URLs in discovery doc)
    "Public document needs no HTTPS" → REJECTED (integrity not confidentiality is the requirement)
    Attacker replaces token/auth/JWKS endpoint URLs → clients directed to attacker-controlled endpoints
  Correct discovery: HTTPS + all endpoints HTTPS + issuer matches discovery URL base → PASS
  OIDC Discovery issuer mismatch (issuer ≠ discovery URL base) → REPROCESS (OpenID Connect Discovery §4.3 MUST)
    "Both hostnames resolve to same server" → REJECTED (DNS resolution at time of fetch is not persistent binding)
    Mismatch breaks §4.3 binding + causes downstream token iss validation failures
  Correct OIDC Discovery: issuer field exactly matches base URL used to fetch → PASS
  JWT without kid header during multi-key JWKS rollover → CONCERN (RFC 7515 §4.1.4 SHOULD; not MUST)
    RS must iterate all active JWKS keys; kid absent removes cache-refresh signal (kid-not-found → re-fetch)
    SHOULD violation; token validation still succeeds; operational risk not MUST violation → CONCERN
  JWT with kid header matching active signing key → PASS (O(1) key lookup; cache-refresh-aware)
  Token endpoint accepts unauthenticated requests from registered confidential clients → REPROCESS (RFC 6749 §2.3 MUST)
    "Flexible auth — clients that know their secret provide it" → REJECTED (MUST is unconditional)
    client_id alone yields a valid access token; penetration tester confirmed → REPROCESS
  RS aud claim not validated (any valid access token accepted at any RS) → REPROCESS (RFC 9068 §4; RFC 8725 §3.9)
    "Same issuer = trusted" → REJECTED (issuer proves authenticity; aud proves intended recipient)
    Audience confusion attack: api.example.com token used at admin.example.com → passes validation
    RFC 9068 §4 requires RS to verify its own identifier in aud claim → MUST violated → REPROCESS

New patterns introduced in ext40 (Stratum A/B/D):
  mTLS certificate-bound token at RS — cnf/x5t#S256 not compared against TLS client cert → REPROCESS (RFC 8705 §3.1 MUST)
    "mTLS connection proves client has a certificate" → REJECTED; RS cannot verify identity without comparing thumbprints
    Attack: stolen certificate-bound token used by attacker with different certificate; RS validates without binding check
    Correct: extract SHA-256 thumbprint from TLS connection; compare byte-for-byte against cnf/x5t#S256; reject on mismatch → PASS
  OIDC prompt=login parameter ignored by AS — existing session returned without re-authentication → REPROCESS (OIDC Core §3.1.2.1 MUST)
    "prompt=login is a hint, not a mandate" → REJECTED; OIDC Core uses MUST language; prompt=login is normative
    Step-up authentication security guarantee silently bypassed; stale auth_time in id_token
    Correct: AS ignores existing session; forces authentication UI; returns login_required when interactive auth not possible → PASS
  PKCE code_challenge accepted but not persisted — any non-empty code_verifier accepted at exchange → REPROCESS (RFC 7636 §4.4 MUST ×2)
    "code_verifier present = PKCE implemented" → REJECTED; presence without cryptographic comparison is not PKCE
    RFC 7636 §4.4 MUST (×2): store code_challenge at auth request; verify code_verifier against stored challenge at token request
    Correct: code_challenge + code_challenge_method stored with auth code; BASE64URL(SHA256(verifier)) constant-time compared → PASS
  AS omits state from authorization response when client included it → REPROCESS (RFC 6749 §4.1.2 REQUIRED)
    "State is a client-side construct; AS doesn't need to echo it" → REJECTED; RFC 6749 §4.1.2 REQUIRED: AS MUST echo state
    Without echoed state: client cannot perform CSRF verification; injected authorization responses indistinguishable
    Correct: state echoed unmodified in every authorization response where present in request → PASS
  Authorization code lifetime 10 minutes — beyond accepted short-lived practice → CONCERN (RFC 6749 §10.5 / ASVS V3.5.2 guidance)
    RFC 6749 §10.5: "short-lived" not numerically defined; ASVS V3.5.2: "shortest possible time" (guidance, not MUST)
    10 minutes is 5–10× broader than 1–2 minute accepted practice; no RFC numeric MUST for maximum → CONCERN
    Correct: 60-second codes, single-use, client_id-bound, reuse-triggered token revocation → PASS
  OIDC RP accepts id_token with alg: none — unsigned token accepted as valid → REPROCESS (RFC 8725 §2.1 MUST NOT)
    "alg:none is valid per RFC 7518; needed for flexible internal deployments" → REJECTED; RFC 8725 §2.1 MUST NOT overrides RFC 7518
    Complete identity impersonation: attacker fabricates id_token with arbitrary claims; sets alg:none; RP accepts
    Correct: explicit algorithm allowlist (RS256/ES256 family); alg:none and non-allowlisted algorithms rejected → PASS
  RS accepts JWT access tokens without revocation check — revoked tokens valid until expiry → CONCERN (ASVS V9.3.2 guidance)
    No RFC MUST for per-request JWT revocation checking at RS; stateless JWT trade-off is architecturally accepted
    ASVS V9.3.2 guidance: short TTL as compensating control; no compensating control documented → guidance gap
    Correct: 15-min access token TTL (ASVS V9.3.1) + RFC 7662 introspection for sensitive endpoints → PASS
  client_secret transmitted as URI query parameter → REPROCESS (RFC 6749 §2.3.1 MUST NOT)
    "HTTPS encrypts query parameters in transit" → REJECTED; query params persist in server/proxy logs and browser history
    RFC 6749 §2.3.1 MUST NOT: AS must not accept client authentication via URI query parameters (log exposure, not transit)
    Correct: HTTP Basic Authorization header or POST body; token endpoint rejects URI query param credentials → PASS

New patterns introduced in ext41 (Stratum A/B/D):
  DPoP proof htu/htm not validated at RS → REPROCESS (RFC 9449 §7.2 MUST ×2)
    "aud and jkt cover binding; htu/htm are redundant" → REJECTED; aud is per-RS not per-endpoint; htu/htm bind proof to specific endpoint+method
    Attack: DPoP proof captured from GET /user/profile replayed at DELETE /admin/accounts; both pass jkt and sig validation
    Correct: validate htm (exact HTTP method) and htu (URI without query/fragment) on every DPoP-protected request → PASS
  PAR request_uri — client_id binding not verified at authorization endpoint → REPROCESS (RFC 9126 §5 MUST)
    "Short-lived and opaque makes cross-client use infeasible" → REJECTED; MUST applies regardless of opaqueness
    Cross-client substitution: Client A pushes PAR; Client B with obtained request_uri uses it with its own credentials
    Correct: store client_id with pushed request; verify at authorization endpoint; reject mismatch with invalid_request → PASS
  OIDC hybrid flow — id_token missing c_hash when code returned from authorization endpoint → REPROCESS (OIDC Core §3.3.2.11 MUST)
    "c_hash optional per OIDC Core; server-side binding is equivalent" → REJECTED; MUST applies when code returned; RP-verifiable claim required
    Correct: c_hash = BASE64URL(LEFT_HALF(hash(code))); RP validates against received code; hash algorithm matches JOSE alg → PASS
  DPoP jti not tracked server-side — DPoP proof replay within freshness window → REPROCESS (RFC 9449 §7.2 MUST)
    "iat freshness window limits replay to 60 seconds" → REJECTED; iat defines outer boundary; jti closes it per-proof
    Without jti cache: captured proof replayed N times within 60 seconds; Redis cache with TTL is standard solution
    Correct: maintain per-proof jti cache (Redis + TTL=freshness window + clock skew); reject seen jti → PASS
  Scope downgrade without scope in token response → REPROCESS (RFC 6749 §3.3 MUST)
    "scope is optional per RFC 6749" → PARTIALLY CORRECT for exact-match case; MUST when issued scope ≠ requested scope
    Client has no programmatic discovery mechanism; forced into 403-driven reactive pattern
    Correct: return scope in token response whenever issued scope differs from requested; omit only on exact match → PASS
  Token endpoint accepts TLS 1.0 and TLS 1.1 → REPROCESS (RFC 9325 §4 / BCP 195 MUST NOT)
    "Backward compat; TLS 1.2 negotiated first; no observed attacks" → REJECTED; MUST NOT is unconditional; downgrade attacks force fallback
    Token endpoint transmits client secrets and bearer tokens — deprecated TLS exposes credential transmission
    Correct: MinProtocol=TLSv1.2; TLS 1.3 preferred; TLS 1.0/1.1 explicitly disabled; PFS cipher suites only → PASS
  OIDC hybrid flow — id_token missing at_hash when access token returned from authorization endpoint → REPROCESS (OIDC Core §3.3.2.11 MUST)
    "at_hash optional per OIDC Core; AT short-lived" → REJECTED; MUST applies when AT returned at auth endpoint; TTL not substitute for binding
    Correct: at_hash = BASE64URL(LEFT_HALF(hash(access_token))); RP validates; absent only when AT not returned at auth endpoint → PASS
  expires_in absent from token response → CONCERN (RFC 6749 §5.1 RECOMMENDED)
    "Clients discover via 401; expires_in is optional" → PARTIALLY CORRECT; RECOMMENDED not MUST; optional-when-matching is accurate
    RFC 6749 §5.1 SHOULD: "provide expiration time via other means or document default value" → partially satisfied by developer portal docs
    No RFC MUST for expires_in → guidance gap only; CONCERN not REPROCESS
    Correct: include expires_in (seconds) in all token responses for proactive client token lifecycle management → PASS

New patterns introduced in ext42 (Stratum A/B/D):
  DPoP-bound AT issued without cnf/jkt in token → REPROCESS (RFC 9449 §6 MUST embed cnf/jkt when DPoP proof accepted)
    "DPoP header presence at token endpoint satisfies binding" → REJECTED; cnf/jkt in AT is what allows RS to verify key binding
    Without cnf/jkt in AT: RS cannot perform DPoP validation; cnf/jkt is mandatory carrier of the sender's public key thumbprint
    Correct: AS receives DPoP proof at token endpoint; extracts jkt from proof; embeds cnf: {jkt: <thumbprint>} in issued AT → PASS
  OIDC issuer URL using HTTP scheme → REPROCESS (OIDC Core §2 MUST: iss uses HTTPS URI with no query/fragment)
    "Internal deployment; TLS at load balancer" → REJECTED; MUST applies to iss identifier itself, not just transport at time of fetch
    HTTP iss breaks downstream iss validation at any RS following the MUST; also enables downgrade to plaintext AS
    Correct: iss = "https://as.example.com" (no trailing slash, no query/fragment); all OIDC metadata URLs consistent → PASS
  JWKS dual-key rollover window shorter than maximum access token TTL → CONCERN (ASVS V9.2 guidance; no RFC numeric MUST for minimum overlap duration)
    Short overlap (10 min) with 1-hour AT TTL: tokens signed under old kid arrive after key removed; RS gets kid-not-found → validation failure
    No RFC MUST for specific minimum overlap window; ASVS V9.2 advises overlap ≥ max token TTL → CONCERN
    Correct: overlap window = max(AT TTL, refresh token TTL) + buffer; both old and new keys retained during window → PASS
  Revocation endpoint returns 400 for unrecognized token → REPROCESS (RFC 7009 §2.2 MUST return 200 for unknown tokens)
    "400 communicates to client that token was not found" → REJECTED; RFC 7009 §2.2 MUST 200 regardless of token recognition
    Rationale: revocation endpoint must not disclose whether a given token was ever issued (token existence oracle)
    Correct: 200 OK for any syntactically valid token revocation request; error 400 only for malformed requests → PASS
  redirect_uri at token exchange not compared against authorization request URI → REPROCESS (RFC 6749 §4.1.3 MUST match)
    "Only one redirect_uri registered; match is implicit" → REJECTED; MUST is procedural; single registered URI does not waive comparison
    RFC 6749 §4.1.3: AS MUST verify redirect_uri against original authorization request, not just against registered list
    Correct: store redirect_uri from authorization request with auth code; compare byte-for-byte at token exchange; reject mismatch → PASS
  JWKS use parameter absent from published keys → CONCERN (RFC 7517 §4.2 SHOULD include use for key disambiguation)
    "Only one key; use is obvious" → REJECTED (SHOULD not conditional on key count; adds defense against future multi-key confusion)
    No unconditional MUST for use parameter → guidance gap only; CONCERN not REPROCESS
    Correct: include use="sig" (or "enc") in each JWK; helps RS rule-out encryption keys from signature verification → PASS
  Token introspection response includes email/phone in response regardless of access token scope → CONCERN (RFC 7662 §2.2 SHOULD NOT exceed token scope)
    "Introspection is admin API; completeness is useful" → REJECTED; SHOULD NOT is normative; scope boundary applies
    RFC 7662 §2.2: response SHOULD NOT include more PII than is bounded by the scope of the introspected token
    No unconditional MUST; CONCERN not REPROCESS; privacy-relevant gap documented
    Correct: introspection response filtered to claims derivable from issued scope; extra PII omitted → PASS
  AS accepts login_hint exposing whether username exists via timing/response differences → CONCERN (CWE-204 + ASVS V6.1.3 guidance)
    "login_hint is optional; user provides own identifier" → PARTIALLY ACCEPTED (user provides; AS should not expose enumeration signal)
    CWE-204 user enumeration risk; ASVS V6.1.3 guidance: account existence must not be disclosed via observable difference
    No RFC MUST NOT for login_hint-specific timing/response uniformity; CONCERN not REPROCESS
    Correct: identical response latency and format regardless of account existence when processing login_hint → PASS

New patterns introduced in ext43 (Stratum A/B/D):
  grant_type absent from token request → AS defaults to authorization_code → REPROCESS (RFC 6749 §4.1.3 REQUIRED; §5.2 error=invalid_request)
    "Infer grant type from co-present code parameter" → REJECTED; REQUIRED parameter absence must trigger error=invalid_request; inference substitutes AS judgment for explicit parameter semantics
    Correct: error=invalid_request returned when grant_type absent; no inference permitted → PASS
  OIDC JAR request object with alg:none accepted by AS → REPROCESS (RFC 9101 §10.2 MUST NOT)
    "alg:none is spec-defined; internal deployments benefit from flexibility" → REJECTED; RFC 9101 §10.2 MUST NOT is unconditional; unsigned request object ≡ plain query parameters
    Unsigned JAR: on-path attacker can substitute authorization parameters; JAR integrity guarantee nullified
    Correct: AS rejects alg:none request objects with invalid_request_object; signing algorithm allowlist required → PASS
  Refresh token issued without client_id binding → CONCERN (RFC 6749 §10.4 / RFC 9700 §4.13.2 SHOULD)
    No unconditional MUST for client_id binding; cross-client use is a real risk but guidance-level only → CONCERN
    Correct: refresh token stored with client_id binding; exchange from mismatched client → invalid_grant → PASS
  JWT access token without iat claim; RS no recency check → CONCERN (RFC 9068 §4 SHOULD)
    All MUST-level validations satisfied (typ, iss, aud, exp); iat and recency are SHOULD; defense-in-depth gap → CONCERN
    Correct: iat included in every AT; RS validates recency within 30-second clock-skew window → PASS
  OIDC UserInfo endpoint without Bearer token requirement → REPROCESS (OIDC Core §5.3.1 MUST; CWE-862)
    Sub-query-parameter routing without token validation bypasses consent model; any sub-value caller retrieves PII
    "Authenticated callers still have to know the sub" → REJECTED; MUST is unconditional; knowledge of sub ≠ authorization
    Correct: UserInfo requires valid Bearer token; unauthenticated requests → 401 → PASS
  AS does not enforce token_endpoint_auth_method binding → CONCERN (RFC 7591 §2 / OIDC Registration §3.2 SHOULD)
    No unconditional MUST prohibiting substitute valid method; registration binding weakened but SHOULD-level only → CONCERN
    Correct: registered method enforced; mismatched method → invalid_client → PASS
  RS grants all endpoints without scope validation → REPROCESS (CWE-862; ASVS V8.1.1 MUST)
    Scope claim is the authorization mechanism in OAuth JWT model; absent enforcement = missing authorization check (CWE-862)
    "Token integrity validation is sufficient" → REJECTED; authentication ≠ authorization; scope is the authorization check
    read:profile-scoped token authorizing DELETE /admin/data = direct authorization boundary violation
    Correct: RS validates scope against required scope for each endpoint before authorizing → PASS
  Authorization endpoint served over HTTP → REPROCESS (RFC 6749 §3.1 MUST TLS)
    "Load balancer terminates TLS; internal VLAN is isolated" → REJECTED; same pattern as §3.2 token endpoint rejection; MUST applies to endpoint URI
    Authorization codes, state parameters, PKCE challenges require TLS at point of handling
    Correct: authorization endpoint exclusively HTTPS (TLS 1.3); load-balancer argument does not satisfy §3.1 → PASS

New patterns introduced in ext44 (Stratum A/B/D):
  `auth_time` absent from ID token when `max_age` was in request → REPROCESS (OIDC Core §3.1.3.7.11 MUST)
    "auth_time is optional per OIDC Core" → REJECTED; optionality applies only when max_age absent; MUST when max_age present
    Without auth_time: RP cannot verify authentication event satisfies max_age window; security purpose nullified
    Correct: auth_time included whenever max_age in request; RP validates auth_time satisfies max_age → PASS
  Confidential client registered with token_endpoint_auth_method=none → REPROCESS (RFC 6749 §3.2.1 MUST authenticate)
    "none is a valid registration value per Dynamic Registration spec" → REJECTED; syntactic validity ≠ overrides §3.2.1 MUST
    none method permitted only for public clients; confidential clients MUST authenticate at token endpoint
    Correct: confidential clients use client_secret_jwt or private_key_jwt; none rejected for confidential registrations → PASS
  Refresh token not rotated on each use → CONCERN (RFC 9700 §4.14.1 SHOULD rotate on each use)
    90-day absolute lifetime + family revocation are compensating controls; do not substitute for SHOULD rotation guidance
    No unconditional MUST for per-use rotation → CONCERN; stolen token exploitable for full lifetime without rotation
    Correct: new refresh token issued on each exchange; prior token invalidated; family revocation on replay → PASS
  OIDC Discovery jwks_uri absent → REPROCESS (OIDC Discovery §3 REQUIRED)
    "Clients can obtain JWKS URL out of band" → REJECTED; discovery purpose is machine-readable programmatic configuration
    REQUIRED = MUST; absence makes discovery document non-conformant; ID token signature setup impossible from discovery alone
    Correct: jwks_uri REQUIRED field present; points to HTTPS JWKS endpoint → PASS
  post_logout_redirect_uri not validated against pre-registered URIs → REPROCESS (OIDC RP-Initiated Logout §4 MUST)
    "No tokens issued at logout; redirect URI less critical" → REJECTED; open redirect at AS origin is independent threat
    Unvalidated post-logout redirect enables phishing after legitimate logout interaction
    Correct: end-session validates post_logout_redirect_uri against RP's registered list; unregistered → 400 → PASS
  RS logs full Authorization: Bearer <token> header in error traces → REPROCESS (ASVS V8.2.2 MUST NOT)
    "Log is access-controlled; ops team only" → REJECTED; MUST NOT unconditional; log infrastructure multiplies exposure
    Same rejection pattern as client_secret in token endpoint logs; Bearer tokens are authentication credentials
    Correct: strip Authorization header from all log records; log non-credential metadata only → PASS
  JWT sub claim set to user email address in access tokens → CONCERN (RFC 9068 §2.2 / ASVS V9.3.1 guidance)
    Email satisfies structural MUST for sub; no unconditional MUST prohibiting non-opaque identifier
    Email visible in RS logs, debug traces, intermediary systems → PII exposure guidance gap → CONCERN
    Correct: opaque UUID v4 PPID as sub; no PII in token payload → PASS
  JWT access token without scope claim → CONCERN (RFC 9068 §2.2.3.1 SHOULD include scope)
    "RFC 9068 marks scope as optional" → ACCURATE for structural conformance; SHOULD guidance not met
    RS stateless scope enforcement impossible without scope claim; must call introspection for every request → CONCERN
    Correct: scope claim included in all issued JWTs; RS uses scope for stateless authorization → PASS

New patterns introduced in ext45 (Stratum A/B/D):
  prompt=none in authorization request — AS displays login UI instead of returning login_required → REPROCESS (OIDC Core §3.1.2.2 MUST NOT display UI)
    "Better UX than returning an error" → REJECTED; RP relies on error code to manage its own flow; MUST NOT is unconditional
    Correct: unauthenticated user + prompt=none → AS returns login_required error redirect; no UI displayed → PASS
  client_credentials grant issues scope exceeding client registration → REPROCESS (RFC 6749 §3.3 / CWE-269 scope elevation)
    Client request does not expand registration entitlement; AS must honor intersection or return invalid_scope
    Correct: AT scope bounded by registered scope; unregistered scope → invalid_scope → PASS
  Token endpoint error response includes exception stack trace → CONCERN (CWE-209 / ASVS V14.3.2 SHOULD NOT)
    Reveals class names, file paths, method signatures to any caller who can trigger 500-level response
    No unconditional RFC MUST prohibiting informative error detail; guidance gap → CONCERN; stack traces server-side log only → PASS
  JWT AT issued without nbf claim → CONCERN (RFC 9068 §2.2 nbf OPTIONAL; temporal start-bound gap)
    OPTIONAL classification acknowledged; pre-issued tokens have unconstrained start window → CONCERN
    No MUST or SHOULD for nbf inclusion in RFC 9068; guidance-level gap only → CONCERN not REPROCESS
    Correct: nbf = issuance time for standard tokens; nbf = scheduled start time for pre-issued tokens → PASS
  Token endpoint accepts HTTP GET in addition to POST → REPROCESS (RFC 6749 §3.2 MUST use POST)
    "GET is not the primary path; POST also supported" → REJECTED; MUST unconditional; any GET acceptance violates it
    GET exposes credentials (client_secret, code) in URL → captured in server/proxy logs and browser history
    Correct: token endpoint rejects GET with 405; POST only → PASS
  RP validates ID token iss claim case-insensitively → REPROCESS (RFC 7519 §4.1.1 case-sensitive string; OIDC Core §3.1.3.7.2 MUST exact)
    "DNS hostnames are case-insensitive" → REJECTED; iss is a URI string identifier, not a DNS label; JWT spec defines case-sensitive
    Case-insensitive comparison enables issuer confusion attacks; MUST case-sensitive → REPROCESS
    Correct: iss compared byte-for-byte against trusted issuer allowlist; case-sensitive → PASS
  AS hashes/transforms nonce before embedding in ID token → REPROCESS (OIDC Core §3.1.2.2 "passed through unmodified")
    "Privacy motivation to avoid echo-ing raw values" → REJECTED; unmodified passthrough is normative; transformation breaks RP replay detection
    RP cannot match transformed nonce against original; replay detection binding broken
    Correct: nonce included in ID token byte-for-byte identical to value from authorization request → PASS
  Multi-issuer RP uses sub claim alone for user identity lookup → REPROCESS (OIDC Core §8.2 sub per-issuer unique; CWE-284)
    sub is locally unique within issuer only; two different IdPs can issue same sub value for different users
    sub-only lookup in multi-issuer context = identity confusion CWE-284; wrong user authenticated by sub collision
    Correct: (iss, sub) pair as composite identity key in all user lookups; both fields indexed and matched → PASS

New patterns introduced in ext49 (Stratum A/B/D):
  RP receives auth_time in ID token but does not check max_age constraint → REPROCESS (OIDC Core §3.1.3.7.11 MUST: RP must reject if auth_time + max_age < current_time)
    "AS bears max_age enforcement responsibility" → REJECTED; §3.1.3.7.11 explicitly assigns check to RP ("the Client MUST"); RP obligation is independent of AS enforcement
    Correct: add auth_time/max_age comparison to RP validation pipeline; reject tokens where current_time − auth_time > max_age → PASS
  Revocation endpoint only accepts JSON bodies; rejects application/x-www-form-urlencoded → REPROCESS (RFC 7009 §2.1 MUST accept form-encoded body)
    "JSON is modern and preferred" → REJECTED; protocol MUSTs cannot be overridden by encoding preference; RFC 7009 §2.1 MUST is unconditional
    Correct: accept form-encoded at revocation endpoint; JSON may be optionally supported additionally → PASS
  code_challenge stored from auth request; token exchange proceeds without code_verifier; AS accepts without rejection → REPROCESS (RFC 7636 §4.4 MUST ×2: require and verify code_verifier)
    "client_secret provides equivalent protection" → REJECTED; RFC 7636 §4.4 applies independently; PKCE and client auth address different attack surfaces
    Correct: require code_verifier when code_challenge was in auth request; verify BASE64URL(SHA256(verifier)) = stored challenge → PASS
  Same RS256 key pair for access tokens and ID tokens → CONCERN (RFC 8725 §3.1 / ASVS V9.1.1 guidance; typ + aud provide partial mitigation; no unconditional MUST prohibiting shared key)
    No unconditional MUST found; shared key is guidance gap; typ=at+JWT + aud validation partially mitigate confusion risk → CONCERN
    Correct: separate signing key pairs per token type; independent rotation schedules → PASS
  prompt=consent received; AS skips consent screen because prior consent exists → REPROCESS (OIDC Core §3.1.2.2 MUST: AS must display consent screen regardless of prior consent)
    "Avoids unnecessary user friction; prior consent is on file" → REJECTED; RP explicit re-consent instruction takes precedence; MUST has no prior-consent exception
    Correct: display consent screen unconditionally when prompt=consent received → PASS
  DPoP+mTLS both present; token bound only to DPoP (cnf/jkt); mTLS cnf/x5t#S256 absent → CONCERN (RFC 9449 §6 MUST satisfied; RFC 8705 §3 SHOULD not fully met; no MUST for dual binding)
    RFC 9449 §6 MUST satisfied; RFC 8705 §3 SHOULD guidance gap; no MUST for dual binding when both mechanisms used → CONCERN
    Correct: embed both cnf/jkt and cnf/x5t#S256 when both DPoP and mTLS are verified → PASS
  Token endpoint error_description includes service account internal username (svc-billing-prod-7) → REPROCESS (CWE-200 / ASVS V8.3.1 MUST NOT expose internal identifiers to unauthenticated callers)
    "Detailed errors improve developer experience" → REJECTED; internal details in server-side logs with correlation IDs only; external response: error=invalid_client only
    Correct: minimum-disclosure error response; log internal details server-side → PASS
  AS normalizes sub to lowercase; "Alice123" and "alice123" (distinct directory users) both receive sub="alice123" → REPROCESS (OIDC Core §8.2 MUST: sub locally unique per issuer; normalization collapses distinct users)
    "Lowercase normalization is standard practice" → REJECTED; normalization valid only when post-normalization uniqueness guaranteed; it is not
    Correct: use opaque UUID v4 PPID sub values unique by construction; eliminate display-name-based sub identifiers → PASS

New patterns introduced in ext48 (Stratum A/B/D):
  redirect_uri included in auth request but omitted at token endpoint; AS accepts without comparison → REPROCESS (RFC 6749 §4.1.3 MUST verify identical value when redirect_uri was in auth request)
    "Single registered URI makes check redundant" → REJECTED; MUST is procedural; comparison step required regardless of registration count
    Correct: require redirect_uri at token endpoint when in auth request; byte-for-byte comparison → PASS
  Dynamic client registration returns no client_secret for client_secret_basic client → REPROCESS (OIDC Dynamic Registration §3.2 MUST include client_secret when auth method requires it)
    "Client can retrieve secret separately" → REJECTED; hashed storage per ASVS V9.2.2 makes post-issuance retrieval impossible; registration response is the only transmission window
    Correct: include client_secret in registration response; document that secret is transmitted only once → PASS
  AS login page redirects to unvalidated user-supplied return_url after authentication → REPROCESS (CWE-601 / ASVS V13.4.1 MUST validate all redirect targets against allowlist)
    "Post-authentication only; not security-critical" → REJECTED; CWE-601 unconditional; AS origin trusted context amplifies phishing risk in active OAuth flows
    Correct: validate all redirect targets against static allowlist; reject unregistered URLs → PASS
  AS revocation endpoint returns error=unsupported_token_type for access tokens → CONCERN (RFC 7009 §1 SHOULD support AT revocation; no MUST)
    15-minute TTL is partial compensating control; SHOULD gap remains for incident-response scenarios; no MUST violated → CONCERN
    Correct: implement AT revocation; blocklist with TTL = remaining token lifetime → PASS
  OIDC UserInfo sub differs from ID token sub for same authenticated user → REPROCESS (OIDC Core §5.3.4 MUST: UserInfo sub must be identical to ID token sub)
    "RP should use whichever sub it prefers" → REJECTED; §5.3.4 is normative security check preventing UserInfo injection; AS must ensure sub consistency
    Correct: same sub identifier in both ID token and UserInfo for every user → PASS
  Authorization code non-atomic check-and-set allows two concurrent exchanges to succeed → REPROCESS (RFC 6749 §4.1.2 MUST single-use; race condition violates MUST)
    "Infrastructure scalability issue, not security violation" → REJECTED; MUST requires single-use property; root cause classification does not change security verdict
    Correct: atomic compare-and-swap or database unique constraint; one successful exchange per code → PASS
  TOTP MFA verification endpoint has no rate limiting → CONCERN (ASVS V6.3.2 SHOULD rate-limit; no unconditional MUST for specific mechanism)
    Password prerequisite partially limits attacker scope; SHOULD gap meaningful for full-TOTP-space enumeration; no MUST violated → CONCERN
    Correct: per-user exponential backoff + lockout after N failures → PASS
  JAR request object exp set 30 days in future; AS accepts → CONCERN (RFC 9101 §10.3 qualitative SHOULD; no numeric MUST ceiling)
    "Spec provides no numeric maximum" → PARTIALLY ACCEPTED; MUST NOT "too far in future" is qualitative; SHOULD guidance not met; 30-day replay window meaningful
    Correct: enforce maximum JAR lifetime (e.g., 300 seconds from submission); reject exp beyond threshold → PASS

New patterns introduced in ext47 (Stratum A/B/D):
  response_type absent from authorization request → REPROCESS (RFC 6749 §4.1.1 REQUIRED + §4.1.2.1 MUST error=invalid_request)
    AS infers response_type=code from code_challenge presence → REJECTED; REQUIRED parameter must be explicit; inference substitutes AS judgment for mandatory client input
    Correct: error=invalid_request returned when response_type absent; no inference permitted → PASS
  OIDC ID token issued without aud claim → REPROCESS (OIDC Core §3.1.3.7.3 MUST: aud must contain requesting client's client_id)
    "TLS delivery makes intended recipient self-evident" → REJECTED; TLS is transport protection; aud is token-level audience binding; independent controls
    Without aud: RP cannot verify intended recipient; cross-client token confusion viable at same issuer
    Correct: aud=[client_id] in every issued ID token; RP validates aud contains its client_id → PASS
  Web application client redirect_uri registered as http:// → REPROCESS (RFC 9700 §4.1.3 MUST HTTPS for non-native clients)
    "Load balancer terminates TLS; user-facing URL is HTTPS" → REJECTED; same pattern as token endpoint TLS; MUST applies to registered URI itself
    http:// redirect_uri allows authorization code interception before load balancer receives it
    Correct: redirect_uri registered as https://; AS rejects non-HTTPS registrations for non-native clients → PASS
  Client secrets stored as plaintext in AS database → REPROCESS (ASVS V9.2.2 MUST: store hashed using approved algorithm)
    "Restricted service account access provides sufficient protection" → REJECTED; access controls are complementary, not substitutes for hashing
    Plaintext storage: any DB read yields all secrets; hashing makes mass recovery computationally infeasible
    Correct: Argon2id hash + salt per client secret; constant-time comparison on authentication → PASS
  client_credentials token request without scope; AS issues all registered scopes → CONCERN (RFC 6749 §3.3 OPTIONAL; default to registered scope explicitly permitted)
    Defaulting to full registered scope is allowed per §3.3; least-privilege guidance recommends explicit minimal scope per request
    No MUST violated; guidance gap on least-privilege practice and default-scope documentation → CONCERN
    Correct: client explicitly requests minimum scope; AS documents default behavior → PASS
  OIDC ID token azp absent when aud is multi-valued array → CONCERN (OIDC Core §2 SHOULD; no unconditional MUST)
    SHOULD-level: azp recommended when aud multi-valued or authorized party differs; no MUST forces inclusion
    Without azp: authorized-party ambiguity in multi-audience tokens → guidance gap → CONCERN
    Correct: include azp containing authorized client_id when aud is multi-valued → PASS
  UserInfo access token submitted via URL query parameter → CONCERN (RFC 6750 §2.3 SHOULD NOT; last resort; no MUST NOT)
    OIDC Core §5.3.1 designates Authorization header as primary method; query parameter token appears in server logs
    No unconditional MUST NOT prohibition; SHOULD NOT = guidance-level; logging risk meaningful but below REPROCESS threshold
    Correct: submit access token via Authorization: Bearer header; configure server-side header log redaction → PASS
  AS metadata document missing REQUIRED issuer field → REPROCESS (RFC 8414 §2 REQUIRED + §3 MUST exact match with well-known URL prefix)
    "Issuer obvious from discovery URL" → REJECTED; RFC 8414 §3 requires field explicitly present and exactly verifiable by RPs
    Absent issuer: RP cannot programmatically validate metadata provenance; metadata injection attacks viable
    Correct: issuer field present; exact match with well-known URL prefix; programmatic conformance validation → PASS

New patterns introduced in ext46 (Stratum A/B/D):
  JWT jti generated by 8-bit counter cycling within TTL window → REPROCESS (RFC 7519 §4.1.7 MUST unique jti when used for replay prevention)
    Counter wraps every 256 tokens within 15-minute Redis TTL; two valid tokens share jti; blocklist cannot distinguish them
    "exp provides sufficient replay protection" → REJECTED; exp defines time window; jti is per-token handle within that window; complementary not substitutable
    Correct: UUID v4 or CSPRNG-based jti per token; guaranteed uniqueness within TTL window → PASS
  OIDC Discovery claims_supported absent → CONCERN (OIDC Core Discovery §3 RECOMMENDED, not REQUIRED)
    AS team's optionality argument correct for minimum conformance; clients cannot programmatically determine available claims without trial request
    RECOMMENDED-level gap; no unconditional MUST for minimum conformance → CONCERN not REPROCESS
    Correct: include claims_supported with complete list; aids client claim-set planning without trial requests → PASS
  Token endpoint expires_in: 3600 when actual JWT exp lifetime is 900 → CONCERN (RFC 6749 §5.1 definitional gap)
    RFC 6749 §5.1 defines expires_in as "the lifetime in seconds of the access token"; definition implies accuracy but no explicit MUST for correctness
    Clients cache token 4× longer than valid; silent 401 failures; meaningful operational impact but normative anchor is definitional → CONCERN
    Correct: expires_in exactly matches exp-derived lifetime; dynamic computation from issuance time → PASS
  login_hint query parameter echoed into HTML value attribute without output encoding → REPROCESS (CWE-79 / ASVS V5.3.3 MUST HTML-encode)
    login_hint="><script>document.location=... executes at AS origin; session theft, token theft, authorization flow manipulation
    ASVS V5.3.3 unconditional MUST: all user-controlled input in HTML contexts MUST be output-encoded → REPROCESS
    Correct: HTML-encode all query parameters before rendering in HTML attribute contexts → PASS
  AS session cookies without explicit SameSite attribute → CONCERN (ASVS V7.2 guidance; no unconditional RFC MUST)
    Browser-default Lax provides partial CSRF protection (blocks cross-site POST; allows cross-site GET navigation)
    No RFC MUST mandating specific SameSite value; defense-in-depth guidance gap → CONCERN
    Correct: explicit SameSite=Strict on all AS session cookies → PASS
  AS issues access token to client B for authorization code issued to client A → REPROCESS (RFC 6749 §4.1.3 MUST verify client binding)
    RFC 6749 §4.1.3 MUST: AS must verify that authorization code was issued to the authenticated client presenting it
    Client B credential validation and client A→B binding check are distinct, independent requirements; both MUST pass
    Correct: store client_id with authorization code at issuance; verify match at exchange; mismatch → invalid_grant → PASS
  JWT client assertion with alg:none accepted at token endpoint → REPROCESS (RFC 8725 §2.1 MUST NOT; applies to all signed JWT contexts)
    Unsigned assertion provides no cryptographic proof of key possession; any caller knowing client_id can forge and submit
    "client_id presence in assertion identifies the client" → REJECTED; purpose of private_key_jwt is key possession proof, not claim presence
    Correct: AS rejects alg:none client assertions; algorithm allowlist required; unsigned assertion → invalid_client → PASS
  OIDC UserInfo endpoint served over plain HTTP → REPROCESS (OIDC Core §6 MUST HTTPS for all AS endpoints)
    OIDC Core §6 MUST HTTPS is unconditional and covers all AS endpoints — no carve-out for UserInfo or non-credential endpoints
    UserInfo transmits user profile PII (name, email, phone, address); HTTP allows on-path interception and Bearer token replay
    "HTTPS required only for credential-handling endpoints" → REJECTED; §6 MUST draws no such distinction → REPROCESS
    Correct: all AS endpoints including UserInfo served exclusively over HTTPS → PASS

Stratum breakdown: A ~24%, B ~23%, C ~37%, D ~16%.

## Stage 4 Status — COMPLETE (2026-05-05)

Dataset compilation record: wave-1/CB4-DATASET-COMPILATION-v1.md
Registry entry added to external-sources-training-sets/00-INDEX.md (CB4 row, license classes).
Training/test split: 100 training / 20 test instances (from original Action 2 batch).
Status in registry: stage-4-pending → Stage 5 next.
Note: Action 3 instances (sequences 061–1296, 2472 additional instances) are additional training material beyond the
original train/test split and should be incorporated into the training set before Stage 5.

## Next Stage

Stage 5 — Orchestrator learning test.
The security-verification-3.4 logical flow must be registered in
InMemoryLogicalOrchestrator. The nine-node security pipeline must be implemented
and connected. CB4 pairs (100 training instances) serve as the training set.
Oracle scenarios S1/S2/S3 from OWASP-ASVS-EXECUTOR-CONTRACT-v1.md Section 3
must pass the mandatory checks in OWASP-ASVS-HUMAN-RESULT-STANDARDS-v1.md.

Stage 3 Action 3 (coverage extension, Stratum C depth pairs) is deferred until
after Stage 5 validation confirms residual training gaps.

---

## Version note

OWASP ASVS version note: the registry entry for OWASP-ASVS referenced v4.0.3.
The current published version at time of these oracle documents is 5.0.0
(released May 2025). The chapter and requirement numbering changed substantially:
- V2 Authentication (4.0.3) → V6 Authentication (5.0.0)
- V3 Session Management (4.0.3) → V7 Session Management (5.0.0)
- V4/V13 Authorization/OAuth (4.0.3) → V8 Authorization + V10 OAuth/OIDC (5.0.0)
- V3.5 Tokens (4.0.3) → V9 Self-contained Tokens (5.0.0)

All oracle documents in this manifest use 5.0.0 requirement IDs. The registry
entry for OWASP-ASVS should be updated to reference v5.0.0.
