# opus-pass-50-roadmap — Консолидированная дорожная карта закрытия XIIGen-расширений до FULL implementation

Это финальный проход (P50 из 50). Он НЕ добавляет новых дыр — он читает весь накопленный реестр
`opus-PROJECT-CLOSURE-PLAN.md` (P01–P49, ~245 дыр) и синтезирует из него один упорядоченный план
закрытия до FULL implementation (Full Extension Definition + dual-path same-scenario coverage для
Claude/Cursor/Codex поверх `llm_mvp_core`). Всё evidence-bound: каждая работа ссылается на дыру
реестра (Gxx-yy / Pnn-gap), а каждая дыра — на file:line в самом реестре. Никаких заявлений о
готовности; это план, а не completion-claim.

Тот же текст добавлен секцией `## CONSOLIDATED CLOSURE ROADMAP` в конец
`opus-PROJECT-CLOSURE-PLAN.md` (над маркером). Здесь — расширенная человекочитаемая версия.

---

## 1. Честный REAL baseline (одним абзацем)

Продуктовый DONE = **0/15** фаз; PROVEN dual-path = **0/28** сценариев (собственный счёт канонического
`product-readiness-scenario-board.json`: `full_joined_pass:0`, все 14 «accepted» — narrow/scoped
`formal_joined_pass=false`; P31). Три host-репо `C:\Projects\{claude,cursor,codex}-xiigen-smart-skills`
РЕАЛЬНО существуют (коррекция ранних «host MISSING»): **Cursor** — самый зрелый (реальный `activate()`,
3 команды, chat participant, webview-панель, собранный `dist/extension.js`); **Claude** = скелет из одного
SessionStart-hook; **Codex** = рукописный `.js`, не зарегистрированный в реальном marketplace Codex
(P06/P09/P13/P17). Ядро имеет настоящий in-process boundary `XiigenExtensionApiService`, но единственный
реальный транспорт — **per-call `dotnet run`-компиляция** всего `43.SmartSkillsDomain` через PowerShell-шим
с абсолютными путями `C:\Projects\llm_mvp_core` (непоставляемо; требует .NET SDK; P41-1/P08-1). Три
«обучаемые» машины конвейера (prompt-to-task/orchestration/skill-selection) на деле — **детерминированные
keyword/lexical-lookup'ы** (нарушение R4); `semantic_quality_claimed` зашит в `false`, агрегирующий гейт
семантики мёртв по построению (P10-1/P21/P22/P23). Настоящий градиентный DPO-движок (`11.DPO`) существует,
но **отсоединён** от расширений; единственный исполненный градиентный run **РЕГРЕССИРОВАЛ** (loss вырос к
ln2≈0.693, reward-margin сжался ~3×, held-out≈0.56; P25-1/P05-3). Экономия токенов, семантическое качество,
learnability, lifecycle, promotion, locator — **STUB/BUILT_NOT_PROVEN** (P24–P30). Dual-path = **0/28
joined**: Path A обходит расширение (in-process fallback), Path B = re-photo/stale/чужая поверхность, **0
видео** (P31–P36). Extension-дельта ядра **НЕ закоммичена** (331 файл/+18 574/−3 576, 545 untracked), ядро
не first-class member `LlmFromScratch.sln`, нет записанного зелёного `.trx` baseline (P39/P40/P41). Нет
license/provenance-гейта на промоутнутых common-корпусах; **sonnet-derived строки в common** (distillation
без teacher-гейта); **RCE-вектор** в host-адаптерах; **нет устанавливаемого/публикуемого артефакта**
(P45/P49). ИТОГ: пакет `extension development` доказывает только copy/status; реальный код — честный
untrained scaffold с host-скелетами; до FULL implementation — дистанция во ВСЕХ измерениях.

