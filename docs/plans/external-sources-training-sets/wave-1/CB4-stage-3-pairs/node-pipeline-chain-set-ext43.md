# Node Pipeline Chain Set Extension 43
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1279–1286 (16 instances — 8 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.11, RFC 7009 §2.1, RFC 7636 §4.4,
#          RFC 8725 §3.1 / ASVS V9.1.1, OIDC Core §3.1.2.2,
#          RFC 9449 / ASVS V10.4, CWE-200 / ASVS V8.3.1,
#          OIDC Core §8.2
# Running total after this file: 2572 instances (1286 base pairs)
# Formula check: 120 + (1286 − 60) × 2 = 120 + 2452 = 2572 ✓

---

## Pair[CB4-3.4-1279]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1271]/A:
RP requests max_age=300; AS includes auth_time indicating 7,200-second-old
authentication; RP validates signature and standard claims but does not check
auth_time against max_age; RP team argues AS bears max_age enforcement responsibility.

## Pair[CB4-3.4-1279]/B

Node 1 (Domain Splitter): Input domain = OIDC Relying Party, sub-domain = ID token
auth_time/max_age enforcement at RP. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §3.1.3.7.11 (RP MUST
reject ID token if auth_time + max_age < current_time; RP has independent obligation
from AS). Complementary to ext44 pattern (AS-side auth_time absence).
Node 3 (Scheme Planner): Determine whether §3.1.3.7.11 imposes a MUST on the RP
specifically; evaluate AS-responsibility argument; assess whether partial validation
(signature + standard claims) satisfies the spec.
Node 4 (Domain Processor): OIDC Core §3.1.3.7.11: "If a max_age request was made
and the auth_time Claim is present in the ID Token, the Client MUST validate the
auth_time Claim value and MUST reject the ID Token if it is more than max_age seconds
prior to the issue of the ID Token." auth_time 7200 seconds > max_age 300 seconds;
RP did not perform this check; MUST violated.
Node 5 (Applier): AS-responsibility argument tested: §3.1.3.7.11 explicitly assigns
the check to the RP ("the Client MUST"); RP's obligation is independent of the AS's
enforcement; REJECTED.
Node 6 (Result Translator): MUST violated. OIDC Core §3.1.3.7.11.
Node 7 (Arbiter Selector): OIDC Core §3.1.3.7.11 MUST arbiter; REPROCESS threshold
met.
Node 8 (Arbiter): REPROCESS. OIDC Core §3.1.3.7.11 MUST: RP must reject ID token
if auth_time exceeds max_age; RP omitted this check; AS-responsibility argument
rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Core §3.1.3.7.11. Remediation:
add auth_time/max_age comparison to RP ID token validation pipeline; reject tokens
where current_time − auth_time > max_age.

## Pair[CB4-3.4-1279]/Reason

Nine-node trace: RP auth_time check absent → OIDC Core §3.1.3.7.11 MUST RP reject
when auth_time + max_age < current_time; AS-responsibility argument rejected;
REPROCESS.

## Pair[CB4-3.4-1279]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms OIDC Core §3.1.3.7.11 MUST assigns
auth_time validation to the RP. Node 5 rejects AS-responsibility. Nodes 7–8 confirm
MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1280]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1272]/A:
Revocation endpoint only accepts JSON bodies; RFC 7009 clients sending
application/x-www-form-urlencoded receive 415. Team argues JSON is modern and
preferred.

