# Layer 0-C — Knowledge Edge Pairs (040)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-040-001 — StorageClasses: gap vocabulary — storage runtime operations

Confirmed gap vocabulary for storage runtime operations (zero predicates):
  Action verbs: mount, unmount, attach, detach, resize, expand, clone, bind (runtime), reclaim, delete (volume lifecycle), format, wipe, snapshot (as operational verb)
  Storage runtime nouns: IOPS limit (operational), throughput cap (operational), disk quota, filesystem mount point
  None of the above fire any B14 predicate.

---

## B14-edge-040-002 — StorageClasses: SECURITY boundary — storage encryption compound phrases

SECURITY fires for these storage-specific compound phrases:
  "encryption at rest for persistent volumes", "KMS (Key Management Service) key rotation for storage", "FIPS-140-2 compliant encryption for PVs (Persistent Volumes)", "AES-256 storage encryption", "encryption-at-rest policy"
  These fire regardless of surrounding verb when the compound noun phrase is present.

---

## B14-edge-040-003 — StorageClasses: AUTHORIZATION boundary — storage access control

AUTHORIZATION fires for these storage access control phrases:
  "PVC (Persistent Volume Claim) binding role", "storage-admin ClusterRole", "grant PVC binding permission to service account"
  "grant use permission on storage class" → AUTHORIZATION | "restrict namespace to premium storage class" → AUTHORIZATION

---

## B14-edge-040-004 — StorageClasses: disambiguation — "configure" is context-dependent

"configure storage class" (operational, no code artifact) → gap
"configure the StorageClass manifest" (IaC artifact: manifest) → IMPLEMENT
"configure encryption at rest" (security context) → SECURITY fires from encryption vocabulary; IMPLEMENT fires if "configuration" artifact follows
Dual-predicate: "configure the storage class encryption configuration" → SECURITY (encryption) + IMPLEMENT (configuration artifact)
