# Сверка двух одновременно опубликованных пакетов: kNN, память и перевод

**Planning pin:** `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`, 20.10.2026. Источник — сохранённый GitHub compare `audit_state/planning-final-concurrent-compare.json` (2 commits, 46 files). Проверены все три `routing/*.md`, routing fixture и `61-LEARNING-FIRST-AUDIT-2026-10-20/02-HISTORY-AND-KNN-HYPOTHESES.md`; дополнительно прочитан конкретный E1/E2 overlay и точечно сопоставлены coverage/live-delta документы. Полного независимого повторного аудита всего нового пакета здесь нет. Native эксперименты не запускались, protected не менялся.

**Результат:** появились три внешние конкретные гипотезы, которых нет в нашем T-TRAIN-CONTEXT: KN-ROUTER, H-M1-RETENTION и H-M1-IDENTITY. Их следует включить ссылками как уже опубликованные другими авторами. Наш E2 и входящий E2 частично совпадают до конкретного learned source-span контракта; различается место reader и схема. Минимальный E1 fixture остаётся отдельным уточнением существующей гипотезы, а не новым дефектом.

## 1. Что уже найдено другим пакетом

| Входящая находка | Точный механизм и область | Решение для нашей публикации |
|---|---|---|
| **KN-ROUTER** | `RoutingProfileTrainer.Train` при записи Keywords заменяет C#/.NET на csharp/dotnet, а `BaselineKeywordCandidateGenerator` читает запрос только после lowercase. Воспроизведение соединяет настоящий Train с `TrainableRoutingFlow.RouteAsync`; поправка — симметричная canonicalization reader. C#/NET routing; `CodeDocumentationRoutingFlow` может уже маскировать эффект task slice/hint. [Кодовая карта][router-code], строки 5–37; [малый протокол][router-card], строки 7–50. | Признать отдельной внешней находкой. Не смешивать с POS ContextualDictionary, `PredicateDatasetRouter.test_failed` или neural DPO. Ограничение унаследовано из orchestrator-extensions; причинность конкретного ухудшения RP01 не установлена. |
| **H-M1-RETENTION** | Урок с новым PairId прежнего FlowRef остаётся в KNN, но `PromptFlowGroundingModel.Train` заменяет `_weights[pair.FlowRef]` новыми input features. Предлагается tokenwise-max объединение только InputFeatureWeights. Получатель — настоящий Mode1 `OrchestrationPlanMachine`; последовательное удержание старого маршрута, не LoRA. [History §5][mode1], строки 72–148. | Признать отдельной внешней гипотезой. Это не duplicate PairId/revision P67: PairIds различны. 3 урока на плечо, 6 native Train calls и 40 GeneratePlanAsync включая cold panel — предложенный бюджет, не проведённое измерение. |
| **H-M1-IDENTITY** | Grounding выбирает FlowRef по weights, а окончательный Chosen берётся глобальным Mode1 KNN без этого фильтра. Заголовок flow и тело плана могут иметь разные источники. Единственная предлагаемая поправка ограничивает финальный retrieval grounding.FlowRef до прежнего ранжирования. [History §6][mode1], строки 150–192. | Признать отдельной внешней гипотезой. Не смешивать с RETENTION: один урок на flow уже допускает этот механизм. 2 урока на плечо, 4 Train calls и 24 GeneratePlanAsync — проект. Финальный filter не исправляет одновременно иной global-confidence путь. |

Все три — **source-backed hypotheses / NOT_RUN** по входящим авторам. В этой короткой сверке проверены точные байты документов и их причинные спецификации; заново выкачивать и независимо исполнять каждый процитированный C# метод не стали. Расчёт Jaccard в Mode1 — аналитическое ожидание, не native receipt. Равный или нулевой эффект при достигнутом пути остаётся допустимым исходом.

Входящие KN-ROUTER и H-M1-IDENTITY корректно относятся к использованию приобретённого состояния. Правило 20.10 не требует декоративного дополнительного Train для такого предмета; необходимы настоящий обученный state, тот же panel, exact original method и поправка. Нельзя снова отклонять их только как «query/consumer вмешательство».

## 2. Честная карта пересечений с нашей работой

