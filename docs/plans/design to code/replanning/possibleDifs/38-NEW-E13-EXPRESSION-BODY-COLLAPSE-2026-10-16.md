# N38-01 — E13: expression-bodied C# method теряет все обучающие признаки

**Дата:** 2026-10-16. **Тип:** отдельная гипотеза, подтверждённый source-level механизм. **Статус:** SOURCE_CONFIRMED; JS_SOURCE_LOGIC_PROXY_RUN; NATIVE_ROSLYN_NOT_RUN; DOMAIN_QUALITY_NOT_MEASURED; INDEPENDENT_CODEX_ASTRA_REVIEW_NOT_RUN.
**Исходник:** llm_mvp_core/codex/dpo-implementation, зафиксированный для просмотра SHA d1862939bfe24c3bbeb12a49af917c7ca569a10f, blob MethodKnn.cs e0ecebb6d513259a0930e8706737254f926197cd. **Область публикации:** только xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs. Защищённый core/docs/plans/replanning-possible-diffs READ ONLY.

## Для человека: какая часть успешного старого обучения недоступна

Исторический E13 учился находить похожий C# метод и по соседям предсказывать тип следующего метода. Однако метод вида **Task FetchAsync() => FetchAsyncCore();** или **async Task FetchAsync() => await ReadAsync();** имеет Roslyn ExpressionBody, а не обычный Block Body. Текущий код извлекает структуру ТОЛЬКО из Body. Поэтому такие разные методы записываются с пустым KindBag и PackedString с отметкой [Empty]. KNN получает заведомо одинаковые признаки даже для различающихся async-вызовов, guarded-вызовов и обычных выражений. Ученик может корректно сохранить оба примера, но не сможет отличить их через эту метрику.

**Новый вклад относительно реестра:** [N17 E13 train/split](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md) уже показал неэквивалентное обучение train pairs; [N11A order](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md) исследовал порядок в generic KnnLogicalUnit. Здесь **другое условие**: AST синтаксически допустимого expression-bodied метода вообще превращается в пустой набор Е13, ещё до сортировки соседей или train/split. У проверенных N17/N35/N37 E13-описаний этого точного условия не найдено; глобальная уникальность среди всех исследований НЕ ЗАЯВЛЯЕТСЯ.

## Прямые доказательства — не предположение об устройстве Roslyn

1. [MethodExtractor.Extract L20–43](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L20-L43) получает MethodDeclarationSyntax и вызывает PackMethod и ExtractKindBag.
2. [PackMethod L47–59](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L47-L59) читает method.Body?.DescendantNodes(), а при null записывает буквальное Empty.
3. [ExtractKindBag L62–75](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L62-L75) немедленно возвращает пустой HashSet, если method.Body is null; проверки method.ExpressionBody нет. В Roslyn expression-bodied MethodDeclarationSyntax использует ExpressionBody при отсутствующем block Body. Это условие нужно подтвердить конкретными SyntaxTree в native microprobe.
4. [MethodKnnPredictor.JaccardDistance L149–157](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L149-L157) возвращает **0 для двух пустых наборов**; [Predict L133–147](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/12.SemanticCompiler/Prediction/MethodKnn.cs#L133-L147) голосует соседями по этой дистанции. Таким образом пустые методы неразличимы для retrieval независимо от смыслового кода.
5. Проверены исторические [DesignToCode](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/12.SemanticCompiler/Prediction/MethodKnn.cs) и [language_translate](https://github.com/lubochka/llm_mvp_core/blob/language_translate/12.SemanticCompiler/Prediction/MethodKnn.cs): в обоих точный прежний blob 2f1153a8284a6757b4905bd76573aa81ab520b31 имеет ту же Body-only развилку. **Inherited limitation**, а не доказанная регрессия RP01.
6. [Архив E13 Tier2 7 мая](https://github.com/lubochka/llm_mvp_core/blob/d1862939bfe24c3bbeb12a49af917c7ca569a10f/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E13-METHOD-KNN/results/run-2026-05-07-001.json) сообщает 48 методов, 31 пару, baseline 2/7, KNN 5/7, PASSED. Это реальная историческая положительная запись, но не свидетельствует о качестве на expression-bodied методах; раздел train/val дополнительно оспорен N17.

## Новый причинный тест MICRO38-EXPR (C# methods / CodeDocs / Design→Code)

**L0, ≤ один маленький native xUnit/console probe:** использовать Microsoft.CodeAnalysis.CSharp для четырёх компилируемых объявлений:
- expression-bodied async/await: async Task<int> Fetch() => await ReadAsync();
- expression-bodied вызов: int Get() => Calculate();
- block-bodied async: async Task<int> Fetch() { return await ReadAsync(); }
- block-bodied обычный вызов: int Get() { return Calculate(); }

Для каждого сохранить BodyIsNull, ExpressionBodyPresent, Roslyn descendant kinds, PackedString, KindBag, CanonicalType. В baseline ожидаются оба пустых KindBag для expression-bodied; если Roslyn на данной версии показывает обратное — H38-EXPR опровергнута. На одинаковом наборе двух expression-bodied типов Jaccard=0. Контроль block-bodied должен сохранять Await/Call.

**L1, один изменяемый фактор:** A = существующий MethodExtractor. B = test-only extractor, который при Body=null использует ExpressionBody.Expression.DescendantNodesAndSelf() с тем же AbbreviateKind, тем же k, dataset, label codec, train/holdout, tie-break, без изменений weights/LoRA/teacher. Фиксировать отдельный frozen train source-family набор ≥12 методов (6 paired families), heldout ≥12 с другими именами/структурами. Мерить top1 **next-method semantic type** и долю пустых KindBag, Jaccard margin, ответ реального consumer (только если вызов E13 установлен). Нужны равные train-pairs и нет overlap N17.

**Фальсификатор:** если доля пустых KindBag не уменьшилась при B, либо улучшились только training labels, либо heldout quality не меняется/ухудшается — verdict MECHANISM_ONLY/NOT_BENEFICIAL. Обязательно негативный контроль с block-bodied файлами: их ranking не должен меняться. До доказанной actual consumer reachability не связывать находку с RP01 C#500/ENRU500 DPO Student и не менять production.

### Domain scope и приоритет

- **C# / Design-to-Code / code documentation:** P1 только для фактически вызванного E13 MethodExtractor + learned KNN; вероятен input-shape blind spot в репозиториях с expression-bodied кодом, частота пока НЕ измерена.
- **Angular / SQL / EN↔RU:** НЕ ПЕРЕНОСИТЬ Roslyn-признак напрямую: понадобится свой parser/AST/word-order encoder и собственный независимый gold exam.
- **RP01 LoRA-only standalone free exam:** NOT_APPLICABLE без доказанного вызова данного learned Unit.

Никаких teacher/Student новых вызовов не было. В законном будущем DPO человеческий отбор выполняет **CODEX на GPT-6 Astra 6 UltraCode**; внешняя открытая китайская модель создаёт **обе** стороны chosen/rejected; качество каждой пары и качество выходов Student оценивает **CODEX на Astra 6 UltraCode** независимо, с разделением train и закрытого экзамена.

Смежные материалы: [N38-LAMBDA](38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md), [MICRO38](38-MICRO-E13-DOMAIN-CAUSAL-PROTOCOL-2026-10-16.md), [source-only proxy receipt](38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json).
