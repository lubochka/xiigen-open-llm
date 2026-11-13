# Машинно сверяемые величины документа: откуда каждая получена

Дата перевывода: 12 ноября 2026 года, 19:49.

## Способ получения

Все значения ниже получены одним проходом механически, а не перенесены
из прежних кругов:

- номера строк и число строк — из ЖИВОГО дерева: `grep -n` по файлу и
  `wc -l`;
- отпечатки исходников — `git hash-object <путь>` по живому дереву;
- отпечатки библиотек и чужих файлов — из переснятых улик плеч
  `arm-before/assembly-stamp.txt`, `arm-after/assembly-stamp.txt`,
  `arm-before/source-stamp.txt`, `arm-after/source-stamp.txt`, то есть из
  тех самых файлов, на которые документ ссылается.

## Живое дерево: разметчик `12.SemanticCompiler/Prediction/FileActionKnn.cs`

| Величина | Значение | Прибор |
|---|---|---|
| число строк сейчас | 239 | `wc -l` |
| число строк в вершине | 201 | `git show HEAD:… \| wc -l` |
| `internal static SyntaxNode Parse` | строки 22 | `grep -n` |
| `public static int Label(string` | строки 33 | `grep -n` |
| `internal static int Label(SyntaxNode` | строки 53 | `grep -n` |
| `public static float\[\] FeatureVector(string` | строки 142 | `grep -n` |
| `internal static float\[\] FeatureVector(SyntaxNode` | строки 151 | `grep -n` |
| `public int Predict` | строки 203 | `grep -n` |
| `OrderBy` | строки 209, 212 | `grep -n`; второй адрес — это `OrderByDescending` в строке 212, его ловит поиск по той же подстроке |
| `OrderByDescending` | строки 212 | `grep -n` |
| `ThenBy` | строки 213 | `grep -n` |
| `private static double CosineDistance` | строки 219 | `grep -n` |
| `public FileActionKnnPredictor` | строки 190 | `grep -n` |
| отпечаток файла | `a80489ddf372ca879d4430b54853658afdd17d83` | `git hash-object` |

## Отпечатки изменённых исходников до и после переноса, прибор `git rev-parse`

Эти значения цитируются в таблице первого раздела документа. Они же лежат в
отметках плеч (`arm-before/source-stamp.txt` и `arm-after/source-stamp.txt`);
здесь они приведены ещё и прибором истории, чтобы объявленный объём выписки был
закрыт полностью.

**Важная оговорка о приборе, и я на ней сам споткнулся при закрытии этого
пробела.** Сначала я взял `git rev-parse HEAD:<путь>` — и получил
**после**-значения вместо **до**-значений. Причина: перенос уже опубликован
коммитом `07dfcbe36af`, поэтому вершина содержит его, а не состояние до него.
Правильный адрес состояния «до» — родитель публикации. Это тот же класс дефекта,
что устаревший пин в документе проекта: ссылка верна лишь относительно того
среза, в котором её взяли.

| Объект | до переноса, `07dfcbe36af^` | после переноса, `07dfcbe36af` |
|---|---|---|
| `12.SemanticCompiler/Prediction/FileActionKnn.cs` | `1f967628d150ce5affe691321c457fde682e36dc` | `a80489ddf372ca879d4430b54853658afdd17d83` |
| `12.SemanticCompiler/SemanticCompilerFacade.cs` | `7f4f316954a78f1f1e3b3e5074c4e45de58dc5d9` | `0c861c30755cd15f81fa208f9671b2846ae3e04d` |

Прибор: `git rev-parse 07dfcbe36af^:<путь>` и `git rev-parse 07dfcbe36af:<путь>`.
Вершина на момент этой записи — `040d2039aa2b`, и она уже дальше публикации,
поэтому `HEAD` для состояния «до» не годится.

## Что физически лежит на строках 138-139 разметчика сейчас

```
            predefined.Keyword.RawKind == (int)SyntaxKind.IntKeyword);
    }
```

## Живое дерево: фасад `12.SemanticCompiler/SemanticCompilerFacade.cs`

| Величина | Значение | Прибор |
|---|---|---|
| число строк сейчас | 405 | `wc -l` |
| прямые разборы текста | строки 281,374 | `grep -n` |
| вызов помощника разбора | строка 259 | `grep -n` |
| отпечаток файла | `0c861c30755cd15f81fa208f9671b2846ae3e04d` | `git hash-object` |

## Отпечатки библиотек из переснятых улик плеч, прибор `git hash-object`

| Объект | плечо «до» | плечо «после» |
|---|---|---|
| `SemanticCompiler.dll` | `7ddcf34af37640e4d75267c69bae1772cd4ac009` | `6cb03875031e543a925ddb201b75b4a7e8766c86` |
| `LlmFromScratch.Tests.dll` | `17b53bac28b0f0679ba2f6fc5a25e7b4859611f9` | `154bd278f072b947d5263cf315df2c10df4127bd` |

## Отпечатки чужих файлов из улик плеч, прибор `git hash-object`

| Объект | плечо «до» | плечо «после» | совпали |
|---|---|---|---|
| `R0201SummaryRawProvenanceTests.cs` | `073a00676ede9f08662ae2cb0a32a93596db1c62` | `073a00676ede9f08662ae2cb0a32a93596db1c62` | да |
| `summarize-test-batches.ps1` | `800fdc32e1a2774f10d036eceb49b382b27d612b` | `800fdc32e1a2774f10d036eceb49b382b27d612b` | да |

## Отпечатки файлов проверок из улик плеч, прибор `git hash-object`

| Объект | плечо «до» | плечо «после» | совпали |
|---|---|---|---|
| `FileActionPreparationLearningTests.cs` | `8e90cdd5ddcc51ae19138948956a09e3394e4a6a` | `8e90cdd5ddcc51ae19138948956a09e3394e4a6a` | да |
| `FileActionPreparationFixture.cs` | `e7226ed0f5dea104541070b6e34c165fb0613374` | `e7226ed0f5dea104541070b6e34c165fb0613374` | да |

## Расход из улик плеч

| Плечо | выделено за окно | улика |
|---|---:|---|
| before | 5004544 байт | `arm-before/alone/prepared-pairs-observation.md` |
| after | 4966168 байт | `arm-after/alone/prepared-pairs-observation.md` |
