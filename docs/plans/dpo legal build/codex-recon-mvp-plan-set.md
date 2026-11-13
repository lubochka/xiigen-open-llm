# Codex Recon — Как планируются обучающие сеты (llm mvp / plan the learning set)

Цель рекона: собрать evidence для плана **ILearningDynamicGraph** (авто-построение DPO для
ILearningUnit / ILearningFlow / ILearningOrchestrator / ILearningDynamicGraph).
Вопрос: **как в этой области планируются обучающие (training) сеты**.
Статус: только чтение + запись этого файла. Это evidence, не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Все пути в каталоге `C:\Users\lubao\Documents\xiigen\llm mvp\plan the learning set\`:

- **`EXAMINATION-METHOD-FINAL.md`** (1643 строки) — ГЛАВНЫЙ, самодостаточный метод
  превращения ЛЮБОГО документа (внутреннего или внешнего) в пер-узловые training-сеты
  для dynamic-flow конвейера. Явно сказано «supersedes all prior versions». Дубликаты:
  `EXAMINATION-METHOD-FINAL (1).md`, `EXAMINATION-METHOD-FINAL (2).md`, и
  `EXAMINATION-CONTINUATION-PLAN-v1.0.md` (последний — побайтово тот же текст метода).
- **`EXTERNAL-SOURCES-FULL-PIPELINE-PLAN-v1.md`** (701 строка) — авторитетный
  сквозной конвейер из 7 стадий (Stage 0..7): от выбора источника и лицензии до
  компиляции датасета и **теста обучаемости оркестратора**. Объединяет много прежних гайдов.
- **`DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md`** (690 строк) — метод для
  ВНУТРЕННЕГО документа: 7 «семейств» строк (Families 1-7), форма строки (16 полей),
  6 симуляций, acceptance-чеклист. Источник: `...\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
  + диаграмма `improve algo.drawio.xml`.
- **`FLOW-01-USER-REGISTRATION-DYNAMIC-FLOW-LEARNING-BATCH-v1.md`** (604 строки) —
  КОНКРЕТНЫЙ материализованный батч по одному документу (`business flows\01-user-registration.md`):
  строки L001..L357, каждая привязана к узлам конвейера. Это «как выглядит готовый сет».
- **`EXTERNAL-SOURCES-EXAMINATION-PLAN-v1.md` / `-v2.md`**, **`EXTERNAL-SOURCE-REGISTRY-v1.0.md`** —
  реестр внешних источников и лицензий (вход для Stage 0). (Прочитан по содержанию через
  таблицы реестра внутри FINAL и FULL-PIPELINE.)
- **`ANALYSIS-MATRIX.md` + `ANALYSIS-STATE.json`** — ОТДЕЛЬНОЕ усилие: 116-раундовый
  разбор `docs.zip` (по 1 файлу за раунд) ради плана
  `ORCHESTRATOR-TRAINING-CLAUDE-CODE-PLAN-v1.md`. Это про планирование оркестратора,
  а не про метод сетов; полезно как контекст источников обучения.
- `codex open source research.txt` — заметки по open-source источникам (не открывал детально, защита от перегрузки).

Скилы классификации (просмотрены): `docs/skills/trainable-architecture-decision-SKILL.md`
(есть прямой **Graph-Style / DynamicGraph Reconstruction Gate** и **Legal Teacher / Provenance Gate**),
`docs/HOW-TO-USE-SKILLS.md` (триггеры для graph/DPO/teacher).

---

## 2. Как устроено / работает (кратко)

### Конвейер-мишень (одинаков для всех сетов)
9 узлов, 4 из них — массивы (одна функция параллельно к каждому элементу):
1 Domain splitter → 2 Domain translator[] → 3 Scheme planner → 4 Domain processor[] →
5 Applier[] → 6 Result translator[] → 7 Arbiter selector → 8 Arbiter units → 9 Formatter.
Арбитры могут отправлять фрагменты назад в Node 1 (reprocessing loop). Это и есть граф с
fan-out/fan-in и обратными рёбрами — прямой аналог ILearningDynamicGraph.

### Что такое «обучающая строка» = пара A→B (ядро метода FINAL)
- **A** — точный РАНТАЙМ-вход узла, целиком, не метка и не описание (для Node 1 — реальный
  промпт пользователя; для Node 2 — фрагмент + ярлык домена; для Node 3-9 — выход предыдущего узла).
- **B** — точный РАНТАЙМ-выход узла, целиком (реальный план/перевод/действие), НЕ описание выхода.
- **Rephrasing rule**: каждая пара минимум дважды (та же ситуация под другим углом, без потери глубины).
- **Node 8** обязан иметь хотя бы один pass-кейс (иначе арбитр учится «флагать всё»).
- **Array-узлы (2,4,5,6)**: одна пара на один элемент, нельзя «4 домена в одной паре».
- Метод построен на разборе 5 провалов прошлых zip-сетов (A=строка документа; B=описание;
  нет rephrasing; нет pass-кейсов; симуляция проверяла «все строки представлены» вместо «пары дают верный выход»).

