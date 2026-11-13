# Layer 0-C — Knowledge Edge Pairs (052)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-052-001 — PodSecurityPolicy: gap vocabulary — PSP (Pod Security Policy) runtime operations

Confirmed gap vocabulary for pod security runtime operations (zero predicates):
  Action verbs: enable (admission), disable (PSP), check (seccomp), verify (pod security), debug (pod security), list (PSPs), describe (PSP)
  Runtime nouns: seccomp profile (runtime), AppArmor annotation (runtime), securityContext (runtime field), privileged flag (runtime)

---

## B14-edge-052-002 — PodSecurityPolicy: SECURITY boundary — pod hardening compound phrases

SECURITY fires for:
  "enforce restricted pod security standard" → SECURITY ("enforce" + security context)
  "harden container security contexts" → SECURITY ("harden")
  "configure seccompProfile for pod hardening" → SECURITY (security hardening context)
  "enable privileged mode" → gap ("enable" is operational gap)

---

## B14-edge-052-003 — PodSecurityPolicy: AUTHORIZATION boundary — PSP binding

AUTHORIZATION fires for:
  "bind PSP to service account" → AUTHORIZATION ("bind" in service account binding context)
  "grant use permission on restricted PSP to service account" → AUTHORIZATION ("grant", "service account")
  "restrict privileged pods via RoleBinding" → AUTHORIZATION ("RoleBinding", "restrict")

---

## B14-edge-052-004 — PodSecurityPolicy: disambiguation — Pod Security Admission (PSA) vs PSP (deprecated)

Both PSA (Pod Security Admission) and PSP (Pod Security Policy, deprecated in Kubernetes 1.25) vocabulary:
  fires IMPLEMENT when followed by "manifest", "configuration", "module" (IaC artifact)
  fires SECURITY when "enforce", "restrict", "harden" precede
  fires ARCHITECTURE when "design", "architect", "structure" precede
  The specific technology (PSA vs PSP) does not change predicate routing — the verb determines the predicate.
