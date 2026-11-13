# Layer 3 — Teaching Unit Pairs (014)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU014-001 — GDPR alone is gap; encryption vocabulary fires SECURITY
A: "Audit the user data for GDPR-related encryption gaps." Does SECURITY fire?
B: Yes — but NOT because of "GDPR". "GDPR" is gap vocabulary. SECURITY fires on "encryption gaps"
   (encrypt trigger) and "audit" (action verb). If the request were "Ensure GDPR compliance" only → zero predicates.
   Rule: The technical action ("encryption") fires SECURITY. The regulatory label ("GDPR") is always gap.

## TU014-002 — "ensure GDPR compliance" is a classic gap pattern
A: "Ensure the platform is GDPR compliant for all user data." Does SECURITY fire?
B: No. "ensure" → gap. "GDPR" → gap. "compliant" → gap. "user data" → gap.
   CompiledPredicateIds: [] → knowledge_edge. GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING).

## TU014-003 — "data breach" fires SECURITY; "data issue" is gap
A: "Assess the UserDataService for data breach vulnerability." Does SECURITY fire?
B: Yes. "assess" + "data breach vulnerability" → SECURITY. "breach" is a security-domain noun. "vulnerability" → SECURITY.
   Contrast: "There's a data issue in UserDataService" → [] (gap — "issue" is gap vocabulary).

## TU014-004 — "right to erasure" is gap even with IMPLEMENT
A: "Implement the right to erasure flow for GDPR." What predicates fire?
B: "implement" → IMPLEMENT. "right to erasure" → gap. "GDPR" → gap. CompiledPredicateIds: ["IMPLEMENT"].
   SECURITY does not fire on GDPR data-rights vocabulary (right to erasure, data portability, consent).

## TU014-005 — DOCUMENT fires on "document"; SECURITY+DOCUMENT produce separate sub-phases
A: "Audit the PII encryption and document the findings." What sub-phases are produced?
B: SECURITY → 6.3. DOCUMENT → 6.2. Two routing decisions. Two flow instances.
   TransitionType: documentation (DOCUMENT present, IMPLEMENT absent).
