# LLM-MVP-CORE: Master Implementation Plan v1.0
## Внедрение E01-E50 — Подпроекты 12–17
## Подробный план для Claude Code v1.0

**Основание:** 50 экспериментов завершены. 28 PASS, 4 SCALE_UP, 18 FAILED/ARCH.
**Этот документ не касается XIIGen.**

---

## Что уже готово (не входит в этот план)

| Что | Где | Статус |
|---|---|---|
| Базовый пайплайн 01-11 | `/home/claude/llm_mvp2/llm_mvp_core/` | ✅ done |
| AgentInfrastructure 21 | `21.AgentInfrastructure/` | ✅ db80dc5 |
| AgentAlgorithms 22 v5.0 | plan готов, не реализован | ⏳ следующий |
| OssEquivalents 23 | `23.OssEquivalents/` | ✅ done |
| Session & Skills library | plan готов | ⏳ следующий |
| Experiments E01-E50 | `35.ExperimentsV2/` | ✅ done |

---

## Канонические имена доменов

Использовать единообразно везде: в enum, в коде, в тестах, в документах.

| Краткое (в коде) | Полное (в комментариях) | Что означает |
|---|---|---|
| `CODE_Q` | CodeQuestion | Вопрос о работе кода, объяснение |
| `BUG` | BugFix | Исправление ошибки |
| `PLAN` | Planning | Планирование, архитектура задачи |
| `ARCH` | Architecture | Проектирование системы |
| `REFACTOR` | Refactoring | Улучшение кода без изменения поведения |

Короткие формы (`CODE_Q`, `BUG`, `PLAN`, `ARCH`, `REFACTOR`) используются
в enum-значениях и сравнениях. Длинные формы — только в комментариях.
Этот список является единственным источником правды. D-04 в DECISIONS-LOCKED.md
и все планы подпроектов 12 и 15 должны использовать именно эти имена.

---

## Шесть подпроектов — порядок и зависимости

```
01.Tokenizer ──────┐
09.Inference ──────┤──► 12.SemanticCompiler ──┬──► 13.VS Extension
10.LoRA ───────────┤                          │
11.DPO ────────────┤──► 14.ContinuousLearning─┤
22.AgentOrchestrator──► 15.OrchestratorModel  │──► 17.DynamicDomains
                        16.AutonomousLearning ─┘
```

**Три независимых трека которые можно вести параллельно:**
- Трек A: 12 → 13 (компрессор + VS Extension)
- Трек Б: 12 → 14 → 16 (обучение)
- Трек В: 22 → 15 → 17 (оркестратор)

12.SemanticCompiler нужен всем — он первый.

---

---

# Подпроект 12 — SemanticCompiler
## Все E01-E27 результаты в один пайплайн

**Папка:** `12.SemanticCompiler/`
**Дней:** 15
**Зависит от:** 01.Tokenizer, 09.Inference (уже готовы)

### Структура

```
12.SemanticCompiler/
├── Compression/
│   ├── AstTokenizer.cs         — E01 +93%: AST вместо BPE
│   ├── RoleAssigner.cs         — E08+E09 +56%/+45%: синт. роли
│   ├── SemanticNormalizer.cs   — E15 +9.7%: стоп-слова + стемминг
│   ├── FamilyCollapser.cs      — E16 +8.4%: семейная группировка
│   └── LogicExtractor.cs       — E17 +97%: предикаты → 5% размера
├── Routing/
│   ├── StackDetector.cs        — E20: C#/TS/PY ПЕРЕД нормализацией
│   └── RouteClassifier.cs      — E18: 95% точность домена
├── Planning/
│   ├── Decomposer.cs           — E21 +143%: границы подзадач
│   ├── StopSignalDetector.cs   — E33 +25%: когда остановить
│   └── DependencyGraph.cs      — E35 +30%: A→B vs A||B
├── Translation/
│   ├── PatternClassifier.cs    — E25 +154%: SVO/SOV/VSO
│   ├── ContextualDictionary.cs — E26 +150%: слово + контекст
│   └── PatternReorderer.cs     — E27 +250%: в английский SVO
└── Beautification/
    ├── TemplateBeautifier.cs   — E23 +820%: SUCCESS=RETURN маппинг
    ├── CSharpBeautifier.cs     — предикаты → C# код
    └── MergeAssembler.cs       — объединить ответы доменов
```

### Ключевые правила реализации

