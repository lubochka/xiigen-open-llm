# E09 — Патч: синтаксическая роль вместо позиции
## Plan v1.0 · 35.ExperimentsV2

**Этот документ не касается XIIGen.**

---

## Проблема в двух словах

Сейчас вектор хранит `PositionInParent=0`. Это число означает
"первый ребёнок своего родителя". Но первым ребёнком может быть:
- левая часть присваивания (`A` в `A = B + F`)
- первый аргумент вызова (`x` в `Foo(x, y)`)
- первый элемент массива (`1` в `new[] {1, 2, 3}`)

Это разные **роли** — но вектор их не различает.
K-NN считает их похожими → предсказание плохое.

Нужно добавить одно поле: **SyntacticRole** — именованная роль
которую узел играет в своём родителе.

---

## Таблица ролей (30 значений, vocab=30)

```
 0  UNKNOWN           — не подошло ни одно правило
 1  ASSIGNED_TO       — левая часть присваивания:   A в  A = B
 2  ASSIGNED_VALUE    — правая часть присваивания:  B в  A = B
 3  BINARY_LEFT       — левый операнд:              A в  A + B
 4  BINARY_RIGHT      — правый операнд:             B в  A + B
 5  CALLED_TARGET     — что вызывается:             Foo в  Foo(x)
 6  ARGUMENT          — аргумент вызова:             x в  Foo(x)
 7  IF_CONDITION      — условие:                    x>0 в  if(x>0)
 8  IF_THEN           — тело then:                  {...} в  if(...) {...}
 9  IF_ELSE           — тело else:                  {...} в  if(...) {} else {...}
10  RETURN_VALUE      — возвращаемое значение:       x в  return x
11  VAR_NAME          — имя переменной:              x в  int x = 5
12  VAR_INIT          — начальное значение:          5 в  int x = 5
13  MEMBER_TARGET     — объект перед точкой:        list в  list.Add(x)
14  MEMBER_NAME       — имя после точки:            Add в  list.Add(x)
15  METHOD_PARAM      — параметр метода:             x в  void Foo(int x)
16  METHOD_BODY       — тело метода:                {...} в  void Foo() {...}
17  METHOD_RETURN_TYPE — тип возврата:              int в  int Foo()
18  WHILE_CONDITION   — условие цикла:              i<10 в  while(i<10)
19  WHILE_BODY        — тело цикла:                 {...} в  while(...) {...}
20  FOR_INIT          — инициализация for:          int i=0 в  for(int i=0;...)
21  FOR_CONDITION     — условие for:                i<10 в  for(...;i<10;...)
22  FOR_INCREMENT     — шаг for:                    i++ в  for(...;...;i++)
23  FOR_BODY          — тело for:                   {...} в  for(...) {...}
24  CAST_TYPE         — тип приведения:             int в  (int)x
25  CAST_EXPRESSION   — что приводится:             x в  (int)x
26  LAMBDA_PARAM      — параметр лямбды:            x в  x => x+1
27  LAMBDA_BODY       — тело лямбды:                x+1 в  x => x+1
28  ARRAY_ELEMENT     — элемент массива:            1 в  new[]{1,2,3}
29  BLOCK_STATEMENT   — утверждение в блоке:        return x; внутри {}
```

---

## Шаг 1 — Добавить поле в `StructuralVector`

**Файл:** `35.ExperimentsV2/Infrastructure/StructuralVectorExtractor.cs`

В `StructuralVector` record — добавить одно поле после `AstNodeType`:

```csharp
public readonly record struct StructuralVector
{
    public short AstNodeType      { get; init; }
    // ДОБАВИТЬ:
    public byte  SyntacticRole    { get; init; }  // роль из таблицы выше (0-29)
    // остальные поля без изменений
    public short Parent1Type      { get; init; }
    public short Parent2Type      { get; init; }
    public short Parent3Type      { get; init; }
    public byte  PositionInParent { get; init; }
    public byte  SiblingCount     { get; init; }
    public byte  DepthInMethod    { get; init; }
    public byte  ScopeDepth       { get; init; }
    public short FileId           { get; init; }
    ...
}
```

В `DistanceTo` — добавить роль с самым высоким весом:

