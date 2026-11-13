# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Cross-source combinations (ext21 — ext28 cross-source)
# Sequences: CB4-3.4-741 through CB4-3.4-750
# Instances: 20 (10 base pairs × 2 phrasings each)
# Stratum: B/C (cross-source pattern combinations)
# Running total after this file: 1500 instances (750 base pairs)

---

## Pair[CB4-3.4-741]

### A
Cross-source input — ext28 combination (AS discovery over HTTP + RS aud validation absent):

A system has two concurrent security gaps: (1) the authorization server's discovery endpoint is served over plain HTTP at http://as.example.com/.well-known/oauth-authorization-server; the document lists the token endpoint and JWKS URI; (2) the three resource servers in the system (api.example.com, payments.example.com, admin.example.com) each validate JWT signature and expiry on incoming access tokens but do not validate the aud claim; any valid token from the AS is accepted by any RS regardless of its aud value. Team: "Both are low-risk in our environment — the discovery endpoint is internal-only, and our single trusted AS means any valid token is authorized."

### B
REPROCESS. Two independent MUST-level violations identified. The "internal-only" and "single trusted AS" arguments are both rejected.

Finding 1 — RFC 8414 §3 MUST HTTPS (discovery endpoint transport): HTTP transport for the discovery document allows on-path adversaries within the internal network to substitute the document in transit, replacing token endpoint and JWKS URI values with attacker-controlled URLs. The "internal-only" argument is rejected — the same pattern applies here as for JWKS HTTP transport: internal networks have on-path threats (VLAN hopping, misconfigured switch ports, compromised internal hosts). RFC 8414 §3 MUST HTTPS is unconditional. REPROCESS trigger.

Finding 2 — RFC 9068 §4 + RFC 8725 §3.9 (RS aud validation absent): With three resource servers serving distinct authorization scopes, the absence of aud validation enables audience confusion attacks: a valid token for api.example.com (low-privilege) is accepted at admin.example.com (high-privilege) without token forgery. The "single trusted AS = authorized token" argument conflates token authenticity (issuer-based) with authorization scope (aud-based). The AS trusting assertion covers token signing; it does not cover which RS the token was intended for. REPROCESS trigger.

Two independent REPROCESS triggers. REPROCESS.

### Reason
Both RFC 8414 §3 and RFC 9068 §4 are MUST-level requirements violated by distinct gaps. The "internal-only" and "single AS" arguments address different concerns than the requirements being violated, leaving both MUST violations in place.

### Phrasing-B
REPROCESS — two independent MUST violations. Neither argument addresses the actual requirement violated.

RFC 8414 §3: HTTP discovery enables endpoint URL substitution within the internal network — the internal-only scope does not prevent on-path attacks from internal threats. MUST HTTPS: violated.

RFC 9068 §4: RS accepts tokens with aud=api.example.com at admin.example.com. Single trusted AS does not mean all tokens are authorized for all RSs — the AS deliberately issues distinct aud values per RS to enforce authorization boundaries. Ignoring aud nullifies that boundary. Audience confusion attack viable. REPROCESS.

---

## Pair[CB4-3.4-742]

### A
Cross-source input — ext28 combination (correct discovery + correct RS aud validation):

A system implements: (1) AS discovery at https://as.example.com/.well-known/oauth-authorization-server; issuer field "https://as.example.com" matches the discovery base URL; all listed endpoint URLs use HTTPS; (2) each resource server (api.example.com, payments.example.com, admin.example.com) validates the aud claim on every incoming access token — each RS confirms its own registered identifier appears in the token's aud array before processing; tokens with mismatched aud are rejected with 401.

### B
PASS. Both discovery integrity and RS audience validation are correctly implemented.

Discovery: HTTPS delivery with valid certificate; issuer matches discovery base URL; all endpoint URLs use HTTPS. RFC 8414 §3 MUST HTTPS satisfied. OpenID Connect Discovery §4.3 issuer binding satisfied.

RS aud validation: each RS checks its own identifier in the token aud claim before any other processing. A token with aud=["api.example.com"] is rejected at payments.example.com and admin.example.com with 401. Audience confusion attack is not viable — legitimate tokens are constrained to their intended recipient RSs. RFC 9068 §4 and RFC 8725 §3.9 requirements satisfied.

