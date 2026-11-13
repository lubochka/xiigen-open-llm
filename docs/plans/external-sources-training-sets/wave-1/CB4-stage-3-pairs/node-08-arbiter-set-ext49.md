# Node 08 — Arbiter Set Extension 49
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1271–1278 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.11, RFC 7009 §2.1, RFC 7636 §4.4,
#          RFC 8725 §3.1 / ASVS V9.1.1, OIDC Core §3.1.2.2,
#          RFC 9449 / ASVS V10.4, CWE-200 / ASVS V8.3.1,
#          OIDC Core §8.2
# Running total after this file: 2556 instances (1278 base pairs)
# Formula check: 120 + (1278 − 60) × 2 = 120 + 2436 = 2556 ✓

---

## Pair[CB4-3.4-1271]/A

An OIDC relying party requests authentication with `max_age=300` in its
authorization request. The AS includes an `auth_time` claim in the ID token
indicating the user's last authentication was 7,200 seconds ago (2 hours prior).
The RP receives the ID token, validates the signature, `iss`, `aud`, `exp`, and
`nonce` claims, but does not check the `auth_time` value against the requested
`max_age` constraint. The RP accepts the token and proceeds with the session.
The RP team argues that the AS is responsible for enforcing `max_age` by
triggering re-authentication before issuing the token.

## Pair[CB4-3.4-1271]/B

REPROCESS. OIDC Core §3.1.3.7.11 assigns the max_age enforcement obligation to
both parties: the AS MUST re-authenticate if elapsed time exceeds max_age (covered
in ext44), AND the RP MUST check the `auth_time` claim and reject the ID token
if `auth_time + max_age < current_time`. The RP's validation pipeline omits the
auth_time/max_age comparison — the RP is independently responsible for this check.
The AS-responsibility argument is rejected: the AS may fail to enforce max_age (as
ext44 demonstrates); the RP's check is an independent normative obligation that
exists precisely to catch such AS failures. An `auth_time` that satisfies the
claimed authentication window but violates the max_age constraint represents a
security failure that the RP must detect and reject. MUST violated; REPROCESS.

## Pair[CB4-3.4-1271]/Reason

OIDC Core §3.1.3.7.11 MUST: RP must check auth_time satisfies max_age constraint;
auth_time 7200s > max_age 300s and RP does not reject; AS-responsibility argument
rejected (RP has independent obligation) → REPROCESS.

## Pair[CB4-3.4-1271]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.3.7.11 MUST: the RP must validate that
auth_time + max_age ≥ current_time. This is an independent RP obligation, not
solely the AS's responsibility. The auth_time/max_age check was omitted; the
violation is on the RP side. REPROCESS.

---

## Pair[CB4-3.4-1272]/A

An authorization server's token revocation endpoint (`/revoke`) only accepts
requests with `Content-Type: application/json` — a JSON body with
`{"token": "...", "token_type_hint": "refresh_token"}`. RFC 7009-compliant clients
that submit a `Content-Type: application/x-www-form-urlencoded` body receive a
`415 Unsupported Media Type` error. The AS team argues that JSON is a modern
encoding and clients should send JSON bodies.

## Pair[CB4-3.4-1272]/B

REPROCESS. RFC 7009 §2.1 MUST: the revocation request MUST use HTTP POST with
`Content-Type: application/x-www-form-urlencoded`. This is an explicit MUST in
the RFC that defines the transport binding for the revocation endpoint. JSON bodies
are not the specified encoding and are not an acceptable substitute. The AS team's
argument that JSON is a "modern encoding" is rejected: a protocol specification
that defines an explicit MUST encoding cannot be overridden by subjective
preference; RFC 7009 clients that correctly implement the standard by sending
form-encoded bodies are rejected, which constitutes non-conformance with the RFC.
MUST violated; REPROCESS.

## Pair[CB4-3.4-1272]/Reason

RFC 7009 §2.1 MUST: revocation endpoint MUST accept application/x-www-form-urlencoded;
rejecting form-encoded bodies violates MUST; JSON substitution argument rejected →
REPROCESS.

## Pair[CB4-3.4-1272]/Phrasing-B

Verdict REPROCESS. RFC 7009 §2.1 explicitly requires HTTP POST with
`application/x-www-form-urlencoded` for revocation requests. JSON is not the
specified encoding. Rejecting conformant form-encoded requests violates the MUST.
REPROCESS.

---

## Pair[CB4-3.4-1273]/A

A client initiates an authorization code flow and includes `code_challenge=A3b...`
(S256 method) in its authorization request. The AS stores the code challenge with
the authorization code. At the token endpoint, the same client submits the
authorization code but omits the `code_verifier` parameter entirely. The AS
locates the stored code challenge, notices the verifier is absent, but proceeds
to issue an access token anyway because "the code was already verified via client
authentication." The team argues that the confidential client's `client_secret`
provides equivalent protection.

## Pair[CB4-3.4-1273]/B

