# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E013-001 — "authorization header" fires API_CONTRACT not AUTHORIZATION

A: "Review the API spec to ensure all endpoints declare the Authorization header requirement."
  "Authorization header" → API_CONTRACT (HTTP header specification); "review" → gap without security co-occurrence; "Authorization header" ≠ AUTHORIZATION predicate
  CompiledPredicateIds: ["API_CONTRACT"]
  Teaching: "Authorization" as an HTTP header name fires API_CONTRACT, not the AUTHORIZATION predicate. Context determines which predicate fires.

---

## E013-002 — "OAuth" fires SECURITY (oauth trigger); "OAuth documentation" alone is gap

A: "Update the OAuth 2.0 documentation to reflect the new redirect URI policy."
  "OAuth 2.0" → SECURITY trigger? "update... documentation" — the action is documentation update, not a security audit.
  Correction: "OAuth" is in the SECURITY trigger list, but "update documentation" shifts context to DOCUMENT, not security audit.
  CompiledPredicateIds: ["DOCUMENT"] — OAuth here is a noun being documented, not a security audit target.
  Teaching: Trigger vocabulary in a documentation-update context shifts the predicate set.

---

## E013-003 — "authentication" alone fires SECURITY regardless of action verb

A: "Look into the authentication flow before the next sprint."
  "authentication" → SECURITY fires independently; "look into" → gap action verb but does not suppress "authentication" trigger
  CompiledPredicateIds: ["SECURITY"] — weak signal (gap action verb + single security noun)
  TrustState: probationary_accepted (single security noun, gap action verb)
  Teaching: Security domain nouns fire SECURITY even with weak/gap action verbs.

---

## E013-004 — "session" fires SECURITY; "session management" without audit verb is weak

A: "Rethink the session management approach for mobile clients."
  "session management" → SECURITY trigger (session); "rethink" → gap action verb
  CompiledPredicateIds: ["SECURITY"] — weak signal
  TrustState: probationary_accepted
  Gap warning: "rethink" is gap vocabulary. SECURITY fires on "session" alone.

---

## E013-005 — "credential" fires SECURITY; "credential rotation" is a direct SECURITY trigger

A: "Rotate the database credentials before the deployment."
  "Rotate" → SECURITY context (secret/credential rotation); "database credentials" → SECURITY (credential trigger)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 (audit/review context) or 5.2 (implementation)
  Note: "Rotate credentials" is a security action that fires SECURITY. Not a gap.
