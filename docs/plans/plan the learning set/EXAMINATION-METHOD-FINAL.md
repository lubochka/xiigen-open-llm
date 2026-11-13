# Document Examination to Training Sets — Final Method

## What This Document Is

This is the complete, self-contained method for turning any document — internal
or external — into proper per-node training sets for the dynamic-flow pipeline.

It supersedes all prior versions of the examination method and the external
sources plan. It incorporates the corrections identified through simulation
checking of the Flow-01 sets produced under the previous method, and the
adaptations required for external open-source sources.

---

## Why the Previous Sets Failed

The Flow-01 training sets produced before this method (the zip format) were
examined against the pipeline. Every failure mode in them is documented here
so it cannot be repeated.

**Failure 1 — A was a source document line, not a runtime input.**
In every node file, A was labelled "Raw document line from the
user-registration architecture document." No node in the pipeline ever receives
a raw document line at runtime. A user types a prompt. That prompt is what
arrives at Node 1. The node was trained on something it will never see.

**Failure 2 — B described the output instead of showing it.**
Node 2 (domain translator): every B said "A clear work statement in the language
of: identity and access." That is a description of a translation. It is not a
translation. Node 3 (scheme planner): every B said "A planned piece with its
place in the provider path." That is a description of a plan. It is not a plan.
Node 6 (result translator) and Node 9 (formatter): same pattern throughout.
A node trained on a B that describes its output learns to describe its job.
It does not learn to do it.

**Failure 3 — No rephrasing pairs existed anywhere.**
Zero rephrasing pairs across all nine node files. The rephrasing rule requires
every pair to appear at least twice from different angles. Without rephrasing, a
node trained on "sign up with Google" does not recognize "I want to use my Google
account to register." These are the same situation. The node needs both.

**Failure 4 — Node 8 (arbiter units) had zero pass cases.**
All seven rows in the arbiter-units file were derived flag cases. The node
needs to learn when to pass as much as when to flag. A node with no pass
training flags everything.

**Failure 5 — The simulation check passed its own wrong criteria.**
The simulation check verified that all 357 source lines were represented. That
is a ledger criterion. The correct criterion is that the node files contain
runtime A-to-B pairs that would produce the correct output if the trained node
received a real user prompt. The simulation check was testing the wrong thing.

---

## The Pipeline

Every document examined under this method feeds the same nine-node pipeline.
Four of the nine nodes are arrays — the same function applied independently to
each element, all at the same time.

**Node 1 — Domain splitter.**
Receives a user prompt. Identifies which functional territories the prompt
touches. Emits an array of domain labels — always an array, never a single label.
One prompt can touch several domains simultaneously.

**Node 2 — Domain translator (array: one unit per domain).**
Receives one prompt fragment paired with one domain label. Translates the fragment
into the precise, formal vocabulary of that domain — the language a specialist in
that domain would use. Does not add concepts absent from the fragment. Does not
resolve ambiguity by assumption — surfaces it as an open question.

**Node 3 — Scheme planner.**
Receives all translated domain descriptions together. Produces an ordered set of
sub-tasks, each paired with a transformational prompt that tells the applier
exactly how to approach that sub-task. The plan must include ordered steps,
parallel steps where allowed, wait states, recovery paths, and review points.
None of these may be hidden or collapsed into a single happy path.

**Node 4 — Domain processor (array: one unit per sub-task).**
Receives one sub-task plus its domain context. Produces the fully processed
result for that sub-task: all success states, all failure states, all wait-state
outcomes. The output must contain enough information for the next stage to act
without re-reading the original input. If the same sub-task arrives twice, the
output must be identical — this is the idempotency requirement.

**Node 5 — Applier (array: one unit per sub-task).**
Receives one sub-task paired with its transformational prompt, plus the processed
result from Node 4. Produces the concrete action taken and its observable outcome:
what was written, what was published, what was returned, in what order. When the
source document defines record creation followed by event publication, the pair
must show the record written before the event is published.

**Node 6 — Result translator (array: one unit per result).**
Receives one formal result. Also sees the full collection of all results for
consistency checking. Translates the formal result into natural language the user
can read and act on. Internal system details — event names, queue names, table
names, HTTP status codes — stay internal unless the user explicitly needs them.
If two results from the same flow run contradict each other, the inconsistency is
flagged rather than translated.

**Node 7 — Arbiter selector.**
Receives the full collection of formal results (R1...Rn) plus the flow context.
Operates alongside the result-translation stage — it sees the formal results
before they are translated into natural language, so its choices are based on
what the system actually produced, not on user-facing wording. Chooses which
review units must fire before formatting. Separate node file from Node 6.
Both complete before the formatter runs.

**Node 8 — Arbiter units.**
Each unit receives one review responsibility plus one result or result group.
Produces one of exactly three outputs: pass (no explanation needed — silence is
the pass signal), a concern naming the specific rule violated and where the
source states that rule, or a reprocess instruction naming the exact fragment
to send back and what must change. Arbiters may send fragments back to Node 1
for reprocessing. The formatter receives only what passes arbitration.

**Node 9 — Formatter.**
Receives all approved translated results. Produces one combined, user-readable
output shaped for a specific audience. The same approved results produce
different output for a user, a product manager, a security reviewer, and a
devops engineer — the facts do not change, the shape does. The formatter never
receives raw results, unchecked results, or results that failed arbitration.

---

## What A and B Mean

Every training pair in every node file is an A-to-B pair.

A is the exact input the node receives at runtime, written out in full. Not a
label for the input. Not a description of the input category. The actual content:
a user prompt, or a domain fragment, or a translated description, or a processed
result — whatever actually arrives at that node during a live run.

B is the exact output the node produces, written out in full. Not a description
of what the output should contain. Not a label for the output type. The actual
content: the domain labels themselves, or the full formal translation text, or the
labelled sub-task list with transformational prompts, or the concrete action and
its outcome.

A node trained on a B that says "a clear work statement in the language of that
domain" learns to produce that description. It does not learn to produce the
actual work statement. The actual work statement must appear as B.

---

## The Rephrasing Rule

Every training pair must appear at least twice: once in its original phrasing and
once rephrased. The same A in different words. The same B in different words. The
same situation from a different angle, at the same depth.

Rephrasing is not summarising. A rephrasing that is shorter than the original has
lost content. A rephrasing that changes the perspective without reducing the
detail is correct.

The reason for rephrasing: a node trained on one phrasing of a situation will not
recognise the same situation when it arrives in different words. "Sign up with
Google" and "use my Google account to register" are the same situation. The node
needs both phrasings before it can generalise.

---

## The One-Document Rule

Each examination round works from exactly one source document.

For internal documents: one file from the gathered corpus.
For external sources: one source unit as defined in the unit boundary table below.

Related documents — older versions, session files, expansions, reference plans —
are found and recorded in the index, but their content does not enter the training
pairs for this round. They may be the source document for a future round.

Every pair in the set must be traceable to the one source document. A pair that
cannot be traced does not belong in this round.

---

## Before Writing Any Pairs — Internal Documents

### Step 1 — Name the source document

Record: full path, file name, line count, byte size, first meaningful line, last
meaningful line. State why this document was chosen.

Do not begin with several related documents grouped under one name.

### Step 2 — Find all versions

Search the root folder and all subfolders — including `old`, `Older`, `history`,
`session_files`, `.claude\worktrees` — for copies of the same document.

For each version found, record: path, size, line count, last modified time.
Compare versions and list the exact lines that differ. State clearly whether two
files are duplicates or whether they differ and how.

### Step 3 — Find session or history evidence

Search session files for the document being requested or produced. Look for the
document name, its title, and distinctive phrases from its content.

Classify what is found as one of:
- Creation evidence — the session clearly shows the document being produced.
- Candidate history evidence — clearly related but the connection cannot be confirmed.
- Later use — the document is referenced in a session after it existed.
- Unrelated mention — the match is coincidental.

If no creation session is found, state that directly. Do not guess.

### Step 4 — Read the pipeline diagram as the mapping frame

Extract every node and every edge from the pipeline diagram. Record them
completely. Do not collapse any node without stating which node was collapsed
and why.

