# CB4 Stage 3 — Node Pipeline Chain Set ext31
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-967 through CB4-3.4-974 (8 base pairs × 2 phrasings = 16 instances)
# Full 9-node pipeline traces for all 8 ext37 arbiter patterns
# Running total after this file: 120 + (974 − 60) × 2 = 1948 instances (974 base pairs)
# Domains: client_credentials+refresh_token, OIDC max_age, nbf RS validation, wildcard aud

---

## Pair[CB4-3.4-967-A]

**Territory:** T-rfc-oauth2
**Domain:** client_credentials grant response includes refresh_token (RFC 6749 §4.4.3 MUST NOT)
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: OAuth 2.0 token endpoint response for grant_type=client_credentials. Security sub-domain: token response content restrictions, RFC 6749 §4.4.3. Input routed to grant type response validation branch.

[Node 2 — Domain Translator] Standard terminology: The client credentials grant (RFC 6749 §4.4) is a two-party flow — the client authenticates on its own behalf. RFC 6749 §4.4.3 defines the access token response for this grant type and explicitly restricts which fields may be present. `refresh_token` is explicitly excluded.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 6749 §4.4.3 restriction on refresh_token in client credentials grant. (2) Evaluate "lifecycle simplification" justification. (3) Determine verdict severity.

[Node 4 — Domain Processor] RFC 6749 §4.4.3: "If the access token request is valid and authorized, the authorization server issues an access token as described in Section 5.1. A refresh token MUST NOT be included." The normative term is MUST NOT — the strongest prohibition in RFC normative language. The rationale is design-based: a client in the client credentials flow holds its own credentials and can obtain a new token by re-presenting them; a refresh_token would be an additional long-lived secret without utility.

[Node 5 — Applier] Input includes refresh_token in client_credentials grant response. Directly violates RFC 6749 §4.4.3 MUST NOT. "Lifecycle simplification" argument evaluated: the service can re-authenticate to obtain new tokens using its credentials — no additional mechanism is needed or permitted. MUST NOT has no lifecycle-simplification exception.

[Node 6 — Result Translator] Finding: RFC 6749 §4.4.3 MUST NOT violated. refresh_token present in client credentials response. No exception clause applies. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected. Primary citation: RFC 6749 §4.4.3. Normative keyword: MUST NOT. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 6749 §4.4.3 MUST NOT: a refresh token must not be included in the client credentials grant access token response. The prohibition is unconditional; "lifecycle simplification" is rejected. Client re-authentication achieves the same renewal goal without creating an additional long-lived credential.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 6749 §4.4.3 | Normative level: MUST NOT | Gap: refresh_token included in client_credentials grant response | Remediation: Remove refresh_token from client_credentials responses; implement pre-expiry re-authentication in client services.

**B:**
REPROCESS. RFC 6749 §4.4.3 MUST NOT: the client credentials grant access token response must not include a refresh_token. The prohibition is unconditional and design-intentional — the client holds its credentials and can re-authenticate directly; a refresh token adds an unnecessary additional long-lived secret. "Lifecycle simplification" does not override a MUST NOT clause. Remediation: remove refresh_token from client_credentials responses.

**Reason:** RFC 6749 §4.4.3 MUST NOT violated. refresh_token in client_credentials response. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.4.3 MUST NOT issue refresh token with client credentials grant. No exception for lifecycle convenience. The client's own credentials are the re-authentication mechanism — a refresh_token is redundant and creates additional attack surface. REPROCESS.

---

## Pair[CB4-3.4-968-A]

**Territory:** T-rfc-oauth2
**Domain:** client_credentials response correctly omits refresh_token
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: OAuth 2.0 token endpoint response for grant_type=client_credentials. Validation of correct response composition.

[Node 2 — Domain Translator] Standard terminology: client_credentials flow response per RFC 6749 §4.4.3 + §5.1. Response contains: access_token, token_type=Bearer, expires_in=3600, scope. refresh_token absent. Services use pre-expiry re-authentication.

[Node 3 — Scheme Planner] Validation plan: (1) Verify refresh_token absent. (2) Verify all §5.1 MUST fields present. (3) Render verdict.

[Node 4 — Domain Processor] RFC 6749 §4.4.3 MUST NOT: refresh_token absent — satisfied. RFC 6749 §5.1 MUST: access_token, token_type, expires_in — all present. Pre-expiry re-authentication is a correct and efficient renewal pattern.

[Node 5 — Applier] All requirements satisfied. No MUST violations. No SHOULD gaps identified.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 6749 §4.4.3, §5.1. No violations.