**Ключевое узкое место (из P48):** критический путь `M1→M2` — реальный обучаемый инференс. От него зависит
~60% дыр слоя «доказательство/сценарии». Всё, построенное перед M2, возвращается `not-proven` — что и
подтверждает 0/28 joined pass при 14 «scoped accepted». Поэтому порядок ниже — capability-first
(dependency-ordered), а не coverage-ordered (в отличие от `product-readiness-action-plan.md`, P48-1/P48-2).

---

## 2. Дорожная карта (упорядочена по зависимостям: M0 → M6)

Легенда ярусов: **P0** фундамент/блокирует всё · **P1** продуктовый хребет (зависит P0) · **P2**
доказательство/измерение/QA/dual-path (зависит P0+P1) · **P3** честность/гигиена/governance (параллелизуемо).

### M0 — Фундамент: сохранить и собрать (P0, блокирует всё)
Область: git/build/tests.
- Закоммитить extension-дельту ядра фазовыми коммитами (Rule 6) — **P40-1** (331 файл незакоммичен).
- Добавить `43.SmartSkillsDomain` и `44.DesignConversionDomain` как `Project()` в `LlmFromScratch.sln` — **P41-2**.
- Прогнать зелёный `dotnet build` под текущее (тяжело-модифицированное) дерево — **P35-6**.
- Записать полный `.trx` baseline (total/passed/failed + commit-hash), сейчас есть только узкие filtered — **P39-4**.
- Стабилизировать worktree/checkout ядра (9 расходящихся) до одного канонического — **P40-5**.
- **Gate M0:** зелёный build + сохранённый full `.trx` + все extension-исходники под git + `git merge-tree`
  чисто в `codex/chat-gpt-5.5` И `main` ПОСЛЕ коммита реальной дельты (сейчас merge-tree считался по дереву
  без 18,5 тыс. строк — бессодержателен; P40-3).

### M1 — De-risk обучающего движка (P0, критический узел, feasibility-спайк ПЕРЕД стройкой)
Область: DPO/движок.
- Bounded de-risk-спайк: реальный токенизатор (не hash-токены P05-3) + модель адекватного размера на ОДНОЙ
  extension-задаче; требовать монотонное loss↓ + held-out ≫0.5 — **P48-4**, чинит корень **P25-1** (регрессия), **P25-3** (held-out≈шум).
- **Gate M1:** ≥1 успешная кривая обучения на extension-задаче (loss↓, held-out значимо >0.5). Если
  from-scratch (Rule 10) не достигает качества — **эскалация Luba** (scope/model-решение; см. decision-ledger).
  ⚠️ БЕЗ прохождения M1 весь capability-трек (M2) — на неспособном движке; запрет строить downstream (P48-2).

### M2 — Реальный обучаемый инференс в 3 машинах (P0, УЗКОЕ МЕСТО, зависит M1)
Область: pipeline (prompt-to-task / orchestration / skills) + DPO.
- **prompt-to-task:** заменить hardcoded alias-словарь на выученный/эмбеддинговый экстрактор — **P21-1**;
  калибровать пороги (сейчас 0.01/0.01, гейт всегда открыт) — **P21-5**; реальный training-update/continue
  (сейчас append готового паттерна) — **P21-4**; подключить two-leg (personal/common) — **P21-6**.
- **orchestration (task→plan):** согласовать словарь стадий 1↔2 (12/15 паттернов недостижимы) — **P22-1**;
  реальная DPO reward-scalarization, использовать rejected-сторону — **P22-2**; обучаемый planner вместо
  replay/константного baseline — **P22-4**; continue-training update вместо append — **P22-6**.
- **skill-selection (task/plan→skills):** эмбеддинг вместо lexical bag-of-words argmax — **P23-3**; использовать
  rejected-сторону (DPO-маржа) — **P23-4**; ablation-harness — **P23-5**; лексический leakage-guard (Jaccard) — **P23-6**;
  построить настоящий source-disjoint runtime eval (сейчас self-recorded `NO_TRUE_SOURCE_DISJOINT_...`) — **P23-1**;
  снять circular held-out=1 — **P23-2**.