REPROCESS. RFC 7636 §4.4 MUST: when a code challenge was included in the
authorization request, the AS MUST require the code_verifier in the token request
and MUST verify that `BASE64URL(SHA256(code_verifier))` equals the stored
`code_challenge`. The AS stored the challenge but then failed to enforce the
verification — bypassing the PKCE binding entirely. The client_secret equivalence
argument is rejected: RFC 7636 §4.4's MUST applies to the code_challenge/code_verifier
verification independently of whether the client uses a secret. PKCE provides
code binding protection even for confidential clients; skipping it when it was
initiated violates the MUST unconditionally. MUST violated; REPROCESS.

## Pair[CB4-3.4-1273]/Reason

RFC 7636 §4.4 MUST: when code_challenge in auth request, AS must require and verify
code_verifier at token exchange; AS stored challenge but accepted exchange without
verifier; client_secret equivalence argument rejected → REPROCESS.

## Pair[CB4-3.4-1273]/Phrasing-B

Verdict REPROCESS. RFC 7636 §4.4 MUST: code_challenge in auth request creates a
mandatory verification obligation at token exchange; absent code_verifier MUST be
rejected. Client_secret does not substitute for the code binding check. REPROCESS.

---

## Pair[CB4-3.4-1274]/A

An OIDC authorization server uses the same RS256 signing key pair for both JWT
access tokens (RFC 9068 profile) and OIDC ID tokens. The rationale is operational
simplicity: key rotation is performed once for all token types simultaneously.
A security reviewer notes that if an ID token or access token is decoded, an
attacker with knowledge of the shared key context could attempt a token confusion
attack. The team argues no RFC explicitly prohibits shared signing keys across
token types.

## Pair[CB4-3.4-1274]/B

CONCERN. RFC 8725 §3.1 cautions that when the same key is used for multiple
purposes, applications MUST verify the audience to prevent cross-purpose token
use. ASVS V9.1.1 guidance recommends using separate keys per token type to enforce
type isolation. However, no unconditional RFC MUST explicitly prohibits using the
same signing key for both access tokens and ID tokens, and the team is correct
that no MUST appears in RFC 9068 or OIDC Core for key isolation. The practical
risk — that an access token could be mistaken for an ID token or vice versa in
a misconfigured RS — is partially mitigated by the `typ` claim requirement (RFC
9068 §2.1 MUST `typ=at+JWT` for access tokens) and `aud` validation. Shared key
usage is a guidance-level gap. CONCERN not REPROCESS.

## Pair[CB4-3.4-1274]/Reason

RFC 8725 §3.1 / ASVS V9.1.1 guidance: separate keys per token type recommended;
no unconditional RFC MUST prohibiting shared key; typ+aud provides partial
mitigation; guidance-level gap → CONCERN.

## Pair[CB4-3.4-1274]/Phrasing-B

Verdict CONCERN. RFC 8725 §3.1 and ASVS V9.1.1 recommend per-type key isolation.
No unconditional MUST prohibits shared signing keys. The `typ` and `aud` claims
provide partial token-type discrimination. Guidance-level gap → CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1275]/A

A user has previously authenticated with an OIDC provider and consented to a
specific application's requested scopes. The application subsequently sends a
new authorization request with `prompt=consent`. The OIDC provider detects the
existing authenticated session and the prior consent record, determines that the
requested scopes are already consented, and issues a new ID token and authorization
code without displaying any consent screen. The team argues this avoids unnecessary
user friction since the consent is already on file.

## Pair[CB4-3.4-1275]/B

REPROCESS. OIDC Core §3.1.2.2 MUST: when `prompt=consent` is included in the
authorization request, the AS MUST prompt the End-User for consent even if a
prior consent record exists. The `prompt=consent` parameter is an explicit
instruction from the relying party to force re-consent — the RP uses it when it
needs to confirm that the user's consent is current and intentional (for example,
after a scope change, terms update, or regulatory requirement). The team's user-
friction argument is rejected: the MUST is unconditional; the RP's explicit
re-consent request takes precedence over the AS's caching of prior consent.
Silently skipping the consent screen violates the MUST. REPROCESS.

## Pair[CB4-3.4-1275]/Reason

OIDC Core §3.1.2.2 MUST: prompt=consent requires AS to display consent screen
even if prior consent exists; AS skipped consent based on cached prior consent;
user-friction argument rejected → REPROCESS.

## Pair[CB4-3.4-1275]/Phrasing-B

Verdict REPROCESS. OIDC Core §3.1.2.2 MUST: prompt=consent is a normative
instruction to force re-consent regardless of prior consent records. The RP's
re-consent request takes precedence. Silently issuing tokens without the consent
screen violates the MUST. REPROCESS.

---

## Pair[CB4-3.4-1276]/A

An OIDC deployment uses both DPoP (RFC 9449) and mTLS (RFC 8705) for token
endpoint client authentication on the same request. The AS processes both the
DPoP proof and the mTLS certificate but binds the issued access token only to the
DPoP key (embedding `cnf/jkt` in the token) — it does not include an mTLS-based
`cnf/x5t#S256` binding. A reviewer asks whether the partial token binding
constitutes a violation.

## Pair[CB4-3.4-1276]/B

