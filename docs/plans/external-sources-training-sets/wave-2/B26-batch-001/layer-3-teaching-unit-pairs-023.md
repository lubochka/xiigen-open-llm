# Layer 3 — Teaching Unit Pairs (023)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU023-001 — AES, RSA, bcrypt, HMAC fire SECURITY; generic "algorithm" is gap
A: "Audit the encryption service — AES-256 key size, bcrypt cost factor, HMAC-SHA256 integrity." What fires?
B: SECURITY fires on ALL of: "AES-256" (cipher/encrypt), "bcrypt" (hash), "HMAC-SHA256" (hash/integrity). "audit" → SECURITY.
   Dense signal. CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3.

## TU023-002 — "CBC mode" fires SECURITY as a vulnerable cipher mode
A: "Audit the EncryptionService for CBC mode padding oracle vulnerabilities." Does SECURITY fire?
B: Yes. "CBC mode" → SECURITY (cipher trigger). "padding oracle vulnerabilities" → SECURITY.
   CompiledPredicateIds: ["SECURITY"]. Teaching: Cipher mode names (CBC, GCM, ECB) fire SECURITY in vulnerability context.

## TU023-003 — "encryption at rest" fires SECURITY; "database field" setup is gap
A: "Audit the database for fields storing PII without encryption at rest." vs "Add a new field to the User table."
B: "audit" + "encryption at rest" + "PII without encryption" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "add a new field" → gap (schema change). CompiledPredicateIds: [].

## TU023-004 — IMPLEMENT changes TransitionType from documentation to feature_addition
A: "Audit the encryption and document findings." vs "Audit the encryption and implement AES-GCM."
B: SECURITY+DOCUMENT → TransitionType: documentation.
   SECURITY+IMPLEMENT → TransitionType: feature_addition. IMPLEMENT is the decisive predicate.
   Rule: Whenever IMPLEMENT is present, TransitionType is feature_addition regardless of DOCUMENT co-occurrence.

## TU023-005 — TLS version matters: "TLS 1.3" fires SECURITY; TLS 1.0/1.1 rejection is SECURITY remediation
A: "Audit TLS configuration — confirm TLS 1.3, reject TLS 1.0/1.1." Does SECURITY fire?
B: Yes. "TLS" → SECURITY. "TLS 1.3" / "TLS 1.0" / "TLS 1.1" → SECURITY (TLS version enforcement is security action).
   CompiledPredicateIds: ["SECURITY"]. Multiple TLS references produce dense signal.
