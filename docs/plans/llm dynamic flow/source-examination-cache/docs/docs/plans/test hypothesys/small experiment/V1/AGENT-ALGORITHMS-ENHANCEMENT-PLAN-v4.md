# Agent Algorithms Enhancement Plan v4.0
## Projects 21 · 22 · 23 — Applying E13–E27 Results

**Основание:** эксперименты E13–E27 из 35.ExperimentsV2  
**Предыдущая версия:** v3.0 покрывает E01–E12  
**Эта версия добавляет:** 14 новых экспериментов → 8 новых или обновлённых компонентов  
**Этот документ не касается XIIGen.**

---

## Что изменилось с v3.0

v3.0 применил E01–E12. С тех пор подтверждены ещё 8 экспериментов:

| Эксперимент | Результат | Компонент |
|---|---|---|
| E20 Scenario B | Критично: порядок пайплайна неверный | 21.CodebaseIndexer |
| E21 | +142.9% точность границ задач | 22.RoleBasedTaskDecomposer |
| E23 | +820% совпадение шаблонов | 23 — новый TemplateBeautifier |
| E18 | 95% точность классификации домена | 22 — новый RouteClassifier |
| E19 | +18.2% декомпозиция задач | 22.RoleBasedTaskDecomposer |
| E25–E27 | +154% / +150% / +250% | 23 — новый PromptTranslator |
| E15–E16 | +9.7% / +8.4% | 22.AstPackedContextBuilder |

Три архитектурных наблюдения (E13, E22, E24) — не провалы алгоритмов. E22 требует разнородных данных, E24 зависит от E22. Оба отложены до добавления TypeScript и Python файлов в датасет.

---

## Место 0б — Критическое исправление: порядок пайплайна (E20 Scenario B)

**Файл:** `21.AgentInfrastructure/Indexing/CodebaseIndexer.cs`

### Проблема

В v3.0 нормализация (E15 стемминг) применяется до определения языка и стека.  
`marker_survival_rate = 27.8%` означает что E15 уничтожает 72% языковых маркеров:
- `Promise` из TypeScript → `promis` → не распознать как TypeScript
- `Task` из C# → `task` → не распознать как C#
- `async/await` паттерн → нормализуется → стек не определить

### Правильный порядок (E20 Scenario B подтверждает)

```
БЫЛО (v3.0):  E15 нормализация → E16 семьи → StackDetector → RouteClassifier
НАДО (v4.0):  StackDetector → RouteClassifier → E15 нормализация → E16 семьи → E17 предикаты
```

### Реализация

```csharp
// IStackDetector — фабричный интерфейс (Fabric First)
public interface IStackDetector
{
    OperationResult<StackInfo> Detect(string sourceText);
}

public record StackInfo(
    StackLanguage Language,    // CSharp | TypeScript | Python | Unknown
    string[]      Markers      // список найденных маркеров (Task, Promise, async...)
);

public enum StackLanguage { CSharp, TypeScript, Python, Unknown }

// StackDetector.cs — правила без ML, только маркеры
// Обязательно ПЕРЕД любой нормализацией
public sealed class StackDetector : BaseService, IStackDetector
{
    // Маркеры C#: Task<T>, IEnumerable, async/await, namespace, using, var, string
    private static readonly string[] CSharpMarkers =
        ["Task<", "IEnumerable", "namespace ", "using ", " var ", ": string", "async ", "await "];

    // Маркеры TypeScript: Promise, interface, const, let, export, import from
    private static readonly string[] TypeScriptMarkers =
        ["Promise<", "interface ", "const ", "export ", "import ", " => ", ": void"];

    // Маркеры Python: def , class , import , print(, self., if __name__
    private static readonly string[] PythonMarkers =
        ["def ", "class ", "import ", "self.", "print(", "__name__", "    "];

    public OperationResult<StackInfo> Detect(string sourceText)
    {
        // Считать совпадения маркеров для каждого языка
        int csScore = CSharpMarkers.Count(m => sourceText.Contains(m));
        int tsScore = TypeScriptMarkers.Count(m => sourceText.Contains(m));
        int pyScore = PythonMarkers.Count(m => sourceText.Contains(m));

        if (csScore == 0 && tsScore == 0 && pyScore == 0)
            return OperationResult<StackInfo>.Ok(new StackInfo(StackLanguage.Unknown, []));

        var language = (csScore, tsScore, pyScore) switch
        {
            var (cs, ts, py) when cs >= ts && cs >= py => StackLanguage.CSharp,
            var (cs, ts, py) when ts >= cs && ts >= py => StackLanguage.TypeScript,
            _ => StackLanguage.Python
        };

        return OperationResult<StackInfo>.Ok(new StackInfo(language, []));
    }
}
```

