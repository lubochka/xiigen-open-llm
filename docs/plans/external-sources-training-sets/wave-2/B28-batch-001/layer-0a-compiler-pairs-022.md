# Layer 0-A — Compiler Pairs (022)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-022-001 — QA+DOCUMENT: compliance release criteria

A: "Document the compliance release criteria and QA sign-off before the SOC 2 (System and Organization Controls 2) audit."
  "document" → DOCUMENT | "compliance release criteria" → DOCUMENT | "QA sign-off" → QA | "SOC 2 audit" → gap
  CompiledPredicateIds: ["QA", "DOCUMENT"]

---

## B28-compiler-022-002 — QA+IMPLEMENT+SECURITY: compliance gate service

A: "Build the compliance gate service, secure the audit trail endpoints, and QA the compliance checklist."
  "build" → IMPLEMENT | "secure" → SECURITY | "QA" → QA | "audit trail" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "SECURITY"]

---

## B28-compiler-022-003 — gap: compliance audit execution

A: "Schedule the SOC 2 audit and upload the compliance evidence to the auditor portal."
  "schedule" → gap | "upload evidence" → gap | "compliance" alone → gap | "auditor portal" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-022-004 — QA+DOCUMENT+SECURITY: compliance documentation with security

A: "Document the security controls, write the compliance audit report, and QA review the release checklist."
  "document" → DOCUMENT | "security controls" → SECURITY | "audit report" → DOCUMENT | "QA review" → QA | "release checklist" → QA
  CompiledPredicateIds: ["QA", "DOCUMENT", "SECURITY"]

---

## B28-compiler-022-005 — QA+IMPLEMENT+DOCUMENT+SECURITY: full compliance lifecycle

A: "Build the audit trail service, secure the PII (Personally Identifiable Information) endpoints, document the compliance report, and QA approve."
  "build" → IMPLEMENT | "secure" → SECURITY | "PII" → gap | "document" → DOCUMENT | "QA approve" → QA
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