**E20 КРИТИЧНО — порядок пайплайна:**
```
ПРАВИЛЬНО: StackDetector → RouteClassifier → E15 → E16 → E17
НЕВЕРНО:   E15 → E16 → StackDetector (маркеры разрушаются)
```

**E23 КРИТИЧНО — SUCCESS=RETURN маппинг:**
```csharp
// Без этого: template_match_rate = 10.9%
// С этим:    template_match_rate = 100%
["VALIDATE_OP(X) → SUCCESS"] = "return Validate({X});",  // SUCCESS = return!
```

**E44 КРИТИЧНО — max_weight формула:**
```csharp
// НЕ константа ×10 — это вызвало катастрофическое забывание
double maxWeight = totalExamples / ((double)numClasses * rareExamples);
// При 1000 примерах, 5 классах, 50 security: maxWeight = 4.0
```

### Gate B

```bash
# Полный пайплайн на Tier 3 (xiigen 343 + TS + PY файлы):
# E17 perplexity на тесте: < 10.0 (было 5.6 в эксперименте)
# E18 domain accuracy: > 90%
# E23 template_match_rate: = 100%
# E20 StackDetector: TypeScript найден ДО нормализации
dotnet test --filter "SemanticCompiler" --no-build
```

---

# Подпроект 13 — VisualStudioExtension
## Локальный AI-ассистент в IDE

**Папка:** `13.VisualStudioExtension/`
**Дней:** 15
**Зависит от:** 09.Inference, 12.SemanticCompiler, 23.OssEquivalents (McpTransport)

### Архитектура

```
Пользователь в VS / VS Code / JetBrains / Cursor
         ↕ Named Pipe (McpTransport из 23 — уже готов!)
LlmServer.exe (фоновый процесс)
         ↕ внутри
12.SemanticCompiler → 09.Inference → ответ
```

### Структура

```
13.VisualStudioExtension/
├── LlmServer/
│   └── Program.cs              — загрузить модель + Named Pipe loop
├── Chat/
│   ├── ChatToolWindow.cs       — VSIX ToolWindow
│   └── ChatWindowControl.xaml  — WPF UI (поле ввода + лента)
├── Context/
│   └── VsSolutionContextProvider.cs — DTE.ActiveDocument + Solution
└── Server/
    └── LlmServerProxy.cs       — клиент к Named Pipe
```

### VS Code / JetBrains (один JSON файл, плагин не нужен)

```json
// .vscode/mcp.json — VS Code, Cursor, JetBrains 2025.2+
{
  "servers": {
    "llm-local": {
      "type": "stdio",
      "command": "LlmServer.exe"
    }
  }
}
```

Visual Studio (full) — отдельный VSIX.
VS Code, Cursor, JetBrains 2025.2+ — только mcp.json.

### Freemium модель монетизации

Активация через сайт (как Tabnine/Wallaby):
- **Free:** Small 29M модель, базовый чат
- **Pro $15-30/мес:** Default 888M, LEGACY + DOCS домены
- **Enterprise $50-100/seat:** air-gapped, корп. сервер, SLA

### Gate B

```bash
# Интеграционный тест:
# 1. LlmServer.exe запускается
# 2. Named Pipe принимает соединение
# 3. Ответ на "Add null check" приходит < 5 секунд
# 4. VS Code mcp.json подключается без ошибок
```

---

# Подпроект 14 — ContinuousLearning
## LoRA адаптация на проект пользователя

**Папка:** `14.ContinuousLearning/`
**Дней:** 12
**Зависит от:** 10.LoRA, 11.DPO, 12.SemanticCompiler

### Структура

```
14.ContinuousLearning/
├── ProjectIndexer.cs      — сканировать проект → доменные датасеты
├── DomainDatasetCache.cs  — SQLite: (промпт, ответ, домен, вес, тип)
├── FeedbackRouter.cs      — ACCEPTED/EDITED/REJECTED → DPO пара
├── IncrementalTrainer.cs  — LoRA fine-tuning на новых примерах
└── DomainModelRegistry.cs — base.bin + project.lora ≤2 МБ
```

### Три сигнала обратной связи

```
Принял ответ   → positive example (chosen)    → domain dataset
Отклонил       → negative example (rejected)  → domain dataset
Поправил вручную → DPO пара: chosen=правка, rejected=оригинал
```

DPO пары в 10x ценнее обычных примеров (информация о том что НЕ делать).

### Параметр max_weight