No findings.

### Reason
RFC 8414 §3 and RFC 9068 §4 both satisfied. Correct implementation of both controls. PASS.

### Phrasing-B
PASS — discovery served correctly over HTTPS with issuer match; RS aud validation enforced per recipient.

AS discovery: HTTPS, issuer = discovery base URL, all endpoints HTTPS. RFC 8414 §3 satisfied.
RS aud: each RS validates its own identifier in aud before processing. Mismatched-aud tokens rejected with 401. Audience confusion attack eliminated. RFC 9068 §4 and RFC 8725 §3.9 satisfied. PASS.

---

## Pair[CB4-3.4-743]

### A
Cross-source input — ext28 combination (OIDC issuer mismatch + JWT without kid during rollover):

A review of an OIDC system finds: (1) the OIDC discovery document at https://idp.example.com/.well-known/openid-configuration returns issuer: "https://idp.internal.example.com" — a mismatch with the discovery fetch URL; (2) during key rollover, both the old and new RSA keys are active in the JWKS; issued JWTs do not include a kid header; resource servers must try all active keys per validation attempt. Team: "The issuer naming will be aligned in the next release cycle, and the kid issue is minor — validation always succeeds."

### B
REPROCESS. One MUST violation and one SHOULD violation identified. REPROCESS is determined by the MUST violation; the SHOULD violation adds a CONCERN finding that is absorbed.

Finding 1 — OpenID Connect Discovery §4.3 MUST (issuer mismatch): The issuer field "https://idp.internal.example.com" does not match the discovery base URL "https://idp.example.com". This is a MUST requirement with no grace period or "next release cycle" exception. Downstream consequence: tokens issued with iss="https://idp.internal.example.com" will fail iss validation for clients that configured their expected issuer from the external discovery URL. The mismatch creates both a security gap (§4.3 binding broken) and an operational failure (token validation inconsistency). REPROCESS trigger.

Finding 2 — RFC 7515 §4.1.4 SHOULD (kid absent during multi-key rollover): The absence of kid during rollover with two active keys forces RS to try all keys per token. This is a SHOULD-level gap — token validation succeeds eventually; no MUST violated. The team's "always succeeds" characterization is accurate but ignores the operational risk of absent cache-refresh signals. CONCERN. Absorbed into REPROCESS determination.

Overall verdict: REPROCESS (from Finding 1). The "next release cycle" deferral does not affect compliance status of a current MUST violation.

### Reason
REPROCESS overrides CONCERN — the graduation rule in the escalation direction. A MUST violation (§4.3 issuer mismatch) determines the verdict regardless of the count or severity of additional CONCERN findings. "Next release cycle" is not a compliance argument for a current MUST violation.

### Phrasing-B
REPROCESS — §4.3 MUST violation overrides the CONCERN-level kid finding.

OpenID Connect Discovery §4.3: issuer "https://idp.internal.example.com" ≠ discovery URL base "https://idp.example.com". MUST identical: violated. "Next release cycle" does not satisfy a present-tense MUST requirement. REPROCESS trigger.

RFC 7515 §4.1.4: kid absent during rollover. SHOULD-level gap. RS tries both active keys. Validation succeeds. CONCERN — absorbed by REPROCESS determination.

Verdict: REPROCESS.

---

## Pair[CB4-3.4-744]

### A
Cross-source input — ext28 combination (correct OIDC issuer + JWT with kid):

An OIDC system under review: (1) discovery at https://idp.example.com/.well-known/openid-configuration returns issuer: "https://idp.example.com" — exact match to discovery base URL; (2) during key rollover, issued JWTs carry kid: "rsa-2025" in the JOSE header, matching the current signing key in the JWKS; RS performs direct key lookup by kid; if kid not found in cached JWKS, RS re-fetches once before failing.

### B
PASS. OIDC issuer binding correct and JWT kid-directed key lookup correctly implemented.

