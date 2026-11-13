# Held-Out and Corruption Pairs (017)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H017-001 — Held-out: QA+ARCHITECTURE — infrastructure release design

A: "Design the infrastructure release gate architecture and QA the design before provisioning."
  Expected: ["QA", "ARCHITECTURE"]
  Held-out: "design" → ARCHITECTURE; "architecture" → ARCHITECTURE (dedup); "QA" → QA; "provisioning" → gap. documentation.

---

## H017-002 — Held-out: gap — infrastructure provisioning

A: "Provision the Kubernetes cluster using Terraform and deploy the infrastructure stack."
  Expected: []
  Held-out: "provision" → gap; "deploy infrastructure" → gap. knowledge_edge.

---

## H017-003 — Held-out: QA+ARCHITECTURE+IMPLEMENT — design + build + gate

A: "Design the release gate architecture, build the gate service, and QA approve."
  Expected: ["QA", "ARCHITECTURE", "IMPLEMENT"]
  Held-out: "design" → ARCHITECTURE; "build" → IMPLEMENT; "service" → IMPLEMENT (dedup); "QA approve" → QA. feature_addition.

---

## C017-001 — Corruption: "provisioning" fires IMPLEMENT

A: "Provision the infrastructure stack."
  Correct: [] — gap
  Corrupt: ["IMPLEMENT"] ← wrong ("provision" treated as implementation verb)
  Teaching: "provision" is gap vocabulary. It does not fire IMPLEMENT.

---

## C017-002 — Corruption: IaC vocabulary fires ARCHITECTURE

A: "Apply the Terraform infrastructure code."
  Correct: [] — gap
  Corrupt: ["ARCHITECTURE"] ← wrong (IaC execution treated as architecture design)
  Teaching: IaC execution (Terraform apply, Ansible run) → gap. "design the architecture" fires ARCHITECTURE; "apply Terraform" does not.