An edge answers four questions: what moves from the source node, what the target
node receives, what the target node must produce, and what breaks if this edge is
missing. Record all four for every edge. This inventory goes into `01-UML-FRAME.md`
and is the binding frame for all node files — a pair that cannot be connected to
an edge in that inventory does not belong in this round.

### Step 5 — Read the source document completely

Read the entire document before deriving any runtime situations or writing any
pairs. A sentence near the beginning sets context that a sentence near the end
depends on. Pairs derived without reading the whole document miss that context.

After reading, identify the domains this document defines. Name them explicitly
before writing any pairs.

---

## Before Writing Any Pairs — External Sources

External sources require three additional steps and two step replacements.

**License gate (runs before anything else).** Confirm the exact license of the
specific source unit — not the repository, the unit. License may vary within a
repository.

Usable without restriction: MIT, BSD, Apache 2.0, public domain, CC-BY (any
version), CC-BY-SA (any version). CC-BY-SA pairs must carry attribution: record
the canonical URL and license name in every pair's source field.

Not usable: CC-BY-NC-ND, CC-BY-NC, proprietary, all rights reserved. If the
license is not usable, record the unit as `license_blocked` in the coverage
ledger and stop. Write no pairs.

If the license cannot be confirmed, record as `license_unknown` and write no
pairs until it is confirmed.

**Source provenance record (replaces session evidence search).** External sources
have no session history. Record instead: canonical URL, fetch date, and the
revision history the source unit itself states. For an RFC: its status dates.
For a PEP: the header fields and status. For an AOSA chapter: the edition and
author. If the unit contains no revision history, state that directly.

**Fetch before examining (separate step).** Fetch the source unit before the
examination round begins. Do not fetch during examination.

```bash
# Kubernetes KEPs — sparse clone to get only the keps directory
git clone --depth=1 --filter=blob:none --sparse \
  https://github.com/kubernetes/enhancements keps-local
cd keps-local && git sparse-checkout set keps

# OpenTelemetry Specification — sparse clone specification directory
git clone --depth=1 --filter=blob:none --sparse \
  https://github.com/open-telemetry/opentelemetry-specification otel-local
cd otel-local && git sparse-checkout set specification

# OpenAPI Specification — sparse clone versions directory
git clone --depth=1 --filter=blob:none --sparse \
  https://github.com/OAI/OpenAPI-Specification oas-local
cd oas-local && git sparse-checkout set versions

# Google AIP — sparse clone aip directory
git clone --depth=1 --filter=blob:none --sparse \
  https://github.com/aip-dev/google.aip.dev aip-local
cd aip-local && git sparse-checkout set aip

# OWASP ASVS — clone full repo, CSV export is in the root
git clone --depth=1 https://github.com/OWASP/ASVS asvs-local

# OWASP Cheat Sheet Series — sparse clone cheatsheets directory
git clone --depth=1 --filter=blob:none --sparse \
  https://github.com/OWASP/CheatSheetSeries cheatsheets-local
cd cheatsheets-local && git sparse-checkout set cheatsheets

# NIST SSDF — fetch the HTML version directly
curl -s "https://csrc.nist.gov/publications/detail/sp/800-218/final" \
  -o nist-ssdf.html

# CWE Top 25 — fetch the current list page
curl -s "https://cwe.mitre.org/top25/archive/2023/2023_top25_list.html" \
  -o cwe-top25.html

# IETF RFCs — fetch specific RFCs as text
curl -s "https://www.rfc-editor.org/rfc/rfc7519.txt" -o rfc7519-jwt.txt
curl -s "https://www.rfc-editor.org/rfc/rfc6749.txt" -o rfc6749-oauth2.txt
curl -s "https://www.rfc-editor.org/rfc/rfc8446.txt" -o rfc8446-tls13.txt

# SWE-bench — clone the dataset repo
git clone --depth=1 https://github.com/swe-bench/SWE-bench swebench-local

# Microsoft Learn architecture pages — fetch individual pages
# Confirm CC-BY 4.0 footer before using any page
curl -s "https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs" \
  -o ms-cqrs.html
# Check footer: look for "Creative Commons Attribution 4.0" before writing pairs
```

Record the fetch date. External source content changes over time. The fetch date
is part of the pair's source identity.

**Steps 4 and 5 are the same as for internal documents,** with one note: the
`01-UML-FRAME.md` for external sources is shared across all source units in the
same corpus run. The pipeline does not change between an RFC round and a PEP
round. Confirm the frame is current and reference it. Do not rewrite it per unit.

---

## Source Unit Boundaries for External Sources

One examination round works from exactly one source unit. The unit is the
smallest item that contains a complete reasoning chain.

| Source group | One unit is |
|---|---|
| Kubernetes KEPs | One KEP directory — all files within one numbered KEP folder |
| OpenTelemetry Specification | One specification file or one named section within a specification file |
| OpenAPI Specification | One versioned specification file |
| Google API Improvement Proposals | One AIP page — one numbered AIP document |
| Microsoft Learn architecture | One named article page — confirm CC-BY 4.0 notice present before starting |
| OWASP ASVS | One numbered requirement group — all requirements within that group (e.g., section 2.1 Passwords) |
| OWASP Cheat Sheet Series | One cheat sheet — one named markdown file |
| NIST SSDF | One practice — one named practice section with its tasks and examples |
| CWE Top 25 | One weakness entry — one CWE ID with its description, consequences, and mitigations |
| IETF RFCs | One complete RFC document |
| SWE-bench | One task instance — one issue description with its verified patch and test outcome |
| Defects4J / BugsInPy / QuixBugs | One bug entry — only after license confirmed |
| Semgrep rules / CodeQL examples | One rule file — only after license confirmed |
| CNCF Apache 2.0 project docs | One design document or enhancement proposal — only after per-repo license confirmed |

Do not split a unit across rounds. Do not merge two units into one round.

---

## Source Registry

### Hard rule

No source enters the training set unless its LICENSE file or official terms
explicitly allow reuse, modification, redistribution, or commercial
research and development use. A public GitHub repository with no LICENSE file
is not open-source — GitHub itself states that a repository requires a license
for others to be free to use, change, and distribute it. Do not treat "publicly
visible" as "freely usable."

Sources are listed below as one of three use classes:

- **TRAINING OK** — license confirmed; raw text may be used for derived pairs.
- **ATTRIBUTION REQUIRED** — usable, but every pair produced from this source
  must carry the source URL and license name in its source field.
- **VERIFY BEFORE USE** — license not confirmed at time of writing; do not write
  any pairs until the repo's LICENSE file is read and its terms confirmed.

