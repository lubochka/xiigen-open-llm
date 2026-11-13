# N33-MICRO — один быстрый причинный опыт: сохранить связь отфильтрованного слова с исходным токеном

**16.10.2026 · EXECUTABLE WORK ORDER; source-logic proxy = EXECUTED; native .NET, Student DPO, независимый CODEX/Astra judge = NOT_RUN.** Цель — выяснить, *улучшает ли исправление одного нового index seam качество после обучения*, а не собрать ещё один набор зелёных тестов.

Основание: [N33 evidence report](33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md). [Proxy source](33-MICRO-PHRASE-INDEX-SOURCE-PROXY.mjs) и [receipt](33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json). Proxy — контролируемая модель *логики* исходных строк, а не оригинального C#-исполнения.

## 1. Что уже проверено, чего пока нет

**Source-only paired proxy (действительно выполнен 16.10):** 4 искусственных RU→EN запроса, одна фиксированная PhraseMap/WordMap; менялся **только** source ordinal lookup. Без пунктуации и с символом в конце ответы совпадают; с `—` в начале старый алгоритм даёт `check check` вместо `check file` при acceptance 100%; с `—` между словами старая phrase branch не принимается (50% coverage) против принятия с сохранёнными индексами (100%). Формальные proxy counts: baseline accepted 3/4, wrong-accepted 1/4, correct-accepted 2/4; aligned accepted 4/4, wrong-accepted 0/4, correct-accepted 4/4. **Эти числа НЕ являются quality measurement реальной обученной модели и не валидируют настоящие данные.**

## 2. Гипотеза (зафиксировать ДО native run)

**H33:** когда `TranslationSemanticTokenizer` сохраняет punctuation-only token, а `ExecutorUnit` фильтрует его для phrase matching, исходная позиция пропадает; за счёт не того word-map fallback страдает семантическая правильность выученного перевода либо его покрытие. Сохранение `OriginalTokenIndex` без изменения других правил уменьшит неверные принятые результаты на source-family-disjoint запросах и не ухудшит ровные контрольные строки.

**Альтернатива H0:** проблему полностью прикрывают exact-source map, полноценная фраза, обычный word-map или verifier; native candidate не меняется или семантическая оценка не растёт.

Не смешивать проверку H33 с исправлением N30-03 `->` fake adjacency, с обучением LoRA Student, с заменой judge, с изменением 0.85 acceptance threshold или с глобальной нормализацией знаков.

## 3. L0 — быстрое воспроизведение доказательства механизма

Запустить в локальном clone нужной ветки:
```bash
node "llm mvp/design to code session/replanning/possibleDifs/33-MICRO-PHRASE-INDEX-SOURCE-PROXY.mjs"
```
Это **путь в planning repo**, а не проект из `llm_mvp_core`. Сохранённый JSON сравнить с [receipt](33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json). Перейти к L1 лишь если явная index drift есть на текущих source lines.

## 4. L1 — один tiny native C# A/B (основной эксперимент)

**Цепочка, на которую обязан реально попасть stimulus:** `TranslationMachine.TrainBatchAsync` → `ExecutorUnit.TrainOnExample` → trained `_phraseMap/_wordMap` → `TrainedTranslationAdapter.TranslateAsync` → `SemanticTokenizerUnit.Tokenize` → `ExecutorUnit.Execute` → `TryComposeFromPhraseMappings` → `QualityVerifierUnit.Verify`. Эксперимент не засчитывается, если fixture побеждает exact-source or unrelated KNN/fallback.

- **Data freeze.** Создать 2–4 небольших законных TRAIN lessons для RU_EN, которые дают `PhraseMap[проверь]=check`, `WordMap[проверь]=check`, `WordMap[файл]=file`, **без** полного exact-source `— проверь файл`. Проверить реальный `TranslationLearningExample`/UnitId, rejected side и support по дежурному law oracle. Teacher/author requirements применить лишь если создаются новые DPO пары: содержательные обе стороны от разрешённой внешней открытой китайской модели, отбор случая и независимая финальная оценка CODEX/Astra 6 UltraCode. Уже существующие frozen500 не перезаписывать.
- **A baseline.** С одного зафиксированного обученного checkpoint/manifest на существующей C# реализации вернуть candidate, `AdapterId`, confidence, phrase coverage, filtered text, original token positions, verifier/pass, phrase-map override/fallback flags для `проверь файл`, `— проверь файл`, `проверь — файл`, `проверь файл —`. Если native memory не дал требуемую composition branch — записать `NOT_TRIGGERED`, диагностировать карту, не объявлять эффект.
- **B одно изменение.** В `ExecutorUnit.TryComposeFromPhraseMappings` заменить временную коллекцию нормализованных строк на записи `{NormalizedWord, OriginalTokenIndex}`. При word-map fallback брать `tokens.Tokens[filtered[i].OriginalTokenIndex]`. Совпадение фраз, порог покрытия, direction/admission/verification **не менять**. Та же память, тот же порядок lesson, same seed/config, same source, same budget и judge.
- **План экзамена.** 4 указанных механических пары + **8–12 заранее замороженных** RU_EN source-family-disjoint естественных запросов, включая переносимые (пунктуация незначима), семантически значимые `->`, отрицание и untouched no-punctuation controls. Сначала сохранить gold смысл с независимым reviewer/доменным oracle, скрыв A/B identity; не отдавать SEALED примеры Train, CODEX-selector или Teacher. Для операторных контрпримеров ожидается **не** автоматическое принятие удаления символа, а законное корректное решение/abstain.
- **Cold run.** Экспортировать тот же trained Unit/KNN checkpoint, восстановить в новом процессе, повторить 4+8 caseIDs; fingerprint и path/adoption обязаны совпасть. Это не checkpoint Student LoRA.
- **Качественные поля на каждый case:** `case_id,source_family,law_version,expected_semantics,arm,model_or_unit_id,training_manifest_hash,route,source_tokens,filtered_index_map,composed_text,verifier_pass,final_text,correct|wrong|abstain,critical_error,wall_ms,peak_ram`. Главная метрика `wrongAccepted` и корректность финального ordinary response по слепому rubric; вторичные `accepted coverage`, `correctAccepted`, `falseAbstain`, median wall, memory. Парные wins/losses; **малый N = диагностика, не статистический многодоменный успех**.
- **Ранний останов.** Если L0 подтвердился, но L1 exact/word-map/KNN не допускает phrase path, сначала зафиксировать `NOT_TRIGGERED` и причину; не менять алгоритм дальше. Если baseline и corrected **после реального phrase-map acceptance/verifier bypass и product flow** идентичны, гипотеза о native consumer quality **не подтвердилась**. Не считать `verification.Pass=false` достаточным veto: [TrainedTranslationAdapter L127–139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L127-L139) может принять `-phrase-map` confidence 0.86. Если B ухудшает хоть один критический закон и это не шум, не продвигать B.

