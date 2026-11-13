# 03. Новые гипотезы KNN и композиции inference

19.10.2026. Проверены исходники `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` и `codex/dpo-implementation@a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`, а также сохранённые JSON. Новые native-эксперименты и обучение в этом аудите **не запускались**. Каждый протокол ниже имеет статус **PLAN / NOT_RUN**.

Сначала выполняются [восстановительные проверки language_translate](01-LANGUAGE-TRANSLATE-RESTORATION-FIRST.md). Этот файл добавляет три узкие гипотезы для C# и один смежный gate; количество карточек не равно количеству доказанных причин ухудшения качества.

**Сравнение веток.** Три файла проверены прямым diff: [donor FileActionKnn][new-donor-action], [donor MethodKnn][new-donor-method], [donor SemanticCompilerFacade][new-donor-facade] и их current аналоги в цитатах ниже имеют одинаковую функциональную реализацию; добавлена пятистрочная лицензия. Поэтому это **новые конкретные унаследованные риски**, не подтверждённые donor→current регрессии. Историческое успешное обучение совместимо с наличием такого риска на другой панели.

**Новизна.** Выполнена адресная сверка механизмов по обоим существующим каталогам: 579 основных Markdown получены и индексированы; дополнительно получены 10 свежих результатов и 12 RAG/родительских планов. Эти точные причинные цепочки не найдены при поиске и содержательной сверке релевантных разделов. Это не заявление о построчном прочтении всего массива; [точный охват](06-COVERAGE-AND-EXECUTION.md) указан отдельно. Уже известные семьи «плохие признаки», «переходы между файлами», «E05 train/val» не объявлены новыми; ниже указана разница на уровне метода. Это не обещание исчерпать все возможные будущие или недоступные документы.

**Роли нового эксперимента.** CODEX astra6ultracode выбирает случаи и независимо оценивает качество; разрешённая внешняя китайская открытая модель создаёт обе стороны новых DPO — chosen и rejected. Фиксировать фактическое имя/версию внешней модели и receipt вызова. Исходные примеры и gold не подменять строками из production-эвристик. Архивные fixed fixtures остаются историческими контролями; они не выдаются за новые teacher-authored пары. Ни один протокол не требует обхода текущих admission или подписей.

## LT-N01 — одинаковые признаки FileActionKnn требуют разных обучающих ответов

**Статус:** новый конкретный source mechanism; качество — проверяемая гипотеза; исторически унаследован, а не donor→current потеря.

В [ActionLabelGenerator.Label, 41–55][action-label] цель ADD_LOOP зависит от строкового имени типа параметра: IEnumerable, [] или List< при отсутствии цикла. [FeatureVector, 75–107][action-feature] видит частоты 30 SyntaxKind, включая GenericName, но не имя generic-типа. Обе части одного training example используют один source; [Facade, 193–202][action-train] и [E14, 29–33][e14-run] связывают эти несовместимые уровни представления.

Аналитический контрпример, не результат запуска:

```csharp
class C { int F(System.Collections.Generic.List<int> xs) { return 1; } }
class C { int F(System.Threading.Tasks.Task<int> xs) { return 1; } }
```

Имена в qualified/generic types различаются, но последовательность видов AST-узлов и число узлов совпадают. Поэтому оба 30D-вектора равны. В первом source детерминистическая Label возвращает ADD_LOOP; во втором — ADD_METHOD. Оба блока тела не содержат member access, поэтому более приоритетный ADD_NULL_CHECK не вмешивается; statement kinds недостаточно для REFACTOR_EXTRACT. [Predict, 128–141][action-predict] получает только вектор, поэтому различить такие запросы не может. При двух сбалансированных истинно неоднозначных по представлению классах максимальная accuracy детерминистической функции этих признаков равна 50% **внутри такого блока**, не во всём C# домене.

Это сильнее общего «мало данных» или «неверный k». Дополнение корпуса копиями такого конфликта не устраняет потерю информации; меняется лишь tie/majority. Комментарий [E14, 5–11][e14-claim] выводит из неудачи KNN отсутствие сигнала для LoRA. Такой вывод не следует, если LoRA получает raw source и различает List и Task: задачи для двух learners представлены по-разному.

**Что исправлять и чего не смешивать.** Сначала провести representability audit пары target/features. Возможное локальное изменение — добавить в learned representation семантику типа параметра или хотя бы согласованный типовой признак collection-kind, не подменяя весь ответ детерминистическим Label. Лексическое правило Contains само по себе не доказывает, что любой List-параметр требует цикла; независимый judge должен отдельно проверить предметную разумность выбранного действия. Изменение features и изменение gold нельзя смешивать в одном A/B.

