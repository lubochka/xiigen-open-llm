# Layer 0-A — Compiler Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-024-001 — Complete predicate reference: ARCHITECTURE triggers

ARCHITECTURE fires on ALL of the following vocabulary:
  Action verbs: design, architect, structure, define (in structural context), blueprint, diagram
  Structural nouns: architecture, topology, layer, boundary, component, infrastructure, pattern, framework, module (structural), pipeline (structural), system, cluster (in design context), network (in design context), service mesh, microservice, monolith, distributed, event, queue, cache, coupling, cohesion, scalability, deployment (in design context)
  Compound phrases: "service mesh", "data layer", "deployment topology", "network topology", "infrastructure layer"

---

## B14-compiler-024-002 — Complete predicate reference: IMPLEMENT triggers

IMPLEMENT fires on: build, implement, create, scaffold, write (IaC/script context), configure (in concrete coding context), provision (in scripting context), set up (only when in IaC coding context — otherwise gap)
  Code artifact nouns: module, script, Dockerfile, playbook, chart, configuration (concrete)
  IaC context: Terraform modules, Helm charts, Ansible playbooks, CloudFormation templates

---

## B14-compiler-024-003 — Complete predicate reference: SECURITY triggers

SECURITY fires on: secret, certificate, tls, ssl, encrypt (any form), credential, key rotation, vault (tool name fires), security (in policy/configuration context), mTLS, zero-trust, IAM (in security binding context)
  Does NOT fire on: "secure" as an adjective alone, "firewall" alone, "audit" alone
  Compound phrases: "TLS termination", "mTLS enforcement", "secret rotation", "certificate lifecycle", "key hierarchy"

---

## B14-compiler-024-004 — Complete gap reference: DevOps operational vocabulary

GAP vocabulary (zero predicates): AWS, Azure, GCP, Kubernetes, EKS, ECS, GKE, S3, Lambda, CloudFront, Route53, Terraform (tool name alone), Helm (tool name alone), ArgoCD (tool name alone)
  Operational actions that are gap: scale, upgrade, restart, rollback, deploy (standalone), run, enable, disable, purge, open (port), rotate (alone), add, delete, install
  Platform names are never predicate triggers; only action vocabulary on top of platform references fires predicates.

---

## B14-compiler-024-005 — Complete rule: TransitionType determination

TransitionType rules:
  feature_addition: IMPLEMENT predicate present → always feature_addition
  documentation: ARCHITECTURE or DOCUMENT only (no IMPLEMENT) + primary verbs produce documentary artifacts (document, diagram, write up ADRs)
  refactoring: "refactor" verb → overrides documentation for ARCHITECTURE flows
  unknown: zero predicates → knowledge_edge; TransitionType = unknown
  Multi-predicate: IMPLEMENT drives feature_addition regardless of other predicates present.
