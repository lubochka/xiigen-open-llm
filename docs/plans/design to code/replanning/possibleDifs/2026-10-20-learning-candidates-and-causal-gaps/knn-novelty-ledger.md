# KNN novelty ledger, 20.10.2026

Срез: planning `de270758087502a350c4fcf460acdf1638bdd318`, core `9622b575a6aabb79939b12cc9f5d96313f1bf370`. Полнотекстово просмотрено поиском **662 Markdown** в указанном possibleDifs и **296 Markdown** в защищённом `docs/plans/replanning-possible-diffs`. Это адресный поиск по всем файлам плюс чтение релевантных совпадений; не утверждается последовательное прочтение958 полных документов. Защищённые файлы только читались. Полный inventory:92 root +8 experimets +130 cards +24 small-learning-probes +42 results =296 MD; отсутствующих файлов0.

## Поисковые группы

1. Exact owners: `FileActionKnn`, `ActionLabelGenerator`, `TrainFileActionKnn`, `FeatureVector(source)`, `Label(source)`, `ParseText`, `MethodKnn`, `TrainMethodKnn`, `_pairs.Clear`, `TrainingSize`, `allMethods.Count`.
2. Reuse/cost: duplicate/double/twice/shared/reuse/cache рядом с parse/SyntaxNode/AST и русские повторный/двойной/дважды/общий разбор/парс/дерево. Дополнительно выполнен file-level AND точного file-action owner с reuse/duplicate/double parse, чтобы не потерять описание на разных строках.
3. Retention: MethodKnn рядом с clear/memory/retention/sparse/insufficient/pairs и русские потеря/стирание/недостаточно/память/пары; отдельное чтение найденного gate LT-N02.

После дозагрузки21 результатов выполнен повтор тех же exact/file-level AND запросов по полным296 protected MD. Новых совпадений не появилось. File-level AND в группе2 по-прежнему дал5 planning и3 core документа; прочитаны соответствующие sections. Они относятся к reference-score reuse, Tensor initialGradient double copy, повторному использованию baseline, источникам уже известного N-KU или повторным parse в отменённом Compile experiment. Ни один не задаёт фактор «один общий root вместо двух ParseText при подготовке того же native Train row».

| Находка | Ближайший prior art | Итог |
|---|---|---|
| KH-COST-01, два AST parse в actual FileAction training row | N48 false-label scope; LT-N01 collection features; N-KU label fix; LF-03 double Tensor copy относится к другому owner | **Новая узкая гипотеза в проверенном corpus**, source-confirmed / effect NOT_RUN. Настоящий recipient: FileActionKnnPredictor.Train; новый cost опыт описан отдельной карточкой |
| KH-EXT-01, ранее непустая память очищается sparse accepted replacement | [LT-N02 L48](https://github.com/lubochka/xiigen-planning/blob/de270758087502a350c4fcf460acdf1638bdd318/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md#L48) уже формулирует methods-count/pair-count gate | **Уточнение известного механизма**. T1→T2→reader и контракт сохранения прежней компетенции добавлены; не новая фундаментальная гипотеза, не измеренное падение, RP01 caller не доказан |
| DoStatement teacher fix, ForEachVariableStatement,30D missing Do и k3 majority | N-KU-01 source/report, предшествующий coverage audit | Известно. Source verification и положительный кандидат; не назначать повтор как новую гипотезу |
| E25 donor KNN против current heuristic | P28/P107/P122 и language_translate audits | Известно. Сохранены первичные численные receipts и code links |
| E09 structural utility и E07 отрицательный matrix результат | P26 и многочисленные matrix-to-KNN audits | Известно. Нельзя по E07 запретить AST; нельзя назвать несопоставимые runs matched transfer experiment |
| E26 .4→1 context selection | Существующие E26 provenance audits | Исторический положительный context result без Train в этом Run; не успех before/after обучения и не новый эксперимент |

## Границы

Ни новый parser, ни Train, ни Predict, ни CLR build здесь не выполнялись. Отсутствие совпадения в этом pin corpus не доказывает абсолютную новизну в невыгруженных обсуждениях, будущих коммитах или каждой соседней ветке. Исходники MethodKnn/FileActionKnn/Facade соседней `language_translate` и current текстуально совпадают после5 copyright lines; оба условных риска унаследованы. Утверждение о причине исторического падения RP01 не сделано.

Новые документы — overlays в planning. Защищённый каталог не изменён; новую узкую cost цепь ещё нельзя считать включённой в него. Карточки заранее содержат paths, owner, fixtures, commands, budget, conflicts, actual learner/reader и правило «любое измеренное улучшение полезной метрики — успех этой метрики и домена, рост accuracy не обязателен, ухудшения рядом».