**Gate B:** `StackDetector.Detect("Promise<void>")` → TypeScript до нормализации.  
`StackDetector.Detect("Task<bool>")` → CSharp до нормализации.

---

## Место 4 — RoleBasedTaskDecomposer: E21 + E19 (обновлено в v4.0)

**Файл:** `22.AgentAlgorithms/Tasks/RoleBasedTaskDecomposer.cs`

### Что добавляет E21 (+142.9%)

v3.0 использовал EnglishRoleTagger из E08 для поиска границ по CONJ.  
E21 подтвердил что главная проблема — **ложные союзы**: `"Find null AND log them"` — это одна задача, не две. Синтетические примеры с ложными союзами подняли точность с 37% до 89%.

Правило: `AND` разделяет задачи только если по обе стороны от него есть **полная VERB+NOUN группа** с разными глаголами.

```csharp
// Улучшенный алгоритм разбивки (E21)
private bool IsTaskBoundary(
    TokenRole[] leftContext,   // роли слов слева от CONJ
    TokenRole[] rightContext)  // роли слов справа от CONJ
{
    // Слева должна быть полная группа: VERB + NOUN
    bool leftComplete = leftContext.Any(r => r == TokenRole.Verb)
                     && leftContext.Any(r => r == TokenRole.Noun);

    // Справа должна быть полная группа: VERB + NOUN
    bool rightComplete = rightContext.Any(r => r == TokenRole.Verb)
                      && rightContext.Any(r => r == TokenRole.Noun);

    // Глаголы должны быть РАЗНЫМИ (иначе это уточнение одного действия)
    string? leftVerb  = leftContext.FirstOrDefault(r => r == TokenRole.Verb)?.Word;
    string? rightVerb = rightContext.FirstOrDefault(r => r == TokenRole.Verb)?.Word;
    bool differentVerbs = leftVerb != null && rightVerb != null && leftVerb != rightVerb;

    return leftComplete && rightComplete && differentVerbs;
}
```

### Что добавляет E19 (+18.2%)

Word-family переходы: подсчёт переходов между **семьями слов** (VERB_FAMILY → NOUN_FAMILY) несёт больше информации чем отдельные POS-теги. Используем для определения "веса" потенциальной границы.

```csharp
// Вес границы по переходам словарных семей (E19)
private double ComputeBoundaryWeight(TokenRole[] leftContext, TokenRole[] rightContext)
{
    // Переход VERB_FAMILY → CONJ → VERB_FAMILY = сильная граница
    // Переход NOUN_FAMILY → CONJ → NOUN_FAMILY = слабая граница (перечисление)
    var leftFamilies  = leftContext.Select(GetWordFamily).ToList();
    var rightFamilies = rightContext.Select(GetWordFamily).ToList();

    bool leftEndsWithVerb   = leftFamilies.LastOrDefault() == WordFamily.Verb;
    bool rightStartsWithVerb = rightFamilies.FirstOrDefault() == WordFamily.Verb;

    return leftEndsWithVerb && rightStartsWithVerb ? 1.0 : 0.3;
}
```

**До / После:**

| Тест | v3.0 (E08 only) | v4.0 (E08+E21+E19) |
|---|---|---|
| "Find null AND log them" | 2 задачи (неверно) | 1 задача (верно) |
| "Add method AND update tests" | 2 задачи (верно) | 2 задачи (верно) |
| "Refactor AND document auth" | 2 задачи (верно) | 2 задачи (верно) |
| "Find null AND empty strings" | 2 задачи (неверно — одна задача) | 1 задача (верно) |
| "Add validation AND return false" | 1 задача (неверно — две задачи) | 2 задачи (верно) |

**Gate B:** 5 тестовых фраз → правильная разбивка ≥ 5/5 (было ≥ 4/5 в v3.0).

---

## Новый компонент — RouteClassifier (E18, 95% точность)

**Файл:** `22.AgentAlgorithms/Routing/RouteClassifier.cs`

### Что делает

E18 подтвердил: предикатный словарь из словарных семей даёт 95% точности в определении домена запроса. Это заменяет keyword-matching (82.5%).