Sources removed from the previous version of this document — TypeScript issue
comments, CloudEvents issue comments, AOSA chapters, Rust RFCs, Python PEPs,
Go Proposals, Kafka KIPs, and System Design Primer — were either using GitHub
issue content whose license is ambiguous (issue comments are not covered by the
repo's code license), or were lower priority given the sources now included.
They are not forbidden; they simply require the same license check before use.

---

### Group 1 — Architecture and orchestration

| Source ID | Source | Fetch location | License | Use class | Primary nodes |
|---|---|---|---|---|---|
| `KUBERNETES-KEPS` | Kubernetes Enhancement Proposals | `https://github.com/kubernetes/enhancements` — clone `keps/` directory | Apache 2.0 | TRAINING OK | 3, 7, 8 |
| `OPENTELEMETRY-SPEC` | OpenTelemetry Specification | `https://github.com/open-telemetry/opentelemetry-specification` — clone `specification/` | Apache 2.0 | TRAINING OK | 2, 3, 8 |
| `OPENAPI-SPEC` | OpenAPI Specification | `https://github.com/OAI/OpenAPI-Specification` — clone `versions/` | Apache 2.0 | TRAINING OK | 1, 3, 8 |
| `GOOGLE-AIP` | Google API Improvement Proposals | `https://github.com/aip-dev/google.aip.dev` — clone `aip/` directory | CC-BY 4.0 (content); Apache 2.0 (code samples) | ATTRIBUTION REQUIRED | 1, 3, 7, 8 |
| `MICROSOFT-LEARN-ARCH` | Microsoft Learn architecture docs | `https://learn.microsoft.com/en-us/azure/architecture/` — fetch individual pages where CC-BY 4.0 notice is present | CC-BY 4.0 where stated — confirm per page | ATTRIBUTION REQUIRED — confirm per page | 2, 3, 4 |

**Notes for Group 1:**

Kubernetes KEPs and OpenTelemetry Spec are both Apache 2.0 with confirmed
LICENSE files. Raw text may be used for derived pairs; attribution is recommended.

Google AIP content is CC-BY 4.0. Every pair from this source must record the
canonical AIP URL and "CC-BY 4.0" in the source field. Code samples within AIPs
are Apache 2.0 and may be used without the CC-BY attribution requirement.

Microsoft Learn architecture pages carry a CC-BY 4.0 notice at the page footer
where it applies — but not all pages carry it. Before writing any pair from a
Microsoft Learn page, confirm the footer shows "Content licensed under Creative
Commons Attribution 4.0." If the footer is absent or shows different terms, mark
the unit `license_unknown` and write no pairs.

---

### Group 2 — Security and governance

| Source ID | Source | Fetch location | License | Use class | Primary nodes |
|---|---|---|---|---|---|
| `OWASP-ASVS` | OWASP Application Security Verification Standard | `https://github.com/OWASP/ASVS` — clone, read CSV export | CC-BY-SA 4.0 | ATTRIBUTION REQUIRED (ShareAlike applies) | 7, 8 |
| `OWASP-CHEAT-SHEETS` | OWASP Cheat Sheet Series | `https://github.com/OWASP/CheatSheetSeries` — clone `cheatsheets/` | CC-BY-SA 4.0 — verify repo LICENSE file before each unit | ATTRIBUTION REQUIRED (ShareAlike applies) | 8 |
| `NIST-SSDF` | NIST Secure Software Development Framework | `https://csrc.nist.gov/publications/detail/sp/800-218/final` — fetch PDF or HTML | Not subject to US copyright (NIST government work) | TRAINING OK — attribution appreciated | 7, 8 |
| `CWE-TOP25` | MITRE CWE Top 25 | `https://cwe.mitre.org/top25/` — fetch HTML | MITRE terms: free for research, development, and commercial use with required notice | ATTRIBUTION REQUIRED — include MITRE notice | 8 |
| `IETF-RFCS` | Selected IETF RFCs | `https://www.rfc-editor.org/rfc/` — fetch RFC 7519 (JWT), 6749 (OAuth 2.0), 8446 (TLS 1.3), 9110 (HTTP), 7617 (Basic Auth) | IETF Trust Legal Provisions — educational use permitted; commercial training use is not explicitly addressed | ATTRIBUTION REQUIRED — confirm use case with TLP before promoting pairs commercially | 2, 8 |

**Notes for Group 2:**

OWASP ASVS and OWASP Cheat Sheets are CC-BY-SA 4.0. ShareAlike means any
derived work that incorporates the text must carry the same CC-BY-SA license.
Training pairs that paraphrase or summarise OWASP content rather than copying
it verbatim are derived works and should carry attribution; whether ShareAlike
extends to trained model weights is a legal question that is not resolved. Record
the source URL on every pair and note the CC-BY-SA status so a legal review can
be applied before commercial deployment.

NIST SSDF is produced by US government employees and is not subject to US
copyright law. It may be used freely. Attribution is recommended practice.

CWE Top 25: MITRE's terms allow research, development, and commercial use but
require the notice "CWE™" and the MITRE copyright statement. Include this notice
in the source field of every pair produced from CWE content.

IETF RFCs: The Trust Legal Provisions explicitly permit reproduction for
implementing or describing a standard. Using RFC prose to produce training pairs
for a system that implements those protocols falls within that scope. If the
training corpus will be used commercially, obtain a legal review of the TLP
before promoting IETF-derived pairs to production.

---

### Group 3 — Code agent and implementation datasets

| Source ID | Source | Fetch location | License | Use class | Primary nodes |
|---|---|---|---|---|---|
| `SWE-BENCH` | SWE-bench | `https://github.com/swe-bench/SWE-bench` | MIT | TRAINING OK | 3, 4, 5, 8 |
| `DEFECTS4J` | Defects4J | `https://github.com/rjust/defects4j` | Verify repo LICENSE before use | VERIFY BEFORE USE | 4, 5, 8 |
| `BUGSINPY` | BugsInPy | `https://github.com/soarsmu/BugsInPy` | Verify repo LICENSE before use | VERIFY BEFORE USE | 4, 5, 8 |
| `QUIXBUGS` | QuixBugs | `https://github.com/jkoppel/quixbugs` | Verify repo LICENSE before use | VERIFY BEFORE USE | 4, 5, 8 |
| `SEMGREP-RULES` | Semgrep rules registry | `https://github.com/returntocorp/semgrep-rules` | Verify repo LICENSE before use | VERIFY BEFORE USE | 8 |
| `CODEQL-EXAMPLES` | CodeQL query examples | `https://github.com/github/codeql` | Verify repo LICENSE before use | VERIFY BEFORE USE | 8 |

**Notes for Group 3:**

SWE-bench is MIT licensed and confirmed. It provides issue descriptions, repository
state, and verified patches — directly usable for domain-processor and applier
pairs showing a problem received, a solution applied, and an outcome verified.

All other sources in this group are marked VERIFY BEFORE USE. Before writing any
pair from these sources, open the repository's LICENSE file, read the terms, and
confirm they explicitly permit the intended use. If the LICENSE file is absent,
treat the source as `license_blocked`. Record the confirmed license in the coverage
ledger entry for that source unit.

---

### Group 4 — CNCF project design documents

| Source ID | Source | Fetch location | License | Use class | Primary nodes |
|---|---|---|---|---|---|
| `CNCF-APACHE2-REPOS` | CNCF projects with confirmed Apache 2.0 | Check https://github.com/cncf/landscape for project list; verify each repo's LICENSE file | Apache 2.0 — must confirm per repo | TRAINING OK after per-repo confirmation | 2, 3, 7, 8 |

**Note:** The CNCF landscape includes projects under various licenses. Only use a
CNCF project's design documents, enhancement proposals, or specification files
after confirming that specific repository's LICENSE file states Apache 2.0 (or
another confirmed-usable license). Do not assume CNCF membership implies Apache 2.0.

---

### Removed from previous version and why

The following sources appeared in earlier drafts of this document and have been
removed. They are not forbidden, but they require the same license check before
use and were lower priority than the sources above.

| Source | Reason removed |
|---|---|
| Rust RFCs (MIT) | Lower priority given new orchestration sources; may be readded after license recheck |
| Python PEPs (public domain) | Same — may be readded |
| Go Proposals (BSD) | Same |
| Kafka KIPs (Apache 2.0) | Same |
| TypeScript issue comments | GitHub issue comments are not covered by the repo's code license; license ambiguous |
| CloudEvents spec issues | Same — issue comments, not spec text |
| AOSA volumes (CC-BY 3.0) | May be readded; CC-BY 3.0 is permissive |
| 500 Lines or Less (CC-BY 3.0) | Same |
| System Design Primer (CC-BY 4.0) | Same |
| Company engineering blogs | Not open-source; cite only, no copying |
| GitHub issues and PRs without confirmed license | License ambiguous; require explicit check |

---

## Pair Format by Source Group

The A-to-B pair structure is the same for every source. What changes is
what A looks like and what B looks like, because different source groups
contain different kinds of reasoning material.

### Format for proposal and debate archives (RFCs, PEPs, KIPs, Go Proposals)

These sources contain a problem statement, proposed solution, community
debate, and a verdict. Each of those parts maps to a different node.

**Domain-splitter pairs from a proposal:**

```
Pair [source-id]-S1 — problem statement
Source: https://github.com/rust-lang/rfcs/blob/master/text/2229-capture-disjoint-fields.md
License: MIT
A: I need closures in my code to capture only the specific fields they
   actually use, not the entire struct they access — so two closures that
   each use a different field of the same struct can run at the same time.
B: [concurrent execution], [ownership and borrowing], [closure behaviour]
Reason: The RFC's motivation section frames this as a problem where the
   borrow checker rejects valid concurrent code. Three domains: how
   concurrent tasks share data, how Rust tracks who owns what, how closures
   decide what to capture.

Pair [source-id]-S1 — Phrasing B
A: Two separate closures both reference the same struct but touch different
   fields. The compiler refuses because it treats the whole struct as captured.
   I need it to see them as independent.
B: [concurrent execution], [ownership and borrowing], [closure behaviour]
```

