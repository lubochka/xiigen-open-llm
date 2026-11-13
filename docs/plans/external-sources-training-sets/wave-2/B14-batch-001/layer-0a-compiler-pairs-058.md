# Layer 0-A — Compiler Pairs (058)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-058-001 — TLSCertRotation: ARCHITECTURE fires on cert rotation design vocabulary

"architect the TLS (Transport Layer Security) certificate rotation topology" → ARCHITECTURE ("architect", "topology")
"design the certificate lifecycle management model" → ARCHITECTURE ("design", "model")
"structure the PKI (Public Key Infrastructure) hierarchy for certificate issuance" → ARCHITECTURE ("structure", "hierarchy")
"renew the TLS certificate" → gap ("renew" is operational gap)

---

## B14-compiler-058-002 — TLSCertRotation: IMPLEMENT fires on cert rotation IaC artifacts

"implement the cert-manager CertificateRequest manifest" → IMPLEMENT ("implement" + "manifest")
"write the Vault PKI (Public Key Infrastructure) Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the automatic cert rotation CronJob configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-058-003 — TLSCertRotation: SECURITY fires on TLS/certificate vocabulary

"enforce automatic TLS certificate rotation every 90 days" → SECURITY ("TLS", "certificate", "rotation" = key rotation equivalent)
"rotate the root CA (Certificate Authority) certificate annually" → SECURITY ("rotate" + "certificate")
"configure mTLS with auto-renewed certificates via cert-manager" → SECURITY ("mTLS")
Note: "certificate rotation" = "key rotation" equivalent → always fires SECURITY

---

## B14-compiler-058-004 — TLSCertRotation: AUTHORIZATION fires on cert issuance authority

"grant certificate issuance role to the cert-manager service account" → AUTHORIZATION ("grant", "role", "service account")
"restrict Vault PKI issuer access to authorized namespaces via ClusterRole" → AUTHORIZATION ("ClusterRole", "restrict")

---

## B14-compiler-058-005 — TLSCertRotation: DOCUMENT compilation

"document the TLS certificate rotation ADR and renewal schedule" → DOCUMENT ("document" + "ADR")
"write the certificate rotation runbook for on-call engineers" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]
CapabilityGapWarnings: gap-devops-cloud-001 ("cert-manager", "ACME (Automatic Certificate Management Environment)", "Let's Encrypt" are gap)
