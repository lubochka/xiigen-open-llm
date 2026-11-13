# Node Cross-Source Set Extension 42
# Sub-phase: 3.4 Security and Authorization Model
# Pair ID prefix: CB4-3.4-
# Sequences: 1287–1296 (20 instances — 10 base pairs × 2 phrasings)
# Sources: OIDC Core §3.1.3.7.11, RFC 7009 §2.1, RFC 7636 §4.4,
#          RFC 8725 §3.1 / ASVS V9.1.1, OIDC Core §3.1.2.2,
#          RFC 9449 / ASVS V10.4, CWE-200 / ASVS V8.3.1,
#          OIDC Core §8.2, RFC 7009 §1, RFC 9101 §10.3, NIST SSDF PO.1
# Running total after this file: 2592 instances (1296 base pairs)
# Formula check: 120 + (1296 − 60) × 2 = 120 + 2472 = 2592 ✓

---

## Pair[CB4-3.4-1287]/A

An OIDC authorization server uses the same RS256 key pair to sign both JWT
access tokens and OIDC ID tokens. The same AS accepts DPoP and mTLS simultaneously
for token endpoint authentication but binds issued tokens only to the DPoP key,
omitting the mTLS certificate fingerprint. A security review flags both observations.
The team asks whether two concurrent guidance gaps require a REPROCESS classification.

## Pair[CB4-3.4-1287]/B

CONCERN. Two guidance-level observations: (1) shared signing key for access tokens
and ID tokens — RFC 8725 §3.1 / ASVS V9.1.1 guidance recommends separate keys
per token type; `typ` and `aud` provide partial discrimination; no unconditional
MUST prohibiting shared keys; (2) DPoP + mTLS partial binding — RFC 9449 §6 MUST
for DPoP binding is satisfied (cnf/jkt present); RFC 8705 §3 SHOULD for mTLS
cnf/x5t#S256 binding when both mechanisms are used is a guidance gap; no MUST for
dual binding. The graduation rule applies: N CONCERNs do not escalate to REPROCESS
regardless of count. CONCERN.

## Pair[CB4-3.4-1287]/Reason

Two guidance-level gaps: shared signing key (RFC 8725 §3.1 / ASVS V9.1.1 guidance)
+ DPoP+mTLS partial binding (RFC 8705 §3 SHOULD not fully met); graduation rule:
N CONCERNs ≠ REPROCESS → CONCERN.

## Pair[CB4-3.4-1287]/Phrasing-B

Verdict CONCERN. Neither finding has an unconditional MUST anchor: key sharing is
guidance-level (RFC 8725 §3.1 / ASVS V9.1.1), and dual-binding when both mechanisms
are used is SHOULD-level (RFC 8705 §3). Graduation rule applies. CONCERN.

---

## Pair[CB4-3.4-1288]/A

An OIDC AS uses the same RS256 signing key for both JWT access tokens and OIDC
ID tokens. The same AS's revocation endpoint lacks access token revocation support —
only refresh tokens can be revoked. Both are noted in an audit. An engineer
argues that two simultaneous guidance gaps in key management and lifecycle control
together constitute a REPROCESS-level system deficiency.

## Pair[CB4-3.4-1288]/B

CONCERN. Two guidance-level observations: (1) shared signing key — RFC 8725 §3.1 /
ASVS V9.1.1 guidance; no unconditional MUST prohibiting shared use; (2) access
token revocation absent — RFC 7009 §1 SHOULD; 15-minute TTL provides partial
compensating control; no MUST. The engineer's system-deficiency escalation argument
is rejected: the graduation rule is categorical. N CONCERNs = CONCERN regardless
of thematic overlap (both concern token lifecycle), count, or operational framing.
No MUST-level violation is present. CONCERN.

## Pair[CB4-3.4-1288]/Reason

Shared signing key (RFC 8725 §3.1 guidance) + access token revocation absent
(RFC 7009 §1 SHOULD); both guidance-level; graduation rule categorical → CONCERN.

## Pair[CB4-3.4-1288]/Phrasing-B

Verdict CONCERN. Both findings are guidance-level. The graduation rule forecloses
system-deficiency escalation arguments. CONCERN.

---

## Pair[CB4-3.4-1289]/A

An OIDC provider has no formal security requirements documentation for its
authorization server (NIST SSDF PO.1 gap). A concurrent investigation confirms
that the AS normalizes all `sub` claim values to lowercase before embedding
them in tokens — the user directory contains "Alice123" and "alice123" as
distinct users, but both receive `sub="alice123"` in their tokens, making them
indistinguishable at any resource server. The team proposes treating PO.1 as
administrative backlog and the sub collision as a future refactoring item.

