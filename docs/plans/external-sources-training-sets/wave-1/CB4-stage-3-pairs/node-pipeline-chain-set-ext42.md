# Node Pipeline Chain Set Extension 42
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1253–1260 (16 instances — 8 base pairs × 2 phrasings)
# Sources: RFC 6749 §4.1.3, OIDC Dynamic Registration §3.2,
#          CWE-601 / ASVS V13.4.1, RFC 7009 §1,
#          OIDC Core §5.3.4, RFC 6749 §4.1.2,
#          ASVS V6.3.2, RFC 9101 §10.3
# Running total after this file: 2520 instances (1260 base pairs)
# Formula check: 120 + (1260 − 60) × 2 = 120 + 2400 = 2520 ✓

---

## Pair[CB4-3.4-1253]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1245]/A:
Client includes redirect_uri in authorization request but omits it at the token
endpoint. AS issues access token without verifying redirect_uri at exchange.
AS team argues single-registered-URI makes the check redundant.

## Pair[CB4-3.4-1253]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 token endpoint, sub-domain =
redirect_uri verification at authorization code exchange. Security-verification-3.4
flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 6749 §4.1.3 (redirect_uri
REQUIRED at token endpoint when included in authorization request; MUST verify
identical value).
Node 3 (Scheme Planner): Verify whether §4.1.3 redirect_uri requirement is conditional
on registration count; evaluate whether single-registration argument creates an
exception to the MUST.
Node 4 (Domain Processor): RFC 6749 §4.1.3: "if the redirect_uri parameter was
included in the authorization request ... the AS MUST verify that it is identical
to the value used." No exception for single-registered-URI clients. The requirement
is procedural: the client must supply the redirect_uri and the AS must compare it.
Omitting the comparison step — even when the result would be trivially obvious —
violates the MUST.
Node 5 (Applier): Single-registration redundancy argument tested: REJECTED. The
MUST requires the comparison procedure itself, not merely a prediction of its
outcome. The AS substituted assumption for the required verification step.
Node 6 (Result Translator): MUST violated. RFC 6749 §4.1.3.
Node 7 (Arbiter Selector): RFC 6749 §4.1.3 MUST arbiter selected; REPROCESS
threshold met.
Node 8 (Arbiter): REPROCESS. RFC 6749 §4.1.3 MUST: redirect_uri required at token
endpoint when in auth request; AS must verify identical value; single-registration
argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 6749 §4.1.3. Remediation:
require redirect_uri at token endpoint when it was in auth request; compare byte-
for-byte against stored authorization request value.

## Pair[CB4-3.4-1253]/Reason

Nine-node trace: redirect_uri in auth request absent at token exchange; RFC 6749
§4.1.3 MUST verify identical value; single-registration argument rejected;
REPROCESS.

## Pair[CB4-3.4-1253]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms RFC 6749 §4.1.3 MUST without single-
registration exception. Node 5 rejects the redundancy argument. Nodes 7–8 confirm
MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1254]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1246]/A:
OIDC Dynamic Client Registration returns registration response for confidential
client with token_endpoint_auth_method=client_secret_basic but omits client_secret
from the response. Team argues client can retrieve secret separately.

## Pair[CB4-3.4-1254]/B

