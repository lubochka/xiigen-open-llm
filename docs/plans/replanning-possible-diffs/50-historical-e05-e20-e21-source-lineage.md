# Исторические E05/E20/E21: что измерялось и что заменили при переносе

16 октября2026. READ_ONLY_RESEARCH. Source, git history и сохранённые JSON прочитаны; ни один опыт/скрипт/тест здесь не запускался. Новые helper материалы N30 сопоставлены с первичными источниками.

**Найден конкретный потерянный механизм:** E21 действительно подбирал порог по учебным примерам.6 мая его заменили другим Decomposer, а название `tuned_threshold` оставили. **Ещё одно различие переноса:** E05 раньше ранжировал разрешённые продолжения тем же trigram, что и baseline; нынешний GrammarMaskFilter использует unigram/bigram. Это изменения алгоритма и смысла сравнения, не измеренные причины плохой генерации RP01.

## E21: положительный результат относится к настоящему прежнему подбору порога

В [receipt1мая](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E21-PROMPT-SPLITTER/results/run-2026-05-01-001.json) сохранено36.84%→89.47%,19 validation cases, выбранный threshold0.30. Это точность **числа сегментов**, а не доказательство правильного положения каждой границы или качества итогового выполнения задачи.

История файла результата указывает публикацию `530c1e0581fa854574bd8608a7576a80236f2ebd` и зеркальную lineage `4db575b1c6…` от1мая. Прочитанный E21 source в первом из этих commits совпадает с source до изменения6мая. Это связывает опубликованный результат с соседствующим исходником, но не является аттестацией реально запущенного binary.

В том source:

- `TuneThreshold` создаёт `PromptSplitter(t)` для каждой точки0.20…0.90 и оценивает его на train.
- Enhanced создаёт `PromptSplitter(bestThreshold)` и применяет на val.
- Исторический `35.ExperimentsV2/Infrastructure/PromptSplitter.cs` действительно использует `_changeThreshold`: считает cosine distance профилей `_OP` по двум окнам и сравнивает с порогом. Это реальный настраиваемый параметр.

Точный read-only diff commit **`10364aef4016ceeb66877f0f62dac8cf6fecd43d` от6мая** (`wire canonical semantic sources through orchestration`) показывает обе замены:

| Было | Стало |
|---|---|
| В tuning: `new PromptSplitter(t)` | `ProductionSplitCount(ex.Prompt)`, без t |
| В val: `new PromptSplitter(bestThreshold).Split(...)` | тот же `ProductionSplitCount(...)`, без порога |

Нынешний [E21,215–231](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E21PromptSplitter.cs:215) повторяет один и тот же расчёт внутри цикла и сохраняет первую лучшую точку. При стабильном детерминированном splitter она будет0.20. Разница с архивным0.30 **объяснена найденным source delta**, а не доказывает ошибку старого результата.

### Два нынешних Decomposer не равнозначны

[12.SemanticCompiler.Decomposer,17–48](C:/Projects/llm_mvp_core/12.SemanticCompiler/Planning/Decomposer.cs:17) сохраняет cosine-window и changeThreshold. [29.SemanticCompiler.Decomposer,19–54 и67–89](C:/Projects/llm_mvp_core/29.SemanticCompiler/Planning/Decomposer.cs:19), который теперь вызывает E21, использует заданные списки conjunction/verb/noun/pronoun и фиксированную проверку формы задачи. Его header всё ещё содержит старую ссылку `E21:+142.9%`.

Полезность правил отрицать нельзя: отказ разделять «найти объекты и обработать их» может быть правильным предметным поведением. Но числом прежнего cosine-window опыта нельзя подтвердить качество другого набора правил. При этом старый splitter тоже не безусловно лучше: он получает границы после Normalize/FamilyCollapser, затем применяет индексы к rawWords. При изменении числа токенов привязка границ требует отдельной проверки. Повторно переносить старый код целиком по одному PASS не предлагается.

**Будущий короткий опыт:** frozen panel задач, две сохранённые реализации, в прежней threshold выбран только на train. Показать текстовые spans и независимость/зависимость частей, а не только число частей. Сначала сохранить отдельные результаты двух алгоритмов; далее изменять один фактор. Прогон нынешнего TuneThreshold сам по себе не обучает splitter. Никакой новой реализации или исполнения этим отчётом не авторизуется.

## E20: 27.78% не означает потерю72% присутствовавших признаков

Первичный [E20,37–46](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E20StackDetector.cs:37) делит число найденных после нормализации markers на `6 × число примеров` для трёх стеков. Он не считает, какие из шести markers присутствовали до нормализации.

В [synthetic generator,96–114](C:/Projects/llm_mvp_core/35.ExperimentsV2/Infrastructure/StackDetector.cs:96) C# содержит Task/IEnumerable, TypeScript — Injectable/Promise, Python — def. По этому списку получается5 присутствующих из18 потенциальных; **даже при сохранности всех исходно присутствовавших** старый показатель будет5/18≈27.78%.

[Receipt1мая](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E20-STACK-DETECTOR/results/run-2026-05-01-001.json) действительно содержит0.2778, accuracy1→1, Passed=false, ShouldScaleUp=true. False здесь не означает плохую классификацию: нового выигрыша над100%baseline нет. [ShouldScaleUp,85–86](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E20StackDetector.cs:85) реагирует также на малую val-панель.

