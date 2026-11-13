// N37-TAIL-01: conditional source-logic proxy for 09.Inference/Sampler.cs.
// DOES NOT run .NET TensorOps.Softmax, native Student, or domain-quality evaluation.
// Assumption: after topK=61 of 62, the accepted 61 logits are tied and
// the 62nd masked logit has probability 0. Float32 softmax = 1/61 each.
const n = 62;
const accepted = 61;
const probs = new Float32Array(n);
for (let i = 0; i < accepted; i++) probs[i] = Math.fround(1 / accepted);
let mass = 0;
for (let i = 0; i < n; i++) mass += probs[i]; // C# 'double acc += float probability'
const r = 0.99999999;                        // injected test-only NextDouble
let acc = 0;
let selected = n - 1;                        // source fallback
for (let i = 0; i < n; i++) {
  acc += probs[i];
  if (r <= acc) { selected = i; break; }
}
const result = {
  probabilityValue: probs[0],
  mass,
  r,
  selected,
  selectedProbability: probs[selected],
  maskedIndex: n - 1,
  sourceArithmetic: "Float32Array proxy, NOT native C#",
  tailChance: 1 - mass
};
if (!(mass < r && selected === 61 && probs[selected] === 0))
  throw new Error("SOURCE_LOGIC_PROXY_CONTRADICTION");
console.log(JSON.stringify(result, null, 2));
