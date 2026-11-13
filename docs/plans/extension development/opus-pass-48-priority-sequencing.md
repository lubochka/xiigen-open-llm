# Pass P48 — Приоритизация и порядок закрытия дыр (priority / sequencing)

Роль: Opus 4.8 REAL-STATE ASSESSOR. Фокус P48 (из 50): не искать новую поверхность,
а прочитать весь накопленный реестр P01–P47 (~230 дыр) и наложить на него слой
ПРИОРИТЕТА и ПОСЛЕДОВАТЕЛЬНОСТИ — P0/P1/P2/P3 + зависимости (blocks / blocked-by) +
критический путь. Плюс — выявить критично-путевые дыры, которые видны ТОЛЬКО через
призму последовательности, и занести их как НОВЫЕ.

Честная рамка сохраняется: COPIED_DOC_ONLY != BUILT_NOT_PROVEN != PROVEN. Ни один
проход не объявляет проект завершённым. Ниже — план порядка, а не заявление о готовности.

---

## 0. Что уже есть и чего нет (проверено в этом проходе)

- Реестр `opus-PROJECT-CLOSURE-PLAN.md` P01–P47 = ИНВЕНТАРЬ дыр. Ни одна строка не
  несёт поля приоритета (P0/P1/P2), ни `blocks`/`blocked_by`, ни критического пути.
  Grep `priority|P0|P1|blocked_by|dependency order|milestone` по мастеру = только
  случайные вхождения в прозе, НЕ структурированная секция порядка.
- В пакете СУЩЕСТВУЕТ один план действий:
  `supporting-files/evidence-extension-full-readiness-dual-path/product-readiness-action-plan.md`
  (8 фаз + очередь §11). НО он упорядочен ПО СТРОКАМ scenario board
  (row 11 → 12/13 → 14-18 → Claude 1-9 → Codex 19-28 → workflow 1/10/19 → final),
  а НЕ по зависимостям возможностей. Он прямо ставит доказательную работу
  (Фаза 2: «превратить token/semantic guard evidence в concrete proof малыми
  packets»; Фаза 3: learnability/DPO/lifecycle packets) ПЕРЕД перестройкой
  обучаемого инференса, от которого эти доказательства зависят. Счётчики этого
  плана: 28 строк, full joined pass 0, product-ready 0, scoped accepted 14.
- Вывод: план ЕСТЬ, но он coverage-ordered и инвертирует порядок; capability-first
  dependency-ordered roadmap отсутствует. Это и закрывает данный проход (секция ниже),
  и рождает новые дыры G48-1..G48-4.

---

## 1. Определения ярусов (tiers)

- **P0 — ФУНДАМЕНТ / блокирует всё.** Пока не закрыто, ни одно downstream-доказательство
  (token/quality/learnability/lifecycle/dual-path) не имеет смысла: оно либо
  не сохранится, либо структурно вернётся `not-proven`. Сюда входит и «сохранить
  продукт» (commit+build), и «сделать возможность реальной» (обучаемый инференс).
- **P1 — ПРОДУКТОВЫЙ ХРЕБЕТ (host + транспорт + wiring).** Зависит от P0. Реальные
  host-точки входа, поставляемый транспорт, единый versioned-контракт, сборка
  конвейера через оркестратор, реальный lifecycle/locator wiring. Нужно для
  Full Extension Definition; но наполнять его смыслом можно только после P0-B.
- **P2 — ДОКАЗАТЕЛЬСТВО / ИЗМЕРЕНИЕ / QA / dual-path покрытие.** Зависит от P0+P1.
  Экономия токенов, семантическое качество, DPO-обучение с числами, lifecycle-операции,
  dual-path same-scenario (28×хоста), Path A харнесс + Path B desktop, correctness-oracle,
  провенанс/легал, UX, dashboard route-tied.
- **P3 — ЧЕСТНОСТЬ / ГИГИЕНА / GOVERNANCE.** Во многом параллелизуемо, НЕ на
  capability-критическом пути, но обязательно для правдивого закрытия: устранение
  оверклеймов/противоречий статус-файлов, mojibake, утечки секрета, git-провенанс/
  интеграция веток/worktree-разрастание/repo-split, false-completion аудита.

Правило ярусов: capability-зависимую работу P2 НЕЛЬЗЯ засчитывать, пока не пройден
её P0/P1-предшественник. P3 можно вести параллельно (F0-правило: Luba-решения
блокируют только decision-dependent продуктовое поведение, не независимые
scaffolding/docs/tests/hygiene).

