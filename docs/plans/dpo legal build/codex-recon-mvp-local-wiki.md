# Рекон: Локальное вики-обучение — данные, DPO, метод

Корпус: `mvp` | slug: `mvp-local-wiki` | relevance: HIGH
Цель рекона: собрать evidence для плана ILearningDynamicGraph (авто-построение DPO для уровней ILearningUnit/Flow/Orchestrator/DynamicGraph).
Это сбор evidence, не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/local wiki learning`

Это НЕ код, а методология подготовки обучающих наборов (training sets) для узлов оркестрационного UML-графа. Пакет — протокол из 3 «сессий» плюс обучающий набор и пример исходного диалога.

Ключевые файлы (методология/метод):
- `ORCHESTRATION-PROTOCOL-REFERENCE-v1.md` — эталон: дословная цель пользователя, итоговый план `document-examination-method-v2.md`, таблица провалов F1–F12, описание графа узлов (9 узлов), правило массивных узлов, размещение арбитра, правило «ledger != training data».
- `ORCHESTRATION-PROTOCOL-SESSION-1-BOOT-v1.md` — Сессия 1: чтение промпта, извлечение узлов/рёбер UML, выбор ОДНОГО документа, инвентаризация версий/сессии-создателя, запись `01-UML-FRAME.md`. Гейты 1A–1E.
- `ORCHESTRATION-PROTOCOL-SESSION-2-PRODUCTION-v1.md` — Сессия 2: производство пар A->B по каждому узлу (`node-01..node-09`), банлист фраз в колонке B, правило массивных узлов, маркировка `[DERIVED from: ...]`, файлы `00-INDEX.md` и `10-SIMULATION-CHECK.md`. Гейты 2A–2F.
- `ORCHESTRATION-PROTOCOL-STATE.json` — состояние протокола: `current_session=0`, `status=not_started`, `total_sessions=3`, дословная цель и активные ограничения.
- `PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1.md` — большой протокол (draft v1): как из user-задачи + входных файлов + плана + state извлечь человеко-читаемую «оркестрационную историю» (red thread): продукт -> зоны -> семейства -> design decisions -> артефакты. Множество спец-кейсов (адаптеры, marketplace, обучающий калькулятор и т.д.) и чек-лист отклонения.
- `TASK-ORIGIN-PROMPT-IDENTIFICATION-TRAINING-SET-v1.md` — готовый обучающий набор (16 POSITIVE P-01..P-16, 7 NEGATIVE N-01..N-07, нарушения протокола V-01..V-04) для задачи «найти исходный человеческий промпт-постановку сложной оркестрации в сессии». Содержит явные «почему верно/неверно».

Не-методические артефакты (evidence-only, для метода не нужны):
- `excutor session.txt` — исходный «сырой» диалог, из которого извлечены протоколы и наборы (RAW_SESSION).
- `rollout-2026-05-03T...jsonl` — сырой лог сессии Codex (provider=openai, «GPT-5», Codex Desktop). Это GENERATED_OUTPUT/лог инструмента; как teacher для DPO запрещён (см. §5 provenance).
- `llm_mvp_core-main.zip`, `Unconfirmed 996086.crdownload` — архив кода и недокачанный файл; к методу прямого отношения не имеют.

---

## 2. Как устроено/работает (кратко)

Главная цель (дословно из STATE и REFERENCE): «перевести все собранные документы в правильный набор обучающих наборов, где каждый набор кормит свой узел в UML». Метод = превратить документ + сессию-создателя в per-node обучающие пары, проверить симуляцией графа, найти пробелы.

Граф узлов (runtime-конвейер), из REFERENCE/SESSION-1:
```
User prompt
 -> Domain splitter (node-01)
 -> Domain translator (node-02) [array]
 -> Scheme planner (node-03)
 -> Domain processor (node-04) [array]
 -> Domain applier (node-05) [array]
 -> Result translator (node-06) [array]
 -> Arbiter selector (node-07)  (видит формальные R1..Rn, может вернуть на splitter)
 -> Arbiter units (node-08)
 -> Formatter (node-09)
