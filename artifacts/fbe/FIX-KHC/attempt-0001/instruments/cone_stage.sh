#!/bin/bash
# Ограниченная регрессия по конусу зависимостей правки FIX-KHC.
# Параметры: $1 — имя подкаталога улик; $2 — rebuild|nobuild.
set -o pipefail
cd /c/Projects/llm_mvp_core
STAGE="$1"; MODE="${2:-nobuild}"
OUT="artifacts/fbe/FIX-KHC/attempt-0001/$STAGE"
mkdir -p "$OUT/tests"
CONE='FullyQualifiedName~LlmFromScratch.Tests.E02E03E05E11IntegrationTests.|FullyQualifiedName~LlmFromScratch.Tests.E09E10FeatureFlagTests.|FullyQualifiedName~LlmFromScratch.Tests.E13E14FeatureFlagTests.|FullyQualifiedName~LlmFromScratch.Tests.E13E14Tests.|FullyQualifiedName~LlmFromScratch.Tests.MethodKnnAdmissionLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnExpressionBodyLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.MethodKnnTargetScopeLearningTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnTieIsResolvedByDistanceTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.MethodKnnTieReachedThroughFacadeTests.|FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.NullGuardLabelNarrowingTests.|FullyQualifiedName~LlmFromScratch.Tests.SemanticCompilerTests.|FullyQualifiedName~LlmFromScratch.Tests.StructuralSiblingCacheTests.|FullyQualifiedName~LlmFromScratch.Tests.FileActionPreparationLearningTests.'
DLLS="Tests/bin/Release/net9.0/SemanticCompiler.dll Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll"

busy() { powershell -NoProfile -ExecutionPolicy Bypass -File /c/Users/lubao/AppData/Local/Temp/busy_check.ps1 2>/dev/null | tr -d ' \r\n'; }
require_free() {
  local n; n=$(busy)
  { echo "=== $STAGE: занятость, $1, $(date '+%Y-%m-%d %H:%M:%S') ==="
    echo "чужих процессов на общем каталоге: $n"; } >> "$OUT/occupancy.log"
  if [ "$n" != "0" ]; then echo "ОСТАНОВ: каталог занят ($n) в момент «$1»" | tee -a "$OUT/occupancy.log"; return 1; fi
  return 0
}
stamp() {
  echo "=== $STAGE: отпечатки библиотек, $1, $(date '+%Y-%m-%d %H:%M:%S') ===" >> "$OUT/assembly-stamp.txt"
  for dll in $DLLS; do
    { echo "объект: файл $(pwd)/$dll"
      echo "  размер в байтах: $(stat -c %s "$dll")"
      echo "  git hash-object: $(git hash-object "$dll")"
      echo "  sha256sum: $(sha256sum "$dll" | cut -d' ' -f1)"
      echo "  время записи: $(stat -c %y "$dll")"; } >> "$OUT/assembly-stamp.txt"
  done
}

: > "$OUT/occupancy.log"; : > "$OUT/assembly-stamp.txt"
echo "=== $STAGE: отпечатки исходников ===" > "$OUT/source-stamp.txt"
# Отпечатки моих файлов И файлов второго исполнителя: если его файлы совпадут
# при обеих моих сборках, единственной разницей между сторонами остаётся мой
# перенос, и это доказано отпечатками, а не предположением.
for src in 12.SemanticCompiler/SemanticCompilerFacade.cs 12.SemanticCompiler/Prediction/FileActionKnn.cs Tests/FileActionPreparationLearningTests.cs Tests/ExperimentFixes/FileActionPreparationFixture.cs Tests/ExperimentFixes/R0201SummaryRawProvenanceTests.cs tools/test-batches/summarize-test-batches.ps1; do
  if [ -f "$src" ]; then
    { echo "объект: файл $(pwd)/$src"
      echo "  размер в байтах: $(stat -c %s "$src")"
      echo "  git hash-object: $(git hash-object "$src")"
      echo "  sha256sum: $(sha256sum "$src" | cut -d' ' -f1)"; } >> "$OUT/source-stamp.txt"
  else
    echo "объект: файл $(pwd)/$src НЕ НАЙДЕН на момент этапа" >> "$OUT/source-stamp.txt"
  fi
done

require_free "перед этапом" || exit 20
if [ "$MODE" = "rebuild" ]; then
  echo "=== $STAGE: пересборка, $(date '+%H:%M:%S') ==="
  dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild /nodeReuse:false > "$OUT/build.log" 2>&1
  BE=$?
  echo "код возврата сборки: $BE" > "$OUT/build-exit.txt"; cat "$OUT/build-exit.txt"
  require_free "сразу после сборки" || echo "предупреждение: чужой процесс во время сборки" >> "$OUT/occupancy.log"
  if [ "$BE" != "0" ]; then echo "СБОРКА УПАЛА"; grep -E "error " "$OUT/build.log" | head -8; exit 10; fi
else
  echo "код возврата сборки: пересборка не требовалась, библиотека от предыдущего этапа" > "$OUT/build-exit.txt"
fi
stamp "до прогона конуса"

require_free "перед обнаружением" || exit 21
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests > "$OUT/discovery-full.log" 2>&1
echo "код возврата обнаружения: $?" > "$OUT/discovery-exit.txt"
grep -E "LlmFromScratch\.Tests\.(E02E03E05E11IntegrationTests|E09E10FeatureFlagTests|E13E14FeatureFlagTests|E13E14Tests|MethodKnnAdmissionLearningTests|MethodKnnTargetScopeLearningTests|SemanticCompilerTests|StructuralSiblingCacheTests|FileActionPreparationLearningTests|ExperimentFixes\.MethodKnn[A-Za-z]*Tests|ExperimentFixes\.NullGuardLabelNarrowingTests)\." "$OUT/discovery-full.log" | sed 's/^ *//' | sort > "$OUT/discovery-cone.txt"
echo "обнаружено имён конуса: $(wc -l < "$OUT/discovery-cone.txt")"

require_free "перед прогоном конуса" || exit 22
echo "=== $STAGE: прогон конуса, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "$CONE" \
  --logger "trx;LogFileName=cone-$STAGE.trx" --results-directory "$OUT/tests" > "$OUT/cone-run.log" 2>&1
echo "код возврата прогона конуса: $?" > "$OUT/cone-exit.txt"
tail -3 "$OUT/cone-run.log"
stamp "после прогона конуса"
# Наблюдение проверки пишется по постоянному пути, поэтому следующий прогон
# затёр бы предыдущий: копируем его в каталог ЭТОГО прогона сразу после него.
for obs in prepared-pairs-observation.md before-compile-outcomes.md; do
  if [ -f "artifacts/fbe/FIX-KHC/attempt-0001/$obs" ]; then
    cp "artifacts/fbe/FIX-KHC/attempt-0001/$obs" "$OUT/$obs"
    echo "наблюдение скопировано в улику этапа: $OUT/$obs"
  fi
done
echo "=== $STAGE готов, $(date '+%H:%M:%S') ==="
exit 0
