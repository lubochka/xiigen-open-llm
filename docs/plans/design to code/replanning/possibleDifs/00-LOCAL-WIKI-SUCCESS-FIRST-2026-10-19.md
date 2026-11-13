# ПЕРВАЯ ГЛАВА: вернуть реально работавшее обучение из local_wiki_planning

**19.10.2026 · Priority P0 · исследование первичных исходников и сохранённых артефактов. Нативный .NET/Student A/B в этом проходе НЕ запускался.**

Это отдельный обязательный приоритет к уже существующим главам о `language_translate`, `PREPARETOGPL` и других успешных ветках. Не заменять его восстановлением неопределённого «общего DPO» и не переносить исторические проценты между разными получателями обучения.

**Донор:** [`llm_mvp_core@local_wiki_planning`](https://github.com/lubochka/llm_mvp_core/tree/local_wiki_planning), проверенная вершина `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a`. **Цель:** [`codex/dpo-implementation`](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation), включая научный RP01. Подробности/быстрые протоколы: [пакет 49](49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/README.md). Второй репозиторий `llm_mvp_core/docs/plans/replanning-possible-diffs` **не изменять**.

## Восстановительная очередь — сначала те способности, у которых есть первичный положительный архив

| Очередь | Донор, подтверждённый сохранённым артефактом | Чего нельзя терять | Первый различающий опыт |
|---|---|---|---|
| **P0-A** | **CodeTraceBuilder batch064**: 50 TRAIN + 50 heldout **разных** DPO-триплетов, DPO heldout **0,92**, публикация состоялась | `pair.A/Chosen/Rejected` как предметный вход; отличающиеся caller/callee/value-flow/terminal return | Один и тот же heldout C# корпус: исходные содержательные DPO-пары против нынешних проекций только по `RejectedType`, затем проверка реальной трассы |
| **P0-B** | **QueryPlanning batch134**: DPO heldout **1,00**, публикация состоялась | Согласованные порядок запросов и все границы client→API→DTO→server→response; при этом **100 архивных строк содержат только один unique chosen и один unique rejected**, а не 100 законов | Контрфактические противоположные C#/Angular/.NET вопросы: меняет ли модель план в зависимости от входа при одинаковых маркерах сторон |
| **P0-C** | **RequestRouter batch092**: предметная accuracy **1,00**, но DPO heldout **0,66**, **не опубликован** | Не принимать зелёную предметную метрику за опубликованный нейронный навык; нынешняя DPO-проекция router игнорирует конкретный prompt/маршрут | Проверка пары одинаковых типов дефекта с разными целевыми маршрутами; затем отдельная независимая semantic gate и фактический consumer |
| **P0-D** | У донора свой обучаемый lexical perceptron и другие Wiki Units; у RP01 — отдельный `B0 → exact pairs → q/v-LoRA` Student | Доказать `lesson→changed state→fresh process→ordinary answer` именно у нужного получателя; JSON manifest/guardrail/score сами по себе недостаточны | Четыре плеча: old Unit, current Unit, Student, Student+Unit; один вход, тот же закрытый смысловой экзамен |
| **P1** | `language_translate` E25/E26/E09/E43 и современные positive translation checkpoint receipts (из предыдущих пакетов) | POS/AST, контекст слов, memory/replay, kNN и реальные ограничения переноса, отдельно от Wiki CodeTrace | Профили RU→EN/EN→RU и C#: контекст/порядок/syntax-aware kNN только там, где применимы |

### Самая важная выявленная потеря предметного сигнала

В `local_wiki_planning/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs` исходная сторона `pair.A` и обе реальные стороны `pair.Chosen/Rejected` идут в DPO scorer. В `codex/dpo-implementation` вместо них передаются `BuildMetricPrompt(pair.RejectedType)` и статические `BuildChosenSignal/BuildRejectedSignal`. [Донор: строки 119–120](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120) · [цель: строки 124–129](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L129).

Это **подтверждённое изменение обучающего входа DPO-scorer**. Оно **пока не доказано** как единственная причина плохого ordinary RP01: RP01 — другая цепочка `CodeDocumentationSimulationDpoExperiment`; значение получает конкретный вызвавший pipeline. Общая потеря source→pair→recipient — главный кандидат для короткого переноса, но смешивать потребителей нельзя.

Ещё одно прямое несоответствие: текущий RequestRouter DPO формирует `CODE_TRACE_DPO` и `PREFERRED_CODE_TRACE_OUTPUT` **из `RejectedType`**, не читая `pair.Prompt`, `pair.Chosen` и `pair.Rejected`. [Донор](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L110-L116) · [нынешняя проекция](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L131-L167). Детальный механизм и опыт: [49/02](49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/02-VERIFIED-DIFFS-AND-NOVEL-HYPOTHESES.md).

## Жёсткое правило проверки «вернули то, что работало»

1. Поднять **фактический archived donor checkpoint/corpus/version**; не приписывать содержимое neural `policy_final.bin` опубликованному JSON `policy.bin`.
2. Сопоставить `DPO pair → фактически видимые токены/признаки → получатель обновления → прочитанное состояние → конкретный потребитель`.
3. Выполнить маленький **same-case A/B с единственным изменением механизма**, с negative controls и удержанной по семействам контрольной панелью.
4. Оценить одновременно DPO preference, предметную correctness, полноту трассы/плана, отклонение неверных ответов и **обычный ответ после холодного восстановления**; denom и бюджет одинаковы.
5. Явно разделить C# AST/синтаксический порядок, RU/EN POS/морфологию, Wiki retrieval и универсальные границы состояния. Не переносить доменную функцию на все темы обучения.
6. Если Student не использует кNN/Unit, улучшение этих механизмов не засчитывать как исправление Student-only; отдельно проверить разрешённую интеграцию.

**Границы:** архивный DPO preference/primary gate ≠ реальное свободное нейронное обобщение; по одним только source+JSON нельзя объявлять достигнутый прирост качества, истинность causal claim или готовую публикацию. Новых запусков .NET/LoRA/кNN, независимой CODEX/Astra проверки, изменения `llm_mvp_core` не производилось.

### Откуда взяты числа
[CodeTrace сохранённый запуск](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/project-wiki-training-loop-result.json) · [QueryPlanning сохранённый запуск](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/project-wiki-training-loop-result.json) · [RequestRouter сохранённый запуск](https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/project-wiki-training-loop-result.json). Проверка кардинальности DPO JSONL и источников: [49/01](49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/01-DONOR-RECEIPTS-AND-PAIR-CARDINALITY.md).
