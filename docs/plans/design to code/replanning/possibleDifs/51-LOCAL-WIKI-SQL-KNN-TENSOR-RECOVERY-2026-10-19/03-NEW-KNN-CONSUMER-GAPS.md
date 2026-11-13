# KNN и структурный поиск: три новые адресные проверки

19.10.2026. Проверены C = 4945b8e85c17b5f150eb5b840d16bbdc4b506242 и W = 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a. Это исходники, один малый Python-разбор механизма и планы причинных опытов. Native C#, Roslyn, обучение модели и качество пользовательских ответов здесь не запускались.

## Что установлено

Выделены три новые адресные проверки реализации по сравнению с просмотренным корпусом: неверное накопление частот при повторной индексации файла; несовпадение структурного ключа запроса с ключом реального AST; выбор соседей из другого множества, чем список допустимых навыков. В каждом случае ниже указан непосредственный потребитель и предел доказательства.

**Все три механизма уже присутствуют в local_wiki_planning.** Содержательные тела AstRelevanceRanker, KnnSkillFilter, SemanticRanker и ContextWindowManager совпадают с current после удаления пяти строк copyright. [Полные blob SHA и сравнение](evidence/knn-semantic-parity.json), [source manifest](evidence/knn-source-manifest.json). Поэтому они не доказывают регрессию, внесённую RP01. Возможное ухудшение возникает при изменении условий использования: повторные обновления вместо одноразового построения; другой вид структурных запросов; общий индекс вместо индекса того же списка навыков. Наличие такого изменения в RP01 ещё предстоит установить.

Прочитанные ранее положительные результаты полезного обучения сохраняют свой статус. В W лежит E04 с 20 документами, 13 запросами и recall@3 = 1; E38V2 с 19 случаями и 0.4737 → 0.9474, но этот показатель обозначает попадание в категорию и часто использует fallback, а не строгую precision@5. Эти результаты не проверяют перечисленные ниже условия повторного использования индекса. [E04][hist-e04], [E38][hist-e38], [разбор фактической метрики E38][prior-e38].

### Для первой главы восстановительных опытов

| Очередь | Действие | Связь с работавшей веткой | Предел |
|---|---|---|---|
| Сначала, существующий recovery-пункт | Воспроизвести тот же набор обученных W-единиц, состояния и обычный consumer | Пользователь указал W как рабочую ветку; равенство файлов не обеспечивает равенство состояния и вызывающего кода | Основной unit/restore-аудит; здесь не повторяется |
| После установления actual AST consumer | Проверить fresh index W/C на одинаковом source corpus, затем только повторное обновление одинакового ChunkId | Может восстановить поведение одноразово построенного рабочего индекса при режиме обновлений | NK-01; прямой RP01 caller не установлен |
| Для C# структурных запросов | Проверить один и тот же if/null-контекст в W/C и реальный источник запроса | Проверяет заявленный перенос структурных E09-признаков в другой адаптер; тождество алгоритмов не предполагается | NK-02; не переносить на SQL/TS без отдельного parser |
| При реальном KnnSkillFilter | Сохранить исходный набор навыков и ограничить поиск этим набором до topK | Проверяет потерю условий, при которых работал skill retrieval | NK-03; отдельный компонент от SkillRetriever.Retrieve |

Общий откат формулы KNN в W не исправляет эти свойства: формулы уже одинаковы. Эти опыты дополняют восстановление, а не заменяют его.

## NK-01. Повторное обновление одного файла делает совпадающий признак штрафом

**Тема:** программирование и документация по C#. **Получатель:** поисковое состояние AstRelevanceRanker, поля _index и _docFreq; нейронные веса, LoRA и DPO в этом механизме не меняются.

Сейчас Index заменяет частоты внутри одного ChunkId, но суммарную частоту присутствия термина только увеличивает. Старый вклад этого же документа не вычитается. Число документов n при этом остаётся числом уникальных ChunkId. [Точная запись состояния, C L37–66][index], [формула IDF, C L69–89][idf].