[Node 8 — Arbiter] PASS. RFC 6749 §4.4.3 MUST NOT satisfied: no refresh_token. §5.1 MUST satisfied: access_token, token_type, expires_in present. Renewal via re-authentication is correct pattern.

[Node 9 — Formatter] Verdict: **PASS** | Standards: RFC 6749 §4.4.3, §5.1 | No violations found.

**B:**
PASS. RFC 6749 §4.4.3 MUST NOT satisfied: refresh_token absent from client_credentials grant response. RFC 6749 §5.1 MUST satisfied: access_token, token_type=Bearer, expires_in present. Pre-expiry re-authentication is the correct renewal mechanism. No violations found.

**Reason:** RFC 6749 §4.4.3 and §5.1 satisfied. Correct client_credentials response composition. PASS.

**Phrasing-B:**
PASS. Client credentials grant response correctly contains access_token, token_type, expires_in, scope — no refresh_token per RFC 6749 §4.4.3 MUST NOT. Pre-expiry automatic re-authentication satisfies service lifecycle requirements. PASS.

---

## Pair[CB4-3.4-969-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** OIDC max_age parameter present but not evaluated — AS uses stale session
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: OIDC authorization server session handling in response to max_age parameter. Security sub-domain: authentication freshness enforcement, OIDC Core §3.1.2.2. Routed to OIDC parameter enforcement branch.

[Node 2 — Domain Translator] Standard terminology: max_age is an OIDC authorization request parameter specifying the maximum elapsed time (in seconds) since the user's last authentication. OIDC Core §3.1.2.2 requires the AS to re-authenticate the user if the elapsed time exceeds max_age. Input describes AS that ignores max_age, issues authorization code from 4-hour-old session when max_age=300 (5 minutes).

[Node 3 — Scheme Planner] Validation plan: (1) Identify OIDC Core §3.1.2.2 MUST. (2) Assess "advisory hint" framing. (3) Evaluate practical impact. (4) Determine verdict severity.

[Node 4 — Domain Processor] OIDC Core §3.1.2.2: "If the End-User is not already authenticated, or if the max_age request parameter is provided and the elapsed time since the last authentication is greater than the max_age value, the OP MUST attempt to actively re-authenticate the End-User." Operative normative keyword: MUST. The elapsed time (4 hours = 14400 seconds) is far greater than max_age (300 seconds). Re-authentication is mandated. The "advisory" framing is incorrect — max_age is a binding RP requirement used for step-up authentication, session freshness verification, and sensitive operation gating.

[Node 5 — Applier] Input: AS does not evaluate max_age; issues authorization code from 4-hour-old session. Elapsed time (14400s) >> max_age (300s). Direct OIDC Core §3.1.2.2 MUST violation. "Session continuity" argument does not override MUST. Re-authentication required but not triggered.

[Node 6 — Result Translator] Finding: OIDC Core §3.1.2.2 MUST violated. max_age not evaluated. Re-authentication not triggered. "Advisory" framing rejected. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter selected (OIDC Core is IETF-registered). Primary citation: OIDC Core §3.1.2.2. Normative keyword: MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. OIDC Core §3.1.2.2 MUST: when max_age is present and elapsed time > max_age, AS MUST re-authenticate the user. The AS ignoring max_age violates this unconditionally. max_age is a security requirement from the RP, not an advisory preference; ignoring it removes the RP's ability to enforce authentication freshness for sensitive operations.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: OIDC Core §3.1.2.2 | Normative level: MUST | Gap: max_age not evaluated; stale session (4h) used when max_age=300 | Remediation: Evaluate max_age on every authorization request; compare elapsed time against auth_time; trigger re-authentication when elapsed > max_age; update auth_time in session and ID token.

**B:**
REPROCESS. OIDC Core §3.1.2.2 MUST: AS must re-authenticate when elapsed time since last authentication exceeds max_age. The AS using a 4-hour-old session for a max_age=300 request violates this unconditionally. "Session continuity" is a UX preference that does not override a MUST requirement. The max_age parameter enables RP-controlled authentication freshness — critical for step-up authentication scenarios. Remediation: evaluate max_age on every request; trigger re-auth when elapsed > max_age.

**Reason:** OIDC Core §3.1.2.2 MUST violated. max_age ignored; stale session used. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.1.2.2 MUST is explicit: max_age present + elapsed time > max_age → OP must re-authenticate. 14400 seconds elapsed against max_age=300 is an unambiguous trigger. "Advisory hint" characterisation of max_age is incorrect — it is a binding RP security requirement. REPROCESS.

