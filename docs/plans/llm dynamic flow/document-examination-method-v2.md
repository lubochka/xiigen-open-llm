# Document Examination to Training Sets — Method v2

## What This Document Is

This is the method for turning one gathered document into proper per-node
training sets for the dynamic-flow system.

Version 1 of this method produced a source-line ledger — one row per source
line, each row labelled with a node assignment. That ledger was useful as an
audit trail but it was not training data. The B column in every row described
what the output should look like without ever showing what the output actually
looks like. A node trained on that material would learn to describe its own job,
not to do it.

This version corrects that. The output of each examination round is a set of
A-to-B training pairs. A is exactly what arrives at the node at runtime. B is
exactly what the node must produce. The reason and the rephrasing rules fill the
pairs out so the node can recognize the same situation from multiple angles.

---

## The Pipeline the Training Sets Must Serve

Every document examined under this method feeds the same pipeline. The pipeline
has nine active nodes. Four of them are arrays — the same function applied
independently to each element in a collection, all at the same time.

The nine nodes in execution order:

**Node 1 — Domain splitter.**
Receives a user prompt. Identifies which functional territories appear in it.
Emits an array of domain labels. One prompt can touch several domains. The output
is always an array, never a single label.

**Node 2 — Domain translator (one unit per domain).**
Receives one prompt fragment paired with one domain label. Translates the fragment
into the formal, precise language of that domain — the vocabulary of someone who
works in that domain and needs no further clarification. Does not add concepts that
are not in the fragment. Does not resolve ambiguity by guessing — surfaces it as an
open question. This is an array: one translator unit fires per domain.

**Node 3 — Scheme planner.**
Receives all translated domain descriptions together. Produces an ordered set of
sub-tasks, where each sub-task is paired with a transformational prompt telling the
next stage how to approach it. The plan must include ordered steps, parallel steps
where allowed, wait states, recovery paths, and review points. None of these can be
hidden or collapsed into a single happy path.

**Node 4 — Domain processor (one unit per sub-task).**
Receives one sub-task plus its domain context. Produces the fully processed result
for that sub-task — all success states, all failure states, all wait-state outcomes.
The output must contain enough information for the next stage to act without
re-reading the original input. This is an array: one processor unit fires per
sub-task.

**Node 5 — Applier (one unit per sub-task).**
Receives one sub-task paired with its transformational prompt, plus the processed
result from Node 4. Produces the concrete action taken and its outcome. When
the source document defines record creation followed by event publication, the
pair must show the record written before the event — the write succeeds first,
and only then is the event published. This is an array: one applier unit fires
per sub-task.

**Node 6 — Result translator (one unit per result).**
Receives one formal result. Also sees the full collection of all results for
consistency checking. Translates the formal result into natural language the user
can read and act on. Internal system details stay internal — only what the user
needs to know appears in the translation. If two results from the same flow run
contradict each other, the inconsistency is flagged rather than translated. This
is an array: one translator unit fires per result.

**Node 7 — Arbiter selector.**
Receives the full collection of formal results (R1...Rn) plus the context of the
flow run. Chooses which review units need to fire on these results. This node
operates alongside the result-translation stage, not after it — it sees the formal
results before they are translated into natural language, so its choices are based
on what the system actually produced, not on the user-facing wording. It is
separate from the result translator. Both complete before the formatter runs.

**Node 8 — Arbiter units.**
Each arbiter unit receives one review responsibility and one result or result group.
It produces one of three outputs: pass (no explanation needed), a concern with the
specific rule violated, or a reprocess instruction naming the fragment to send back
and what to fix. Arbiters may send fragments back to the domain splitter to be
reprocessed. The formatter receives only what passes arbitration.

**Node 9 — Formatter.**
Receives all approved translated results. Produces one combined, user-readable
output shaped for the appropriate audience. The same set of approved results can
be formatted differently for a user, a product manager, a security reviewer, or a
devops engineer — the underlying facts do not change, the shape of the output does.
The formatter does not receive raw results, unchecked results, or results that
failed arbitration.

---

## What A and B Mean

Every training pair in every node file is an A-to-B pair.

