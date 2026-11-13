# Layer 0-A — Compiler Pairs (053)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-053-001 — AdmissionControllers: ARCHITECTURE fires on admission design vocabulary

"architect the admission controller chain for the production cluster" → ARCHITECTURE ("architect", "chain" in design context)
"design the mutating and validating webhook topology" → ARCHITECTURE ("design", "topology")
"structure the admission pipeline for policy enforcement" → ARCHITECTURE ("structure", "pipeline" in infra context)
"enable an admission controller" → gap ("enable" is operational gap)

---

## B14-compiler-053-002 — AdmissionControllers: IMPLEMENT fires on admission IaC artifacts

"implement the ValidatingAdmissionWebhook configuration manifest" → IMPLEMENT ("implement" + "manifest")
"write the admission webhook Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the OPA (Open Policy Agent) Gatekeeper constraint template" → IMPLEMENT ("scaffold" + "template" in IaC context)

---

## B14-compiler-053-003 — AdmissionControllers: SECURITY fires on admission security vocabulary

"enforce pod security policy via admission controller" → SECURITY ("enforce" in security context)
"harden API server admission with TLS webhook authentication" → SECURITY ("harden", "TLS")
"validate admission certificates and rotate webhook TLS credentials" → SECURITY ("certificate", "rotate", "TLS credentials")

---

## B14-compiler-053-004 — AdmissionControllers: AUTHORIZATION fires on admission binding vocabulary

"grant webhook admission control permissions to the policy-controller service account" → AUTHORIZATION ("grant", "service account", "permissions")
"restrict admission webhook invocation to authorized namespaces" → AUTHORIZATION ("restrict" in access context)

---

## B14-compiler-053-005 — AdmissionControllers: DOCUMENT compilation

"document the admission controller chain ADR and webhook configuration decision" → DOCUMENT ("document" + "ADR")
"write the admission controller runbook for policy enforcement changes" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]
