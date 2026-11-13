# Layer 0-A — Compiler Pairs (052)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-052-001 — PodSecurityPolicy: ARCHITECTURE fires on PSP (Pod Security Policy) design

"architect the pod security standard topology for multi-tenant clusters" → ARCHITECTURE ("architect", "topology")
"design the restricted pod security profile model" → ARCHITECTURE ("design", "model")
"structure pod security admission control layers" → ARCHITECTURE ("structure", "layers" in design context)
"check if PSP is enabled" → gap ("check" is operational gap)

---

## B14-compiler-052-002 — PodSecurityPolicy: IMPLEMENT fires on PSP IaC artifacts

"implement the Pod Security Admission (PSA) policy manifest" → IMPLEMENT ("implement" + "manifest")
"write the namespace-level PodSecurityPolicy Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the restricted security context configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-052-003 — PodSecurityPolicy: SECURITY fires on PSP security vocabulary

"enforce the restricted pod security standard" → SECURITY ("enforce" in security context)
"harden container security contexts with no-root policy" → SECURITY ("harden")
"configure seccompProfile and AppArmor annotations for pod hardening" → SECURITY (security configuration for pod)

---

## B14-compiler-052-004 — PodSecurityPolicy: AUTHORIZATION fires on PSP binding vocabulary

"bind the pod security policy to the workload service account" → AUTHORIZATION ("service account binding")
"grant use permission on the restricted PSP to the deployment service account" → AUTHORIZATION ("grant", "service account")
"restrict privileged pod execution to the kube-system namespace via RoleBinding" → AUTHORIZATION ("RoleBinding", "restrict")

---

## B14-compiler-052-005 — PodSecurityPolicy: multi-predicate compilation

"Design the pod security standard model, enforce the restricted profile, and implement the admission configuration."
  "design" → ARCHITECTURE | "model" → ARCHITECTURE | "enforce" → SECURITY | "restricted profile" → SECURITY | "implement" → IMPLEMENT | "admission configuration" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "IMPLEMENT"]
