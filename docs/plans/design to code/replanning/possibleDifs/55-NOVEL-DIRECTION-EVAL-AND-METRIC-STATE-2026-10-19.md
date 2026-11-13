# N55-01 и N55-02 — оценка шести направлений перевода через один RU→EN путь

**19 октября 2026.** Доказанный source-seam, а не доказанная причина neural RP01 regression. Новизна ограничена конкретным вспомогательным вызовом и влиянием на нынешнюю 6-direction оценку. Это **унаследованная особенность**, а не новая строка между ветками. Предыдущие агенты уже исследовали метрику покрытия Unit и macro/micro средние, поэтому их не присваиваем.

## Где именно теряется информация

1. [EvaluateAsync — current строки 148–201](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L148-L201): для каждого EvalItem вызывает TranslateForEvaluationAsync(item.Prompt, ct); сравнивает returned FinalHumanResult и ExpectedResponse через StringComparison.Ordinal; Accuracy записывается обратно в _accuracyAfterLastBatch. В вызов не передаются source/target language или topic context элемента.
2. [TranslateForEvaluationAsync — current строки 686–705](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L686-L705) конструирует один и тот же metadata для **любого** Item: source_language="ru", target_language="en", topic_domain="general".
3. [CreateRuntime — current строки 714–790](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L714-L790) создаёт ExecutorUnit для RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES.
4. [Gold PrepareToGPL helper — строки 527–545](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L527-L545) уже имел ru/en hardcode; [gold runtime](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/27.TranslationDomain/TranslationMachine.cs#L555-L620) включал 4 направления. Следовательно N55-01 — старый пробел, способный проявляться сильнее при расширении контекста, **не самостоятельная regression diff**.
5. [TrainStructuredBatchAsync — строки 89–145](https://github.com/lubochka/llm_mvp_core/blob/6d36492714ebc1b9ae5f31a5a1da00673f524d83/27.TranslationDomain/TranslationMachine.cs#L89-L145) использует _accuracyAfterLastBatch как AccuracyBefore следующего batch, затем сохраняет новый score; это общий mutable register разных способов измерения.

## N55-01: ложный verdict многоязычного Unit

**Формулировка:** если в evalSet смешаны русско-английский, англо-русский, иврит/английский или испанский/английский задачи, нынешний EvaluateAsync передаёт их всех одному RU_EN маршруту. Успешный EN_RU Executor может остаться вообще не вызван, что даёт ложное впечатление о деградации обучения. Вход этого eval helper не сохраняет исходное направление в переданном Request.

**Установлено по коду:** маршрутизация в helper; шесть зарегистрированных directions; сравнение текста по exact bytes. **Не установлено:** что frozen RP01 C#500/ENRU500 вызывал этот public EvaluateAsync; что именно он объясняет уже сохранённые 0/3 или 0/4; что typed route исправит все смысловые переводы; что все обычные consumers используют этот helper.

**Условность:** если caller всегда формирует лишь RU_EN eval, этот seam в данном caller безвреден. Для production impact требуется реальный mixed-direction caller и его trace. Проблема решается typed metadata/item contract, а не выбором направления по Unicode/префиксу слов.

## N55-02: неверный domain score становится baseline следующего batch

После EvaluateAsync с ошибочным source-target выбором текущий общий _accuracyAfterLastBatch перезаписывается. Последующий TrainStructuredBatchAsync читает именно его как AccuracyBefore. При продолжении обучения на том же instance новый batch может выглядеть ухудшившимся или улучшившимся по сравнению с другой смесью направлений или с другой формулой. Это **source-derived causal chain**, но не факт реально достигнутого downstream promotion gate; если runner использует свежий экземпляр/сброс, эффект отсутствует.

Соседнее **уже известное** ограничение [protected core №58](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/58-unit-metrics-and-generation-consumer-boundaries.md): если held-out отсутствует, AccuracyAfter означает покрытие специальной тройки Unit, а не точность перевода. Его нельзя переименовывать в нашу новую находку.

## Короткая фальсифицируемая проверка

**S0 / source-equivalent proxy:** 12 изолированных synthetic oracle cases — по два на RU_EN, EN_RU, HE_EN, EN_HE, ES_EN, EN_ES. Oracle каждому направлению возвращает уникальную строку, не «настоящий перевод». При искусственно forced RU_EN совпадают только 2/12, при typed per-item направлении 12/12. Контроль: на 2/2 RU_EN обе версии совпадают. Это арифметическое следствие дизайна oracle, а **не измерение приложения**; native .NET ещё не выполнен.

**S1 / настоящий C# routing:** test-only ITranslationDirectionAdapter (без сети) возвращает уникальный направленный marker; 12 fixed eval items; проверить actual EvaluateAsync call trace old vs typed; при необходимости расширить EvaluationItem явным source/target, не извлекать его эвристически; отдельно capture direct ordinary TranslateAsync для каждого направления. **STOP** если невозможно передать типизированный target, — это specification gap, не автоматический FAIL обучения.

**S2 / score-state contamination:** train baseline → смешанный Eval A → последующий TrainBatch B на том же instance; сравнить AccuracyBefore следующего batch и его реальный held-out на корректных направлениях. Negative control: 2 независимых instances, RU_EN-only cases. Проверить, использует ли кто-либо это поле для promotion, не предполагать.

**S3 / предметная проверка:** по 2 независимых смысловых кейса на направление, наличие отрицания и субъект/объект; заранее отмеченные допустимые формулировки, verifier и abstention. Два языка в одном общем accuracy не объединять без per-direction знаменателя. Возвращаемое русское предложение может быть верным без строкового совпадения с одним эталоном.

## Критерии отмены гипотез

- Найденный конкретный production consumer не вызывает EvaluateAsync на mixed directions → это дефект общего API, но пока не причина текущего продукта.
- Native correct per-item route не меняет ответы/вердикты того же frozen heldout → полезное архитектурное исправление, но не causal learning recovery.
- Перед следующей проверкой score каждого domain очищается и пересчитывается независимо → N55-02 downstream effect отсутствует.
- Нарушена честность legal provenance или невозможна typed миграция старого checkpoint → нельзя обходить admission ради зелёной проверки.

**Статусы:** ROUTE_CODE=CONFIRMED; GOLD_COMPARISON=CONFIRMED_INHERITED; SYNTHETIC_EXPECTATION=DETERMINISTIC_SPEC; NATIVE=NOT_RUN; QUALITY_CAUSALITY=NOT_PROVEN.
