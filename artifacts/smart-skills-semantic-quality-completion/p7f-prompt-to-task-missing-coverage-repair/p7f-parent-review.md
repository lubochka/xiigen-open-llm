# P7F parent review

Вердикт: **принято к phase commit как repair-кандидат, не как финальный проход P7**.

Что проверено:

- В checkpoint-кандидате 7 learned patterns.
- Новые P7F rows являются train-only repair material, а не eval/holdout rows.
- В checkpoint signals нет row id, source id, file path, eval/holdout markers или exact prompt authority.
- Две провалившиеся в P7E комбинации не скопированы как exact learned signal set.
- Focused test прошёл: `PromptToTaskP7FMissingCoverageRepairTests`, 1 passed / 0 failed.
- `git diff --check` прошёл на P7F artifacts, focused test и state.
- True-claim scan не нашёл `semantic quality`, `P7 pass`, `API ready` или `final model ready` как положительные claims.

Почему это принимается:

P7F исправляет конкретную дыру P7E: у PromptToTask не хватало общего train-only материала для поведения, похожего на `P5H-DPO-PTT-002` и `P5H-DPO-PTT-005`. Сабагент добавил две независимые строки из локальных источников, очистил их до универсальных правил постановки задачи и недостающего контекста, затем собрал отдельный checkpoint-кандидат под P7F.

Что это **не** доказывает:

- Это не финальная модель.
- Это не общий pass semantic quality.
- Это не доказательство readiness API/extensions.
- Это не разрешение перезаписать common/final checkpoint.

Следующий шаг после commit/push:

`P7G` должен формально переиграть source-disjoint eval + ablation confirmation уже против P7F candidate checkpoint. Только этот отдельный шаг может решать, закрыта ли PromptToTask P7-дыра, и он всё равно не должен заявлять readiness для всего API.
