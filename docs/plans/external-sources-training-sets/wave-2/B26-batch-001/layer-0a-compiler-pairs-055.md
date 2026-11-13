# Layer 0-A — Compiler Pairs (055) — MalwareReview
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C055-001 — Malware scan of dependency artifacts → SECURITY

A: "Scan all NuGet package artifacts for embedded malware signatures before deploying to production."
  "scan" → SECURITY | "malware signatures" → SECURITY (attack-vector context)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C055-002 — Malware review + supply chain → SECURITY

A: "Audit the build pipeline for supply chain injection of malicious code in third-party dependencies."
  "audit" → SECURITY | "malicious code" → SECURITY | "injection" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C055-003 — "update antivirus software" → gap

A: "Update the antivirus software definitions on the build server."
  "update" → gap | "antivirus software" → gap (maintenance task, no audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C055-004 — Malware + backdoor audit → SECURITY

A: "Audit the deployed application image for backdoor binaries and unauthorized outbound connection patterns."
  "audit" → SECURITY | "backdoor binaries" → SECURITY (attack vector) | "unauthorized" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C055-005 — Malware review + DOCUMENT → SECURITY+DOCUMENT

A: "Review detected malware artifacts and document the infection vector and affected credential stores."
  "review" → SECURITY | "malware artifacts" → SECURITY | "credential stores" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