A is the exact input the node receives at runtime, in the form it actually arrives.
It is not a description of the input. It is not a label for the input. It is the
actual content — the user's words, or the domain fragment, or the translated
description, or the processed result — written out in full.

B is the exact output the node must produce, in the form it actually leaves.
It is not a description of the output. It is not a label for the output category.
It is the actual content — the domain labels, or the formal translation, or the
plan with sub-tasks, or the concrete action and outcome — written out in full.

A node that receives a training pair where B says "a clear work statement in the
language of that domain" learns nothing about what a clear work statement looks
like. A node that receives a training pair where B actually shows the work
statement — the full formal text that a domain specialist would write — can learn
to produce one.

---

## The Rephrasing Rule

Every training pair must appear at least twice: once in its original phrasing, and
once rephrased. The same A expressed in different words. The same B expressed in
different words. The same situation from a different angle.

The purpose of rephrasing is not variety for its own sake. It is so the node
recognizes the same situation when it arrives wearing a different costume. Without
rephrasing, a node trained on "sign up with Google" will not recognize "I want to
use my Google account to register." These are the same A. The node needs to have
seen both.

A rephrasing is not a shorter version. It is not a summary. It covers the same
content at the same depth from a different starting point or with different word
choices.

---

## The Source Document Rule

Each examination round works from exactly one source document.

Related documents — older versions, session files, deep research expansions,
reference plans, implementation notes — are found and recorded, but their content
does not enter the training pairs for this round. They are listed as related
material. They may be the source document for a future round.

The reason for the one-document rule is not speed. It is so the training set
can be verified: every pair in the set should be traceable to the one source
document. If a pair cannot be traced, it does not belong in this round's set.

---

## What to Do Before Writing Any Pairs

### Step 1 — Name the one source document

Record the full path, the file name, the line count, the byte size, the first
meaningful line, and the last meaningful line. State why this document was chosen.

Do not begin with five related documents and call them one.

### Step 2 — Find all versions

Search the root folder and its subfolders — including `old`, `Older`, `history`,
`session_files`, and `.claude\worktrees` — for copies of the same document.

For each version found, record the path, size, line count, and last modified time.
Then compare versions and list the exact lines that changed between them.

Record the version history honestly. If two files are duplicates, say they are
duplicates. If they differ, say what differs.

### Step 3 — Find session or history evidence

Search for the document being requested or produced in session files. Look for the
document's name, its title, and distinctive phrases from its content.

Classify what is found as one of:

- Creation evidence: the session clearly shows the document being asked for
  and produced.
- Candidate history evidence: the session content is clearly related but the
  connection to the final document cannot be confirmed.
- Later use: the document is referenced in a session after it existed.
- Unrelated mention: the match is coincidental.

If no creation session is found, say so directly. Do not guess.

### Step 4 — Read the UML as the mapping frame

Extract every node and every edge from the pipeline diagram. Record them
completely. Do not collapse visual support nodes without saying you did and why.

An edge is not just a line between two boxes. An edge answers four questions:
what moves from the source node, what the target node receives, what the target
node must produce, and what breaks if that edge is missing. Record all four for
every edge.

### Step 5 — Read the source document completely

Read the entire document before deriving runtime situations or writing any
training pairs. The document has structure. A sentence near the beginning sets
context that a sentence near the end depends on. Pairs derived without reading
the whole document tend to miss that context and misrepresent what the node
should produce.

After reading completely, identify the domains the document defines. Not every
document uses the same domain vocabulary. Name the domains this document teaches
and record them before writing any pairs.

---

## How to Write the Training Pairs

### For the domain splitter

A is a user-prompt-shaped statement — the kind of thing a human would write when
asking the system to do something related to this document's subject. Not a line
from the document itself. A prompt a real person might type.

B is the array of domain labels that prompt touches. Not the description of a
domain. The names of the domains.

Write pairs for each domain individually (a prompt that touches only one domain),
then write pairs for combinations (a prompt that touches several domains). Include
at least one pair per domain and at least three multi-domain pairs. Each pair
appears with its rephrasing.

