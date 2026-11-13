# P5C promotion review

Этот обзор проверил десять строк-кандидатов из P3, P4 и P5A. P5B использован как контекст split draft, но новых row id он не создал.

Главный вывод: ни одна строка не является training-ready. Три строки можно использовать только как кандидаты для следующей фазы DPO (Direct Preference Optimization) authoring, где будут написаны новые redacted chosen/rejected rows, source-disjoint grouping и ablation controls. Это не разрешает training, eval, checkpoint rewrite или semantic-quality claim.

## Что повышено до кандидатов для следующего authoring

- `P3-PTT-DRAFT-001`: представитель PromptToTask семьи `basic prompt.txt`.
- `P4-ORCH-DPO-DRAFT-001`: представитель OrchestrationReasoning семьи phase-transition protocol.
- `P5A-CAND-PTT-AUTHORITY-RULES-001`: независимая PromptToTask семья из `AGENTS.md`, где source state уже `done`.

Эти строки можно брать только как input для будущего authoring. Следующая фаза обязана переписать их без raw private text, filename/path/title clues и stack-specific mechanics.

## Что осталось evidence-only

- `P5A-CAND-BOTH-SKILLS-INVENTORY-001`: полезная граница universal core vs adapter, но source state остается named-reference/todo.
- `P5A-CAND-ORCH-DOC07956-001`: полезная граница multi-branch review/targeted repair, но source state остается named-reference/todo и есть near-duplicate concern с NineNodeFlow.

Эти строки требуют parent/source acceptance перед любой фактической DPO materialization.

## Что merge/drop

- `P3-PTT-DRAFT-002`: same-source variant, не независимая PromptToTask семья.
- `P4-ORCH-DPO-DRAFT-002`: same-source variant, не независимая OrchestrationReasoning семья.

Их можно использовать как repair notes для representative rows, но нельзя считать отдельными source-disjoint examples.

## Что quarantined

- `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`: code-source leakage risk через `.cs`, class/method names и CoNLL-U tokens.
- `P5A-CAND-ORCH-NINE-NODE-FLOW-001`: code-source leakage risk и near-duplicate risk с doc07956.
- `P5A-CAND-ORCH-FLOW-ISOLATION-001`: test-source leakage risk через xUnit, Assert, test names и run ids.

Эти строки не должны входить в next DPO authoring input без отдельной anonymized rewrite и повторной review.

## Counts by verdict

| Verdict | Rows |
|---|---:|
| DPO-ready candidate for next authoring only | 3 |
| Evidence-only, needs source acceptance | 2 |
| Merge/drop same-source variant | 2 |
| Quarantine shortcut risk | 3 |
| Reject | 0 |
| Training-ready | 0 |

## Per-machine counts

`both` row counted in both machine views for planning visibility, not as two independent rows.

| Machine view | Candidate | Evidence-only | Merge/drop | Quarantine | Training-ready |
|---|---:|---:|---:|---:|---:|
| PromptToTask | 2 | 1 | 1 | 0 | 0 |
| OrchestrationReasoning | 1 | 2 | 1 | 3 | 0 |

Parent review should treat this as `PASS_WITH_REPAIR`: the required P5C artifacts exist and the no-training boundary is clean, but the next phase must repair/source-accept evidence-only and quarantined material before any real DPO row materialization beyond the three safe candidates.