**Быстрый опыт LT-N01, PLAN/NOT_RUN.** Для компонента взять 8 таких контрастов generic types и 8 неконфликтных C# контролей, со source families, не пересекающими train/heldout; настоящий compiler syntax extraction. A — существующие 30D, B — те же 30D плюс один заранее определённый признак роли типа параметра, k=3, порядок corpus и labels неизменны, cosine одинаков. Зафиксировать collisions `featureHash → distinct labels`, actual neighbors, top1 отдельно на collision/control blocks. Компонентная метрика называется fidelity to declared action label, не human usefulness. Затем на обычном потребителе FileAction обучить на teacher-authored/reviewer-approved правках и оценить реальное действие; если поле только диагностическое, этот шаг CONDITIONAL. Достаточное опровержение гипотезы: source parser даёт различимые признаки либо новая размерность убирает collision, но не меняет ошибки релевантной панели. Не объявлять повышение separability доказательством лучшего перевода.

**Новизна относительно старого каталога.** [Core37](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/37-new-helper-findings-data-history-and-language.md) и E14 summaries уже обсуждают дисбаланс/неодинаковые corpus и ограниченность deterministic labels; [core65](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) — исчерпывающий выбор rule label и null/pattern semantics. Новый delta — точная несовместимость имени generic-type, используемого label oracle, с безымянным GenericName в representation, создающая неизбежный target conflict даже на валидных исходниках. Это одно уточнение семейства representation/target, а не новая независимая причина каждого падения.

## LT-N02 — MethodKnn переносит обучающее отношение через границу независимых классов

**Статус:** новый конкретный inherited mechanism; вред зависит от задачи. Для намеренного предсказания следующего метода в полном порядке файла такое отношение может быть частью выбранного target. Для дополнения одного класса оно постороннее.

[PackedMethod, 11–16][method-record] хранит FileId и IndexInFile, без containing-type identity. [MethodExtractor, 21–43][method-extract] берёт все descendant methods файла и нумерует общей последовательностью. [MethodKnnPredictor.Train, 119–130][method-train] сохраняет соседей по FileId и подряд идущим индексам. Последний метод class A и первый метод независимого class B удовлетворяют обоим условиям и становятся training pair. KNN затем [Predict, 133–146][method-predict] выбирает canonical type следующего метода только по Jaccard body KindBag текущего.

**Состояние, которое действительно учится:** список пар Current/Next. Это не просто кодовая эвристика на inference: искусственная межклассовая последовательность получает такой же голос памяти, как внутри одного класса. Перестановка независимых class declarations может поменять обученные Next labels и ответы, хотя методы и внутриклассовые законы не изменились. Для nested types обход DescendantNodes также не выражает отдельный scope обучаемого отношения. Риск отличается от уже известного E09/E43 перехода между файлами: здесь FileId-проверка проходит, но отсутствует более узкая граница цели.

**Локальное решение для проверки.** Carry containing-type key и relation scope при извлечении, учить внутри нужного scope. Не использовать номер класса в исходном порядке как семантическую идентичность; он тоже изменится от перестановки. Если задача допускает межклассовую зависимость, хранить её явно отдельным relation type, а не считать любую соседнюю декларацию зависимой.

**Быстрый опыт LT-N02, PLAN/NOT_RUN.** В C# 6 учебных файлов, по два независимых класса с 2–3 методами, плюс 4 heldout файла. A — текущие пары; B — те же извлечённые методы, features, labels и k, но оставлены только пары внутри выбранного class-scope. Зафиксировать один query set «какой следующий метод нужен внутри этого класса», gold которого заранее проверен независимым judge; повторить с переставленными независимыми классами. Метрики: количество cross-scope pairs, доля изменившихся predictions от такой перестановки, correct↔wrong по gold, внутри одного класса как контроль. Это одно вмешательство в train relation; changing k, feature bag, label ontology и file split одновременно запрещает causal attribution. Если целевая задача на самом деле «следующая декларация файла», уточнить профиль задачи и область отношения, сохранив домен C#, и не требовать class-invariance.

**Смежный gate, не четвёртая причина.** [Facade TrainMethodKnn, 178–190][method-facade-train] разрешает обучение при ≥4 **методах**, но [PredictNextMethodType, 313–323][method-facade-predict] требует ≥4 TrainingSize, а [TrainingSize, 187–189][method-count] считает **пары**. Один файл с 4 методами даёт 3 пары: learner наполнен, facade возвращает null. При 5 методах одного файла появляется 4 пары. Это отдельный короткий boundary check для восстановительного опыта; измерять отсутствие ответа, не качество semantic law. Смысл threshold может быть намеренным, но критерии training и inference имеют разные единицы.

## LT-N03 — Compile меняет учебный корпус до собственного предсказания; частота анализа становится весом обучения

**Статус:** новый конкретный inherited path. Политика online-unsupervised session adaptation может быть намеренной; тогда нужно объявить её и отделить от независимой проверки. Для frozen quality measurement это contamination, а для работы IDE — потенциальное неявное перевзвешивание от частоты действий пользователя.