**Scheme-planner pairs from the accepted design section:**

```
Pair [source-id]-P1 — accepted decomposition
Source: https://github.com/rust-lang/rfcs/blob/master/text/2229-capture-disjoint-fields.md
License: MIT
A: [ownership and borrowing translated]: the borrow checker must track
   captures at field granularity rather than at variable granularity, while
   remaining backward-compatible with all existing code that does not use
   disjoint field access.
   [closure behaviour translated]: the compiler must infer the minimal
   capture set — only the paths actually used, not the parent binding.
   [concurrent execution translated]: two closures that access disjoint
   fields of the same struct must be allowed to run in parallel when neither
   capture requires exclusive access to the whole struct.
B: Sub-tasks and transformational prompts:
   b1: Analyse which fields each closure path actually reads or writes.
       fb1: Walk the closure body. Record every field path accessed. Do not
       record the parent struct — record the exact path (struct.field,
       struct.field.subfield). Stop at the first level that determines
       exclusivity.
   b2: Determine the minimal capture mode for each recorded path (by value,
       by reference, by mutable reference).
       fb2: Apply the existing capture-mode inference rules to each path
       independently. Do not promote a field path to a whole-variable capture.
   b3: Check whether any two closures share a path with incompatible modes.
       fb3: If no paths overlap, or all overlapping paths are shared-reference:
       allow parallel execution. If any path overlap is mutable: block.
   b4: Emit a migration lint for code where the new inference changes the
       capture set compared with the old behaviour.
       fb4: Compare old capture set (whole variable) with new capture set
       (field paths). Where they differ and existing code depended on the
       old behaviour, emit a lint with an automatic migration suggestion.
Reason: RFC 2229 accepted design section, subsections on "How capture analysis
   works" and "Migration path."

Pair [source-id]-P1 — Phrasing B
A: The borrow checker needs to see closures as borrowing specific fields, not
   whole structs, so that parallel closures on different fields of the same
   struct become legal — without breaking existing code that relied on whole-
   variable capture.
B: Four ordered sub-tasks. First, discover the exact field paths each closure
   touches. Second, assign a borrow mode to each path on its own. Third, check
   whether any two closures conflict on the same path. Fourth, warn about any
   existing code where the new narrower capture changes observable behaviour.
   Each sub-task has a specific stopping condition — do not collapse them.
```

**Arbiter-unit pairs from community objections:**

```
Pair [source-id]-A1 — flag from objection
Source: https://github.com/rust-lang/rfcs/blob/master/text/2229-capture-disjoint-fields.md
License: MIT
A: Result: "The implementation captures struct.field_a and struct.field_b
   separately. A closure that uses struct.field_a.0 is recorded as capturing
   struct.field_a."
   Arbiter specialisation: capture-granularity correctness.
B: Flag.
   Rule: The RFC requires capture at the exact path used, not at the
   parent field. "struct.field_a.0" is the used path; "struct.field_a" is
   the parent. Recording the parent instead of the path violates the
   field-granularity requirement.
   Reprocess: Re-examine the closure body and record struct.field_a.0,
   not struct.field_a. The capture set must contain the deepest path
   actually accessed, not any ancestor of that path.
Reason: RFC 2229 unresolved question on tuple-field captures, and the
   community concern raised in the RFC thread that naive implementations
   over-capture at the field level.

Pair [source-id]-A1 — Phrasing B
A: A closure uses the first element of a tuple stored in a struct field.
   The recorded capture is the whole tuple field, not the element within it.
   Arbiter: capture-granularity checker.
B: Flag. The capture is recorded at the wrong depth. The path actually
   used ends at the tuple element; the capture should end there too. Send
   the capture analysis back: re-walk the access path and stop at the
   last component actually read or written.
```

### Format for architecture narrated by builders (AOSA, .NET Microservices, System Design Primer)

These sources describe design decisions in terms of constraints that forced
a choice and alternatives that were rejected. The decision and its rationale
are what A and B are built from.

**Domain-translator pairs from a constraint description:**

```
Pair [source-id]-T1 — constraint to formal vocabulary
Source: https://aosabook.org/en/v1/nginx.html
License: CC-BY 3.0
A: "The cost of context switches from one-thread-per-connection was becoming
   prohibitive as connection counts grew into the tens of thousands. Each
   blocked thread waiting on slow I/O was sitting on an 8 MB stack."
   Domain: concurrent execution.
B: The system faces a C10K concurrency problem. Each connection held one
   OS thread. Thread context switching overhead grows O(n) with connection
   count. Stack memory per thread: 8 MB fixed allocation regardless of
   activity. Total memory at 10,000 connections: 80 GB in thread stacks
   alone. Blocking I/O model requires the thread to be descheduled while
   waiting, transferring scheduling control to the OS. The bottleneck is
   not computation — it is scheduling overhead and memory pressure from
   idle threads.
Reason: NGINX chapter, "Why Is High Concurrency Important?" section.
   The constraint is stated in plain language; the formal translation gives
   the precise terms (C10K, O(n) switching, fixed stack allocation) needed
   for a scheme-planner to decompose the problem.

Pair [source-id]-T1 — Phrasing B
A: Having one thread per connection was working fine at a few hundred
   connections but falling apart at ten thousand. The memory and switching
   cost was the problem, not the processing.
   Domain: concurrent execution.
B: Ten-thousand-connection concurrency with a thread-per-connection model
   requires 80 GB of stack memory at 8 MB per thread. Context switching
   at this scale introduces latency from OS scheduling that dwarfs the
   actual I/O wait time. The system is memory-bound and scheduler-bound,
   not CPU-bound. Any solution must decouple connection count from thread
   count and avoid blocking the execution unit while I/O is pending.
```

**Scheme-planner pairs from a decomposition decision:**

```
Pair [source-id]-D1 — rejected alternative as negative example
Source: https://aosabook.org/en/v1/nginx.html
License: CC-BY 3.0
A: [concurrent execution translated]: the system must handle tens of
   thousands of simultaneous connections without assigning one OS thread
   per connection. Stack memory and context-switching overhead are the
   binding constraints.
B (rejected plan — negative example):
   b1: Use a thread pool with a fixed upper bound of 500 threads.
       fb1: Accept incoming connections up to 500. Queue excess.
   b2: Assign each accepted connection to an available thread.
       fb2: Use a work-stealing queue. Block threads on slow connections.
   Why rejected: The thread pool cap creates a hard connection limit.
   Queued connections time out. Slow connections still hold threads and
   block other requests from using those threads. The constraint (decouple
   connection count from thread count) is not satisfied.
Reason: NGINX chapter, discussion of the Apache prefork model and why it
   was not adopted. This is the plan that was tried and rejected before
   the event-loop model was chosen.

Pair [source-id]-D2 — accepted plan
A: [same domain translations as D1]
B (accepted plan):
   b1: For each incoming connection, register a file descriptor with the
       event loop. Do not create a thread.
       fb1: Use epoll (Linux) or kqueue (BSD). Store connection state in
       a small structure on the heap, not on a thread stack.
   b2: When the event loop signals that a file descriptor is ready, execute
       the handler for that connection's current state.
       fb2: Apply state-machine pattern. Each handler runs to completion
       without blocking. If the handler would block (waiting for a backend
       or a disk read), register a new event and yield.
   b3: For operations that cannot be made non-blocking (some disk I/O),
       offload to a small fixed-size worker thread pool.
       fb3: Worker pool size is proportional to disk I/O concurrency, not
       to connection count. Typically 4–8 workers regardless of connection
       load.
Reason: NGINX chapter, "NGINX Architecture" section.
```

### Format for verifiable requirement sets (OWASP ASVS, IETF RFCs)

These sources contain explicit pass/fail conditions. The requirement is A.
Whether a given result satisfies or violates it is B.