Node 1 (Domain Splitter): Input domain = OIDC Dynamic Client Registration, sub-domain =
registration response required fields for confidential clients. Security-verification-
3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Dynamic Client Registration
§3.2 (client_secret MUST be returned when auth method requires it). ASVS V9.2.2
provides hashed storage context establishing why post-issuance retrieval is impossible.
Node 3 (Scheme Planner): Determine whether client_secret is MUST-level in the
registration response when client_secret_basic is the auth method; evaluate the
separate-retrieval argument against hashed storage constraints.
Node 4 (Domain Processor): OIDC Dynamic Client Registration §3.2: when
`token_endpoint_auth_method` requires a client secret, the `client_secret` MUST
be included in the registration response. This is the unique issuance window; after
issuance the secret must be stored hashed and cannot be reversed for transmission.
Without the secret in the response, the confidential client cannot authenticate.
Node 5 (Applier): Separate-retrieval endpoint argument tested against ASVS V9.2.2:
a retrieval endpoint returning plain secrets would violate hashed-storage MUST;
therefore, separate retrieval is architecturally impossible. REJECTED.
Node 6 (Result Translator): MUST violated. OIDC Dynamic Registration §3.2.
Node 7 (Arbiter Selector): OIDC Dynamic Registration §3.2 MUST arbiter; REPROCESS
threshold met.
Node 8 (Arbiter): REPROCESS. OIDC Dynamic Registration §3.2 MUST: client_secret
must be in registration response when auth method requires it; separate retrieval
impossible under hashed storage; MUST violated.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Dynamic Registration §3.2.
Remediation: include client_secret in registration response when token_endpoint_auth_method
requires it; clearly document that secret is only transmitted once at registration.

## Pair[CB4-3.4-1254]/Reason

Nine-node trace: client_secret absent from dynamic registration response for
client_secret_basic client → OIDC Dynamic Registration §3.2 MUST; separate-
retrieval argument rejected (hashed storage); REPROCESS.

## Pair[CB4-3.4-1254]/Phrasing-B

Pipeline verdict REPROCESS. Node 2 identifies OIDC Dynamic Registration §3.2 and
ASVS V9.2.2. Node 5 rejects separate retrieval as architecturally impossible under
hashed storage. REPROCESS.

---

## Pair[CB4-3.4-1255]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1247]/A:
AS login page redirects to user-supplied return_url after authentication without
validating against any allowlist. Attacker-supplied return_url sends authenticated
victims to attacker-controlled domain.

## Pair[CB4-3.4-1255]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
unvalidated redirect at AS origin. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = CWE-601 (URL Redirection to
Untrusted Site — Open Redirect) and ASVS V13.4.1 (redirect target validation
against allowlist). OAuth 2.0 context: AS is a high-trust origin with active
authorization flows.
Node 3 (Scheme Planner): Verify whether redirect-target validation is a MUST-level
requirement; evaluate the post-authentication scope argument; assess AS-specific
impact of open redirect.
Node 4 (Domain Processor): ASVS V13.4.1 MUST: all redirects to external URLs must
be validated against a configured allowlist. CWE-601 is unconditional — untrusted
redirect targets are a vulnerability regardless of authentication state. AS-specific
impact: victims redirected from trusted AS domain to attacker domain while in active
OAuth flow; authorization codes and tokens may follow the redirect chain.
Node 5 (Applier): Post-authentication scope argument tested: CWE-601 applies
regardless of authentication state; an authenticated user is still a victim of
phishing when redirected to attacker domain; OAuth authorization context amplifies
risk. REJECTED.
Node 6 (Result Translator): MUST violated. ASVS V13.4.1 / CWE-601.
Node 7 (Arbiter Selector): ASVS V13.4.1 / CWE-601 MUST arbiter; REPROCESS threshold
met.
Node 8 (Arbiter): REPROCESS. CWE-601 / ASVS V13.4.1 MUST: redirect targets must
be validated against allowlist; AS open redirect via unvalidated return_url; post-
authentication scope argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=CWE-601 / ASVS V13.4.1.
Remediation: validate all redirect targets against static allowlist before issuing
redirect; reject unregistered URLs with error response; no user-supplied redirect
targets without prior validation.

## Pair[CB4-3.4-1255]/Reason

Nine-node trace: unvalidated return_url redirect at AS → CWE-601 / ASVS V13.4.1
MUST allowlist validation; post-authentication argument rejected; REPROCESS.

