# External Knowledge Sources — Examination Plan v1.0

**Purpose:** Instructions for a Claude Code session to fetch, examine, and convert
open-source knowledge sources into the same document structure already established
for the internal corpus (the batch format, domain mapping, and row schema).

**Prerequisite reading for the executing agent:**
- `MODEL-EXAMINATION-COVERAGE-SUMMARY-v1.md` — the ledger rules and status vocabulary
- `domain claude research/INDEX.md` — the eight domains and the a→f(b)+c functional form
- `domain claude research/batches/batch-001-025.md` — the batch format to produce
- `DOMAIN-CODEX-RESEARCH-ROWIZED-DRAFT-v1.json` — the row schema to target

---

## What this plan adds

The internal corpus examination (IDs 1–850+) covers files already on disk:
skills, experiment results, session history, governance documents, and C# source.

This plan covers **external** open-source material fetched from the internet.
The goal is the same: produce examined records and draft training rows
in the eight dataset families, using the same a→f(b)+c lens.

The eight families, for reference:
1. `userPrompt_to_machineCommands`
2. `machineCommand_to_machineActionItems`
3. `machineActionItem_to_domainResult`
4. `domainResultsArray_to_resultI`
5. `arbitrateResultI_with_resultsContext`
6. `resultI_to_finalMachineResults`
7. `finalMachineResults_to_userFormat`
8. `historyAndVersions_to_reprocessingDecision`

---

## Source registry

Each source has: a fetch location, a license, a primary domain mapping,
and the family or families it feeds best.

### Group 1 — Proposal and debate archives (highest priority)

These are the richest sources. Each proposal is a complete pipeline:
problem statement → proposed solution → community objections → revision → verdict.
That maps directly to families 1, 5, and 8.

| Source | Fetch location | License | Primary domain | Families |
|--------|---------------|---------|---------------|---------|
| Rust RFCs | `https://github.com/rust-lang/rfcs` — clone `text/` directory | MIT | E-Arbiter, H-Governance | 1, 5, 8 |
| Python PEPs | `https://github.com/python/peps` — clone `peps/` directory | Public domain | H-Governance, C-Translation | 1, 5, 8 |
| Go Proposals | `https://github.com/golang/proposal` — clone `design/` directory | BSD | H-Governance | 1, 5, 8 |
| Kafka KIPs | `https://cwiki.apache.org/confluence/display/KAFKA/Kafka+Improvement+Proposals` — fetch index, then fetch each KIP page | Apache 2.0 | A-Topology, F-Registry | 1, 5, 8 |
| TypeScript design notes | `https://github.com/microsoft/TypeScript/issues?q=label%3A%22Design+Meeting+Notes%22` | MIT | B-Routing, C-Translation | 5, 8 |
| CloudEvents spec issues | `https://github.com/cloudevents/spec/issues?q=is%3Aclosed` | Apache 2.0 | F-Registry, A-Topology | 5, 8 |

### Group 2 — Architecture narrated by builders (high priority)

These explain why systems are shaped the way they are — constraints, failures,
forced decisions. Maps to families 1, 3, and 5.

| Source | Fetch location | License | Primary domain | Families |
|--------|---------------|---------|---------------|---------|
| Architecture of Open Source Applications vol 1 | `https://aosabook.org/en/v1/` — fetch each chapter | CC-BY 3.0 | A-Topology, D-Execution | 1, 3, 5 |
| Architecture of Open Source Applications vol 2 | `https://aosabook.org/en/v2/` — fetch each chapter | CC-BY 3.0 | A-Topology, D-Execution | 1, 3, 5 |
| 500 Lines or Less | `https://aosabook.org/en/500L/` — fetch each chapter | CC-BY 3.0 | D-Execution, C-Translation | 1, 3 |
| .NET Microservices book | `https://github.com/dotnet-architecture/eShopOnContainers/wiki` + `https://learn.microsoft.com/en-us/dotnet/architecture/microservices/` | MIT / CC-BY 4.0 | A-Topology, F-Registry | 1, 2, 3 |
| System Design Primer | `https://github.com/donnemartin/system-design-primer` — clone README sections | CC-BY 4.0 | A-Topology, B-Routing | 1, 5 |

### Group 3 — Verifiable requirement sets (medium priority)

Each entry is a testable condition with a pass/fail structure.
Maps to family 5 (arbiter training) almost exclusively.

| Source | Fetch location | License | Primary domain | Families |
|--------|---------------|---------|---------------|---------|
| OWASP ASVS v5.0 | `https://github.com/OWASP/ASVS` — clone, read CSV export | CC-BY-SA 4.0 | E-Arbiter | 5 |
| OWASP Testing Guide | `https://github.com/OWASP/wstg` — clone `document/` | CC-BY 4.0 | E-Arbiter | 5 |
| IETF RFCs (selected) | `https://www.rfc-editor.org/rfc/` — fetch RFC 7519 (JWT), 7617 (Basic Auth), 9110 (HTTP), 6749 (OAuth2), 8446 (TLS 1.3) | Public domain | E-Arbiter, H-Governance | 3, 5 |
| OpenAPI Specification | `https://github.com/OAI/OpenAPI-Specification` — clone `versions/` + issue archive | Apache 2.0 | B-Routing, F-Registry | 1, 3, 5 |

