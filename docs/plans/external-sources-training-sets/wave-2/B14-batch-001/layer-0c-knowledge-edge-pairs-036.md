# Layer 0-C — Knowledge Edge Pairs (036)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-036-001 — Complete gap vocabulary index: observability and monitoring operational actions

Confirmed gap action verbs for observability (zero predicates when used alone):
  view, check, query, watch, tail, stream, grep (log search), filter (log/metric filter), export (data export), import, reload (config reload), flush (buffer flush), drain (queue drain), snapshot (metric snapshot), sample, probe

---

## B14-edge-036-002 — Complete gap vocabulary index: GitOps and deployment operational actions

Confirmed gap action verbs for GitOps and deployment:
  sync, merge, push, pull, commit, branch, tag (git tag), release, promote, rollback, rollforward, deploy (standalone), undeploy, canary (promote), drain, cordon, uncordon, evict, taint, label (kubectl), annotate

---

## B14-edge-036-003 — Complete IMPLEMENT boundary: IaC artifact noun reference

IMPLEMENT fires when these artifact nouns follow "write", "implement", "scaffold", "create" (IaC context):
  module, Dockerfile, playbook (Ansible), chart (Helm), manifest, template (Terraform/CloudFormation), configuration file, values file, policy file, rule file, pipeline definition, workflow file, script (automation), operator (Kubernetes CRD), controller (code artifact)

---

## B14-edge-036-004 — Complete SECURITY boundary: compound phrases that always fire

SECURITY fires regardless of surrounding verb context for these compound phrases:
  TLS termination, mTLS enforcement, secret rotation, certificate lifecycle, key hierarchy, secrets injection, credential binding, encryption at rest, encryption in transit, zero-trust policy, permission boundary enforcement, vault dynamic credentials, API key rotation, RBAC binding (in security context)
