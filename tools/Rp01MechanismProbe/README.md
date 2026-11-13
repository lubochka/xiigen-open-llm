# MICRO-KNN-ORDER-01

Изолированный диагностический runner для Э-01 из `docs/plans/replanning-possible-diffs/05-small-discriminating-experiments.md`.

Использует настоящий `KnnLogicalUnit.cs` через Compile Link и project references на core contracts, Tokenizer и VectorSearch. Никакой копии vector/search алгоритма нет. Reflection только читает private tokenizer/index и вызывает настоящий private Vectorize; продуктовый код не меняется.

```powershell
dotnet build tools/Rp01MechanismProbe/Rp01MechanismProbe.csproj -c Release --nologo
dotnet tools/Rp01MechanismProbe/bin/Release/net9.0/Rp01MechanismProbe.dll <core-root> <fixture-json> <new-output-directory>
```

Runner фиксирует intent/recipe/hashes до Train, проверяет preflight и останавливается при неподтверждённой bag-order предпосылке. При выполненном preflight: три A/B rebuild, label-flip и disk recipe replay; максимум 8 Train / 128 ExecuteAsync, ноль Student/Teacher/DPO. Scientific negative не является ошибкой процесса. Техническая ошибка возвращает exit 1.

RNG HNSW остаётся unseeded; ANN-разность exploratory. Exact cosine контроль использует core `BruteForceIndex` на фактических core vectors. Recipe replay не является cold binary restore. Известные labels не используются extractor-ом: он принимает только raw input и возвращает FIRST_ACTION predicate.

Все результаты пишутся в новый research directory; существующий intent перезаписать нельзя. Сохранённые index/tokenizer/vectors служат evidence; обычный consumer этим инструментом не перенастраивается.