```csharp
public double DistanceTo(StructuralVector other) =>
    // ДОБАВИТЬ первой строкой:
    (SyntacticRole    != other.SyntacticRole    ? 4.0 : 0) +  // новое — самый важный
    // существующие поля — уменьшить вес PositionInParent:
    (Parent1Type      != other.Parent1Type      ? 2.0 : 0) +
    (AstNodeType      != other.AstNodeType      ? 2.0 : 0) +
    (Parent2Type      != other.Parent2Type      ? 1.5 : 0) +
    (Parent3Type      != other.Parent3Type      ? 1.0 : 0) +
    (PositionInParent != other.PositionInParent ? 0.3 : 0) +  // было 1.0 → теперь 0.3
    Math.Abs(ScopeDepth    - other.ScopeDepth)    * 0.3 +
    Math.Abs(DepthInMethod - other.DepthInMethod) * 0.2;
```

---

## Шаг 2 — Добавить `SyntacticRoleMapper.cs`

**Новый файл:** `35.ExperimentsV2/Infrastructure/SyntacticRoleMapper.cs`

```csharp
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// SyntacticRoleMapper определяет роль узла в дереве компилятора.
// Роль = что именно этот узел означает для своего родителя:
// "левая часть присваивания", "аргумент вызова", "условие if" и т.д.
// Это не то же самое что тип родителя — два узла одного типа могут играть
// разные роли (левый и правый операнд Binary выражения).
public static class SyntacticRoleMapper
{
    // Получить числовой идентификатор роли узла node в его родителе.
    // Возвращает 0 (UNKNOWN) если родитель неизвестен или правило не подошло.
    public static byte GetRole(SyntaxNode node)
    {
        var parent = node.Parent;
        if (parent == null) return 0;

        switch (parent.Kind())
        {
            case SyntaxKind.SimpleAssignmentExpression:
            case SyntaxKind.AddAssignmentExpression:
            case SyntaxKind.SubtractAssignmentExpression:
            case SyntaxKind.MultiplyAssignmentExpression:
            {
                // AssignmentExpression: первый ребёнок = левая часть, второй = правая
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)1   // ASSIGNED_TO
                     : idx == 1 ? (byte)2   // ASSIGNED_VALUE
                     : (byte)0;
            }

            case SyntaxKind.AddExpression:
            case SyntaxKind.SubtractExpression:
            case SyntaxKind.MultiplyExpression:
            case SyntaxKind.DivideExpression:
            case SyntaxKind.ModuloExpression:
            case SyntaxKind.EqualsExpression:
            case SyntaxKind.NotEqualsExpression:
            case SyntaxKind.LessThanExpression:
            case SyntaxKind.GreaterThanExpression:
            case SyntaxKind.LogicalAndExpression:
            case SyntaxKind.LogicalOrExpression:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)3   // BINARY_LEFT
                     : idx == 1 ? (byte)4   // BINARY_RIGHT
                     : (byte)0;
            }

            case SyntaxKind.InvocationExpression:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)5   // CALLED_TARGET
                     : (byte)6;             // ARGUMENT (ArgumentList)
            }

            case SyntaxKind.Argument:
                return 6;   // ARGUMENT

            case SyntaxKind.IfStatement:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)7   // IF_CONDITION
                     : idx == 1 ? (byte)8   // IF_THEN
                     : (byte)9;             // IF_ELSE
            }

            case SyntaxKind.ReturnStatement:
                return 10;  // RETURN_VALUE

            case SyntaxKind.VariableDeclarator:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)11  // VAR_NAME
                     : (byte)12;            // VAR_INIT
            }

            case SyntaxKind.MemberAccessExpression:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)13  // MEMBER_TARGET
                     : (byte)14;            // MEMBER_NAME
            }

            case SyntaxKind.Parameter:
                return 15;  // METHOD_PARAM

            case SyntaxKind.MethodDeclaration:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                // Последний ребёнок обычно тело метода (Block)
                if (node.IsKind(SyntaxKind.Block)) return 16;      // METHOD_BODY
                if (idx == 0) return 17;                            // METHOD_RETURN_TYPE
                return 15;                                          // METHOD_PARAM
            }

            case SyntaxKind.WhileStatement:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)18  // WHILE_CONDITION
                     : (byte)19;            // WHILE_BODY
            }

            case SyntaxKind.ForStatement:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)20  // FOR_INIT
                     : idx == 1 ? (byte)21  // FOR_CONDITION
                     : idx == 2 ? (byte)22  // FOR_INCREMENT
                     : (byte)23;            // FOR_BODY
            }

            case SyntaxKind.CastExpression:
            {
                var children = parent.ChildNodes().ToList();
                int idx = children.IndexOf(node);
                return idx == 0 ? (byte)24  // CAST_TYPE
                     : (byte)25;            // CAST_EXPRESSION
            }

            case SyntaxKind.SimpleLambdaExpression:
            case SyntaxKind.ParenthesizedLambdaExpression:
            {
                if (node.IsKind(SyntaxKind.Parameter)) return 26;  // LAMBDA_PARAM
                return 27;                                          // LAMBDA_BODY
            }

            case SyntaxKind.ArrayInitializerExpression:
            case SyntaxKind.ImplicitArrayCreationExpression:
                return 28;  // ARRAY_ELEMENT

            case SyntaxKind.Block:
                return 29;  // BLOCK_STATEMENT
        }

        return 0; // UNKNOWN
    }
}
```