---

## 2. Критический путь (самая длинная цепь жёстких зависимостей)

```
M0  Сохранить продукт + build-graph:
    commit extension-дельты ядра (P40-1) + добавить 43/44 в .sln (P41-2)
    + зелёный build под текущим деревом (P35-6) + записанный full .trx baseline (P39-4)
        │  (без этого ничего не версионируемо/не воспроизводимо/не мержабельно)
        ▼
M1  ФИЗАБИЛИТИ-СПАЙК обучающего движка (de-risk P0-B):
    реальный токенизатор + модель адекватного размера на ОДНОЙ extension-задаче;
    требовать монотонное loss↓ + held-out ≫0.5 (сейчас: единственный реальный
    градиентный DPO РЕГРЕССИРОВАЛ — P25-1; toy hash-tokenizer/1-слой — P05-3)
        │  (если from-scratch недостижимо при нужном качестве → эскалация Luba ДО стройки)
        ▼
M2  РЕАЛЬНЫЙ обучаемый инференс в 3 машинах:
    заменить keyword/lexical authority (P21-1/P22-1/P23-3) обучаемым;
    оживить dead semantic-quality gate (P10-1); DPO с реальным training-run
    + held-out + source-disjoint + ablation (P24/P25/P21-2/P23-1/P23-5);
    снять governance-downgrade R4 (P38-1/P38-2)
        │
        ▼
M3  ПРОДУКТОВЫЙ ХРЕБЕТ:
    поставляемый тёплый транспорт (P41-1/P08-1/P13-1) + versioned wire-contract
    единого владельца (P44-2/P37-1..6/P44-3) + реальные host-точки входа
    (Claude P09-1, Codex P17-1, Cursor productization) + конвейер через
    ILogicalOrchestrator/15 (P10-6/P44-1) + skill-каталог/feedback/stage-3 wiring
    (P10-2/P14/P18/P11-2/P14-5) + lifecycle/locator/promotion/integrity wiring
    (P26-1..8/P05/P30-1)
        │
        ▼
M4  ИЗМЕРЕНИЕ ПОЗАДИ РЕАЛЬНОГО ИНФЕРЕНСА:
    token-savings система (P27/P15/P47-1/2/6/7) + семантическое качество
    (P28/held-out) + lifecycle-операции round-trip (P30/P11/P19) + перф/цель/
    статистика/контроль/не-регрессия (P47-3/4/5) + провенанс/легал (P45)
        │
        ▼
M5  DUAL-PATH SAME-SCENARIO ПОКРЫТИЕ (28 × 3 хоста):
    Path A через собранную команду расширения, НЕ in-process fallback (P32-2/P33-2/P34-2);
    Path B видимый desktop с prompt-hash-привязкой (P36/P32/P33/P34);
    correctness-oracle + e2e-слой (P39/P12/P16/P20); A↔B comparison-поле (P31-5/P44-3)
        │
        ▼
M6  ФИНАЛ: ≥10 независимых execution-review (P02-8) + completion coverage matrix
    по Full Extension Definition (P42-2) + git-интеграция/мержабельность (P40)
```

Замечание о длине пути: узкое место — **M1→M2** (реальный обучаемый инференс). От него
зависит ~60% дыр P2 (весь token/quality/learnability/lifecycle/dual-path-capability слой).
Всё, что построено ПЕРЕД M2 в этом слое, вернётся `not-proven` — что и подтверждает
0/28 joined pass после 14 «scoped accepted» проходов.

---

## 3. Раскладка существующих дыр реестра по ярусам (индекс)

### P0 — фундамент
- **Сохранность/build:** P40-1 (uncommitted 18 574 строки ядра), P41-2 (43/44 не в .sln),
  P35-6 (собираемость под текущим деревом at-risk), P39-4 (нет записанного full-green .trx).
- **Возможность (корень R4):** P02-2 (PromptToTask stub not-ready), P02-3 (OrchestrationReasoning
  stub), P21-1 (alias-словарь как authority), P22-1 (stage1→stage2 словари не пересекаются,
  ready-путь недостижим), P23-3 (lexical bag-of-words authority), P10-1 (dead semantic-quality
  gate — true недостижим), P05-1 (обучение = словари/счётчики), P05-3 (toy hash-tokenizer),
  P25-1 (реальный DPO-run регрессировал), P38-1 (аудит false-completion goal_reached=true),
  P38-2 (R4 переопределён «можно детерминизм если claimed=false»).

