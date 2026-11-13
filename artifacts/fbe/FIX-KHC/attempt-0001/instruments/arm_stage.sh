#!/bin/bash
# Одно плечо измерения: пересборка, обнаружение, прогон конуса, прогон одиночной
# проверки, копирование наблюдений в каталоги своих прогонов.
# Параметр: имя плеча (before | after).
set -o pipefail
cd /c/Projects/llm_mvp_core
ARM="$1"
ROOT="artifacts/fbe/FIX-KHC/attempt-0001/arm-$ARM"
mkdir -p "$ROOT/cone/tests" "$ROOT/alone/tests"
NEW='FullyQualifiedName=LlmFromScratch.Tests.FileActionPreparationLearningTests.SharedParse_PreservesPreparedPairsThroughFacade'
CONE='FullyQualifiedName~LlmFromScratch.Tests.E02E03E05E11IntegrationTests.|FullyQualifiedName~LlmFromScratch.Tests.E09E10FeatureFlagTests.|FullyQualifiedName~LlmFromScratch.Tests.E13E14FeatureFlagTests.|FullyQualifiedName~LlmFromScratch.Tests.E13E14Tests.|FullyQualifiedName~LlmFromScratch.Tests.MethodKnnAdmissionLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnExpressionBodyLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.MethodKnnTargetScopeLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnTieIsResolvedByDistanceTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnTieReachedThroughFacadeTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.NullGuardLabelNarrowingTests.|FullyQualifiedName~LlmFromScratch.Tests.SemanticCompilerTests.|FullyQualifiedName~LlmFromScratch.Tests.StructuralSiblingCacheTests.|FullyQualifiedName~LlmFromScratch.Tests.FileActionPreparationLearningTests.'
DLLS="Tests/bin/Release/net9.0/SemanticCompiler.dll Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll"

busy() { powershell -NoProfile -ExecutionPolicy Bypass -File /c/Users/lubao/AppData/Local/Temp/busy_check.ps1 2>/dev/null | tr -d ' \r\n'; }
require_free() {
  local n; n=$(busy)
  { echo "=== плечо $ARM: занятость, $1, $(date '+%Y-%m-%d %H:%M:%S') ==="
    echo "чужих процессов на общем каталоге: $n"; } >> "$ROOT/occupancy.log"
  if [ "$n" != "0" ]; then echo "ОСТАНОВ: каталог занят ($n) в момент «$1»" | tee -a "$ROOT/occupancy.log"; return 1; fi
  return 0
}
stamp_src() {
  echo "=== плечо $ARM: отпечатки исходников, $(date '+%Y-%m-%d %H:%M:%S') ===" > "$ROOT/source-stamp.txt"
  for src in 12.SemanticCompiler/Prediction/FileActionKnn.cs 12.SemanticCompiler/SemanticCompilerFacade.cs Tests/FileActionPreparationLearningTests.cs Tests/ExperimentFixes/FileActionPreparationFixture.cs Tests/ExperimentFixes/R0201SummaryRawProvenanceTests.cs tools/test-batches/summarize-test-batches.ps1; do
    if [ -f "$src" ]; then
      { echo "объект: файл $(pwd)/$src"
        echo "  размер в байтах: $(stat -c %s "$src")"
        echo "  git hash-object: $(git hash-object "$src")"
        echo "  sha256sum: $(sha256sum "$src" | cut -d' ' -f1)"; } >> "$ROOT/source-stamp.txt"
    else
      echo "объект: файл $(pwd)/$src НЕ НАЙДЕН" >> "$ROOT/source-stamp.txt"
    fi
  done
}
stamp_lib() {
  echo "=== плечо $ARM: отпечатки библиотек, $1, $(date '+%Y-%m-%d %H:%M:%S') ===" >> "$ROOT/assembly-stamp.txt"
  for dll in $DLLS; do
    { echo "объект: файл $(pwd)/$dll"
      echo "  размер в байтах: $(stat -c %s "$dll")"
      echo "  git hash-object: $(git hash-object "$dll")"
      echo "  sha256sum: $(sha256sum "$dll" | cut -d' ' -f1)"
      echo "  время записи: $(stat -c %y "$dll")"; } >> "$ROOT/assembly-stamp.txt"
  done
}
copy_obs() {
  for obs in prepared-pairs-observation.md before-compile-outcomes.md; do
    [ -f "artifacts/fbe/FIX-KHC/attempt-0001/$obs" ] && cp "artifacts/fbe/FIX-KHC/attempt-0001/$obs" "$1/$obs"
  done
}

: > "$ROOT/occupancy.log"; : > "$ROOT/assembly-stamp.txt"
stamp_src
require_free "перед сборкой" || exit 20
echo "=== плечо $ARM: пересборка, $(date '+%H:%M:%S') ==="
dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild /nodeReuse:false > "$ROOT/build.log" 2>&1
BE=$?; echo "код возврата сборки: $BE" > "$ROOT/build-exit.txt"; cat "$ROOT/build-exit.txt"
require_free "сразу после сборки" || echo "предупреждение: чужой процесс во время сборки" >> "$ROOT/occupancy.log"
if [ "$BE" != "0" ]; then echo "СБОРКА УПАЛА"; grep -E "error " "$ROOT/build.log" | head -8; exit 10; fi
stamp_lib "после сборки, до прогонов"

require_free "перед обнаружением" || exit 21
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests > "$ROOT/discovery-full.log" 2>&1
echo "код возврата обнаружения: $?" > "$ROOT/discovery-exit.txt"
grep -E "LlmFromScratch\.Tests\.(E02E03E05E11IntegrationTests|E09E10FeatureFlagTests|E13E14FeatureFlagTests|E13E14Tests|MethodKnnAdmissionLearningTests|MethodKnnTargetScopeLearningTests|SemanticCompilerTests|StructuralSiblingCacheTests|FileActionPreparationLearningTests|ExperimentFixes\.MethodKnn[A-Za-z]*Tests|ExperimentFixes\.NullGuardLabelNarrowingTests)\." "$ROOT/discovery-full.log" | sed 's/^ *//' | sort > "$ROOT/discovery-cone.txt"
echo "обнаружено имён конуса: $(wc -l < "$ROOT/discovery-cone.txt")"

require_free "перед прогоном конуса" || exit 22
echo "=== плечо $ARM: прогон конуса, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "$CONE" \
  --logger "trx;LogFileName=cone-$ARM.trx" --results-directory "$ROOT/cone/tests" > "$ROOT/cone/run.log" 2>&1
echo "код возврата прогона конуса: $?" > "$ROOT/cone/exit.txt"; tail -3 "$ROOT/cone/run.log"
copy_obs "$ROOT/cone"

require_free "перед одиночным прогоном" || exit 23
echo "=== плечо $ARM: одиночный прогон новой проверки, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "$NEW" \
  --logger "trx;LogFileName=alone-$ARM.trx" --results-directory "$ROOT/alone/tests" > "$ROOT/alone/run.log" 2>&1
echo "код возврата одиночного прогона: $?" > "$ROOT/alone/exit.txt"; tail -3 "$ROOT/alone/run.log"
copy_obs "$ROOT/alone"

stamp_lib "после прогонов"
echo "=== плечо $ARM готово, $(date '+%H:%M:%S') ==="
exit 0
