# 73. Редкие классы и SiblingCount: два разных ограничения, без отмены успешного E40V2

16 октября 2026. READ_ONLY_RESEARCH. Core793d7b7. Только чтение helper, исходников, локальных Git refs и15 сохранённых JSON E40/E44; новые вычисления моделей и эксперименты не выполнялись. Два [N35 helper](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md>) / [SiblingCount](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md>) прочитаны полностью.

**Оба исходных условия существуют, но архивное успешное обучение E40V2 нельзя объяснить отсутствием проверки редкого класса: в его receipts присутствуют все три класса.** Для E09 обнаружено неиспользуемое поле, однако польза этого поля ещё требует содержательных C# примеров и включённого потребителя.

## 1. Split не может дать независимый контроль единственного примера

[StratifiedSplit:23–35](C:/Projects/llm_mvp_core/35.ExperimentsV2/Infrastructure/StratifiedSplit.cs:23) группирует по label и оставляет в train минимум1. При ratio0,8 singleton остаётся только в train; два примера дают1/1. Это source-свойство. Нельзя «починить» его копированием единственного примера в validation: тогда независимости не будет. Требуется явно ограничить вывод для непроверенного класса либо подготовить отдельный пример для будущего опыта.

Но класс в этом splitter — то, что задаёт caller. [E40V2:33–40](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs:33) группирует по LOW/MEDIUM/HIGH; [E44V2:40–41](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs:40) — по ID предиката. Это не автоматически «независимый доменный закон». Стратификация по label сама по себе не обеспечивает разделение исходных файлов, семейств или семантических правил.

E40V2 действительно допускает только общую проверку val.Count≥3; notes выводят N/A для отсутствующей категории ([69–98](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs:69)). При этом scaling signal содержит долю LOW, а ShouldScaleUp зависит от общего val.Count. Утверждение комментария «все классы в обоих split» шире общего контракта функции.

## 2. Исторический positive E40V2 против гипотезы о пропущенном LOW

Полностью разобраны поля15 доступных отдельных E40/E44 JSON в `35.ExperimentsV2/results`. При первом чтении JSON.parse отказал на UTF-8 BOM; после удаления только ведущего BOM в читаемой строке все15 разобраны, файлы не переписаны. Повторяющиеся результаты разных дат не объявляются независимыми подтверждениями.

| Сохранённый опыт | Baseline → enhanced | Что было в validation |
|---|---|---|
| [E40V2, 1 мая, tier1](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-001.json) | 0,6364 → 0,8182; PASS | LOW2/2, MEDIUM3/4, HIGH4/5. Всего11. |
| [E40V2, 1 мая, tier2](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E40V2-TRUST-SCORER/results/run-2026-05-01-002.json) | 0,6667 → 1; PASS | LOW4/4, MEDIUM8/8, HIGH9/9. Всего21. |
| [Прежний E40, 1 мая, tier2](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E40-TRUST-SCORER/results/run-2026-05-01-002.json) | 0 → 0; FAIL | LOW0/20, MEDIUM/HIGH=N/A. |

30апреля у V2 записаны те же per-level результаты; поздний архив7мая содержит прежний E40 с0/20, а не новое опровержение V2. [Dispatcher](C:/Projects/llm_mvp_core/35.ExperimentsV2/ExperimentsV2Dispatcher.cs:174) и [отдельный V2 entry:213](C:/Projects/llm_mvp_core/35.ExperimentsV2/ExperimentsV2Dispatcher.cs:213) сохраняют оба варианта. Суффикс версии существенен.

Успех V2 подтверждает полезность kNN над учебными feature→trust примерами **на сохранённой панели**. [GenerateExamples:118–141](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E40TrustScorerV2.cs:118) выбирает с возвращением из14 фиксированных шаблонов; это не100 независимых реальных источников. Старый [E40:31–36](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E40TrustScorer.cs:31) использует общий shuffle, новый — stratified. Сопоставление0% старого и100% нового не является matched A/B на одинаковом heldout. Но это ограничение **не отменяет** отдельный выигрыш V2 над его baseline на одной и той же панели. Точные train IDs и loaded binary SHA здесь не восстановлены.

