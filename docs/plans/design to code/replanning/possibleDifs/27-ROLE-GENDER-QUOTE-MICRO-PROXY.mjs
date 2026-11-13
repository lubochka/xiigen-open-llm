// XIIGEN MICRO-N27. Source-equivalent *formula* proxy; not a .NET Unit/Flow run.
// Counterexamples are synthetic DEV_ONLY, not admitted DPO examples.
import assert from 'node:assert/strict';

function genderAlternate(normalized) {
  if (!normalized || normalized.length < 3) return null;
  if (![...normalized].some(ch => ch.charCodeAt(0) > 'z'.charCodeAt(0))) return null;
  if (normalized.endsWith('ла') && normalized.length > 3) return normalized.slice(0, -1);
  if (normalized.endsWith('л') && !normalized.endsWith('ла') && normalized.length > 2) return normalized + 'а';
  return null;
}
function teach(map, word, target, role, posGate) {
  const key = word.toLowerCase().trim();
  map.set(key, target);
  if (!posGate || role === 'PAST_VERB') {
    const alternate = genderAlternate(key);
    if (alternate) map.set(alternate, target);
  }
}
function targetMarker(prefix) {
  const parts = prefix.match(/[\p{L}\p{N}_$']+/gu) || [];
  return parts.length ? parts.at(-1) : prefix.trim();
}
function rejectedText(chosen, marker) {
  if (!marker.trim()) return chosen;
  const escaped = marker.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
  const result = chosen.replace(new RegExp('\\b' + escaped + '\\b\\s*', 'gi'), '')
    .replace(/\s{2,}/g, ' ').trim();
  return result || chosen;
}
const current = new Map();
teach(current, 'пил', 'drank', 'PAST_VERB', false);
const previousCorrectVerb = current.get('пил');
teach(current, 'пила', 'saw', 'NOUN', false);
const afterNoun = current.get('пил');
const guarded = new Map();
teach(guarded, 'пил', 'drank', 'PAST_VERB', true);
teach(guarded, 'пила', 'saw', 'NOUN', true);
const afterNounGated = guarded.get('пил');
const roleBlind = new Map();
roleBlind.set('пила|UNKNOWN', 'saw');
roleBlind.set('пила|UNKNOWN', 'drank');
const roleAware = new Map();
roleAware.set('пила|NOUN', 'saw');
roleAware.set('пила|PAST_VERB', 'drank');
const misaligned = targetMarker('She bought a');
const chosenTie = 'She bought a saw';
const textTie = rejectedText(chosenTie, targetMarker('unseen marker'));
const chosenContrast = 'She drank water';
const rejectedContrast = rejectedText(chosenContrast, targetMarker('She drank'));
assert.equal(previousCorrectVerb, 'drank');
assert.equal(afterNoun, 'saw');
assert.equal(afterNounGated, 'drank');
assert.equal(roleBlind.size, 1);
assert.equal(roleAware.size, 2);
assert.equal(misaligned, 'a');
assert.equal(textTie, chosenTie);
assert.notEqual(rejectedContrast, chosenContrast);
const report = {
  evidenceKind: 'JS_SOURCE_EQUIVALENT_FORMULA_PROXY_NOT_NATIVE_DOTNET_NOT_MEASURED_QUALITY',
  source: 'TranslationFrequencyQuoteMap.cs, TranslationTrainedUnits.cs',
  gender: { verbBefore: previousCorrectVerb, afterNounCurrent: afterNoun, afterNounPOSGate: afterNounGated },
  roleMemory: { unscopedKeys: roleBlind.size, roleScopedKeys: roleAware.size },
  marker: { sourceWord: 'пила', humanApprovedGold: 'saw', targetPrefix: 'She bought a', extractedMarker: misaligned },
  negative: { chosenTie, rejectedTie: textTie, sameText: textTie === chosenTie, chosenContrast, rejectedContrast },
  assertionsPassed: 8,
  dotnetExecuted: false,
  ordinaryConsumerQualityMeasured: false
};
console.log(JSON.stringify(report, null, 2));
