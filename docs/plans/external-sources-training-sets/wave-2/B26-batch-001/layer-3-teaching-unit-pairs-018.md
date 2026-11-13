# Layer 3 — Teaching Unit Pairs (018)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU018-001 — "penetration" fires SECURITY; "pen test" and "pentest" also fire SECURITY
A: "Review the penetration test findings for the AccountService." Does SECURITY fire?
B: Yes. "penetration" is an explicit SECURITY trigger. "penetration test" → SECURITY. "review" (security context) → SECURITY.
   All three forms fire: "penetration test", "pen test" (two words), "pentest" (compound).
   CompiledPredicateIds: ["SECURITY"]. SubPhase: 6.3 (review context, not 5.2 test-writing context).

## TU018-002 — "schedule the pen test" is gap; "review the penetration test findings" fires SECURITY
A: "Schedule the pen test for Q3." vs "Review the penetration test findings from Q3."
B: "schedule the pen test" → gap (logistics noun phrase). CompiledPredicateIds: [].
   "review the penetration test findings" → SECURITY (review verb + penetration + findings). CompiledPredicateIds: ["SECURITY"].
   Rule: Scheduling events is gap. Reviewing or auditing security outputs fires SECURITY.

## TU018-003 — MFA fires SECURITY; "authentication" fires SECURITY
A: "Audit the MFA bypass paths in the AdminConsole." Does SECURITY fire?
B: Yes. "audit" → SECURITY. "MFA" (Multi-Factor Authentication) → SECURITY (mfa trigger). "bypass paths" → SECURITY context.
   CompiledPredicateIds: ["SECURITY"]. MFA is an explicit predicate trigger per rules.

## TU018-004 — "brute-force" fires SECURITY; "high traffic" does not
A: "Audit the /login endpoint for brute-force resistance." vs "The /login endpoint receives high traffic during peak hours."
B: "brute-force" → SECURITY. "audit" → SECURITY. CompiledPredicateIds: ["SECURITY"].
   "high traffic during peak hours" → gap (performance context). CompiledPredicateIds: [].

## TU018-005 — "credential harvesting" fires SECURITY via credential trigger
A: "Review the red team report for credential harvesting findings." Does SECURITY fire?
B: Yes. "credential harvesting" → SECURITY ("credential" is an explicit trigger). "review" (security context) → SECURITY.
   "red team" → SECURITY (adversarial security context). Dense signal. CompiledPredicateIds: ["SECURITY"].
