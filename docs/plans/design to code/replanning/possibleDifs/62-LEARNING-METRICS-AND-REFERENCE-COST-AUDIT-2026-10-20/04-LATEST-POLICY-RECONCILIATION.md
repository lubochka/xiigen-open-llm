# Дельта protected-каталога перед публикацией: f5dd532, 20.10.2026

**Последующая актуализация:** [финальная a822-дельта и точные experimental copies](05-CONCURRENT-PUBLICATION-AND-EXPERIMENT-COPIES.md). Этот документ сохраняет указанный ниже снимок; общий approval gate не возвращается.

Основной source-аудит сохраняет pin `bf9b543e149ef197da85a767821d98c9b67916d2`. Перед публикацией прочитано обновление core до `f5dd532045716495a2c7668573250e5d7cc46810`: commits a318d137, de4ef0f1, f5dd5320. Сохранённый parent compare показывает 152 изменённых файла, все внутри `docs/plans/replanning-possible-diffs/`; нативный код этой дельтой не изменён. Это не основание повторять source-аудит математики. [Сравнение revisions][compare].

Этот адресный review прочитал **10 текущих документов**: execution policy; experiment README; малые README/method/neural04/N60-23; ближайшие владельцы P09/P78/P110/P119. Это проверка релевантного изменения покрытия, **не** новый полный семантический аудит152 файлов. Git blob SHA, changed hunks и поисковое свидетельство сохранены в [TARGETED-DELTA-EVIDENCE.json](TARGETED-DELTA-EVIDENCE.json); общая граница изменений — в [core-head-delta-manifest.json](core-head-delta-manifest.json). Protected файлы этим аудитом не изменены.

## 1. Вывод для трёх нейронных тем

| Тема пакета61 | Что в актуальном protected | Решение |
|---|---|---|
| Frozen-reference score reuse внутри повторного DPO update | Нового cache/memoization/precompute рецепта нет. P09 по-прежнему full/cached generation; PAD в N60 режет вычисляемый хвост, сохраняя reference forwards | Новизна кандидата в проверенном corpus сохраняется. Цена actual update допускается без обязательного Generate. |
| Pre-update trainer metrics рядом с post-update checkpoint | В N60-23:166 отмечен gradient доApplyOptimizer/ZeroGrad и Δweights после; нет временного контракта LastLoss, weighted after-evaluator или contrast pre/post monitor | Это по-прежнему prior54 + недостающее конкретное protected покрытие; не новый общий факт. |
| Default window25 при cap5 | P78.1 остался window1 против5, max5, threshold.002, тот же DefaultLogical caller | Уточнение другого caller/default trigger не добавлено. Сохраняется статус prior35/38/P78 refinement, без заявления о блокировке обучения. |

[P78.1 current,57–64][p78]. [N60 PAD,158–170][pad]. Точный targeted search: reference-score-cache matches0; window25 matches0; phase matches1 — только приведённый gradient/Δweights абзац. Нулевой текстовый match сам по себе не доказывает новизну; verdict опирается также на смысл владельцев.

## 2. Шесть B-bindings не материализованы, но это уже не шесть approval-blockers

Current neural04 сохраняет таблицу B1–B6. Технические значения в ней не появились:

| Поле | Текущее состояние |
|---|---|
| B1 — конкретный repair factor | Всё ещё не выбран в общей D-карточке. Именованные N60-рецепты дают отдельные факторы, но не автоматически закрывают любой D-run. |
| B2 — tiny native caller | `micro-paired` остаётся проектом; указаны original dispatch/ValidOptions/admission seams, существующая CLI-команда не заявлена. |
| B3 — runtime bytes/admission | Полные тексты заданы, materialized files и bindings отсутствуют. |
| B4 — actual B0/reference files | Config/tokenizer SHA и source constants известны, свежая проверка доступных actual weights/manifest по конкретным путям не выполнена. |
| B5 — optimizer/RNG state | Конкретные settings/state/cursors для обоих arms не материализованы. |
| B6 — actual call/output ownership | Итоговый command/output/private run/files остаётся подготовить после реализации нужного caller. |

[Таблица64–71][bindings]; [не существующая пока micro-paired команда73–93][micro]; [исходные config/tokenizer и непроверенные weights/state29–36][initial].

Новая политика прямо меняет смысл статуса: исполнитель сам выполняет доступную подготовку, не перекладывает её на пользователя и не ждёт одобрения каждого ID/SHA. Незаполненная таблица не доказывает внешнего блокера. `BLOCKED` требует конкретной неудачной попытки и объяснения, почему доступная альтернатива не решает задачу. [Policy19–25][readiness].