**Pass для кода:** A/B различаются *только* оригинальным индексом; ожидаемые контрольные входы неизменны, начальный symbol не даёт ложную удвоенную лексику, mid-symbol candidate покрывает оба содержательных слова, cold restore не искажает результат. **Pass для полезного обучения:** исправленная ветка стала чаще выводить законно правильный **финальный** ответ на неподсмотренных доменных примерах, а не только подняла coverage/завершила unit test. При нулевом или неопределённом эффекте verdict `NO_GAIN_OBSERVED` или `INCONCLUSIVE`.

## 5. Минимальный scope кода и проверок; не Windows CI

Продуктовый фикс — **одна ветвь одного метода**: [TranslationTrainedUnits.cs, L798–859](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L798-L859). Никаких изменений `DpoTrainer`, `LoraLinear`, BPE, HNSW, Router, `results/Rxx/*`, старых RAG или protected `docs/plans/replanning-possible-diffs`. Для native L1 достаточно узкой compilation of changed project, одного адресного C# probe и слепого доменного сравнения; **не запускать Windows matrix или full suite ради learning signal**. Любой product code fix только отдельной разрешённой исполнителю задачей; этот документ является research work order, не утверждением о фиксе.

## 6. Роли и независимость

- **Architect:** проверяет реальную цепочку обучения→получатель→обычный вызов и отчёт по атрибутам, не переписывает исходники.
- **Executor:** применяет один индексный repair в изолированной ветке и собирает raw A/B receipts; не меняет judge/gold/threshold по результату.
- **QA/Code review:** независимо читает метод целиком, callers, consumer, test; проверяет отсутствие скрытого deterministic authority и сохранение meaning of operators.
- **CODEX/Astra 6 UltraCode:** выбирает новые учебные ситуации и **независимо** оценивает candidate/final outputs по опубликованному роли протоколу; для новых DPO sides generator — только разрешённая внешняя open Chinese модель. Если внешняя модель или независимый судья недоступны, честно пометить `NOT_RUN`; не заменять своим score и не утверждать real learning.
- **Human communication:** один русский отчёт: что именно ломалось, что проверено, 4-case A/B, один выигрыш и одна потеря (если есть), доменная область и решение `FIX / NOT_TRIGGERED / INCONCLUSIVE / NO_GAIN / ROLLBACK`.

## 7. Return packet — обязательные поля

```yaml
id: N33-MICRO
repo_branch_commit: actual_sha
source_file_and_method: 27.TranslationDomain/TranslationTrainedUnits.cs::ExecutorUnit.TryComposeFromPhraseMappings
training_data_and_provenance: manifest_sha_or_NOT_RUN
path_confirmed: YES|NO
same_checkpoint_same_examples: YES|NO
independent_law_gold: receipt_or_NOT_RUN
baseline_final_outputs: file_path_or_NOT_RUN
one_factor_final_outputs: file_path_or_NOT_RUN
source_logic_proxy: EXECUTED_NOT_NATIVE
native_dotnet: PASS|FAIL|NOT_RUN
cold_restore: PASS|FAIL|NOT_RUN
semantic_wins_losses_wrong_accepted: counts_or_NOT_MEASURED
product_adoption: PROVEN|NOT_PROVEN
decision: FIX|NOT_TRIGGERED|INCONCLUSIVE|NO_GAIN|ROLLBACK
proof_links: [...]
```

**Фальсификатор перекоса:** если кто-то отчитается о «100% улучшении качества» на 4 JS mock examples, verdict автоматически `INVALID_EVIDENCE`. Требуется actual C# Unit + blinded ordinary output.
