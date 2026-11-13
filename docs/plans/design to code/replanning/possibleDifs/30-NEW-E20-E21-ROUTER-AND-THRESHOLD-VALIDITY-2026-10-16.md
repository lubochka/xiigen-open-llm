# N30-D/E — неверная метрика E20 и неработающий подбор порога E21

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## После объединения веток: узкий зелёный журнал не закрывает всех потребителей

Для переноса `RouteClassifier` сохранены исторические журналы от 30 октября 2026 с разными выборками: узкий прогон 90/90 и широкий 225/226; повтор одного child readiness test также неуспешен. Это не новые запуски на merge от 2 ноября. Узкий успех подтверждает свой контракт владения массивами/сохранности партии. Он не является результатом успешной проверки всех ordinary consumers и не отменяет широкий отказ.

В строке приёмки хранить exact test project/filter, выбранные TestId, source/assembly identity, counts и terminal status каждого прогона. Наборы не суммируются как уникальные тесты. Если сбой относится к потребителю, сначала установить его стадию и контракт; не объявлять его ни новой регрессией алгоритма, ни посторонним шумом без свидетельства. Опубликованный source blob маршрутизатора уже совпадает с прежним проверенным срезом; новый текст merge не является повторным исправлением.

Успешный повтор именно оставшегося consumer либо доказанный ограниченный статус закрывает соответствующую строку. Честное `component_verified=true / full_native_acceptance=open` сохраняет полезный перенос и видимый остаток. Эти уровни не смешиваются с численной проверкой saved gate из POST107.

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

**Числовой договор проверяется и у загруженного подтверждения качества.** Конечные оценки кандидата не защищают сохранённые RouteAccuracy, WrongAnnotationRate и AverageConfidence: +Infinity проходит сравнение >= в `HasPassedEvaluationGate`. Обычный `ProjectWikiRequestRoutingUnit.ExecuteAsync` читает ModelSnapshot; этот класс находится в файле `ProjectWikiRequestRoutingTrainableModel.cs`.

Хороший снимок сохраняет выбор маршрута. Затем по одному заменить каждый показатель и каждый используемый порог на NaN, +Infinity и −Infinity. Обычный Unit должен отказать, назвать поле и вернуть конечную confidence. Сохранённые пороги также должны соответствовать версии политики; одной конечности недостаточно.

У `OrchestrationPlanMachine` два входа. `ImportAsync` принимает accuracy 0.1 при eval-ссылке, хотя местный порог равен 0.75. `ImportCheckpoint` напрямую копирует LastEvaluationGate с PassesGate=true; после Load его можно повторно экспортировать как проверенный. Выбран общий существующий порог 0.75 и валидация обоих входов до публикации gate. Противоречащий checkpoint отклоняется до изменения живого состояния; честный failed gate остаётся непроверенным. Проверить обе цепочки до повторного ExportAsync. Возврат старого >0 или прямого копирования bool должен ломать соответствующий тест. Это контрпример по исходнику, не зарегистрированный инцидент.

Основание: [POST107-аудит](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) и [проверка реального кода](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md). Исторические результаты и бюджеты сохранены; это условия будущего переноса, не новый запуск.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