> _index[chunkId] = termFreqs;  
> _docFreq[term] = df + 1;

В формуле IDF это даёт log((n + 1) / (df + 0.5)). Когда df превышает n + 0.5, совпадающий термин получает отрицательный вес. У документа без этого термина его вклад равен нулю. Это не обычное изменение редкости слова из-за добавления нового урока: corpus по уникальным ID и все тексты могут оставаться прежними. Ошибка находится в учёте замены существующей записи.

**Непосредственное последствие.** SearchAsync сортирует по score; SemanticRanker переносит эти scores на chunks; ContextWindowManager ранжирует и выбирает текст для ограниченного контекста. Показана совместимая цепочка методов через ISemanticSearchProvider, но установка конкретного AstRelevanceRanker в обычном продукте и повторные Index из actual caller ещё не установлены. Комментарий Index рекомендует одноразовый вызов при инициализации; повторный ID выходит за пределы именно этого показанного first-build сценария. Поэтому нельзя утверждать, что разрешённый штатный сценарий уже нарушен. [Сортировка][scores], [передача score][ranker], [consumer контекста][manager].

**Выполненный Python proxy.** На двух заранее заданных однотермовых документах первый имеет NullLiteralExpression, второй ForStatement. При неизменных двух ID повторяется запись первого. Его score меняется 0.693147 → 0.182322 → −0.154151. С третьей записи первым становится документ без совпадения, score = 0. При вычитании предыдущего вклада df остаётся 1 и score 0.693147 на всех четырёх шагах. Это вычисление формулы на конструированных term bags, не Roslyn и не замер качества обучения. [Результат](evidence/probe_results.json), [исполняемый proxy](evidence/probe_retrieval.py).

**Конкретное исправление, если actual обновляемый consumer подтвердится.** В том же Index перед заменой получить старые term keys, уменьшить их df ровно на один, удалить нулевые значения, затем записать новый term bag и добавить новый вклад. При batch-rebuild допустимо строить _docFreq заново из уникальных _index.Values. Не заменять отрицательный IDF clamp-ом: это скроет неправильное состояние. Хранить SourceIndexState с SchemaVersion, ChunkId → SourceHash/TermCounts, ExtractorVersion; DocFrequency считать производным состоянием. Сохранение/восстановление обязано восстанавливать df из того же набора уникальных документов. Если поиск станет частью обучаемой Unit, этот индекс остаётся внутренним производным состоянием такой Unit, а не самостоятельным псевдообучением.

**Малый native опыт NK-01, PLAN/NOT_RUN.** Tier 1: 6–8 реальных C# source files с независимыми семьями if/null, loop, exception, async. Зафиксировать corpus, source SHA, k, запросы, parser version. A: текущий алгоритм; B: только вычитание старого вклада при замене. После первой индексации и после двух повторов тех же ChunkId снять n/df, scores и правильный файл по запросу. Primary metric — доля запросов, сохранивших правильный top1 после повторного Index; дополнительно recall@3 и df≤n. Контроли: свежий полный rebuild, новый уникальный ID, замена source с исчезновением старого термина. После устойчивого native эффекта один actual потребитель документации получает те же source text/context budget; отдельная метрика — правильность описания метода. Без изменения ответа результат остаётся доказательством поиска.

**Новизна.** Прежние N20/core15 описывают пересчёт корректных corpus statistics при добавлении нового урока и нормализацию scores относительно пула. [P115.1–2][prior-p115] предлагает immutable cache для TransformerEmbeddingKnnLogicalUnit и обновление этого cache при новой corpusVersion; его direct-scan формула служит эталоном. NK-01 находится в другом owner, AstRelevanceRanker: повторный Index с тем же ID и байтами увеличивает df даже без новой версии уникального корпуса. Это реальное ошибочное накопление статистики, а не предложенный stale-cache diagnostic из P115.2. Прежние duplicate-ID KNN описывают несовпадение вектор→output, а здесь output-map и число документов остаются правильными, портится IDF. Exact owner AstRelevanceRanker и _docFreq в pinned prior не найдены.

