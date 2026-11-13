# Layer 0-A — Compiler Pairs (021)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-021-001 — ARCHITECTURE: GitOps topology

A: "Design the GitOps deployment topology — Git repository as the source of truth, reconciliation loop, and environment promotion pipeline."
  "design" → ARCHITECTURE | "GitOps deployment topology" → ARCHITECTURE ("topology", "deployment") | "reconciliation loop" → ARCHITECTURE ("pattern") | "environment promotion pipeline" → ARCHITECTURE ("pipeline", "layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "Git repository", "source of truth" → tool/operational vocabulary, gap

---

## B14-compiler-021-002 — ARCHITECTURE+IMPLEMENT: ArgoCD pipeline config

A: "Architect the continuous delivery pipeline topology and implement the ArgoCD application set configuration."
  "architect" → ARCHITECTURE | "continuous delivery pipeline topology" → ARCHITECTURE ("topology", "pipeline") | "implement" → IMPLEMENT | "ArgoCD application set configuration" → IMPLEMENT
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "ArgoCD", "application set" → tool vocabulary, gap

---

## B14-compiler-021-003 — ARCHITECTURE+DOCUMENT: deployment pipeline ADR

A: "Define the deployment pipeline architecture and document the promotion gates and approval workflows."
  "define" → ARCHITECTURE | "deployment pipeline architecture" → ARCHITECTURE ("pipeline", "architecture") | "document" → DOCUMENT | "promotion gates" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "approval workflows" → process vocabulary, gap

---

## B14-compiler-021-004 — ARCHITECTURE: environment topology

A: "Structure the environment topology — dev, staging, pre-prod, and production environments with promotion boundaries and rollback capabilities."
  "structure" → ARCHITECTURE | "environment topology" → ARCHITECTURE ("topology") | "promotion boundaries" → ARCHITECTURE ("boundary") | "rollback capabilities" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "dev", "staging", "pre-prod", "production" → environment label vocabulary, gap

---

## B14-compiler-021-005 — gap: "run the deployment pipeline"

A: "Run the deployment pipeline for the payment-service."
  "run" → gap (execution action, not in any predicate lexicon) | "deployment pipeline" → ARCHITECTURE? — pipeline as a noun without design context; "run" as the primary verb overrides
  CompiledPredicateIds: []
  Teaching: "run" is operational execution gap. "deployment pipeline" as a noun does not fire ARCHITECTURE without a design verb. To fire ARCHITECTURE: "design the deployment pipeline topology."