**Arbiter-unit pairs from OWASP ASVS:**

```
Pair [source-id]-R1 — pass case
Source: https://github.com/OWASP/ASVS — section 2.1.1
License: CC-BY-SA 4.0
A: Result: "User password is validated against a minimum length of 12
   characters, rejected if found in the top 10,000 common passwords list,
   and hashed with bcrypt at cost factor 12 before storage."
   Arbiter specialisation: authentication credential requirements (ASVS 2.1).
B: Pass.
Reason: ASVS 2.1.1 requires passwords to be at least 8 characters (12
   exceeds this). ASVS 2.1.7 requires checking against known-breached or
   common password lists. ASVS 2.4.1 requires bcrypt, scrypt, or Argon2.
   All three conditions are satisfied.

Pair [source-id]-R1 — Phrasing B
A: The system checks that passwords are long enough, not in a list of
   common choices, and stored in a form that cannot be reversed.
   Arbiter: ASVS authentication section 2.1 requirements.
B: Pass.

Pair [source-id]-R2 — flag case
Source: https://github.com/OWASP/ASVS — section 2.1.7
License: CC-BY-SA 4.0
A: Result: "User password is accepted if it is at least 8 characters.
   No check against common or breached password lists is performed."
   Arbiter specialisation: authentication credential requirements (ASVS 2.1).
B: Flag.
   Rule: ASVS 2.1.7 requires verifying that new passwords are not found in
   known-breached or top-common-password lists. Accepting a password without
   this check allows users to set passwords like "password123" or passwords
   that have appeared in data breaches, making credential-stuffing attacks
   trivial.
   Reprocess: Add a check against a known-password list before accepting
   the credential. The list must include at minimum the OWASP top 10,000
   common passwords. Reject and prompt for a different choice if matched.
Reason: ASVS v5.0 section 2.1.7.

Pair [source-id]-R2 — Phrasing B
A: A password meets the length requirement but was not checked against
   any list of passwords known to be weak or compromised.
   Arbiter: ASVS 2.1 credential strength requirements.
B: Flag. The length check alone does not satisfy ASVS 2.1.7. The system
   must also confirm the password is not in a known-bad list. Send this
   step back: add the list check before the credential is accepted.
```

---

## Good and Bad Pair Examples

These examples show the difference between a pair that will train a node
correctly and one that will not. Every node in the pipeline has a good
and bad example here.

---

### Node 1 — Domain splitter

**Bad pair — A is a source document line, B is a label description:**

```
A: "Handles new user sign-up through multiple SSO providers (Google,
   Facebook, LinkedIn, Figma) or email-based registration."
B: Domain labels covering identity and access topics.
```

Why it fails: A is a line copied from the architecture document. No user
ever sends that line to the system. B describes what the output category
is rather than naming the labels. A node trained on this learns nothing
about what arrives at runtime or what it must produce.

**Good pair — A is a real user prompt, B is the actual labels:**

```
A: "I want users to be able to sign up with their Google account or with
   an email address they choose."
B: [authentication], [communication]
Reason: authentication because two identity-claim paths are described
   (SSO and credentials). Communication because the email path requires
   a verification message to be delivered before the account is active.
```

---

### Node 2 — Domain translator

**Bad pair — B describes a translation rather than showing one:**

```
A: "User signs up with Google." + domain: authentication
B: A clear work statement explaining the authentication steps for Google
   SSO in precise technical language.
```

Why it fails: B tells the node what kind of thing to produce. It does not
show the thing. A node trained on this learns that its output should be
"clear" and "precise." It does not learn what a formal authentication
description actually contains.

**Good pair — B is the actual translation, fully written:**

```
A: "User signs up with Google." + domain: authentication
B: The user presents an authorization code obtained from Google's OAuth 2.0
   authorization endpoint. The system exchanges this code for a Google access
   token at Google's token endpoint. The system requests the user's profile
   from Google's userinfo endpoint using that access token. The returned
   email is normalised to lowercase. The system checks whether this email
   already exists in the user store. If the email is new: create a user
   record with status=active and a permanent userId (UUID v4). Issue platform
   credentials: access token with 15-minute validity, refresh token with
   7-day validity, rotation enforced on each use, both stored in httpOnly
   Secure SameSite=Lax cookie. Publish UserSSOAuthenticated. If the email
   already exists: apply account merge rule — preserve the existing userId,
   link the new provider, keep any existing password.
Reason: authentication domain formal vocabulary for OAuth 2.0 authorization
   code flow, derived from the registration flow document's security section.
```

---

### Node 3 — Scheme planner

**Bad pair — B is a description of a plan's position, not the plan:**

```
A: [authentication and onboarding domain translations for SSO path]
B: A plan with the SSO steps in their correct sequential order, covering
   the provider path and the shared onboarding section.
```

Why it fails: B tells the reader what shape the plan has. It does not
contain the plan. A node trained on this learns to describe a plan's shape.

**Good pair — B is the actual plan with sub-tasks and transformational prompts:**

```
A: [authentication domain translated]: The user presents a Google
   authorization code. Exchange it for a profile. Normalise the email.
   Check uniqueness. Create or merge. Issue JWT in httpOnly cookie. Publish
   UserSSOAuthenticated.
   [onboarding domain translated]: After UserCreated, generate a personalised
   questionnaire. Deliver via in-app chat within 5 seconds. Wait for
   completion. Verify three criteria. Publish UserOnboardingCompleted.
B:
   b1: Exchange the authorization code for the user's Google profile and
       decide: create or merge.
       fb1: Normalise email to lowercase before the uniqueness check.
       Branch before any write. If new: proceed to b2. If existing email
       match: proceed to b_merge.
   b2: Write the new user record and publish the authentication event.
       fb2: Write the user record first. Only after the write succeeds:
       publish UserSSOAuthenticated. Never publish before the write.
   b3: Deliver the initial questionnaire via chat.
       fb3: Consume UserCreated. Generate the questionnaire for this
       userId. Publish QuestionnaireRequired. Confirm delivery within
       5 seconds of account creation. Publish QuestionnaireSent.
   b4: Wait for questionnaire completion and verify all three criteria.
       fb4: Wait for QuestionnaireCompleted. Check: profile created (yes,
       automatic). Questionnaire answered (yes, this event confirms it).
       Business details included (verify in the questionnaire response
       payload). Only when all three confirmed: publish
       UserOnboardingCompleted.
   b_merge: Preserve the existing userId, link the new SSO provider.
       fb_merge: The existing userId is the permanent identifier referenced
       by all other services. Do not create a new record. Add the new
       provider to the existing account's provider list. Keep any existing
       password.
Reason: registration flow document, SSO path and onboarding sections.
```

---

### Node 4 — Domain processor

**Bad pair — B is a description of a processed requirement:**

```
A: b1 (SSO path): validate SSO provider callback and exchange code for user profile.
B: A processed requirement that preserves the source line and can be handled
   by one parallel unit.
```

Why it fails: B says what a processed requirement is supposed to be. It
is not one.

**Good pair — B contains the actual outcomes with specific fields:**

```
A: b1 (SSO path): exchange the Google authorization code for the user's
   profile. Normalise email. Check uniqueness. Branch on result.
B:
   Success (new user):
     { userId: new UUID v4, status: "active", provider: "google",
       email: "normalised@example.com",
       profile: { name: "from Google", picture: "URL from Google" } }
   Success (existing email — merge required):
     { userId: existing UUID, mergeRequired: true,
       existingMethod: "email" | "sso",
       existingProviders: ["list of already linked providers"] }
   Failure (provider timeout):
     { error: "provider_timeout", retryable: true, attemptsMade: 1,
       backoffNextAttemptMs: 1000 }
   Failure (invalid code — single use already consumed):
     { error: "code_already_used", retryable: false }
   Idempotency: if the same authorization code arrives twice, the second
   exchange attempt fails at the provider with code_already_used. The
   processor must return the idempotency result, not create a duplicate.
Reason: registration flow document, SSO validation section and idempotency
   requirements.
```

---

### Node 5 — Applier

**Bad pair — B is a description of a result:**

