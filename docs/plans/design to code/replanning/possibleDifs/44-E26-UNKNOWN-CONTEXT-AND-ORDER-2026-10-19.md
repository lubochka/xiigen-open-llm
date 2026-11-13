# 44 — Новые локальные гипотезы E26: отсутствующий контекст и порядок POS

**Дата:** 2026-10-19. **Уровень подтверждения:** прямой C# код старой ветки + source-equivalent Python proxy, **native .NET и качество после обучения НЕ ПРОВЕРЕНЫ**. Срез: `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`. Эти механизмы находятся в **доноре исторического контекстного алгоритма**, не доказано, что их ошибочное поведение существует в нынешнем RP01 Student. Найденное — предупреждение, что успешный старый E26 **нельзя переносить без исправления предположений о контексте**.

## Уже известно, НЕ новая находка

Архив [E26ContextualDictionary.Run](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) на 15 фиксированных случаях показывает baseline0.40→1.00. Функция [ContextualDictionary](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L12-L117) использует POS слева и справа; current `TranslationSemanticTokenizer` в `ResolveConcept` смотрит previous concept и не читает future POS, а обучаемая память `ExecutorUnit` использует (lemma, semantic concept). Пробел representation и его доменный микроэксперимент уже были сформулированы в [N23](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) и [MICRO-25](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md). Не объявлять старые E26 results «обучением LoRA» или «100% перевода».

## K44-01 — UNKNOWN query POS получает **нулевую дистанцию** до специфического примера

**Прямой код** [ContextDistance, строки 96–115](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Infrastructure/ContextualDictionary.cs#L96-L115):

```csharp
if (rb.Length > 0 && lb.Length > 0)
    dist += lb.Any(x => rb.Contains(x)) ? 0 : 1.5;
else if (rb.Length == 0 && lb.Length > 0)
    dist += 0.5;
// нет ветки для rb.Length > 0 и lb.Length == 0
...
if (rb.Length == 0 && ra.Length == 0) dist += 1.0;
```

`lb/la` — POS слева/справа у запроса; `rb/ra` — POS у записи в словаре. Если запрос **не имеет POS** слева/справа, требование записи игнорируется, совпадение считается идеальным; explicit fallback запись без контекста **штрафуется единицей**. Вместо «контекст неизвестен» алгоритм получает предпочтение наиболее приоритетной конкретной записи.

**Воспроизводимый L0 пример:** в `BuildDefault` для «поток» есть: left VERB → `thread` (priority 10), right NOUN → `flow` (priority9), left ADJ→`stream` (priority8), empty fallback→`flow` (priority1). По формуле `TranslateWithContext("поток", [], [])` выбирает `thread` (distance0), не fallback `flow` (distance1). Контекст не доказывает thread. [Source-equivalent Python](44-E26-SOURCE-EQUIVALENT-PROXY.py) и [результат](44-E26-SOURCE-EQUIVALENT-PROXY-RESULT.json) проверяют эту конкретную формулу; **это не native вызов и не learned improvement**.

**Новая проверяемая гипотеза:** часть ложных уверенных переводов неоднозначного слова происходит от такого неправомерного приравнивания отсутствующего контекста к совпадающему. Особенно предметно важно RU→EN «поток» (thread/flow/stream) и «запрос» (request/query/prompt); не переносить однозначно на другие языки.

**X44-01 — минимальный опыт:** A = original distance, B = только penalize missing required POS либо переводить в `UNKNOWN/ABSTAIN`, C = oracle POS присутствует (контроль). Одни и те же словарные записи, priority, k и набор заданий. Входы: empty + leftVERB + rightNOUN + leftADJ для «поток», 4 «запрос», 2 OOV и 2 случая частично недоступного POS. Первичная метрика: **unsupported specific answer rate** при unknown-context (меньше лучше), дополнительная: правильные complete-context ответы/coverage. Границу B согласовать до запуска: автоматически объявлять `flow` правильным при полной неоднозначности нельзя; допустима abstention/низкая уверенность. Отрицательный контроль — когда старый source law действительно допускает wildcard; тогда K44-01 `NOT_APPLICABLE`, а не дефект. Native ordinary Flow нужен отдельно.

## K44-02 — контекстные POS считаются **как Any overlap**, порядок и кратность исчезают

В обоих направлениях до сортировки применяется `lb.Any(x => rb.Contains(x))`, а не сравнение последовательности или роль/позиция. Вспомогательная `_k=3` в оригинале объявлена, но `TranslateWithContext` возвращает первого после сортировки (другие исследователи уже отмечали этот факт, не новая причина).

**Воспроизводимый L0 пример:** две записи одного слова: A left=[NOUN,ADJ] (7,9), target `wrong` priority10; B left=[ADJ,NOUN] (9,7), target `right` priority8. Запрос left=[ADJ,NOUN] (9,7) получает distance0 к обеим, поэтому выбирается `wrong` по priority. Здесь `right/wrong` — *контрольные метки*, не реальные переводы. Python proxy это вычислил.

**Новая проверяемая гипотеза:** если предметная закономерность зависит от синтаксической роли, последовательности или области отрицания, Any-overlap в ranking подавляет различающий признак и учит/возвращает неправильный вариант; если контекст представляет *множество*, добавление порядка может быть вредным. **Нельзя** навязать ordered metric всем доменам.

**X44-02 — минимальный опыт:** A = same source corpus, legacy Any-overlap; B = ordered POS/role distance (другие features, priorities, train unchanged); C = shuffled ordering negative; D = ордер не имеет смысла positive control. RU→EN: 6 обучающих согласованных relation families + 12 **других** проверочных выражений. По желанию C# AST: **только через реальный Roslyn child role parser**, и отдельно 4 train/8 heldout source files; нельзя использовать численные POS 7/8/9 как SyntaxKind. Primary: paired law correctness, critical word-order/negation reversals, false candidates / total; secondary: recall/coverage, ms/retrieval, feature bytes. Если эффект есть только в source proxy, а в реальном learned Unit/Flow не проявляется — **нет доказанной продуктовой причины**.

## K44-03 — пятисимвольный stem (вспомогательный отрицательный контроль, НЕ заявленная новая причина)

Исходный `Stem(word)` оставляет первые пять символов слова длиной >5. Поэтому при практическом тесте добавить пары одинаковых префиксов с разной частью речи, проверяя ложные collisions. Общие роли и морфология уже разобраны [N27](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md); не считать новым DIF без самостоятельной source-family панели.

## Статус новизны и ограничения

K44-01/K44-02 получены из исходного C# и проверены однократным локальным математически эквивалентным исполнением нескольких case. При чтении N23/MICRO-25/N27/N43 они **не были представлены как эти два отдельных контрпримера**. Полный независимый полнотекстовый поиск *всех* ~400 соседних файлов не выполнен; статус **NOVEL_CANDIDATE_NEEDS_DEDUP**, не `CONFIRMED_UNIQUE`. Статус влияния на восстановление: `CAUSE_NOT_PROVEN`. Считать качеством только будущие same-input native results через тот же ordinary consumer после изменения единственного алгоритмического признака.

**Запрет:** не редактировать `llm_mvp_core/docs/plans/replanning-possible-diffs` или какой-либо core runtime ради этого audit; публикация только в `xiigen-planning/llm_plan3/.../possibleDifs`.