---

## Pair[CB4-3.4-970-A]

**Territory:** T-rfc-oidc · T-oauth-oidc
**Domain:** OIDC max_age correctly enforced — re-authentication triggered when elapsed > max_age
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: OIDC authorization server max_age enforcement. Security sub-domain: OIDC Core §3.1.2.2 compliance.

[Node 2 — Domain Translator] Standard terminology: AS evaluates max_age on every authorization request. Computes elapsed = current_time − session.auth_time. If elapsed > max_age: triggers re-authentication. After successful re-auth: updates auth_time; issues authorization code with ID token reflecting new auth_time.

[Node 3 — Scheme Planner] Validation plan: (1) Verify max_age evaluated against session auth_time. (2) Verify re-authentication triggered. (3) Verify auth_time updated. (4) Render verdict.

[Node 4 — Domain Processor] OIDC Core §3.1.2.2 MUST: max_age evaluated. Elapsed time computed. Re-authentication triggered when elapsed > max_age. auth_time updated. ID token reflects re-authentication time. All steps correct.

[Node 5 — Applier] All OIDC Core §3.1.2.2 requirements satisfied. No MUST violations. No SHOULD gaps.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. OIDC Core §3.1.2.2. No violations.

[Node 8 — Arbiter] PASS. OIDC Core §3.1.2.2 MUST satisfied: max_age evaluated, re-authentication triggered when elapsed > max_age, auth_time updated, ID token reflects re-auth time.

[Node 9 — Formatter] Verdict: **PASS** | Standard: OIDC Core §3.1.2.2 | max_age enforced; re-auth triggered; auth_time updated | No violations.

**B:**
PASS. OIDC Core §3.1.2.2 MUST satisfied: AS evaluates max_age on every authorization request, compares elapsed time against auth_time, triggers re-authentication when elapsed > max_age, and updates auth_time in the ID token after re-auth. Authentication freshness guarantee operational. No violations found.

**Reason:** OIDC Core §3.1.2.2 MUST satisfied. max_age enforced; re-auth triggered; auth_time accurate. PASS.

**Phrasing-B:**
PASS. AS evaluates max_age, computes elapsed = current_time − auth_time, re-authenticates when elapsed > max_age, updates auth_time. OIDC Core §3.1.2.2 MUST fully satisfied. RP-controlled authentication freshness operational. PASS.

---

## Pair[CB4-3.4-971-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS skips `nbf` validation — tokens accepted before their not-before time
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server JWT access token validation logic. Security sub-domain: not-before claim enforcement, RFC 9068 §4. Input routed to JWT claim validation branch.

[Node 2 — Domain Translator] Standard terminology: the `nbf` (not-before) claim in a JWT defines the earliest time at which the token may be used (RFC 7519 §4.1.5). RFC 9068 §4 requires the RS to reject tokens where nbf > current time. Input describes RS that validates signature, iss, aud, exp, scope but does not check nbf. A token with nbf one hour in the future is accepted immediately.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 9068 §4 MUST for nbf validation. (2) Evaluate "exp covers validity window" argument. (3) Assess impact. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 9068 §4: "If the JWT has an `nbf` (not before) claim, the JWT MUST be rejected if it contains an `nbf` claim that is greater than the current Unix time." MUST. The exp claim and nbf claim serve complementary boundary functions: exp defines when a token becomes invalid (upper bound); nbf defines when a token becomes valid (lower bound). Skipping nbf allows tokens to be used before their intended start time — relevant for scheduled access scenarios, pre-issued tokens, and misconfiguration detection.

[Node 5 — Applier] Input: RS skips nbf check. Token with nbf = current_time + 3600 is accepted at current_time. Direct RFC 9068 §4 MUST violation. "exp is sufficient" argument rejected: exp and nbf are complementary, not redundant.

[Node 6 — Result Translator] Finding: RFC 9068 §4 MUST violated. nbf check absent. Token accepted before validity window opens. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9068 §4 MUST: RS must reject JWT when nbf > current time. Skipping nbf check violates this unconditionally. exp and nbf are complementary bounds; neither is redundant.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9068 §4 | Normative level: MUST | Gap: nbf not validated; tokens accepted before nbf time | Remediation: Add nbf check: reject if nbf > current_time + clock_skew_tolerance (5 seconds standard); position before signature verification.

