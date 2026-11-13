# Layer 0-C — Knowledge Edge Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-024-001 — Complete gap vocabulary index: DevOps operational actions

Confirmed gap action verbs (zero predicates when used alone):
  scale, upgrade, restart, rollback, enable, disable, purge, run, sync, merge, push, commit, branch, install, start, stop, terminate, reboot, snapshot, add, delete, remove, open (port), close (port), attach, detach, mount, update (operational), refresh, drain, cordon, uncordon, evict

---

## B14-edge-024-002 — Complete SECURITY trigger vocabulary for DevOps contexts

SECURITY fires on:
  Terms: secret, certificate, tls, ssl, mTLS, zero-trust, encrypt, decrypt, credential, key rotation, vault, IAM (in security binding context), API key, RBAC (access control binding), token (authentication context)
  Compound phrases: "TLS termination", "mTLS enforcement", "secret rotation", "certificate lifecycle", "key hierarchy", "secrets injection", "credential binding", "encryption at rest", "encryption in transit"
  Does NOT fire on: "secure" adjective alone, "firewall" alone, "audit" alone, "port 443" alone, "HTTPS" alone

---

## B14-edge-024-003 — IMPLEMENT boundary: IaC context vs operational action

IMPLEMENT fires when: write, build, create, scaffold, implement, configure (IaC/code context) appear with a code artifact noun (module, Dockerfile, playbook, chart, template, script, configuration file)
IMPLEMENT does NOT fire when the same verbs appear in a purely operational context:
  "configure the load balancer" (operational UI action) → gap
  "configure the Helm values file" (code artifact) → IMPLEMENT
  The determining factor is whether the action produces a code artifact

---

## B14-edge-024-004 — Cross-predicate suppression rule (negative): predicates never suppress each other

Rule: When vocabulary for multiple predicates appears in one request, ALL applicable predicates fire.
  SECURITY vocabulary does not suppress ARCHITECTURE. IMPLEMENT does not suppress ARCHITECTURE.
  Gap vocabulary does not suppress any predicate.
  The only exception: zero-predicate requests → all gap → knowledge_edge.
  There is no dominance hierarchy that suppresses predicates; all fire independently.
