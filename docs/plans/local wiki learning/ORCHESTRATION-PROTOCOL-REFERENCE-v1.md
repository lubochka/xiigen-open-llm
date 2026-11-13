# REFERENCE DOCUMENT — Do not execute. Read for context only.
# Orchestration Protocol: From User Prompt to Training Sets
# Source: excutor session.txt — good and bad orchestration examples

---

## Consumer

The EXECUTION sessions (SESSION-1, SESSION-2) are consumed by the **orchestrator agent**
producing per-node LLM training sets for the dynamic flow UML.

This REFERENCE document is consumed by the **human reviewer** and the **plan author**
before session files are handed off.

---

## User Goal (verbatim — do not paraphrase)

> "We would like to prepare a plan how to translate all the documents we gathered so far
> into proper batch of learning set. Where each set will feed the proper node in the uml.
> I need you to try to take a document — architecture design, look for the session that
> has created it, and at all the versions of this document. Then go over each line in
> document and session and try to define to witch node it belongs in the flow.
> We will need to see eventually a proper parallel flows that eventually will result the
> same as the plan. You can test yours success by trying to simulate these nodes as umls
> that were trained on your assumed sets. And then see what is missing.
> What not to do: Not to hurry, take as much tokens as needed. Not cut edges.
> Avoid making texts shorter. Not to use technical or code terms. Use machine language.
> Try to do batches of files — do one file as required, properly, no hurries, check yourself.
> f(x) = rephrase of (x) — try to avoid logical examination by just use the result on
> all the station with different phrases."

---

## Plan Produced in the Session (reference — do not execute)

The session produced `document-examination-method-v2.md` after six correction cycles.
The final accepted plan contains:

1. One source document per round — never mix
2. Evidence inventory before any pairs are written
3. UML frame file: every node (name, A-type, B-type) + every edge as directed pair
4. Per-node training sets: runtime A→B pairs with full B output (not described)
5. Rephrased variants (f(x) = rephrase(x))
6. Derived examples marked explicitly
7. Simulation check file
8. Six-point validation gate before marking complete

---

## Failures Observed in the Session (not to repeat)

| # | What happened | Which step it violated |
|---|---|---|
| F1 | Narrowed intake before proving full document-to-UML chain | Evidence inventory: incomplete |
| F2 | Read 5 documents when prompt said one | One-document constraint |
| F3 | Produced plans instead of stopping after first plan for approval | Stop-after-plan gate |
| F4 | Treated the dynamic-flow architecture plan as the object to process, not the map | Consumer/reference misidentification |
| F5 | B column said "a clear work statement in that domain" — description, not output | Runtime pair format |
| F6 | Merged domain processor, applier, result translator into single-unit pairs | Array node constraint |
| F7 | Collapsed arbiter selector into result translator (node-06) | Arbiter placement |
| F8 | Source-line ledger (one row per document line) confused for training data | Ledger vs training data |
| F9 | Continued batch without stopping after each completed document | Stop-before-next gate |
| F10 | Asked user "which document should I pick?" after prompt said to pick one | Self-selection rule |
| F11 | Marked document complete while bad-phrase scan had hits | Validation gate |
| F12 | Negative arbiter examples produced without source anchors | Derived example marking |

---

## The Ledger Is Not Training Data

A source-line ledger (one row per document line, tagged with node name) is an audit trail.
It proves coverage. It is NOT a training set.

A model trained on a ledger row such as:
> `L047 | section: formal architecture | source: «must inherit from MicroserviceBase» | feeds: Domain processor`

...learns to process ledger rows, not runtime user requests.

Training pairs must look like what the node receives at runtime.

---

## Array Nodes — Constraint

Four pipeline nodes are arrays. They run once per input item, not once per pipeline run:

| Node | Runs once per |
|---|---|
| Domain translator | Selected domain |
| Domain processor | Planned sub-task (b1, b2, ..., bn) |
| Domain applier | Sub-task result (R1, ..., Rn) |
| Result translator | Result in R1...Rn |

Training pair for an array node: A is one item, B is one result.
Never: A is all domains at once, B is all translations at once.

---

## Arbiter Placement

```
User prompt
  → Domain splitter
  → Domain translator [array]
  → Scheme planner
  → Domain processor [array]
  → Domain applier [array]
  → Result translator [array]  ←─── alongside this stage:
  → Arbiter selector           ←─── sees R1...Rn (formal), not translated human text
  → Arbiter units              (can send pieces back to domain splitter)
  → Formatter
```

Arbiter selector sees **formal R1...Rn** — not translated fragments.
Formatter receives **only approved** translated results.

---

END OF REFERENCE DOCUMENT