```
A: (b2_sso, fb2 = "Write the user record first, then publish.") +
   result: { userId: new UUID, status: active }
B: A domain result preserving publisher, consumer, event, service, storage,
   or rule details from the source line.
```

Why it fails: B names the categories of detail it should preserve. It
does not preserve any of them.

**Good pair — B is the concrete action and outcome in full:**

```
A: (b2_sso, fb2 = "Write the user record first. Only after the write
   succeeds: publish UserSSOAuthenticated. Never publish before the write.")
   + result: { userId: "a4f2c891-...", status: "active", provider: "google",
   email: "user@example.com" }
B:
   Auth Service wrote user record to PostgreSQL.
   Record: { userId: "a4f2c891-...", status: "active", provider: "google",
   email: "user@example.com", createdAt: timestamp }.
   Write confirmed (row exists in database).
   Idempotency key checked before publish: userId + "UserSSOAuthenticated"
   not yet in event log.
   UserSSOAuthenticated published to Kafka topic "user-events".
   Access token issued: 15-minute validity.
   Refresh token issued: 7-day validity, rotation enforced on next use.
   Both tokens stored in httpOnly Secure SameSite=Lax cookie.
   No event was published before the database write confirmed.
Reason: registration flow document, write-before-publish rule and JWT
   storage requirements.
```

---

### Node 6 — Result translator

**Bad pair — B is a description of a human-readable fragment:**

```
A: R1 = { userId: new, status: active, provider: google, JWT issued,
          UserSSOAuthenticated published }
B: A human-readable result fragment that still preserves the exact source
   meaning.
```

Why it fails: B describes what the translation should be. It is not a
translation.

**Good pair — B is the actual user-facing text:**

```
A: R1 = { userId: "a4f2c891-...", status: "active", provider: "google",
          jwtIssued: true, event: "UserSSOAuthenticated" }
   Also sees: R3 = { questionnaire: "delivered", channel: "in-app chat" }
B: "You're signed in with Google. Your account has been created."
Reason: "jwtIssued" and "UserSSOAuthenticated" are internal. The user
   needs to know they are authenticated and their account exists — nothing
   else. The consistency check with R3 confirms the same user is signing
   in via SSO, not email — no contradiction.

Pair — Phrasing B
A: R1 = { same fields }
B: "Welcome. You're now signed in using your Google account."
```

**Good pair — consistency flag:**

```
A: R1 = { provider: "google", registrationMethod: "SSO" }
   R4 = { registrationMethod: "email", verificationToken: "sent" }
B: Inconsistency detected. R1 records an SSO registration. R4 records an
   email registration with a verification token sent. A single flow run
   cannot have both. Do not translate — send back for reprocessing. The
   registration path must be resolved to one method before translation.
```

---

### Node 7 — Arbiter selector

**Bad pair — B is a description of which kinds of arbiters apply:**

```
A: R1...Rn for a registration that handled OAuth tokens and user credentials.
B: Reviewer choice such as safety, event order, data consistency, or
   reliability.
```

Why it fails: B lists the categories of arbiters that might apply. It does
not name the arbiters that do apply and say why each one was chosen.

**Good pair — B names each arbiter and states the reason:**

```
A: R1...Rn for a flow run that: used OAuth 2.0 token exchange, stored a
   JWT in a cookie, published 6 events in a chain, checked for duplicate
   SSO callbacks, and completed questionnaire delivery.
B: Fire:
   - security-arbiter: tokens and credentials were handled; JWT storage
     location and token strength must be verified.
   - event-sequence-arbiter: 6 events published; the published order must
     match the documented sequence (UserSSOAuthenticated before UserCreated).
   - idempotency-arbiter: duplicate callback check was performed; the
     idempotency result must be verified as identical to the first execution.
Reason: registration flow document, security section (token storage rules),
   event chain section (ordering requirements), and duplicate-callback edge
   case.

Pair — Phrasing B
A: The flow exchanged an OAuth code, issued a JWT, published an event
   chain, and handled a second callback arriving after the first.
B: Fire security-arbiter (token handling present), event-sequence-arbiter
   (ordered event chain produced), idempotency-arbiter (duplicate callback
   was processed). Each fires on the specific result it is responsible for.
```

---

### Node 8 — Arbiter units

**Bad pair — B is a description of the flag rather than the flag itself:**

```
A: R1 = { JWT stored in localStorage }
   Arbiter: security-arbiter
B: Flag it. The source says tokens must be stored correctly. Send the
   affected fragment back for reprocessing.
```

Why it fails: B says a flag was raised without stating what rule was
violated, why localStorage is wrong, or what the reprocess instruction is.
The node learns to say "flag it" without learning what to flag or why.

**Good pair — B states the rule, the violation, and the exact reprocess instruction:**

```
A: R1 = { JWT: { accessToken: "eyJ...", storageLocation: "localStorage" } }
   Arbiter specialisation: security — token storage location.
B: Flag.
   Rule: The registration flow document requires tokens to be stored in
   httpOnly, Secure, SameSite=Lax cookies. localStorage is accessible to
   any JavaScript running on the page, including injected scripts from
   cross-site scripting attacks. An attacker who achieves XSS can steal
   all tokens from localStorage. An httpOnly cookie is not accessible to
   JavaScript at all — the browser sends it automatically on requests but
   no script can read it.
   Reprocess: Change the token storage step. Move both the access token
   and the refresh token from localStorage to httpOnly Secure SameSite=Lax
   cookies. Verify no JavaScript reference to localStorage.setItem or
   localStorage.getItem remains in the authentication flow.

Pair — Phrasing B
A: The result shows the session token stored in the browser's local
   storage rather than in a cookie.
   Arbiter: token security checker.
B: Flag. Local storage is readable by any JavaScript on the page;
   httpOnly cookies are not. The rule requires the cookie. Reprocess:
   move the token to an httpOnly Secure cookie and remove the localStorage
   storage call.
```

**Good pair — pass case:**

```
A: R1 = { JWT: { accessToken: "eyJ...", storageLocation: "httpOnly cookie",
          cookieFlags: ["Secure", "SameSite=Lax"],
          accessTokenExpiry: "15m", refreshTokenExpiry: "7d",
          refreshRotation: true } }
   Arbiter specialisation: security — token storage location.
B: Pass.
```

---

### Node 9 — Formatter

**Bad pair — B is a description of a section rather than the section:**

```
A: ["You're signed in with Google.", "Check your chat for a questionnaire."]
B: A section, sentence, or boundary in the final answer that keeps this
   source meaning visible.
```

Why it fails: B describes what the output is. It is not the output.

**Good pair — B is the actual combined text, shaped for a specific audience:**

```
A: ["You're signed in with Google. Your account has been created.",
    "We've sent you a quick questionnaire in your chat."]
   Audience: user (mobile screen, first session)
B: "You're signed in with Google. Welcome!
   Check your chat — we've sent you a quick questionnaire to help us
   show you the right business connections."
Reason: Two approved fragments combined into one sentence. Open with the
   action that completed (signed in). Name the provider (Google — confirms
   which method was used). Give one next step (check chat). State why
   (so we show the right connections). Length: two sentences — readable
   immediately on a mobile screen, enough to prompt action.

Pair — Phrasing B (product manager audience)
A: [same approved fragments]
   Audience: product manager
B: "Registration completed. Path: SSO-Google. Time to dashboard: under
   10 seconds. Questionnaire delivered. No drop-off detected."
Reason: Product manager view is a funnel summary — outcome first, then
   path, then timing, then next funnel step, then drop-off status. The
   underlying facts are the same; the structure serves a different reader.
```

---

## How to Read an External Source Unit

Not every part of an external source produces usable pairs.

**Usable — reasoning chains.** A span where a decision is made by weighing
alternatives. In an RFC: the motivation section, the alternatives section, the
community debate, the revision history, the verdict. In a PEP: the abstract, the
motivation, the rationale, the rejected alternatives, the open issues. These
sections contain the reasoning that pairs can be built from.

**Usable — verification criteria.** OWASP requirements, RFC MUST/SHOULD/MAY
conditions, and similar testable conditions with explicit pass/fail structure.
Each requirement becomes an arbiter-unit pair: the requirement is A, meeting or
violating it is B.