**16.10.2026, fresh code study.** Source `llm_mvp_core/codex/dpo-implementation@79082340f01b744cdf08185616f2bd818dbd06b8`. Статус **source-based hypothesis & numerical proxy**, actual normalized corpus/production learning accuracy **NOT_RUN**. В прочитанных master-сводках [00](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [28](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md), [29](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) такие узкие E20/E21 расхождения не перечислены; полного поиска по всем 203 файлам не выполнено.

## Человек видит симптом

Иногда алгоритм работает, но оценка сообщает «слишком плохо», потому что счётчик разделён на неверный знаменатель или параметр в отчёте вообще не используется. Если руководствоваться таким отчётом, можно переставить функции в pipeline, заменить полезную обучаемую KNN статическим маршрутизатором и затем объяснять деградацию «модели», хотя сам вывод о пользе эксперимента не был причинно проверен.

Это **не** утверждение, что поздний RP01 neural C#500/ENRU500 вызвал данный StackDetector/Decomposer. Точный consumer нужно доказать отдельно.

## N30-D: E20 marker retention измеряет плотность списка, а не сохранность после нормализации

1. [E20StackDetector.cs:26–46](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E20StackDetector.cs#L26-L46): `knownMarkers` шесть имён; цикл по CSharp/TypeScript/Python делает `totalMarkers+=6` на **каждый** пример независимо от того, встречались ли все шесть слов до нормализации. `survivedMarkers+=words.Contains(...)` в normalized. Эта дробь есть доля всего потенциального глобального перечня, присутствующая после нормализации, а **не** conditional retention маркеров, встречавшихся в raw.
2. [StackDetector.cs:96–130](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Infrastructure/StackDetector.cs#L96-L130): синтетические C# примеры имеют `Task/IEnumerable`, TS `Injectable/Promise`, Python `def`; `yield` в данном шаблоне Python не появляется. Даже при идеальной сохранности после E15 дробь `(2+2+1)/(6*3)=5/18=0.2778`.
3. [E20StackDetector.cs:68–86](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E20StackDetector.cs#L68-L86): `ShouldScaleUp = survivalRate<0.3 || val.Count<20`, а примечание прямо связывает низкий показатель с потерей маркеров E15.
4. [Archived E20 run](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E20-STACK-DETECTOR/results/run-2026-05-01-001.json) сохраняет `scaling_signal=0.2778`, baseline accuracy=1, enhanced KNN=1, `passed=false` (нет gain над 100% baseline). Архив **не содержит поштучных raw→norm before/after**.

**Вывод:** использование 27.78% как доказательства, что «нормализация уничтожила 72% присутствующих маркеров», логически неверно. Реальная conditional сохранность без запуска `SemanticNormalizer.Normalize` не установлена; часть маркеров она всё же может удалять. Отмена исторического правила порядка StackDetector→Normalizer из одного данного аудита **не предлагается**: [experiment-design-SKILL D-07](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/docs/skills/experiment-design-SKILL.md) фиксирует этот порядок.

**Быстрый E20-RETENTION:**
- Взять 12 заранее разрешённых C#/TS/Python примеров, включая mixed-marker and negative controls, отдельно 12 независимых ручных source/disjoint input.
- Для каждого вычислить `rawPresent=(marker in raw)`, `normalizedPresent=(marker in normalized)`, `true_retention=|raw∩norm∩known|/|raw∩known|`, per-stack precision и lost list, а отдельно старый показатель density для воспроизведения.
- A: native StackDetector before E15; B: after E15; A/B с **одним** frozen rule/label set и одинаковым user-facing route. Результат: `correct_domain@1`, `unknown/mixed_rejection`, `marker_retention`, `latency`. Если обе ветки одинаковы, контраст ограничить metric bug; если потеря markers actual>0 и снижает accuracy, сохранить before E15.
- Не делать вывод о выборе метода маршрута по top1=100% на 12 автоматических синтетических аналогах.

## N30-E: E21 заявляет tuning threshold, но порог не участвует в предсказании

1. [E21PromptSplitter.cs:20–48](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E21PromptSplitter.cs#L20-L48): `bestThreshold=TuneThreshold(train)`, но оценка enhanced вызывает только `ProductionSplitCount(prompt)`; `bestThreshold` не передаётся.
2. [E21PromptSplitter.cs:209–231](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E21PromptSplitter.cs#L209-L231): внутри `for t=0.20..0.90 step .05`, `score=train.Count(ex=>ProductionSplitCount(ex.Prompt)==ex.Expected)`, **без t**. При одинаковых входах `score` всегда одинаков, `best` выбирается при первом `t≈0.20` (если deterministic `Decompose` стабилен).
3. [E21 archived run 01.05](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/results/E21-PROMPT-SPLITTER/results/run-2026-05-01-001.json): `threshold=0.30`, `baseAcc=36.8%, enhAcc=89.5%, passed=true`. В текущем source ожидаемый best=0.20, поэтому **ARCHIVE↔CURRENT_SOURCE_VERSION_PARITY=OPEN**: старый запуск мог работать на другом commit/другой threshold logic, а result нельзя автоматически приписывать нынешнему HEAD. При этом score 89.5% мог быть реальным для Decomposer, просто не результатом настройки t.
4. Контракт отличается от [E21's ordinary Decomposer](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/35.ExperimentsV2/Experiments/E21PromptSplitter.cs#L222-L233): `Decompose` каждый раз получает новый `InMemoryCompilerStorage`, не проверяет будущую адаптивную память; даже честный heldout успех не является доказательством continual learning.

**Мини-опыт E21-THRESHOLD:**
- Сначала найти исторический commit, действительно создавший `run-2026-05-01-001.json`; сопоставить код `TuneThreshold`, аргументы `ProductionSplitCount`, корпус, seed, frozen result SHA.
- На том же exact input сделать два вызова: threshold 0.20 и 0.90; вывести intermediate split decisions и computed output. Если ничего не меняется, **оптимизируемого параметра в текущем path нет**.
- Сделать separate two-arm cause: control current Decomposer without any tuning, enhanced genuinely parameterized threshold forwarded to training/inference (при наличии легального production seam); оценить 20–40 source-family-disjoint задачи: true split boundaries, merged intent, false split, downstream execution correctness. `PASS` только при улучшении downstream результата, а не отчётного `bestThreshold`.
- Возможный outcome: Decomposer хорошо сегментирует запросы, но E21 не измеряет обучение — это полезная deterministic feature. Не подменять algorithmic correctness оценкой качества Student.

## Роли, применимость, принятие решения

**Для C#/TS/Python stack routing** релевантен E20. **Для compound NL task/decomposition** релевантен E21. Ни один результат не должен механически менять EN→RU LoRA или domain-specific AST KNN. Роли DPO неизменны: **CODEX/GPT-6 Astra 6 UltraCode выбирает учебные запросы вместо человека**; разрешённая **внешняя open Chinese model** создаёт **оба текста chosen/rejected**; **CODEX/GPT-6 Astra 6 UltraCode** отдельно, независимо оценивает пару и blind output. Эти роли **НЕ исполнялись** при этом source audit.

Статус источников: historical saved JSON, fresh source inspection; Python toy proxy показывает арифметику, **не** native Roslyn/SemanticNormalizer, **не** причинный gain, **не** model check. [Протокол mini-test](30-MICRO-EXPERIMENT-HISTORICAL-QUALITY-RECHECK-2026-10-16.md). Изменения разрешены только в `xiigen-planning/.../possibleDifs`, каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` **READ_ONLY**.
