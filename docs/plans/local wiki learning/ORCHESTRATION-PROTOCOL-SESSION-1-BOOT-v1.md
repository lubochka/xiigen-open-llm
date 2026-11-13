# EXECUTION INSTRUCTIONS — Session 1 of 2
# Title: Session Boot and Evidence Inventory
# Consumer: Orchestrator agent

---

## Goal advancement

Session advances goal (verbatim):
"We would like to prepare a plan how to translate all the documents we gathered so far
into proper batch of learning set. Where each set will feed the proper node in the uml."

Round-trip step advanced: Step 1 — evidence collected, one document selected, UML frame extracted.

Concrete progress:
By completing the evidence inventory and producing 01-UML-FRAME.md, the document-to-node
mapping moves from NOT_STARTED to FRAME_READY for the selected source document.

---

## Prerequisites

Load before starting:
- `ORCHESTRATION-PROTOCOL-STATE.json` → read `user_goal_verbatim` and `active_constraints`
- `ORCHESTRATION-PROTOCOL-REFERENCE-v1.md` → read Failures table (F1–F12) and Array Nodes section

Do NOT proceed if either file is missing.

---

## STEP 1: Read the full user prompt

**Input:** The task prompt as received.

**Action:**
1. Read the complete prompt before taking any action.
2. Extract and record:
   - The object to process: which document family or folder
   - The target map: which UML or flow diagram
   - All named constraints: list them verbatim, one per line

**Gate 1A — prompt fully read:**
- [ ] Object to process: named and recorded
- [ ] Target map (UML): named and recorded
- [ ] All constraints listed verbatim (not paraphrased)

⛔ Do NOT narrow scope or select files until Gate 1A passes.

---

## STEP 2: Identify the target UML — extract all nodes and edges

**Input:** The UML file named in the prompt (e.g. `improve algo.drawio.xml`).

**Action:**
1. Read the UML file.
2. For each node in the diagram, record:
   - Node name (exact label from the diagram)
   - A-type: what this node receives as input
   - B-type: what this node produces as output
   - Is array: YES if the node runs once per item (not once per pipeline run)
3. For each edge in the diagram, record:
   - Source node name → Target node name

**Gate 1B — UML extracted:**
- [ ] Node count recorded (must be > 0)
- [ ] Edge count recorded (must be > 0)
- [ ] Array nodes identified: domain translator, domain processor, applier, result translator
- [ ] Arbiter selector is NOT merged with result translator — they are separate entries
- [ ] Arbiter selector placed: alongside result translator, before formatter
- [ ] Feedback edge present: arbiter units → domain splitter (reprocessing loop)

⛔ If arbiter selector is missing as a separate node: stop, re-read the UML before continuing.

---

## STEP 3: Identify the one source document

**Input:** The folder named in the prompt.

**Action:**
1. List all candidate documents in the named folder.
2. Select ONE document. Selection rule:
   - If the prompt names a specific document: use that exact document.
   - If the prompt says "architecture design": use the primary business document (not the deep-search, not the reference plan, not the amendments file).
   - Name the selected document verbatim: exact file name and path.
3. Do NOT open or read any other document yet.

**Gate 1C — one document selected:**
- [ ] Exactly one document named with full path
- [ ] Document is not a derivative (deep-search, reference plan, amendments, session execution plan)
- [ ] No other document has been read

⛔ Do NOT ask the user which document to pick if the selection rule resolves it unambiguously.
⛔ Do NOT open a second document until the first document's training package passes validation (Session 2 Gate 2F).

---

## STEP 4: Evidence inventory for the selected document

**Input:** The one selected document from Step 3.

**Action:**
1. Read the selected document. Record: file path, size in bytes, line count.
2. Search for version copies: look in `old/`, `Older/`, `.claude/worktrees/`, and any `.zip` archives.
   For each copy found: record path, size, whether content differs (compare hashes or line counts).
3. Search for the creation session: which session file produced this document?
   Record: session file name, line range that contains the creation event.
   If no creation session found: record "NOT FOUND" — do not invent one.
4. Record all version differences as explicit line-level diffs — not "they are similar."

**Gate 1D — evidence inventory complete:**
- [ ] Active source: path, size, line count recorded
- [ ] Version copies searched (old/, Older/, worktrees/, archives)
- [ ] Each version copy: size and content-match status recorded
- [ ] Version differences: exact changed lines listed (not "minor wording")
- [ ] Creation session: found (file + line range) OR explicitly "NOT FOUND"

---

## STEP 5: Write 01-UML-FRAME.md

**Output file:** `[package-folder]/01-UML-FRAME.md`

**Action:**
Write the file with two sections:

Section 1 — Nodes. For each node:
```
Node: [exact name from diagram]
Receives (A): [what arrives at this node at runtime — be specific]
Produces (B): [what this node outputs — be specific]
Array: YES / NO
```

Section 2 — Edges. For each edge:
```
[Source node name] → [Target node name]
```

**Gate 1E — UML frame file valid:**
- [ ] Every node from Gate 1B is present in the file
- [ ] Every edge from Gate 1B is present in the file
- [ ] No node has A or B written as "a general input" or "a general output" — must be specific
- [ ] Array: YES/NO present on every node entry
- [ ] File saved to correct package folder path

---

## SESSION GATE (run before Stop here)

- [ ] Gate 1A passed: prompt read, object/map/constraints recorded
- [ ] Gate 1B passed: UML nodes and edges extracted
- [ ] Gate 1C passed: one document selected, no derivative, no second document opened
- [ ] Gate 1D passed: evidence inventory complete with version diffs
- [ ] Gate 1E passed: 01-UML-FRAME.md written with all nodes and edges

## STATE UPDATE

Save to `ORCHESTRATION-PROTOCOL-STATE.json`:
```json
{
  "current_session": 1,
  "status": "complete",
  "selected_document": "[path to selected document]",
  "source_line_count": [number],
  "uml_node_count": [number],
  "uml_edge_count": [number],
  "version_copies_found": [number],
  "creation_session": "[session file name or NOT FOUND]",
  "next_session": 2
}
```

## ⛔ STOP HERE
Do NOT begin training pair production (Session 2) without confirming Session 1 gate results.
Report: selected document, node count, edge count, version copies, creation session found/not found.
Wait for explicit approval before opening Session 2.

---

## FOUND-ISSUE PROTOCOL

If any gate fails during this session:
1. STOP at the point of failure — do not continue to the next step.
2. Record in ISSUE INVENTORY:
   - Issue: [description]
   - Severity: BLOCKING (must fix before continuing) or NON-BLOCKING
   - Root cause: [one sentence]
   - Fix: [literal action] OR Escalate: [question for user]
3. BLOCKING: fix before proceeding.
4. NON-BLOCKING: complete current step, then fix before Stop here.
5. Post-fix verification: re-run the failed gate check. If it still fails: do NOT mark FIXED.

## ISSUE INVENTORY
| Issue | Status | Fix applied |
|-------|--------|-------------|
| — | — | — |