## Pair[CB4-3.4-1255]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 identifies CWE-601 / ASVS V13.4.1 MUST. Node 5
rejects the post-authentication scope argument. Nodes 7–8 confirm MUST violation.
REPROCESS.

---

## Pair[CB4-3.4-1256]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1248]/A:
AS revocation endpoint supports refresh token revocation but returns
error=unsupported_token_type for access tokens. Team argues 15-minute TTL makes
access token revocation unnecessary.

## Pair[CB4-3.4-1256]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 token revocation, sub-domain =
access token revocation support scope. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 7009 §1 (implementations
SHOULD support revocation of access tokens). RFC 6749 token lifecycle context.
Node 3 (Scheme Planner): Classify RFC 7009 §1 obligation level (SHOULD vs MUST);
evaluate 15-minute TTL as compensating control; determine verdict.
Node 4 (Domain Processor): RFC 7009 §1: "Implementations SHOULD support revocation
of access tokens." SHOULD is guidance-level — no unconditional MUST. The AS supports
refresh token revocation but not access token revocation. 15-minute TTL limits post-
compromise exposure but does not close all incident-response scenarios (account
compromise, emergency lockout, insider threat).
Node 5 (Applier): TTL compensating control partially accepted — meaningful for
typical short-window attacks; not sufficient for all scenarios where revocation
matters. SHOULD gap: guidance-level.
Node 6 (Result Translator): No MUST violated. RFC 7009 §1 SHOULD not met for
access tokens → CONCERN.
Node 7 (Arbiter Selector): RFC 7009 §1 SHOULD arbiter; REPROCESS threshold NOT
met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 7009 §1 SHOULD access token revocation; absent
support = guidance-level gap; 15-min TTL is partial compensating control; no MUST
violated. CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 7009 §1. Remediation: implement
access token revocation support; accept token_type_hint=access_token at /revoke
endpoint; use blocklist with TTL matching remaining token lifetime.

## Pair[CB4-3.4-1256]/Reason

Nine-node trace: access token revocation absent → RFC 7009 §1 SHOULD; 15-min TTL
partial compensating control; no MUST violated → CONCERN.

## Pair[CB4-3.4-1256]/Phrasing-B

Pipeline verdict CONCERN. Node 4 confirms RFC 7009 §1 is SHOULD-level. Node 5
partially accepts TTL as a compensating control. No MUST violated. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1257]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1249]/A:
OIDC ID token sub claim is a UUID-based PPID; UserInfo endpoint returns same
user's sub as their email address. RP correctly rejects the mismatch. AS team
argues RP should use sub from whichever source it prefers.

## Pair[CB4-3.4-1257]/B

Node 1 (Domain Splitter): Input domain = OIDC UserInfo / ID token consistency,
sub-domain = sub claim identity verification across ID token and UserInfo response.
Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §5.3.4 (RP MUST verify
sub in UserInfo response is identical to sub in ID token). Security context: sub
mismatch indicates identity inconsistency or UserInfo injection.
Node 3 (Scheme Planner): Determine whether §5.3.4 sub cross-check is MUST-level;
evaluate RP-preference argument; establish AS responsibility for sub consistency.
Node 4 (Domain Processor): OIDC Core §5.3.4 MUST: "The Client MUST verify that
the sub Claim in the UserInfo Response is identical to the sub Claim in the ID
Token." The RP's rejection is the correct behavior. The AS is non-conformant: the
same authenticated user has different sub values across ID token and UserInfo,
making the required cross-check always fail. The AS must use the same sub identifier
consistently in both responses.
Node 5 (Applier): RP-preference argument tested — §5.3.4 defines the sub cross-
check as a normative security requirement, not an RP option; it prevents UserInfo
injection by binding UserInfo to the specific ID token sub. REJECTED.
Node 6 (Result Translator): MUST violated. OIDC Core §5.3.4. AS sub inconsistency
makes RP sub cross-check permanently fail.
Node 7 (Arbiter Selector): OIDC Core §5.3.4 MUST arbiter; REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. OIDC Core §5.3.4 MUST: sub in UserInfo must be
identical to sub in ID token; mismatch = non-conformance; RP preference argument
rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Core §5.3.4. Remediation:
ensure the same sub identifier is used consistently in both ID token and UserInfo
response for every user; sub must be stable and consistent across all endpoints.

