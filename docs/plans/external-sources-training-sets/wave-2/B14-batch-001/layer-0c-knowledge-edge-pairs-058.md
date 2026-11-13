# Layer 0-C — Knowledge Edge Pairs (058)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-058-001 — TLSCertRotation: gap vocabulary — certificate runtime operations

Confirmed gap vocabulary for TLS certificate runtime operations (zero predicates):
  Action verbs: renew, check (expiry), verify (certificate — operational), download, export (certificate), import, view, inspect (certificate), trust (add to trust store — operational)
  Runtime nouns: certificate expiry date, certificate fingerprint, trust store, certificate chain (runtime), certificate bundle

---

## B14-edge-058-002 — TLSCertRotation: SECURITY boundary — certificate rotation compound phrases

SECURITY fires for ALL of these (per rule 036-004 extended):
  "TLS certificate rotation", "certificate lifecycle", "rotate root CA (Certificate Authority) certificate", "mTLS with auto-renewed certificates", "cert rotation policy"
  These are SECURITY compound phrases — fire regardless of surrounding verb context (except negation).
  "renew the TLS certificate" → gap (renew = operational gap verb; does not trigger the "certificate rotation" compound phrase)
  Wait: "renew" is operational gap; the SECURITY compound phrase must contain "rotation" not "renewal" to fire.
  Rule: "certificate rotation" → SECURITY always; "certificate renewal" with "renew" verb → gap.

---

## B14-edge-058-003 — TLSCertRotation: IMPLEMENT boundary — cert rotation IaC artifacts

IMPLEMENT fires for:
  cert-manager CertificateRequest manifest, Vault PKI Terraform module, automatic cert rotation CronJob configuration, ClusterIssuer manifest, Certificate manifest (cert-manager)
  "implement the CertificateRequest manifest" → IMPLEMENT
  "renew the TLS certificate" → gap

---

## B14-edge-058-004 — TLSCertRotation: AUTHORIZATION boundary — cert issuance roles

AUTHORIZATION fires for:
  "grant certificate issuance role to cert-manager service account" → AUTHORIZATION
  "restrict Vault PKI issuer access via ClusterRole" → AUTHORIZATION ("ClusterRole", "restrict")
  "authorize the certificate signing service account" → AUTHORIZATION ("authorize" + "service account" in binding context)