Issuer binding: "https://idp.example.com" matches discovery base URL "https://idp.example.com". OpenID Connect Discovery §4.3 MUST satisfied. Token iss values will be consistent with the client's validated expected issuer. No downstream validation inconsistency.

JWT kid: kid="rsa-2025" present in all issued tokens. RS reads kid, performs O(1) JWKS lookup, validates with the matching key. Kid-not-found in cache triggers targeted JWKS re-fetch — deterministic response to rollover rather than ambiguous signature failure. RFC 7515 §4.1.4 SHOULD satisfied.

No findings.

### Reason
Both §4.3 and RFC 7515 §4.1.4 satisfied. Clean PASS across both evaluation domains.

### Phrasing-B
PASS — correct issuer binding and kid-directed key selection confirmed.

§4.3: issuer = discovery base URL. Match confirmed. Tokens consistent. RFC 7515 §4.1.4 SHOULD: kid header in every JWT; O(1) JWKS key lookup; cache-refresh on kid-not-found. PASS.

---

## Pair[CB4-3.4-745]

### A
Cross-source input — ext28 combination (token endpoint no-auth for confidential clients + grant type not enforced):

A security review of the authorization server's token endpoint reveals two policy failures: (1) registered confidential clients can obtain tokens by presenting only their client_id — no client_secret required; the endpoint also accepts requests without any authentication; (2) the authorization server does not validate the grant_type in token requests against the client's registered grant types — a client registered solely for authorization_code can successfully request client_credentials tokens. Team: "We authenticate users through the authorization code flow — the token endpoint is for clients we trust."

### B
REPROCESS. Two independent MUST-level violations. The "clients we trust" argument does not address either requirement.

Finding 1 — RFC 6749 §2.3 MUST (confidential client authentication absent): Confidential clients registered with client_secret must authenticate at the token endpoint. Accepting requests with only client_id — or no credentials at all — nullifies the confidential client classification. Any caller who knows a client_id (observable in any authorization code flow) can impersonate the registered client. REPROCESS trigger.

Finding 2 — RFC 6749 §3.1.1 + CWE-269 (grant type not bounded per client): A client registered for authorization_code only can successfully use client_credentials. This bypasses the user consent requirement — the client obtains machine-to-machine tokens that were never authorized through user consent. The "trusted client" framing addresses client identity (partially — undermined by Finding 1) but not capability authorization. Client authentication (who the client is) is separate from grant type authorization (what flows the client may use). REPROCESS trigger.

Both violations are structurally independent. REPROCESS.

### Reason
RFC 6749 §2.3 and RFC 6749 §3.1.1 are both MUST-level requirements violated simultaneously. The "trusted clients" argument addresses trust conceptually but does not satisfy either enforcement requirement.

### Phrasing-B
REPROCESS — two RFC 6749 MUST violations. Trust argument does not satisfy enforcement requirements.

RFC 6749 §2.3: confidential clients MUST authenticate. Token endpoint accepts client_id only. Client_id is observable in OAuth flows — any party can impersonate. MUST violated.

RFC 6749 §3.1.1: grant types must be validated against client registration. Authorization_code-only client successfully uses client_credentials — user consent bypassed. MUST violated.

"Clients we trust" is not an enforcement mechanism for either requirement. REPROCESS.

---

## Pair[CB4-3.4-746]

### A
Cross-source input — ext28 combination (token endpoint auth enforced + grant type enforced):

Authorization server controls confirmed: (1) the token endpoint validates client credentials on every request — confidential clients must present client_id + client_secret (or mTLS certificate, or JWT assertion per registration); requests without valid credentials receive 401 with WWW-Authenticate; the token_endpoint_auth_method is enforced per client registration; (2) the token endpoint validates the requested grant_type against the client's registered grant_types list before issuing a token — a client registered for ["authorization_code"] receives 400 (error: unauthorized_client) if it requests client_credentials; each grant type is independently bounded per client.

### B
PASS. Both client authentication enforcement and grant type enforcement are correctly implemented.

Client authentication: each confidential client's registered auth method is enforced — no alternative path accepts unauthenticated or under-authenticated requests. 401 responses correctly signal authentication failure to clients that do not provide valid credentials. RFC 6749 §2.3 MUST satisfied.