**Not usable — fact statements without reasoning.** "OAuth requires PKCE for
public clients" is a fact. It produces a pair where B restates A. Usable form:
the span that explains what failure mode the implicit flow allowed, which CVEs
demonstrated it, and why PKCE closes those modes without degrading the user
experience. That contains reasoning.

**Not usable — tutorial and introductory content.** Step-by-step examples,
hello-world demonstrations, introductory explanations. These produce circular
pairs.

Mark usable spans before writing any pairs. A usable span is a passage containing
a decision with alternatives weighed, a verification criterion with a pass/fail
structure, or a rejection with its reason stated.

---

## Which Nodes Each Source Group Trains Best

This table is a guide, not a restriction. If a source unit contains content
that fits a node not listed, use it.

**Architecture and orchestration sources** (Kubernetes KEPs, OpenTelemetry Spec,
OpenAPI Spec, Google AIP, Microsoft Learn architecture):
These produce the richest pairs for:
- Node 1 (domain splitter): the problem statement of a KEP or AIP names which
  domains it touches — orchestration, observability, API contract, compatibility.
- Node 3 (scheme planner): KEPs contain a detailed design section with motivations
  for ordering decisions; the accepted design becomes sub-tasks, the rejected
  alternatives become negative examples.
- Node 7 (arbiter selector): KEP production-readiness gates explicitly name which
  review criteria apply to a given enhancement — those gates map directly to
  which arbiters fire.
- Node 8 (arbiter units): KEP graduation criteria and production-readiness
  checklists are verifiable conditions; each criterion becomes an arbiter-unit pair.

**Security and governance sources** (OWASP ASVS, OWASP Cheat Sheets, NIST SSDF,
CWE Top 25, IETF RFCs):
These produce pairs almost exclusively for:
- Node 8 (arbiter units): each requirement or CWE mitigation is A. B is pass if
  the condition is met, or a flag naming the specific violation and the exact change
  required.
- Node 7 (arbiter selector): groups of requirements that apply together in a given
  context (all authentication requirements when a token is present, for example)
  are selector pairs.
- Node 2 (domain translator): IETF RFCs supply the formal vocabulary for their
  domain; a plain-language description of an authentication or protocol requirement
  as A, and the RFC-precise restatement as B.

**Code agent and implementation datasets** (SWE-bench, Defects4J, BugsInPy,
QuixBugs):
These produce the richest pairs for:
- Node 3 (scheme planner): a bug issue description is A; the decomposition into
  inspection steps, localisation, fix, and verification is B.
- Node 4 (domain processor): one sub-task (e.g., reproduce the failure) has
  a specific outcome — the reproduction succeeds or the failure cannot be
  reproduced under the described conditions.
- Node 5 (applier): the patch application and its test outcome is the concrete
  action and result.
- Node 8 (arbiter units): the test suite result verifies the fix — pass if all
  tests pass, flag if any regression is introduced.

---

## How to Write the Training Pairs

### For Node 1 — Domain splitter

A is a user-prompt-shaped statement. The kind of thing a human would type when
asking the system to do something related to this document's subject. Not a line
from the document. A prompt a real person might write.

B is the array of domain labels that prompt touches. Not a description of a
domain. The actual names of the domains, listed.

Write at least one pair per domain (a prompt touching only that domain), then at
least three multi-domain pairs (a prompt touching several domains). Each pair with
its rephrasing.

### For Node 2 — Domain translator

A is one prompt fragment paired with one domain label. The fragment is part of a
real user prompt assigned to that domain.

B is the full formal translation of that fragment into the precise vocabulary of
that domain. Not a description of what the translation would contain. The actual
text a domain specialist would write, naming specific rules, specific values,
specific sequences — everything the fragment implies when read through that
domain's lens.

Write one pair per domain the document defines. Each pair with its rephrasing.

### For Node 3 — Scheme planner

A is all the domain translations for one flow path together. Name the path (the
SSO path, the email path, the error path) so it is clear what the planner
receives.

B is the actual plan: the labelled sub-tasks, each paired with its
transformational prompt. The transformational prompt names the specific rule or
pattern and its parameters. It is not "apply the right pattern." It says which
pattern and how.

Write one pair per distinct flow path the document defines. Include error and
edge-case paths alongside the happy path. Each pair with its rephrasing.

### For Node 4 — Domain processor

A is one sub-task by name (b1, b2, b_merge, b_rate, etc.) paired with what that
sub-task requires.

B is the full set of outcomes: the success output, every distinct failure output,
every wait-state outcome. Each outcome expressed as concrete fields, values, and
states — not as a description of what the output might contain. The output must
contain enough for the next stage to act without re-reading the original input.

Write one pair per sub-task the scheme planner produces. Each pair with its
rephrasing.

### For Node 5 — Applier

A is one sub-task paired with its transformational prompt, plus the processed
result from Node 4.

B is the concrete action and its observable outcome: what was written, what was
published, what was returned, in what order. When the source document defines
record creation followed by event publication, the write appears first in B, the
publish after.

Write one pair per sub-task. Include at least one failure case showing what
happens when the action cannot complete. Each pair with its rephrasing.

### For Node 6 — Result translator

A is one formal result in system language (specific fields, values, event names).
Note that the translator also sees the full result collection — this context
determines what the translator may flag.

B is the natural-language version: what the user needs to know, in words the user
would recognise, with system internals removed. Event names, queue names, table
names, and HTTP status codes do not appear in B unless the user explicitly needs
them.

Include at least one consistency-check pair: two results from the same flow run
that contradict each other. B for that pair is not a translation — it is a flag
stating the contradiction and declining to translate until it is resolved.

Write one pair per distinct result the applier array produces. Include graceful-
failure pairs: results that describe a failure the user should not be exposed to
have their own translation rule. Each pair with its rephrasing.

### For Node 7 — Arbiter selector

A is the full collection of formal results from a flow run, plus the context
(which path was taken, what kinds of decisions were made, what kinds of data
were handled).

B is the list of arbiters to fire. The actual names of the chosen arbiters, each
followed by one sentence saying why it applies to this result collection.

Write one pair per meaningfully distinct flow context the document defines.
Include cases where different paths produce different arbiter choices. Each pair
with its rephrasing.

### For Node 8 — Arbiter units

A is one result or result group paired with the arbiter's specialisation.

B is exactly one of three things:
- Pass — no explanation. The arbiter's job is done. Silence is the pass signal.
- A concern — the exact rule violated, the location in the source document where
  that rule is stated, and why the result violates it.
- A reprocess instruction — the exact fragment to send back, what must change in
  that fragment, and which rule the change satisfies.

Write at least one pass pair. Write at least one flag pair per rule the document
states explicitly. Write at least one reprocess pair. Rejected examples derived
from source rules must be marked as derived, not as recorded observations.

Each pair with its rephrasing.

### For Node 9 — Formatter

A is the approved translated result collection — natural language that passed
arbitration.

B is one combined output shaped for one specific audience. Write separate pairs
for at least two audiences (user-facing and one specialist audience such as
product manager, security reviewer, or devops). Include a graceful-failure pair:
a failure that was handled and recovered appears in B as if the recovery is the
current state, without exposing the failure.

Each pair with its rephrasing.

---

## How to Organise the Output Files

### Internal documents

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

### External sources

```
external-sources-training-sets/
  00-INDEX.md
  01-UML-FRAME.md
  COVERAGE-LEDGER.jsonl
  source-groups/
    group-1-proposals/
      <rfc-number>-<title>-sets/
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
    group-2-architecture/
      <chapter-name>-sets/
        (same node files)
    group-3-requirements/
      <requirement-group>-sets/
        (same node files)
```

The `01-UML-FRAME.md` for external sources is shared across all source units
in the same corpus run. Do not recreate it per source unit.

---

## Index and UML Frame File Contents

**`00-INDEX.md`** records:
- Source document identity: path, line count, byte size, first and last
  meaningful lines.
- For external sources: canonical URL, fetch date, license confirmed.
- Version history found (internal) or revision history from the source itself
  (external).
- Session evidence classification (internal) or source provenance (external).
- Domain names this document defines.
- Pair count per node.

