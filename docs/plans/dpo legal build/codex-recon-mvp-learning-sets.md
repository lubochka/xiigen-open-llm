# Codex Recon — `llm mvp/learning-sets` (структура обучающего сета, формат пар)

Сбор evidence для плана **ILearningDynamicGraph**. Только чтение + запись этого файла.
Под-вопрос: **как устроен формат обучающей пары (A / B / Reason и т.п.)** в наборе
`learning-sets`.

---

## 1. Что это и где (точные ключевые пути)

Корень области:
`C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\`

Реально найденные файлы (Glob по всей области):
- `MAPPING-DOCUMENT-TO-NODES.md` — мастер-карта: какой кусок исходных
  архитектурных документов идёт в какой узел UML-потока (NODE-01…NODE-09 + DATA-01).
- `NODE-01-DOMAIN-SPLITTER.md` — обучающий сет узла «Domain Splitter» (примеры в прозе).
- `NODE-02-DOMAIN-TRANSLATOR.md`
- `NODE-03-DECOMPOSER.md` — узел «Decomposer / Scheme planner» (прочитан частично, нач.).
- `FLOW-01-REGISTRATION-TRAINING-DATA.md` — обучающий сет Flow 01 (v1), формат A→узел→B.
- `FLOW-01-REGISTRATION-RUNTIME-TRAINING-DATA-v2.md` — v2 того же, 93 runtime-пары, 9 узлов.
- `flow-01-training-sets-v1\00-INDEX.md` — индекс комплекта v1 (перечисляет node-01…node-09,
  01-UML-FRAME.md, 10-SIMULATION-CHECK.md — эти под-файлы я НЕ открывал, защита от перегрузки).
- `previos session.txt` — лог сессии (не открывал; evidence-only).

Прочитано полностью/целево (3–8 файлов, как просили): `00-INDEX.md`,
`MAPPING-DOCUMENT-TO-NODES.md`, `NODE-01-DOMAIN-SPLITTER.md`,
`FLOW-01-REGISTRATION-RUNTIME-TRAINING-DATA-v2.md` (целиком),
`NODE-03-DECOMPOSER.md` (нач. ~120 строк), шапка `FLOW-01-REGISTRATION-TRAINING-DATA.md`.

Источник-исходник, на котором построен Flow 01 (упомянут в файлах, НЕ в этой папке):
`C:\Users\lubao\Documents\xiigen\business flows\01-user-registration.md`.
Авторитетный план счётчиков пар:
`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\FLOW-01-TRAINING-DATASET-PLAN-v3.md`
(тоже вне папки — для до-исследования).

---

## 2. Как устроено / работает (формат пары)

### 2.1 Базовый контракт пары: `A → обученный узел → B`
Это НЕ классическая DPO-тройка `(prompt, chosen, rejected)`. Это **supervised
input→output** пара (teacher-forcing / distillation-стиль). Ключевые поля
(терминология файлов, дословно):

- **`Runtime A:`** — вход узла «как в реальности» (то, что человек реально напишет
  или что узел реально получит на runtime). Правило v2: исходную строку документа
  НЕ кладут во вход, если человек её бы так не напечатал.
- **`Expected B:`** — конкретный выход. Жёсткое требование: B всегда конкретен
  («покажи реальные метки / реальный план / реальное решение арбитра»), запрещены
  формулировки вида «составь план» / «определи».
- **`Alternate A:` / `Alternate B:`** — другая формулировка того же концепта
  (минимум для разнообразия перефразировок; в NODE-01 — ≥3 фразировки на концепт).
- **`Source evidence:`** — ссылка-обоснование на исходный документ (почему пара
  легитимна). В v1 встречается также поле смысла «Reason» в выходе узлов-селекторов
  (например NODE-07 Pair 7.10: `Reason: …`). То есть «Reason» присутствует, но как
  часть В у решающих узлов, а не как отдельная rejected-ветка.

Пример (NODE-01, Pair 1.2, дословная форма):
```
Runtime A:  "Users should also be able to register with email ... verification link."
Expected B: [authentication, communication]
Alternate A: "...confirmation email before the account becomes active."
Alternate B: [authentication, communication]
Source evidence: The document defines email/password registration ... verification email.
```

### 2.2 Контракт цепочки: B одного узла = часть A следующего
Поток из 9 обучаемых целей + 1 хранилище данных (DATA-01 Prompt Library). Узлы:
1. Domain Splitter: промпт → метки доменов `d1..dm`.
2. Domain Translator: фрагмент + домен → формальная доменная формулировка.
3. Decomposer / Scheme Planner: все доменные формулировки → куски `b1..bn` + инструкции `fb1..fbn`.
4. Domain Processor (массив): один `bi` → обработанный результат.
5. Applier (массив, с self-loop): обработанный `bi`+`fbi` → конкретный результат `Rqi`.
6. Result Translator (массив): `Rqi` → человеческий язык (видит все `R1..Rn` для согласованности).
7. Arbiter Selector: все `R1..Rn`+контекст → набор арбитров.
8. Arbiter Units: результат → `pass` ИЛИ `flag + fbx` (фрагмент уходит обратно во фронт потока).
9. Formatter: одобренные фрагменты → единый ответ.

Массивы узлов (4,5,6) исполняются параллельно (fan-out/fan-in). Арбитры работают
ДО форматтера и дают петлю обратной связи (reroute → Domain Splitter). Self-loop у
Applier — повтор при недостаточном качестве.

### 2.3 Негативные / «не делай» примеры (важно для DPO-плана)
Прямых rejected-полей в тройке нет, но «отрицательная» сигнатура реализована тремя способами:
- **Failure-кейсы** (NODE-01 Batch 3): пары «Wrong behavior / Correct behavior»
  (например «не решай запрос», «не добавляй непрошенные домены»).
- **Feedback-correction** (NODE-01 Batch 4): «Original output → Feedback received →
  Corrected output» — учит, КАК исправляться после обратной связи.
- **Arbiter flag-пары** (v2 NODE-08): пара = плохой результат + `Flag` (почему плохо
  со ссылкой на документ) + `fbx` (как переписать). Это фактический материал для
  построения preference-пары (текущий результат = rejected, fbx-исправление = chosen).
- **Consistency-rejection** (v2 Pair 6.12): узел отказывается переводить
  противоречивый прогон и шлёт на реарбитраж.

### 2.4 Объёмы и самопроверка
v2 итог: 93 runtime-пары (10/10/10/10/10/12/10/11/10 по узлам). Каждый файл-сет
несёт собственный self-check (только natural language; объяснять ПОЧЕМУ; ≥3
фразировки; failure-кейсы; формат вывода стабилен) и блок Simulation/Test-кейсов.

---

## 3. Релевантность к ILearningDynamicGraph

Очень высокая — это, по сути, **готовый корпус-черновик и контракт I/O для узлов
будущего ILearningDynamicGraph**:

- **DPO/обучение:** даёт форму обучающих рядов на узел (A/B + Alternate + evidence)
  и три источника негативов (failure / feedback-correction / arbiter-flag). Для
  настоящего DPO эти ряды нужно ДОстроить до `(prompt, chosen, rejected)` —
  материал для rejected уже есть в flag/feedback/wrong-behavior блоках.
- **Граф/оркестрация:** прямое описание узлов, рёбер, fan-out/fan-in, массивов,
  self-loop, обратной петли арбитров → ровно то, чего требует Graph-Style /
  DynamicGraph Reconstruction Gate (роли/иерархия, nodes/edges, input/output
  contracts, many-to-many, async, кто владеет авторитетом на ребре).
- **Измерение:** Simulation-Checks + «Missing-if-fail» + per-node self-check — основа
  для held-out метрик и для ablation/coverage по каждому узлу.
- **Teacher-provenance:** **ПРОБЕЛ/РИСК.** В файлах НЕ указано, какой моделью
  сгенерированы `Expected B`. Если B писал закрытый проприетарный учитель
  (Claude/OpenAI/Gemini), то по Legal Teacher / Provenance Gate скила это
  `DATASET_REBUILD_REQUIRED` для DPO/дистилляции. Поля `teacher_access_mode`,
  `model_id`, `model_license`, `provider_terms_checked` отсутствуют — обязательны к
  заполнению до использования как training rows. Тексты выглядят как ручная/
  модельная экстракция из одного бизнес-документа — это надо явно подтвердить.

---

## 4. Классификация (по trainable-architecture-decision-SKILL.md)

Здесь нет исполняемого кода — это **датасет/контент-артефакты**, не реализация.
Поэтому классифицирую не «уровень класса», а **намерение узлов**, которое сет
описывает, относительно разрешённых уровней:

- **Каждый узел NODE-01…NODE-09** в формулировке файлов = «trained LLM unit, A→B».
  Целевой разрешённый уровень — **`ILogicalUnit<TInput,TOutput>`** (чистый inference;
  по A-LM-02 без Train*-методов внутри). Сами по себе ряды сета — это
  training-данные для такого юнита.
- **Поток из 9 узлов + петли/массивы** = **`ILogicalFlow` / `ILogicalOrchestrator`**
  (топологическое планирование, fan-out массивов 4–6, reroute-петля арбитров,
  self-loop Applier). Это уровень оркестрации, а не один юнит.
- **Domain Splitter / Arbiter Selector** в текстах названы «multi-label / which
  arbiters to fire». По скилу `selector`/`classifier`/`router` сами по себе — **НЕ
  архитектурный уровень**, а внутренняя деталь. Они допустимы только обёрнутыми в
  `ILogicalUnit`/flow с lifecycle-доказательствами (checkpoint, export/import,
  locator, fresh-load, continue-training, ablation). Правило:
  «scorer/selector/classifier/router … are not architecture patterns … may be
  implementation details inside an allowed level only» (CLAUDE.md / skill).
- **DATA-01 Prompt Library** = хранилище данных, не обучаемый юнит (в самих файлах
  помечено «rounded box = data, not a processing unit»).
- **Детерминистических хелперов как финальной runtime-авторитетности в датасете
  нет** (это датасет). Но при реализации Arbiter/Splitter есть риск, что их сведут
  к regex/dictionary/lookup — это был бы `FORBIDDEN_RUNTIME_SHORTCUT` / нарушение R4
  (No hidden determinism). Сет сам по себе — `OK` как training corpus при условии
  закрытия provenance-гейта; иначе по dataset-вердиктам скила ближе к
  `KEEP_FOR_EVAL_ONLY` / `REWRITE_TO_DPO` (нужно дострой до троек) до проверки лицензии.

Итоговый dataset-вердикт (предварительный): **`REWRITE_TO_DPO` + provenance-gate
pending**. Ряды качественные и доменно-чистые (один источник), но (а) это
supervised A→B, не DPO-тройки; (б) не подтверждён легальный учитель.

---

## 5. Пробелы / что доисследовать

1. **Teacher-provenance (критично):** кто/чем сгенерировал `Expected B`. Без этого
   нельзя классифицировать как training-данные DPO/дистилляции.
2. **Не открыты под-файлы v1:** `flow-01-training-sets-v1\01-UML-FRAME.md`,
   `node-02…node-09-*-set.md`, `10-SIMULATION-CHECK.md` — там точные рёбра/контракты
   (01-UML-FRAME назван «binding frame for all node files»). Нужны для рёбер графа.
2-bis. `NODE-02-DOMAIN-TRANSLATOR.md` и хвост `NODE-03-DECOMPOSER.md` прочитаны не полностью.
3. **Внешние авторитетные планы вне папки:** `FLOW-01-TRAINING-DATASET-PLAN-v3.md`
   (точные счётчики пар, доменные имена), `DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`,
   `llm_pipeline_corrected.svg` / `improve algo.drawio.xml` — топология/петля арбитров.
4. **Несоответствие имён узлов между файлами:** MAPPING называет NODE-07=Formatter,
   NODE-08=Arbiter Selector, NODE-09=Arbiter Units; а v2/INDEX — NODE-07=Arbiter
   Selector, NODE-09=Formatter. Нужно зафиксировать каноническую нумерацию для плана.
5. **Нет rejected-ветки в явном DPO-виде:** план должен задать правило построения
   троек из failure/feedback/arbiter-flag блоков (что = chosen, что = rejected,
   причина утечки/ошибки) и запретить обучение на shortcut-сигналах
   (порядок кандидатов, id источника, точный реплей промпта).
6. **Лицензия/легальность корпуса:** источник — приватный бизнес-документ Любы →
   это user-adaptive/private материал, не common-training, пока явно не одобрено
   (R5 + external-source license gate).
7. `previos session.txt` не прочитан (evidence-only).

---

### Заметки о границах задачи
- CURRENT TASK: рекон формата обучающего сета в `learning-sets` для плана ILearningDynamicGraph.
- NOT THE TASK: реализация/тренировка/правка кода, правка чужих/claude-* файлов,
  продолжение чужих планов (`dcm-*`, dynamic-flow артефакты — evidence-only).
- EVIDENCE-ONLY: все исходные документы, `previos session.txt`, внешние планы вне папки.
