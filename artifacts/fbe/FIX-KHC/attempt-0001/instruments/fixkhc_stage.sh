#!/bin/bash
# Этап работы по единому разбору для признака и метки урока (FIX-KHC).
# Проверка занятости стоит УСЛОВИЕМ внутри скрипта и останавливает этап.
# Параметр: имя подкаталога улик.
set -o pipefail
cd /c/Projects/llm_mvp_core
STAGE="$1"
OUT="artifacts/fbe/FIX-KHC/attempt-0001/$STAGE"
mkdir -p "$OUT" "$OUT/tests"
NEW='FullyQualifiedName=LlmFromScratch.Tests.FileActionPreparationLearningTests.SharedParse_PreservesPreparedPairsThroughFacade'
EXIST='FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_MemberAccessNoNullCheck_ReturnsAddNullCheck|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_SmallClass_ReturnsAddMethod|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_CollectionParamNoLoop_ReturnsAddLoop|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_FeatureVector_HasCorrectLength|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_FeatureVector_IsNormalised|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.ActionLabelGenerator_LabelIsDeterministic|FullyQualifiedName=LlmFromScratch.Tests.E13E14Tests.FileActionKnnPredictor_PredictsMajorityOnHomogeneousTraining|FullyQualifiedName=LlmFromScratch.Tests.E13E14FeatureFlagTests.UseFileActionKnnFalseAlwaysLeavesPredictedFileActionNull|FullyQualifiedName=LlmFromScratch.Tests.E13E14FeatureFlagTests.UseFileActionKnnTrueWithUntrainedPredictorSkipsSilently|FullyQualifiedName=LlmFromScratch.Tests.E13E14FeatureFlagTests.TrainFileActionKnnPopulatesTheFacadePredictor|FullyQualifiedName=LlmFromScratch.Tests.E13E14FeatureFlagTests.UseFileActionKnnTrueWithTrainedPredictorProducesKnownLabel'
DLLS="Tests/bin/Release/net9.0/SemanticCompiler.dll Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll"

busy() { powershell -NoProfile -ExecutionPolicy Bypass -File /c/Users/lubao/AppData/Local/Temp/busy_check.ps1 2>/dev/null | tr -d ' \r\n'; }

require_free() {
  local n; n=$(busy)
  { echo "=== $STAGE: занятость, $1, $(date '+%Y-%m-%d %H:%M:%S') ==="
    echo "чужих процессов на общем каталоге (сборки+прогоны): $n"
    echo "свежих файлов в Tests/obj,Tests/bin за 2 минуты: $(find Tests/obj Tests/bin -type f -newermt '-2 minutes' 2>/dev/null | wc -l)"; } >> "$OUT/occupancy.log"
  if [ "$n" != "0" ]; then echo "ОСТАНОВ: каталог занят ($n) в момент «$1»" | tee -a "$OUT/occupancy.log"; return 1; fi
  return 0
}

stamp_libraries() {
  echo "=== $STAGE: отпечатки собранных библиотек, $1, $(date '+%Y-%m-%d %H:%M:%S') ===" >> "$OUT/assembly-stamp.txt"
  for dll in $DLLS; do
    if [ -f "$dll" ]; then
      { echo "объект: файл библиотеки $(pwd)/$dll"
        echo "  размер в байтах: $(stat -c %s "$dll")"
        echo "  git hash-object (SHA-1 git-объекта): $(git hash-object "$dll")"
        echo "  sha256sum (SHA-256 байтов): $(sha256sum "$dll" | cut -d' ' -f1)"
        echo "  время записи файла: $(stat -c %y "$dll")"; } >> "$OUT/assembly-stamp.txt"
    else
      echo "объект: файл библиотеки $dll НЕ НАЙДЕН" >> "$OUT/assembly-stamp.txt"
    fi
  done
  powershell -NoProfile -Command "'номер модуля библиотеки семантического компилятора (MVID): ' + [System.Reflection.Assembly]::LoadFrom((Resolve-Path 'Tests/bin/Release/net9.0/SemanticCompiler.dll')).ManifestModule.ModuleVersionId" 2>&1 | sed 's/^/  /' >> "$OUT/assembly-stamp.txt"
}

