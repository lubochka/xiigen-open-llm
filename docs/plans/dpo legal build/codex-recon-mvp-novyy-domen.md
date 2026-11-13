# Codex Recon — MVP: как заводится обучаемый домен с нуля

Источник: рекон для плана ILearningDynamicGraph (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph).
Цель рекона: область `новый домейн`. Только чтение + запись своего файла.

---

## Что это и где (точные ключевые пути)

**Цель рекона (одна папка, один файл):**
- `C:/Users/lubao/Documents/xiigen/llm mvp/новый домейн/рабочая сессия.txt`
  — единственный файл в области (867 КБ). Это **транскрипт рабочей сессии
  архитектора**, где проектировалась топология `ILearningMachine<TDomain>` и
  правила запуска новых обучаемых доменов. Не код, а планировочный диалог с
  цитатами планов/отчётов.

**Скилы классификации (прочитаны для правил уровней):**
- `C:/Projects/llm_mvp_core/docs/skills/trainable-architecture-decision-SKILL.md`
  (есть; прочитан полностью — даёт уровни и KNN-ноту).
- `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md` (есть; частично, триггеры
  классификации видны).

**Артефакты репозитория, на которые ссылается транскрипт** (это evidence-пути
из текста, не проверены в самом репо в рамках рекона — отметить как «доисследовать»):
- `00.Core/LearningMachineContracts.cs` — контракты `ILearningMachine<TDomain>`,
  `ILearningMachinePair<TPersonal,TGlobal>`, records `LearningBatch`,
  `LearningEvaluationResult`, `LearningMachineExport` (строки ~108–138, 415).
- `42.ProjectWikiDomain/` — 6 Bootstrap `ILogicalUnit` обёрток над статикой
  (TD1) + flow-регистрация в `15.OrchestratorModel` (строки ~81–84, 416, 428).
- `27.TranslationDomain` (+ `27.TranslationDomain.Tools`) — пример нового домена;
  на момент сессии это **scaffold, не обученная машина** (строки ~2785, 9427).
- `25.ModelServer` + `ILearningMachineRepository` — export/import/upgrade/
  rollback нового домена (строки ~214–215, 3140).
- Планы: `docs/plans/PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md` (scaffolding
  template любого `ILearningMachine<TDomain>`), `PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md`,
  `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md` (строки ~517–518, 9392).

---

## Как устроено / работает (кратко): запуск домена с нуля

Транскрипт даёт связную «рецептуру» заведения нового обучаемого домена. Главное
правило (строка 9386):

> «Ключевой принцип: **новый домейн не обучается с нуля** — он копирует веса
> базовой модели и дообучается на доменном шарде.»

То есть «с нуля» по продукту = «копия базовой модели + fine-tune на доменном
DPO-шарде», а НЕ обучение случайной модели на голом месте. Схема (строки 9367–9386):
источники whitelist → частотные списки слов → отбор предложений → DPO-пары для
3 юнитов → **BASE TRAINED MODEL (general)** → `copy + fine-tune` → доменные ветки
(Legal/Literary/Medical/Technical и т.п.).

**Пошаговый запуск НОВОГО домена** (обобщено из affiliate/entrepreneurship-кейса,
строки 991–1014, и из «правильного плана машины», строки 2787–2796):
1. Регистрация источника: `source_id`, `source_family`, `license_state`,
   `supported_contract_signatures: [ILearningMachine<NewDomain>, ...]` (стр. 992–995).
2. Извлечение материала (PDF/audio/код → текст; streaming-манифесты).
3. Генерация DPO-пар существующими `*DpoPairBuilder`: A = реальный вопрос,
   Chosen = правильный ответ из материалов, Rejected = «правдоподобно плохой»
   с одной проверяемой ошибкой `[WRONG: ...]` (стр. 1002–1007, 2820).
4. Создать `ILearningMachine<NewDomain>` (или расширить leg существующей машины,
   напр. `AffiliateGlobal`); спроектировать топологию: внутренние flows + все
   `ILogicalUnit` с точным input/output (стр. 1011, 2789–2793).
5. `TrainBatchAsync` через существующий `TrainingRunner` (стр. 1012, 554).
6. Проверка не отдельного unit, а **всей машины на новых человеческих задачах**
   (held-out), затем promotion по Gate B: `AccuracyAfter ≥ baseline + 0.03` +
   цитаты из источника + тон (стр. 1029–1034, 2795).
7. Только после этого export/import через `25.ModelServer` (стр. 2796, 3140).

**Жизненный цикл «с нуля → обучен» честно размечен:** scaffold выдаёт
`[DIR:BOOTSTRAP] текст` / honest-untrained, не претендуя на обученность
(строки 2785, 3134); «модель не учит язык с нуля, она учит предпочитать
правильный ответ над правдоподобно-неправильным» (стр. 2484). Это совпадает с
проектным паттерном `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` vs `PASS_AS_TRAINED_FEATURE`.

**Механика обучаемого слоя** (строки 3125–3142): инференс = извлечь структурную
сигнатуру (POS-биграммы/паттерн SOV/SVO/IMPERATIVE) → KNN K=3 ближайших обученных
примера → взвешенный ответ; LoRA (rank=4) держит Bootstrap замороженным и
добавляет per-direction матрицу `delta_W = A×B`, загружаемую из ModelServer.

---

## Релевантность к ILearningDynamicGraph

