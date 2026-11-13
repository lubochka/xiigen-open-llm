# Layer 0-A — Compiler Pairs (030) — SecurityScanGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-030-001 — "SecurityScanGate" vocabulary

A: "Run the SecurityScanGate before merging to main."
  "SecurityScanGate" → SECURITY (security scan = security vocabulary) | "merging" → gap
  Teaching: SecurityScanGate fires SECURITY. "merging" is CI/CD (Continuous Integration/Continuous Delivery) pipeline execution vocabulary; gap.
  CompiledPredicateIds: ["SECURITY"]

---

## B28-compiler-030-002 — "security scan" + "QA review"

A: "Run a security scan on the container image and QA review the findings."
  "security scan" → SECURITY (security verb/noun) | "QA review" → QA (abbreviation compound)
  Teaching: "security scan" fires SECURITY; "QA review" fires QA.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-030-003 — "secure endpoints" + "implement patch"

A: "Secure the authentication endpoints and implement the CVE (Common Vulnerabilities and Exposures) patch service."
  "secure" → SECURITY | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup)
  Teaching: "secure" verb fires SECURITY; "implement" fires IMPLEMENT; "service" fires IMPLEMENT (deduplicated).
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT"]

---

## B28-compiler-030-004 — "document security criteria"

A: "Document the security gate criteria and QA sign-off on compliance."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "QA sign-off" → QA
  Teaching: DOCUMENT fires twice (deduplicated). "QA sign-off" fires QA on abbreviation.
  CompiledPredicateIds: ["DOCUMENT", "QA"]

---

## B28-compiler-030-005 — Pure gap: "trigger security scan"

A: "Trigger the SecurityScanGate automatically on each commit."
  "trigger" → gap | "automatically" → gap | "commit" → gap
  Teaching: "trigger", "automatically", "commit" are CI/CD execution vocabulary; pure gap.
  CompiledPredicateIds: [] → gap (knowledge_edge)