CONCERN. When DPoP and mTLS are both present in the same token request, ASVS
V10.4 guidance recommends binding the issued token to both mechanisms when both
are verified by the AS. RFC 9449 §7 and RFC 8705 §3 each independently define
their own token binding mechanisms; no RFC provides a MUST for dual-binding when
both are simultaneously presented. The DPoP binding satisfies RFC 9449 §6 MUST
(cnf/jkt MUST be embedded when DPoP proof accepted). The absence of mTLS binding
is a guidance gap — the AS verified the certificate but did not encode the
fingerprint in the token. Without both bindings, an attacker who steals the access
token can replay it without the mTLS certificate (using the DPoP key alone if
they obtain it). Guidance-level gap; no MUST for dual-binding violated. CONCERN
not REPROCESS.

## Pair[CB4-3.4-1276]/Reason

DPoP cnf/jkt binding present (RFC 9449 §6 MUST satisfied); mTLS cnf/x5t#S256
binding absent (ASVS V10.4 guidance; no MUST for dual-binding when both mechanisms
used); guidance-level gap → CONCERN.

## Pair[CB4-3.4-1276]/Phrasing-B

Verdict CONCERN. RFC 9449 §6 MUST for DPoP binding is satisfied. No RFC MUST
requires dual-binding when both DPoP and mTLS are used. The absent mTLS binding
is an ASVS V10.4 guidance gap. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1277]/A

An authorization server's token endpoint returns a `400 Bad Request` error for
failed client credential authentication. The `error_description` field contains:
`"Authentication failed for service account svc-billing-prod-7 (client_id:
billing-client-42): incorrect client_secret provided."` This response is returned
to any unauthenticated caller who presents a valid `client_id` with a wrong secret.
The AS team argues that detailed error messages improve developer experience during
integration.

## Pair[CB4-3.4-1277]/B

REPROCESS. CWE-200 (Exposure of Sensitive Information) and ASVS V8.3.1 MUST: error
responses MUST NOT disclose internal system information, account names, or
infrastructure identifiers to unauthenticated callers. The error_description exposes
the service account's internal username (`svc-billing-prod-7`) and client identifier
— both are internal infrastructure details that aid an attacker in reconnaissance,
lateral movement, and targeted attacks. RFC 6749 §5.2 specifies only `error`,
`error_description`, and `error_uri` in error responses, with no guidance to include
account names. The developer-experience argument is rejected: detailed errors should
be logged server-side; callers receive a minimum-disclosure response
(`error=invalid_client` only). MUST violated; REPROCESS.

## Pair[CB4-3.4-1277]/Reason

CWE-200 / ASVS V8.3.1 MUST NOT disclose internal account names in error responses;
error_description leaks service account username to unauthenticated callers;
developer-experience argument rejected → REPROCESS.

## Pair[CB4-3.4-1277]/Phrasing-B

Verdict REPROCESS. ASVS V8.3.1 and CWE-200 MUST: error responses must not expose
internal identifiers to unauthenticated callers. The error_description reveals the
service account name and client identifier — reconnaissance data. Developer-
experience rationale rejected; internal details belong in server-side logs only.
REPROCESS.

---

## Pair[CB4-3.4-1278]/A

An OIDC authorization server normalizes all `sub` claim values to lowercase before
embedding them in ID tokens and access tokens. In the user directory, "Alice123"
and "alice123" are distinct users with separate accounts. After normalization,
both users receive tokens where `sub = "alice123"`. A resource server that
validates tokens cannot distinguish between the two users based on the `sub` claim.
The AS team argues that lowercase normalization is a standard practice that prevents
case-sensitivity confusion.

## Pair[CB4-3.4-1278]/B

REPROCESS. OIDC Core §8.2 requires that `sub` values be locally unique within the
issuer's namespace. If two distinct users in the user directory map to the same
normalized `sub` value, the uniqueness requirement is violated. RFC 7519 §4.1.2
treats `sub` as a case-sensitive StringOrURI; OIDC Core §8.2 reinforces uniqueness
per-issuer. When "Alice123" and "alice123" both receive `sub="alice123"` in their
tokens, any sub-based authorization check, session lookup, or audit trail is
compromised — a token for one user is indistinguishable from a token for the other
at the resource server. The normalization-as-standard-practice argument is rejected:
normalization is only safe when the pre-normalization identifiers are guaranteed
unique in the normalized domain, which they are not here. MUST violated; REPROCESS.

## Pair[CB4-3.4-1278]/Reason

OIDC Core §8.2 MUST: sub values must be locally unique per issuer; lowercase
normalization collapses "Alice123" and "alice123" to same sub value; uniqueness
violated; normalization-practice argument rejected → REPROCESS.

## Pair[CB4-3.4-1278]/Phrasing-B

Verdict REPROCESS. OIDC Core §8.2 MUST: sub values must be locally unique within
the issuer. Lowercasing two distinct directory entries to the same sub value
destroys uniqueness — tokens for different users become indistinguishable. The
normalization argument is rejected; normalization is only safe when post-normalization
uniqueness is guaranteed. REPROCESS.
