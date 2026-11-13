# Layer 3 — Teaching Unit Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T024-001 — Complete predicate rule: ARCHITECTURE vocabulary hierarchy

ARCHITECTURE fires on (ranked by reliability):
  Most reliable (always fire): design, architect, topology, architecture, infrastructure, layer, boundary, pattern
  Reliable (fire in design/structural context): structure, define, blueprint, diagram, framework, model, component, deployment (design context), pipeline (infra context), cluster (design context), distributed, scalability, system, coupling, cohesion
  Context-dependent: network, service (in "service mesh"), queue (in message queue design), cache (in "cache layer")

---

## T024-002 — Complete predicate rule: SECURITY vocabulary hierarchy

SECURITY fires on (ranked by reliability):
  Most reliable (always fire): tls, ssl, mTLS, zero-trust, encrypt, decrypt, secret, certificate, credential, key rotation, vault
  Reliable: IAM (in credential/binding context), RBAC (in access control binding context), API key, token (auth context)
  Context-dependent: "secure" (adjective alone does not fire; "secure communication" → SECURITY via "communication" in TLS context)

---

## T024-003 — Complete gap rule: when predicates fire despite gap vocabulary

Rule: Gap vocabulary in a request does NOT suppress predicates fired by other non-gap vocabulary in the same request.
  "Set up AWS EKS" → gap + gap → zero predicates (ALL vocabulary is gap)
  "Design the AWS infrastructure" → ARCHITECTURE fires on "design" + "infrastructure"; "AWS" is gap but does not suppress
  "Write the Terraform modules for the VPC" → IMPLEMENT fires on "write" + "modules"; "Terraform", "VPC" are gap but don't suppress
  Gap emits CapabilityGapWarnings but never suppresses a fired predicate.

---

## T024-004 — Complete rule: "infra vocabulary" signals 3.5 vs 3.1

When ARCHITECTURE fires, sub-phase 3.5 is selected when the compiled text contains infra-domain vocabulary:
  3.5 signals: infrastructure, topology, cluster, node, mesh, VPC, network (infra context), CDN, edge, deployment layer, container, Kubernetes, EKS, GitOps, observability, pipeline (infra), DR (disaster recovery)
  3.1 signals: domain, service boundary, bounded context, aggregate, DDD (Domain-Driven Design), CQRS (Command Query Responsibility Segregation), SAGA, module (app-level)
  Mixed: dominant vocabulary wins; infra vocabulary density > app vocabulary → 3.5

---

## T024-005 — Complete TransitionType summary for B14 scenarios

feature_addition: IMPLEMENT present; OR ARCHITECTURE alone with design verb (design, architect, structure, define); OR TEST (test artifact addition)
documentation: ARCHITECTURE + DOCUMENT without IMPLEMENT; OR DOCUMENT alone
unknown: zero predicates → knowledge_edge
Never: ARCHITECTURE alone routes to documentation only when the request's primary verb is a recording verb (document, diagram, write up) not a design/creation verb.