### For the domain translator

A is one prompt fragment paired with one domain label. The fragment is a portion
of a user prompt — or a full short prompt — that has been assigned to that domain.
The domain label follows it so the translator knows which formal language to use.

B is the full formal translation of that fragment into the precise vocabulary of
that domain. Not the description of what a formal translation would contain. The
actual text a domain specialist would write, naming specific rules, specific
values, specific sequences — everything the fragment implies when understood
through that domain's lens.

Write one pair per domain the document defines. Each pair with its rephrasing.

### For the scheme planner

A is all the domain translations for a given flow path together. Name the path
(the SSO path, the email path, the error path, etc.) so it is clear what set
of translations the planner receives.

B is the actual plan: the list of sub-tasks, each labelled, each paired with its
transformational prompt. The transformational prompt is a specific instruction
to the applier about how to approach that sub-task — which rule to apply, which
sequence to follow, which branch condition to check. It is not "apply the right
pattern." It names the pattern and its parameters.

Write one pair per distinct flow path the document defines. Include error and
edge-case paths — they are decomposed alongside the happy path, not treated as
afterthoughts. Each pair with its rephrasing.

### For the domain processor

A is one sub-task labelled by name (b1, b2, b_merge, b_rate, etc.) paired with
a short description of what that sub-task requires.

B is the full set of outcomes for that sub-task: the success output, every
distinct failure output, every wait-state outcome. Each outcome is expressed as a
concrete result — specific fields, specific values, specific states — not as a
description of what an outcome might contain. The output must contain enough
information for the next stage to act without re-reading the original input.

The idempotency requirement belongs in the domain processor pairs. If the same
sub-task arrives twice, the output must be identical to the first execution.

Write one pair per sub-task the scheme planner produces. Each pair with its
rephrasing.

### For the applier

A is one sub-task paired with its transformational prompt, plus the processed
result from the domain processor.

B is the concrete action taken and its observable outcome: what was written,
what was published, what was returned to the caller, in what order. The
write-before-publish rule is a B-level fact: the record is written before the
event is published. This appears explicitly in the pairs where it applies.

Write one pair per sub-task, including at least one failure case that shows
what happens when the action cannot complete. Each pair with its rephrasing.

### For the result translator

A is one formal result expressed in system language (specific fields, specific
values, specific event names). The pair also notes that the translator sees the
full result collection for consistency — this context shapes what the translator
may flag.

B is the natural-language version of that result: what the user needs to know,
expressed in words the user would recognize, with internal system details left
out. The translator does not mention event names, queue names, table names, or
HTTP status codes unless the user explicitly needs to see them.

Include at least one consistency-check pair: a case where two results from the
same flow run contradict each other. The B for that pair is not a translation —
it is a flag noting the contradiction and a refusal to translate until the
contradiction is resolved.

Write one pair per distinct result the applier array produces. Include graceful
failure translations — results that describe a failure the user should not be
exposed to have their own translation rule. Each pair with its rephrasing.

### For the arbiter selector

A is the full collection of results from a given flow run, plus the context
(which path was taken, what kinds of decisions were made, what kinds of data
were handled).

B is the list of arbiter units to fire. Not a description of what kind of
arbiters might be relevant. The names of the arbiters chosen and one sentence
per arbiter saying why it was chosen for this result collection.

Write one pair per meaningfully distinct flow run context that the document
defines. Include cases where different paths produce different arbiter choices.
Each pair with its rephrasing.

### For the arbiter units

A is one result or result group paired with the arbiter's specialization.

B is one of three things: pass (with no further explanation — a passing result
needs no commentary), a concern with the exact rule violated and a reference to
where the document states that rule, or a reprocess instruction naming the exact
fragment to send back and what must change.

Write at least one pass pair. Write at least one flag pair per rule the document
states explicitly. Write at least one reprocess pair showing a fragment being
sent back. Rejected examples derived from source rules must be marked as derived,
not as recorded observations.

Each pair with its rephrasing.

### For the formatter

A is the approved translated result collection — what the result translator
produced and the arbiters passed. The formatter receives translated natural
language, not formal system output.