## 3. E44: подстановка доменной метрики реальна, но не объясняет прочитанный провал

[E44V2:52–54](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs:52) и [70–85](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs:70) при secVal.Count≤1 подставляют general accuracy вместо security. Тогда и scaling signal, названный security gain, фактически становится general gain. ShouldScaleUp=true при secVal<3 сигнализирует малость, но не отменяет числовой подстановки. Название weighted_f1 не делает это F1: здесь взвешенная сумма двух accuracy, что уже разобрано в прежних материалах.

В [прочитанном V2 receipt](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-05-01-001.json) general=0%, security23,8%→0,6%, secPreds875, should_scale_up=false. Эти значения **не соответствуют** ветке подстановки security=general рассматриваемого кода. Поэтому новую гипотезу нельзя объявить причиной сохранённого−97,67%. Прежнее смешение training targets/порядка/числа updates остаётся отдельным вопросом [57](57-e44-e45-target-confound-and-future-controls.md).

Сам «security»-домен здесь определяется [IsSecurityFile:94–98](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E44DatasetWeighterV2.cs:94) через ActionLabelGenerator.ADD_NULL_CHECK. Это более узкая эвристическая категория, не все законы безопасности. При переводе результата в SQL или EN→RU нужен новый смысловой контракт.

## 4. SiblingCount сохранён, но не используется в расстоянии E09

[StructuralVector:13–35](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:13) содержит SiblingCount и FileId, но DistanceTo не читает их. [Extractor:54–59](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:54) считает **все дочерние SyntaxNodes родителя, включая рассматриваемый узел**, с насыщением255; это не число всех соседних punctuation tokens и не dependency-связь языка. Название поля не должно заменять реальную семантику извлечения.

[Predict:145–161](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:145) выбирает по этому расстоянию, затем голосам и числовому tie-break. Если два вектора отличаются только SiblingCount, расстояние равно; при k1/равенстве порядок материала может решать выбор. Для реальных C# контекстов сначала надо найти такое различие при действительно одинаковых прочих признаках и разных корректных targets. Критерий helper «не нашлись в маленькой панели — FEATURE_NOT_DISCRIMINATIVE» следует ограничить **этой панелью**, а не объявлять поле бесполезным вообще.

Игнорирование FileId может быть полезной инвариантностью для переноса между файлами. Добавлять его штраф вместе с SiblingCount означало бы менять два разных свойства. Оба поля в старых refs `DesignToCode`/`language_translate` принадлежат одному blob `f5132ce4800082797ca7d0197373cbdbd82fdcae`; старый splitter там тоже один blob `259663c8c9575aa604a4b51e281e6e96ac73c8f8`. Эти inherited ограничения не объявлены новой регрессией RP01.

Реальный source consumer StructuralKnnPredictor — SemanticCompilerFacade; [38](38-e09-transfer-labels-and-opt-in-consumer.md) уже проследил обучение, opt-in UseStructuralKnn=false по умолчанию и отсутствие найденного обычного читателя PredictedNextTokens. Новый exact-name поиск подтверждает facade/E09 callers; он не добавляет доказательства включения функции в RP01. Трасса38 не выдана за новый аудит. Для EN→RU Roslyn SiblingCount не заменяет собственные признаки роли/отрицания.

## Будущие проверки, без исполнения

Для редких классов сначала показать per-label и per-source-family support уже сохранённой панели. Новые независимые rare-law случаи дадут новую оценку той же модели; разницу scores на разных наборах нельзя назвать изменением модели. Только последующее добавление урока при фиксированном экзамене проверит обучение. Для E44 случай secVal≤1 должен выводить UNMEASURED в исследовательском отчёте вместо заимствованного general score; это исправление интерпретации, не улучшение learner.

Для SiblingCount отдельно сравнить текущую метрику и заранее заданный слабый sibling-term на одних C# train/query/targets, удерживая k, tie-break, FileId и order. Проверить перенос на новые source families и сохранение обычных случаев. Сначала нужна source-доказанная различающая панель и реально включённый consumer. Ни этих fixtures, ни runtime проверок здесь не создано.