## NK-02. Структурный запрос «null check» не соответствует ключу, который создаёт индекс

**Тема:** программирование и документация C#; задача — различать null в условии и null как возвращаемое значение. **Получатель:** структурные query/index features AstRelevanceRanker. Это самостоятельный адаптер поиска с BM25 по AST-признакам, а не StructuralKnnPredictor из 12.SemanticCompiler.

В roleMap «null check», «null condition», «if null» превращаются в NullLiteralExpression:7. При индексации роль определяется по непосредственному parent. В обычном if (x == null) непосредственный parent null — EqualsExpression; null справа получает роль 4. Роль 7 получает всё EqualsExpression, потому что именно оно непосредственно является IfStatement.Condition. [Query mapping, C L221–239][query-role], [источник ролей, C L248–266][roles].

> ["null check"] = "NullLiteralExpression:7"

Таким образом, для этого valid source индекс создаёт EqualsExpression:7 и NullLiteralExpression:4, а структурный канал запроса ищет другую пару. Обычный NullLiteralExpression из однословного map может всё ещё дать лексическое попадание, поэтому нельзя заключить, что Search всегда пустой или итоговый ответ обязательно неверный.

Есть **отдельный предшествующий фактор**: ExtractKindsFromQuery сначала парсит любой текст Roslyn и принимает результат при kinds.Count > 2 без проверки diagnostics. Natural-language запрос, для которого Roslyn вернул достаточно узлов восстановления ошибочного текста, вообще обойдёт roleMap. Это source-условие доказано; перечень натуральных запросов, которые реально попадают в него, требует native Roslyn trace. Нельзя смешивать исправление этого gate с исправлением relation features в одном A/B. [C L197–208][query-gate].

**Почему прежняя проверка не отвечает на этот вопрос.** AstIndex_NullCheckQuery_FindsFiles проверяет только Count > 0. Search возвращает до topK документов и при нулевых scores, поэтому такой assert не доказывает структурное совпадение или правильность первого файла. Это обнаружено после формулировки гипотезы по source, а не выведено из ожидаемого теста. [Test L623–634][test-ast].

**Конкретная целевая форма.** Сохранить прежние частоты kinds и непосредственных ролей. Добавить один общий extractor структурных отношений для corpus/query: например NullOperandInIfCondition с явными operator kind и operand side, чтобы is null, == null, != null не превращались в один закон. Query compiler возвращает SourceKindQuery либо HumanStructuralQuery и решение о режиме с parser diagnostics. В дальнейшем такая логика размещается внутри ILogicalUnit<StructuralQueryInput, StructuralSearchResult>; state содержит SourceHash, parser/extractor version, term counts и ID признака отношения. Learned memory остаётся прежней; deterministic parsing не объявляется обучением. Никакого нового LoRA-получателя здесь нет.

**Малый опыт NK-02a, PLAN/NOT_RUN.** Tier 1: 8–12 пар C# методов, source families раздельны. В пары включить if(x==null), if(null==x), x is null, if(x!=null), return null и unrelated literal. A/B получают одинаковую память и явный заданный StructuralQuery, чтобы ранний natural-query gate не менялся: A ищет нынешний ключ, B — только согласованный relation key. Primary metric — correct relation retrieval@1; отдельно вывести emitted keys и matched contributions. Следующий независимый опыт NK-02b меняет только выбор режима natural query, оставляя один набор features. Для документации после retrieval проверяется сохранение направления условия: «вернуть при null» и «вернуть при not null» считаются разными ответами.

**Новизна.** E09→production codec/tie, потеря SiblingCount и конструктора DepthInMethod уже описаны. NK-02 относится к другому AstRelevanceRanker: запрос требует не тот уровень AST, который индексируется. Это уточнение адаптера реального структурного поиска, а не повтор общей гипотезы «нужен AST». Непосредственный product composition caller пока не найден.

## NK-03. Global topK может успешно удалить все допустимые навыки

**Тема:** оркестрация и качество инструкций для программирования/документации. **Получатель:** отбор skill chunks для ContextWindowManager. Состояние обученной модели не меняется; меняется доступный ей контекст.