### Планирование внутреннего документа (DYNAMIC-FLOW-FIRST-FILE)
Один документ за раунд (One-Document Rule). Шаги: назвать документ → найти все версии →
найти session/history evidence (creation / candidate / later-use / unrelated) → прочесть диаграмму
как mapping-frame (узлы+рёбра, по 4 вопроса на ребро) → прочесть документ целиком.
Затем 7 семейств строк: (1) строки документа, (2) строки сессии-создания, (3) поздние ссылки,
(4) узлы диаграммы, (5) рёбра диаграммы, (6) версии, (7) симуляции. Форма строки = 16 полей
(источник, файл, № строки, оригинальный текст, узел, входное/выходное ребро, что забудется без строки,
симуляция-проверка и т.д.). 6 симуляций проверяют, что по сетам реально реконструируется архитектура/граф.

### Планирование внешних источников (EXTERNAL-SOURCES-FULL-PIPELINE) — 7 стадий
- **Stage 0** Выбор источника + ЛИЦЕНЗИОННЫЙ гейт (TRAINING_OK / ATTRIBUTION_REQUIRED /
  ShareAlike / public-domain / VERIFY_BEFORE_USE / REMOVED) + фазовая классификация документа.
- **Stage 1** Документ → дизайн эксперимента (центральный вопрос с позитивной И негативной половиной;
  5 oracle-документов; измеримые acceptance-критерии).
- **Stage 2** Прогон 3 сценариев (позитив / худший негатив / граничный) → evidence-файл из 6 секций
  (baseline-флор тестов; verbatim-трейсы; сравнение с oracle; вердикт PASS/PASS_WITH_GAPS/CONCERN;
  каталог пробелов structural/contract/judgment; next action).
- **Stage 3** Производство сета из evidence: 3 действия строго по очереди — correction pass
  (только правки, 5 pre-promotion checks), gap closure pass (новые пары по правилам формы A→B),
  coverage extension pass (узлы, что были «проксированы» человеком).
- **Stage 4** Компиляция датасета: 4 обязательные аннотации на файл (source-id, license class,
  attribution, anonymisation); 4-стратовый стратифицированный сплит перед 100/20 train/test; волны 1-5.
- **Stage 5** ТЕСТ ОБУЧАЕМОСТИ оркестратора: 5 категорий по 4 документа (роутинг ≥0.70 conf;
  детекция knowledge-edge без выдуманных ответов; cross-sub-phase; конфликты; edge-cases),
  порог 17/20; отдельный **learning-signal test** (baseline-провал → 1 корректирующая пара →
  повтор: тот же провал не повторяется); cross-phase transition test.
- **Stage 6** Анти-регрессия (волновой regression-check ≥17/20 на прошлых волнах) + adaptation test.
- **Stage 7** Обновление реестра + выбор следующего батча (приоритет по числу Category-2 провалов).

---

## 3. Релевантность к ILearningDynamicGraph

ВЫСОКАЯ. Это фактически готовый чертёж того, как наполнять граф обучающими данными:

- **Граф/оркестрация**: конвейер из 9 узлов = узлы графа; reprocessing-loop и array-узлы =
  fan-out/fan-in и обратные рёбра. Каждое ребро в методе описывается 4 вопросами
  (что выходит / что принимает цель / что цель производит / что ломается без ребра) — это ровно
  то, что требует **Graph-Style / DynamicGraph Reconstruction Gate** скила (nodes/edges/contracts).
- **DPO/обучение**: пара A→B — это «chosen»-сторона; негативные/rejected-примеры присутствуют как
  rejected-планы (AOSA «rejected alternative as negative example»), CONCERN/reprocess арбитра и
  «bad pair» анти-паттерны. То есть материал для DPO-троек (chosen/rejected/reason) уже структурирован.
- **Измерение**: Stage 5 (17/20, confidence-флор 0.70, learning-signal, cross-phase) и Stage 6
  (regression ≥17/20) — это метрики обучаемости/анти-регрессии, нужные для lifecycle-доказательств графа.
- **Teacher-provenance/легальность**: лицензионный гейт (Stage 0) и атрибуция (Stage 4) напрямую
  совпадают с **Legal Teacher / Provenance Gate**. ВАЖНО: метод нигде не использует выход
  закрытых моделей (Claude/OpenAI/Gemini) как teacher для обучения — источники это
  лицензированные документы (RFC/OWASP/KEP/SWE-bench и т.п.). Это совместимо с правилом
  «closed proprietary teacher output ⇒ DATASET_REBUILD_REQUIRED».
- **Anti-shortcut**: запрет «B описывает выход», требование rephrasing, запрет
  cross-reference в параллельных шагах, обязательный pass-кейс — всё это снижает риск, что граф
  выучит детерминированный shortcut/leakage вместо семантики.

