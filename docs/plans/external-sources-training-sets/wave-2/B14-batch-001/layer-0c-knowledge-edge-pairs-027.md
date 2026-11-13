# Layer 0-C — Knowledge Edge Pairs (027)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-027-001 — ArgoCD: "sync" is always gap

Gap: "sync the ArgoCD application", "refresh the repository", "force sync the app"
No predicates fire from ArgoCD operational sync/refresh actions
Teaching: GitOps operational triggers (sync, refresh, force-sync) are runtime actions → gap. Designing the GitOps topology fires ARCHITECTURE.

---

## B14-edge-027-002 — ArgoCD: SECURITY boundary — RBAC vs operational access

SECURITY fires: "configure RBAC for ArgoCD" — RBAC = access control binding → SECURITY fires even with operational verb "configure"
Gap: "add the user to ArgoCD" (operational user management), "change the ArgoCD password" (operational credential change, not rotation)
Teaching: RBAC vocabulary fires SECURITY; generic user management actions are operational gap.

---

## B14-edge-027-003 — ArgoCD: IMPLEMENT boundary for ApplicationSet

IMPLEMENT fires: "implement the ApplicationSet controller", "write the app-of-apps chart", "scaffold the ArgoCD project manifest"
Gap: "create the ArgoCD project" (UI operational create), "add the application" (operational add action)
Teaching: "create" in UI/console context is operational gap. "implement" or "write" with code artifact noun fires IMPLEMENT.

---

## B14-edge-027-004 — ArgoCD: "GitOps" fires ARCHITECTURE only with design verb

ARCHITECTURE fires: "design the GitOps pipeline topology" ("GitOps" + infra vocabulary + design verb)
Gap: "adopt GitOps" (strategic/organizational statement — no design verb + infra noun structure)
Gap: "enable GitOps for the cluster" (operational enable action)
Teaching: "GitOps" alone is infra vocabulary but requires a design verb to fire ARCHITECTURE.