| Наша часть | Сопоставление с новым опубликованным материалом | Формулировка новизны |
|---|---|---|
| **T-TRAIN-CONTEXT-01** | В scoped routing/Mode1 материалах отсутствует такой опыт. Он меняет сохранение POS в `ContextualDictionary.AddEntry`; query POS одинаковы. Входящая coverage-карта только перечисляет существующий T. KN-ROUTER меняет canonicalization другого learned-state consumer. | Дополнительная локализация известной P43 темы. Не новый алгоритм и не доказанный production caller. Сохраняем две стороны T: действующий consumer опыт допустим; наш writer опыт — отдельный вопрос, не обязательная замена. |
| **E1: минимальные бесшумно/громко, silently/noiselessly** | [Входящий overlay §2][translation-overlay], строки 66–87, уже использует admission `NO_DISTINCT_NEGATIVE`, но проверяет EN→RU ordinary retention через 8 warmup Executor уроков и 2 новых урока. Наша версия — RU→EN, два исходных урока, собственный Verify, без warmup/ordinary Flow claim. Входящий общий обзор оставляет marker reachability открытой. | Гипотеза false-negative admission уже известна. Наше полезное уточнение — достижимые существующим tokenizer markers и компактный компонентный panel. Не называть это новым дефектом или доказанным улучшением полного перевода. |
| **E2-ORDERED-SUPPORT** | Входящий overlay уже задаёт `SourceSpanFrames` из TRAIN: `spanTokenLengths`, gaps, provenance, checkpoint и reader `TryComposeFromPhraseMappings`. Те же два delete/save урока, четыре recomposition/operator задания, 2 batch/4 Unit calls/16 ordinary outputs. Наша `OrderedPhraseSupport/v1` хранит source-phrase→slot membership и observed slot sequence; reader расположен **перед exact/phrase/wordmap/kNN**. [Overlay][translation-overlay], строки 9–62. | Широкая идея и малый материал уже опубликованы; нельзя заявлять уникальное закрытие E2. Сохраняется инженерное различие схемы/области чтения. Наш общий reader устраняет обход через word-map/kNN; входящий вариант оставляет fallback неизменным и честно допускает нулевой final effect. Это два варианта одной известной темы, не два новых открытия. |
| **Checkpoint/state как общий класс** | Mode1 сохраняет pairs/weights; translation E2 сохраняет frames; neural SFT обсуждает optimizer moments. Наличие слова state не делает механизмы одинаковыми. | По scoped файлам Mode1 механизмы не дублируют наш translation/T state. Новизна neural HEAD/AdamW/PAD проверяется отдельно нейронным обзором, здесь не аттестована. |
| **История E09/E26/E43, P125** | Те же основные положительные изменения и оговорки уже присутствуют в incoming history. P125 11/24→19/24, wrong accepts 1→3 — уже реализованный ingress, а не новая задача исправления. | Наш материал — дополнительная проверка источников/знаменателей. Не считать повторное обнаружение этих чисел новой гипотезой. Наши E22 и E34plain/V2 разборы сохраняют дополнительную пользу. |

E2 требует особенно точной маркировки: две положительные команды не обучают смыслу отрицания/вопроса. Снижение wrong accepted за счёт learned-support отказа может быть полезным успехом, одновременно уменьшая coverage. На этом panel новый frame также может дать тот же outcome, что известный P54 coverage guard; уникальное превосходство сложной representation таким сравнением не доказано. Это прямо признаёт и входящий overlay, строки 58–62.

## 3. Полезные новые исторические уточнения

[Входящий routing history][routing-history] добавляет **E25 POS-classification**: опубликованный report accuracy `0.3438→0.875`, 128 TRAIN/32 validation; отображённое время `0.4→15.4 ms` не включает Train. Числа 11→28 правильных совместимы с округлёнными долями, но без raw prediction panel остаются восстановлением знаменателя, не новым первичным наблюдением.

Там же **E07 structural-context** имеет `0.1531→0.1462`, а apparent time `198.8→7.2 ms` получен из различных областей таймеров: baseline включает BPE Train/Encode и AST, enhanced — только MultidimensionalNGram.Train. Это не доказанный выигрыш обучения. E26 receipt `run-2026-05-01-003` с `0.1→0.1 ms` и наш `run-2026-05-01-001` с `0.8→2.2 ms` — разные запуски; общие 15 cases не разрешают смешивать их времена.

Сами входящие документы также **не доказывают точный парный исторический перенос матрица→функция kNN**. Есть подтверждённые различия представлений/доменных потребителей, отдельные улучшения метрик и ограничения измерения. Нельзя превратить это в утверждение, что определённый неудачный matrix run после единственной правки превратился в конкретный успешный kNN run.

## 4. Проверка источника и действие

Из added-file patch восстановлены UTF-8 bytes; повторно вычислен Git blob SHA (`blob <length>\0<bytes>`) и сопоставлен с compare:

| Документ | Git blob SHA |
|---|---|
| routing/HISTORY-AND-DOMAIN-EVIDENCE | `898da49fcf16abf463efd9a77695bf1c48ddd15c` |
| routing/KN-ROUTER-LEARNED-KEY-CANONICALIZATION | `a8e25fe289832af8f2e24eabd6d3c3f571ed0fda` |
| routing/KN-ROUTER-SMALL-LEARNING-EXPERIMENT | `cacb94be413143b0c830588e818065190a8304ad` |
| routing/KN-ROUTER-FIXTURE.json | `3ed0bf393dbf463e8b50236e6edd131d4cda0ca7` |
| 61/02-HISTORY-AND-KNN-HYPOTHESES | `1cbc1eb240423888030a729628cc61761a2d0a4e` |
| coverage/02-N60-CONCRETE-OVERLAYS | `5c8019187b3d29d17de15ac03af9d2b4be6079d2` |

В master/crosswalk добавить три incoming ссылки; E2 обозначить параллельной конкретизацией с явной разницей reader; E1 оставить минимальным исправлением fixture существующей карточки. Новых runner, карточек и обязательных прогонов эта сверка не добавляет. Любой будущий измеренный полезный выигрыш считается успехом своей метрики/домена без обязательного роста accuracy; остальные показатели и ухудшения публикуются рядом.

[router-code]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/KN-ROUTER-LEARNED-KEY-CANONICALIZATION-2026-10-20.md
[router-card]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/KN-ROUTER-SMALL-LEARNING-EXPERIMENT-2026-10-20.md
[mode1]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-FIRST-AUDIT-2026-10-20/02-HISTORY-AND-KNN-HYPOTHESES.md
[translation-overlay]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/coverage/02-N60-CONCRETE-OVERLAYS.md
[routing-history]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/routing/HISTORY-AND-DOMAIN-EVIDENCE-2026-10-20.md