```csharp
// ОБЯЗАТЕЛЬНО рассчитывать динамически (E44 урок):
public double ComputeMaxWeight(int totalExamples, int numClasses, int rareExamples)
    => totalExamples / ((double)numClasses * rareExamples);
// НЕ хардкодить ×10
```

### Gate B

```bash
# Первый тест специализации (E28v2 при Tier 3):
# ProjectAdaptor: adapted_accuracy > generic_accuracy
# Если нет → это настоящий провал LoRA, вынести решение
dotnet test --filter "ContinuousLearning" --no-build
```

---

# Подпроект 15 — OrchestratorModel
## Декомпозиция эпизодов + запуск агентов

**Папка:** `15.OrchestratorModel/`
**Дней:** 12
**Зависит от:** 12.SemanticCompiler, 22.AgentOrchestrator (после v5.0)

### Компоненты (все подтверждены экспериментами)

```
15.OrchestratorModel/
├── EpisodeClassifier.cs      — E18 RouteClassifier: тип задачи
├── EpisodeDecomposer.cs      — E32 +11%: RESEARCH→4-5, BUILD→2-3
├── StopSignalDetector.cs     — E33 +25%: 4 сигнала остановки
├── DependencyGraph.cs        — E35 +30%: порядок + параллельность
├── AgentLauncher.cs          — запустить агента для под-эпизода
├── ResultMerger.cs           — E23 Beautifier: объединить ответы
└── EpisodeMemory.cs          — DomainDatasetCache из 14 для истории
```

### Обучающие данные (уже есть)

220 эпизодов из EPISODE-MAP-EXTENDED.md = готовый корпус:
- INPUT: "что пользователь хотел"
- OUTPUT: как задача делится + зависимости + где остановиться

### QualityOrchestrator (16 проверок перед каждым STOP)

```
Обычный оркестратор (горизонтальный):
  задача → [агент A, агент B] → Merger

QualityOrchestrator (вертикальный):
  результат → [проверка 1..16] → PASS или → исправление
```

### Gate B

```bash
# "Deploy new application" → правильный порядок:
# [ARCH, CODE_Q, DevOps] где ARCH на уровне 0
# StopSignal на 3 уровнях не уходит глубже
dotnet test --filter "OrchestratorModel" --no-build
```

---

# Подпроект 16 — AutonomousLearning
## Автономный сбор данных + обучение ночью

**Папка:** `16.AutonomousLearning/`
**Дней:** 10
**Зависит от:** 14.ContinuousLearning, 12.SemanticCompiler

### Трёхуровневое хранилище (≤2.5 ГБ всегда)

```
HOT  ≤500 МБ  — сырые данные, удаляются после E01-E27 обработки
WARM ≤1 ГБ    — предикаты (5% от исходного), ротируются 6 мес.
COLD ≤50 МБ   — дистиллированный LoRA адаптер, не растёт
Base ≤0.9 ГБ  — базовая модель, неизменна
```

### Структура

```
16.AutonomousLearning/
├── DataSourceDiscoverer.cs  — GitHub/SO/corporate по стеку
├── TrustScorer.cs           — E40v2: 0.0-1.0 по метаданным
├── SourceFilter.cs          — дата/домен/trust_score фильтры
├── IdleDetector.cs          — E41: CPU+ввод+батарея+диск
├── DataPipeline.cs          — StackDetector→E15→E16→E17
├── ChunkBuilder.cs          — HOT/WARM/COLD + StratifiedSplit
└── AutonomousTrainer.cs     — E43 ReplayBuffer 70/30 + расписание
```

### Пользовательское управление

```
Источники: [✓] Внутренний Git     Приоритет: ████████ (1.0)
            [✓] GitHub dotnet/*   Приоритет: ███████  (0.9)
            [ ] Интернет (весь)   Приоритет: ░░░░     (выкл.)

Фильтры: Дата с [01.01.2022], язык [C#][TS], trust ≥ [0.7]
Расписание: CPU < [15%], задержка ввода [30 мин], заряд > [20%]

Безопасность: [ ] Интернет ← Enterprise выключает
              [✓] Только корп. Git
              [✓] Логировать источники (audit trail)
```

### Gate B

