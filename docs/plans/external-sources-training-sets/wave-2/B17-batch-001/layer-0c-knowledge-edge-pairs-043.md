# Layer 0-C — Knowledge-Edge Pairs (043)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Redis connector

---

## B17-0C-043-001 — Redis: non-trigger terms

**Proper nouns — no predicate:** Redis, Redis Labs, Upstash
**Domain nouns — no predicate:** key, value, hash, list, set, sorted set, stream, pub/sub, TTL (Time To Live), eviction, cluster, sentinel, cache, session
**Acronyms — no predicate:** TTL (Time To Live), AOF (Append Only File)

---

## B17-0C-043-002 — Redis: "TLS" and "SSL" alone do not fire SECURITY

TLS (Transport Layer Security) and SSL (Secure Sockets Layer) are transport-level encryption acronyms. They do not fire SECURITY in B17. SECURITY fires only on token, authenticate/authentication, or HMAC-SHA256 validation phrases.

---

## B17-0C-043-003 — Redis: ACL as acronym vs token

"ACL" (Access Control List) alone → no predicate.
"ACL authentication token" → SECURITY fires (compound contains "token").
"ACL password" → no SECURITY (no "token", no authenticate phrase).
Teaching: Even security-related acronyms do not fire predicates without the explicit trigger word.

---

## B17-0C-043-004 — Redis: "cache" and "store" are not IMPLEMENT triggers

"Cache query results in Redis" uses "cache" as a verb — not an IMPLEMENT trigger.
"Store session data in Redis" uses "store" as a verb — not an IMPLEMENT trigger.
Only the explicit IMPLEMENT trigger list (build, implement, write, create, code, develop, refactor, restructure, rewrite) fires IMPLEMENT.