## Pair[CB4-3.4-1289]/B

REPROCESS. PO.1 absence alone is CONCERN. The PO.1 escalation rule applies: PO.1
absence combined with a traceable production MUST violation escalates to REPROCESS.
OIDC Core §8.2 MUST requires sub values to be locally unique per issuer; the
lowercase normalization collapses two distinct directory users into the same sub
in their tokens, creating an active uniqueness violation in production. The PO.1
gap means the sub uniqueness obligation was never formally tracked. The team's
backlog-and-refactoring framing is rejected: the PO.1 escalation rule mandates
REPROCESS when PO.1 is traceable to an active MUST violation. REPROCESS.

## Pair[CB4-3.4-1289]/Reason

PO.1 gap (CONCERN alone) + active MUST violation (OIDC Core §8.2 MUST unique sub;
lowercase normalization collapses two distinct users to same sub) = PO.1 escalation
→ REPROCESS.

## Pair[CB4-3.4-1289]/Phrasing-B

Verdict REPROCESS. PO.1 escalation: PO.1 absence + traceable active MUST violation
= REPROCESS. OIDC Core §8.2 MUST is violated: two distinct users share the same
sub in production. REPROCESS.

---

## Pair[CB4-3.4-1290]/A

An AS uses the same RS256 signing key for access tokens and ID tokens. The same
OIDC relying party connected to this AS receives ID tokens with `auth_time`
indicating authentication 7,200 seconds ago when `max_age=300` was requested,
but the RP does not check `auth_time` against `max_age` in its validation pipeline.
The team notes the signing key guidance gap as lower severity and asks if it
mitigates the auth_time finding.

## Pair[CB4-3.4-1290]/B

REPROCESS. The shared signing key finding is CONCERN-level (RFC 8725 §3.1 /
ASVS V9.1.1 guidance; no MUST). The RP's absent auth_time/max_age check is
REPROCESS-level: OIDC Core §3.1.3.7.11 MUST requires the RP to check auth_time
against max_age and reject the ID token if auth_time + max_age < current_time;
auth_time 7200 > max_age 300; this check was omitted. REPROCESS overrides CONCERN
— the guidance-level key finding does not moderate or mitigate the MUST violation.
The two findings are independent. REPROCESS.

## Pair[CB4-3.4-1290]/Reason

Shared signing key (RFC 8725 §3.1 guidance, CONCERN) + RP auth_time check absent
(OIDC Core §3.1.3.7.11 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1290]/Phrasing-B

Verdict REPROCESS. Shared signing key is guidance-level (CONCERN). Absent RP
auth_time/max_age check violates OIDC Core §3.1.3.7.11 MUST. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1291]/A

An AS's DPoP+mTLS token binding only embeds cnf/jkt in tokens (mTLS cnf/x5t#S256
absent). The same AS's revocation endpoint returns `415 Unsupported Media Type`
for `application/x-www-form-urlencoded` revocation requests, accepting only JSON.
The team asks whether the binding guidance gap and the revocation encoding issue
need to be addressed together.

## Pair[CB4-3.4-1291]/B

REPROCESS. The DPoP+mTLS partial binding is CONCERN-level (RFC 9449 §6 MUST
satisfied for DPoP; RFC 8705 §3 SHOULD not fully met for mTLS binding; no MUST
for dual binding). The revocation JSON-only behavior is REPROCESS-level: RFC 7009
§2.1 MUST requires the revocation endpoint to accept
`application/x-www-form-urlencoded` bodies; rejecting form-encoded requests
violates this MUST unconditionally. REPROCESS overrides CONCERN. The guidance gap
in token binding does not moderate the MUST violation in the revocation endpoint.
REPROCESS.

## Pair[CB4-3.4-1291]/Reason

DPoP+mTLS partial binding (RFC 8705 §3 SHOULD, CONCERN) + revocation JSON-only
(RFC 7009 §2.1 MUST form-encoded, REPROCESS); REPROCESS overrides CONCERN →
REPROCESS.

## Pair[CB4-3.4-1291]/Phrasing-B

Verdict REPROCESS. DPoP+mTLS partial binding is guidance-level (CONCERN). Rejecting
form-encoded revocation bodies violates RFC 7009 §2.1 MUST. REPROCESS overrides
CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1292]/A