В пакете61 различаются: `PREPARABLE` — надо реализовать предусмотренное небольшое соединение или оформить доступные данные; `NEEDS_INPUT` — конкретный обязательный артефакт недоступен после адресного поиска; `NOT_RUN` — только отсутствие измеренного результата. Никакой из этих статусов не создаёт нового общего запрета уже порученной программы.

## 3. Правила, учтённые в пакете61

1. **Не повторять личный approval-gate.** Разрешение текущей программы действует между сообщениями/исполнителями. Поля паспорта нужны для проверяемости; выбор доступного подслучая и обычная подготовка — работа исполнителя. [Policy3–13][authority].
2. **Не исключать классы serving/cold/scorer по названию.** При доказанной зависимости обучения либо использования выученного результата от загрузки, tokenizer, restore или consumer допустим соответствующий A/B. Обязательны exact file/method, причинная цепь, конкретная неисправность и оригинальное исправление. [Policy29–33][learning-chain].
3. **Не добавлять лишние updates.** Для restore/consumer можно взять один доказанно обученный checkpoint и одинаковую панель до/после исправления звена. Это проверка сохранности/использования результата, а не новое обучение. Для самого learning-factor сохраняются настоящие A0/A1/B0/B1. [Policy33][learning-chain].
4. **Не делать generation условием cost-опыта.** Actual DPO/SFT update имеет самостоятельные полезные ресурсы. Если ответы не измерялись, accuracy пишется NOT_MEASURED, не0/unchanged. Current PAD уже имеет основной бюджет4updates и0Generate; +16answers — отдельное необязательное расширение. [Policy34–35][resources]; [PAD164–170][pad].
5. **Не превращать общий success rule в accuracy gate.** Любое полезное измеренное улучшение — успех своей оси и домена, регрессии рядом; корректировка измерителя может улучшить достоверность, но не физическую скорость. [Method5,15–19][method].
6. **Роли будущего executor/reviewer сохранять точно.** В policy заявлены gpt-6-astra/ultra и не более одного активного субагента. Это требование к будущему исполнению; документный аудит не должен задним числом заявлять запуск такой конфигурации. [Policy36–38][roles].

В текущем04 ещё остались старые фразы «Когда Люба проверила…» и «обновлённое решение Любы» в prompt137–139, а D-context175/192 всё ещё несёт прежнюю широкую OUT_OF_LEARNING_SCOPE формулировку. **Header3 и централизованная policy явно отменяют такие старые общие ограничения.** Это локальная редакционная неоднородность, не действующий запрет. При использовании этих рецептов нужен адресный trace согласно новой policy; нельзя ни автоматически объявить весь D-context готовым, ни исключить его только из-за нулевого update. [Current04 header][neural-head]; [оставшиеся формулировки137–145][stale-prompt]; [D-context175–194][context].

## 4. Что действительно добавлено в N60

N60-23 расширен со171 до317строк. Добавлены конкретные E1/E2 translation/Verifier/Executor предложения и адресная таблица оставшихся N60 условий; поздний section265–279 фиксирует новый read-only code review на a318d137. Он подтверждает source/config scope и честно оставляет runtime/artifact/host измерения открытыми. Это улучшение подготовки/покрытия, не выполненные native results. [N60 адресная перепроверка265–279][recheck].

Для нейронных рецептов существенна компактность PAD: новая версия убрала обязательные генерации из ресурсоёмкого сравнения и показывает exact update counters. Source API для MASK/STATE/ORDER/PAD и actual tiny host всё ещё требуют указанной ограниченной реализации. Независимые эксперименты не должны становиться обязательными этапами друг друга.

Эта дельта **не закрывает** cache/phase/window25 конкретными измерениями и **не меняет** выводы source-аудита bf9b. Она требует исправить язык готовности и границ допустимой учебной цепи, а также сохранить маленькие бюджеты.

[compare]: https://github.com/lubochka/llm_mvp_core/compare/bf9b543e149ef197da85a767821d98c9b67916d2...f5dd532045716495a2c7668573250e5d7cc46810
[authority]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L3-L13
[readiness]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L19-L25
[learning-chain]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L29-L33
[resources]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L34-L35
[roles]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/00-execution-policy-2026-10-20.md#L36-L38
[method]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/01-method-and-report.md#L5-L19
[initial]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L29-L36
[bindings]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L64-L71
[micro]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L73-L93
[neural-head]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L3-L11
[stale-prompt]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L135-L145
[context]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L175-L194
[pad]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L158-L170
[recheck]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L265-L279
[p78]: https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/docs/plans/replanning-possible-diffs/experimets/cards/P78-loss-plateau.md#L57-L64