---

## Examination procedure

The agent runs this procedure for every source unit (one RFC, one PEP,
one AOSA chapter, one ASVS section, one KIP). The output format matches
the internal batch format exactly.

### Step 1 — Fetch

```bash
# For GitHub repos: clone sparse
git clone --depth=1 --filter=blob:none --sparse <repo-url> <local-dir>
cd <local-dir>
git sparse-checkout set <subdirectory>

# For individual pages: use curl or fetch
curl -s "<url>" -o <local-file>

# For AOSA: fetch chapter list first, then each chapter
curl -s https://aosabook.org/en/v1/ | grep -oP 'href="[^"]+\.html"' | sort -u
```

### Step 2 — Identify source unit boundaries

Each source has a natural unit of examination. The unit is the smallest item
that contains a complete reasoning chain.

| Source | Unit boundary |
|--------|--------------|
| Rust RFCs | One `.md` file per RFC |
| Python PEPs | One `.rst` file per PEP |
| Go Proposals | One `.md` file in `design/` |
| Kafka KIPs | One wiki page per KIP |
| AOSA | One chapter (one HTML file or one major section) |
| OWASP ASVS | One requirement group (one numbered section, e.g., 2.1 Passwords) |
| IETF RFC | One complete RFC document |

### Step 3 — Examine each unit

For each source unit, produce a record with this exact structure.
This is the same structure as the internal batches.

```markdown
### EXT-<GROUP>-<SEQUENCE> <source-name> | <unit-title>

**Source:** <canonical URL>
**License:** <license name>
**Fetched:** <date>
**Unit type:** <RFC | PEP | chapter | requirement-group | proposal>

**Structure:**
<brief description of the document's internal structure —
sections, headings, key parts>

**Domain:** <one of A through H from the internal taxonomy>

**Reasoning chain present:** <YES | PARTIAL | NO>
<If YES: note which sections contain the live reasoning —
motivation, alternatives, community debate, revision, verdict>

**a→f(b)+c:**
- a (trigger/input): <what starts this reasoning>
- f (transform): <what reasoning process is applied>
- b (content): <what the content being reasoned about is>
- c (output/verdict): <what the conclusion or decision is>

**Families this unit feeds:**
<list the dataset family numbers and why>

**Positive training examples in this unit:**
<describe 1-3 specific spans that can become positive rows —
a question + correct answer, a proposal + accepted solution,
a requirement + compliant implementation>

**Negative training examples in this unit:**
<describe 1-3 specific spans that can become negative/rejected rows —
a rejected proposal, a downvoted alternative, a failing check>

**Draft rows:**
<produce 1-3 rows in the DOMAIN-CODEX-RESEARCH-ROWIZED-DRAFT schema>
```

### Step 4 — Draft row schema

Each draft row must follow the same schema as the internal corpus rows:

```json
{
  "rowId": "EXT-<GROUP>-<SEQUENCE>-R<ROW_NUMBER>",
  "sourceId": "EXT-<GROUP>-<SEQUENCE>",
  "sourceUrl": "<canonical URL>",
  "sourceLicense": "<license>",
  "datasetFamily": "<one of the 8 family names>",
  "subdomain": "<architecture | security | distributed-systems | language-design | ...>",
  "input": {
    "a": "<the trigger or user intent>",
    "b": "<the content being operated on>",
    "c": "<the context or constraint>"
  },
  "function": "<description of the transform or reasoning applied>",
  "expectedOutput": {
    "<output type>": "<what the correct output looks like>"
  },
  "arbiterDecision": {
    "primaryArbiters": ["<arbiter name from the source's own review process>"],
    "draftVerdict": "<usable_as_positive | usable_as_negative | requires_span_extraction | skip>"
  },
  "notes": "<anything the human reviewer should check before promoting this row>"
}
```

### Step 5 — Batch file structure

Group examined records into batch files of 25 records each,
following the same naming convention:

```
external-sources/
  batches/
    ext-batch-001-025.md      <- first 25 source units examined
    ext-batch-026-050.md
    ...
  domain-files/
    ext-domain-A-topology.md       <- all external A-domain findings
    ext-domain-B-routing.md
    ext-domain-C-translation.md
    ext-domain-D-execution.md
    ext-domain-E-arbiter.md
    ext-domain-F-registry.md
    ext-domain-G-learning.md
    ext-domain-H-governance.md
  rows/
    ext-rowized-draft-v1.json      <- all draft rows accumulated
  INDEX.md                         <- same structure as internal INDEX.md
  COVERAGE-LEDGER.jsonl            <- one row per source unit, status tracking
```

---

## Execution order

Run sources in this order. The ordering reflects value density —
the sources most likely to produce rich family-5 and family-8 rows
come first because those are the two most under-covered families.

