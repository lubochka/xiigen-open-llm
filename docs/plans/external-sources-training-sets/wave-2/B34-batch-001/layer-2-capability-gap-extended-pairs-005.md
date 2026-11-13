# Layer 2 — Capability Gap Extended Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-005 — GapHandler nine-node: "Train the team on Kubernetes"

Request: "Train the backend team on Kubernetes deployment and container orchestration."

### Flow: GapHandler Nine-Node

**Node 1**: Parse. "train" (human learning) → guidance vocabulary → gap.
**Node 2**: Predicate scan: "Kubernetes deployment" = topic noun; no code-action verb. Predicate set = [].
**Node 3**: Gap classification: skill training/education. Gap category: training.
**Node 4**: Gap ID: gap-skills-guidance-train-001
**Node 5**: DomainId: software-development
**Node 6**: Emit SCHEDULE_CAPABILITY_LEARNING
**Node 7**: Log: {id: gap-skills-guidance-train-001, request: "train team on Kubernetes", domainId: software-development}
**Node 8**: Response: "Capability not available. Request logged for learning pipeline."
**Node 9**: Verdict: GAP_LOGGED

---

**Contrast**: "Build the Kubernetes deployment pipeline" → IMPLEMENT (code verb overrides).
