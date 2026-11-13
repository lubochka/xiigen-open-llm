# Layer 1 — Transition Orchestrator Pairs (055)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-055-001 — Edge case: "write" verb + IaC artifact vs documentary artifact

"Write the Terraform module" → IMPLEMENT ("write" + IaC artifact: "module")
"Write the ADR" → DOCUMENT ("write" + documentary artifact: "ADR")
"Write the runbook" → DOCUMENT ("write" + documentary artifact: "runbook")
Both fire feature_addition individually? No: "write the ADR" → DOCUMENT alone → documentation.
TransitionType: documentation for DOCUMENT alone; feature_addition for IMPLEMENT alone.

---

## B14-trans-055-002 — Edge case: "write the runbook and implement the configuration"

"Write the runbook and implement the OPA configuration."
  "write the runbook" → DOCUMENT | "implement" + "configuration" → IMPLEMENT
  IMPLEMENT present → feature_addition (overrides documentation from DOCUMENT)
  Sequential: 4.1 (IMPLEMENT) → 6.2 (DOCUMENT) | Note: sequential shows IMPLEMENT before DOCUMENT even when "write" came first in the sentence.

---

## B14-trans-055-003 — Edge case: multiple IMPLEMENT verbs in one request → single IMPLEMENT predicate

"Implement the manifests, build the Terraform modules, scaffold the configuration."
  Three IMPLEMENT verbs → single IMPLEMENT predicate (deduplication)
  FlowCount: 1 (BackendServiceFlow41 × 1) | feature_addition
  Rule: Multiple firings of the same predicate do not create multiple flows. Each predicate creates exactly one flow.

---

## B14-trans-055-004 — Edge case: multiple SECURITY compound phrases in one request → single SECURITY predicate

"Enforce zero-trust, rotate TLS certificates, encrypt data at rest."
  Three SECURITY triggers → single SECURITY predicate (deduplication)
  FlowCount: 1 (SecurityAuditFlow63 × 1) | feature_addition
  Rule: Multiple SECURITY compound phrases in one request = one SECURITY predicate = one SecurityAuditFlow63 flow.