### P1 — продуктовый хребет
- **Транспорт:** P41-1 (per-call compile), P08-1 (hardcoded absolute-path shim), P13-1
  (Windows-only powershell), P05-4 (UploadAsync no-op), P05-5 (locator in-memory), P26-8 (IModelServer мёртв).
- **Контракт/адаптеры:** P44-2 (нет versioned wire-contract, 3-й владелец на транспорте),
  P37-1..6 (redaction/honesty/response-synth/presentation/locator в host + нет shared-SDK),
  P44-3 (нет correlation/prompt-hash в ответе).
- **Host-точки входа:** P09-1/2/3 (Claude surface/фиктивное имя/settings), P17-1/2/3/4
  (Codex marketplace-регистрация/видимый ответ/манифест-triplication/settings),
  P13-4/P13-5 (Cursor test-команды в проде/chat unstable-API), P46-* (UX-слой).
- **Wiring конвейера:** P10-6/P44-1 (через 15/оркестратор), P10-2/P14-2/P14-3/P18-2/P18-3
  (skill-каталог), P11-2/P14-1 (feedback-метод), P14-5 (stage-3 берёт task+plan), P22-1 (словари стадий).
- **Lifecycle/checkpoint wiring:** P26-1..7 (bypass 25.ModelServer/hardcode версии/index.json/
  integrity/promotion-gate/decision-без-исполнителя), P30-1 (нет lifecycle-операции на границе).

### P2 — доказательство/покрытие
- **Token:** P27-1..6, P15-1..4, P47-1 (latency), P47-2 (ресурс-прокси), P47-6 (контроль без XIIGen), P47-7 ($).
- **Качество:** P28-1..6, P21-2/P21-3 (ablation/held-out), P23-1/P23-5/P23-6 (source-disjoint/ablation/lexical-leakage), P38-4 (портировать эталонные guards).
- **DPO/обучение (числа):** P25-2..6, P24-1..5 (accepted rows/инфляция/материализация пар/online≈0/divergent roots).
- **Lifecycle proof:** P30-2..8, P11-1/P11-3, P19-1..4.
- **Dual-path покрытие:** P31-1..7, P32-1..5, P33-1..6, P34-1..8, P35-1..5 (Path A харнесс), P36-1..5 (Path B desktop), P44-* (e2e), P12/P16/P20 (QA-oracle), P39-1/P39-5 (correctness-oracle/e2e-слой), P29-1..7 (dashboard route-tied), P47-3/4/5 (цель/статистика/не-регрессия).
- **Провенанс/легал:** P45-1..5 (может выполняться раньше как gate данных M2, но проверяется на уровне доказательства).

### P3 — честность/гигиена/governance (параллельно)
- P24-2 (инфляция 900), P42-1 (ledger 0↔10 противоречие + self-cert), P42-2 (coverage-matrix подменяет набор требований), P31-2/3/6/7 (board defects/mojibake/matrix-asymmetry), P20-4 (утечка секрета), P02-10 (mojibake источника), P07-* (git-провенанс сессии/пакета), P08-4 (board мис-локализует), P40-2/3/4/5/6 (коммиты «latest»/мержабельность-строка/интеграция ветки/worktree-разрастание/repo-split).

---

## 4. НОВЫЕ дыры, видимые только через призму последовательности (G48-1..G48-4)

Занесены в мастер. Ни одна не дублирует существующие: прежние проходы фиксировали
ОТДЕЛЬНЫЕ дефекты; здесь — дефекты СЕКВЕНИРОВАНИЯ/критического пути поверх них.

### G48-1 — Нет capability-dependency / critical-path-ordered closure-roadmap
Заявленный дельайвербл — «PLAN до FULL implementation». Реестр P01–P47 = инвентарь дыр
без порядка; единственный план (`product-readiness-action-plan.md`) упорядочен по строкам
scenario board, а не по зависимостям возможностей, и ставит доказательные packets
(Фаза 2/3) перед перестройкой инференса, от которого они зависят. Строитель не может
узнать «что первым» и повторяет ошибку «доказательство поверх стаба».
Real: PARTIAL (coverage-ordered план есть; dependency-ordered — нет).
Closure: принять P0/P1/P2/P3 + DAG (§2) как мастер-порядок; каждой дыре — tier + blocks/blocked-by.

