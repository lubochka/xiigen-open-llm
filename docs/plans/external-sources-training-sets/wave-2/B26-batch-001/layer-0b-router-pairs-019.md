# Layer 0-B — Router Pairs (019)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R019-001 — SECURITY+QA → [6.3, 7.3]: secret rotation audit + rotation pipeline QA
Request: "Audit the SecretStore for API keys past their rotation deadline and QA the automated rotation pipeline."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R019-002 — SECURITY → 6.3: signing key lifecycle review
Request: "Review the signing key lifecycle — confirm rotation cadence, algorithm strength, and revocation procedures."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

## R019-003 — SECURITY+DOCUMENT → [6.3, 6.2]: hardcoded secrets audit documented
Request: "Audit all repositories for hardcoded secrets and document the remediation plan for each exposure."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R019-004 — Zero predicates → GapHandler: "secrets manager configuration" (setup task)
Request: "Configure the secrets manager to store environment variables for the deployment pipeline."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: "configure" is a setup task, not a security audit action. No audit/scan/review vocabulary.

## R019-005 — SECURITY+QA → [6.3, 7.3]: encryption key management + HSM audit + QA
Request: "Audit the encryption key management service — verify key storage, cipher algorithm, and HSM integration — and QA key lifecycle controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
