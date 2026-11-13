# Layer 0-D — Flow Factory Pairs (025)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-025-001 — "show how to" factory: GapHandler only

A: "Show the platform team how to configure the service mesh."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: show how to = learning target infinitive → GapHandler. No code flow selected.

---

## FF-025-002 — "demonstrate" factory: GapHandler only

A: "Demonstrate the blue-green deployment strategy to the DevOps (Development Operations) team."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: demonstrate = guidance vocabulary → GapHandler. No code flow selected.

---

## FF-025-003 — "introduce [person]" factory: GapHandler only

A: "Introduce the new QA (Quality Assurance) engineers to our testing infrastructure."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: introduce [person] to = onboarding vocabulary → GapHandler → gap-skills-guidance-onboard-001.

---

## FF-025-004 — "introduce [feature]" factory: ImplementationFlow41

A: "Introduce request batching into the API (Application Programming Interface) gateway."
Factory: ImplementationFlow41 | TransitionType: feature_addition
Evidence: introduce [feature] into [system] = IMPLEMENT → ImplementationFlow41. No gap.

---

## FF-025-005 — "brief" factory: GapHandler only

A: "Brief the infrastructure team on the new Terraform (Terraform IaC tool) module conventions."
Factory: GapHandler | TransitionType: none | SCHEDULE_CAPABILITY_LEARNING: emitted once
Evidence: brief [person] on = knowledge-transfer vocabulary → GapHandler → gap-skills-guidance-mentor-001.