## Pair[CB4-3.4-1280]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 token revocation, sub-domain =
revocation endpoint request encoding requirement. Security-verification-3.4 flow
selected.
Node 2 (Domain Translator): Relevant standard = RFC 7009 §2.1 (revocation request
MUST use HTTP POST with application/x-www-form-urlencoded body).
Node 3 (Scheme Planner): Verify whether RFC 7009 §2.1 form-encoded body is MUST-
level; evaluate whether JSON substitution satisfies or violates the MUST.
Node 4 (Domain Processor): RFC 7009 §2.1: "The client constructs the request body
by adding the following parameters using the
'application/x-www-form-urlencoded' format with a character encoding of UTF-8."
This is an explicit MUST for the encoding format. Rejecting conformant form-encoded
requests constitutes RFC non-conformance.
Node 5 (Applier): JSON-is-modern argument tested: protocol MUSTs cannot be overridden
by encoding preferences; RFC 7009 §2.1 must be satisfied by any conformant
implementation. REJECTED.
Node 6 (Result Translator): MUST violated. RFC 7009 §2.1.
Node 7 (Arbiter Selector): RFC 7009 §2.1 MUST arbiter; REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. RFC 7009 §2.1 MUST: revocation endpoint must accept
application/x-www-form-urlencoded; rejecting it violates MUST; JSON substitution
rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 7009 §2.1. Remediation:
accept application/x-www-form-urlencoded at revocation endpoint per RFC 7009 §2.1;
JSON may optionally be supported additionally but cannot replace form-encoded.

## Pair[CB4-3.4-1280]/Reason

Nine-node trace: revocation endpoint rejects form-encoded body → RFC 7009 §2.1
MUST accept application/x-www-form-urlencoded; JSON-modern argument rejected;
REPROCESS.

## Pair[CB4-3.4-1280]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms RFC 7009 §2.1 MUST form-encoded body.
Node 5 rejects JSON substitution. Nodes 7–8 confirm MUST violation. REPROCESS.

---

## Pair[CB4-3.4-1281]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1273]/A:
Client includes code_challenge (S256) in auth request; AS stores it; at token
exchange, code_verifier is absent; AS issues token without enforcing verification;
team argues client_secret provides equivalent protection.

## Pair[CB4-3.4-1281]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 PKCE, sub-domain = code
challenge/verifier enforcement when challenge was present in auth request.
Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 7636 §4.4 (when code_challenge
in auth request, AS MUST require code_verifier at token exchange and MUST verify
BASE64URL(SHA256(code_verifier)) = stored code_challenge).
Node 3 (Scheme Planner): Verify whether RFC 7636 §4.4 MUST applies when challenge
was initiated; evaluate client_secret substitution argument.
Node 4 (Domain Processor): RFC 7636 §4.4 (×2 MUST): (1) if code_challenge was
present in authorization request, the AS MUST require code_verifier in the token
request; (2) the AS MUST verify code_verifier against the stored code_challenge.
The AS stored the challenge but failed to require the verifier and enforce
verification. Both MUSTs are violated.
Node 5 (Applier): client_secret equivalence argument tested: RFC 7636 §4.4 applies
independently; PKCE provides code binding protection complementary to client
authentication; the two mechanisms address different attack surfaces; REJECTED.
Node 6 (Result Translator): MUST violated (×2). RFC 7636 §4.4.
Node 7 (Arbiter Selector): RFC 7636 §4.4 MUST (×2) arbiter; REPROCESS threshold
met.
Node 8 (Arbiter): REPROCESS. RFC 7636 §4.4 MUST (×2): code_verifier required and
must be verified when challenge was in auth request; both MUSTs violated; client_secret
argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=RFC 7636 §4.4. Remediation: when
code_challenge stored with auth code, require code_verifier at token exchange and
verify BASE64URL(SHA256(verifier)) = stored challenge; reject on mismatch or
absence.

## Pair[CB4-3.4-1281]/Reason

Nine-node trace: code_challenge stored; token exchange accepted without code_verifier
→ RFC 7636 §4.4 MUST (×2) violated; client_secret equivalence rejected; REPROCESS.

## Pair[CB4-3.4-1281]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms RFC 7636 §4.4 MUST (×2). Node 5
rejects client_secret substitution. Nodes 7–8 confirm MUST violation (×2). REPROCESS.

---

## Pair[CB4-3.4-1282]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1274]/A:
Same RS256 signing key used for both JWT access tokens and OIDC ID tokens.
Team argues no RFC explicitly prohibits shared keys; reviewer notes token confusion
risk.