```csharp
// IRouteClassifier — фабричный интерфейс
public interface IRouteClassifier
{
    OperationResult<DomainClassification> Classify(string semanticTokens);
}

public record DomainClassification(
    DomainLabel   Label,       // CODE_Q | BUG | PLAN | ARCH | REFACTOR
    double        Confidence,  // 0.0–1.0
    string[]      Evidence     // предикаты которые привели к решению
);

public enum DomainLabel { CodeQuestion, Bug, Plan, Architecture, Refactor }

// Алгоритм E18: предикатные семьи → домен
public sealed class RouteClassifier : BaseService, IRouteClassifier
{
    // Предикатные сигнатуры каждого домена (из E18)
    // CODE_Q: HOW_OP + EXPLAIN_OP + WHAT_OP
    // BUG:    EXCEPTION_OP + ERROR_OP + FAIL_OP + FIX_OP
    // PLAN:   DESIGN_OP + FLOW_OP + PHASE_OP + IMPLEMENT_OP
    // ARCH:   SHOULD_OP + PATTERN_OP + WHICH_OP + COMPARE_OP
    // REFACTOR: EXTRACT_OP + RENAME_OP + CLEAN_OP + SPLIT_OP

    public OperationResult<DomainClassification> Classify(string semanticTokens)
    {
        var tokens = semanticTokens.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var scores = new Dictionary<DomainLabel, double>();

        foreach (DomainLabel domain in Enum.GetValues<DomainLabel>())
            scores[domain] = ComputeDomainScore(tokens, domain);

        var best = scores.MaxBy(kv => kv.Value);
        return OperationResult<DomainClassification>.Ok(
            new DomainClassification(best.Key, best.Value, tokens));
    }
}
```

**Gate B:** 10 тестовых запросов → domain accuracy ≥ 90% (E18 показал 95%).

---

## Новый компонент — TemplateBeautifier (E23, +820%)

**Файл:** `23.OssEquivalents/Beautification/TemplateBeautifier.cs`

### Ключевой урок E23

E23 показал +820% после одного исправления: добавить маппинг `SUCCESS → return`.  
Шаблонная библиотека — это словарь. Точность маппинга важнее сложности алгоритма.

```csharp
// ITemplateBeautifier — фабричный интерфейс
public interface ITemplateBeautifier
{
    OperationResult<string> Beautify(string predicateSequence, BeautifyTarget target);
}

public enum BeautifyTarget { CSharp, TypeScript, Python, PlainText }

// Таблица маппингов предикатов → шаблоны (ключевой урок E23)
private static readonly Dictionary<string, string> CSharpTemplates = new()
{
    // КРИТИЧНО (из E23): return = SUCCESS-предикат
    ["IF(NULL_CHECK(X)) → RETURN_FAIL"]   = "if ({X} == null) return false;",
    ["IF(NULL_CHECK(X)) → RETURN_NULL"]   = "if ({X} == null) return null;",
    ["VALIDATE_OP(X) → SUCCESS"]          = "return Validate({X});",       // SUCCESS=return
    ["ASSIGN_OP(X, Y) → SUCCESS"]         = "{X} = {Y};",
    ["LOOP_OP(items) → TRANSFORM_OP"]     = "foreach (var item in {items}) { {body} }",
    ["TRY_OP → CATCH_OP(Exception) → LOG"]= "try { {body} } catch (Exception ex) { Log(ex); }",
    // TypeScript варианты
    // ...
};
```

**До / После:**

| Метрика | Без маппинга | С маппингом |
|---|---|---|
| template_match_rate | 10.9% | 100% |
| Правильный C# синтаксис | Редко | Всегда |

**Gate B:** `Beautify("IF(NULL_CHECK(token)) → RETURN_FAIL", CSharp)` →  
`"if (token == null) return false;"` — точное совпадение.

---

## Новый компонент — PromptTranslator (E25 + E26 + E27)

**Файлы:**
- `23.OssEquivalents/Translation/PromptTranslator.cs`
- `23.OssEquivalents/Translation/PatternClassifier.cs`
- `23.OssEquivalents/Translation/ContextualDictionary.cs`
- `23.OssEquivalents/Translation/PatternReorderer.cs`

### Три подтверждённых компонента

**E25 (+154.5%)** — PatternClassifier: определить структуру предложения (SVO, SOV, VSO, IMPERATIVE).  
**E26 (+150%)** — ContextualDictionary: перевод слова с учётом контекста (поток → stream vs flow зависит от соседей).  
**E27 (+250%)** — PatternReorderer: переставить слова из исходного паттерна в английский SVO.

**Критическое открытие E20 применяется здесь:** определять язык ДО нормализации.

