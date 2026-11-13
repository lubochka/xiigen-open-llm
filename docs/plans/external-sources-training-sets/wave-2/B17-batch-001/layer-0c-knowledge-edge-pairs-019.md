# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Okta identity connector edges

---

## B17-0C-019-001 — Edge: "OAuth" fires AUTHORIZATION; "OIDC" does NOT fire AUTHORIZATION

**This is the critical Okta edge case.**

**Rule 7:** AUTHORIZATION fires on "OAuth" or "oauth" (case-insensitive). It does NOT fire on protocol names that implement OAuth: OIDC, PKCE, SAML, OpenID Connect (written out).

**Evidence table:**
| Input trigger | AUTHORIZATION fires? | Reason |
|---|---|---|
| "OAuth 2.0" | Yes | Literal "OAuth" present |
| "oauth flow" | Yes | Case-insensitive match |
| "OAUTH connector" | Yes | Case-insensitive match |
| "OIDC connector" | No | "OIDC" ≠ "OAuth" |
| "OpenID Connect" | No | Written-out form, not abbreviation "OAuth" |
| "SAML connector" | No | SAML is a separate standard, not "OAuth" |
| "PKCE flow" | No | PKCE is a method, not "OAuth" |

---

## B17-0C-019-002 — Edge: "authentication" fires SECURITY; "authorization" (English word) does NOT fire AUTHORIZATION

**Critical disambiguation:**

| Term | Predicate fired |
|---|---|
| "authentication" | SECURITY (Rule 8) |
| "authenticate" | SECURITY (Rule 8) |
| "OAuth" | AUTHORIZATION (Rule 7) |
| "authorization" (English word, not OAuth) | No predicate — English word is not the trigger |
| "authorization code flow" (OAuth concept) | AUTHORIZATION only if "OAuth" is also present |

**The trigger is the word "OAuth", not the English word "authorization".**

**Example:** "Build the authorization connector for the access control service."
- "Build" → IMPLEMENT
- "connector" → API_CONTRACT
- "authorization" (English word) → NO predicate (English word, not the OAuth trigger)
- Result: ["IMPLEMENT", "API_CONTRACT"]

---

## B17-0C-019-003 — Edge: Okta connector with "token introspection" → SECURITY fires

**Input:** "Create the Okta OIDC connector and implement the authentication token introspection endpoint."

**Edge assertions:**
- "Create" → IMPLEMENT
- "connector" → API_CONTRACT
- "OIDC" — no predicate (not "OAuth")
- "authentication" → SECURITY (Rule 8)
- "token" → SECURITY (Rule 6: "token introspection" contains "token"; fires once)
- "endpoint" → API_CONTRACT (reinforces)

**Result:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — NOT ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

**AUTHORIZATION is absent** because "OIDC" and "authentication" do not fire AUTHORIZATION. Only "OAuth" fires AUTHORIZATION.

---

## B17-0C-019-004 — Edge: Okta SAML connector → only IMPLEMENT + API_CONTRACT (no SECURITY without explicit trigger)

**Input:** "Develop the Okta SAML connector for the enterprise SSO portal."

**Edge assertions:**
- "Develop" → IMPLEMENT
- "connector" → API_CONTRACT
- "SAML" — no predicate
- "SSO" (Single Sign-On) — no predicate
- No "token", no "authentication", no "OAuth" → no SECURITY, no AUTHORIZATION

**Result:** ["IMPLEMENT", "API_CONTRACT"]

**Teaching point:** SECURITY does not fire automatically on identity connectors. It fires only when a trigger word (token, authentication, authenticate, secure, validate signature) is explicitly present in the input.
