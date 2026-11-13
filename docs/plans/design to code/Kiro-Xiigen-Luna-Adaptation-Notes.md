# What this adaptation changes — explicitly

This ZIP is a proposed task-local successor. Adoption happens through START-PROMPT.md,
not through silently modifying the user's original repository policies.

| Original source / observed issue | Successor behavior | Preserved boundary |
|---|---|---|
| Old pack 00/01/06: one slice, ask before next execution | Live adoption authorizes the bounded S00–S02 sequence | Every slice still needs ready design, tests and separate review; S03 excluded |
| Skill readiness blocks: fixed 30 passes, some historical 50-pass packets, GPT-5.5/xhigh | Explicit task-local replacement with Luna and coverage-driven role reviews/corrections | No invented pass counts; later owner-specified counts still bind |
| Huge governance/skill references and full transcripts repeated | Per-role actual reads and one current packet | Required semantics/guards retained; originals not shortened or overwritten |
| Old seven-file adapter inventory | Nine known flat sources plus five actual requested skill groups | Inventory is not proof of loading |
| Original context manifest pins 4bbb2ca | Original pin remains unchanged in reference; separate current context map | Drift is visible, not fixed by rewriting history |
| S01 1–210 test window missed later cases | Full relevant methods/classes and explicit range freshness | No duplication just to generate a change |
| Repeated baseline-generic errors and false-positive test | Exact failed boundary and direct trainer/route observations | No security weakening or false-green error substitution |
| S02 helpers repeatedly substituted for promised wiring | Explicit S02.3 caller/identity/continuation acceptance | Component progress cannot close integration |
| Legacy optimizer-only brief versus original all-state S02 queue | Separate optimizer acceptance and final applicable-family inventory | Other families become recovery-only S02 slices under explicit live scope, not implicit source rewriting |
| PowerShell Boolean wrapper, nested shells, log self-append, blind sleeps | Single foreground supervisor, numeric exits, distinct logs/TRX, lock and heartbeat | No denial bypass or unrelated process killing |
| Old clean/pushed state equated with completion | Candidate-matched tests and review independent of publication | No Git/model publication without current instruction |

## Newly inspected source observations

At remote revision 77dc21d0414df342a01aac4d7be077b44b5d8659, the handoff test still
uses `LlmConfig`, manually constructs batch/pair fields, creates trainers without
mutation admission and does not clean up every temporary directory it creates.
DpoTrainer's constructors document that mutation without admission is refused;
Step checks pair admission and a mutation permit. These are source-inspection
findings, not test runs or proof that every guessed fixture field is invalid.

The state file's expected hash is optional at utility level. The integration work
must make required recovery identity mandatory through its owning contract and
validate saved length, phase, model/checkpoint/step. Do not silently mutate the
utility signature based solely on this observation.

## Not changed or claimed

No user repository source/configuration was edited or published to produce this
ZIP. No application test, live Kiro agent or Luna benchmark was executed here.
The original source requirements and safety semantics remain referenced; the
explicit task-local choices above are proposed adaptations, not statements that
the source originally said them. Existing host auto-loaded AGENTS.md can still
consume context; this package cannot suppress it merely by using smaller packets.
