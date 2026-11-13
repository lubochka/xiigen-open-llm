# Layer 0-A — Compiler Pairs (060)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-060-001 — Complete factory reference: IMPLEMENT predicate IaC artifact nouns (extended)

IMPLEMENT fires when these extended artifact nouns follow design/creation verbs:
  All prior nouns (036-003) PLUS:
  ConstraintTemplate, ConstraintPolicy, AdmissionWebhook, PeerAuthentication, AuthorizationPolicy, DaemonSet, CronJob, PodSecurityAdmission, OPA rule file, Rego policy module, Falco rule file, DLM (Data Lifecycle Manager) policy, WAF web ACL, RateLimit rule, Certificate, CertificateRequest, Issuer (cert-manager), ClusterIssuer

---

## B14-compiler-060-002 — Complete factory reference: SECURITY predicate compound phrases (extended)

SECURITY fires for ALL compound phrases listed at 036-004 PLUS:
  zero-trust policy, permission boundary enforcement, runtime security enforcement, container security context hardening, syscall policy enforcement, WAF rule enforcement, DDoS mitigation policy, pod security standard enforcement, admission webhook TLS, OPA constraint enforcement, Falco rule enforcement

---

## B14-compiler-060-003 — Complete factory reference: AUTHORIZATION predicate vocabulary (extended)

AUTHORIZATION fires on ALL vocabulary listed at 036-001 PLUS extended role nouns:
  AuthorizationPolicy (Istio), PodSecurityPolicy (PSP) binding, webhook admission service account, OPA controller ClusterRole, cert-manager ClusterRole, Falco ClusterRole, WAF management IAM role, backup-operator service account, snapshot-role, capacity-admin role

---

## B14-compiler-060-004 — Complete factory reference: gap vocabulary master list (topics 037–059)

Confirmed gap for topics 037–059 (fire zero predicates, emit CapabilityGapWarning gap-devops-cloud-001):
  Platform names: spot, on-demand, preemptible, savings plan, reserved instance, FinOps, DLM, ACME, Let's Encrypt, cert-manager, Calico, Falco, OPA, Gatekeeper, Patroni, IRSA, Shield Advanced, Cloudflare
  Operational verbs: flush, warm, invalidate, evict, mount, attach, purchase, renew, commit (financial), ban, block (IP), scrub, absorb, quarantine, blackhole, mute (alert), silence, acknowledge, tail, ping, nslookup, dig

---

## B14-compiler-060-005 — Complete predicate reference: all 6 B14 predicates — final master summary

Full trigger vocabulary consolidated from files 001–060:
  ARCHITECTURE: design, architect, structure, blueprint, diagram, define (design context), topology, architecture, infrastructure, layer, boundary, framework, model, component, hierarchy, pattern
  IMPLEMENT: implement, write (IaC), scaffold, build (container), create (IaC artifact) + IaC artifact noun
  SECURITY: TLS, mTLS, zero-trust, encrypt, decrypt, secret, certificate, key rotation, vault, harden, rotate (keys/certs), credential, permission boundary enforcement
  AUTHORIZATION: authorize, grant, revoke, restrict (access context), permit (access context), IAM role, RBAC role, ClusterRole, RoleBinding, service account binding, trust policy, permission boundary
  API_CONTRACT: define (contract context), specify (API), expose (endpoint), publish (API), API contract, endpoint, schema, OpenAPI, contract, service interface
  DOCUMENT: document, record, write (ADR/runbook), describe (doc context), ADR, runbook, playbook (doc), README, wiki, guide
