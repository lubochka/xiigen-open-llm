# Layer 0-C — Knowledge Edge Pairs (053)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-053-001 — AdmissionControllers: gap vocabulary — admission runtime operations

Confirmed gap vocabulary for admission controller runtime operations (zero predicates):
  Action verbs: enable (admission controller), disable, list, describe, debug, check (admission), bypass, skip
  Runtime nouns: admission review request, admission response, MutatingWebhookConfiguration (runtime object without design/impl context)

---

## B14-edge-053-002 — AdmissionControllers: SECURITY boundary — webhook TLS compound phrases

SECURITY fires for:
  "TLS webhook authentication" → SECURITY ("TLS")
  "validate admission certificates" → SECURITY ("certificate" in security context)
  "rotate webhook TLS credentials" → SECURITY ("rotate" + "TLS credentials" = key rotation)
  "enforce pod security policy via admission controller" → SECURITY ("enforce" + security context)

---

## B14-edge-053-003 — AdmissionControllers: IMPLEMENT boundary — admission IaC artifacts

IMPLEMENT fires for:
  ValidatingAdmissionWebhook configuration manifest, MutatingAdmissionWebhook manifest, admission webhook Terraform module, OPA ConstraintTemplate (in admission context), Gatekeeper constraint template
  "implement the ValidatingAdmissionWebhook manifest" → IMPLEMENT
  "enable the admission controller" → gap

---

## B14-edge-053-004 — AdmissionControllers: disambiguation — "validate" in admission vs TEST context

"validate admission certificates" → SECURITY (certificate validation = security, not TEST)
"validate the infrastructure connectivity" → TEST (validate correctness/connectivity = TEST action verb)
"validate the webhook configuration" → could be TEST (validation of config correctness) or gap (operational check)
Rule in B14: "validate" with infrastructure connectivity/correctness checking → TEST. "validate" with certificate/credential checking → SECURITY. Disambiguation by the following noun.
