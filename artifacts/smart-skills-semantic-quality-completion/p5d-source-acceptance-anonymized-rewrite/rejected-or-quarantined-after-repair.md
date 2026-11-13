# P5D rejected or quarantined after repair

Статус: это объяснение блокировок после source-acceptance/anonymized rewrite review. Ни одна строка не стала training-ready.

## Quarantine: station-chain flow

Row: `P5A-CAND-ORCH-NINE-NODE-FLOW-001`.

Почему все еще заблокирована: после удаления code identifiers остается полезная, но слишком близкая к уже принятой branch/review/targeted-repair границе тема station-chain flow. Если продвинуть обе строки сейчас, future authoring легко выучит одну и ту же flow/station идею дважды или вернет exact-count/code-shape shortcut через названия станций.

Что можно безопасно оставить: evidence note для parent review. Позже parent может принять ее как held-out/eval-style boundary только если явно разведет две роли:

- multi-branch source teaches branch separation, pre-format review and targeted repair;
- station-chain source teaches evidence carried across ordered stations, without exact station count or code-shaped labels.

Что нельзя делать сейчас:

- не включать как next authoring candidate;
- не использовать exact station count;
- не использовать class/file/method names, node ids, bridge names, metrics keys or async/code envelope;
- не считать source availability source acceptance for training.

## Почему остальные четыре строки не training-ready

Четыре строки получили `DPO_READY_CANDIDATE_FOR_NEXT_AUTHORING`, но это только право на будущую redacted authoring phase:

- `P5A-CAND-BOTH-SKILLS-INVENTORY-001`;
- `P5A-CAND-ORCH-DOC07956-001`;
- `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`;
- `P5A-CAND-ORCH-FLOW-ISOLATION-001`.

Они остаются неготовыми для training because:

- analysis-state observed state still says `todo`;
- parent review must accept named-reference use before materialization;
- future authoring must create final chosen/rejected rows and source-disjoint grouping;
- ablation controls have not run;
- no eval, training, checkpoint rewrite, fresh-load or continue-training gate has run.
