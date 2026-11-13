# Layer 0-A — Compiler Pairs (040) — XXEAudit
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C040-001 — XXE (XML External Entity) injection audit → SECURITY

A: "Audit all XML parsers for XXE vulnerability exposure."
  "audit" → SECURITY | "XXE" → SECURITY (attack vector solo noun)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C040-002 — External entity resolution review → SECURITY

A: "Review whether external entity resolution is disabled in all XML (Extensible Markup Language) processing modules."
  "review" → SECURITY | "XXE" context: "external entity resolution" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C040-003 — XXE + SSRF (Server-Side Request Forgery) chaining → SECURITY

A: "Audit for XXE-to-SSRF chaining vectors in the document processing pipeline."
  "audit" → SECURITY | "XXE" → SECURITY | "SSRF" → SECURITY (two independent attack-vector triggers)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C040-004 — "disable external entity processing" → SECURITY (harden vocabulary)

A: "Harden all XML parsers by disabling DTD (Document Type Definition) processing and external entity resolution."
  "harden" → SECURITY | "disabling" = hardening action + security context → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C040-005 — "parse XML from API response" → gap (no security vocabulary)

A: "Parse the XML response from the third-party shipping API."
  "parse" → gap | "XML" → gap (no security-action vocabulary co-occurring)
  CompiledPredicateIds: [] → knowledge_edge
