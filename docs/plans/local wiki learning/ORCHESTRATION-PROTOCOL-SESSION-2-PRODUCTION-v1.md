# EXECUTION INSTRUCTIONS — Session 2 of 2
# Title: Training Set Production and Validation
# Consumer: Orchestrator agent

---

## Goal advancement

Session advances goal (verbatim):
"We would like to prepare a plan how to translate all the documents we gathered so far
into proper batch of learning set. Where each set will feed the proper node in the uml."

Round-trip step advanced: Step 2 — per-node training sets produced, validated, and batch advanced.

Concrete progress:
By producing validated per-node training sets and marking the document complete in the run index,
the document moves from FRAME_READY to COMPLETE in the run ledger, and the queue advances to
the next source document.

---

## Prerequisites

- Session 1 gate fully passed (STATE.json `current_session` = 1, `status` = "complete")
- `01-UML-FRAME.md` written and validated
- One source document selected — `selected_document` in STATE.json
- Evidence inventory complete — `creation_session` recorded

---

## INLINE CONSTRAINTS (active for all steps in this session)

```
ACTIVE CONSTRAINTS (verbatim from user goal):
1. Do not cut edges — every source line is considered, no line is merged away
2. Do not shorten texts — source text is not reduced
3. No code or technical terms in training pairs — use plain human language
4. f(x) = rephrase(x) — every concept appears in rephrased form in at least one pair
5. One document — no content from other documents enters training pairs
6. Do one file, check yourself — validate before marking complete
```

```
BANNED PHRASES IN B COLUMN — scan for these after writing each node file:
"a clear work statement"
"produce a result that preserves"
"translate it"
"plan it"
"a translated version"
"a processed requirement"
If any banned phrase found in B: the pair is not a training pair — rewrite with full actual output.
```

```
ARRAY NODE RULE:
Domain translator / Domain processor / Domain applier / Result translator are ARRAYS.
For array node pairs:
  A = one item (one domain, one sub-task, one result fragment) — NOT a full list
  B = one output for that item — NOT all outputs combined
Pairs that show A as a full list and B as a combined output: INVALID — rewrite.
```

```
DERIVED EXAMPLE MARKING:
Any pair not directly taken from the source document must be marked:
  [DERIVED from: name the source section or line that implies this rule]
Unmarked pairs that are not from the source document: INVALID.
```

---

## STEP 1: Write per-node training sets (node-01 through node-09)

For each node in 01-UML-FRAME.md:

**Action — for each node:**
1. Open the source document. Read the section(s) relevant to this node.
2. Identify what a runtime user would actually send to this node (not a document line — a real request fragment).
3. Write pair 1:
   ```
   A: [what the node receives at runtime — a real human request or real domain fragment, full text]
   B: [what the node produces — the actual output written in full, not described]
   ```
4. Write pairs 2 through 8+ by rephrasing: same scenario, different wording (f(x) = rephrase rule).
5. If the source document contains edge-case or rejection material for this node: write one derived pair,
   mark it [DERIVED from: section name].

**Output file per node:** `[package-folder]/node-0N-[node-name]-set.md`

**Gate 2A — per-node files check (run after writing ALL nine files):**
- [ ] Nine node files present: node-01 through node-09
- [ ] Each node file has ≥ 8 runtime A→B pairs
- [ ] No banned phrase found in any B column (scan: "a clear work statement", "translate it", "plan it", "produce a result", "a translated version", "a processed requirement")
- [ ] Array nodes (node-02, node-04, node-05, node-06): A is one item, B is one result
- [ ] Arbiter selector is node-07 — NOT merged into node-06
- [ ] Every non-derived pair traceable to source document
- [ ] Every derived pair marked with [DERIVED from: ...]

⛔ If Gate 2A fails on any node file: fix that file before recording as done.

---

## STEP 2: Write 00-INDEX.md

**Output file:** `[package-folder]/00-INDEX.md`

**Action:**
Write with these sections:
1. Source document: path, size, line count, hash (if available)
2. Version copies: list each copy found with path and content-match status
3. Creation session: session file + line range, or "NOT FOUND"
4. Version differences: exact changed lines (current vs older)
5. UML target: path to 01-UML-FRAME.md
6. Node file list: one line per node file with pair count