[Compile, 155–159][facade-consumer] сначала `RememberCompiledSource(source)`, затем `PredictNextKinds(source)`. [RememberCompiledSource, 228–236][facade-remember] каждый раз добавляет source в List без документного ID, дедупликации и замены версии. [EnsureGrammarMaskFilter, 278–294][facade-grammar] заново строит vocabulary, ngram и mask по всей накопленной List; [PredictNextKinds, 257–274][facade-grammar-predict] оценивает позиции того же source, который уже включён в corpus. Donor [GrammarMaskFilter.Train, 11–27][grammar-state] строит NgramPredictor и таблицу допустимых продолжений; [BuildGrammarTable, 98–116][grammar-mask] включает observed actual next tokens.

На обычном [RankCompletions, 99–111][facade-rank] этот же source list имеет ещё один consumer: [EnsureMethodFrequencyTable, 239–254][facade-frequency] строит частотную таблицу. В donor [MethodFrequencyTable.Train, 17–33][frequency-state] каждый вызов метода увеличивает счётчик; [RankMethods, 40–53][frequency-rank] сортирует по нему. Поэтому десять повторных Compile одного документа представлены в памяти как десять наблюдений его вызовов; если пользователь часто анализирует одно неудачное решение, его паттерн получает больший вес без нового независимого урока.

**Что здесь меняется:** данные ngram/mask и частотные счётчики. Это не подтверждение изменения LoRA или KNN веса. Не следует писать, что каждый повтор меняет каждый ответ: эффект зависит от конкурирующих переходов/кандидатов; source только доказывает скрытую схему весов. Старые E05 аудиты про train/val и границы потока не описывали этот конкретный обычный facade путь, поэтому он полезен как отдельное extension, а не переименование E05 metric caveat.

**Два последовательных однофакторных опыта, PLAN/NOT_RUN.** Первый: сохранить начальный corpus из 8 C# документов и 6 независимых heldout документов; A текущий train-before-predict, B frozen snapshot на всё время проверки. Первую панель ограничить случаями, все SyntaxKind которых уже присутствуют в train, чтобы не смешать этот опыт с расширением vocabulary и удалением OOV. Те же semantic labels сравнивать по SyntaxKind, не голым ID. Считать known-context coverage и top1 на равной панели; в raw receipt записать включение heldout source в training corpus. Возможный рост A — transductive advantage, не автоматически честное обобщение.

Второй: зафиксировать те же 8 разрешённых учебных документов, один повторить 1 или 10 раз через Compile перед неизменным heldout query. A текущее append, B document-identity replacement/dedup одного и того же content. Не менять grammar feature design. На consumer RankCompletions сохранить candidate list, receiver и actual frequency counts. Считать rank flips, correct↔wrong, стабильность при одинаковом source. Нужна панель с конкурирующими вызовами одного receiver, иначе ожидаемо ничего не изменится. Если пользователь действительно разрешил frequency-of-use adaptation, сравнить её предметную пользу как явную политику, не автоматически удалять.


## Очерёдность и критерий решения

Начать с LT-N01: source даёт конкретный information-loss witness, для первого ответа достаточно native extraction/ranking pilot. LT-N02 применим, когда пользовательская задача ограничена классом; для полного порядка файла отношение может быть намеренным. LT-N03 отделяет training corpus от частоты анализа и transductive self-evaluation.

Во всех случаях сохранять положительный контроль donor и обычный answer consumer. Изменение внутренних счетчиков или feature hashes доказывает механизм, но не автоматически пользу пользователю. Новый эффект должен подтверждаться paired correct↔wrong на соответствующем domain; teacher и judge receipts нужны для новых предметных DPO. Ни один native запуск данного пакета не выполнен.

## Источники

[e14-run]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L29-L70
[e14-claim]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E14ContextActionKnn.cs#L5-L11
[action-label]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L41-L55
[action-feature]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L75-L107
[action-train]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L193-L202
[action-predict]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/FileActionKnn.cs#L128-L141
[method-record]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs#L11-L16
[method-extract]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs#L21-L43
[method-train]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs#L119-L130
[method-predict]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs#L133-L146
[method-facade-train]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L178-L190
[method-facade-predict]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L313-L323
[method-count]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/MethodKnn.cs#L187-L189
[facade-consumer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L155-L172
[facade-remember]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L228-L236
[facade-grammar]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L278-L294
[facade-grammar-predict]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L257-L274
[grammar-state]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L11-L27
[grammar-mask]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs#L98-L116
[facade-frequency]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L239-L254
[frequency-state]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L17-L33
[frequency-rank]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Completion/MethodFrequencyTable.cs#L40-L53
[facade-rank]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/SemanticCompilerFacade.cs#L99-L111
[new-donor-action]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/FileActionKnn.cs#L17-L154
[new-donor-method]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Prediction/MethodKnn.cs#L7-L182
[new-donor-facade]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/SemanticCompilerFacade.cs#L111-L328

