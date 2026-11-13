# Layer 0-C — Knowledge-Edge Pairs (027)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-027-001 — NightlyBuild: "build" verb vs noun boundary

The word "build" fires IMPLEMENT (rule 6 — absolute) when used as a code-authoring verb.
When "build" is used as a scheduling noun, it does NOT fire IMPLEMENT.

### Verb context (fires IMPLEMENT)
| Input | "build" usage | IMPLEMENT fires? |
|---|---|---|
| "Build the nightly pipeline" | Code-authoring verb | YES |
| "Build a nightly release service" | Code-authoring verb | YES |
| "Build and validate the nightly artifacts" | Code-authoring verb | YES |

### Noun context (does NOT fire IMPLEMENT)
| Input | "build" usage | IMPLEMENT fires? |
|---|---|---|
| "Schedule the nightly build" | Schedule noun | NO |
| "The nightly build completed at 2am" | Completed job noun | NO |
| "Monitor the nightly build output" | Monitoring noun | NO |

---

## B38-0C-027-002 — WeeklyRelease knowledge edge boundary

### WeeklyRelease terms that produce knowledge_edge
| Input | Reason | PredicateSet |
|---|---|---|
| "Schedule the weekly release cadence." | "Schedule" not in lexicon; "cadence" is context | [] |
| "Assign a weekly release window." | "Assign" not in lexicon | [] |
| "Plan the weekly release train." | "Plan" not in lexicon | [] |
| "Update the weekly release calendar." | "Update" alone not sufficient — not code context | [] |

---

## B38-0C-027-003 — Gap vocabulary in build cadence sentences

| Term | Gap ID | Example sentence |
|---|---|---|
| rollout | gap-project-delivery-rollout-golive-001 | "Approve the weekly rollout" → ["QA"] + gap: rollout |
| deployment | gap-project-delivery-deployment-001 | "Approve the nightly deployment" → ["QA"] + gap: deployment |
| cutover | gap-project-delivery-cutover-001 | "Plan the weekly cutover" → [] + gap: cutover (knowledge_edge) |
| launch | gap-project-delivery-launch-001 | "Launch the weekly build" → [] + gap: launch (knowledge_edge) |
| go-live | gap-project-delivery-go-live-001 | "Weekly go-live sign-off" → ["QA"] + gap: go-live |