Grant type enforcement: the token endpoint checks the requested grant_type against the client's registered grant_types list as a separate step from client authentication. Client identity (authentication) is established first; grant type capability (authorization) is validated second. Correct two-step model: clients that authenticate successfully but request an unauthorized grant type receive 400 (unauthorized_client), not a token. RFC 6749 §3.1.1 satisfied. CWE-269 privilege escalation via grant confusion eliminated.

No findings.

### Reason
Both RFC 6749 §2.3 and §3.1.1 satisfied with independent enforcement steps. The two-step model (authenticate then authorize by grant type) correctly separates identity verification from capability authorization.

### Phrasing-B
PASS — confidential client auth and per-client grant type enforcement both confirmed.

Token endpoint: client_secret (or registered alternative) validated per client; 401 on any missing or invalid credential. RFC 6749 §2.3 MUST satisfied.

Grant type: requested grant_type validated against registered list; 400 (unauthorized_client) if not registered. Authentication success does not confer grant type capability — separate enforcement step. RFC 6749 §3.1.1 satisfied. PASS.

---

## Pair[CB4-3.4-747]

### A
Cross-source input — ext28 + PO.1 escalation (OIDC issuer mismatch traceable to training gap):

A post-incident analysis of an OIDC integration failure reveals: (1) the OIDC discovery document at https://idp.example.com/.well-known/openid-configuration returns issuer: "https://idp.internal.example.com"; clients that validate per OpenID Connect Discovery §4.3 reject the discovery document; clients that do not validate accept it but receive tokens with a mismatched iss value; (2) SSDF PO.1 audit finds: the engineering team has no formal OIDC security training; the developer who configured the IdP issuer value states they were unaware that the issuer field must match the discovery URL; they set the issuer to the internal hostname because "that's what the server is called internally"; the gap is directly traceable to absence of OIDC protocol training.

### B
REPROCESS. OpenID Connect Discovery §4.3 MUST violated, with the violation directly traceable to a SSDF PO.1 training absence.

Primary finding — OpenID Connect Discovery §4.3 MUST: issuer "https://idp.internal.example.com" does not match discovery base URL "https://idp.example.com". The MUST requirement is unconditional. Production impact: clients implementing §4.3 validation reject the discovery document; clients not implementing §4.3 accept mismatched tokens that subsequently fail iss validation. Both outcomes impair production OIDC functionality and security.

PO.1 escalation: SSDF PO.1 requires the organization to establish security training for personnel with security-relevant responsibilities. The developer responsible for IdP configuration explicitly lacked knowledge of the §4.3 issuer-URL binding requirement. This is not an operational oversight — it is a demonstrable knowledge gap that would have been addressed by OIDC protocol training. The causal chain (no training → developer unaware of §4.3 → §4.3 MUST violated in production) satisfies the escalation condition. PO.1 gap escalates from CONCERN to REPROCESS because the training absence is the proximate cause of a MUST-level production violation.

REPROCESS on two grounds: (1) §4.3 MUST directly violated; (2) PO.1 training absence traceable to production MUST violation — escalation rule applies.

### Reason
§4.3 is already REPROCESS independently. PO.1 escalation provides a second, independent REPROCESS ground. The developer's explicit statement of unawareness establishes the causal chain without ambiguity.

### Phrasing-B
REPROCESS — §4.3 MUST violated; PO.1 absence confirmed proximate cause.

OpenID Connect Discovery §4.3: issuer does not match discovery URL. MUST requirement violated. Clients conforming to §4.3 reject the discovery document. Tokens carry iss mismatched to external discovery URL — downstream validation failures.

PO.1 escalation: developer who configured IdP issuer was unaware of §4.3. Organization has no formal OIDC training program. The configuration error is a direct consequence of the knowledge gap, not a typo or oversight. SSDF PO.1 requires training for security-critical roles. Configuring an IdP's issuer is a security-critical operation. Training absence + traceable production MUST violation = REPROCESS (escalation from what would be CONCERN for PO.1 alone). REPROCESS.

---

## Pair[CB4-3.4-748]