Обе сравниваемые ветки [55–64](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E20StackDetector.cs:55) получают **Normalized**, поэтому этот опыт сам по себе не является A/B порядка StackDetector до/после нормализации. Он сравнивает marker voting с kNN на одинаковом уже преобразованном вводе. Запретить/разрешить перестановку стадий по этой дроби нельзя.

Будущая проверка: на одном корпусе показать marker identity до/после и долю сохранённых **из присутствовавших**, отдельно ошибку маршрутизации. Менять порядок стадий — отдельное вмешательство. Не запускалось; source-normalizer по всему synthetic corpus здесь не вычислялся.

## E05: таблица допустимости видит holdout, а перенесённый predictor изменён

В нынешнем [E05,20–43](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E05SyntaxFilter.cs:20) train/validation делятся по AST positions, но grammarSources по умолчанию содержат весь csharpSource. [GrammarMaskFilter.BuildGrammarTable,104–125](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs:104) записывает наблюдаемые тройки из всех переданных исходников. Это таблица **увиденных продолжений**, не независимо заданная полная грамматика C#.

Так future validation target может войти в mask до оценки. Baseline обучен только на train. Полный vocabulary также построен до split, но наличие словаря и наличие target-троек — разные виды информации; нельзя смешивать их эффекты. [Receipt1мая](C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E05-SYNTAX-FILTER/results/run-2026-05-01-001.json) сохраняет50.79%→65.97%. Размер вклада holdout-mask в эти15.18 процентного пункта не измерен. Польза train-only structural masks остаётся открытой положительной гипотезой.

Прочитанный E05 в snapshot `530c1e0581` прямо строит table из train+val и выбирает в ней `argmax trigram.Probability(prev2,prev1,next)`; baseline использует этот же trigram. Следовательно, риск информационного преимущества был и в прежнем source, а не только в новом переносе.

**Дополнительное отличие, которого нет в узкой helper-формулировке:** перенос commit **`d2b04be03454c90fc57ef21cdfd9679e04a9c5f5` от7мая** создаёт нынешний GrammarMaskFilter. В нём [25–30](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs:25) обучается `NgramPredictor`, а [44–47](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/GrammarMaskFilter.cs:44) ранжирование зависит от `Probability(prev1,token)` и tie по token ID. [NgramPredictor,323–370](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/NgramPredictor.cs:323) выбирает unigram или bigram по плотности наблюдений, не trigram. Нынешний E05 baseline по-прежнему TrigramModelV2.

То есть current baseline/enhanced отличаются одновременно mask и внутренним ранжированием. `prev2` участвует в выборе mask, но не вероятности candidate. Это может быть полезной защитой от разреженности, а может потерять различающий контекст; влияние по домену/данным не измерено. Нельзя объявить его автоматически деградацией.

### Поправка к будущему протоколу

В [micro E05-LK](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-MICRO-EXPERIMENT-HISTORICAL-QUALITY-RECHECK-2026-10-16.md>) плечо нынешнего GrammarMaskFilter названо `archive parity`. Это слишком широко: нынешний NgramPredictor не прежний trigram-ranker. Такое A/B корректно проверяет **влияние источников mask в текущем алгоритме**, если остальное одинаково; оно не воспроизводит весь архивный опыт.

Для будущей проверки нужно раздельно фиксировать два фактора: источник mask(train-only/full) и ранжирование(старый trigram/нынешний adaptive unigram-bigram). Сначала один фактор на одном frozen panel, затем другой; не выполнять полную большую матрицу автоматически. Словарь, OOV и fallback должны быть одинаковыми в сравниваемых плечах и явно описанными. Чистота нового исследования не требует переписывать старый receipt.

## E07 и чужие proxy

N30 снова описывает token-index/char-index mismatch и неиспользуемые parent/scope в tier. Это уже первично рассмотрено в [06,H-01](06-helper-findings-reconciliation.md); новое имя файла не создаёт новой причины. Предложенный Python использует вручную заданные куски `public`,пробел,`class` и т.д. Его7/8 mismatches не являются замером реального BPE/Roslyn корпуса.

[Python source](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-historical-seams-microproxy.py>) и [receipt](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/30-historical-seams-proxy-result.json>) прочитаны полностью. Они демонстрируют арифметику/lookup, не запускают рассматриваемые .NET методы. Значение E21 score10 в proxy задано вручную; найденная выше git lineage сильнее этой демонстрации для вопроса о реальном прошлом. Здесь proxy не исполнялся.

## Что остаётся открытым

Найденные перемены6–7мая устанавливают различия реализаций, но не runtime причинность. Нужны matched сохранённые данные либо отдельно разрешённые будущие проверки, подтверждение ordinary consumer и область применимости: C# AST, stack routing и разбиение составных запросов различаются. Самостоятельная нейронная генерация RP01 не установлена как consumer этих механизмов. Старые успешные результаты сохранены с их настоящим ограниченным смыслом.

Прочитаны конкретные helper N30-E05/E07, E20/E21, historical micro-design и оба Python artifacts. Остальные новые файлы перечислены в48 и не считаются обработанными этим досье. Source snippets/current history проверены; полнота binary provenance не заявляется. Изменён только Markdown.
