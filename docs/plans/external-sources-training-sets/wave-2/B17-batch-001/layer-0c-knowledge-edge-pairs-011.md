# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: knowledge-edge

---

## B17-0C-011-001 — Full B17 predicate vocabulary table

**IMPLEMENT triggers (build verbs):**
build, implement, write, create, code, develop, refactor, restructure, rewrite

**API_CONTRACT triggers:**
integrate, connector, webhook, wire up, connect (+external), API, APIs, gateway, SDK

**SECURITY triggers:**
token, authentication, secure, validate signature, verify signature, HMAC validation, webhook secret, credential security

**AUTHORIZATION triggers:**
oauth, OAuth, OAUTH, oauth2, OAuth2 (any case, any version suffix)

**TEST triggers:**
write tests, create tests, add tests, write unit tests, write integration tests, write end-to-end tests

**DOCUMENT triggers:**
document, spec out, spec, write documentation, write docs

**QA triggers:**
QA (the abbreviation only — not "quality assurance" spelled out)

---

## B17-0C-011-002 — Non-triggering vocabulary (common false positives)

| Word/phrase | Does NOT fire | Correct predicate (if any) |
|---|---|---|
| "quality assurance" (full phrase) | QA | None |
| "authorization" (English word, no oauth) | AUTHORIZATION | None |
| "security review" (no token/auth/validate) | SECURITY | None (generic phrase) |
| "internal connection" | API_CONTRACT | None (internal, not external) |
| "import" | IMPLEMENT | None |
| "integration layer" (no build verb) | IMPLEMENT | None |
| "payment" alone | Any predicate | None |
| "service layer" alone | IMPLEMENT | None |

---

## B17-0C-011-003 — Ambiguous vocabulary resolution

**"authentication" vs "oauth":**
- "authentication" → SECURITY
- "oauth" → AUTHORIZATION
- "OAuth authentication" → both SECURITY (authentication) and AUTHORIZATION (oauth)

**"validation" context sensitivity:**
- "signature validation" → SECURITY (webhook security pattern)
- "input validation" → no predicate (business logic, not security pattern)
- "schema validation" → no predicate

**"secure" context:**
- "secure the endpoint" → SECURITY
- "SecurePaymentService" (class name) → SECURITY (contains "secure")
- "insecure" → SECURITY (contains "secure" as substring? — context-dependent; prefer explicit positive match)

---

## B17-0C-011-004 — GapHandler: not present in B17

**Rule 19:** No GapHandler in B17. Gap vocabulary does not appear in B17 scenarios.

**GapHandler appears in other scenarios** where the classifier encounters vocabulary it cannot classify. In B17, all relevant vocabulary maps cleanly to predicates above.

**If vocabulary appears that doesn't map to any B17 predicate:** treat as no trigger (do not invent predicates — v6-01 prevention). The correct response to unrecognized vocabulary is silence, not predicate invention.