**`01-UML-FRAME.md`** records the complete edge inventory from Step 4. For every
edge: what moves from the source node, what the target node receives, what the
target node must produce, what breaks if the edge is missing. This file is the
binding frame: a pair in any node file that cannot connect to an edge listed here
does not belong in this round.

---

## Pair Format

Every pair in every node file follows this structure:

```
Pair [ID] — [optional descriptor]
A: [the exact input, written out in full]
B: [the exact output, written out in full]
Reason: [which source rule or passage B comes from]

Pair [ID] — Phrasing B
A: [the same situation in different words]
B: [the same output in different words]
```

For external sources, add a source field after the pair ID line:
```
Source: [canonical URL] — [section or line range within the source unit]
License: [license name]
```

---

## Coverage Ledger for External Sources

The coverage ledger records one entry per source unit examined. Entry format:

```
source unit identifier | canonical URL | source type | license | unit title |
status | nodes trained | pair count | fetch date | promotion status
```

Status values:
- `examined` — read, usable spans marked, not yet written into pairs.
- `pairs_written` — pairs written, simulation not yet run.
- `simulation_complete` — simulation run, gaps named.
- `promoted` — all five pre-promotion checks passed.
- `license_blocked` — license not usable, no content used.
- `license_unknown` — license cannot be confirmed, no content used.
- `circular` — failed check 2 (B restates A).
- `requires_span_extraction` — failed check 3 (context insufficient without the
  source unit).
- `duplicate` — failed check 5 (already covered by internal corpus with equal
  depth).

Every examined unit must appear in the ledger. An examined unit with no ledger
entry is invisible to the next session.

---

## Pre-Promotion Checks for External Sources

Before any external pair moves to the final training set, it must pass these
checks. A pair that fails any check is marked with the label and held — not
deleted.

**Check 1 — License clean.** License confirmed usable. CC-BY-SA pairs carry
attribution in their source field.

**Check 2 — Reasoning present.** A is a situation requiring a decision or
evaluation. B is the decision or evaluation with its basis stated. B does not
restate A in different words.

**Check 3 — Context sufficient.** A and B together contain enough information for
a node to reproduce the output on a similar input without reading the source unit.
If the pair only makes sense with the full source unit in context, mark it
`requires_span_extraction` and note the exact section that must be included in A.

**Check 4 — Node assignment correct.** The pair belongs to the node file it is in.
A requirement statement belongs in Node 8. A decomposition belongs in Node 3.
A misassigned pair is correct content in the wrong file — move it.

**Check 5 — Not duplicated in internal corpus.** The external pair adds a
reasoning angle the internal corpus does not have. If it adds nothing new, mark
it `duplicate`.

---

## The Simulation

After the pairs for one source document or source unit are written, simulate the
flow as if the nodes had been trained only on those pairs.

The simulation answers these questions in order:

1. Given a prompt about this document's subject, what domain labels would the
   splitter emit?
2. What formal translations would the translator array produce for each domain?
3. What sub-tasks and transformational prompts would the scheme planner produce?
4. Which sub-tasks would the processor array handle in parallel?
5. What concrete outcomes would the applier array produce?
6. What natural-language fragments would the result translator array produce?
7. Which arbiters would the selector choose, and why?
8. Which results would arbiters pass, flag, or send back?
9. What final output would the formatter produce for at least two different
   audiences?
10. What is missing?

For external sources, use the source unit's own problem statement or requirement
context as the simulation input — not a scenario invented independently.

The missing-parts section must be present and honest. State which future document
or source unit would address each gap. A gap that is hidden is a failure.

---

## Validation Before Closing a Round

Check these before the round is complete:

Every domain the source document defines has at least one domain-splitter pair
and one domain-translator pair.

Every sub-task the scheme planner produces has a domain-processor pair and an
applier pair.

Every distinct result the applier array produces has a result-translator pair.

Every arbiter the selector chooses has an arbiter-units pair with at least one
pass case and at least one flag case.

The formatter has at least two audience pairs.

Every pair appears with at least one rephrasing.

No node file contains a B that describes the output instead of showing it.

The array nodes — domain translator, domain processor, applier, result translator
— are written as arrays: each pair covers one unit receiving one element, not one
large unit receiving all elements.

The arbiter selector is a separate node file from the result translator.

The formatter receives only approved translated results.

The simulation exists, runs to completion, and names its missing parts.

For external sources: every examined source unit appears in the coverage ledger
with a status.

---

## Examination Order for External Sources

Examine sources in this order. The ordering reflects where the current training
set has the largest gaps. KEPs and AIPs come first because their acceptance/
rejection structure directly feeds Nodes 7 and 8, which are the thinnest nodes.
Security and governance sources come second because they extend Node 8 with
systematic pass/fail pairs. Code agent datasets come third because they cover
Nodes 3 through 5 with real implementation evidence.

**Phase 1 — Architecture and orchestration (target: 20 source units)**

Begin with Kubernetes KEPs. Use the KEP index at
`https://github.com/kubernetes/enhancements/tree/master/keps` to identify KEPs
with "Production Readiness Review" sections — those contain the richest
arbiter-unit material. The first five units to examine:

- KEP-2799 (Reduction of Secret-based Service Account Tokens) — security
  decision with clear acceptance criteria; arbiter-unit pairs from each
  production-readiness gate.
- KEP-3488 (CEL for Admission Control) — multiple competing designs considered;
  scheme-planner pairs from the alternatives section.
- KEP-1282 (Conformance Testing for Pod Security Standards) — graduation criteria
  map directly to arbiter-selector pairs.
- KEP-2305 (Server-Side Unknown Field Validation) — contains explicit rejection
  of alternative approaches with reasons stated; negative scheme-planner examples.
- One KEP at "Stable" graduation — production-readiness review completed in full.

Then OpenTelemetry Specification. Begin with the trace specification:

- Trace semantic conventions (resource attributes section) — each semantic
  convention is a verifiable requirement; arbiter-unit pairs from MUST/SHOULD.
- Baggage specification — interface contract design with alternatives section;
  scheme-planner and domain-translator pairs.
- One metrics specification section — observability domain-translator pairs.

Then Google AIP. Examine:

- AIP-121 (Resource-oriented design) — the foundational AIP; domain-splitter
  and scheme-planner pairs from the design principles.
- AIP-131 (Standard methods: Get) — precise API contract; arbiter-unit pairs
  from each MUST condition.
- AIP-4232 (Long-running operations) — wait-state decomposition; scheme-planner
  pairs showing ordered steps with a wait state.

**Phase 2 — Security and governance (target: complete OWASP ASVS sections 2 and 4,
plus selected IETF RFCs)**

Examine OWASP ASVS section 2 (Authentication) and section 4 (Access Control)
completely. Every numbered requirement becomes an arbiter-unit pair. Work in
section order. Do not skip a requirement because it seems narrow — a narrow
requirement still provides a pass-case pair, which Node 8 needs.

Then examine CWE Top 25 entries relevant to the registration and authentication
domain: CWE-287 (Improper Authentication), CWE-306 (Missing Authentication),
CWE-522 (Insufficiently Protected Credentials), CWE-798 (Hardcoded Credentials).
Each weakness entry provides arbiter-unit pairs from its mitigations section.

Then IETF RFC 7519 (JWT), RFC 6749 (OAuth 2.0), and RFC 8446 (TLS 1.3). These
supply the formal vocabulary for authentication domain-translator pairs and extend
the arbiter-unit pairs with RFC-level precision.

**Phase 3 — Code agent datasets (target: 30 SWE-bench task instances)**

Examine SWE-bench instances from the verified split. Begin with instances where
the issue description is clear and the verified patch is small — these produce
the cleanest scheme-planner and domain-processor pairs. Work through instances
in order of patch size (smallest first) to build the pairs in increasing
complexity.

---

## After One Document or Source Unit

Stop. Record the unit in the index or coverage ledger. Review the pairs produced.
Confirm they pass the validation checklist. Only then choose the next document or
source unit.

One document examined carefully and completely is worth more than ten examined
superficially. A pair where B describes the output instead of showing it
contaminates the node it is assigned to. The node learns to produce descriptions.
That is the failure this method was written to prevent.
