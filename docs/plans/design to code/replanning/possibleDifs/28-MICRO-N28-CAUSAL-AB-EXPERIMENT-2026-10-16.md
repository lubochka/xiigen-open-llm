# MICRO-N28 — дешёвый причинный эксперимент: сохраняется ли выученный смысл при передаче SemanticTokenizer → Executor → Verifier?

**Дата:** 2026-10-16. **Тип:** READY-TO-IMPLEMENT PROTOCOL, **NOT_RUN**.  
**Цель:** не просто обнаружить несовпадение KNN-ключей, а на **одинаковых независимых source-disjoint ответах** установить, меняет ли оно качество обучения. Второй маленький шаг проверяет, не выдаёт ли встроенный Unit scorer ложный учебный успех.

## 0. Строгая граница и права доступа

Работать с `llm_mvp_core` как с источником и будущим кодовым полигоном. В рамках **этого исследования** исходный core код и каталог `docs/plans/replanning-possible-diffs` НЕ изменять. Этот файл — инструкция для отдельного **будущего санкционированного** native эксперимента. Нынешняя сессия такого запуска не выполняла.

Создание нового DPO запрещено из сырых, неразрешённых пар. Использовать готовые approved train-ready пары с сохранённой provenance. Если без дополнительного teacher невозможно построить контрастную правку Pattern, роли: CODEX на Astra 6 UltraCode выбирает законные кейсы/закрытые рубрики; **только разрешённая для DPO внешняя открытая китайская модель** создаёт обе стороны пары; независимый CODEX/Astra оценивает semantic truth и конечный Student ответ. Здесь никто из этих исполнителей не вызывался.

## 1. Гипотеза, альтернативы и условия опровержения

**H28A (key-drift):** если действительно выученный `SemanticTokenizerUnit` меняет `Pattern/PrefixSignature/StructureSignature`, а Executor продолжает индексировать урок `new TranslationSemanticTokenizer()`, то source-disjoint ответ с правильным выученным правилом иногда превращается в fallback или неверный сосед. Индексация Executor по тому **же** feature extractor, что у ordinary inference, должна улучшить качество на таких примерах при прочих равных.

**H28B (metric mismatch):** локальный `ExecutorUnit.MeasureAccuracy` или negative-only `QualityVerifierUnit.MeasureAccuracy` может быть высоким при ошибочном обычном переводе; balanced lawful/unlawful full-adapter проверка покажет разницу.

**Нулевая гипотеза:** ключи по факту одинаковы либо divergence не меняет выбранного ответа; legacy vs true evaluation также не отличается после контроля. Тогда пробел остаётся архитектурным/метрическим, **не доказанной причиной learning regression**.

## 2. Минимальная подготовка, без 500 DPO и CI Windows

1. Зафиксировать exact core commit + SHA для `TranslationTrainedUnits.cs`, `TranslationUnitContracts.cs`, `TranslationDirectionAdapters.cs`, `TranslationStructuredBatchBuilder.cs`. Использовать только локальный targeted test run (предпочтительно `dotnet test --filter FullyQualifiedName~MicroN28`); нет внешнего GitHub Windows matrix.
2. Выбрать **4 approved учебных примера**: 2 из семей, где независимая правильная поправка SemanticTokenizer реально меняет Pattern/signature, 2 контрольных без поправки. Из тех же доменов сформировать **12 закрытых unseen запросов**: 4 с изменяющимися признаками, 4 контрольных, 4 контрастных (роль/порядок/отрицание). SourceId и semantic family split не должны утекать через exact phrases, шаблоны, дубликаты или словарь; скрытые 12 gold надо заморозить до обучения.
3. Пререгистрировать каждый случай по [матрице MICRO-N28](28-MICRO-N28-CASE-MATRIX.json): source, direction, domain, нужный закон, allowed outputs, запреты, approved provenance, separate oracle status, train/heldout split. Если нет approved пары или независимой метки — кейс `EXCLUDED`, не угадывать gold.
4. Отдельно подготовить 8 valid + 8 invalid translated candidates для двухклассовой проверки Verifier. Нельзя выбирать кандидаты после просмотра того, что именно принял текущий verifier.
5. Запустить **один** минимальный C# runner в памяти. Ни DPO neural Student, ни 50 optimizer updates, ни HNSW reruns в этом опыте нет; изучается learned structured Units.

## 3. Preflight — сначала доказать, что опыт вообще различает варианты

На одной fresh `SemanticTokenizerUnit`: `bootstrap = new TranslationSemanticTokenizer().Tokenize(input, lang)`, затем `TrainOnExample` c externally approved corrected Pattern, затем `trained = semanticUnit.Tokenize(input, lang)`. Зафиксировать `bootstrap.Signature/PrefixSignature/StructureSignature` и `trained.Signature/PrefixSignature/StructureSignature`.

Обязательное условие `DISTINGUISHING`: **хотя бы одна** из исправленных строк имеет реальный изменённый ключ. Дополнительно для query source-disjoint показать, что восстановленное правило применилось вне exact-source. Если поправка сработала только на дословном train source или изменённого ключа нет, **STOP: H28A НЕ ПРОВЕРЕНА**, а не объявлять нулевой learning gain; сначала исправить допустимую постановку семантического урока по [N21-01](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md).

