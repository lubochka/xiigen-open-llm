# Node 08 — Arbiter Set Extension 50
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1297–1304 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.3, CWE-347 / ASVS V9.1.1,
#          RFC 6749 §3.1.2.3, RFC 6749 §3.1.2,
#          OIDC Core §3.1.3.7 / CWE-347, OIDC Core §5.5,
#          RFC 6750 §3.1, RFC 6749 / ASVS V6.4
# Running total after this file: 2608 instances (1304 base pairs)
# Formula check: 120 + (1304 − 60) × 2 = 120 + 2488 = 2608 ✓

---

## Pair[CB4-3.4-1297]/A

An OIDC relying party validates ID tokens by checking that the `aud` claim is
present and non-null, but it does not verify that the array contains its own
`client_id`. The RP accepts any ID token from the trusted issuer whose `aud` field
is non-empty, even if the `aud` array contains only other clients' identifiers
(e.g., `["other-client-1", "other-client-2"]`). The RP team argues that "verifying
the issuer and the audience field is present" is sufficient audience validation.

## Pair[CB4-3.4-1297]/B

REPROCESS. OIDC Core §3.1.3.7.3 MUST: the Client MUST identify itself as one of
the intended audiences — the check requires verifying that the RP's own `client_id`
is present as a value in the `aud` array, not merely that the `aud` field is non-
empty. An `aud` array that contains other clients' identifiers but not the RP's
`client_id` means the token was not issued for this RP. The "truthy audience"
check is a logical error: it allows the RP to accept tokens issued for entirely
different clients at the same issuer — the canonical token confusion attack that
the `aud` claim is designed to prevent. The RP's argument is rejected; checking
existence is not the same as checking membership. MUST violated; REPROCESS.

## Pair[CB4-3.4-1297]/Reason

OIDC Core §3.1.3.7.3 MUST: RP must verify own client_id is in aud array; truthy
existence check without membership check fails MUST; allows cross-client token
confusion → REPROCESS.

## Pair[CB4-3.4-1297]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7.3 MUST: the RP must verify its own
`client_id` is a member of the `aud` array — presence of the array is insufficient.
An `aud` without the RP's client_id is a token issued for a different client. Token
confusion attack enabled by membership gap. REPROCESS.

---

## Pair[CB4-3.4-1298]/A

A resource server receives JWT access tokens and uses the `sub` and `scope` claims
from the token payload to make authorization decisions (per-user rate limits and
endpoint access control). However, the RS processes the JWT payload without
performing cryptographic signature verification — it decodes the Base64URL payload
directly and applies the claims. The RS team argues that the AS is the trusted source
and that HTTPS transport between client and RS protects against tampering.

## Pair[CB4-3.4-1298]/B

REPROCESS. CWE-347 (Improper Verification of Cryptographic Signature) and ASVS
V9.1.1 MUST: every JWT access token MUST be cryptographically verified before any
claim in its payload is trusted for authorization decisions. A JWT whose signature
is not verified provides no authenticity guarantee — any caller can craft a JSON
payload, Base64URL-encode it, and present it as a valid token. The HTTPS transport
argument is rejected: transport security protects the JWT in transit but does not
validate that the payload was issued by the AS and has not been tampered with before
or after transmission. Without signature verification, the RS grants arbitrary
authorization to any caller who can construct a plausible JWT payload. MUST violated;
REPROCESS.

## Pair[CB4-3.4-1298]/Reason

CWE-347 / ASVS V9.1.1 MUST: JWT signature must be verified before trusting claims;
RS uses claims without signature verification; HTTPS-protects-transit argument
rejected → REPROCESS.

## Pair[CB4-3.4-1298]/Phrasing-B

Verdict REPROCESS. CWE-347 / ASVS V9.1.1 MUST: JWT signature verification is
mandatory before any claim-based authorization decision. HTTPS transport is orthogonal
to JWT authenticity; an unverified JWT can be forged by any caller. REPROCESS.

---

## Pair[CB4-3.4-1299]/A

A client is registered at an OIDC provider with three distinct redirect URIs:
`https://app.example.com/callback`, `https://app.example.com/callback-dev`, and
`https://app.example.com/callback-mobile`. The client sends an authorization request
that includes `client_id` and other required parameters but omits the `redirect_uri`
parameter. The AS cannot determine which of the three registered URIs the client
intends and defaults to the first registered URI, completing the flow. The team
argues this is a convenience feature for clients with a "primary" redirect URI.

## Pair[CB4-3.4-1299]/B