### A
Cross-source input — ext28 + ext18 combination (JWT without kid + client secret never rotated):

Two operational security gaps identified in a review: (1) JWTs are issued without kid headers; the JWKS currently has two active keys during a rolling rotation; RS validation iterates all keys per token (RFC 7515 §4.1.4 SHOULD not met); (2) the authorization server's registered confidential clients have held the same client_secret values since initial registration three years ago; no rotation policy exists; the team states "the spec doesn't require a rotation schedule." Reviewer notes: "Two deficiencies found in the same system."

### B
CONCERN. Two SHOULD-level gaps identified. No REPROCESS trigger present. N CONCERNs do not escalate to REPROCESS — graduation rule applies.

Finding 1 — RFC 7515 §4.1.4 SHOULD (kid absent during multi-key rollover): Token validation succeeds by iterating all active keys, but the absence of kid creates operational risk — cache-refresh signals are absent, and validation latency scales with JWKS size. SHOULD-level gap. CONCERN.

Finding 2 — BCP 2.1.1 / key hygiene (client secret never rotated): RFC 6749 does not specify a mandatory rotation interval for client secrets. BCP 2.1.1 recommends periodic rotation for long-lived credentials. Three years without rotation increases the exposure window if the secret is compromised — any compromise in that period has gone undetected by the absence of rotation. The team's "spec doesn't require it" statement is accurate — this is a SHOULD/recommended practice gap, not a MUST violation. CONCERN.

Combined verdict: CONCERN. Two SHOULD-level gaps; graduation rule (N CONCERNs ≠ REPROCESS) applies.

### Reason
Both findings are SHOULD-level — RFC 7515 §4.1.4 SHOULD and BCP 2.1.1 recommended practice. Neither triggers REPROCESS. The graduation rule explicitly prevents N CONCERNs from escalating.

### Phrasing-B
CONCERN — two SHOULD/recommended-practice gaps. Graduation rule applied.

RFC 7515 §4.1.4 SHOULD: kid absent, two active JWKS keys, RS iterates both per token. Operational risk without MUST violation. CONCERN.

Client secret rotation: three-year static secret. No RFC 6749 MUST for rotation frequency — the "spec doesn't require it" statement is accurate. BCP recommends rotation for long-lived credentials. Multi-year static secret is a hygiene gap. CONCERN.

Two CONCERNs. No REPROCESS. Verdict: CONCERN.

---

## Pair[CB4-3.4-749]

### A
Cross-source input — ext28 + ext24 combination (RS aud validation absent + access token in URL query string):

A mobile application review uncovers two token security violations: (1) the mobile app passes access tokens as URL query parameters — GET /api/resource?access_token=eyJ...; the app also enables full-URL debug logging in production, meaning complete token values appear in server access logs and mobile OS URL caches; (2) the resource server accepts the token from the query parameter and validates only signature and expiry — the aud claim is not checked; the RS accepts any valid AS-issued token regardless of its intended audience. Team: "The URL token is a legacy integration pattern, and all our RSs trust the same AS."

### B
REPROCESS. Two independent MUST-level violations. Neither legacy justification nor single-AS trust argument modifies MUST obligations.

Finding 1 — RFC 6750 §2.3 SHOULD NOT + V9.3.1 MUST NOT (access token in URL + debug logging): Access token in URL query parameter is RFC 6750 §2.3 SHOULD NOT — the weakest violation. However, full-URL debug logging in production transforms this to a MUST NOT violation (V9.3.1): tokens appear in server logs, mobile OS URL history, browser navigation logs, and any log aggregator in the pipeline. The SHOULD NOT transport combined with MUST NOT logging exposure = REPROCESS. The "legacy integration" argument does not satisfy a MUST NOT logging requirement. REPROCESS trigger.

Finding 2 — RFC 9068 §4 + RFC 8725 §3.9 (RS aud validation absent): Any valid AS-issued token is accepted at this RS regardless of aud value. Audience confusion attack: a token issued for a low-privilege RS is accepted at this RS. The "single trusted AS" argument conflates token authenticity with authorization scope — the AS issues distinct aud values to define per-RS authorization boundaries; ignoring aud nullifies those boundaries. REPROCESS trigger.