**B:**
REPROCESS. RFC 9068 §4 MUST: RS must reject JWT access tokens where nbf > current time. The RS skipping nbf validation allows pre-issued or misconfigured tokens to be used before their intended validity window. "exp covers the validity window" is rejected — exp and nbf are upper and lower bounds respectively; both are required. Penetration test confirms acceptance of tokens with nbf one hour in the future. Remediation: add nbf ≤ current_time + 5-second skew check.

**Reason:** RFC 9068 §4 MUST violated. nbf check absent. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4: RS MUST reject if nbf > current time. nbf check absent from RS validation pipeline. A token with nbf 1 hour in future is accepted immediately — the lower validity bound is unenforced. exp sets the upper bound; nbf sets the lower bound; both are necessary. REPROCESS.

---

## Pair[CB4-3.4-972-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `nbf` claim with 5-second clock skew tolerance
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server JWT validation pipeline. Security sub-domain: nbf claim enforcement, RFC 9068 §4.

[Node 2 — Domain Translator] Standard terminology: RS validation step 5: assert `nbf ≤ current_time + 5s`. If nbf > current_time + 5s: reject with 401. 5-second tolerance accommodates minor clock synchronization differences per established practice (RFC 7519 §4.1.5 clock skew guidance).

[Node 3 — Scheme Planner] Validation plan: (1) Verify nbf check present. (2) Verify clock skew tolerance is reasonable. (3) Verify correct positioning in validation pipeline. (4) Render verdict.

[Node 4 — Domain Processor] RFC 9068 §4 MUST: nbf check present. 5-second forward tolerance is standard clock skew practice. Positioned in pipeline after signature check (step 5 of 7). No token before its validity window is accepted beyond the 5-second tolerance. All requirements satisfied.

[Node 5 — Applier] All nbf requirements satisfied. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. No violations.

[Node 8 — Arbiter] PASS. RFC 9068 §4 MUST satisfied: nbf validated with 5-second clock skew tolerance. Tokens with nbf more than 5 seconds in the future are rejected. No violations found.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 9068 §4 | nbf validated with 5-second clock skew tolerance | No violations.

**B:**
PASS. RFC 9068 §4 MUST satisfied: RS validates nbf claim, rejecting tokens where nbf > current_time + 5-second clock skew tolerance. The 5-second tolerance is standard practice for distributed systems. Full validation pipeline (sig → iss → aud → exp → nbf → typ → scope) is correct and ordered. No violations found.

**Reason:** RFC 9068 §4 MUST satisfied. nbf validated with appropriate clock skew tolerance. PASS.

**Phrasing-B:**
PASS. RS nbf check: reject if nbf > current_time + 5s. Satisfies RFC 9068 §4 MUST. Standard 5-second clock skew tolerance applies. Lower validity bound enforced. Upper bound (exp) also checked. Full pipeline correct. PASS.

---

## Pair[CB4-3.4-973-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domain:** JWT `aud` claim issued as wildcard pattern `*.example.com` — per-RS audience isolation defeated
**Pipeline verdict:** CONCERN

**A:**
[Node 1 — Domain Splitter] Input classified: JWT access token audience claim configuration. Security sub-domain: aud claim specificity and audience isolation, RFC 8725 §3.9. Input routed to JWT audience validation branch.

[Node 2 — Domain Translator] Standard terminology: the `aud` (audience) claim identifies the intended recipient(s) of the JWT. RFC 9068 §4 requires the RS to verify that its own identifier is present in aud. Input describes aud="*.example.com" accepted by all RSes on the example.com domain.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 8725 §3.9 guidance on aud specificity. (2) Assess practical impact of wildcard aud. (3) Determine if any MUST violation exists. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 8725 §3.9 (JWT Best Current Practices): "Audience restriction is a fundamental JWT security property. If the aud claim is not validated, the token can be used at any service. Applications SHOULD restrict the audience of tokens to a specific service or set of services." The operative keyword is SHOULD, not MUST. RFC 9068 §4 requires RS to verify its identifier is in aud — a wildcard pattern can technically satisfy this if pattern matching is used, but this is not a MUST prohibition on wildcards. The security impact is meaningful: a token for api.example.com is equally valid at admin.example.com with wildcard aud, defeating the purpose of audience restriction.

[Node 5 — Applier] Input: aud="*.example.com" accepted by all RSes. Per-RS isolation defeated. RFC 8725 §3.9 SHOULD guidance not satisfied. No explicit MUST violation found (wildcards not specifically prohibited by normative MUST). Gap is SHOULD-level. Verdict: CONCERN.

