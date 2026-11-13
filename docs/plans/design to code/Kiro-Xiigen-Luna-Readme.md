# XIIGEN · Luna slice pack
## Startup → S01 → S02 recovery completion

**Use this as a replacement workflow, not as replacement product code.** This
package adapts the original Kiro ZIP and the subsequent execution history for
GPT-5.6 Luna. It does not execute or finish S02 merely by being installed.

### Install without overwriting your work

Select **GPT-5.6 Luna**, not Auto, in Kiro. Extract this ZIP outside the repository.
Copy the contents of `overlay/` into your EXISTING checkout, preserving any
conflicting file. All delivered runtime paths are new, under `luna/` or named
`xiigen-luna-through-s02`; the old work records, AGENTS.md, CLAUDE.md and original
skills are not replaced. The package contains no pre-populated runtime work state.

Alternatively, preview and then apply the additive installer from this extracted
package, using an already available Python 3.10+:

```powershell
python .\install.py --repo C:\Projects\llm_mvp_core
python .\install.py --repo C:\Projects\llm_mvp_core --apply
```

The default is a read-only preview. `--apply` refuses different existing files,
unsafe paths, a different origin repository, or a different branch. It does not
change permissions, install software, stage, commit or push. Manual copying must
observe the same conflict rule. The original ZIP in `reference/` is provenance,
not a second overlay to install.

### Start or resume

Open `START-PROMPT.md` and paste its text into Kiro. An identical copy is installed
at `.kiro/xiigen-slices/luna/START-PROMPT.md`. The same prompt handles a fresh
checkout or your current interrupted S02.3: it selects the earliest unmet predicate
from actual source and evidence, rather than replaying S00/S01 or recreating the
portable optimizer/file layer. The prompt explicitly adopts the task-local model,
reading, review and continuation choices described in `ADAPTATION-NOTES.md`.

A new session must actually receive the existing work files. Untracked files in
another cloud sandbox are not assumed to persist. See `HANDOFF.md`.

### What is different

One compact work order is active at a time. Each contains the eight requested
sections and names real APIs, bounded source ranges, skills, good/bad examples,
checks and error branches. Architecture/plan review, execution and code review use
separate sequential contexts. The worker that edits source does not accept its own
work. Technical errors lead to a focused correction, not another permission loop.

The runner builds once per invocation, then runs a focused gate and an impacted
gate against that candidate. It records numeric exits, separate logs, source
fingerprints and parsed TRX counts. It rejects an empty or incomplete test run.
It never uses repeated blind sleep commands as completion evidence.

### Meaning of “finish S02”

The later S02 brief narrowed the immediate work to AdamW memory and its step
counter; the original queue is broader: all mutable training state on the selected
path. The package preserves both meanings. It first closes the existing optimizer
sequence. Its final inventory then checks weights, adapters, optimizer settings,
scheduler/counters and stochastic state applicable to that path. Missing required
families become separate bounded S02 work orders, not an invisible deferral to S03.
The start prompt explicitly authorizes that recovery-only completion scope.

No result may claim general training recovery from optimizer-only tests. An
in-process reconstruction remains labelled in-process. Real model quality,
certification, data generation, external services and remote model publication are
outside this package's execution scope. Stop before S03.

### Model and verification boundaries

Kiro's published Luna context is **272K tokens**, while OpenAI's API model page
lists a different, larger API window. This pack targets Kiro's limit, not the API
limit. The supplied context budgets are tuning defaults, not benchmark results.
No Kiro-internal model identifier or reasoning-control schema is invented.

The pack was checked locally as described in `validation/REPORT.md`. Those checks
are not your repository's .NET test results and not a live Luna benchmark.
See `SOURCES.md` for dated primary sources and the inspected repository revision.