Two independent REPROCESS triggers. REPROCESS.

### Reason
SHOULD NOT transport + MUST NOT logging = REPROCESS (the combination elevates the transport SHOULD NOT to a MUST violation through logging exposure). RS aud absent is independently REPROCESS. Two triggers from distinct standards.

### Phrasing-B
REPROCESS — access token URL transport + production logging exposure, and RS aud validation absent.

RFC 6750 §2.3 SHOULD NOT access token in URL: by itself a CONCERN-level finding. V9.3.1 MUST NOT: full-URL debug logging in production exposes complete token values in server logs, OS URL cache, log aggregators. The combined violation (SHOULD NOT transport + MUST NOT exposure) = REPROCESS. Legacy-pattern argument rejected for MUST NOT.

RFC 9068 §4: RS accepts any AS-signed token without aud validation. Token with aud=["api.low-priv.example.com"] is accepted at this RS. Audience confusion viable. Single-AS trust does not address per-RS authorization scope. REPROCESS.

---

## Pair[CB4-3.4-750]

### A
Cross-source input — ext28 comprehensive PASS (all discovery, kid, client auth, and RS aud controls correct):

A complete implementation review covers all ext28 domains: (1) AS discovery at https://as.example.com/.well-known/oauth-authorization-server; issuer "https://as.example.com" matches discovery base URL; all endpoints HTTPS; valid CA certificate. (2) OIDC discovery at https://idp.example.com/.well-known/openid-configuration; issuer "https://idp.example.com" matches; all endpoints HTTPS. (3) JWTs issued with kid header matching the active signing key; RS performs O(1) key lookup; kid-not-found triggers single targeted JWKS re-fetch. (4) Token endpoint enforces client authentication per registered method; confidential clients must present valid credentials; no unauthenticated path exists. (5) Token endpoint validates grant_type against client registration; unauthorized_client returned for unregistered grant types. (6) All resource servers validate aud claim first — each RS confirms its identifier in the token aud array before processing; mismatched-aud tokens rejected with 401.

### B
PASS. All ext28 security controls comprehensively and correctly implemented. No findings.

Discovery integrity: RFC 8414 §3 MUST HTTPS satisfied at both AS and OIDC metadata endpoints. Issuer fields match discovery base URLs — OpenID Connect Discovery §4.3 MUST satisfied for OIDC; RFC 8414 issuer binding correct for OAuth AS.

JWT key selection: kid header in all tokens enables O(1) RS key lookup and targeted cache refresh. RFC 7515 §4.1.4 SHOULD satisfied.

Client authentication: confidential client auth method enforced per registration; no unauthenticated token requests accepted. RFC 6749 §2.3 MUST satisfied.

Grant type enforcement: grant_type validated against registered list per client; authentication success does not confer grant type capability. RFC 6749 §3.1.1 satisfied.

RS aud validation: each RS validates its identifier in the token aud claim before any processing; audience confusion attack eliminated. RFC 9068 §4 and RFC 8725 §3.9 satisfied.

No REPROCESS triggers. No CONCERN findings.

### Reason
All five ext28 control domains — discovery HTTPS + issuer binding, JWT kid, client auth enforcement, grant type enforcement, RS aud validation — are implemented at or above their normative requirement level. Comprehensive PASS.

### Phrasing-B
PASS — all ext28 domains correctly implemented. No findings across all evaluation points.

(1) RFC 8414 §3 MUST HTTPS: satisfied at AS and OIDC discovery endpoints. Issuer fields match discovery URLs — §4.3 binding intact.
(2) RFC 7515 §4.1.4 SHOULD kid: kid header in all JWTs; O(1) RS key lookup; cache-refresh on kid-not-found.
(3) RFC 6749 §2.3 MUST: confidential client auth enforced; no unauthenticated path to token issuance.
(4) RFC 6749 §3.1.1: grant_type validated against registered list; unauthorized_client on mismatch.
(5) RFC 9068 §4 + RFC 8725 §3.9: RS aud validation as first step; mismatched-aud tokens rejected with 401.

All controls meet or exceed normative requirements. PASS.