KnnSkillFilter получает skillCandidates, однако SearchAsync принимает только taskDescription и topK. ID допустимых навыков в provider не передаются. Сначала считаются confidence и topK общего ответа поиска, затем берётся пересечение с входным списком. Если совпадений нет, возвращается успешный пустой список. [Контракт provider][interface], [весь фильтр C L19–42][filter].

> var filtered = skillCandidates.Where(c => topIds.Contains(c.Id)).ToList();

ContextWindowManager действительно отделяет SkillDocument, вызывает этот фильтр, присоединяет его результат к code chunks и дальше ранжирует. Пустой успешный фильтр не восстанавливает навыки. Однако default manager использует PassThroughSkillFilter; если KnnSkillFilter не установлен, этот риск отсутствует. Найдена explicit инъекция в тесте, product composition owner не установлен. [C L22–53][manager], [инъекция и слабый assert ≤2, Tests L1090–1110][test-filter].

**Выполненный proxy.** Во входе допустимы два skills. Global top2 — два посторонних ID со scores 0.9/0.1; вычисленный confidence ≈0.9, выход []. На тех же сохранённых scores поиск внутри eligible ID до topK возвращает два skills с confidence ≈0.9. Это чистая демонстрация control flow, не оценка правильности навыков и не восстановленная E38-метрика.

**Конкретное исправление.** Provider должен получать CandidateScope со списком eligible ChunkId до отсечения topK либо выполнять exact scoring только этого списка. Вычислять confidence по eligible hits. Если scoped retrieval невозможно, безопасный локальный результат для отсутствия eligible hits — текущий полный skillCandidates с явной причиной fallback. Простого увеличения topK недостаточно: общий индекс может быть сколь угодно больше допустимого списка. Не менять skill quality score одновременно с областью поиска. Внутри SkillContextSelectionUnit (ILogicalUnit) вход содержит TaskText, CandidateId/ContentHash и ScopeId; выход — SelectedIds, scores, fallback reason. Отдельного обученного состояния у этого фильтра нет; versioned selection policy и manifest доступных skills должны восстанавливаться вместе, а исходный trained provider остаётся существующим owner.

**Малый native опыт NK-03, PLAN/NOT_RUN.** Tier 1: по 4 задачи документации и оркестрации, 12 допустимых skills и 12 вне области. Код и gold selected-skill IDs фиксируются заранее. A: global topK → intersection; B: scope → topK, всё остальное одинаково. Primary metric — eligible relevant-skill recall; дополнительно пустые успешные результаты при непустом eligible set. Контроли: индекс содержит только eligible; неизвестный запрос с низким confidence; одинаковый content разных ScopeId. Затем ContextWindowManager с тем же token budget должен передать обязательный материал в один реальный consumer, и CODEX оценивает полноту выполненной задачи. Отбор списка по ожидаемому ответу после результатов запрещён.

**Новизна.** E38 SkillRetriever.Retrieve и отсутствующий ScoreForKnn consumer уже разобраны; [P32.1–4][prior-p32] проверяет подключение score, настоящую category, обязательные skills и соответствие метрики. KnnSkillFilter — другой класс, использующий ISemanticSearchProvider. Здесь проблема не в недостатке KNN-голосования или пороге .40, а в порядке eligible scope и topK; все scores и category остаются неизменными. Общий принцип eligibility до topK уже дан в [P104.1][prior-p104] и прежнем P64. Новизна NK-03 — именно другой concrete owner KnnSkillFilter и успешный пустой результат после confident foreign hits, а не новый универсальный принцип scope→topK. В полном pinned prior имён KnnSkillFilter, SemanticRanker и ContextWindowManager нет.

## Два дополнительных lead, не записанные как доказанные причины