[Node 6 — Result Translator] Finding: RFC 8725 §3.9 SHOULD not satisfied. Per-RS audience isolation defeated. No MUST violation identified. Verdict: CONCERN.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. Primary citation: RFC 8725 §3.9. Normative keyword: SHOULD. Verdict: CONCERN.

[Node 8 — Arbiter] CONCERN. RFC 8725 §3.9 SHOULD: specific audience identifiers should be used for per-RS token restriction. Wildcard aud="*.example.com" defeats audience isolation — a token issued for any purpose on the domain is valid at all RSes, including admin.example.com. No explicit MUST prohibiting wildcards in normative text; gap is SHOULD-level = CONCERN.

[Node 9 — Formatter] Verdict: **CONCERN** | Standard: RFC 8725 §3.9 | Normative level: SHOULD | Gap: wildcard aud defeats per-RS audience isolation | Recommendation: Issue per-RS aud values using RFC 8707 resource indicators; each RS validates its own URI in aud claim.

**B:**
CONCERN. RFC 8725 §3.9 SHOULD: audience claims should be specific RS identifiers to enforce per-RS token restriction. The wildcard `*.example.com` allows any RS on the domain to accept any token — defeating the audience isolation property. No explicit MUST prohibiting wildcard aud in RFC 8725; gap is SHOULD-level. Recommendation: adopt RFC 8707 resource indicators to bind tokens to specific RS audiences at issuance time.

**Reason:** RFC 8725 §3.9 SHOULD (specific aud) not satisfied. Wildcard aud defeats audience isolation. SHOULD-level gap = CONCERN, not REPROCESS.

**Phrasing-B:**
CONCERN. Wildcard aud="*.example.com" means any RS on the domain accepts any token regardless of which specific RS the client was authorized to access. RFC 8725 §3.9 SHOULD directs use of specific audience identifiers. No normative MUST prohibition on wildcards found. SHOULD gap = CONCERN. Adopt RFC 8707 resource indicators for per-RS aud binding.

---

## Pair[CB4-3.4-974-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domain:** JWT `aud` carries specific RS URI; RS validates its own identifier in aud
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: JWT access token aud configuration and RS validation. Security sub-domain: RFC 8707 resource indicators + RFC 9068 §4 aud validation.

[Node 2 — Domain Translator] Standard terminology: AS uses RFC 8707 resource parameter to select per-RS aud value. RS validates aud contains its own URI. api.example.com accepts only aud="https://api.example.com". admin.example.com accepts only aud="https://admin.example.com". Cross-RS token use rejected.

[Node 3 — Scheme Planner] Validation plan: (1) Verify per-RS aud issued via resource indicators. (2) Verify each RS validates its own URI. (3) Verify cross-RS rejection. (4) Render verdict.

[Node 4 — Domain Processor] RFC 9068 §4: RS validates aud. RFC 8707: resource parameter drives per-RS aud. RFC 8725 §3.9 SHOULD: specific identifiers satisfied. Each RS rejects tokens with wrong aud. Audience isolation operational.

[Node 5 — Applier] All requirements satisfied. Per-RS aud binding correct. Cross-RS tokens rejected. No violations.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4, RFC 8707, RFC 8725 §3.9. No violations.

[Node 8 — Arbiter] PASS. RFC 9068 §4 satisfied: each RS validates its own URI in aud. RFC 8725 §3.9 SHOULD satisfied: specific RS URIs used as aud values. RFC 8707 resource indicators drive per-RS token issuance. Audience isolation fully operational.

[Node 9 — Formatter] Verdict: **PASS** | Standards: RFC 9068 §4, RFC 8707, RFC 8725 §3.9 | Per-RS aud via resource indicators; audience isolation operational | No violations.

**B:**
PASS. Per-RS aud values via RFC 8707 resource indicators: AS binds aud to specific RS URIs at issuance. Each RS validates its own URI in aud per RFC 9068 §4. Cross-RS token rejection enforced. RFC 8725 §3.9 SHOULD satisfied: specific identifiers used. Audience isolation fully operational — access to api.example.com does not extend to admin.example.com. No violations found.

**Reason:** RFC 9068 §4 and RFC 8725 §3.9 satisfied. Per-RS aud via RFC 8707. Audience isolation operational. PASS.

**Phrasing-B:**
PASS. RFC 8707 resource indicators drive specific aud values per target RS. RFC 9068 §4 satisfied at each RS (validates own URI in aud). RFC 8725 §3.9 SHOULD satisfied (specific identifiers). Wildcard absent; per-RS audience isolation enforced. PASS.