## Pair[CB4-3.4-1282]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 / OIDC key management, sub-domain =
signing key isolation between token types. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = RFC 8725 §3.1 (cross-purpose key
usage caution; verify audience when key shared). ASVS V9.1.1 guidance: separate
keys per token type recommended.
Node 3 (Scheme Planner): Determine whether RFC 8725 §3.1 or ASVS V9.1.1 constitute
MUST-level prohibitions; evaluate partial mitigations (typ, aud); classify verdict.
Node 4 (Domain Processor): RFC 8725 §3.1 cautions against cross-purpose key use
and specifies that audience validation MUST be applied when the same key is used
for multiple purposes — but does not provide an unconditional MUST prohibiting
shared keys. ASVS V9.1.1 recommends separate keys as guidance. The `typ` and `aud`
claims provide partial discrimination between access tokens and ID tokens.
Node 5 (Applier): No MUST found for key isolation; RFC 8725 §3.1 audience validation
requirement (partially satisfied by aud checks); guidance-level gap for key
isolation.
Node 6 (Result Translator): No MUST violated. RFC 8725 §3.1 / ASVS V9.1.1 guidance
gap; token confusion risk partially mitigated → CONCERN.
Node 7 (Arbiter Selector): Guidance-level arbiter; REPROCESS threshold NOT met;
CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 8725 §3.1 / ASVS V9.1.1 guidance; no MUST prohibiting
shared signing key; typ + aud provide partial mitigation; guidance-level gap. CONCERN
not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 8725 §3.1 / ASVS V9.1.1.
Remediation: use separate signing key pairs per token type; enables independent
rotation and eliminates cross-type confusion attack surface.

## Pair[CB4-3.4-1282]/Reason

Nine-node trace: shared signing key for AT + ID token → RFC 8725 §3.1 / ASVS V9.1.1
guidance; no MUST prohibiting; typ + aud partial mitigation → CONCERN.

## Pair[CB4-3.4-1282]/Phrasing-B

Pipeline verdict CONCERN. Node 4 finds no unconditional MUST prohibiting shared
keys. Node 5 confirms guidance-level gap. Node 8 classifies as CONCERN. CONCERN
not REPROCESS.

---

## Pair[CB4-3.4-1283]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1275]/A:
User has existing authentication session and prior consent; AS receives
prompt=consent; skips consent screen because prior consent is on file; issues
ID token without re-consent. Team argues unnecessary friction is avoided.

## Pair[CB4-3.4-1283]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
prompt parameter enforcement — prompt=consent. Security-verification-3.4 flow
selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §3.1.2.2 (prompt=consent
MUST cause AS to prompt End-User for consent even if prior consent exists).
Node 3 (Scheme Planner): Verify whether §3.1.2.2 MUST applies unconditionally to
prompt=consent regardless of prior consent; evaluate user-friction argument.
Node 4 (Domain Processor): OIDC Core §3.1.2.2: "consent — The Authorization Server
MUST prompt the End-User for consent before returning information to the Client."
The MUST is stated without a prior-consent exception. The RP explicitly requested
re-consent; the AS overrode the RP's instruction with its own cached-consent logic.
Node 5 (Applier): User-friction argument tested: MUST is unconditional; RP's
explicit re-consent instruction takes precedence over AS operational optimizations.
REJECTED.
Node 6 (Result Translator): MUST violated. OIDC Core §3.1.2.2.
Node 7 (Arbiter Selector): OIDC Core §3.1.2.2 MUST arbiter; REPROCESS threshold
met.
Node 8 (Arbiter): REPROCESS. OIDC Core §3.1.2.2 MUST: prompt=consent requires
consent screen regardless of prior consent; user-friction argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Core §3.1.2.2. Remediation:
when prompt=consent received, display consent screen unconditionally; do not skip
based on cached prior consent.

## Pair[CB4-3.4-1283]/Reason

Nine-node trace: prompt=consent received; AS skips consent screen based on prior
consent → OIDC Core §3.1.2.2 MUST; user-friction argument rejected; REPROCESS.

## Pair[CB4-3.4-1283]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms OIDC Core §3.1.2.2 MUST has no prior-
consent exception. Node 5 rejects user-friction argument. Nodes 7–8 confirm MUST
violation. REPROCESS.

---

