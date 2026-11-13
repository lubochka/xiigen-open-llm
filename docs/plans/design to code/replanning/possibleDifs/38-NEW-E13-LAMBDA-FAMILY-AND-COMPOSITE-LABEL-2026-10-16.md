# N38-02 — E13: fragmentированная Lambda и потеря составного типа метода

**16.10.2026 | SOURCE_CONFIRMED; JS_SOURCE_LOGIC_PROXY_RUN; NATIVE_ROSLYN_NOT_RUN; LEARNED_QUALITY_NOT_MEASURED; CODEX_ASTRA_REVIEW_PENDING.**
Pinned source: llm_mvp_core codex/dpo-implementation @ d1862939bfe24c3bbeb12a49af917c7ca569a10f, MethodKnn.cs blob e0ecebb6d513259a0930e8706737254f926197cd. Модификаций core НЕ выполнялось.

## Два отдельных, ранее не выделенных при просмотре E13 регрессионных механизма

### H38-L — фрагментация одной семантической категории lambda на несовместимые KNN-признаки

[AbbreviateKind L78–104](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L78-L104) содержит точное сопоставление строки **LambdaExpression → Lambda**, но [SyntacticRoleMapper L218–223](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/StructuralKnn.cs#L218-L223) обрабатывает реальный Roslyn **SimpleLambdaExpression** и **ParenthesizedLambdaExpression**. Они не совпадают с точным литералом в E13. Для них fallback EndsWith("Expression") убирает суффикс и создаёт **SimpleLambda** и **ParenthesizedLambda**, а не общий Lambda.

Следствие: методы, использующие семантически сходные лямбды с синтаксисом x => ... и (x,y) => ..., оказываются дальше по Jaccard, потому что именно этот признак разный. По source подтверждена неэквивалентность имени категории. Влияние на actual nearest/quality — **HYPOTHESIS**, оно зависит от прочих KindBag и того, встречаются ли такие пары в обучении.

**Контрпример:** у метода A единственная смысловая фича SimpleLambda, у метода B ParenthesizedLambda. [JaccardDistance](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L149-L157) даёт 1, тогда как при тестовом объединении обеих в Lambda — 0. Это игрушечный расчет пространства признаков, не Roslyn run и не доказательство, что всегда нужно объединять: **arity может быть значимой!** У c# overload binding, side effects и control-flow разность числа аргументов важна; сохранение исходного arity/raw kind в side channel — обязательный контроль.

**Микроопыт MICRO38-L:** взять 8–12 настоящих C# lambda-use contexts из source-disjoint repos; заморозить Roslyn AST/Gold next-method label, train memory и k. A = старый AbbreviateKind, B = общий Lambda + отдельные arity/raw-kind slots (проверять отдельно B1 только canonical, B2 canonical+arity). Сопоставлять nearest ids, accuracy@1 и unsafe false positives на запросах с совпадающей семантикой; негативный контроль — случаи, в которых arity принципиально меняет закон. Не переносить lambda canonicalization на JS/Angular автоматически, нужны локальные JS AST kinds.

### H38-C — один ответный класс скрывает комбинацию механизмов Async+Guard/ErrorHandling

[CanonicalType L171–184](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L171-L184) применяет строго приоритетный single-label switch:
- Try/Catch → ErrorHandling;
- If+Ret → Guard;
- ForEach/For → Loop;
- Await → Async;
- пустое → Simple; прочее → Mixed.

Поэтому **{If,Ret,Await} → Guard** и **{If,Ret} → Guard**, тогда как {Await} → Async; аналогично **{Try,Await} → ErrorHandling**. Исторический E13 predict голосует по [CanonicalType(Next.KindBag)](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L133-L147); вторичная часть семантики **не входит в метку**, поэтому никакой новый KNN/LoRA скорер, обучаемый на этих single-label gold, не может восстановить скрытый признак по target этой задачи. Это не ошибка метрики сама по себе: legacy task определён как single category. Дефект возникает только если downstream требует *составную компетенцию*, например «асинхронная защитная проверка перед запросом БД», а E13 label выдается за полный learned intent.

**Контрпример:** сравнить три фикстуры с заполненными KindBag:
- T1 [If,Ret] → Guard;
- T2 [If,Ret,Await] → Guard (скрыт await);
- T3 [Try,Await] → ErrorHandling (скрыт await).
Тестовый multi-label adapter B возвращает набор категорий/обязательств без смены original single-label для совместимости; метрика — coverage полного набора требований и correct downstream routing, не raw E13 legacy top1 (он считает обоих Guard правильными). Отдельное сравнение A и B и heldout source families. Это **H38-C, НЕ то же самое, что H38-L**.

## Почему это не уже известных N17/N11A/N35

- [N17 E13](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md): разные train-pair split и утечка по файлам, не label composition и lambda-kind mapping.
- [N11A generic order](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md): порядок BPE токенов generic KnnLogicalUnit, не целевая семантическая ёмкость E13 label.
- [N35 E09 sibling](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md): другой AST predictor и пропущенный SiblingCount.
- Существующие архивы уже знают, что E13 small Tier2 reported baseline 2/7→5/7; эта запись не проверяла feature consistency разных lambdas и compositional labels.

**Новизна:** конкретные H38-L/H38-C выявлены при отдельном чтении MethodKnn; в проверенных E13-материалах не нашли этих точных условий. Не утверждается, что во всех 330+ чужих работах они отсутствуют.

## Domain/consumer gate

| Потребитель | H38-L | H38-C |
| --- | --- | --- |
| C# method next-type KNN | Возможен ошибочный nearest; *не измерено* | Single-label может быть валиден по старому API; новый multi-label не нужен без спроса |
| Code documentation / Design→Code | Потеря общего lambda-semantic шаблона; **при условии фактического вызова** | Критично, если downstream требует одновременно async, guard, disposal или transaction |
| Angular/JS | НЕ ПЕРЕНОСИТЬ Roslyn kinds; отдельный AST и domain gold | Отдельная гипотеза составных ролей после подтверждения JS consumer |
| SQL | N/A Roslyn; возможный аналог multi-constraint transaction/join — НЕ РЕЗУЛЬТАТ | Только самостоятельный SQL parser/relations |
| EN↔RU / HE | N/A; синтаксические отношения другие | Аналог negation+tense+gender — отдельный linguistic eval, не копия C# алгоритма |
| RP01 neural-only | Прямой путь не найден | Прямой путь не найден |

**Статус причинности:** source-level математическое свойство CONFIRMED, ухудшение product quality/причина RP01 NOT_MEASURED.

Полный воспроизводимый микроопыт: [MICRO38](38-MICRO-E13-DOMAIN-CAUSAL-PROTOCOL-2026-10-16.md); [proxy source](38-E13-SOURCE-LOGIC-PROXY.mjs); [receipt](38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json). В настоящем DPO **CODEX на Astra 6 UltraCode** выполняет роль человека, выбирает case и проверяет качество пары и Student; **внешняя открытая китайская модель** генерирует финальные chosen и rejected; пока таких model calls НЕ было.