## Pair[CB4-3.4-1257]/Reason

Nine-node trace: UserInfo sub != ID token sub → OIDC Core §5.3.4 MUST identical;
RP-preference argument rejected; REPROCESS.

## Pair[CB4-3.4-1257]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms OIDC Core §5.3.4 MUST. Node 5 rejects
the RP-preference framing. Nodes 7–8 confirm MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1258]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1250]/A:
Non-atomic check-and-set for authorization code single-use allows two concurrent
exchange requests to both succeed. Team frames this as an infrastructure scalability
issue rather than a security violation.

## Pair[CB4-3.4-1258]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 authorization code exchange,
sub-domain = single-use enforcement under concurrent load. Security-verification-
3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 6749 §4.1.2 (authorization
codes MUST be single-use). RFC 6749 §10.5 (MUST revoke all tokens on code
replay detection) provides the full single-use obligation.
Node 3 (Scheme Planner): Verify whether single-use MUST is violated by the race
condition outcome; evaluate infrastructure framing; determine verdict.
Node 4 (Domain Processor): RFC 6749 §4.1.2 MUST: each authorization code may be
used at most once. Two successful exchanges occurred — the MUST is violated
regardless of why the race condition was possible. The implementation mechanism
(non-atomic check-and-set) is the root cause, but the observable violation (two
tokens issued for one code) is the MUST breach.
Node 5 (Applier): Infrastructure scalability framing tested: security requirements
do not have infrastructure exceptions; the MUST defines the required behavior; the
implementation must achieve it via atomic operations, database constraints, or
distributed locks. REJECTED.
Node 6 (Result Translator): MUST violated. RFC 6749 §4.1.2.
Node 7 (Arbiter Selector): RFC 6749 §4.1.2 MUST arbiter; REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. RFC 6749 §4.1.2 MUST single-use; race condition
caused two successful exchanges; MUST violated; infrastructure framing rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 6749 §4.1.2. Remediation:
atomic compare-and-swap (database unique constraint or SELECT FOR UPDATE); ensure
only one successful exchange per code regardless of concurrency.

## Pair[CB4-3.4-1258]/Reason

Nine-node trace: non-atomic single-use check → two successful code exchanges →
RFC 6749 §4.1.2 MUST single-use violated; infrastructure framing rejected; REPROCESS.

## Pair[CB4-3.4-1258]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms RFC 6749 §4.1.2 MUST is violated by
outcome (two tokens issued). Node 5 rejects infrastructure framing. Nodes 7–8
confirm MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1259]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1251]/A:
OIDC provider's TOTP MFA verification endpoint has no server-side rate limiting
on failed attempts. Attacker with compromised password can enumerate TOTP space
via automated requests. Team argues TOTP is still stronger than password-only.

