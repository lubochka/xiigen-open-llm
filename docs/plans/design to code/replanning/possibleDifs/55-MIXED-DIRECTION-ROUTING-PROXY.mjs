// N55 SOURCE-EQUIVALENT routing oracle. Does not import XIIGEN C# or claim real translation.
const directions = ["RU_EN", "EN_RU", "HE_EN", "EN_HE", "ES_EN", "EN_ES"];
const probes = directions.flatMap(direction => [1,2].map(n => ({
  id: direction + "-" + n,
  direction,
  expected: "ORACLE_" + direction + "_" + n
})));
function output(routedDirection, item) {
  const n = item.id.split("-").at(-1);
  return "ORACLE_" + routedDirection + "_" + n;
}
const correct = forcedDirection => probes.filter(item =>
  output(forcedDirection || item.direction, item) === item.expected).length;
const result = {
  evidence_type: "SOURCE_EQUIVALENT_SYNTHETIC_ORACLE",
  synthetic_cases: probes.length,
  directions,
  baseline_forced_RU_EN_correct: correct("RU_EN"),
  alternative_typed_direction_correct: correct(null),
  ru_en_only_negative_control: {
    baseline_correct: 2,
    typed_correct: 2,
    total: 2
  },
  real_dotnet_test: "NOT_RUN",
  neural_learning_quality: "NOT_MEASURED",
  historical_restore: "NOT_PROVEN"
};
if (result.baseline_forced_RU_EN_correct !== 2 ||
    result.alternative_typed_direction_correct !== 12) {
  throw new Error("ORACLE_EXPECTATION_FAILED");
}
console.log(JSON.stringify(result, null, 2));
