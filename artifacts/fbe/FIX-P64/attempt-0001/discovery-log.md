# Журнал обнаружения: где места правки оказались фактически

Опоры в наряде даны как подсказка для поиска, а не как готовый адрес. Ниже — где
места нашлись на самом деле, с текстом-опорой, по которому они были найдены.

## Опора реестра совпала по существу

Строка работ «каскад ищет следующий пригодный ответ» (номер 146 в файле реестра
работ `docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md`)
указывает `TranslationUnitContracts.cs:310`. На опубликованной вершине
`486d8beeb8f694c22b9b467bc5e6f8d51fe0c7b9` файл содержал 436 строк, и строка 310
была ровно вызовом фильтра допуска для самого точного уровня:

    var full = ApplyCandidatePredicate(

То есть опора верна по существу и почти точна по номеру. Других сбитых опор в
этой работе не встретилось: все четыре адреса из наряда (урезание, каскад,
применение допуска, исполнитель) нашлись в названных файлах.

## Где урезание стоит на самом деле

В наряде урезание названо «примерно на строках 243, 253 и 255 (вызовы `Take`)».
Фактически:

| Что | Где нашлось | Текст-опора |
|---|---|---|
| Урезание, которое относится к КАСКАДУ | `TranslationUnitContracts.cs:375` и `:383`, внутри `FindExactWithSourceGuard` | `return admissible.Take(k).ToList();` и `.Take(k)` после отбора по похожести источника |
| Строки 243/253/255 | тот же файл, но ДРУГОЙ метод — `FindNearest` (объявлен на строке 239) | `return exact.Take(k).ToList();` |

Это существенное различие: `FindNearest` — отдельный старый путь поиска, который
каскад не вызывает. Правка его не касается, и все его вызовы остались прежними.
Урезание, из-за которого терялся законный кандидат, живёт в
`FindExactWithSourceGuard` — закрытом вспомогательном методе, который каскад
вызывает по одному разу на каждый уровень.

## Остальные адреса

| Что | Где нашлось | Текст-опора |
|---|---|---|
| Объявление каскада | `TranslationUnitContracts.cs:271` | `public IReadOnlyList<(string Source, string Target)> FindNearestCascade(` |
| Применение допуска (до правки — отдельным методом после урезания) | `TranslationUnitContracts.cs:336` | `private static IReadOnlyList<(string Source, string Target)> ApplyCandidatePredicate(` |
| Вызывающая сторона | `TranslationTrainedUnits.cs:467` | `private bool TrySelectKnnCandidate(` |
| Передача допуска в каскад | `TranslationTrainedUnits.cs:495` | `candidatePredicate: (_, target) =>` |
| Нижний порог источника — применялся ПОСЛЕ каскада | `TranslationTrainedUnits.cs:508` | `if (minimumSourceSimilarity > 0.0 &&` |
| Значение порога обычного направления | `TranslationTrainedUnits.cs:350` | `private const double EnRuOrdinaryKnnMinimumSourceSimilarity = 0.60;` |

## Дефект был признан самим продуктом

Правку не пришлось выводить из поведения: прежний код прямо описывал её
отсутствие в своём же комментарии (`TranslationUnitContracts.cs:331–334`):

    // The honest consequence of filtering afterwards: a sixth acceptable pair
    // standing behind five unacceptable ones in the same tier is still not
    // reached, because the cap already dropped it.

Первый перенос назвал границу честно и не скрывал её. Назначенный остаток эту
границу закрыл.

## Дочерний остаток о выдаче исполнителя

Остаток 124-S10 отдельной карточки не имеет. Его содержание нашлось в карте
владельцев работ
(`docs/plans/replanning-possible-diffs/fixes-based-on-experiments/evidence/2026-11-03-independent-work-accounting.md`,
строки 60 и 128): «дочерний остаток полного admission до cap/tier» и
«Executor source floor остаётся после cascade». Это и есть вторая половина
назначенной работы — согласование нижнего порога источника с тем же порядком.
