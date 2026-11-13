# Layer 0-A — Compiler Pairs (032) — LicenseComplianceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-032-001 — "LicenseComplianceGate" vocabulary

A: "Run the LicenseComplianceGate before publishing the release."
  "LicenseComplianceGate" → SECURITY (compliance gate = security/compliance check) | "publishing" → gap
  Teaching: License compliance fires SECURITY (regulatory compliance check). "publishing" is gap.
  CompiledPredicateIds: ["SECURITY"]

---

## B28-compiler-032-002 — "compliance check" + "QA review"

A: "Run the license compliance check on all dependencies and QA review the report."
  "license compliance check" → SECURITY | "QA review" → QA (abbreviation compound)
  Teaching: License compliance check fires SECURITY; "QA review" fires QA.
  CompiledPredicateIds: ["SECURITY", "QA"]

---

## B28-compiler-032-003 — "document compliance criteria" + "implement checker service"

A: "Document the license compliance criteria and implement the automated checker service."
  "document" → DOCUMENT | "criteria" → DOCUMENT (dedup) | "implement" → IMPLEMENT | "service" → IMPLEMENT (dedup)
  Teaching: DOCUMENT fires twice (deduplicated); IMPLEMENT fires twice (deduplicated).
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

---

## B28-compiler-032-004 — "secure" + "validate license"

A: "Secure the artifact repository and validate license compatibility before release."
  "secure" → SECURITY | "validate" → TEST
  Teaching: "secure" fires SECURITY; "validate" fires TEST in any context.
  CompiledPredicateIds: ["SECURITY", "TEST"]

---

## B28-compiler-032-005 — Pure gap: "check licenses"

A: "Check all open-source licenses in the deployment bundle."
  "check" → gap (alone, without security/vulnerability context)
  Teaching: "check" alone without a security-domain noun is gap in the release pipeline context.
  CompiledPredicateIds: [] → gap (knowledge_edge)
