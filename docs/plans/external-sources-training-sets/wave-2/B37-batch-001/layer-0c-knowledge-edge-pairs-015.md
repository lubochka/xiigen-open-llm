# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-015-001 — "project-charter" knowledge edge: programme governance document

**Knowledge edge: project charter is programme governance vocabulary**

A project charter is the founding document of a project — it authorises the project, defines scope, objectives, and key stakeholders. "Project charter" is B37 gap vocabulary. It has no registered predicate.

**Gap ID:** gap-project-managing-manage-delivery-001

**Important "create" false positive:**
"Create the project charter" → IMPLEMENT fires on "create" (Rule 9). This is a known false positive. The compiler fires IMPLEMENT on the lexical token "create" even when the intent is authoring a governance document, not building software.

---

## B37-0C-015-002 — "create" false positive for document creation

**Knowledge edge: "create" always fires IMPLEMENT**

| Input | Predicates | Reason |
|-------|-----------|--------|
| "Create the project charter" | [IMPLEMENT] | "create" fires IMPLEMENT (Rule 9) |
| "Create the risk log" | [IMPLEMENT] | "create" fires IMPLEMENT |
| "Create the RACI matrix" | [IMPLEMENT] | "create" fires IMPLEMENT |
| "Circulate the project charter" | [] | "circulate" not in any lexicon |
| "Sign off the project charter" | [] | "sign off" not in any lexicon |

**Teaching point:** Avoid "create" in managing requests if zero-predicate output is expected. "Circulate", "distribute", "share", "prepare" do not fire any predicate.

---

## B37-0C-015-003 — "project-charter" in zero-predicate scenarios

**Zero-predicate project charter scenarios:**
- "Circulate the project charter for sign-off." → []
- "Prepare the project charter outline." → []
- "Distribute the project charter to workstream leads." → []
- "Finalise the project charter before the kickoff." → []
- "Approve the project charter with the programme board." → []

All five produce zero predicates. DomainId "project-management". SCL emitted. Gap ID: gap-project-managing-manage-delivery-001.

---

## B37-0C-015-004 — "project-charter" with "define": DOCUMENT fires

**Input:** "Define the scope and objectives in the project charter."

Knowledge edge: "define" fires DOCUMENT. Even though the intent is to establish project scope (a managing activity), the token "define" triggers DOCUMENT regardless of context.

Gap warning: gap-project-managing-manage-delivery-001 (scope definition is delivery management intent).

**Teaching point:** If the user intends to "set out the scope" without firing DOCUMENT, they should use vocabulary like "outline", "establish", "set out" rather than "define".

---

## B37-0C-015-005 — "project-charter" gap ID confirmation

**Gap ID:** gap-project-managing-manage-delivery-001

Project charter vocabulary family:
- project charter
- project initiation document (PID)
- business case (project management context)
- programme mandate
- project brief

All map to: gap-project-managing-manage-delivery-001 (project initiation = delivery management vocabulary)

**Boundary:** "project charter" ≠ "project specification". "Specification" would fire DOCUMENT if "spec" is used. "Project charter" by itself fires nothing.
