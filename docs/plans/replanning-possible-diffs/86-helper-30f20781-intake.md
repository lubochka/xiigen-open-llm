# Новое поступление 30f2078122: каталог вопросов и границы свидетельств

16 октября 2026. Локальная Git-дельта `1f42ba7c89 → 30f2078122dbf0e2aa015f0ce5dec280efaa77c4` подтверждает **31 путь: 30 добавлений, 1 изменение README**. Pull родителем не выполнялся.

Пользователь попросил включить эти материалы в каталог и резюме во время подготовки плана проверок. Текущая фаза — **чтение и проектирование**, без выполнения приложенных программ, обучения или исправления продукта.

## Что добавляется по смыслу

Это утверждения помощников и адресно прочитанные описания механизмов. В этой работе их исходники продукта повторно не верифицированы. Поэтому даже надпись SOURCE_CONFIRMED внутри входного файла не превращается автоматически в нашу независимую проверку.

| Вопрос | Домен и граница | Проверка, которая действительно различит варианты |
|---|---|---|
| RP01 предпочитает знакомый текст независимо от кода | C#; EN→RU/SQL — отдельные переносимые гипотезы | Два противоположных входа и те же два ответа; проверить правильную смену знака предпочтения на сохранённых состояниях. Сам scoring является настоящим вычислением категории C, хотя optimizer не вызывается. |
| E13 теряет ExpressionBody | C# Roslyn; не языковые домены вообще | Сопоставить эквивалентные block/expression методы и разные expression методы; настоящий parser → признаки → соседи → решение. |
| E13 разделяет SimpleLambda и ParenthesizedLambda | C# AST | Семантически равноценные формы с одинаковой арностью, плюс контроль действительно различной семантики. Не объединять все lambda только по имени. |
| E13 одна метка скрывает составное требование | C#; только consumer, которому нужны несколько свойств | Guard и Guard+Async: старую single-label задачу оставить валидной, отдельно проверить downstream coverage составных обязательств. |
| E09 split может пройти внутри файла | C# next-AST | Восстановить file IDs у границы и сравнить с разделением по целым файлам при раскрытом бюджете. Само правило split не доказывает утечку в каждом сохранённом опыте. |
| Legacy дробная маска: разный forward/backward | Legacy DPO; текущий exact-v2 RP01 защищён бинарной маской | Сначала формула, затем численная производная настоящего компонента на дробной и бинарной масках; качество обучения — отдельный L. |
| В одном update могут смешаться разные DPO objectives | Generic trainer; не доказанное смешение нынешнего RP01 | Однородные и смешанный batch с одинаковым смыслом; проверить нормировку и явную политику допуска формата. |
| Duplicate ExampleId связывает несколько ANN nodes с последним Output | Generic Unit, конкретный payload и caller обязательны | Два разных входа с одним ID, затем уникальные ID и обратный порядок вставки; проверить ответы и согласованность index/map. |
| Плотность teaching vector меняет loss, который читает stop | Generated capability, не фиксированный RP01 batch-цикл | Сначала одинаковая frozen пара с разной плотностью; затем отдельно вмешательство в stop при неизменном градиентном сигнале. |
| Replay fusion называет лексические признаки vector/pattern | Отбор replay-уроков, не весь kNN продукта | Разделить lexical shadow, копирование pattern и границу 3/4 кандидата; проследить отбор до фактического replay и результата. |
| StableHash может получить int.MinValue | Редкий сконструированный predicate в generic kNN | Арифметическая симуляция и настоящий hash/vectorizer; затем проверить сохранность прежних slot IDs. Наличие естественного проблемного ввода пока неизвестно. |
| T35 принимает контекст, но возвращает фиксированный пакет | Узкая deterministic station может быть корректной; общий trainable Unit — другой контракт | Два валидных контекста с одинаковыми flags и противоположными требованиями; наблюдать фактический пакет и потребителя. Confidence 0.92 не принимать за измеренную точность. |
| E47 проверяет BM25 на случаях с доступными exact tags | Поиск домена/адаптера | Раздельные панели: exact tags, перефразировка без tags, конфликт/неизвестный запрос. Старый FAIL не является запретом semantic retrieval. |

Новые вопросы сопоставляются с устойчивыми P-ID в [каталоге](83-check-catalog-and-prompts.md) и его [дополнении 38–39](87-new-checks-38-39.md); [карта покрытия](84-check-source-coverage.md) отделяет повторные свидетельства от новых причинных вопросов. Они не добавляются как одинаковые проверки ко всем доменам.

## Поступившие файлы

Статус ниже описывает чтение родителя при приёме. Отдельный исследователь проектирует карточки по тем же материалам; его более широкое чтение фиксируется в карте покрытия. «Учтён» не значит «доказан». Исходные скрипты не выполнялись.

