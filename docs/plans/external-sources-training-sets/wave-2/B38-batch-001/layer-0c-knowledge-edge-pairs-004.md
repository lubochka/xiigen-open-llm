# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-004-001 — Rule: "deployment" alone is GAP in operational context

Rule statement (B38 Core Rule 4):
"deployment" alone → GAP — does not fire IMPLEMENT without code-level context.

"Deployment" in B38 refers to the operational act of deploying — running a deployment, scheduling a deployment, monitoring a deployment. This is operations vocabulary, not code-authoring vocabulary.

IMPLEMENT = code authoring. Running a deployment is not authoring code.

---

## B38-0C-004-002 — "deployment" examples that produce GAP

"Monitor the deployment" → [monitor=GAP, deployment=GAP] → knowledge_edge
"Execute the deployment" → [execute=GAP, deployment=GAP] → knowledge_edge
"Schedule the deployment" → [] → knowledge_edge
"Review the deployment" → ["QA"] (review fires QA; deployment still GAP)
"Validate the deployment readiness" → ["QA"] (validate fires QA; deployment still GAP)
"Deployment criteria" → ["DOCUMENT"] (criteria fires DOCUMENT; deployment still GAP)

In every case, "deployment" does not add IMPLEMENT to the predicate set.

---

## B38-0C-004-003 — Rule: "build" fires IMPLEMENT always (B38 Core Rule 6)

Rule statement:
"build" → IMPLEMENT — fires even in "build provenance", "build artifacts" (not just "build the code")

"Build" is in the IMPLEMENT lexicon because:
- A software build is produced by code authoring
- "build artifacts" are the output of an IMPLEMENT process
- Reviewing "build artifacts" implies code exists and was compiled/packaged
- The IMPLEMENT predicate fires to ensure Phase 4.1 reviews the code-level artifact

---

## B38-0C-004-004 — "build" examples that fire IMPLEMENT

"Audit the build artifacts" → "build" fires IMPLEMENT → ["QA", "IMPLEMENT"]
"Verify build provenance" → "build" fires IMPLEMENT → ["QA", "IMPLEMENT"]
"Review the build pipeline" → "build" + "pipeline" both fire IMPLEMENT → ["QA", "IMPLEMENT"]
"Approve the build for deployment" → "build" fires IMPLEMENT; "deployment" is GAP → ["QA", "IMPLEMENT"]
"Build the authentication module" → "build" fires IMPLEMENT → ["IMPLEMENT"]

---

## B38-0C-004-005 — Boundary: "deployment" (GAP) vs "build" (IMPLEMENT) summary

| Term | Classification | Reasoning |
|---|---|---|
| deployment | GAP | Operational act; not code authoring |
| build | IMPLEMENT | Code artifact; authoring is implied |
| build artifacts | IMPLEMENT (via "build") | Still a code artifact context |
| deployment artifacts | GAP (via "deployment") | Still operational context |
| build deployment | IMPLEMENT + GAP | "build" fires IMPLEMENT; "deployment" still GAP |
| deploy the build | IMPLEMENT + GAP | "build" fires IMPLEMENT; "deploy" context is GAP |
