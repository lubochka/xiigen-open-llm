# Layer 3 — Teaching Unit Pairs (022)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU022-001 — QA+DOCUMENT+SECURITY: compliance release

A: "Document the security controls, write the compliance report, and QA review the release checklist."
B: ["SECURITY", "DOCUMENT", "QA"] — "security controls" → SECURITY; "document" → DOCUMENT; "compliance report" → DOCUMENT (dedup); "QA review" → QA; "release checklist" → QA (dedup).
Reason: Three predicates. TransitionType: documentation (IMPLEMENT absent).
CompiledPredicateIds: ["SECURITY", "DOCUMENT", "QA"]

---

## TU022-002 — Gap: compliance audit scheduling

A: "Schedule the SOC 2 compliance audit for Q3."
B: [] — "schedule audit" → gap; "SOC 2" → gap; "Q3" → gap.
Reason: Compliance audit scheduling → gap. knowledge_edge.
CompiledPredicateIds: []

---

## TU022-003 — QA+IMPLEMENT+SECURITY: compliance gate service

A: "Build the compliance gate service, secure the audit trail endpoints, and QA approve."
B: ["IMPLEMENT", "SECURITY", "QA"] — "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "secure" → SECURITY; "QA approve" → QA.
Reason: Three predicates. TransitionType: feature_addition.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "QA"]

---

## TU022-004 — "secure" fires SECURITY in compliance context

A: "Secure the PII data endpoints for the AdminService."
B: ["SECURITY"] — "secure" → SECURITY; "PII data" → gap (PII alone is gap).
Reason: "secure" fires SECURITY regardless of what is secured. "PII" alone does not fire SECURITY.
CompiledPredicateIds: ["SECURITY"]

---

## TU022-005 — QA+IMPLEMENT+DOCUMENT+SECURITY: full compliance lifecycle

A: "Build the audit trail service, secure the PII endpoints, document the compliance criteria, and QA gate."
B: ["IMPLEMENT", "SECURITY", "DOCUMENT", "QA"] — four predicates. TransitionType: feature_addition.
Reason: Each clause fires independently. "criteria" fires DOCUMENT (dedup). QA last.
CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "DOCUMENT", "QA"]
