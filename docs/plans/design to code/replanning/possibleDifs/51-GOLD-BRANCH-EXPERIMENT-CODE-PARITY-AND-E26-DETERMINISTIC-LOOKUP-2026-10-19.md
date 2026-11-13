# N51 — direct cross-branch verification: positive experiment code already exists; E26 is hard-coded lookup, not proof of learning

**Дата:** 2026-10-19. **Метод:** независимое получение `fetch_file` в `llm_mvp_core@external_domains` и `codex/dpo-implementation`, точная проверка текста файлов за вычетом 5 строк заголовка. **Статус:** `SOURCE_EQUALITY_VERIFIED`; `RUN_COMPARED_NOT_RUN`; `CONSUMER_PARITY_NOT_TRACED`.

## 1. То, что НЕ потерялось в коде

| Компонент | Gold blob SHA | Current blob SHA | Результат точного сравнения |
|---|---|---|---|
| [E09 structural kNN](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E09StructuralKnn.cs) / [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E09StructuralKnn.cs) | `a11af3dbadf2` | `13b8124bf608` | После удаления current header L1–L5: **весь исходный текст идентичен** |
| [E26 contextual dictionary](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) / [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) | `830e24e78690` | `cce1972b86c4` | То же |
| [E43 replay buffer](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs) / [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs) | `5de76a291282` | `0eb6438459e7` | То же |
| [StructuralKnn production predictor](https://github.com/lubochka/llm_mvp_core/blob/external_domains/12.SemanticCompiler/Prediction/StructuralKnn.cs) / [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/12.SemanticCompiler/Prediction/StructuralKnn.cs) | `f5132ce48000` | `b3aa5d376be3` | То же |
| [HnswIndex](https://github.com/lubochka/llm_mvp_core/blob/external_domains/06.VectorSearch/HnswIndex.cs) / [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/06.VectorSearch/HnswIndex.cs) | `c8e547a7ddfd` | `03287c05bcd6` | То же |

**Суть:** существование исходного кода E09/E26/E43 в нынешней ветке **подтверждено, отсутствие или ухудшение этого кода опровергнуто**. Сохранённость файлов ничего не говорит о том, используются ли их результаты в RP01, какой именно asset сериализуется, вызывается ли текущий обучаемый Unit и какой обычный response получает пользователь. Задача — **wire/learning recipient parity**, а не «переписать старый E09».

## 2. N51-A: исторический E26 — полезный контекстный алгоритм, но не обучаемый словарь в данном эксперименте

Прямой источник: [E26 Run L15–42](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L15-L42) вызывает `ContextualDictionary.BuildDefault()` и 15 встроенных `BuildTestCases()` [L65–93](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs#L65-L93). **Никакого вызова Train в E26.Run нет.**

Контекстное содержимое [`Infrastructure/ContextualDictionary.cs L23–58`](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L23-L58) создаётся вызовами `AddEntry(new(...))` с заранее заданными словами и POS-тегами. [`TranslateWithContext L61–79`](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L61-L79) выбирает ближайшую по штрафу `ContextDistance` запись с tie-break по `Priority`, [L93–112](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L93-L112). Это **контекстно обусловленный nearest-neighbour lookup по заранее загруженным записям**, не доказанное online-learning на неизвестных словах, не нейросеть, не general sentence translation.

**Новая точная постановка переноса:** сохранить полезную функцию контекстного расстояния и динамически **пополнять learned store** на проверенных новых примерах, с сохранением/reload и cold query в новом контексте. Эта программа может улучшить перевод; *старые 40→100 на 15 заранее введённых случаях сами по себе этого не доказывают*.

## 3. N51-B: E09 всё ещё содержит несопоставимые denominators в двух ветках

[E09 gold L45–72](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L45-L72): baseline оценивается по `val`, enhanced по `val.Take(500)`. Это **старое** расхождение, оно не было внесено переносом; код совпадает с current после лицензии. [Исторический положительный JSON](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) сохраняется как свидетельство перспективного AST feature, но не matched causal gain. Для A/B применить одинаковые case IDs и одинаковую метку; не переписывать исторический score.

## 4. N51-C: E43 — новый bigram из смешанных данных, не retention нынешних LoRA весов

[E43 gold L29–50](https://github.com/lubochka/llm_mvp_core/blob/external_domains/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L29-L50) строит `baseModel=BuildBigram(oldTrain)`, отдельно `noReplayModel=BuildBigram(newPairs)`, отдельно `replayModel=BuildBigram(newPairs + replay)`. Это честный положительный **retention experiment для нового bigram со смесью наблюдений**, но **не обновление и сохранение того же самого параметрического состояния LoRA** и не тест checkpoint restore. Дополнительный контроль для реальной LoRA/Units обязателен.

## 5. N51-D: файл E26 есть, а исторического `27.TranslationDomain/TranslationTrainedUnits.cs` в gold branch нет

Проверка по путям: [`external_domains` root](https://github.com/lubochka/llm_mvp_core/tree/external_domains) не содержит проекта `27.TranslationDomain`; в [current](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs) он есть. Поэтому не приписывать майскому E26 возможности позднего `TranslationTrainedUnits`. Возможна **новая потеря при переходе от исторического примитивного полезного lookup к большому trainable pipeline**, но это *вопрос к текущему consumer*, а не «из gold пропал готовый persisted Unit». Связь E26→новая learned функция должна быть доказана explicit call graph/experiment, а не одинаковым словом `translation`.

## 6. Самый короткий причинный опыт

**MICRO-N51-1 (read-only, ≤нескольких входов):** на gold и current загрузить **те же** исходные E09 E26 E43 inputs и проверить, что результаты одинаковы при фиксированном семействе входа/seed; если отличаются, искать dependency/runner или input lineage. Исторический источник одинаковый по телу, поэтому различие результата требует другой составляющей. Нельзя claim `RUN_PASSED`, пока в действительности не выполнено.

**MICRO-N51-2 (E26 unknown word):** A — вызвать `BuildDefault().TranslateWithContext` на невиданной паре source/context; B — через `AddEntry` добавить проверенное новое значение; C — пересоздать `BuildDefault` (cold restore) и повторить. Ожидаемое по исходному коду: без собственного save/load новая запись **не переживёт новую инстанциацию**. Потом минимально подключить persisted learned translation store и сравнить `A / B-before / B-after-restart` с 12 unseen examples + negative same-token-order perturbation.

**MICRO-N51-3 (ordinary consumer):** в текущей ветке явно трассировать ID E09/E26/E43 результата, шаг `Training/Save/Load`, Unit/Flow и обычный публичный вызов. Если нет реального соединения от обученного состояния до ответа, фиксировать `CONSUMER_MISSING` и ремонтировать именно этот участок, не подгонять старые тесты.

## 7. Запрет на завышение

**VERIFIED:** exact file body equality for five listed files; E26 uses `BuildDefault + static cases`, E43 builds fresh bigrams. **NOT VERIFIED:** что воспроизведение E09/E26/E43 из CLI было сделано в этом аудите; что RP01 использует эти конкретные historical components; что какое-либо исправление меняет semantic accuracy. **READ-ONLY:** соседний `llm_mvp_core/docs/plans/replanning-possible-diffs/**` не редактировался.

Сверка с существующим каталогом: [N50 corpus evidence](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md), [earlier E09 evaluation misread](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md), [E26 historical review](44-RECOVERY-FIRST-EVIDENCE-AND-PRIORITY-2026-10-19.md). Новое здесь — **точно выполненное двустороннее сравнение исходников** и независимая классификация E26 как preloaded lookup, не подтверждённого learned Unit.
