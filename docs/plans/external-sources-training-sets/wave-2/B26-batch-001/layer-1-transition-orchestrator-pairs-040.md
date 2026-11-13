# Layer 1 — Transition Orchestrator Pairs (040) — XXEAudit TransitionType
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T040-001 — XXE audit → feature_addition

T040-001 test: "Audit all XML parsers for XXE vulnerability exposure."
  SECURITY → 6.3 | TransitionType: feature_addition

---

## T040-002 — XXE + SSRF chain audit → feature_addition

T040-002 test: "Audit for XXE-to-SSRF chaining vectors in the document processing pipeline."
  SECURITY → 6.3 | TransitionType: feature_addition (two SECURITY triggers; no DOCUMENT/IMPLEMENT)

---

## T040-003 — "parse XML" → N/A

T040-003 test: "Parse the XML response from the shipping API."
  CompiledPredicateIds: [] | TransitionType: N/A

---

## T040-004 — XXE harden + DOCUMENT → documentation

T040-004 test: "Harden XML parsers by disabling DTD processing and document the parser configuration findings."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation

---

## T040-005 — XXE + IMPLEMENT → feature_addition

T040-005 test: "Implement DTD processing disable across all parsers and audit existing parser configurations for XXE."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition
