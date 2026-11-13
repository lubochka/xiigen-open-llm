# Layer 0-C — Knowledge Edge Pairs (049)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-049-001 — ServiceAccountConfig: gap vocabulary — service account runtime operations

Confirmed gap vocabulary for service account runtime operations (zero predicates):
  Action verbs: create (without IaC context), delete, bind (runtime), attach, impersonate, switch (service account), rotate (token — wait: "rotate token" fires SECURITY)
  Exception: "rotate service account token" → SECURITY (rotate + token = key rotation equivalent)
  Runtime nouns: token expiry, OIDC (OpenID Connect) token (runtime), JWT (JSON Web Token) claim (runtime), impersonation session

---

## B14-edge-049-002 — ServiceAccountConfig: disambiguation — "create" vs "implement"

"create a service account" (operational, kubectl create serviceaccount) → gap
"create the service account Kubernetes manifest" (IaC artifact: manifest) → IMPLEMENT
"implement the service account manifest" → IMPLEMENT
Rule: "create" fires IMPLEMENT ONLY when an IaC artifact noun explicitly follows. "create (noun)" without an IaC artifact noun = operational gap.

---

## B14-edge-049-003 — ServiceAccountConfig: SECURITY boundary — service account credential rotation

SECURITY fires for:
  "rotate service account token secrets" → SECURITY (rotate + secret + token)
  "restrict OIDC token lifetime" → SECURITY (credential restriction in security context)
  "enforce mTLS for service account inter-service calls" → SECURITY (mTLS)
  "delete the service account" → gap | "impersonate the service account" → gap

---

## B14-edge-049-004 — ServiceAccountConfig: AUTHORIZATION boundary — Workload Identity Federation

AUTHORIZATION fires for IRSA (IAM Roles for Service Accounts) and WIF (Workload Identity Federation):
  "bind the IAM role to the Kubernetes service account" → AUTHORIZATION ("bind" in IRSA = service account binding)
  "grant IAM role to service account via IRSA" → AUTHORIZATION
  "annotate the service account with IAM role ARN" → gap ("annotate" is kubectl operational gap)