1. **UD-дерево теряет head edges при общем annotation contract.** DependencyToken содержит HeadTokenId, но UniversalDependenciesAnnotator переносит в SemanticToken только surface/position/family/relation. HeadTokenId/parent edge в SemanticToken отсутствует. Если будущий KNN получает только SemanticAnnotation.Tokens, он не получает готовое дерево зависимостей. Текст предложения сохранён и теоретически допускает повторный разбор, поэтому это не абсолютная потеря информации обо всём входе. Active retrieval consumer в рамках этого поиска не установлен. Перед переносом синтаксических функций на перевод нужен отдельный edge-preservation опыт, без claims о C# AST или RP01. [UD projection][ud], [типы][ud-types].
2. **AgentPipeline передаёт в gateway только найденные ChunkId.** Контент документов не загружается в prompt. Это адресный новый consumer-разбор уже известной общей категории «retrieved evidence не прочитан ответчиком», а не самостоятельная новая универсальная гипотеза. [Core45][prior-core45] и [P118][prior-p118] уже выделяют память, содержание и только style_source, но для ProjectWiki Mode2/3, а здесь другой gateway chain. Desktop caller использует mock providers, поэтому продуктовая активность не доказана. [AgentPipeline L49–65][pipeline].

## Границы проверки и распределение ролей

По полному pinned корпусу planning/core выполнен адресный поиск имён owner, полей и формул, а ближайшие предшествующие досье прочитаны. Это не утверждение о мировом приоритете или об отсутствии всех возможных переформулировок: реестр [novelty-search.json](evidence/knn-novelty-search.json) сохраняет термы поиска и hits. Проверены adjacent зоны confidence, null/default candidate, отсутствие eligible hits, семантическое равенство веток и тесты, которые могли скрыть дефект. За пределами — actual product DI graph, runtime index updates, neural gradients и влияние на ordinary free answer. Ни одно из трёх свойств не объявлено причиной неудачи RP01.

Для будущих DPO действует [единый договор ролей](05-SMALL-DOMAIN-EXPERIMENTS.md#roles): CODEX с запрошенной моделью Astra 6 ultracode выбирает задачу и материал; внешняя открытая китайская модель пишет обе финальные стороны; отдельный CODEX judge проверяет точную пару и отдельно вслепую оценивает ответы ученика. Здесь эти модели не вызывались, новые DPO не генерировались; Python fixtures являются аналитическими входами.

[index]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L37-L66
[idf]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L69-L89
[scores]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L99-L156
[ranker]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/SemanticRanker.cs#L28-L48
[manager]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/ContextWindowManager.cs#L22-L53
[query-role]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L221-L239
[roles]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L248-L266
[query-gate]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/AstRelevanceRanker.cs#L197-L208
[test-ast]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/Tests/AgentAlgorithmsTests.cs#L623-L634
[filter]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Context/KnnSkillFilter.cs#L19-L42
[interface]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Fabric/Interfaces.cs#L41-L62
[test-filter]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/Tests/AgentAlgorithmsTests.cs#L1090-L1110
[hist-e04]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/35.ExperimentsV2/results/E04-BM25-CONTEXT/results/run-2026-05-01-001.json#L1-L18
[hist-e38]: https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/35.ExperimentsV2/results/E38V2-SKILL-RETRIEVER/results/run-2026-05-01-001.json#L1-L18
[prior-e38]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md
[prior-p115]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/experimets/cards/P115-bm25-statistics-cache.md#L5-L25
[prior-p32]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/experimets/cards/P32-skill-selection.md#L5-L35
[prior-core45]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/45-memory-reachability-and-context-consumption.md
[prior-p118]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/experimets/cards/P118-projectwiki-content.md#L5-L45
[ud]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/28.SemanticNlp/Algorithms/UniversalDependencies.cs#L233-L275
[ud-types]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/28.SemanticNlp/Core/Types.cs#L13-L47
[pipeline]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/22.AgentAlgorithms/Pipeline/AgentPipeline.cs#L49-L65


[prior-p104]: https://github.com/lubochka/llm_mvp_core/blob/4945b8e85c17b5f150eb5b840d16bbdc4b506242/docs/plans/replanning-possible-diffs/experimets/cards/P104-law-eligibility.md#L14-L18
