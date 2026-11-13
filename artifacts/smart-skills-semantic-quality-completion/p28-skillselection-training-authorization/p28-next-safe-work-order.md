# P28 - следующий безопасный work order

## Вердикт для следующего шага

Training сейчас не разрешен, поэтому следующий work order не должен обучать модель. Безопасный следующий slice: P29 `SkillSelection repair source-disjoint training batch gate design`.

## P29 work order

Роль: `EXECUTOR_INTERNAL_BOUNDED` под parent architect/reviewer.

Максимальное активное время: 15 минут.

Текущая цитата Luba: `"Продолжай исполнение плана через суб агентов"`. Она разрешает продолжать план через суб-агентов, но не разрешает training сама по себе.

Тип артефакта: non-training SkillSelection source-disjoint split and training authorization gate design.

Allowed write scope:

- `artifacts/smart-skills-semantic-quality-completion/p29-skillselection-repair-training-gate-design/**`
- `docs/plans/SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json` только P29 block/top status/next_action.

Запрещено:

- training;
- model eval against checkpoints;
- checkpoint mutation;
- common model replacement;
- promotion;
- code/test changes;
- `dotnet test`, `dotnet build`;
- commit, push, branch switch;
- external AI/network.

## Что P29 должен сделать

P29 должен подготовить gate package, без которого будущий candidate-only training/eval slice нельзя безопасно выдать:

1. Source-disjoint split design для 8 P27 rows.
2. Training batch authorization matrix: какие approval/state flags нужны до training.
3. Batch card: local-private, candidate-only, `commonTrainingAllowed=false`.
4. Leakage gate plan: exact prompt, source id, path, filename, pair id, row ordinal, candidate order, selected-skill position, target adapter shortcut.
5. Future P30 training/eval gate list: fresh-load, continue-training from existing checkpoint, export/import, ablation, dashboard/runtime evidence, no promotion.
6. Review cycles, self-check, evidence card.

## Обязательная честная граница

P29 не должен превращать 8 P27 candidate rows в training set. Он должен явно сказать, достаточно ли этих строк только для дизайна gate или для будущей candidate-only training attempt, и какие условия нужны перед запуском.

Если после P29 training все еще не авторизован, следующий шаг остается parent review/commit boundary или другой safe non-training gate. Если Luba/parent позже явно авторизует training, нужен отдельный P30 work order.