## Pair[CB4-3.4-1284]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1276]/A:
AS uses both DPoP and mTLS for token endpoint auth. Issued access token contains
cnf/jkt (DPoP key binding) but no cnf/x5t#S256 (mTLS binding). Reviewer asks
whether partial binding constitutes a violation.

## Pair[CB4-3.4-1284]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 sender-constrained tokens, sub-
domain = dual-mechanism token binding when both DPoP and mTLS are verified.
Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standards = RFC 9449 §6 (DPoP: AS MUST embed
cnf/jkt when DPoP proof accepted at token endpoint); RFC 8705 §3 (mTLS binding:
cnf/x5t#S256 when certificate-bound tokens used). ASVS V10.4 guidance on token
binding.
Node 3 (Scheme Planner): Verify whether RFC 9449 §6 MUST is satisfied by cnf/jkt
presence; determine whether RFC 8705 §3 also creates a MUST when mTLS cert was
verified; classify the absent mTLS binding.
Node 4 (Domain Processor): RFC 9449 §6 MUST: cnf/jkt embedded — this MUST is
satisfied. RFC 8705 §3: when mTLS client authentication is used, the AS SHOULD
embed cnf/x5t#S256 in the token — this is SHOULD-level for the mTLS binding. No
RFC provides a MUST to bind to both when both are used simultaneously. ASVS V10.4
recommends dual binding as guidance.
Node 5 (Applier): RFC 9449 MUST is satisfied. RFC 8705 SHOULD for mTLS binding
not met when both mechanisms are used — guidance-level gap. No MUST for dual binding.
Node 6 (Result Translator): No MUST violated. RFC 8705 §3 SHOULD not fully met;
ASVS V10.4 guidance gap → CONCERN.
Node 7 (Arbiter Selector): Guidance-level arbiter; REPROCESS threshold NOT met;
CONCERN threshold met.
Node 8 (Arbiter): CONCERN. RFC 9449 §6 MUST satisfied (cnf/jkt present); RFC 8705
§3 SHOULD for mTLS binding not met; no MUST for dual binding; ASVS V10.4 guidance
gap. CONCERN not REPROCESS.
Node 9 (Formatter): Verdict=CONCERN. Standard=RFC 8705 §3 / ASVS V10.4. Remediation:
when both DPoP and mTLS are verified, embed both cnf/jkt and cnf/x5t#S256 in issued
tokens.

## Pair[CB4-3.4-1284]/Reason

Nine-node trace: cnf/jkt present (RFC 9449 §6 MUST satisfied); cnf/x5t#S256 absent
(RFC 8705 §3 SHOULD not fully met; ASVS V10.4 guidance); no MUST for dual binding
→ CONCERN.

## Pair[CB4-3.4-1284]/Phrasing-B

Pipeline verdict CONCERN. Node 4 confirms RFC 9449 §6 MUST is satisfied. Node 5
finds RFC 8705 §3 SHOULD not fully met but no MUST for dual binding. CONCERN not
REPROCESS.

---

## Pair[CB4-3.4-1285]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1277]/A:
Token endpoint error response includes service account internal username in
error_description for all failed client_secret_basic authentication attempts.
Team argues detailed errors improve developer experience.

## Pair[CB4-3.4-1285]/B

Node 1 (Domain Splitter): Input domain = OAuth 2.0 token endpoint error response,
sub-domain = information disclosure in authentication error responses. Security-
verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = CWE-200 (Exposure of Sensitive
Information) and ASVS V8.3.1 (MUST NOT disclose internal system identifiers in
error responses). RFC 6749 §5.2 defines permitted error response fields.
Node 3 (Scheme Planner): Determine whether ASVS V8.3.1 / CWE-200 constitute a
MUST-level prohibition; evaluate developer-experience argument; identify what
constitutes sensitive information in this context.
Node 4 (Domain Processor): ASVS V8.3.1 MUST NOT: error responses MUST NOT include
internal identifiers (service account username, internal client names, infrastructure
hostnames) that aid reconnaissance. The error_description exposes the service account
username (svc-billing-prod-7) and client identifier to any unauthenticated caller
with a valid client_id. CWE-200: observable sensitive information in error responses
aids attackers.
Node 5 (Applier): Developer-experience argument tested: internal details belong in
server-side logs with correlation IDs; the external error response should provide
only the minimum required for client-side diagnosis (error=invalid_client); detailed
internal identifiers are not necessary for integration. REJECTED.
Node 6 (Result Translator): MUST violated. ASVS V8.3.1 / CWE-200.
Node 7 (Arbiter Selector): ASVS V8.3.1 / CWE-200 MUST arbiter; REPROCESS threshold
met.
Node 8 (Arbiter): REPROCESS. CWE-200 / ASVS V8.3.1 MUST NOT: internal account
names and identifiers disclosed to unauthenticated callers; developer-experience
argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=CWE-200 / ASVS V8.3.1. Remediation:
return error=invalid_client only in external response; log internal details (account
name, correlation ID) server-side; do not include infrastructure identifiers in any
externally visible field.

## Pair[CB4-3.4-1285]/Reason

Nine-node trace: service account username in error_description → CWE-200 / ASVS
V8.3.1 MUST NOT; developer-experience argument rejected; REPROCESS.

## Pair[CB4-3.4-1285]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms CWE-200 / ASVS V8.3.1 MUST NOT.
Node 5 rejects developer-experience justification. Nodes 7–8 confirm MUST violation.
REPROCESS.

---

## Pair[CB4-3.4-1286]/A

Full pipeline trace for the scenario in Pair[CB4-3.4-1278]/A:
AS normalizes sub to lowercase; directory has "Alice123" and "alice123" as distinct
users; both receive sub="alice123" in tokens; RS cannot distinguish them. Team
argues lowercase normalization is standard practice.

## Pair[CB4-3.4-1286]/B

Node 1 (Domain Splitter): Input domain = OIDC Authorization Server, sub-domain =
sub claim uniqueness under case normalization. Security-verification-3.4 flow selected.
Node 2 (Domain Translator): Relevant standard = OIDC Core §8.2 (sub values must be
locally unique per issuer; sub is case-sensitive per RFC 7519 §4.1.2). Security
context: sub collision causes identity confusion at RS.
Node 3 (Scheme Planner): Verify whether §8.2 uniqueness requirement is MUST-level;
evaluate normalization-practice argument; assess whether distinctness in post-
normalization space satisfies the requirement.
Node 4 (Domain Processor): OIDC Core §8.2: "Subject Identifier: A locally unique
and never reassigned identifier within the Issuer for the End-User." RFC 7519
§4.1.2: sub is case-sensitive. "Alice123" and "alice123" are distinct in the
directory but collide after normalization — sub uniqueness is violated. Two different
users share the same sub in their tokens.
Node 5 (Applier): Normalization-as-standard-practice argument tested: normalization
is only valid when pre-normalization identifiers are guaranteed distinct in the
normalized domain; they are not; the argument fails to account for the collision.
REJECTED.
Node 6 (Result Translator): MUST violated. OIDC Core §8.2 sub uniqueness.
Node 7 (Arbiter Selector): OIDC Core §8.2 MUST arbiter; REPROCESS threshold met.
Node 8 (Arbiter): REPROCESS. OIDC Core §8.2 MUST: sub must be locally unique per
issuer; normalization collapses two distinct users to same sub; uniqueness violated;
normalization argument rejected.
Node 9 (Formatter): Verdict=REPROCESS. Standard=OIDC Core §8.2. Remediation: ensure
sub uniqueness in the normalized domain before applying normalization; use opaque
identifiers (UUID v4 PPIDs) that are inherently unique and case-insensitive.

## Pair[CB4-3.4-1286]/Reason

Nine-node trace: lowercase normalization collapses two distinct users to sub="alice123"
→ OIDC Core §8.2 MUST unique sub per issuer; normalization argument rejected; REPROCESS.

## Pair[CB4-3.4-1286]/Phrasing-B

Pipeline verdict REPROCESS. Node 4 confirms OIDC Core §8.2 MUST. Node 5 rejects
normalization argument. Nodes 7–8 confirm MUST violation. REPROCESS.
