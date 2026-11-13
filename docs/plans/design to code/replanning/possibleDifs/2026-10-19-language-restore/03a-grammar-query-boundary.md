# LT19-C2. Уточнение P79: Compile добавляет текущий исходник в обучающую память до возврата прогнозов

Дата: 19 октября 2026. Статус: **прослеженный экземпляр известной проблемы P79, независимо найденный и в параллельном LT-N03; влияние на качество продукта не измерено**. Это расширение семейства P79, а не самостоятельное новое семейство «grammar leakage».

**Поправка публикационного среза:** до записи нашего пакета другой агент опубликовал точный аналог в [LT-N03 пакета47](https://github.com/lubochka/xiigen-planning/blob/e5310a41ab5c79dce2f913563b7557cc27c6f272/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md). Совпадают self-inclusion до prediction и повторный частотный вес. Поэтому C2 сохраняется как сверка и детализация общего опыта, **не как самостоятельная новая находка относительно актуального possibleDifs**. Поиск по старым снимкам ниже остаётся исторически верным и не заменяет эту финальную дедупликацию.

## Что установлено и чего из этого не следует

В публичном SemanticCompilerFacade.Compile для C# целиком переданный исходник сначала добавляется в список источников текущей сессии. Затем по этому списку обучаются ngram и таблица допустимых AST-продолжений, и фасад возвращает предсказания для переходов внутри того же исходника. Если вызывающий код затем называет эти переходы независимым holdout, правильные проверочные продолжения уже участвовали в построении модели и маски.

**Это условное нарушение независимости оценки, а не доказательство дефекта любой онлайн-адаптации.** Индексация целиком доступного пользователю документа может быть намеренной и полезной. Если исходник является только разрешённым видимым префиксом, запоминание его известных переходов само по себе не раскрывает неизвестное будущее. Нельзя переносить это наблюдение на прямой RP01 TextGenerator, DPO, LoRA или обычный Translation Flow без отдельного вызывающего пути.

В проверенном коде нет замера heldout-качества этого публичного поля обычным продуктовым потребителем. Установлены сам API и его интеграционные тесты. Отсутствие найденного consumer не используется как доказательство его отсутствия во всём репозитории.

## Доказательная цепочка

Текущий pin — a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6, исторический — f6032aff81f1f83de7c09cf9b9efe6b058dd12b9.

| Звено | Исходник | Следствие |
|---|---|---|
| Полный source доступен текущему вызову | [Compile, строки 116–155](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L116-L155) | После optional regex-преобработки передаётся весь оставшийся исходник, без выделения только разрешённой оценочной позиции |
| Запоминание предшествует прогнозу | [Compile, строки 155–170](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L155-L170) | RememberCompiledSource вызывается до PredictNextKinds; результат записывается в PredictedNextKinds |
| Текущий source становится учебными данными | [RememberCompiledSource, строки 229–237](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L229-L237) | Список _compiledSources получает source; grammar mask и method frequency помечаются dirty |
| Модель переобучается по всему накопленному списку | [EnsureGrammarMaskFilter, строки 278–295](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L278-L295) | Строится vocabulary, затем вызывается Train(tokens, _compiledSources, kindToIdx) |
| Правильное продолжение каждого известного перехода входит в маску | [GrammarMaskFilter.Train, строки 17–32](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L17-L32), [BuildGrammarTable, строки 103–123](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L103-L123) | Для каждого source сохраняется связь двух предыдущих AST-kind с реально следующим |
| Поле содержит прогнозы уже представленных в source переходов | [PredictNextKinds, строки 258–275](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L258-L275) | Цикл от index=2 до index<tokens.Count возвращает N−2 прогнозов; он не выдаёт отдельный прогноз неизвестного узла после последнего |
| Это публичное поле результата | [Core.cs, строки 33–44 и 64–96](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Core/Core.cs#L33-L96) | CompilationResult и контракт ISemanticCompiler позволяют получить поле через Compile |
| Нет общего выключателя grammar-обучения | [Options, строки 8–33](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerOptions.cs#L8-L33) | BaselineOnly отключает regex, StructuralKnn, MethodKnn, FileActionKnn, но не описанный grammar-путь |

Последняя строка не означает, что BaselineOnly обязан отключать любую компиляцию. Она лишь показывает: сравнение Default с BaselineOnly не является A/B «grammar training on/off». PredictedNextKinds от E05 и PredictedNextTokens от optional E09 — разные поля и разные модели.

Для текущей внутренней token stream правильный следующий kind входит в маску соответствующего контекста **по самой конструкции BuildGrammarTable**. Это аналитическое следствие кода, не измеренная доля. Оно не означает top1=100%: разные правильные продолжения одного контекста конкурируют по ngram-вероятности, а [PredictNext, строки 35–47](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L35-L47) выбирает только одно.

Сравнение исходного текста показывает, что тело фасада уже было таким в [language_translate, строки 150–165](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/SemanticCompilerFacade.cs#L150-L165) и [строки 224–290](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/SemanticCompilerFacade.cs#L224-L290). Следовательно, это общая граница обеих веток, а не новая регрессия RP01.

## Проверенные потребители и предел утверждения

[SemanticCompilerFacade_CompileIncludesPredictedNextKindsForCSharpSource, строки 240–248](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/E02E03E05E11IntegrationTests.cs#L240-L248) создаёт новый фасад, компилирует один исходник без отдельной стадии Train и проверяет непустое поле. Это подтверждает достижимость описанного поведения. Тест не обещает качество и не выдаёт себя за независимую проверку.

Отдельный [GrammarMaskFilter_DoesNotUnderperformUnmaskedPredictorBeyondToleranceOnKnownContexts, строки 148–170](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/Tests/E02E03E05E11IntegrationTests.cs#L148-L170) честно называется проверкой известных контекстов: модель и маска обучаются на одном source и проверяются на его tokens. Такой smoke/regression-test допустим; его нельзя цитировать как доказательство генерализации на новые исходники.

В capability-пути встречается другая Compile: [SemanticTeachingPromptCompiler, строки 9–26](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/23.SemanticCompilerAdapter/SemanticTeachingPromptCompiler.cs#L9-L26) использует DevelopmentPromptCompiler из проекта 29. Совпадение слова Compile не связывает этот путь с обнаруженной мутацией проекта 12.

## Отличие от уже известного P79

[Protected P79](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/cards/P79-grammar-leakage.md) уже ставит вопрос train-only против train+holdout grammarSources в E05SyntaxFilter. Поэтому общая идея «маска не должна видеть проверочное продолжение» не новая.

Конкретный trace, независимо совпавший с позднее опубликованным LT-N03, — место, где обычный публичный вызов **сам** добавляет текущий source до прогнозирования, даже если автор нового evaluator заранее не передавал validation в явный Train. Сюда же относится время жизни _compiledSources: повторная Compile повторно добавляет source и изменяет частоты следующих вызовов. При предположении, что Compile используется как read-only оценка, это отдельная проверяемая граница контракта. Если вызывающий код намеренно учится по каждому компилируемому документу, повторение — часть его онлайн-протокола, и ожидаемую идемпотентность навязывать нельзя.

Полная сверка завершённого prior-корпуса: 1278 файлов — 417 planning и 861 protected core, включая карточки и сохранённые материалы; inventory сверил их Git blob SHA. Точные RememberCompiledSource, _compiledSources и EnsureGrammarMaskFilter там не найдены; SemanticCompilerFacade ранее обсуждался как E09 owner/consumer. На исходном срезе этот поиск поддерживал предварительный статус нового конкретного trace и не доказывал отсутствие похожего объяснения вне проверенного корпуса. После публикации пакета47 найден точный LT-N03; самостоятельная новизна C2 снята, как указано в начале этой карточки.

## Малый опыт: проверить реальную границу, не переписывая алгоритм

**Тема:** помощь с C# кодом. Один профиль — AST-продолжение небольших guard/loop методов. Это не тест свободного перевода и не дополнительный языковой домен.

**Нулевая стадия — четыре вызова без DPO и neural updates; изменения ngram/grammar-памяти учитываются.** На API consumer, который действительно использует PredictedNextKinds для качества, записать доступный исходник, допустимый префикс, ожидаемую позицию ответа и хеш/число сохранённых source до и после Compile. Если потребитель оценивает только уже известную структуру всего документа и не заявляет holdout, гипотеза об утечке оценки для него не подтверждена; не требуется исправлять такое поведение ради этой карточки.

**Данные основного опыта:** 4 учебных C# исходника и 2 независимых проверочных исходника, 40–80 заранее заданных переходов. Все kind классы проверочных исходников должны уже встречаться в train; новые контекстные тройки должны оставаться. Это позволяет держать один vocabulary без подмены результата эффектом OOV или перенумерации kind. Границы файлов, regex-преобработка и scorer одинаковы в обоих вариантах. Первым измерением проверить именно исходное предположение о доступном suffix.

**A:** clone одного и того же состояния после четырёх учебных source; прежний Compile добавляет проверочный source перед PredictNextKinds. **B:** clone того же состояния; перед первым score учёт проверочного source отключён, а все остальные tokenization, mask/predictor формулы и return-позиции сохранены. В маленьком harness B можно выразить вызовом того же PredictNextKinds до RememberCompiledSource, с отдельным явным Observe после окончания всей оценочной панели. Не менять neural weights, расстояния KNN, данные или ветку модели. Для source-идентичности и воспроизводимости зафиксировать одинаковый AST-kind codec до A/B.

**Метрики:** число evaluation-only троек в маске до первого score; mask_contains_gold, маски с несколькими допустимыми продолжениями, fallback, top1 на тех же 40–80 позициях, правильный семантический SyntaxKind и качество законченного фрагмента. Публиковать все позиционные исходы и группировать по двум независимым source. Случай «маска разрешает syntactically valid, но смысл неверен» служит hard negative. OOD — новое валидное сочетание уже известных kind, которое не было в train.

**Интерпретация:** если A имеет недоступные до ответа тройки и более высокое качество на них, а B их не имеет, старый способ оценки завышает качество будущего продолжения на этой панели. Если в задаче A весь source разрешён как контекст, разница показывает пользу адаптации к видимому документу; это другой режим, а не ошибка. Если меняется только mask_contains_gold без роста качества, влияние на точность не доказано.

**Отдельный дешёвый контроль времени жизни:** две копии одной обученной сессии. В одной выполнить несколько повторных Compile уже видимого source, в другой не повторять; затем одинаково оценить третий фиксированный запрос без его добавления. Сохранить число источников, хеши, частоты, маски и ответы. Это проверяет превращение частоты обращений в учебный вес. Такой контроль не смешивать с основным A/B включения suffix. Если read-only consumer требует неизменного состояния, изменение ответа обозначает нарушение этого требования; если consumer специально собирает пользовательский опыт, это документируемое онлайн-поведение.

**Бюджет и stop:** две копии малого состояния, 80–160 предсказаний A/B и несколько повторных вызовов; новых DPO и neural updates 0. Это предложенный объём, не выполненный запуск и не обещанное время. После установления отсутствия реального heldout consumer или отсутствия доступного запрету suffix карточка останавливается с отрицательным результатом, без общей переделки фасада.