An AS uses the same RS256 signing key for access tokens and ID tokens. The same
AS has stored `code_challenge` for authorization codes but does not enforce
`code_verifier` at token exchange — exchanges proceed without the verifier, and
the stored challenge is never compared. The team considers the signing key finding
a low-priority configuration issue unrelated to the PKCE enforcement problem.

## Pair[CB4-3.4-1292]/B

REPROCESS. The shared signing key is CONCERN-level (guidance gap; no MUST). The
PKCE non-enforcement is REPROCESS-level: RFC 7636 §4.4 MUST (×2) — when
code_challenge was in the authorization request, the AS MUST require code_verifier
at token exchange and MUST verify it cryptographically; both MUSTs are violated.
REPROCESS overrides CONCERN — the guidance gap in key isolation has no bearing on
the MUST violation in PKCE enforcement. The two findings are independent. REPROCESS.

## Pair[CB4-3.4-1292]/Reason

Shared signing key (RFC 8725 §3.1 guidance, CONCERN) + PKCE code_verifier not
required/verified (RFC 7636 §4.4 MUST ×2, REPROCESS); REPROCESS overrides CONCERN
→ REPROCESS.

## Pair[CB4-3.4-1292]/Phrasing-B

Verdict REPROCESS. Shared signing key is a guidance gap (CONCERN). PKCE non-
enforcement violates RFC 7636 §4.4 MUST (×2). REPROCESS overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1293]/A