- **сквозное:** оживить мёртвый semantic-quality гейт (эмитить позитивную evidence) — **P10-1**; материализовать
  настоящие DPO-пары (сейчас `chosen_ref/rejected_ref`, orchestration 0/12 пар) — **P24-3/P24-1/P24-4**; исполнить
  training с loss/reward-margin/DELTA (сейчас 88 report'ов все UNKNOWN delta) — **P25-2/P25-4/P25-5**;
  портировать эталон trainable-integrity из `42.ProjectWikiDomain` (ablation/leakage/exact-prompt gate уже
  реализованы там!) — **P38-4**; снять governance-downgrade R4 «детерминизм ок если claimed=false» — **P38-1/P38-2**;
  §4 `knn_role` — **P38-3**; независимый (не parent-self) audit введённых SS-юнитов — **P38-5**.
- **Gate M2:** на КАЖДОЙ из 3 машин — измеренный held-out semantic uplift на ЗАГРУЖЕННОМ checkpoint + ablation
  (обнуление весов → деградация) + source-disjoint eval + независимое sub-agent-ревью; `claimed=false` снимается
  ТОЛЬКО после реального прироста; promotion гейтится по loss/reward-margin, не по факту прогона.

### M3 — Продуктовый хребет: host + транспорт + wiring (P1, зависит M2)
Область: транспорт, host-точки входа (Claude/Cursor/Codex), pipeline-wiring, lifecycle/locator/promotion, R6.

**Транспорт (общий):**
- Тёплый поставляемый сервис (stdio/HTTP) без per-call компиляции, только runtime, без absolute-path — **P41-1/P08-1/P13-1**.
- Единый versioned wire-contract: `schema_version` на request И response, `CorrelationId`+`UserPromptSha256` в
  ответе, ОДИН владелец (запретить фабрикацию контракта транспортом/host'ом) — **P44-2/P44-3/P37-3**.
- Кросс-платформенность (убрать `powershell.exe`-спавн) — **P13-1**.

**Claude host:**
- Реальная user-точка (slash-command/skill/MCP), не только SessionStart-hook — **P09-1**; settings-поверхность
  для `xiigenSmartSkills.debug` — **P09-3**; вывод в рабочую область (не в чужой репо), реальный промпт — **P09-4**;
  production-локатор после promotion (сейчас test-only qsg28) — **P09-5**; одна каноническая копия — **P09-6**;
  чинить `ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH` — **P11-6**; синхронизировать фиктивное имя
  `xiigen.smartSkills.select` — **P09-2**.

**Cursor host:**
- Кросс-платформенный транспорт — **P13-1**; убрать absolute-path дефолты — **P13-2**; синхронизировать
  settings-схему (3 читаемых незаявленных / 2 мёртвых) — **P13-3**; убрать test-only команды из прод-активации — **P13-4**;
  стабильный chat API + доказать инвокацию в Cursor — **P13-5**; воспроизводимая сборка (`tsconfig`+бандлер+`@types/vscode`) — **P13-7**;
  одна копия — **P13-8**; убрать canned auto-run панели — **P14-4**.

**Codex host:**
- Регистрация в реальной плагин-системе Codex (marketplace-snapshot / app-server / skill-runtime), доказать
  `codex plugin add`+инвокацию — **P17-1**; чинить `runCli` (видимый ответ, сейчас пустой stdout) — **P17-2**;
  свести три расходящихся манифеста к одному — **P17-3**; settings-поверхность — **P17-4**; закрепить один
  бинарь/версию (сейчас ≥5) — **P17-7/P20-7**; согласовать param-контракт ps1-entrypoint — **P17-6**.

**Pipeline-wiring:**
- Провести поставляемый путь через `ILogicalOrchestrator`/`15.OrchestratorModel` (A-to-B/fan-out/arbiter) ИЛИ
  явно задокументировать fixed-linear — **P10-6/P44-1/P22-3**; реализовать handler метода `feedback` (единственный
  online-сигнал) — **P11-2/P14-1**; каталог скиллов из repo skills-index (не пустой/само-ссылочный) — **P10-2/P14-3/P18-2**;
  стадия-3 берёт `StructuredTask`+`ScenarioPlan` (сейчас сырой промпт) — **P14-5**; все 3 метода достижимы из
  каждого хоста (сейчас 1/3) — **P10-3/P14-2/P18-3**; валидировать `Method` против `MethodNames` — **P18-1**.

**Lifecycle / locator / promotion:**
- Lifecycle-операции (export/import/backup/share/continue/rollback) в контракт расширения (сейчас вызыватели
  Phase08 = только Tests) — **P11-1/P19-1/P30-1**; грузить через `IRemoteUnitLocator.LocateAsync` (сейчас обойдён
  весь `25.ModelServer`) — **P26-1**; version-indirection (сейчас версия — литерал в коде) — **P26-2**;
  программный writer `index.json` (сейчас руками правленные sha) — **P26-3**; integrity-гейт на загрузке
  (пересчёт+сверка sha) — **P26-4**; promotion-гейт для 3 машин (эталон — `DesignConversionPromotionGateEvaluator`) — **P26-5**;
  реальный uploader ИЛИ честный local-only — **P26-6/P05-4**; исполнители Rollback/Quarantine/ContinueTraining — **P26-7**;
  реальный переносимый payload (сейчас 555-байт metadata-огрызок) — **P30-2**; per-user adaptive-root (не конфляция
  с common-target) — **P11-4/P17-8**; online write-back — **P11-3**; персистентный локатор, переживающий рестарт — **P05-5/P05-6**.

**Core-boundary (R6, thin adapters):**
- Общий тонкий adapter-SDK; редакция/honesty/презентация/locator-резолв во владении core (сейчас триплицированы и
  расходятся в 3 адаптерах) — **P37-1/P37-2/P37-3/P37-4/P37-5/P37-6**.
- **Gate M3:** реальный prompt→answer ЧЕРЕЗ собранную host-команду каждого хоста (не in-process fallback), тёплый
  транспорт, cross-layer conformance-тест wire-contract, все 3 машины грузятся без load-failure.

### M4 — Измерение позади реального инференса (P2, зависит M2+M3)
Область: token / semantic / perf-proof / lifecycle-proof / provenance-legal.
- **token:** эмитить трейс кросс-хостово — **P27-1**; арифметика редукции + фаза acceptance — **P27-2**; чинить
  инвариант `prompt_hash` (сейчас запрещает input-reduction) — **P27-3**; считать токены downstream-запроса к
  модели, не длину ответа — **P27-4**; соединить token+quality — **P27-5**; позитивный гейт редукции (не пропускать
  рост токенов) — **P27-6**; реальный токенизатор хоста — **P15-1**; независимая переменная baseline↔adaptive — **P15-2**; 14-assert guard после реального инференса — **P15-3**.
- **semantic quality:** реальный per-pair scorer вместо fixture 2/5 — **P28-1/P28-2**; одобренные redacted eval-входы
  (сейчас approvals 0/9) — **P28-3**; прогон ЗАГРУЖЕННОГО checkpoint на source-disjoint (не material-coverage
  тавтология) — **P28-4**; capture-harness реальных 4 rubric-слоёв — **P28-5**; independent inter-rater — **P28-6**.
- **perf/proof улучшения:** latency-метрика (сейчас 0) — **P47-1**; реальные CPU/RAM (сейчас wall-clock/GC-снимок
  под именами CpuMs/PeakRamMb) — **P47-2**; числовой продуктовый порог улучшения — **P47-3**; статистика (повторы/ДИ/дисперсия) — **P47-4**;
  **контрфактный no-XIIGen контроль** (единственный, отвечающий «улучшает ли вообще») — **P47-5**; инфраструктура
  non-regression (golden baseline) — **P47-6**; денежная экономия ($) — **P47-7**.
- **lifecycle-proof:** реальный continue-training uplift (сейчас no_op before==after / счётчик id) — **P30-3/P25-6/P05-8**;
  реальные share/anonymize (сейчас манифест без переноса) — **P30-5/P30-6**; `SourceHashesVerified` честно — **P30-4**;
  personal-нога всех 4 сфер (сейчас 3/4 `not-created-yet`) — **P30-7**; upgrade/rollback над боевым ассетом, не sandbox — **P30-8/P26-8**.
- **provenance/legal:** license+provenance-манифест на каждый common-корпус (эталон — DesignConversion) — **P45-1**;
  teacher/distillation-гейт (`family→canonical_license`+terms_snapshot+provider-disambig) — **P45-2**; применить гейт
  к промоутнутым данным (сейчас декоративен) — **P45-3**; leg-assignment (local-file→adaptive) — **P45-4**; закрыть
  красный DesignConversion license — **P45-5**.
- **Gate M4:** измеренное улучшение над no-XIIGen контролем с объявленным числовым порогом + статистической значимостью
  + non-regression baseline; каждый common-корпус имеет проходной source-license манифест.

### M5 — Dual-path same-scenario 28×3 (P2, зависит M2+M3+M4)
Область: dual-path / harness / tests-e2e.
- **Path A:** через команду built-расширения, не in-process fallback / fabricated catalogue / direct-module — **P32-2/P33-2/P34-2**.
- **Единая Path A харнесс:** один runner (`.runsettings`/скрипт), покрыть 16/28 сценариев без команды — **P35-1/P35-2**;
  end-to-end product-workflow команда — **P35-3**; изолированная mutation-boundary харнесс (learnability/dpo/lifecycle/promotion/locator) — **P35-4**; кросс-платформенность/CI — **P35-5**.
- **Path B:** видимый desktop с prompt-hash, видео (сейчас 0) — **P36-1/P36-2**; маркер на поверхности РАСШИРЕНИЯ
  (не редактор Cursor / нативный чат) — **P36-3/P32-4/P33-5**; детерминированное наведение окна (не свой чат ассессора) — **P36-4**;
  подтверждённый marker-smoke Claude/Codex — **P36-5**; свежий typed→submit→НОВЫЙ ответ (не re-photo/stale) — **P33-3/P34-3/P32-5**.
- **Oracle+e2e:** correctness-oracle (вход→ожидаемый task/plan/skills) поверх реального инференса — **P39-1/P12/P16/P20**;
  e2e/host-слой в .NET-наборе (сейчас отсутствует) — **P39-5**; исполняющий (не null-result) флагман token-04 — **P39-2**.
- **A↔B:** обязательное поле сравнения по одному prompt-hash, одна сессия (не сшивка из разных дней) — **P31-5/P44-3/P33-4**.
- **Инвентарь/board:** нормализованный вектор покрытия, синхронная дата, чинить mojibake (ряды 6/11/15), matrix-asymmetry
  (Codex 10 vs 9), summary count-carrier — **P31-2/P31-3/P31-6/P31-7**; переклассифицировать blocker-as-pass (row21),
  ceremony-as-progress (row23), label-inflation (row19) — **P34-5/P34-6/P34-8**; Codex-папки 24/25/26 — **P34-7**.
- **Gate M5:** ≥1 РЕАЛЬНЫЙ full joined pass на хост (Path A через команду + Path B видимый тот же сценарий +
  A↔B сравнение по prompt-hash), затем рост покрытия до accepted>0 → 28×3.

### M6 — Приёмка / интеграция / публикация (P3 + external, зависит M5)
Область: reviews / matrix / git-integration / security / distribution / risk.
- ≥10 независимых sub-agent execution-review (сейчас 0; пакетный 50-pass = 10 self-certified, header 0/50) — **P02-8/P42-1**.
- Completion-coverage матрица по Full Extension Definition (текущая покрывает только упаковку, «8 covered») — **P42-2**.
- Git-интеграция: запушить, фазовые commit-сообщения (не «latest»), втянуть 32 base-коммита, FF в main, per-phase
  mergeability-строка — **P40-2/P40-3/P40-4**; схема версионирования core+host (3 отдельных репо) — **P40-6**.
- Security: threat-model, Workspace-Trust gating, убрать `-ExecutionPolicy Bypass`+компиляцию из user-пути, подпись — **P49-3**.
- Distribution: лицензия+канал, устанавливаемый/публикуемый артефакт (`.vsix`/плагин), доказать установку на чистой машине — **P49-4**.
- Host-API compat-матрица + мониторинг дрейфа (VS Code/Cursor/Claude/Codex) — **P49-2**.
- Risk register (вероятность×воздействие×митигация×владелец×триггер) — **P49-1**.
- **Gate M6:** устанавливаемый артефакт доказан на чистой машине (Path B) + dual-path coverage принят + все ревью PASS
  + git-интеграция и мержабельность зафиксированы.

### Кросс-секущее (P3, параллелизуемо, НЕ на capability-критпути)
- UX: человекочитаемые ошибки (`showErrorMessage`, next-step), перевод машинных статусов, содержимое webview,
  презентация untrained/degraded, UX-automation на живых поверхностях, help-текст настроек, разведение
  Luba-facing vs end-user статуса — **P46-1..P46-7**.
- Evidence-hygiene: удалить/санитизировать секрет (RUNTIME-04G), pre-commit секрет-скан — **P20-4**.
- Governance: убрать самозаверенный 50-pass ledger / переименовать requirements-matrix — **P42-1/P42-2**.
- Порядок как живой артефакт: проставить каждой дыре tier + blocks/blocked-by, per-milestone entry/exit — **P48-1/P48-2**.

### Пропущенные измерения (требуют решения scope, P43)
- Мультипроектность / cross-project transfer на `xiigen-platform-dot-net-angular` (Q6-03/Q7) — **P43-1**.
- VS Code / Visual Studio project-wiki plugin как 4-я host-семья + orchestrator prompt-suite scenario-классы — **P43-2**.
- Advanced wiki question classes (14 подклассов, не 7 бакетов) — **P43-3**.
- Замкнутый learning-loop Q6-04 (наблюдение вывода AI → gap → DPO → continue → held-out > pre-adaptive) — **P43-4**.

---

## 3. Разбивка: needs реализуй / needs Luba decision / external

### 3.1 Needs РЕАЛИЗУЙ (инженерная работа, внутри нашего контроля) — основная масса
Все M0..M6 работы выше, кроме отмеченных ниже как decision/external. Это ~90% реестра: commit/build/sln/trx
(M0), обучаемый инференс 3 машин + DPO с числами (M1/M2), host-точки/wiring/lifecycle/R6 (M3), измерение
token/quality/perf/legal-манифесты (M4), dual-path harness/oracle/e2e/board (M5), reviews/matrix/UX/hygiene (M6).

### 3.2 Needs LUBA DECISION (продуктово-правово-безопасностные развилки; ветвят критпуть)
Свести в ОДИН decision-gate ledger (P48-3 + P49-6). Каждое решение меняет размер/форму пути:
1. **Трансформерный DPO vs словарь** для инференса расширений — **P05-1/P05-2**.
2. **remote/marketplace vs local-only** lifecycle модели — **P26-6/P26-8/P05-4**.
3. **Маршрут через `ILogicalOrchestrator`/15 vs fixed-linear** — **P44-1/P10-6**.
4. **Мультипроектность** (`xiigen-platform`) в scope? — **P43-1**.
5. **VS Code/Visual Studio wiki-plugin** (4-я host-семья) в scope? — **P43-2**.
6. **Репо-топология** monorepo/submodule vs 3 отдельных репо — **P40-6**.
7. **Teacher/distillation:** sonnet-derived в common vs карантин+пересборка из permissive — **P45-2**.
8. **Лицензия + канал дистрибуции** (Marketplace/Open VSX/Anthropic/OpenAI) — **P49-4**.
9. **Security/Workspace-Trust модель** + кто задаёт пути ядра — **P49-3**.
10. **Приём внешне-правового ToS-риска** distillation — **P49-5**.
11. **Минимально-жизнеспособный набор хостов** (подмножество, напр. только Cursor) vs все 3 при внешнем блокере Codex — **P17/P46**.
12. **Кто платит/владеет хостингом** при remote-lifecycle — **P26-8**.
13. **Эскалация feasibility from-scratch движка** если M1-спайк провалится (Rule 10 vs scope/model) — **P48-4**.

### 3.3 EXTERNAL (вне нашего контроля; требует внешнего действия/ответа)
- **Регистрация плагина Codex** в marketplace / vendor-supported local-invocation — сейчас реальный внешний блокер
  (`true_codex_host_runtime_path_found:false`); один из 3 обязательных хостов может быть неосуществим без OpenAI-курации — **P17-1/P49-2**.
- **Стабильность host-API вендоров** (VS Code/Cursor/Claude plugin+hooks/Codex platform) — нужен мониторинг changelog/deprecation — **P49-2**.
- **Аккаунты издателя + ревью маркетплейса** (VS Code Marketplace/Open VSX; Anthropic/OpenAI каналы) — **P49-4**.
- **ToS вендора-учителя** (Anthropic/OpenAI) для distillation — прочитать+записать `terms_snapshot`, юр. sign-off;
  многие ToS запрещают обучение конкурентов на выводе → может обнулить distillation-value-prop — **P49-5**.
- **Зависимость от .NET SDK** у текущего транспорта — устранимо реинженирингом на предсобранный runtime-сервис (P41-1).

---

## 4. Критический путь и главный риск

`M0(commit+build+trx) → M1(de-risk движка) → M2(реальный инференс 3 машин) → M3(host+транспорт+wiring) →
M4(измерение) → M5(dual-path 28×3) → M6(приёмка+публикация)`.

**Узкое место M1→M2:** от реального обучаемого инференса зависит ~60% дыр слоя P2. **Главный риск (R-01):**
единственный исполненный градиентный DPO РЕГРЕССИРОВАЛ (P25-1), движок игрушечный (P05-3) — feasibility узла
критпути НЕ доказана. Поэтому M1 — обязательный gate-спайк ПЕРЕД вложением в capability-трек; при недостижимости
from-scratch на нужном качестве — эскалация Luba ДО стройки downstream (иначе повтор паттерна прошлой сессии:
proof-packets поверх стабов → 0/28 joined при 14 «scoped accepted»).

**Правило порядка (P48-2):** заморозить всю capability-зависимую proof/scenario-работу (token/quality/learnability/
lifecycle/dual-path) до прохождения M2; до M2 разрешить только host-route scaffolding + не-capability гигиену/доки/тесты.

---

## 5. Границы этого прохода (no overclaim)

- Это ПЛАН, а не completion — ни одна дыра не закрыта данным проходом; 0/15 фаз и 0/28 dual-path остаются.
- Оценка велась bounded-инструментами (Grep/Glob/targeted Read) по живому коду `C:\Projects\llm_mvp_core` и
  host-репо + чтение накопленного реестра P01–P49; 8.9МБ `Extension development session.md` целиком не читан.
- Каждая работа привязана к дыре реестра (Pnn-gap), каждая дыра — к file:line в
  `opus-PROJECT-CLOSURE-PLAN.md`. Ярусы/милстоуны/зависимости взяты из P48; риск/решения/external — из P49.
- P50 не проводил новых grep'ов кода (консолидация чтения реестра); поэтому НЕ заявляю новых code-level фактов
  и НЕ ввожу новых дыр — только синтез уже доказанного.
