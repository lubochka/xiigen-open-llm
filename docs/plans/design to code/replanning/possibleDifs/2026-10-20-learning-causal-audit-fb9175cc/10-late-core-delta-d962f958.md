# Позднее уточнение: core d962f958 и уже выполненная часть N63C

> **Конечный source cutoff —256f603e.** [Последнее уточнение](11-final-cutoff-256f603e.md) отдельно учитывает новый N63C-PROBE, уже признанный core candidate и текущий recovery protocol. Исходный partial A и новый A/B probe не смешиваются; новые запуски здесь не назначены.

**21 октября2026; документация и чтение сохранённых результатов. Новых Train/inference/build/load/повторов —0.** Пока пакет проходил публикационную проверку, source core получил [внешний commit d962f958](https://github.com/lubochka/llm_mvp_core/commit/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19) с сообщением «10th experiment middle results and plan conflicts resolution». Его parent — fb9175cc. Наш основной source audit и1324-file inventory остаются привязаны к прежнему снимку; этот раздел согласует фактическое обновление, не переписывая историю.

## 1. Актуальная очередь и граница полномочий

Свежий [canonical index](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md) назначает **№10 PARENT-ADOPTION-01 → №11 N63-INGRESS-LENGTH-LEARNED-01 → №12 N63-CORRECTION-COLLISION**. Три прежних предложения уже включены как№43 DPO-SHIFTED-CONTEXT-01,№44 IDENTITY-TRAIN-INDEX-01,№45 SFT-EXP-REUSE-01. Поэтому current overlay содержит47 coreID+2 новых=49 uniqueID, а не47+5. Зависимые consumers остаются последними:№46 IDENTITY-M1-01,№47 KN-ROUTER-LEARNED-KEY-01. [Точный реестр и переходы](04-registry.md), [машинная версия](data/resolved-dispatch.json).

№08: attempt consumed, learning objective OPEN. Назначенный corrected№11 не удаляет старую попытку и не разрешает скрытый повтор. У N63C теперь сохранён частичный A и execution claim; это отдельный факт от pending статуса его A/B цели. Указание retired N63R сразу идти в N63C противоречит свежему index и не позволяет обойти PA01/№11. Конкретные строки — в [delta review очереди](evidence/core-delta-coverage.md).

В новых SC01/SEXP документах встречаются предписания отменить входящий запрет правок protected. Для **этого поручения** они неприменимы: прямое правило пользователя «эту папку менять нельзя» остаётся в силе; изменения предлагаются только здесь, в possibleDifs. Ни чужая карточка, ни source contract не расширяют выданные полномочия. Live STOP/NEXT имеет немедленный приоритет; для одного назначенного ID NEXT=END.

## 2. N63C: реальные updates нельзя обнулять из-за позднего Save failure

Новый [report](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/report.md), [raw A](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/A/arm-result.json) и [failure](https://github.com/lubochka/llm_mvp_core/blob/d962f95805ae0c9c4ef5b3f50f0f65b2a4109d19/experiments-v2/RP01-MICRO/N63C/results/runner-failure.txt) показывают: **A выполнило1DPO+1SFT+18 side-score; B не начиналось**. После S2 сохранились веса и optimizer, затем native RNG writer отверг имя `after-sft.rng.json`. Recorder обнулил dpoUpdates/sftUpdates потому, что финальное error неnull, хотя callbacks/returns иS2 доказывают исполнение обоих шагов. Это ошибочная отчётность; она не превращает обучение в «не запускалось» и не разрешает retry.

На T1 измерены полезные изменения:

| Показатель | S0→S1, DPO | S1→S2, последующий SFT | S0→S2 |
|---|---:|---:|---:|
| Chosen NLL, меньше лучше | −0.002092 | −0.0022153 | **−0.0043073** |
| Margin, больше лучше | +0.00345611 | **−0.00047875** | +0.00297736 |
| Mean gold rank, меньше лучше | −0.17391304 | −0.26086957 | −0.43478261 |

Любое измеренное улучшение полезного показателя считается успехом **этого показателя и домена**; рост accuracy для этого не обязателен. Q1/Q2 deltas и все регрессии приведены в [полном readback](evidence/core-delta-n63c.md). Chosen response tokens остаются0/23,0/29,0/21 во всех состояниях; приобретение или стирание навыка не доказано, а безB не установлен эффект замены ошибочного SFT текста правильным. Фраза исходного report «полезного измеренного выигрыша нет» чрезмерна: NLL/rank/margin наблюдения должны сохраняться кандидатами, с этими ограничениями рядом.

## 3. Кандидаты исправления существующего N63C, без нового запуска

| ID кандидата | Подтверждённый разрыв | Назначенное исправление для будущего решения пользователя |
|---|---|---|
| N63C-RNG-SAVE-CONTRACT | Карточка и Bridge требуют after-sft.rng.json; native WriteCreateNew разрешает только BuildFileName(step). Это разрыв сохранения состояния уже выполненного обучения. | В лабораторном Bridge использовать отдельный `armRoot/after-sft/` и `RepositoryOwnedDpoRandomStateFile.BuildFileName(1)`; DPO RNG в checkpoints/ не перезаписывать. Native production validation не ослаблять. |
| N63C-STAGE-ACCOUNTING | Поздняя ошибка записывает0 updates; saveCost0 маскирует уже выполненные записи. | Фиксировать started/completed/unknown-partial каждого шага сразу после фактического return, независимо от final error; Save cost при неполном измерении маркировать unknown/partial, не0. |
| N63C-SCORING-COST-SCOPE | CPU/allocations scoring включают snapshots/setup/encoding между ScoreAll, тогда как wall timer приостановлен. | Отдельные CPU/alloc snapshots на каждом измеряемом вызове, суммировать только одинаковую область; setup/snapshot стоимость показывать отдельно. Старые суммы не выдавать за чистую цену панели. |
| N63C-PARTIAL-A-METRIC-CANDIDATE | Report исключил useful-metric успехи из-за0correct и отсутствующегоB. Финальные state/base/reference checks после RNG-save не выполнены. | Добавить в отчёт перечисленные NLL/rank/margin улучшения и регрессии; отдельно оставить A/B objective OPEN, ZERO_TO_ZERO limitation и unavailable post-SFT state proof. Не достраивать raw обещаниями и не запускать повтор ради report. |

Это **кандидаты ремонта протокола/лабораторного recorder и интерпретации уже сохранённого обучения**, не новые synthetic IO experiments. Конкретные файлы, строки caller→writer→state, параметры исправления и разграничение фактических стадий даны в [N63C source review](evidence/core-delta-n63c.md). Source report и protected карточка не меняются. Нынешняя работа ограничена добавлением доказательств и кандидатов; внедрение и дальнейшее исполнение решает пользователь.

## 4. Parent repair и новые canonical recipes пока не исполнены

PA01/SC01/SEXP source contracts имеют `DESIGNED_NOT_MATERIALIZED_NOT_RUN`. Parent blocker получил конкретный H16 accepted SFT writer/selector/session/handoff рецепт с честным разделением acceptedOrdinal и optimizerStep. Но physical module/ABI/load/full m-v equality/handoff ещё не подтверждены. Нельзя назвать их готовыми, взять planned budgets за actual updates или приписать новому PA01 исторические3/40→4/40 tokens. Whole-answer0/2→0/2 по-прежнему не закрывает пользовательский пункт15; полезная NLL/resource метрика и подтверждение усвоения задачи — отдельные статусы. [Подробное сравнение источников](evidence/core-delta-parent-sft.md).

SEXP повторно использует Exp в `Trainer.BackwardLoss`; новая SiLU карточка сохраняет activation denominator между forward/backward в `TensorOps.Silu`. Это разные операторы, размеры и время жизни памяти. HNSW query norm находится в non-neural Unit.Train. Две новые гипотезы не поглощаются включением SEXP и остаются AUTHOR_SETUP_REQUIRED/NOT_RUN. Source pins и предписанные небольшие бюджеты новых карточек сохраняются; новая очередь согласована без замены моделей и donor за пользователя.

## 5. Что осталось актуальным из исходного аудита

- Недостающий local useful-metric rule у consumed N63I/N63R остаётся; историческое169/171 не переименовано в171PASS. [Раздельные addenda](06-consumed-card-rule-addendum.md).
- В N63C требование C/T/E/N/M и неполная ready table margin/NLL сохранились; наша [таблица](03-card-contract-audit.md#n63c-report-table) актуальна для№12 и не требует новых forward.
- Исходные saved gains/regressions, sibling evidence и source mechanisms не переатрибутируются новому commit как новым запускам. Описания старых snapshots имеют явную историческую границу.
- Старая карточка N63C ещё утверждает, что host не материализован/результатов нет; это противоречит сохранённому частичному A. Отчёт фиксирует расхождение, не стирая исходную карточку.

## 6. Объём дополнительного чтения

[Список GitHub compare](data/core-delta-files.json):213 путей,80 изних protected docs, остальные — лабораторные N63C source/artifacts; canonical production paths не изменились. Fresh index/cards/contracts прочитаны адресно; source blobs и строки новых readback зафиксированы в трёх отчётах: [queue review](evidence/core-delta-coverage.md), [native N63C](evidence/core-delta-n63c.md), [parent/SFT](evidence/core-delta-parent-sft.md). Это ограниченная дополнительная проверка, не новый полный inventory всех protected файлов и не runtime validation.
