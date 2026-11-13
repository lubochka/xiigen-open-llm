# Layer 0-A — Compiler Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-017-001 — QA+ARCHITECTURE: infrastructure release design

A: "Design the infrastructure release architecture and QA the design before provisioning."
  "design" → ARCHITECTURE | "infrastructure release architecture" → ARCHITECTURE | "QA" → QA | "provisioning" → gap
  CompiledPredicateIds: ["QA", "ARCHITECTURE"]

---

## B28-compiler-017-002 — QA+IMPLEMENT: infrastructure release service

A: "Build the infrastructure provisioning service and QA the release gate for the IaC (Infrastructure as Code) pipeline."
  "build" → IMPLEMENT | "IaC pipeline" → gap | "QA" → QA | "release gate" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT"]

---

## B28-compiler-017-003 — gap: infrastructure provisioning

A: "Provision the Kubernetes (K8s) cluster and deploy the infrastructure stack."
  "provision" → gap | "deploy" → gap | "K8s cluster" → gap | "infrastructure stack" → gap
  CompiledPredicateIds: [] → gap

---

## B28-compiler-017-004 — QA+ARCHITECTURE+IMPLEMENT: infra release system

A: "Design the release gate architecture, build the gate enforcement service, and QA the infrastructure release."
  "design" → ARCHITECTURE | "build" → IMPLEMENT | "QA" → QA | "gate enforcement service" → IMPLEMENT
  CompiledPredicateIds: ["QA", "ARCHITECTURE", "IMPLEMENT"]

---

## B28-compiler-017-005 — QA+IMPLEMENT+DOCUMENT: IaC release pipeline

A: "Build the Terraform (IaC) release pipeline, document the infrastructure release criteria, and QA approve."
  "build" → IMPLEMENT | "document" → DOCUMENT | "release criteria" → DOCUMENT | "QA approve" → QA | "Terraform" → gap
  CompiledPredicateIds: ["QA", "IMPLEMENT", "DOCUMENT"]
