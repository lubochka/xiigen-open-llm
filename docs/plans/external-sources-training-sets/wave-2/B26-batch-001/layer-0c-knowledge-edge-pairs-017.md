# Layer 0-C — Knowledge Edge Pairs (017)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E017-001 — "SSRF" fires SECURITY; "server request" alone is gap

A-fires: "Audit the WebhookService for SSRF (Server-Side Request Forgery) exposure — can the callback URL be set to an internal IP?"
  "audit" → SECURITY; "SSRF" → SECURITY (attack vector); "internal IP" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-gap: "The server request times out when calling the external webhook."
  "server request" → gap (performance/connectivity context)
  CompiledPredicateIds: []

---

## E017-002 — "XXE" fires SECURITY; "XML parsing" alone is gap

A-fires: "Scan the DataImportService for XXE (XML External Entity) injection in the XML parser."
  "scan" → SECURITY; "XXE" → SECURITY; "injection" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Update the XML parser to handle schema validation errors gracefully."
  "update XML parser" → IMPLEMENT (borderline); "schema validation" → gap; "gracefully" → gap
  CompiledPredicateIds: [] or ["IMPLEMENT"] — no SECURITY without vulnerability vocabulary.

---

## E017-003 — "path traversal" fires SECURITY; "file path" alone is gap

A-fires: "Audit the FileUploadService for path traversal vulnerabilities — can URL-encoded ../ sequences bypass the upload directory restriction?"
  "audit" → SECURITY; "path traversal" → SECURITY; "vulnerabilities" → SECURITY; "bypass" → SECURITY context
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Update the file path resolution logic to handle Windows and Unix paths consistently."
  "file path resolution" → gap (cross-platform compatibility, no security vocabulary)
  CompiledPredicateIds: []

---

## E017-004 — "deserialization" fires SECURITY; "JSON deserialization" for business logic is gap

A-fires: "Audit the SettingsService for insecure deserialization vulnerabilities — can a crafted JSON payload trigger arbitrary code execution?"
  "audit" → SECURITY; "insecure deserialization" → SECURITY; "vulnerabilities" → SECURITY; "arbitrary code execution" → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Deserialize the incoming JSON payload into a strongly-typed SettingsModel."
  "Deserialize" → gap (routine data mapping, no security vocabulary)
  CompiledPredicateIds: []

---

## E017-005 — "CVSS score" fires SECURITY; "score" alone is gap

A: "Review all open CVEs and prioritize by CVSS (Common Vulnerability Scoring System) score — address CRITICAL (9.0–10.0) first."
  "review" (security context) → SECURITY; "CVEs" → SECURITY; "CVSS score" → SECURITY; "CRITICAL" → severity context
  CompiledPredicateIds: ["SECURITY"]
  Teaching: CVSS is a vulnerability scoring vocabulary — fires SECURITY. "score" alone (e.g., credit score) is gap.