```bash
# Ночной цикл симуляция:
# IdleDetector: все 4 условия → IDLE
# DataPipeline: 200 примеров за 5 минут
# AutonomousTrainer: LoRA обучение < 15 минут
# HOT очищен после обработки, WARM < 1 ГБ
dotnet test --filter "AutonomousLearning" --no-build

# ОБЯЗАТЕЛЬНО: DataPipeline делегирует в 12, не копирует логику
grep -n "ISemanticCompiler" 16.AutonomousLearning/DataPipeline.cs
# Должна быть строка: ISemanticCompiler _compiler (параметр конструктора)
# Если нет → FAIL: DataPipeline содержит дублирующую копию E15-E17
```

**Примечание:** `DataPipeline` в 16 принимает `ISemanticCompiler` через конструктор
и делегирует в `12.SemanticCompiler`. Никакой собственной реализации
`StackDetector → E15 → E16 → E17` в подпроекте 16 нет. Баг в 12 исправляется
один раз — 16 получает исправление автоматически.

---

# Подпроект 17 — DynamicDomains
## Новый домен → найти / обучить / расширить

**Папка:** `17.DynamicDomains/`
**Дней:** 10
**Зависит от:** 14.ContinuousLearning, 15.OrchestratorModel

### Три пути при новом домене

```
RouteClassifier confidence < 0.6 → DomainExpansionOrchestrator
  ↓
  Путь 1: ModelHub.Search → скачать готовый адаптер (30 сек)
  Путь 2: DataCollector → собрать + обучить ночью (3 ночи)
  Путь 3: Спросить пользователя → он уточняет что важно
```

### Структура

```
17.DynamicDomains/
├── DomainRegistry.cs        — реестр: id, описание, задачи, модель, confidence
├── ModelHub.cs              — NuGet-стиль для LoRA адаптеров 2 МБ
├── IModelRepository.cs      — интерфейс (публичный/корп. Hub)
├── DomainDiscoverer.cs      — анализирует задачи нового домена
├── OrchestratorExpander.cs  — E49 +44%: граф зависимостей
└── TransferLearner.cs       — E50 +75%: от ближайшего домена
```

### ModelHub монетизация

```
Free:       скачивать публичные адаптеры
Pro:        приоритетные адаптеры
Enterprise: приватный ModelHub внутри периметра
```

Это как NuGet/npm но для обученных моделей.

### Gate B

```bash
# "Write Dockerfile for .NET app":
# RouteClassifier confidence < 0.6
# ModelHub находит "devops-docker" адаптер
# Скачивание < 30 сек
# Следующий запрос "Create GitHub Actions" → confidence > 0.7 (домен известен)
dotnet test --filter "DynamicDomains" --no-build
```

---

## Временная шкала и параллельные треки

```
Неделя 1-2:  22.AgentAlgorithms v5.0 + 21-23 Session Library
             (ContextWindowManager + AgentOrchestrator)

Неделя 3-5:  12.SemanticCompiler ← все три трека начинаются здесь

Трек A       Трек Б              Трек В
Неделя 6-8:  13.VS Extension     14.ContinuousLearning   15.OrchestratorModel
Неделя 9-10: —                   16.AutonomousLearning    17.DynamicDomains

Итого: ~10-12 недель параллельно
```

## Не входит в этот план (требуют больше данных)

| Эксперимент | Статус | Что нужно |
|---|---|---|
| E28-E31 v2 ProbeNN | SCALE_UP | Tier 3 данные (≥200 файлов) |
| E22 RouteClassifier | SCALE_UP | TypeScript + Python датасет |
| E44-E45 Curriculum | SCALE_UP | Gradient + правильный max_weight |

Эти три проверяются в 14.ContinuousLearning с реальным LoRA —
PatternClassifier + LoRA слой + дообучение на проекте.
Там впервые будет настоящий градиентный сигнал.

## Финальная проверка всей системы

После всех шести подпроектов — интеграционный сценарий:

```
Пользователь: "Write Dockerfile for this .NET app" (в VS Extension)
  ↓
12.SemanticCompiler: сжать промпт → предикаты
  ↓
RouteClassifier: confidence < 0.6 → новый домен
  ↓
17.DynamicDomains: ModelHub → devops-docker.lora (30 сек)
  ↓
14.ContinuousLearning: загрузить base.bin + devops-docker.lora
  ↓
09.Inference: генерировать Dockerfile
  ↓
13.VS Extension: показать в чате IDE
  ↓
Пользователь: принял → FeedbackRouter → DomainDatasetCache
  ↓
16.AutonomousLearning: ночью → дообучить devops домен
```

Весь цикл: от промпта до ответа < 5 секунд.
От незнакомого домена до специализированной модели < 3 ночи.