: > "$OUT/occupancy.log"; : > "$OUT/assembly-stamp.txt"

echo "=== $STAGE: отпечатки исходников, оба прибора, весь файл ===" > "$OUT/source-stamp.txt"
for src in 12.SemanticCompiler/SemanticCompilerFacade.cs 12.SemanticCompiler/Prediction/FileActionKnn.cs Tests/FileActionPreparationLearningTests.cs Tests/ExperimentFixes/FileActionPreparationFixture.cs Tests/Fixtures/ExperimentFixes/v1/Khc01/manifest.json; do
  { echo "объект: файл $(pwd)/$src"
    echo "  размер в байтах: $(stat -c %s "$src")"
    echo "  git hash-object: $(git hash-object "$src")"
    echo "  sha256sum: $(sha256sum "$src" | cut -d' ' -f1)"; } >> "$OUT/source-stamp.txt"
done
sed -n '244,254p' 12.SemanticCompiler/SemanticCompilerFacade.cs > "$OUT/source-preparation-site.txt"

require_free "перед сборкой" || exit 20
echo "=== $STAGE: пересборка, $(date '+%H:%M:%S') ==="
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild /nodeReuse:false > "$OUT/build.log" 2>&1
BUILD_EXIT=$?
echo "код возврата сборки: $BUILD_EXIT" > "$OUT/build-exit.txt"; cat "$OUT/build-exit.txt"
require_free "сразу после сборки" || echo "предупреждение: чужой процесс появился во время моей сборки" >> "$OUT/occupancy.log"
if [ "$BUILD_EXIT" != "0" ]; then echo "СБОРКА УПАЛА"; grep -E "error " "$OUT/build.log" | head -10; exit 10; fi
stamp_libraries "до прогонов"

require_free "перед обнаружением" || exit 21
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests > "$OUT/discovery-full.log" 2>&1
echo "код возврата обнаружения: $?" > "$OUT/discovery-exit.txt"
{ grep -n "FileActionPreparationLearningTests\." "$OUT/discovery-full.log"
  grep -n "\.ActionLabelGenerator_[A-Za-z_]*\$" "$OUT/discovery-full.log"
  grep -n "\.FileActionKnnPredictor_PredictsMajorityOnHomogeneousTraining\$" "$OUT/discovery-full.log"
  grep -n "\.UseFileActionKnn[A-Za-z]*\$" "$OUT/discovery-full.log"
  grep -n "\.TrainFileActionKnnPopulatesTheFacadePredictor\$" "$OUT/discovery-full.log"; } > "$OUT/discovery-selected.txt"
echo "обнаружено выбранных полных имён: $(wc -l < "$OUT/discovery-selected.txt")"
cat "$OUT/discovery-selected.txt"

require_free "перед прогоном одиннадцати существующих" || exit 22
echo "=== $STAGE: одиннадцать существующих проверок, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "$EXIST" \
  --logger "trx;LogFileName=existing-$STAGE.trx" --results-directory "$OUT/tests" > "$OUT/existing-test-run.log" 2>&1
echo "код возврата существующих: $?" > "$OUT/existing-test-exit.txt"; tail -3 "$OUT/existing-test-run.log"

require_free "перед прогоном новой обучающей проверки" || exit 23
echo "=== $STAGE: новая обучающая проверка, свой отдельный процесс, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "$NEW" \
  --logger "trx;LogFileName=learning-$STAGE.trx" --results-directory "$OUT/tests" > "$OUT/new-test-run.log" 2>&1
echo "код возврата новой: $?" > "$OUT/new-test-exit.txt"; tail -4 "$OUT/new-test-run.log"

stamp_libraries "после прогонов: те же байты или нет"
echo "=== $STAGE готов, $(date '+%H:%M:%S') ==="
exit 0
