# Layer 0-B — Router Pairs (040) — XXEAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R040-001 — XXE audit → 6.3

R040-001 test: "Audit all XML parsers for XXE vulnerability exposure."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R040-002 — XXE + SSRF chaining audit → 6.3

R040-002 test: "Audit for XXE-to-SSRF chaining vectors in the document processing pipeline."
  SECURITY → 6.3 | Two attack-vector triggers → TrustState: deterministic_generic

---

## R040-003 — "parse XML from API response" → GapHandler

R040-003 test: "Parse the XML response from the third-party shipping API."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R040-004 — XXE harden action → 6.3

R040-004 test: "Harden all XML parsers by disabling DTD processing and external entity resolution."
  SECURITY → 6.3 | "harden" + security context | CompiledPredicateIds: ["SECURITY"]

---

## R040-005 — XXE + test-writing → 5.2

R040-005 test: "Write security tests to verify DTD processing is disabled in all XML parsers."
  SECURITY → 5.2 (write-test context)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 5.2
