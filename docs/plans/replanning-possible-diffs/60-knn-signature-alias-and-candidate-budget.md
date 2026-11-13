# 60. Один урок может дважды занять место в одном kNN bucket

2026-10-16. **READ_ONLY_RESEARCH**. Адресная сверка трёх N34 файлов из `caae0e29e8..1ebc4bac58` с core `019cb5a43e2fb817d4353c4218f6d5a90342ea0c`. JS прочитан как текст, чужой JSON как сохранённый результат; ничего не запускалось. Продуктовый код и память не менялись.

**Условие двойной записи подтверждается кодом.** Но это ещё не установленная причина качества RP01: речь о памяти structured TranslationMachine, тогда как нейронный RP01 использовал иной adapter. Уточнения ниже делают будущую проверку конкретнее и не требуют ошибочных переводов в обучающих данных.

## Цепочка записи и чтения

1. [Tokenizer](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs:124) строит полную сигнатуру из всех meaningful concept occurrences и rule tags; prefix — из первых двух occurrences и первых трёх tags. При их малом числе строки совпадают.
2. [Executor.TrainOnExample](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:404) вызывает AddPair для `example.SemanticSignature`, повторно вычисленного prefix и structure. Список ключей не дедуплицируется.
3. [AddPair](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationUnitContracts.cs:203) безусловно дописывает source/target в список. В записи нет lesson ID, direction или отдельного веса свидетельства.
4. [Cascade](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationUnitContracts.cs:246) сначала применяет source-law/similarity, затем Take(k). Он возвращает первый непустой уровень. [Executor](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:333) запрашивает k=5 и только после ограничения проверяет язык/совместимость target. Unique-lesson отбора нет.
5. [Export/Import](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationUnitContracts.cs:328) сохраняют все triples. Import очищает store, затем вставляет каждую запись один раз. **Restore сохраняет прежние дубли, но сам по себе не удваивает их заново.** Порядок внутри bucket сохраняется; комментарий об отдельной сортировке source не соответствует показанному LINQ.

Это отличается от повторного обучения того же ExampleId: для alias достаточно одного вызова. При full=prefix один урок занимает два места в одном bucket и одно в другом structure bucket. Число записей памяти поэтому не равно числу независимых уроков или свидетельств.

## Где условие действительно может появиться

В helper сказано «один или два различных concept». Точнее: код не вызывает Distinct; важны **вхождения**, не число различных значений. Три одинаковых meaningful concept уже дают три компонента full, но два prefix.

У базового английского `null property` есть concept NULL_LITERAL, а у `property` UNKNOWN; pattern=SVO. BuildRuleTags для не-ru возвращает пустой набор. Поэтому базовые full и prefix совпадают (`SVO|NULL_LITERAL`), structure отличается (`SVO|TECH_TERM|NOUN`). Это вывод из ветвей исходника, не результат вызова tokenizer. [POS/concept](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs:260), [pattern](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs:351), [языковые tags](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationSemanticTokenizer.cs:153).

Но в [batch builder](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationStructuredBatchBuilder.cs:94) default ExactPair добавляет `|PAIR:id`, затем могут добавиться tags из rejected. Такое `example.SemanticSignature` уже не обязано равняться prefix. При режиме Structural без добавочных tags условие сохраняется. ProductHeldOutRunner действительно использует [Structural](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationProductHeldOutRunner.cs:125), но конкретный материал его запуска здесь не прочитан. Нельзя объявить alias присутствующим во всех принятых наборах.

Есть и более ранние потребители: exact-source, phrase-map и word-map возвращают ответ **до kNN**. Для наблюдения эффекта на ответе нужно подтвердить, что запрос дошёл до памяти. [Порядок](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:264).

## Что доказывает сохранённый proxy

[JS](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY.mjs>) заранее задаёт одинаковые full/prefix и четыре урока. [JSON](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY-RESULT.json>) содержит bucket=8, top5=[A,A,B,B,C], выбранного допустимого D нет; при единственной записи каждого lesson bucket=4 и D доступен.

Это корректная иллюстрация расходования лимита дублями. Proxy не вычисляет реальные сигнатуры, admission, source-law, ранние maps, verifier или конечный Flow. Первые три RU_EN targets в нём содержат кириллицу — их пригодность как настоящих учебных пар не доказана. Поэтому «D стал виден в JS» не равно «перевод улучшился после обучения».

Для будущего честного контроля есть другая возможность: общий store действительно разделяется между [шестью направлениями](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationMachine.cs:714). Правильные испанские/ивритские targets для своих направлений могут быть несовместимы с EN_RU, который [требует кириллицу без Latin](C:/Projects/llm_mvp_core/27.TranslationDomain/TranslationTrainedUnits.cs:924). Тогда ранние несовместимые для текущего запроса записи не обязаны быть ошибочными учебными ответами. Достижимость общего ключа, similarity, absence of early maps и независимый gold всё равно надо установить до запуска. Это предложение, не готовый проверенный fixture.

## Малое будущее различение

Зафиксировать один structured checkpoint, один query и четыре допустимых урока. В обоих вариантах одинаковы порядок, source/target, k, законы, maps и verifier. Единственная разница — повторное добавление одинакового ключа **внутри одного вызова обучения** либо список distinct ключей. Не удалять независимые уроки глобально по одинаковому тексту.

Снять bucket до Take(5), уникальные source/lesson свидетельства, выбранный target и конечный ответ Flow. Два контроля: full≠prefix и запрос, правильно обслуживаемый ранним map. Проверить сохранение результата после restore, если эта стадия будет отдельно разрешена. Изменение числа hits без улучшения конечного смысла — отсутствие показанного выигрыша качества, а не успех обучения.

Не смешивать этот фактор с исправлением namespace направлений, FIFO/ranking или увеличением k. Эти различия уже описаны в [53](53-translation-composition-and-direction-consumers.md); здесь дополнительная причина расходования того же лимита. Взаимодействие факторов важно, но его нельзя приписывать одному исправлению без отдельного контроля.

Применимость: structured перевод и memory learning. Нейронный C#500/ENRU500 не читает этот store через свой [research adapter](C:/Projects/llm_mvp_core/27.TranslationDomain/ResearchCheckpointEnRuAdapter.cs:25); связь с их свободными ответами не установлена. Частота alias в реальном корпусе, native final quality и конкретный checkpoint с таким состоянием остаются открытыми. Новый эксперимент только описан.
