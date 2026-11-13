# N39-E47 — исторический FAIL BM25 скрывает не ошибку «плохой KNN», а неразличающий test split

**16.10.2026 | NEW SOURCE-AUDIT / SOURCE-LOGIC PROXY EXECUTED / NATIVE DOTNET NOT RUN / RP01 CAUSAL LINK NOT PROVEN.** Отдельная ветка исследования доменного маршрутизатора, не подмена задач DPO/LoRA. Исходники закреплены за core `d1862939bfe24c3bbeb12a49af917c7ca569a10f`. Создан только файл в `xiigen-planning/.../possibleDifs`; защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` не изменён.

## Найденная конкретная несовместимость заявления и experiment fixture

Исторический E47 описывает `ModelHubSearcher`: сравнение лексических registry-tags с нормализованным BM25 retrieval (хранилище описаний адаптеров). Документация метода `BuildTestCases` [строка 108](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/E47ModelHubSearcher.cs#L108-L128) обещает «paraphrases ... exact tags not present in query». Но **каждая из 12 фактических строк содержит хотя бы один literal tag нужного адаптера**. Более того, при подсчёте реальной логикой baseline на всех 12 ожидаемая категория имеет top1.

**Здесь отдельная, ранее не зафиксированная нами узкая гипотеза:** оценка E47 не имеет контрастной группы, в которой BM25 действительно нужен как fallback при отсутствии exact-tags. Поэтому `TUNE_OR_ABANDON` нельзя использовать как общий запрет на семантический retrieval, и нельзя молча заменить exact+BM25 одной универсальной BM25-веткой.

### Первичные доказательства

| Source | Что следует |
| --- | --- |
| [E47ModelHubSearcher.cs#L11-L46](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/E47ModelHubSearcher.cs#L11-L46) | Static registry из **10 адаптеров** со своими `Tags` и `Description`. |
| [E47ModelHubSearcher.cs#L51-L86](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/E47ModelHubSearcher.cs#L51-L86) | Baseline: `ToLowerInvariant().Split(' ')` и число literal tags; treatment: BM25 `Search(query, topK:3)`; одинаковые 12 тестов. |
| [E47ModelHubSearcher.cs#L108-L128](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Experiments/E47ModelHubSearcher.cs#L108-L128) | Реальные 12 queries всех 5 представленных expected domains. |
| [BM25EpisodeMatcher.cs#L20-L58](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/Infrastructure/BM25EpisodeMatcher.cs#L20-L58) | Подтверждён нормализованный токенизатор `SemanticNormalizer.Normalize`, IDF/TF и ранжирование. Поэтому нельзя писать, что E47 не нормализует текст только потому, что E47 не вызывает нормализатор явно. |
| [E47 raw 30 апреля](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/results/E47-MODEL-HUB-SEARCHER/results/run-2026-04-30-001.json), [raw 1 мая](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/results/E47-MODEL-HUB-SEARCHER/results/run-2026-05-01-001.json) | **Оба исторических отчёта:** baseline hit@1=1.0000, BM25 hit@1=0.9167, hit@3=0.9167, `passed:false`, `next_action:TUNE_OR_ABANDON`. Время различается; качество одинаковое. |

### Свежая адресная source-logic проверка в исследовании

Выполнена независимая небольшая JS-реконструкция **строковых операций baseline**, прочитанных из E47 source (НЕ .NET ран). Регулярными выражениями извлечены registry `Tags` и все `BuildTestCases` из закреплённого кода; далее применены `query.ToLowerInvariant().Split(' ')`, `Tags.Count(words.Contains)`, descending ranking. Получено **12/12 baseline top1** и **12/12 cases с literal target tag**. Это совпадает с raw `baseline_value:1.0`, но не измеряет реальный BM25 и не доказывает улучшение будущего hybrid.

| Домен | Количество | Literal tag в каждом query | Зачем нужен другой контраст |
| --- | ---: | --- | --- |
| DevOps deployment | 3 | deploy, pipeline, container/kubernetes | paraphrase без deploy/pipeline |
| Security/auth | 3 | jwt, rbac, oauth | roles/permissions без exact auth-tags |
| Data analytics | 2 | query, data/etl | аналитический запрос без query/data |
| iOS/mobile | 2 | ios/notification, mobile/swiftui | мобильный push без имени платформы |
| SQL/database | 2 | database/migration, sql | schema/indexing в перифразе |

**Как интерпретировать исторический отрицательный опыт:** BM25 проиграл на панели, где exact-маршрутизация была почти заведомо применимой ко всем примерам; это реальное измеренное локальное ухудшение 12→11, **а не доказательство** общего провала функции BM25 и **не доказательство** переноса матричного E47 в production.

## MICRO-N39-E47-ROUTE-01 — быстро и по доменам

**Baseline A** = exact-tag routing исходного E47; **Treatment B** = текущий BM25-only; **Treatment C** = conditional hybrid: если есть единственный уверенный exact-tag match — сохранить его; иначе BM25 topK/rerank с abstain при отсутствии надёжного кандидата. Текущие 12 из исходника — **reference regression**, не материал настройки C. Отдельно заранее заморозить по 2–3 **новых, не использованных для выбора правила** запроса в каждом из 5 доменов:

1. `tag-present` (позитивный контроль): все A/B/C обязаны удержать корректный адаптер, а C не должен портить A.
2. `paraphrase-without-registry-tag` (главная группа): использовать полностью новые формулировки и удалить совпадающие словоформы/регистровые tags; заранее разметить `ExpectedAdapterId`; считать правильный top1, top3 и `no_hit`.
3. `ambiguous-or-adversarial`: смешение security+mobile, SQL+analytics или неожиданная тема без адаптера; разрешить abstain, а не награждать случайный индекс.

Для каждого query писать `domain, lexicalMatchCount, BM25 top3 + raw score, hybrid decision, expected id, whether authorized adapter exists`. **Основная метрика** на новых `tag-absent`: `correct_top1 / N`; **ограничение**: `tag-present` не ухудшается относительно A и unsafe cross-domain selections не растут. Вне-domain и ambiguous считать отдельно, не включать в обычный denominator по желаемому score.

**Условия принятия гипотезы:** C выигрывает на новой hidden tag-absent панели, не теряет на tag-present и не увеличивает unsafe route. Если B/C не выигрывают даже на новых парафразах — гипотеза о пользе этого BM25 не подтверждена; можно оставить exact/KNN scoped иной функций. Если A на tag-absent неожиданно успешен — искать lexical leakage, не записывать ложный выигрыш.

**Цена:** 10 адаптеров, ~12 исходных + 12–18 новых строк, запуск `BM25EpisodeMatcher` в небольшом .NET test harness; не нужны pretrained модель, DPO gradient, Windows CI или многодневный план.

## Границы к другим доменам

- **Релевантно:** выбор модели/адаптера, маршрутизация обучаемых domain Units; локальный KNN/BM25 fallback может быть осмысленным только по назначению.
- **Не переносить автоматически:** порядок слов и AST в C# требует собственных features; синтаксические деревья не кодируются простой registry-tag коллизией. Перевод EN↔RU зависит от языка/направления и структуры предложения — E47 не доказывает их.
- **Не установлено:** существование `E47ModelHubSearcher` в ordinary RP01 `DpoTrainer→TextGenerator`; связь архивного 91.67% с нынешним 0/3 либо 0/4; выигрыш hybrid на невидимых данных.
- **Review:** отдельное подтверждение evaluator model candidate; исторические FAIL не переписывать; документ `RESEARCH_DRAFT`, не команда переписать TrainableRouter.

Связанные регистры уже известных отличий: [основной master](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [KNN/hybrid](04-KNN-HYBRID-RETRIEVAL.md), [условная применимость KNN](37-MASTER-RP01-EVIDENCE-GAP-REGISTER-AND-DOMAIN-PRIORITIES-2026-10-16.md). **Новая часть именно здесь:** все 12 purported `tag-absent` оказываются `tag-present`; измеренной ценности semantic fallback на отсутствующих тегах этот E47 опыт не даёт.