---

## Шаг 3 — Использовать роль в `StructuralVectorExtractor.Extract()`

В методе `Extract()` — добавить одну строку для заполнения `SyntacticRole`:

```csharp
public static StructuralVector Extract(SyntaxNode node, short fileId = 0)
{
    // ... существующий код без изменений ...

    return new StructuralVector
    {
        AstNodeType      = NodeTypeId(node),
        // ДОБАВИТЬ:
        SyntacticRole    = SyntacticRoleMapper.GetRole(node),
        // остальные поля без изменений:
        Parent1Type      = p1,
        Parent2Type      = p2,
        Parent3Type      = p3,
        PositionInParent = posInParent,
        SiblingCount     = siblingCount,
        DepthInMethod    = ...,
        ScopeDepth       = ...,
        FileId           = fileId
    };
}
```

---

## Шаг 4 — Сборка и запуск

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

**Запустить E09:**
```csharp
runner.Run("E09", tier: 1);
```

**Ожидаемый вывод — должен улучшиться по сравнению с +3.1%:**
```
[E09] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E09] Запуск...
OK top1_accuracy (up) baseline=0.337 enhanced=0.420 delta=+24.6% | PASSED
  -> .../E09-STRUCTURAL-KNN/results/run-YYYY-MM-DD-002.json
```

**Что смотреть в notes:**
```
tokens=968, k=5, baseAcc=33.7%, knnAcc=42.0%, avgDist=1.85
```

`avgDist` должен упасть (одинаковые роли → соседи ближе).
`knnAcc` должен вырасти (правый операнд предсказывает следующий узел лучше).

**Если delta всё ещё < 5%:** добавить `ShouldScaleUp = avgDist > 1.5`
чтобы автоматически перейти на Tier 2.

---

## Почему это работает одинаково для кода и текста

В коде `A = B + F`:
- `A` имеет роль `ASSIGNED_TO`
- `B` имеет роль `BINARY_LEFT`
- `F` имеет роль `BINARY_RIGHT`

В тексте "кот ловит мышь":
- "кот" имеет роль `nsubj` (подлежащее)
- "ловит" имеет роль `ROOT` (главный глагол)
- "мышь" имеет роль `dobj` (прямое дополнение)

Это одна и та же идея: **каждый элемент играет именованную роль
в структуре к которой принадлежит**. K-NN работает потому что
элементы с одинаковой ролью имеют похожих соседей —
правый операнд чаще всего ведёт к скобке или точке с запятой,
прямое дополнение чаще всего следует за глаголом.

Для E08 (текст) — SpaCy даёт `dep_` метки (`nsubj`, `dobj`, `amod`...)
которые играют ту же роль что `SyntacticRole` здесь.
Когда E08 будет запущен — можно сравнить насколько сильно улучшение
и это даст ответ: универсален ли принцип ролей или специфичен для кода.

---

*E09 Role Patch v1.0 · не касается XIIGen*