Прямые точки переиспользования для плана ILearningDynamicGraph:
форма пары A→B; правило One-Document; 7 стадий внешних источников; стратификация + 100/20;
Stage-5 learning-signal как доказательство «модель учится»; лицензионный/attribution-реестр.

---

## 4. Классификация (уровень архитектуры vs детерминистический хелпер)

Здесь нет C#-кода — это **планы/методы данных**, а не реализация. Классифицирую сущности метода
по правилам `trainable-architecture-decision-SKILL.md`:

- **9 узлов конвейера (splitter / translator / planner / processor / applier / result-translator /
  arbiter-selector / arbiter-units / formatter)** — каждый описан как обучаемая семантическая
  единица «вход→выход». По разрешённым уровням это кандидаты на **ILogicalUnit**
  (`ILearningUnit`), а их последовательность — на **ILogicalFlow**/**ILogicalOrchestrator**.
  Весь граф с обратными рёбрами — **ILearningDynamicGraph** (целевой уровень задачи).
  Узлы-массивы (2,4,5,6) = fan-out того же ILogicalUnit, не отдельный уровень.
- **Arbiter selector (Node 7)** — НЕ «selector-хелпер» в смысле детерминированного выбора:
  метод требует обучать его парами A→B с обоснованием каждого выбранного арбитра. Это
  ILogicalUnit/решение внутри flow, а не финальная детерминированная авторитетность. ВНИМАНИЕ
  (по правилу скила): если в будущей реализации он станет regex/словарём/счётчиком позиций —
  это `FORBIDDEN_RUNTIME_SHORTCUT`, нужен trainable-unit + ablation.
- **Стратифицированный сплит, 4-стратовый split, license-gate, аннотации, batch-registry, волны** —
  это **внутренние детерминистические хелперы пайплайна данных** (data-prep / eval-setup), НЕ
  обучаемая архитектура. По скилу допустимы как «eval-only / helper / internal baseline» —
  они готовят и метят данные, но не являются рантайм-авторитетом семантического решения.
- **Симуляции (Family 7 / Stage 2 сценарии) и Stage 5/6 тесты** — это **eval/верификация**
  (KEEP_FOR_EVAL_ONLY), доказательства обучаемости и анти-регрессии, не часть рантайм-графа.
- **Lifecycle-замечание (важно для плана)**: метод детально описывает данные/обучение/оценку/
  correction-continue, но в этих файлах НЕ описаны явно checkpoint/manifest/export-import/locator/
  fresh-load для самих узлов — это пробел, который план ILearningDynamicGraph должен закрыть
  (см. §5), иначе будет «contract без lifecycle».

Ссылка на правило: `trainable-architecture-decision-SKILL.md` →
«The only allowed trainable architecture levels…», «Graph-Style / DynamicGraph Reconstruction Gate»,
«Legal Teacher / Provenance Gate», «FORBIDDEN_RUNTIME_SHORTCUT».

---

## 5. Пробелы / что доисследовать

1. **Lifecycle узлов как trainable-units**: в методе нет checkpoint/manifest/export/import/locator/
   fresh-load/continue-training для 9 узлов. План ILearningDynamicGraph должен это добавить (требование
   CLAUDE.md «Trainable Unit pattern» + «ILogicalUnit lifecycle checklist» скила).
2. **DPO-формат явно**: метод даёт A→B + rejected-примеры, но не сводит их в каноническую
   DPO-тройку (chosen/rejected/reward-margin). Нужно сопоставить с `dpo-triple-format-SKILL.md`.
3. **Маппинг «узел метода → конкретный уровень/класс»**: метод архитектурно-нейтрален; нет имён
   классов/контрактов ILearningUnit/Flow/Orchestrator/DynamicGraph и их Input/Output records.
4. **Реестр внешних источников детально**: `EXTERNAL-SOURCE-REGISTRY-v1.0.md` и
   `EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md` прочитаны только через цитаты внутри FINAL/FULL-PIPELINE —
   при планировании источников открыть их напрямую (лицензии per-unit).
5. **Связь с llm_mvp_core кодом**: эти файлы — внешний (xiigen) методический корпус; не проверено,
   как 9 узлов соотносятся с реальными `15.OrchestratorModel` / `42.ProjectWikiDomain` /
   bootstrap ILogicalUnit в репозитории. Нужен отдельный recon по коду репо.
6. **ANALYSIS-STATE.json**: это другой, незавершённый процесс (15/116 раундов) под другой выход
   (`ORCHESTRATOR-TRAINING-CLAUDE-CODE-PLAN-v1.md`); не путать с методом сетов. Открыты не все 116 файлов
   (защита от перегруза) — при необходимости добрать целевые раунды (12 dataset, 85-94 training).
7. **Объёмные файлы прочитаны частично** (FINAL 1315/1643, FLOW-01 264/604) — хвосты (полные таблицы
   «How to Write Pairs» по Node 8/9 и acceptance) дочитать при детальном планировании.
