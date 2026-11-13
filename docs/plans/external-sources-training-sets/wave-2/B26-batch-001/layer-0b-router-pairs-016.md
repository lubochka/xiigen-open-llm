# Layer 0-B — Router Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R016-001 — SECURITY+QA → [6.3, 7.3]: threat model STRIDE review + mitigations QA
Request: "Review the threat model for the authentication subsystem — validate STRIDE coverage — and QA all mitigations."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R016-002 — SECURITY → 6.3: attack surface audit
Request: "Audit the external-facing attack surface — enumerate all ingress points and authentication mechanisms."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R016-003 — SECURITY+AUTHORIZATION+DOCUMENT → [6.3, 6.3, 6.2]: threat model privilege escalation
Request: "Review the existing threat model for privilege escalation paths and document all newly discovered attack vectors."
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION", "DOCUMENT"] | SubPhases: [6.3, 6.3, 6.2]
  Teaching: SECURITY and AUTHORIZATION both route to 6.3 in audit context; not collapsed — two independent routing decisions both landing at 6.3.

## R016-004 — Zero predicates → GapHandler: architecture diagram update
Request: "Update the system architecture diagram to reflect the new microservice boundaries."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

## R016-005 — SECURITY+QA → [6.3, 7.3]: IDOR + mass assignment threat findings + QA remediation plan
Request: "Audit the threat modeling session findings for IDOR and mass assignment vulnerabilities, and QA the remediation plan."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
