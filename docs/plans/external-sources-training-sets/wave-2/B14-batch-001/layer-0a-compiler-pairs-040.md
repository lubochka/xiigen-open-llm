# Layer 0-A — Compiler Pairs (040)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-040-001 — StorageClasses: ARCHITECTURE fires on storage design vocabulary

"architect the storage class hierarchy for persistent workloads" → ARCHITECTURE ("architect", "hierarchy")
"design the storage tier model" → ARCHITECTURE ("design", "model")
"structure block vs object storage boundaries" → ARCHITECTURE ("structure", "boundaries")
"use the fast storage class" → gap ("use" is operational gap)

---

## B14-compiler-040-002 — StorageClasses: IMPLEMENT fires on storage IaC artifacts

"implement the StorageClass Kubernetes manifest" → IMPLEMENT ("implement" + "manifest")
"write the storage class Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the CSI (Container Storage Interface) driver configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-040-003 — StorageClasses: SECURITY fires on storage encryption vocabulary

"encrypt storage volumes at rest with KMS (Key Management Service)" → SECURITY ("encrypt", "at rest", "key")
"enforce encryption-at-rest policy on all persistent volumes" → SECURITY ("enforce", "encryption-at-rest")
"configure storage class with AES-256 encryption" → SECURITY (encryption in code-artifact context)
Note: "configure" fires IMPLEMENT when followed by code-artifact noun; SECURITY fires from encryption vocabulary

---

## B14-compiler-040-004 — StorageClasses: AUTHORIZATION fires on storage access control

"grant PersistentVolumeClaim (PVC) binding role to application service account" → AUTHORIZATION ("grant", "role", "service account binding")
"define RBAC (Role-Based Access Control) role for storage admin operations" → AUTHORIZATION ("RBAC role")
"restrict namespace access to premium storage class" → AUTHORIZATION ("restrict" in access policy context)

---

## B14-compiler-040-005 — StorageClasses: DOCUMENT compilation

"document the storage class ADR and tier selection criteria" → DOCUMENT ("document" + "ADR")
CompiledPredicateIds: ["DOCUMENT"]
CapabilityGapWarnings: gap-devops-cloud-001 ("StorageClass", "CSI", "EBS (Elastic Block Store)", "gp3" are gap platform vocabulary)