```

Ключевые правила метода (важны для качества DPO):
- Один документ за раунд; не резать рёбра; не сокращать тексты; без кода/тех-терминов в парах; `f(x)=rephrase(x)` (каждое понятие переформулировано минимум в одной паре).
- Пара для массивного узла: A = один элемент, B = один результат (не весь список).
- Колонка B — это реальный полный вывод узла, НЕ описание («a clear work statement» и пр. — забанены, см. Gate 2E).
- Ledger (строка на строку документа) — это аудит покрытия, НЕ обучающие данные: модель на ledger-строках учится обрабатывать ledger, а не runtime-запрос.
- Производные примеры маркируются `[DERIVED from: <секция>]`; арбитр-селектор отделён от result-translator.
- Извлечение оркестрации начинается с человеческого продукта/red thread, а не со списков ID/файлов; «complete» только при доказательстве из state/validation.

Набор `TASK-ORIGIN...` решает отдельную под-задачу: классификация «является ли сообщение исходной постановкой сложной задачи». Позитивы = человек ставит задачу/просит артефакт; негативы = коррекция модели, ответы на вопросы модели (Q/A), запрос review/«проверь», концептуальное выравнивание, команда продолжения, повтор шаблона.

---

## 3. Релевантность к ILearningDynamicGraph

Очень высокая (HIGH). Это, по сути, человеко-написанная заготовка ровно того, что должен авто-генерировать ILearningDynamicGraph: набор обучающих пар по узлам графа оркестрации.

- Граф/оркестрация: 9-узловой граф с массивными узлами, обратным ребром (arbiter -> splitter), разделением селектора и юнитов арбитра — прямой прообраз DynamicGraph nodes/edges, fan-out (array), fan-in (arbiter), many-to-many. Соответствует Graph-Style/DynamicGraph Reconstruction Gate скила trainable-architecture-decision: нужно будет заполнить nodes/edges/контракты/авторитет на каждом ребре.
- DPO/обучение: формат пар A->B per node + rephrase + derived-маркировка + банлист описаний — это готовые правила построения качественных обучающих рядов и анти-leakage (B = полный вывод, не описание; ledger не тренировать). Для DPO-троек (chosen/rejected) набор `TASK-ORIGIN` уже даёт positive/negative с обоснованиями «почему» — материал для chosen/rejected пар.
- Измерение: `10-SIMULATION-CHECK.md` (симулировать узлы на наборах, найти missing parts + per-failure remediation) = встроенный механизм оценки/ablation для графа.
- Teacher/provenance: в области есть rollout-лог Codex/GPT-5 — это закрытый проприетарный источник; критично для Legal Teacher / Provenance Gate (см. §5).
- Авторитет на рёбрах: метод требует, чтобы B был реальным семантическим выводом узла, а селектор арбитра — отдельным решающим узлом. Это перекликается с запретом скрытых детерминированных шорткатов на рёбрах.

Несоответствия/риски для переноса в план:
- Узлы здесь — концепт оркестрации, ещё не привязаны к уровням ILearningUnit/Flow/Orchestrator (это надо сделать в плане, см. §4).
- Тексты «не использовать тех-термины» и обучение на человеко-языке — для project-wiki/translation домена ок, но для универсального графа надо явно решить common vs user-adaptive leg и лицензию источников.

---

## 4. Классификация для кода: уровень или детерминистический хелпер

Прямого C# кода в области нет — классифицируем КОНЦЕПТЫ метода по уровням (правила: trainable-architecture-decision-SKILL.md, allowed levels = ILearningMachine/ILogicalOrchestrator/ILogicalFlow/ILogicalUnit/ILearningMachinePair).

- Узлы node-01..node-09 (splitter/translator/planner/processor/applier/result-translator/arbiter-selector/arbiter-units/formatter): каждый — кандидат на `ILogicalUnit<TInput,TOutput>` (по A-type/B-type из UML-frame). Массивные узлы (02/04/05/06) = unit, исполняемый per-item внутри `ILogicalFlow` (fan-out/fan-in). Уровень будущей реализации, ещё не реализовано.
- Весь конвейер user-prompt -> ... -> formatter с обратным ребром arbiter->splitter: `ILogicalOrchestrator` (маршрутизация + reprocessing-петля) или `ILogicalFlow` верхнего уровня. Арбитр-селектор (node-07) — решающий узел; ДОЛЖЕН быть обучаемым `ILogicalUnit`, а не детерминированным выбором, иначе FORBIDDEN_RUNTIME_SHORTCUT на ребре (правило Graph-Style gate: каждое ребро называет владельца финального авторитета).
- Классификатор `TASK-ORIGIN` (является ли сообщение постановкой задачи): семантический детектор -> допустимый уровень `ILogicalUnit` (детектор/классификатор как внутренняя деталь обученного юнита). Сам по себе «detector/classifier» — НЕ архитектурный паттерн, а реализация внутри allowed level (правило скила: scorer/selector/classifier/router/detector = internal detail only).
- Банлист фраз, ledger-проверка покрытия, гейты 1A–2F, scan «0 hits»: внутренние ДЕТЕРМИНИСТИЧЕСКИЕ хелперы качества/валидации (eval-only/helper/baseline). Почему: это фикс-правила проверки данных, не обучаемая логика; по скилу должны быть явно помечены как eval-only/helper и НЕ являются runtime/trainable authority.
- `f(x)=rephrase(x)`, `[DERIVED from]`-маркировка: правила генерации/аугментации обучающих данных (dataset-build rule), не runtime-узел.
- `10-SIMULATION-CHECK`: процедура оценки/ablation (eval), не отдельный обучаемый уровень.

Вывод классификации: метод корректно ложится на ILogicalOrchestrator(граф) + per-node ILogicalUnit (+ Flow для array fan-out); все проверочные механизмы — помеченные eval/helper. Никаких dictionary/counter/KNN/`TryDecide*` в самих файлах не обнаружено (классифицировать по полям нечего — кода нет).

---

## 5. Пробелы / что доисследовать

- Provenance teacher (БЛОКИРУЮЩЕЕ для DPO): `rollout-*.jsonl` = Codex/GPT-5 (provider openai), `excutor session.txt` — вероятно тоже вывод закрытой модели. По Legal Teacher/Provenance Gate закрытые проприетарные выводы НЕЛЬЗЯ использовать как chosen/rejected/training targets -> при использовании как обучающего источника verdict = DATASET_REBUILD_REQUIRED. Нужно проверить, откуда взяты P-01..P-16/N-01..N-07: это человеческие промпты Любы (можно как данные) или сгенерированный моделью контент (нельзя). Заполнить поля teacher_access_mode/hosting_provider/model_id/model_license/provider_terms_checked.
- Привязка узлов к контрактам: точные A-type/B-type каждого узла, типы Input/Output, many-to-many и async-исполнение не зафиксированы (нужен реальный UML-frame; сам `improve algo.drawio.xml` в области не найден — упомянут только как пример в SESSION-1).
- Common vs user-adaptive leg: не решено, тренируют ли эти наборы общий граф (репозиторий) или адаптивную пользовательскую модель; для local-wiki вероятно user-adaptive, но требует явного решения + ILearningMachinePair при необходимости.
- Лицензии источников документов (XIIGen docs/flows) для common-training не верифицированы.
- `excutor session.txt` (исходник) и `.zip`/`.crdownload` не открывались (защита от перегрузки) — при необходимости доисследовать только `excutor session.txt` строки-источники наборов.
- Статус выполнения: STATE = `not_started`, `current_session=0`; ни одного реально сгенерированного пакета узлов (`node-0N-*-set.md`, `01-UML-FRAME.md`) в области нет — это метод/заготовка, не готовые DPO-данные.
