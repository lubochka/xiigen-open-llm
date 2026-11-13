# Layer 0-C — Knowledge Edge Pairs (041)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-041-001 — PersistentVolumes: gap vocabulary — PV lifecycle operations

Confirmed gap vocabulary for PV (Persistent Volume) lifecycle operations (zero predicates):
  Action verbs: mount, unmount, attach, detach, resize, expand, snapshot (operational), clone, bind (runtime bind), reclaim, delete, format, scrub, wipe
  PV runtime nouns: PV status, binding phase, volume handle, filesystem (runtime), disk quota

---

## B14-edge-041-002 — PersistentVolumes: SECURITY boundary — PV encryption compound phrases

SECURITY compound phrases for PV context:
  "encryption at rest for persistent volumes", "KMS key for volume encryption", "FIPS-140-2 compliant PV encryption", "rotate KMS keys for volume encryption"
  "rotate KMS keys" = key rotation → always fires SECURITY
  "snapshot" alone is gap; "snapshot for backup" is gap; only "snapshot encryption at rest" fires SECURITY

---

## B14-edge-041-003 — PersistentVolumes: IMPLEMENT boundary — PV IaC artifacts

IMPLEMENT fires for: PersistentVolume manifest, PersistentVolumeClaim manifest, StatefulSet with volume claim templates, dynamic storage provisioner Terraform module, CSI (Container Storage Interface) driver configuration
  "implement PV and PVC manifests" → IMPLEMENT | "write the StatefulSet" → IMPLEMENT
  "mount the PVC to the pod" → gap

---

## B14-edge-041-004 — PersistentVolumes: AUTHORIZATION boundary — PV binding roles

AUTHORIZATION fires for:
  "storage binding ClusterRole", "PVC creation RoleBinding", "grant storage binding role to service account"
  "bind PVC at runtime" → gap (runtime bind operation)
  "define RoleBinding for PVC creation" → AUTHORIZATION (policy definition, not runtime operation)