**Phase 1 — Rust RFCs (target: 50 RFCs examined)**

Priority RFCs to examine first (richest reasoning chains):
- RFC 2229 — closures capturing disjoint fields (accepted after heavy debate)
- RFC 1977 — owned ASCII string type (declined — full negative example)
- RFC 2582 — `impl Trait` in type aliases (long debate, revision history)
- RFC 2996 — async closures (multiple revisions visible)
- Any RFC with >50 GitHub comments — use GitHub API to sort by comment count

Command to get comment count ranking:
```bash
gh api repos/rust-lang/rfcs/issues \
  --jq '.[] | {number: .number, title: .title, comments: .comments}' \
  --paginate | jq -s 'sort_by(-.comments) | .[:50]'
```

**Phase 2 — Python PEPs (target: 30 PEPs examined)**

Priority PEPs:
- PEP 572 — walrus operator (most contested PEP in history, full debate)
- PEP 634 — structural pattern matching (multiple competing designs)
- PEP 3333 — WSGI (interface contract design with alternatives section)
- PEP 484 — type hints (Guido's original proposal + Jukka Lehtosalo responses)
- Any PEP with status `Withdrawn` or `Rejected` — these are pure negative examples

**Phase 3 — AOSA chapters (target: 10 chapters examined)**

Priority chapters:
- LLVM — explains IR design decisions, why SSA was chosen
- Nginx — event loop vs threading decision
- PostgreSQL — MVCC design and alternatives
- Python packaging — multiple failed attempts before current design
- Selenium — why the WebDriver protocol replaced RC

**Phase 4 — OWASP ASVS (target: complete section 2 and section 4)**

Section 2 (Authentication) and Section 4 (Access Control) are the most
directly relevant to the XIIGen auth gaps identified in the governance files.
Each requirement becomes one arbiter-training row for family 5.

**Phase 5 — Kafka KIPs (target: 20 KIPs examined)**

Priority KIPs:
- KIP-500 — removal of ZooKeeper dependency (years of debate)
- KIP-848 — next-generation consumer rebalance protocol
- KIP-98 — exactly-once delivery (multiple competing designs)
- KIP-1150/1176/1183 — diskless storage (three competing proposals simultaneously)

---

## Quality gates before promoting a row

Before any row moves from `draftVerdict: usable_as_positive` to the
final training corpus, it must pass these checks:

1. **License clean** — confirm the exact license of the source unit.
   CC-BY-SA is acceptable with attribution. CC-BY-NC-ND is not usable.
   Proprietary is not usable.

2. **Reasoning present** — the row must capture a reasoning step,
   not a fact statement. "OAuth requires PKCE for public clients" is a fact.
   "We rejected the implicit flow because server-side token interception
   was demonstrated in three CVEs, and PKCE provides the same UX with
   provable security" is reasoning. Only the second form is usable.

3. **Context sufficient** — the input fields (a, b, c) must contain enough
   context that a model could reproduce the output without having read
   the source document. If the row only makes sense with the full document
   in context, mark it `requires_span_extraction` and note the exact lines.

4. **Family assignment correct** — verify the row genuinely belongs to
   the declared family. A requirement statement (ASVS) is family 5.
   A step decomposition (KIP implementation plan) is family 2.
   A user's question + accepted answer (Stack Overflow) is family 1 or 7.

5. **No duplication with internal corpus** — check that the concept covered
   by the external row is not already captured in a draft row from the
   internal codex-research examination.

---

## Output files the agent must produce

At the end of each phase:

1. One or more `ext-batch-NNN-MMM.md` files in the batches directory
2. Updated `ext-domain-*.md` files for each domain touched
3. Updated `ext-rowized-draft-v1.json` with new rows appended
4. Updated `COVERAGE-LEDGER.jsonl` with one new row per source unit examined
5. Updated `INDEX.md` with phase summary

The ledger row format:

```jsonl
{"sourceId": "EXT-G1-001", "sourceUrl": "https://...", "sourceType": "RFC", "license": "MIT", "unitTitle": "RFC 2229 closures", "status": "domain_reviewed_draft_rowized", "domainsTagged": ["E", "H"], "familiesTagged": [5, 8], "rowsProduced": 2, "examinedDate": "2026-05-03"}
```

---

## What the agent must not do

- Do not fetch any source under CC-BY-NC-ND or proprietary license and
  include its text in rows. Note the source as `license_blocked` in the ledger.

- Do not produce rows from tutorial content (step-by-step how-to guides,
  introductory examples, "hello world" demonstrations). These have no
  reasoning chain and contaminate the training set.

- Do not produce rows where the input and output are the same thing rephrased.
  "What is the Strategy pattern? — The Strategy pattern is..." is circular
  and adds no signal.

- Do not batch more than 25 source units per file. The internal batches
  are 25 units each for a reason: larger batches lose coherence
  and become harder to review.

- Do not skip the ledger update. Every examined unit must appear in the
  ledger with a status. An examined unit with no ledger row is invisible
  to the next session.
