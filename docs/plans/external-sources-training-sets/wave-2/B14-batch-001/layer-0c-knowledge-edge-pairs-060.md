# Layer 0-C — Knowledge Edge Pairs (060)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-060-001 — Complete gap vocabulary index: security operations (extended 037–059)

Confirmed gap action verbs for security runtime operations (fire zero predicates when used alone):
  renew (certificate), check (expiry), verify (operational), download (cert), export (cert), trust (add to trust store), inspect (operational), enable (security feature), disable (security feature)
  Also gap: absorb, scrub, redirect (traffic), blackhole, ban, block (IP), quarantine, mute (alert), silence, acknowledge (alert), escalate (alert), triage

---

## B14-edge-060-002 — Complete gap vocabulary index: storage/data operations (extended 037–059)

Confirmed gap action verbs for storage/data runtime operations:
  mount, unmount, attach, detach, resize, expand, clone, bind (runtime), reclaim, delete (volume/backup), format, wipe, purge (backup/cache), flush (cache), invalidate (cache), evict (cache), warm (cache), set (cache key), get (cache key)

---

## B14-edge-060-003 — Complete SECURITY compound phrase master list (topics 037–059 additions)

SECURITY fires regardless of verb context for these compound phrases (topics 037–059):
  zero-trust policy, TLS certificate rotation, certificate lifecycle, mTLS enforcement, encryption at rest, encryption in transit, key rotation (any key type), SASL credential rotation, Redis authentication token rotation, IMDSv2 enforcement, permission boundary enforcement, pod security standard enforcement, WAF rule enforcement, DDoS mitigation policy, Falco runtime security enforcement, OPA constraint enforcement, syscall policy enforcement

---

## B14-edge-060-004 — Complete disambiguation master: verb-based predicate routing (topics 037–059)

ARCHITECTURE: design, architect, structure, blueprint, diagram, define (design context)
IMPLEMENT: implement, write (IaC), scaffold, build (container/IaC), create (with IaC artifact noun)
SECURITY: harden, enforce (security noun), rotate (key/cert/credential), encrypt, configure mTLS/TLS; also: SECURITY compound phrases fire from noun alone
AUTHORIZATION: grant, revoke, permit (access), restrict (access), authorize, bind (ClusterRole/RoleBinding context), define (role/policy noun)
API_CONTRACT: define (contract/endpoint/schema noun), specify (API noun), expose (endpoint), publish (API)
DOCUMENT: document, record, write (ADR/runbook noun), describe (doc context)
