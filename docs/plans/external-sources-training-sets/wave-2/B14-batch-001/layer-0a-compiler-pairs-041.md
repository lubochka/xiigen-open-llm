# Layer 0-A — Compiler Pairs (041)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-041-001 — PersistentVolumes: ARCHITECTURE fires on PV topology design

"architect the persistent volume topology for stateful services" → ARCHITECTURE ("architect", "topology")
"design the volume binding and provisioning model" → ARCHITECTURE ("design", "model")
"structure dynamic vs static PV (Persistent Volume) allocation" → ARCHITECTURE ("structure")
"attach a PV to the pod" → gap ("attach" is operational gap)

---

## B14-compiler-041-002 — PersistentVolumes: IMPLEMENT fires on PV manifest artifacts

"implement PersistentVolume and PersistentVolumeClaim manifests" → IMPLEMENT ("implement" + "manifests")
"write the dynamic storage provisioner Terraform module" → IMPLEMENT ("write" + "module")
"build the StatefulSet with volume claim templates" → IMPLEMENT ("build" + code artifact)

---

## B14-compiler-041-003 — PersistentVolumes: SECURITY fires on PV encryption vocabulary

"enforce encryption at rest for all persistent volumes" → SECURITY ("enforce", "encryption at rest")
"rotate KMS keys for volume encryption" → SECURITY ("rotate" + "key" = key rotation)
"configure FIPS-140-2 compliant encryption for PVs" → SECURITY (encryption vocabulary)

---

## B14-compiler-041-004 — PersistentVolumes: AUTHORIZATION fires on PV access control

"grant storage binding ClusterRole to storage-provisioner service account" → AUTHORIZATION ("grant", "ClusterRole", "service account")
"restrict PVC creation to authorized namespaces via RoleBinding" → AUTHORIZATION ("RoleBinding", "restrict" in access context)

---

## B14-compiler-041-005 — PersistentVolumes: gap disambiguation — volume lifecycle ops

Confirmed gap vocabulary for PV operations (fire zero predicates):
  mount, unmount, attach, detach, resize, expand, snapshot (operational), clone, bind (runtime op), reclaim, delete (volume lifecycle)
  "resize the PV" → gap | "snapshot the volume" → gap | "mount the PVC" → gap
