# NEW-26B — E09: точный Git-перенос KNN и изменение двух алгоритмических контрактов

**16.10.2026 | COMMIT-LINEAGE VERIFIED по Git и source | quality impact NOT_MEASURED | native A/B NOT_RUN | independent CODEX/Astra review NOT_RUN.** Прочитанный core HEAD: 8c7d41611aecba98d3d1bbdbf30d69bd784949ae.

## Что впервые достоверно связано одним коммитом

Commit [27ae03266ae2f7300d63e915795c0ff62369fa67](https://github.com/lubochka/llm_mvp_core/commit/27ae03266ae2f7300d63e915795c0ff62369fa67) называется “E09 E10 phase 2: move structural KNN to production”. Он добавляет [12.SemanticCompiler/Prediction/StructuralKnn.cs](https://github.com/lubochka/llm_mvp_core/blob/27ae03266ae2f7300d63e915795c0ff62369fa67/12.SemanticCompiler/Prediction/StructuralKnn.cs) (+225 lines) и удаляет два файла: [35.ExperimentsV2/Core/StructuralKnnPredictor.cs](https://github.com/lubochka/llm_mvp_core/blob/f8a41b76b26b7de4c53aecb845661d37837d686c/35.ExperimentsV2/Core/StructuralKnnPredictor.cs) (−50) и [35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs](https://github.com/lubochka/llm_mvp_core/blob/f8a41b76b26b7de4c53aecb845661d37837d686c/35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs) (−98). Parent commit f8a41b76b26b7de4c53aecb845661d37837d686c. Оригинальные классы и production replacement прочитаны в обеих версиях. Это **подтверждённый Git-перенос E09 AST/KNN**, а не вывод по сходству имен. Но **не** доказан перенос именно «неудачная матрица → очень успешная KNN-функция»: точный matrix donor commit остаётся OPEN.

Архивный [E09 Tier2](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json): baseline 0.2413, KNN 0.3507, заявлен относительный прирост 45.33%. Но [E09StructuralKnn.Run](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L30-L79) использует всю validation для baseline и только первые 500 случаев для KNN. **Эффект на строго одной панели не доказан**, как уже отмечали [09-HISTORICAL](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) и [12-MATRIX-KNN](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md).

## E09T-01 — при переносе изменилось пространство учебных меток

[Historical E09StructuralKnn.BuildTokenVectors](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L101-L124) для каждого C# файла независимо создаёт kindMap: sorted distinct SyntaxKind → локальные порядковые BpeId. Через [Run](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L53-L67) NextToken каждой обучающей пары — **локальный BpeId следующего AST узла**.

В перенесённом [StructuralKnn.cs:99–103, 124–141](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/12.SemanticCompiler/Prediction/StructuralKnn.cs#L99-L141) обучение TrainFromSource теперь присваивает NextToken = NodeTypeId(nextNode), а NodeTypeId = ((int)SyntaxKind)%200. То есть **голое целое NextToken не имеет общего значения между историческим E09 и production E09**.

**Вывод с ограничением:** это доказанное source-расхождение в label codec, но **не автоматически баг**: если production создаёт и читает только свой собственный target codec, внутреннее обучение согласовано. Риск — сравнение old/new checkpoint и качества без семантического декодирования, перенос labels в Unit/DPO без mapping. Различные SyntaxKind, имеющие одинаковый остаток при делении на 200, стали бы неразличимы на выходе; нужно проверить реальную частоту collisions, не придумывать её.

## E09T-02 — изменилось правило на случай равенства голосов соседей

[Удалённый predictor:28–35](https://github.com/lubochka/llm_mvp_core/blob/f8a41b76b26b7de4c53aecb845661d37837d686c/35.ExperimentsV2/Core/StructuralKnnPredictor.cs#L28-L35) считает Dictionary голосов top-k и вызывает MaxBy(count) **без explicit tie-break**. При равном count первое максимальное значение в порядке перечисления словаря выигрывает; порядок зависит от фактического поведения Dictionary/runtime и первой вставки.

[Нынешний production:150–162](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/12.SemanticCompiler/Prediction/StructuralKnn.cs#L150-L162) использует GroupBy(NextToken).OrderByDescending(Count).ThenBy(Key).First(). Это **заранее установленная deterministic preference к меньшему numeric label** при равенстве голосов, а не выученное доверие/предметное право. При top5 с labels 99,10,99,10,7 предыдущий winner при порядке вставки ключей 99→10→7 — 99, новый — 10. Это доказательство различающихся политик **в соответствующем runtime порядке**, не измеренная частота ошибок на живом корпусе.

## Что осталось эквивалентным

Оригинальные и перенесённые StructuralVectorExtractor/StructuralVector содержат AST node type, три parent kinds, позицию, sibling count, method/scope depth, FileId и syntactic role. Веса distance, включая большой role-weight=4.0, сохранены; обучаемое состояние остаётся таблицей (AST vector, next label) с KNN. Неверно говорить, что весь E09 KNN заменён статической эвристикой или исчез при переносе. Но два контракта label identity/tie поменялись, что требует проверки.

## Минимальный проверяемый опыт

**E09T-A — label codec, только чтение:** на 4–8 настоящих C# файлах CODEX фиксирует исходные AST SyntaxKind. Для каждого node вычислить исторический per-file ordinal и новый SyntaxKind % 200, сохранить kind→old/new mapping и collision groups. Декодировать обе системы обратно к **семантическому SyntaxKind**, а не сравнивать accuracy по несопоставимым int. Указать число конфликтов и unseen kinds. Если decoder и train совпадают в production — нет доказанного quality failure от смены кода.

**E09T-B — только один фактор tie:** с тем же frozen train cache/AST features/k/neighbor sort и 24+ blind source-family-disjoint запросами запустить shadow first-tie против smallest-id-tie, без обучения и без изменения labels. Зафиксировать exact ties, changed decision rate, correctness@1, false-correct/false-wrong, coverage, latency. Если ties отсутствуют — источник неактивен. **Не** соединять А и B в один experiment: это разные causal variables.

**Сторож:** сначала найти действующий caller TrainFromSource→Predict→ordinary product output; наличие класса и Git переноса недостаточно для RP01. Если caller отсутствует — HISTORICAL_NOT_ACTIVE. Не менять production и защищённый core/docs/plans/replanning-possible-diffs из этого document-only исследования.

## Неизменяемые роли

**Человеческий выбор учебного закона/целевого Unit и арбитраж допустимости — CODEX с GPT-6 Astra 6 UltraCode.** Для создания DPO **внешняя открытая китайская модель** генерирует chosen **и** rejected; **CODEX / GPT-6 Astra 6 UltraCode** независимо оценивает качество DPO до допуска, а после обучения отдельно судит blind semantic output. Сейчас DPO, native E09 эксперимент и независимое судейство не выполнены. Skills: architecture, planning, plan review, code review, experiment design, prompt engineering, human communication из [core docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills).