Контроли: exact-source должен проверяться отдельно, потому что `_exactSourceMap` может скрыть KNN недоступность; случай без semantic correction обязан давать одинаковые ключи. Ни одно учебное утверждение не выводить из proxy теста на вручную придуманных строковых ключах.

## 4. A/B №1 — влияет ли несоответствие индексации на качество обученного ответа?

Две изолированные fresh машины, одинаковые пары, порядок, source versions, tokenizer corrections, memory state до обучения Executor и frozen verifier policies.

| Arm | Единственная разница | Ожидаемое проверяемое изменение |
| --- | --- | --- |
| A current | `ExecutorUnit.TrainOnExample` строит prefix/structure через `new TranslationSemanticTokenizer()` (старый код). | Index keys могут соответствовать rule-only значениям. |
| B aligned | Тот же `ExecutorUnit` получает reference к *уже обученному* общему `SemanticTokenizerUnit` и строит **те же** prefix/structure через `semanticUnit.Tokenize(source, lang)`; live path не изменяется. | Training/serving feature lineage становится одним; KNN может найти исправленное правило. |

Для B в тестовом harness разрешена минимальная dependency injection, а не переписывание архитектуры или новых heuristic fallbacks. Не менять `k`, shortlist, thresholds, `TranslationMemorySourceLaw`, phrase/word/exact maps, исходные пары, порядок индексации или `QualityVerifierUnit`. Иначе причинный вывод некорректен.

Для каждой из 12 blind задач записывать:
`case_id, source_family_id, direction, topic, corrected_pattern_applied, rule_key, trained_key, index_bucket_keys, full/prefix/structure_hit, hit_id, admissibility, selected_candidate, adapter_id, verifier_pass, fallback_used, final_text, independent_gold_correct, latency_ms`.

**Главная метрика:** `correct_ordinary_final_text / 12` на фиксированных source-disjoint запросах, отдельно по corrected 4 и unchanged-control 4; сопутствующие — `wrong-accepted`, `abstention`, `previously_correct_regressed`. Не выдавать рост числа hits за рост качества.

**Критерий полезного микро-сигнала:** на corrected семействе B исправляет хотя бы 2 заранее adjudicated случая A; ни один originally-correct control не портится и нет нового незаконного accepted answer. Это **smoke causal result**, не статистическая гарантия переноса. Для решения об общем архитектурном изменении повторить на 3 independent family splits/доменах.

## 5. A/B №2 — измеряет ли Unit held-out реальное качество?

На **одном** зафиксированном trained state, без нового обучения:

- **Arm E0 legacy:** `ExecutorUnit.MeasureAccuracy` + `QualityVerifierUnit.MeasureAccuracy` на том held-out, который реально строит `TranslationStructuredBatchBuilder`. Сохранить число positive/negative меток.
- **Arm E1 actual:** те же source pairs и кандидат-тексты, но end-to-end `TrainedTranslationAdapter.TranslateAsync`, learned upstream tokenizer и frozen independent evaluation, 8 valid + 8 invalid candidates. Зафиксировать confusion matrix.
- **Arm E2 shadow evaluator (дополнительный, если E0/E1 расходятся):** evaluation использует ту же semantic-unit state и фактический SourceLang, что serving, и сбалансированную панель. Не менять learned policies во время оценки.

Обязательные outputs: `true_valid_accepted`, `false_valid_rejected`, `false_invalid_accepted`, `true_invalid_rejected`, balanced accuracy и coverage. Для Verifier отрицательные chosen из `BuildGeneralBatch` не считать доказательством, что правильные переводы пройдут. Если E0 высокий, а E1 низкий — это **measurement illusion**, не исправление модели. Если E1 выше при одинаковом state и только смене eval метода — тоже не training gain.

## 6. Exit status — только доказательные формулировки

| Исход | Что разрешено утверждать |
| --- | --- |
| Нет реально исправленного Pattern/source-disjoint применения | `NOT_DISCRIMINATING`: эксперимент не способен проверить H28A. |
| Changed train/serve key, но одинаковый live output | `KEY_DRIFT_CONFIRMED; QUALITY_IMPACT_NOT_OBSERVED`. |
| При одной смене extractor improved independent final answer, без control regression | `SMALL_CAUSAL_LEARNING_QUALITY_GAIN` (только указанная семья/домен/панель). |
| Встроенный evaluator высок, а balanced actual consumer провален | `EVALUATION_FALSE_SIGNAL_CONFIRMED`; **не** объявлять новое обучение. |
| Correctness выросла только на train/exact-source, не на source-disjoint | `MEMORIZATION_ONLY`, никакого переноса закона не доказано. |
| Побочный regression или нарушение law | `REJECT_CHANGE`, сохранить raw evidence, не подгонять тесты. |

## 7. Дешёвый order of operations (работа исполнителя)

Один короткий кодовый срез: (1) pinned source + 4 approved train + gold; (2) ключевой preflight; (3) A/B #1 на 12 frozen запросах; (4) A/B #2 на 16 scored candidates; (5) один raw JSON+human markdown отчёт с кодом, exact commit, logs, исполняемой командой. При non-discriminating выходе **не продолжать** 500 пар, Windows CI, ложные GREEN gates или массовую переработку планов.

**Результат этой исследовательской сессии:** описан протокол, **ни один из этих C# запусков не выполнялся**.
