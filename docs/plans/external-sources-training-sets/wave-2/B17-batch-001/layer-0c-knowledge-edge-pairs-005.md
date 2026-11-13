# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-005-001 — "token" → SECURITY; "oauth" → AUTHORIZATION; independent firing

**Rule 6:** "token" fires SECURITY.
**Rule 7:** "oauth" (case-insensitive) fires AUTHORIZATION.

**These are two completely independent predicates.** The presence of one does not imply the other. Either can fire alone. Both can fire together.

**Vocabulary isolation:**
- "token" → SECURITY ONLY. Does NOT fire AUTHORIZATION.
- "oauth" → AUTHORIZATION ONLY. Does NOT fire SECURITY (unless "authentication" also appears).
- "OAuth token" → both fire: "oauth" → AUTHORIZATION, "token" → SECURITY.

---

## B17-0C-005-002 — "token" firing: comprehensive examples

| Input fragment | SECURITY fires? | Reason |
|---|---|---|
| "implement token refresh" | Yes | "token" present |
| "store the access token" | Yes | "token" present |
| "JWT token validation" | Yes | "token" present (JWT: JSON Web Token) |
| "API token authentication" | Yes | "token" + "authentication" (double SECURITY signal) |
| "implement OAuth" (no token) | No | No SECURITY trigger; only AUTHORIZATION |
| "implement authentication" | Yes | "authentication" fires SECURITY |
| "implement token-based auth" | Yes | "token" fires SECURITY |

---

## B17-0C-005-003 — "oauth" firing: case-insensitive examples

| Input fragment | AUTHORIZATION fires? | Reason |
|---|---|---|
| "implement OAuth connector" | Yes | "OAuth" (capital O) |
| "implement oauth flow" | Yes | "oauth" (lowercase) |
| "implement OAUTH redirect" | Yes | "OAUTH" (uppercase) |
| "Auth0 OAuth2 integration" | Yes | "OAuth2" contains "oauth" |
| "implement authentication" | No | "authentication" → SECURITY, not AUTHORIZATION |
| "implement authorization logic" | No | "authorization" (without "oauth") → not AUTHORIZATION predicate |

**Important:** The AUTHORIZATION predicate in B17 fires specifically on the "oauth" token. "authorization" (the English word) does not itself fire the AUTHORIZATION predicate — "oauth" does.

---

## B17-0C-005-004 — "OAuth token" fires both SECURITY and AUTHORIZATION

**Input:** "Build the connector and implement the OAuth token refresh flow."

**Firing breakdown:**
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION
- "token" → SECURITY

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**Teaching point:** A single phrase "OAuth token" contributes to two different predicates. Each token ("oauth", "token") fires its own predicate independently. This is additive, not exclusive.
