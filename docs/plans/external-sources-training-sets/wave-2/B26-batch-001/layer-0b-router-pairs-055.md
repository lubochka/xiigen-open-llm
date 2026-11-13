# Layer 0-B — Router Pairs (055) — MalwareReview Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R055-001 — Malware scan → 6.3

R055-001 test: "Scan all NuGet package artifacts for embedded malware signatures."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R055-002 — "update antivirus software" → GapHandler

R055-002 test: "Update the antivirus software definitions on the build server."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R055-003 — Build pipeline malware audit → 6.3

R055-003 test: "Audit the build pipeline for supply chain injection of malicious code."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R055-004 — Backdoor binary audit → 6.3

R055-004 test: "Audit the deployed application image for backdoor binaries and unauthorized outbound connections."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R055-005 — Malware + DOCUMENT → 6.3 + 6.2

R055-005 test: "Review malware artifacts and document the infection vector and affected credential stores."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