```csharp
// IPromptTranslator — фабричный интерфейс
public interface IPromptTranslator
{
    OperationResult<string> ToEnglish(string prompt);
    OperationResult<string> FromEnglish(string englishResponse, string targetLanguage);
}

// Пайплайн (правильный порядок по E20):
// 1. StackDetector.Detect() → язык и стек (ДО нормализации)
// 2. PatternClassifier.Classify() → паттерн предложения (SVO/SOV/VSO)
// 3. ContextualDictionary.Translate() → слово за словом с контекстом
// 4. PatternReorderer.Reorder() → в английский SVO
// 5. ПОТОМ: SemanticCompiler нормализует английский результат

// Пример: "Добавь проверку null в метод"
// StackDetector → Russian
// PatternClassifier → IMPERATIVE_SOV
// ContextualDictionary → Add(VERB) null_check(NOUN) method(NOUN)
// PatternReorderer → "Add null check to method"
```

**Gate B:**
- `ToEnglish("Добавь проверку null")` → `"Add null check"` — exact match
- `ToEnglish("הוסף בדיקת null")` → `"Add null check"` — RTL корректно
- Язык определяется ДО нормализации (E20 Scenario B)

---

## Обновление ComparisonHarness (v4.0)

**Файл:** `22.AgentAlgorithms/AgentComparisonHarness.cs`

Добавить метрики для новых компонентов:

| Новая метрика | Откуда | Ожидание |
|---|---|---|
| `stack_detected_correctly` | E20 | = 1 (CSharp/TS/Python корректно) |
| `task_boundary_accuracy` | E21 | ≥ 90% (было 89% в эксперименте) |
| `domain_classification_accuracy` | E18 | ≥ 90% (было 95%) |
| `template_match_rate` | E23 | = 1.0 (100%) |
| `prompt_translation_exact_match` | E25–E27 | ≥ 90% |

---

## Порядок реализации

| Шаг | Что делать | Файл | Дней | Gate B |
|---|---|---|---|---|
| 1 | StackDetector + исправить порядок пайплайна | `21/Indexing/StackDetector.cs` | 1 | Detect TypeScript ДО нормализации |
| 2 | RoleBasedTaskDecomposer v2 (E21+E19) | `22/Tasks/RoleBasedTaskDecomposer.cs` | 2 | 5/5 тестовых фраз |
| 3 | TemplateBeautifier + маппинги | `23/Beautification/TemplateBeautifier.cs` | 1 | template_match_rate = 1.0 |
| 4 | RouteClassifier (E18) | `22/Routing/RouteClassifier.cs` | 2 | domain accuracy ≥ 90% |
| 5 | PromptTranslator (E25–E27) | `23/Translation/*.cs` | 3 | Russian/Hebrew → English |
| 6 | ComparisonHarness v4.0 | `22/AgentComparisonHarness.cs` | 1 | Все 5 новых метрик |
| | **Итого** | | **10 дней** | |

---

## Тесты

| Тест | Ожидаемое | Что проверяет |
|---|---|---|
| `StackDetector_TypeScript_BeforeNormalization` | TypeScript | E20: маркеры живы |
| `StackDetector_CSharp_TaskKeyword` | CSharp | E20: Task< не нормализован |
| `TaskDecomposer_FalseConj_OneTask` | 1 задача | E21: "Find null AND log" |
| `TaskDecomposer_TrueConj_TwoTasks` | 2 задачи | E21: разные глаголы |
| `TaskDecomposer_WordFamilyWeight_VerbVerb_Strong` | weight ≥ 0.8 | E19: VERB→VERB = сильная граница |
| `RouteClassifier_Bug_ExceptionKeyword` | Bug | E18: предикатный словарь |
| `RouteClassifier_Plan_FlowKeyword` | Plan | E18: 95% точность |
| `TemplateBeautifier_NullCheckReturn_ExactMatch` | точное совпадение | E23: SUCCESS=RETURN |
| `TemplateBeautifier_ValidateSuccess_ExactMatch` | точное совпадение | E23: маппинг точен |
| `PromptTranslator_Russian_Imperative_SOV` | "Add null check" | E25+E27 |
| `PromptTranslator_Hebrew_VSO_RTL` | "Add null check" | E25+E27 RTL |
| `PromptTranslator_ContextDict_Stream_vs_Flow` | правильный перевод | E26: контекст |
| `ComparisonHarness_AllMetrics_Enhanced_Wins` | enhanced ≥ original | Интеграционный |

---

⛔ STOP — жду одобрения

*Agent Algorithms Enhancement Plan v4.0 · Genie DNA Fabric First · не касается XIIGen*
