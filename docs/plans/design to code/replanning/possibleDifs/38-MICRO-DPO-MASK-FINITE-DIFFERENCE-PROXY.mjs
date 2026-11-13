// N38 source-only mathematical shadow, NOT a C# or Student training result.
// Run: node 38-MICRO-DPO-MASK-FINITE-DIFFERENCE-PROXY.mjs
// Mirrors DpoTrainer.ComputeTargetLogProbs and BackwardLogProbs mask coefficients.
import assert from "node:assert/strict";

const sigmoid = x => 1 / (1 + Math.exp(-x));
const logSigmoid = x => -Math.log(1 + Math.exp(-x));
function score(z, mask) {
  const sumMask = mask.reduce((a, b) => a + b, 0);
  return z.reduce((s, value, i) => s + mask[i] * logSigmoid(value), 0) / sumMask;
}
function probe(z, mask) {
  const eps = 0.00001;
  const sumMask = mask.reduce((a, b) => a + b, 0);
  const active = mask.filter(w => w > 0).length;
  const fd = z.map((value, i) => {
    const plus = [...z], minus = [...z];
    plus[i] = value + eps;
    minus[i] = value - eps;
    return (score(plus, mask) - score(minus, mask)) / (2 * eps);
  });
  const correct = z.map((v, i) => mask[i] / sumMask * (1 - sigmoid(v)));
  const code = z.map((v, i) => mask[i] > 0 ? (1 / active) * (1 - sigmoid(v)) : 0);
  fd.forEach((v, i) => assert.ok(Math.abs(v - correct[i]) < 1e-8));
  return {
    logits: z, mask, score: score(z, mask),
    finiteDifference: fd, expectedAnalyticGradient: correct,
    currentLegacyBackwardGradient: code,
    codeToAnalyticRatio: code.map((v, i) => correct[i] === 0 ? null : v / correct[i]),
    gradientMatches: code.every((v, i) => Math.abs(v - correct[i]) < 1e-8)
  };
}
const fractional = probe([-1, 1], [0.25, 1]);
const binaryControl = probe([-1, 1], [1, 1]);
assert.equal(fractional.gradientMatches, false);
assert.equal(binaryControl.gradientMatches, true);
console.log(JSON.stringify({
  kind: "SOURCE_ONLY_NUMERICAL_PROXY",
  nativeDotnetRun: false, rp01QualityMeasured: false,
  source: "DpoTrainer.cs:1036-1119",
  fractional, binaryControl
}, null, 2));