Высокая — как **методология и evidence-источник**, не как граф-код.
- **DPO/обучение:** прямой рецепт авто-построения DPO-пар (A/Chosen/Rejected,
  taxonomy «правдоподобно плохого», запрет label-shape/prompt-token/synthetic-name
  утечек) — это ровно то, что ILearningDynamicGraph должен авто-строить для
  ILearningUnit/Flow. Источники DPO и их лицензии прописаны (Tatoeba CC-BY, UN,
  Gutenberg PD и т.п.) → совместимо с Legal-Teacher/Provenance Gate скила.
- **Граф/оркестрация:** домен = `ILearningMachine<TDomain>` поверх внутренних
  `ILogicalFlow` + множества `ILogicalUnit` (3-юнитная схема SemanticTokenizer/
  Executor/QualityVerifier); affiliate = `ILearningMachinePair` (Personal+Global,
  sync без PII). Это прямые узлы/рёбра для реконструкции графа.
- **Измерение:** held-out gate (унаследованный contamination-guard: held-out ID
  не пересекается с train ID), promotion `baseline+0.03`, back-translation ≥92%.
- **teacher-provenance:** провенанс источника фиксируется при регистрации
  (`source_id/family/license_state`); приватные данные пользователя ≠ common-train.
- **Запуск нового узла/домена графа с нуля:** «copy base + fine-tune shard» — это
  и есть continue-training-from-checkpoint, а не retrain-from-zero.

---

## Классификация (по правилам trainable-architecture-decision-SKILL.md)

Файл — **планировочный транскрипт**, не исполняемый код, поэтому классифицирую
архитектурные сущности, которые он описывает:

- `ILearningMachine<TDomain>` (новый домен целиком) → **уровень ILearningMachine**
  (Level 5). Это allowed-уровень; именно сюда вешаются Train*/Evaluate*/Export*.
- Affiliate (Personal+Global, анонимный sync) → **ILearningMachinePair** — allowed.
- Внутренние flows домена (напр. TranslationRoutingFlow) → **ILogicalFlow** — allowed.
- SemanticTokenizer/Executor/QualityVerifier; 6 Bootstrap-обёрток (TD1) →
  **ILogicalUnit** — allowed (Bootstrap = честный untrained default, A-LM-03).
- **KNN by sentence structure (K=3 по структурной сигнатуре)** → НЕ отдельная
  архитектура, а **компонент модели внутри ILogicalUnit** (`OK_MODEL_COMPONENT`).
  По KNN-ноте скила (стр. 112–117): KNN допустим как memory/retriever внутри
  allowed-уровня; нарушение было бы только если KNN — единственная «обучаемая
  архитектура» или неразмеченный финальный авторитет.
- **LoRA-адаптер per-direction** → обучаемое состояние (delta-веса) внутри
  ILogicalUnit/Machine; грузится через `ILearningMachineRepository` — это
  lifecycle-evidence (checkpoint/export/import), а не отдельный уровень.
- **BootstrapTranslationAdapter (`[DIR:BOOTSTRAP] текст`), exact-match/lookup,
  «if word==…» переводы** → **внутренний детерминистический хелпер**, допустим
  ТОЛЬКО как honest-untrained scaffold / fallback. Как runtime-авторитет для
  семантического перевода это `FORBIDDEN_RUNTIME_SHORTCUT` (явно запрещено в
  §14 плана, стр. 9402, и в обсуждении «детерминированная игрушка», стр. 2828).
- Sourcing/indexing pipeline (`TranslationCorpusSourcingTool`, lemmatizer,
  Tatoeba index, contamination guard, DPO builder) → **внутренние хелперы
  данных** (corpus labeling / fixture / eval-setup), не runtime-авторитет —
  допустимы, т.к. переводные соответствия приходят из accepted/rejected пар
  (стр. 9355).

Итог: описанная топология ложится на allowed-уровни корректно; единственный
риск — детерминистический scaffold/lookup, который НЕ должен стать финальным
семантическим авторитетом (что в транскрипте уже осознано и заблокировано).

---

## Пробелы / что доисследовать

1. **Только один файл** в области `новый домейн` — это транскрипт, а не
   спецификация. Нет отдельного «как завести новый домен» гайда как самостоятельного
   документа; рецепт рассеян по диалогу (строки 991–1014, 2787–2796, 9367–9386).
2. **Несоответствие отчётов и файлов:** сам транскрипт фиксирует завышенные
   claim'ы (68 vs 23 промпта, «решено» vs реально нет раздела — стр. 862–874).
   Любые цифры/«готово» отсюда брать как evidence-only, перепроверять в репо.
3. **Контракты в коде не верифицированы рекона:** существование и точные
   сигнатуры `ILearningMachine<TDomain>` / `ILearningMachineRepository` /
   `TrainingRunner` нужно подтвердить чтением `00.Core` и `25.ModelServer`
   (вне области данной под-задачи).
4. **ILearningDynamicGraph как таковой в файле НЕ описан** — есть
   ILearningMachine/Flow/Unit/Pair, но не DynamicGraph-узлы/рёбра/fan-in-out/
   many-to-many/async. Для плана графа эти контракты надо искать отдельно.
5. **LoRA-механика** (rank=4, delta_W=A×B, заморозка Bootstrap) описана только на
   уровне идеи в диалоге; реализация/тесты/чекпойнты не подтверждены — доисследовать
   `25.ModelServer` и runner-отчёты.
6. **Provenance/лицензии источников** перечислены текстом (Tatoeba/UN/Gutenberg/
   приватный אחותי.zip); фактическая регистрация source-manifest с license-файлами
   не показана — проверить перед использованием как common-training.