An AS's token endpoint does not implement mTLS certificate fingerprint binding in
tokens when both DPoP and mTLS are used (cnf/x5t#S256 absent). The same AS skips
the consent screen when `prompt=consent` is received, because a prior consent record
exists. The team asks whether the partial token binding guidance gap and the consent
screen finding require different handling.

## Pair[CB4-3.4-1293]/B

REPROCESS. The DPoP+mTLS partial binding is CONCERN-level (RFC 8705 §3 SHOULD not
fully met; no MUST for dual binding when both mechanisms are used). The prompt=consent
bypass is REPROCESS-level: OIDC Core §3.1.2.2 MUST requires the AS to display the
consent screen when `prompt=consent` is received, regardless of prior consent on file;
the AS skipped it in violation of this MUST. REPROCESS overrides CONCERN — the
guidance gap in token binding does not moderate the MUST violation. The team is
correct that they require different handling: the binding gap is remediation-track;
the prompt=consent violation is REPROCESS. REPROCESS.

## Pair[CB4-3.4-1293]/Reason

DPoP+mTLS partial binding (RFC 8705 §3 SHOULD, CONCERN) + prompt=consent skipped
(OIDC Core §3.1.2.2 MUST, REPROCESS); REPROCESS overrides CONCERN → REPROCESS.

## Pair[CB4-3.4-1293]/Phrasing-B

Verdict REPROCESS. DPoP+mTLS binding gap is guidance-level (CONCERN). Bypassing
consent screen on prompt=consent violates OIDC Core §3.1.2.2 MUST. REPROCESS
overrides CONCERN. REPROCESS.

---

## Pair[CB4-3.4-1294]/A

An OIDC authorization server is reviewed against all eight ext49 control points:
OIDC relying parties check `auth_time` against `max_age` and reject tokens where
`current_time − auth_time > max_age`; the revocation endpoint accepts
`application/x-www-form-urlencoded` bodies per RFC 7009 §2.1; the AS stores
`code_challenge` and requires and verifies `code_verifier` at token exchange;
separate RS256 key pairs are used for access tokens and ID tokens; the AS displays
the consent screen when `prompt=consent` is received regardless of prior consent;
tokens are bound to both DPoP (cnf/jkt) and mTLS (cnf/x5t#S256) when both
mechanisms are used; error responses from the token endpoint contain only
`error=invalid_client` without internal account names; and `sub` values are opaque
UUID v4 PPIDs that are unique by construction before and after any normalization.
No anomalies are detected.

## Pair[CB4-3.4-1294]/B

PASS. All eight ext49 control points are satisfied: (1) RP auth_time/max_age check
per OIDC Core §3.1.3.7.11 MUST; (2) form-encoded revocation per RFC 7009 §2.1
MUST; (3) PKCE code_verifier required and verified per RFC 7636 §4.4 MUST (×2);
(4) separate signing keys per token type per RFC 8725 §3.1 / ASVS V9.1.1 guidance
(exceeds guidance floor); (5) prompt=consent enforced per OIDC Core §3.1.2.2 MUST;
(6) dual DPoP+mTLS binding per RFC 9449 §6 MUST + RFC 8705 §3 SHOULD (exceeds
SHOULD floor); (7) minimum-disclosure error responses per CWE-200 / ASVS V8.3.1
MUST; (8) UUID v4 PPID sub values guarantee uniqueness per OIDC Core §8.2 MUST.
No violations or guidance gaps found. PASS.

## Pair[CB4-3.4-1294]/Reason

All eight ext49 controls compliant: OIDC Core §3.1.3.7.11 (auth_time), RFC 7009
§2.1 (revocation form-encoded), RFC 7636 §4.4 (PKCE), RFC 8725 §3.1 (separate keys),
OIDC Core §3.1.2.2 (prompt=consent), RFC 9449+RFC 8705 (dual binding), CWE-200/
ASVS V8.3.1 (error disclosure), OIDC Core §8.2 (sub uniqueness) → PASS.

## Pair[CB4-3.4-1294]/Phrasing-B

Verdict PASS. All eight ext49 control points satisfied: auth_time validation (OIDC
Core §3.1.3.7.11), form-encoded revocation (RFC 7009 §2.1), PKCE enforcement
(RFC 7636 §4.4), separate signing keys (RFC 8725 §3.1), prompt=consent honored
(OIDC Core §3.1.2.2), dual token binding (RFC 9449 §6 + RFC 8705 §3), minimum-
disclosure errors (ASVS V8.3.1), unique sub (OIDC Core §8.2). PASS.

---

## Pair[CB4-3.4-1295]/A

An AS accepts DPoP and mTLS simultaneously but issues tokens bound only to the
DPoP key (cnf/jkt present; cnf/x5t#S256 absent). The same AS accepts JAR
request objects with `exp` values set 30 days in the future without rejection.
An auditor notes that both gaps involve incomplete enforcement of binding or
lifetime constraints and argues that this pattern constitutes a systemic weakness
requiring REPROCESS.

## Pair[CB4-3.4-1295]/B

CONCERN. Two guidance-level observations: (1) DPoP+mTLS partial binding — RFC
9449 §6 MUST for DPoP binding is satisfied (cnf/jkt present); RFC 8705 §3 SHOULD
for mTLS binding not fully met when both mechanisms are used; no MUST for dual
binding; (2) JAR exp 30-day — RFC 9101 §10.3 qualitative SHOULD; no numeric MUST
ceiling. The auditor's systemic-pattern escalation argument is rejected: the
graduation rule is categorical — N CONCERNs = CONCERN regardless of thematic
pattern, enforcement category, or count. Only a MUST-level violation triggers
REPROCESS. CONCERN.

## Pair[CB4-3.4-1295]/Reason

DPoP+mTLS partial binding (RFC 8705 §3 SHOULD, CONCERN) + JAR exp 30 days
(RFC 9101 §10.3 SHOULD, CONCERN); graduation rule: N CONCERNs ≠ REPROCESS;
systemic-pattern argument rejected → CONCERN.

## Pair[CB4-3.4-1295]/Phrasing-B

Verdict CONCERN. Both findings are SHOULD-level guidance gaps. The graduation rule
forecloses pattern-based escalation. CONCERN.

---

## Pair[CB4-3.4-1296]/A

An AS uses the same RS256 key pair for signing both access tokens and ID tokens.
The same AS's token endpoint returns `400 Bad Request` error responses that include
the service account's internal username in the `error_description` field when
client authentication fails. The team notes the key sharing is a low-priority
item and focuses on addressing the error response separately.

## Pair[CB4-3.4-1296]/B

REPROCESS. The shared signing key is CONCERN-level (RFC 8725 §3.1 / ASVS V9.1.1
guidance; no unconditional MUST). The internal-username disclosure in error responses
is REPROCESS-level: CWE-200 and ASVS V8.3.1 MUST NOT expose internal system
identifiers (service account usernames) to unauthenticated callers in error responses.
REPROCESS overrides CONCERN — the team is correct to address them separately, but
REPROCESS overrides the guidance-level key finding for the overall verdict. REPROCESS.

## Pair[CB4-3.4-1296]/Reason

Shared signing key (RFC 8725 §3.1 guidance, CONCERN) + internal username in error
response (CWE-200 / ASVS V8.3.1 MUST NOT, REPROCESS); REPROCESS overrides CONCERN
→ REPROCESS.

## Pair[CB4-3.4-1296]/Phrasing-B

Verdict REPROCESS. Shared signing key is guidance-level (CONCERN). Internal username
in error_description violates CWE-200 / ASVS V8.3.1 MUST NOT. REPROCESS overrides
CONCERN. REPROCESS.