## Pair[CB4-3.4-1259]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
multi-factor authentication brute-force protection. Security-verification-3.4 flow
selected.
Node 2 (Domain Translator): Relevant standard = ASVS V6.3.2 (SHOULD rate-limit
authenticator code entry). CWE-307 (improper restriction of excessive authentication
attempts) provides context.
Node 3 (Scheme Planner): Classify ASVS V6.3.2 obligation level (SHOULD vs MUST);
evaluate "password-first" scope limiter; determine verdict.
Node 4 (Domain Processor): ASVS V6.3.2: implementations SHOULD rate-limit
authenticator code entry attempts and lock out after repeated failures. SHOULD is
guidance-level. No unconditional MUST mandates a specific rate-limiting mechanism
for TOTP verification. The "password first" argument partially limits the attacker
scope; with no rate limiting, full TOTP space enumeration is feasible within the
validity window.
Node 5 (Applier): "Password-first" argument partially accepted as scope limiter;
SHOULD gap: guidance-level. No MUST violation.
Node 6 (Result Translator): No MUST violated. ASVS V6.3.2 SHOULD not met →
CONCERN.
Node 7 (Arbiter Selector): ASVS V6.3.2 SHOULD arbiter; REPROCESS threshold NOT
met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. ASVS V6.3.2 SHOULD rate-limit TOTP attempts; no MUST;
password-prerequisite partially accepted; guidance gap → CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=ASVS V6.3.2 / CWE-307. Remediation:
implement per-user exponential backoff with lockout after N failures (e.g., 5);
require support channel unlock to prevent persistent lockout attacks.

## Pair[CB4-3.4-1259]/Reason

Nine-node trace: TOTP no rate limiting → ASVS V6.3.2 SHOULD; no MUST; password-
prerequisite partially accepted; guidance gap → CONCERN.

## Pair[CB4-3.4-1259]/Phrasing-B

Pipeline verdict CONCERN. Node 4 confirms ASVS V6.3.2 is SHOULD-level. Node 5
partially accepts the password prerequisite. No MUST violated. CONCERN not REPROCESS.

---

## Pair[CB4-3.4-1260]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1252]/A:
JAR request object submitted with exp claim 30 days in the future. AS validates
signature and processes request. Team argues spec provides no numeric maximum.

## Pair[CB4-3.4-1260]/B

Node 1 (Domain Splitter): Input domain = OIDC JAR (JWT-Secured Authorization
Requests), sub-domain = request object expiry claim validation. Security-verification-
3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 9101 §10.3 (exp must not be
too far in the future; SHOULD limit validity period).
Node 3 (Scheme Planner): Evaluate whether RFC 9101 §10.3 contains an unconditional
MUST with a numeric threshold; assess 30-day expiry against qualitative guidance;
classify verdict.
Node 4 (Domain Processor): RFC 9101 §10.3: "the exp Claim should not be too far in
the future to limit their validity period." This uses SHOULD-level language; the
MUST NOT qualifier ("too far in the future") is qualitative without a numeric ceiling.
A 30-day expiry creates a meaningful replay window for intercepted request objects
but no numeric MUST maximum is violated. The AS team is partially correct about the
absence of a defined ceiling.
Node 5 (Applier): Spec-provides-no-maximum argument partially accepted for the
MUST question; SHOULD gap remains: guidance recommends short expiry; 30 days is
significantly beyond the authorization request interaction window. No MUST violated.
Node 6 (Result Translator): No MUST violated. RFC 9101 §10.3 SHOULD not met;
30-day replay window is a guidance concern → CONCERN.
Node 7 (Arbiter Selector): RFC 9101 §10.3 guidance arbiter; REPROCESS threshold
NOT met; CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 9101 §10.3 qualitative guidance; no numeric MUST
ceiling; 30-day replay window = guidance gap; AS team partially correct about
absence of defined maximum. CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 9101 §10.3. Remediation:
limit JAR exp to the expected authorization interaction window (typically 60–300
seconds); AS should enforce a maximum configurable JAR lifetime.

## Pair[CB4-3.4-1260]/Reason

Nine-node trace: JAR exp 30 days future → RFC 9101 §10.3 SHOULD/qualitative
guidance; no numeric MUST ceiling; 30-day replay window; AS team partially correct
→ CONCERN.

## Pair[CB4-3.4-1260]/Phrasing-B

Pipeline verdict CONCERN. Node 4 confirms RFC 9101 §10.3 is qualitative SHOULD-level
with no numeric MUST ceiling. Node 5 partially accepts the team's no-maximum argument.
Guidance gap on 30-day replay window → CONCERN not REPROCESS.