**Gate 2B — index file check:**
- [ ] Source document section present with path, size, line count
- [ ] Version differences section present — exact lines listed, not "minor wording"
- [ ] Creation session recorded (or explicitly "NOT FOUND")
- [ ] Node file list shows pair counts

---

## STEP 3: Write 10-SIMULATION-CHECK.md

**Output file:** `[package-folder]/10-SIMULATION-CHECK.md`

**Action:**
Write with these sections:
1. Test input: a real user request relevant to the source document's domain
2. Expected node sequence: what each node produces given that input
3. Expected final output: what the formatter produces
4. Missing parts: list what the training set does NOT yet teach (from simulation)
5. Per-failure remediation: for each missing part, name which node file to revisit

**Gate 2C — simulation check file:**
- [ ] Test input is a real user request (not "given a document...")
- [ ] All nine nodes appear in the expected sequence
- [ ] Missing parts section is present and non-empty (if simulation found gaps)
- [ ] Per-failure remediation maps each gap to a specific node file

---

## SESSION GATE (run before Stop here)

**Gate 2A:** Per-node files — ≥ 8 pairs each, no banned phrases, array nodes correct, arbiter separate
**Gate 2B:** Index file — source, versions, creation session, pair counts
**Gate 2C:** Simulation check — real input, full node sequence, gap list with remediation

**Gate 2D — package completeness:**
- [ ] `00-INDEX.md` present
- [ ] `01-UML-FRAME.md` present
- [ ] `node-01-domain-splitter-set.md` present
- [ ] `node-02-domain-translator-set.md` present
- [ ] `node-03-scheme-planner-set.md` present
- [ ] `node-04-domain-processor-set.md` present
- [ ] `node-05-applier-set.md` present
- [ ] `node-06-result-translator-set.md` present
- [ ] `node-07-arbiter-selector-set.md` present
- [ ] `node-08-arbiter-units-set.md` present
- [ ] `node-09-formatter-set.md` present
- [ ] `10-SIMULATION-CHECK.md` present

**Gate 2E — banned phrase scan:**
Run this check against every node file before marking complete:
Scan each `node-0N-*.md` for: "a clear work statement", "translate it", "plan it",
"produce a result that preserves", "a translated version", "a processed requirement"
Expected: 0 hits across all nine files.

**Gate 2F — one-document boundary:**
- [ ] No pair contains text from a file other than the selected source document
- [ ] No pair references another flow document's content as if it were the source

---

## STATE UPDATE

Save to `ORCHESTRATION-PROTOCOL-STATE.json`:
```json
{
  "current_session": 2,
  "status": "complete",
  "documents_complete": ["[path to document just completed]"],
  "pair_counts_by_node": {
    "node-01": [number],
    "node-02": [number],
    "node-03": [number],
    "node-04": [number],
    "node-05": [number],
    "node-06": [number],
    "node-07": [number],
    "node-08": [number],
    "node-09": [number]
  },
  "simulation_gaps_found": [number],
  "next_document": "[path to next queued document or QUEUE_EMPTY]"
}
```

## ⛔ STOP HERE
Report: document completed, pair counts per node, simulation gaps found, next queued document.
Do NOT begin the next document without explicit approval.

---

## CORRECTION PROTOCOL

If user says the output is wrong:

```
CORRECTION PROTOCOL:
1. STOP producing artifacts immediately.
2. Identify which Gate (2A/2B/2C/2D/2E/2F) the output violated.
3. State the violated gate explicitly: "Gate 2A failed: banned phrase found in node-06 B column."
4. Produce one corrected pair as example.
5. Wait for confirmation that the corrected pair is acceptable.
6. Only then rewrite the affected node file and re-run the gate.
7. Do NOT patch prose around the problem — fix the pair that violated the gate.
```

If the correction changes the method (not just one pair):
1. STOP the batch.
2. Update STATE.json `status` = "METHOD_CORRECTION".
3. Identify which constraint from the inline constraints block was violated.
4. Produce one corrected example pair that satisfies the constraint.
5. Wait for approval before resuming.

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