### G48-2 — Инверсия «доказательство раньше возможности» закреплена планом (структурная трата)
`product-readiness-action-plan.md` §4 Фаза 2 «превратить token/semantic guard evidence в
concrete proof малыми packets», Фаза 3 learnability/DPO/lifecycle — ВСЁ downstream от
стаб-машин (P10-1 dead-gate, P21/P22/P23 lexical/keyword, P27 схема запрещает savings-claim,
P25-1 обучение регрессировало). Нет правила, гейтящего capability-зависимую доказательную/
сценарную работу за milestone M2 (реальный инференс). Прошлая сессия так и производила
`not-proven` артефакты (0/28 joined при 14 «scoped accepted»).
Real: MISSING GATE (инверсия активно закодирована).
Closure: заморозить всю capability-зависимую proof/scenario-работу до прохождения M2
(независимый held-out+ablation gate); до M2 разрешены только host-route + не-capability scaffolding.

### G48-3 — Не инвентаризован набор Luba-only продуктовых решений; он ветвит критический путь
Открытые решения разбросаны и не сведены: P05-2 (трансформерный DPO vs словарь для инференса
расширений — «принять решение»), P26-6/P26-8+P05 (remote/marketplace vs local-only lifecycle),
P44-1/P10-6 (маршрут через ILogicalOrchestrator/15 vs fixed-linear), P43-1 (мультипроектность
xiigen-platform в scope?), P43-2 (VS Code/Visual Studio wiki-plugin как 4-я host-семья в scope?),
P40-6 (топология репо: monorepo/submodule vs 3 отдельных), P45-2 (teacher/distillation:
sonnet-derived common допустим с terms vs карантин). `product-readiness-action-plan.md` §12
перечисляет продуктовые gaps, но НОЛЬ decision-gates. Каждое решение меняет размер/форму
критического пути ⇒ финальную последовательность нельзя зафиксировать, пока они открыты.
Real: MISSING (нет консолидированного decision-gate ledger).
Closure: собрать decision-gate ledger (вопрос / опции / радиус влияния / заблокированные ветки);
получить ответы Luba; ТОЛЬКО потом финализировать последовательность (F0: блокируют лишь
decision-dependent поведение, не независимый scaffolding/docs/tests/hygiene).

### G48-4 — Фундаментальный узел критпути (реальный обучаемый инференс) непроверен по физабилити; нет de-risk-спайка ПЕРЕД стройкой
Самый длинный столб (M1→M2), от которого зависит ~60% downstream, опирается на from-scratch
движок (Rule 10, no external ML), который НИ РАЗУ не показал ни одной успешной кривой обучения
на extension-задаче: единственный реальный градиентный DPO РЕГРЕССИРОВАЛ (P25-1: loss↑ к ln2,
reward-margin ↓~3×, held-out ≈0.56 на toy hash-tokenizer/1-слой — P05-3), распределение
held-out ≈ шум (P25-3). При этом в последовательности НЕТ feasibility-спайка, который де-рискует
узел ДО вложения в весь capability-трек. Отличие от P25-1 (это was defect-finding): здесь —
СЕКВЕНС-дыра: узел-фундамент недоказуем по физабилити и не гейтирован спайком перед стройкой.
Real: BUILT_NOT_PROVEN / AT-RISK (feasibility узла критпути).
Closure: выполнить bounded de-risk-спайк ПЕРВЫМ (реальный токенизатор + модель адекватного
размера на одной extension-задаче; требовать монотонное loss↓ + held-out ≫0.5) как входной
gate всего capability-трека; при недостижимости from-scratch на нужном качестве —
эскалировать Luba (scope/model-решение, связка с G48-3) ДО секвенса downstream.

---

## 5. Рекомендованный порядок исполнения (сжатый)

1. **Сначала** решить G48-3 (decision-gate ledger → ответы Luba): без scope-решений нельзя
   зафиксировать форму M3/M4/M5.
2. **M0** (P0 сохранность/build) — параллельно можно вести P3-гигиену.
3. **M1 de-risk-спайк** (G48-4) — go/no-go для всего capability-трека.
4. **M2** реальный инференс (P0-B) — снять инверсию G48-2: до этого capability-proof заморожен.
5. **M3** хребет (P1) → **M4** измерение (P2-proof) → **M5** dual-path покрытие (P2-coverage)
   → **M6** финал (≥10 review + completion matrix + git-интеграция).
6. Весь путь — с человекочитаемым phase/step-отчётом и per-milestone entry/exit-критериями;
   G48-1 закрывается принятием этого порядка как мастер-последовательности.

Текущий корректный статус: план ПОРЯДКА задан (этот проход); он не завершает продукт и не
открывает заявление о готовности. Продуктовое исполнение остаётся заблокированным до
M0→M1→M2 и до ответов Luba по decision-gates.
