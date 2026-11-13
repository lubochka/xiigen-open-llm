# N58-02 — PRIOR ART: две отдельные повторные проверки already-known ordinary EN→RU kNN

**Статус:** PRIOR_ART_N27_N37; BASE CODE VERIFIED; HYPOTHESIZED PRODUCT EFFECT; NATIVE A/B NOT_RUN; EXACT SOURCE-BRANCH REACHABILITY UNKNOWN. **Не выдавать частично знакомый механизм за новый универсальный алгоритм.**

## Прямой исходный код (core eb05219)

[ExecutorUnit.SourceTokenSimilarity/SourceContentTokens L961–989](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L961-L989):
```csharp
var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();
if (overlap < EnRuOrdinaryKnnMinimumSharedSourceTokens)
    return 0.0;
return overlap / (double)queryTokens.Count;
```
Обе стороны — `HashSet<string>`, не упорядоченные последовательности. Из SourceContentTokens исключаются `not`, `no`, `do` и служебные слова. Обычная ENRU-ветвь [Execute L264–335](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L335) использует floor=0.60 и minimum 2 shared tokens; [TrySelectKnnCandidate L333–363](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationTrainedUnits.cs#L333-L363) берёт первый допустимый из k=5, не агрегирует закон. [SourceLaw L12–42](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/27.TranslationDomain/TranslationMemorySourceLaw.cs#L12-L42) содержит закрытое регулярное выражение English copular clause `the door is not open`, а не общий imperative parser.

## N58-K1 — асимметричная containment-оценка скрывает лишнее действие в найденном соседе

Пример **ожидаемого поведения ФОРМУЛЫ (не запущенный продукт):**
- Query: `open the door`. Content set: `{open,door}`.
- Stored candidate: `open the door and delete the file`. Content set: `{open,door,delete,file}`.
- Current `|Q∩C| / |Q|=2/2=1.0` — >=0.60, кандидат может пройти admission несмотря на лишнее действие.
- Alternative **только** score-policy, `Jaccard(Q,C)=2/4=0.50` — ниже 0.60; память, learner, index, k, признаки и threshold без изменений. Это proposed A/B, не текущий production comparator.

**Потенциальная учебная потеря:** stored lesson правильный сам по себе; повторение его ответа вместо правильного более узкого результата превращает качественную память в ошибочный answer. Конкретная причинность возможна только когда feature/key cascade приводит соседа, exact/phrase/wordmap не перехватывают, verifier принимает ответ. Поддерживает ENRU imperative; для SQL bag-of-tokens нельзя переносить числовой порог без own consumer.

**Counterexample:** подзадача явно требует multi-action completion — extra action может быть допустима; gold проверяется по task contract, не по длине строки. Jaccard может понизить recall на допустимых paraphrases; учитывать coverage.

## N58-K2 — source-overlap может не различить `do not` при выполнении императива

- Query: `open the door`; stored neighbor: `do not open the door`. Content sets ОБЕ = `{open,door}` из-за stopword `do/not`: сходство 1.0.
- В режиме SourceLaw imperative обе стороны возвращают `UNKNOWN`, а не `CONTRADICTION`; это НЕ доказательство final false answer, а механическая уязвимость при достижимом kNN candidate.
- Proposed B: **не менять similarity, k, source vectors**; на заранее явно распознанных императивах добавлять ровно один signed polarity veto при mismatch, UNKNOWN не приравнивать к false/true и не использовать TEST gold. Политика NOT поддерживает только закрытую controlled подгруппу; не переносить в повествовательные языки или Hebrew/Spanish без независимо подтверждённого parser/contract.

**Полная коррекция прежнего заявления новизны:** точные асимметричные overlap/queryTokens и not/no уже исследованы [N27-04](../27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) и [N37-C](../37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md), а не впервые открыты в N58. Нашими являются лишь составленные здесь маленькие staged рецепты/конкретные фразы. **Осторожность в заявлении новизны:** общие проблемы потери negation, word order, kNN top1, узости SourceLaw и stopword описаны [P16](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P16-semantic-order-roles.md), [P54](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P54-phrase-composition.md), [P55](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P55-translation-laws.md), [P117](https://github.com/lubochka/llm_mvp_core/blob/eb05219c27663835ce927c7bf97cf1274c07cddb/docs/plans/replanning-possible-diffs/experimets/cards/P117-translation-direction-memory.md). Здесь новая **точная cross-layer постановка двух независимых факторов в ordinary ENRU (query containment / imperative polarity under narrow copular gate)**. Полная литературная уникальность среди 493 прежних элементов НЕ доказана. При точном совпадении старой карточки это PRIOR_ART, а не открытие N58.

## Нужно проследить ДО модели

`lesson.ExampleId, UnitId, Direction, original source/target, Signature, PrefixSignature, StructureSignature, stored state digest, query text/tokens, selected candidate source and direction, QueryContentSet, CandidateContentSet, similarity, SourceLaw verdict, candidate ranking, route, verifier issues, final answer`. Без реального `selectedCandidate != null` и обычного `TrainedTranslationAdapter` результат **NOT_REACHED**. Никогда не подменять consumer свой oracle или повторять gold как его output.

Отдельный, пока не найденный в просмотренном prior точный source seam находится в [N58-K3](06-NEW-PREDICATE-COMPLEMENT-LAW-AND-SMALL-EXPERIMENT.md): уже распознаваемые SourceLaw антонимные предикаты safe/unsafe и valid/invalid ошибочно classified UNKNOWN вместо CONTRADICTION. Его доказательство и L0/L1 protocol не смешаны с K1/K2.