REPROCESS. RFC 6749 §3.1.2.3 MUST: if the client has multiple redirect URIs
registered, it MUST include the `redirect_uri` parameter in the authorization
request. The AS must fail the request and return `error=invalid_request` when
multiple URIs are registered and none is specified — the AS cannot make an
unambiguous choice on the client's behalf. The first-registered-as-default
convenience argument is rejected: defaulting to the first registered URI is a
substitution of AS judgment for an explicit REQUIRED specification; it introduces
an ambiguity that could be exploited to redirect authorization codes to unintended
destinations. MUST violated; REPROCESS.

## Pair[CB4-3.4-1299]/Reason

RFC 6749 §3.1.2.3 MUST: client must include redirect_uri when multiple URIs
registered; AS must fail request if absent and multiple registered; default-to-first
argument rejected → REPROCESS.

## Pair[CB4-3.4-1299]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1.2.3: when multiple redirect URIs are registered,
the client MUST specify redirect_uri; AS MUST reject if absent. Defaulting to the
first registered URI substitutes AS judgment for a required explicit parameter.
REPROCESS.

---

## Pair[CB4-3.4-1300]/A

A client attempts to register a redirect URI of
`https://app.example.com/callback#auth-section` — an HTTPS URI containing a
fragment component (`#auth-section`). The AS's dynamic client registration endpoint
accepts the registration without error. The team argues that the fragment is
ignored by the server and only processed client-side, making it harmless.

## Pair[CB4-3.4-1300]/B

REPROCESS. RFC 6749 §3.1.2 MUST NOT: the redirect URI endpoint MUST NOT include
a fragment component. This prohibition exists because when the AS appends the
authorization code to the redirect URI, the presence of an existing fragment
component produces a malformed or ambiguous redirect URL — HTTP servers do not
receive fragment components; they are only visible to the browser. If the AS
constructs the redirect URL by appending `?code=...` to the fragment-containing
URI, the resulting URL places the authorization code after a fragment delimiter
where it may not be properly processed. The team's "harmless fragment" argument
is rejected: the MUST NOT applies at registration time and does not provide a
runtime-impact exception. The AS should reject fragment-containing redirect URIs
at registration with a `invalid_redirect_uri` error. MUST violated; REPROCESS.

## Pair[CB4-3.4-1300]/Reason

RFC 6749 §3.1.2 MUST NOT: redirect URI must not include fragment component; AS
accepted fragment-containing URI at registration; harmless-fragment argument
rejected → REPROCESS.

## Pair[CB4-3.4-1300]/Phrasing-B

Verdict REPROCESS. RFC 6749 §3.1.2 MUST NOT: redirect URIs must not contain
fragment components. Registration of a fragment-containing URI violates this MUST.
The harmless-at-runtime argument does not override the registration-time MUST NOT.
REPROCESS.

---

## Pair[CB4-3.4-1301]/A

An OIDC relying party caches its JWKS response. An ID token arrives with
`kid="2025-key-01"` in the JOSE header, but the cached JWKS only contains
`kid="2024-key-01"`. Rather than failing the request or re-fetching the JWKS,
the RP implements a "soft failure" path: if no matching key is found, it logs a
warning and accepts the token without signature verification to avoid service
disruption during key rotation. The team argues this prevents outages when JWKS
is momentarily unavailable.

## Pair[CB4-3.4-1301]/B

REPROCESS. OIDC Core §3.1.3.7 and CWE-347: ID token signature verification is
MUST — the RP must verify the signature before accepting any claims. Accepting an
ID token without signature verification when the signing key is not available
is a complete bypass of the cryptographic authentication guarantee. The RP's
soft-failure path means an attacker who presents a token with an unknown `kid`
can bypass all signature verification. The service-continuity argument is rejected:
the correct behavior for a kid-not-found situation is to attempt a JWKS cache refresh
(re-fetch the JWKS from the provider); if the key still cannot be found after a
refresh, the token MUST be rejected. Accepting unsigned tokens to prevent outages
is a critical security failure. MUST violated; REPROCESS.

## Pair[CB4-3.4-1301]/Reason

OIDC Core §3.1.3.7 / CWE-347 MUST: ID token signature must be verified; soft-failure
path accepts tokens without signature check when kid not found; service-continuity
argument rejected → REPROCESS.

## Pair[CB4-3.4-1301]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7 / CWE-347: signature verification is
mandatory. A soft-failure path that accepts tokens without verification on kid-not-
found completely bypasses authentication. Service continuity is not a valid exception.
REPROCESS.