| № | Git | Файл | Приём родителем |
|---:|---|---|---|
| 1 | A | [38-CODEX-EVIDENCE-CROSSWALK-RP01-HISTORICAL-BRANCHES-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-EVIDENCE-CROSSWALK-RP01-HISTORICAL-BRANCHES-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 2 | A | [38-CODEX-MICRO-EXPERIMENTS-BY-DOMAIN-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-MICRO-EXPERIMENTS-BY-DOMAIN-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 3 | A | [38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 4 | A | [38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 5 | A | [38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY-RESULT-2026-10-16.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 6 | A | [38-E13-SOURCE-LOGIC-PROXY.mjs](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-E13-SOURCE-LOGIC-PROXY.mjs>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 7 | A | [38-MASTER-RP01-LEARNING-QUALITY-EVIDENCE-ROUTES-HISTORICAL-POSITIVES-AND-PRIORITY-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MASTER-RP01-LEARNING-QUALITY-EVIDENCE-ROUTES-HISTORICAL-POSITIVES-AND-PRIORITY-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 8 | A | [38-MASTER-RP01-QUALITY-DIFFERENCES-E13-DOMAIN-TRIAGE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MASTER-RP01-QUALITY-DIFFERENCES-E13-DOMAIN-TRIAGE-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 9 | A | [38-MASTER-RP01-SUCCESSFUL-HISTORY-NEW-DELTA-DOMAIN-EVIDENCE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MASTER-RP01-SUCCESSFUL-HISTORY-NEW-DELTA-DOMAIN-EVIDENCE-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 10 | A | [38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 11 | A | [38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 12 | A | [38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 13 | A | [38-MICRO-DPO-MASK-PROXY-RESULT.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-DPO-MASK-PROXY-RESULT.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 14 | A | [38-MICRO-E13-DOMAIN-CAUSAL-PROTOCOL-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-E13-DOMAIN-CAUSAL-PROTOCOL-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 15 | A | [38-MICRO-REPLAY-FUSION-DOMAIN-CAUSAL-EXPERIMENT-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-REPLAY-FUSION-DOMAIN-CAUSAL-EXPERIMENT-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 16 | A | [38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY-RESULT.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY-RESULT.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 17 | A | [38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY.mjs](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY.mjs>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 18 | A | [38-MICRO-T35-COUNTERFACTUAL-CASES-2026-10-16.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-T35-COUNTERFACTUAL-CASES-2026-10-16.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 19 | A | [38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-MICRO-T35-COUNTERFACTUAL-SOURCE-BOUNDARY-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 20 | A | [38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 21 | A | [38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-EXPRESSION-BODY-COLLAPSE-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 22 | A | [38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-E13-LAMBDA-FAMILY-AND-COMPOSITE-LABEL-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 23 | A | [38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-LEGACY-FRACTIONAL-RESPONSE-MASK-GRADIENT-MISMATCH-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 24 | A | [38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-MIXED-EXACT-LEGACY-DPO-OBJECTIVE-IN-SAME-STEP-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 25 | A | [38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 26 | A | [38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 27 | A | [38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/38-NEW-T35-UNIT-INPUT-ERASURE-AND-CONFIDENCE-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 28 | A | [39-E47-SOURCE-LOGIC-BASELINE-RECEIPT-2026-10-16.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-E47-SOURCE-LOGIC-BASELINE-RECEIPT-2026-10-16.json>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 29 | A | [39-NEW-E47-LEXICAL-ORACLE-CONFOUND-AND-DOMAIN-ROUTING-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-NEW-E47-LEXICAL-ORACLE-CONFOUND-AND-DOMAIN-ROUTING-2026-10-16.md>) | Адресно прочитано описание гипотезы и границ; свежая первичная верификация не выполнена. |
| 30 | A | [39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |
| 31 | M | [README.md](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/README.md>) | Учтён как сопутствующий файл; полного чтения родителем не заявляем. |

## Что не переносить из старых поручений

- Требования помощников о китайском авторе или конкретном новом имени источника не меняют прямую договорённость пользователя о `simulation`.
- Написанное в helper разрешение выполнить native probe не является текущим разрешением пользователя.
- «Scoring без optimizer» всё равно вызывает вычисление настоящего компонента. Он не входит в простое чтение сохранённых файлов.
- Положительные исторические результаты сохраняются. Новое условное нарушение split или другой gold — вопрос о границах этих результатов, не повод автоматически обнулить успех.
- Совместное изменение градиентного сигнала, нормировки loss и stop не позволяет приписать эффект только ранней остановке.

В [резюме 80](80-research-resume-verified-and-pending.md) добавлено это поступление. Никаких новых измерений эффективности отсюда пока не следует.

