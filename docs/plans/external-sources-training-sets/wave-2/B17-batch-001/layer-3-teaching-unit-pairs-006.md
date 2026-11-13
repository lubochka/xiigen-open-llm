# Layer 3 — Teaching Unit Pairs (006)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: teaching-unit

---

## B17-TU-006-001 — "token" → SECURITY; "oauth" → AUTHORIZATION; never conflate the two

**Two separate rules, two separate predicates:**

**Rule 6:** "token" → SECURITY
- Rationale: Tokens (access tokens, refresh tokens, API tokens) are security artifacts. Their secure storage, transmission, and validation is a security concern.

**Rule 7:** "oauth" → AUTHORIZATION
- Rationale: OAuth (Open Authorization) is an authorization protocol. It governs who has permission to access what. It is an authorization concern, not a general security concern.

**"OAuth token" → both fire independently:**
- "oauth" triggers AUTHORIZATION
- "token" triggers SECURITY
- Together: both SECURITY and AUTHORIZATION fire

---

## B17-TU-006-002 — Cannot substitute SECURITY for AUTHORIZATION or vice versa

**Wrong (conflation):** "implement OAuth" → SECURITY (WRONG — oauth is AUTHORIZATION)
**Wrong (conflation):** "implement token refresh" → AUTHORIZATION (WRONG — token is SECURITY)

**Correct:**
- "implement OAuth" → AUTHORIZATION (oauth → AUTHORIZATION)
- "implement token refresh" → SECURITY (token → SECURITY)
- "implement OAuth token refresh" → SECURITY + AUTHORIZATION (both tokens and oauth)

---

## B17-TU-006-003 — "authentication" vs "authorization": not the same predicate trigger

**English language note:**
- "authentication" = verifying identity (who you are)
- "authorization" = verifying permissions (what you can do)

**B17 predicate mapping (different from English meaning):**
- "authentication" → SECURITY (Rule 8)
- "oauth" → AUTHORIZATION (Rule 7)
- "authorization" (the English word, without "oauth") → does NOT fire AUTHORIZATION predicate

**This is counterintuitive:** The English word "authorization" does NOT fire the AUTHORIZATION predicate. Only "oauth" fires AUTHORIZATION. The AUTHORIZATION predicate is specifically about OAuth protocol.

---

## B17-TU-006-004 — Summary: three security-domain words and their predicates

| Word | Predicate | Route | Reason |
|---|---|---|---|
| token | SECURITY | 3.4 | Token is a security artifact |
| authentication | SECURITY | 3.4 | Authentication is identity verification |
| oauth / OAuth | AUTHORIZATION | 3.4 | OAuth is authorization protocol |
| secure | SECURITY | 3.4 | Security verb |
| validate signature | SECURITY | 3.4 | Security validation pattern |

All route to 3.4. Different predicates, different flows (SecurityAuditFlow34 vs AuthorizationFlow34), same sub-phase.