---

## Pair[CB4-3.4-1302]/A

An OIDC relying party sends a `claims_locales=fr-CA en` parameter in its
authorization requests to indicate that it prefers French Canadian first, then
English for returned UserInfo claims. The OIDC provider processes the authorization
request but ignores the `claims_locales` parameter — UserInfo responses always
return all claims in English regardless of locale preferences. The RP cannot
provide localized claim values to French-speaking users. The AS team argues that
localization is not a security concern and is optional infrastructure.

## Pair[CB4-3.4-1302]/B

CONCERN. OIDC Core §5.5 specifies that authorization servers SHOULD use preferred
languages for claim values when `claims_locales` is provided — the obligation is
SHOULD, not MUST. No unconditional MUST requires the AS to honor locale preferences.
The AS team is correct that localization support is optional and not a security
requirement in isolation. The gap is guidance-level: OIDC Core §5.5 recommends
locale support for internationalized deployments, and clients cannot provide
localized user experiences when the AS does not honor `claims_locales`. No MUST is
violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1302]/Reason

OIDC Core §5.5 SHOULD use preferred locale for claim values; no unconditional MUST;
AS ignoring claims_locales is guidance gap only → CONCERN.

## Pair[CB4-3.4-1302]/Phrasing-B

Verdict CONCERN. OIDC Core §5.5 designates locale preference support as SHOULD —
no unconditional MUST. Ignoring `claims_locales` is a guidance gap. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1303]/A

An OIDC provider's UserInfo endpoint and resource servers return
`WWW-Authenticate: Bearer error="invalid_token"` challenges to unauthorized
requests. The challenge does not include a `realm` parameter — it contains only
the error and error_description fields. A client developer asks whether the
missing `realm` attribute constitutes a violation.

## Pair[CB4-3.4-1303]/B

CONCERN. RFC 6750 §3.1 specifies that the `realm` attribute SHOULD be provided in
the `WWW-Authenticate` header when using the Bearer scheme. SHOULD is guidance-level
— no unconditional MUST mandates a `realm` attribute for Bearer token challenges.
The `realm` attribute provides additional context for humans and automated clients
about the protection space being requested. Without it, the challenge is still
parseable and processable by OAuth clients. The absence of `realm` is a guidance-
level gap in the Bearer token challenge; it does not prevent correct client
behavior. CONCERN not REPROCESS.

## Pair[CB4-3.4-1303]/Reason

RFC 6750 §3.1 SHOULD: realm attribute recommended in WWW-Authenticate Bearer
challenge; no unconditional MUST; absent realm is guidance gap only → CONCERN.

## Pair[CB4-3.4-1303]/Phrasing-B

Verdict CONCERN. RFC 6750 §3.1 SHOULD: `realm` is recommended in Bearer challenges.
No MUST mandates its presence. Missing `realm` is a guidance-level gap. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1304]/A

An OIDC authorization server's authorization endpoint applies no server-side rate
limiting or anomaly detection on the number of authorization requests from a single
IP address or for a single client. An attacker can submit thousands of authorization
requests per minute to a public client, generating thousands of authorization codes
that expire unused. The team argues that authorization requests are unauthenticated
by nature and that rate limiting would harm legitimate users and automation tools.

## Pair[CB4-3.4-1304]/B

CONCERN. No RFC contains an unconditional MUST for rate limiting or anomaly detection
on the authorization endpoint itself. ASVS V6.4 and general defense-in-depth guidance
recommend rate limiting on authentication and authorization initiation endpoints to
prevent abuse; however, these are SHOULD-level recommendations rather than
unconditional MUSTs. The team's argument that unauthenticated endpoints resist
per-IP rate limits without harming legitimate users is partially accepted as an
implementation trade-off. The guidance gap is real: unbounded authorization request
flooding can drain server resources, generate code database entries, and enable
enumeration of client behavior. No MUST is violated. CONCERN not REPROCESS.

## Pair[CB4-3.4-1304]/Reason

No RFC MUST for authorization endpoint rate limiting; ASVS V6.4 / defense-in-depth
guidance SHOULD; unbounded flooding is a meaningful risk; no MUST → CONCERN.

## Pair[CB4-3.4-1304]/Phrasing-B

Verdict CONCERN. No unconditional RFC MUST mandates authorization endpoint rate
limiting. ASVS V6.4 / guidance-level SHOULD. Unbounded request flooding creates
operational risk at guidance level. CONCERN not REPROCESS.