B is one combined output shaped for one audience. The same result collection
produces a different B for a user, a product manager, a security reviewer, and
a devops engineer. The facts do not change. The shape — what is foregrounded,
what order information appears in, what level of detail is included — changes
per audience.

Write one pair per distinct audience the document's content implies. Include
a pair that shows how a graceful failure (a failure that was handled and
recovered) is presented to a user without exposing the failure itself. Each
pair with its rephrasing.

---

## How to Organize the Output Files

Create one folder per document examined. Name the folder after the document.
Inside it, create these files:

```
<document-name>-training-sets-v1/
  00-INDEX.md
  01-UML-FRAME.md
  node-01-domain-splitter-set.md
  node-02-domain-translator-set.md
  node-03-scheme-planner-set.md
  node-04-domain-processor-set.md
  node-05-applier-set.md
  node-06-result-translator-set.md
  node-07-arbiter-selector-set.md
  node-08-arbiter-units-set.md
  node-09-formatter-set.md
  10-SIMULATION-CHECK.md
```

The index file (`00-INDEX.md`) records: the source document identity (path, line
count, byte size, first and last meaningful lines), the version history found,
the session evidence classification, and the domain names this document defines.

The UML frame file (`01-UML-FRAME.md`) records the complete edge inventory
produced in Step 4. Every node is listed. Every edge is listed with all four
answers: what moves from the source node, what the target node receives, what
the target node must produce, and what breaks if the edge is missing. This file
is the binding frame for all the node files — if a pair in any node file cannot
be connected to an edge in this inventory, the pair does not belong in this round.

Each node file states at its top which node is being trained, which edge the
pairs prepare, and how many pairs are in the file. Then the pairs. Each pair
follows this structure:

```
Pair [ID] — [optional descriptor]
A: [the exact input, written out fully]
B: [the exact output, written out fully]
Reason: [why B is the correct output for this A — which rule or source passage
         it comes from]

Pair [ID] — Phrasing B
A: [the same situation in different words]
B: [the same output in different words]
```

The simulation check file runs through the flow from start to finish using only
the training pairs produced in this round. It names what each node would emit,
calls out what is missing, and states what future round would address the gap.

---

## The Simulation

After all pairs are written, simulate the flow as if the node units had been
trained only on these pairs and nothing else.

The simulation answers these questions in order:

1. Given a prompt about this document's subject, what domain labels would the
   splitter emit?
2. What formal translations would the translator array produce for each domain?
3. What sub-tasks and transformational prompts would the scheme planner produce?
4. Which sub-tasks would the processor array handle in parallel?
5. What concrete outcomes would the applier array produce?
6. What natural-language fragments would the result translator array produce?
7. Which arbiters would the selector choose?
8. Which results would arbiters pass, and which would they flag or send back?
9. What final combined output would the formatter produce for at least two
   different audiences?
10. What is missing?

The missing-parts section names the gaps honestly. A gap is not a failure of
the examination. A gap that is hidden or papered over is.

---

## Validation Before Closing a Round

Check these before considering the round complete:

Every domain the source document defines has at least one domain-splitter pair
and one domain-translator pair.

Every sub-task the scheme planner produces has a domain-processor pair and an
applier pair.

Every distinct result the applier array produces has a result-translator pair.

Every arbiter the selector chooses has an arbiter-units pair, including at least
one pass case and at least one flag case.

The formatter has at least two audience pairs.

Every pair appears with at least one rephrasing.

No node file contains a B that describes the output instead of showing it.

The array stages — domain translator, domain processor, applier, result
translator — are written as arrays: the pairs reflect one unit receiving one
element, not one large unit receiving all elements at once.

The arbiter selector is a separate node file from the result translator.

The formatter receives only approved translated results, not raw or formal results.

The simulation exists, runs to completion, and names its missing parts.

---

## After One Document

Stop after one document and one simulation. Review the produced set. Confirm it
passes the validation checklist. Only then choose the next source document.

The method works only if it is applied slowly and completely to each document
in turn. Rushing through the corpus produces gaps that will be invisible until
a trained node fails at something it was never taught.
