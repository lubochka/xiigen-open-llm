#!/usr/bin/env node
'use strict';
// Conditional KNN alias/top-k mechanism proxy, not native .NET learning.
function runN34Proxy() {
  const full = "SVO|ADD_OP|DATA_ENTITY|RU:TENSE:PAST";
  const prefix = "SVO|ADD_OP|DATA_ENTITY|RU:TENSE:PAST";
  const structure = "SVO|VERB|NOUN|RU:TENSE:PAST";
  const lessons = [
    { id: "old_A", source: "добавь файл A", target: "добавь A" },
    { id: "old_B", source: "добавь файл B", target: "добавь B" },
    { id: "old_C", source: "добавь файл C", target: "добавь C" },
    { id: "recent_D", source: "добавь файл D", target: "add file D" }
  ];
  function build(deduplicateWithinLesson) {
    const buckets = new Map();
    for (const lesson of lessons) for (const signature of [full, prefix, structure]) {
      if (!buckets.has(signature)) buckets.set(signature, []);
      const bucket = buckets.get(signature);
      if (!deduplicateWithinLesson || !bucket.some(entry => entry.id === lesson.id))
        bucket.push(lesson);
    }
    return buckets;
  }
  function inspect(buckets) {
    const exact = buckets.get(full);
    const nearest = exact.slice(0, 5);
    const firstCompatible = nearest.find(entry => !/[а-яё]/iu.test(entry.target));
    return { bucketCount: exact.length, top5: nearest.map(entry => entry.id), selectedCompatible: firstCompatible?.id ?? null };
  }
  return {
    study: "N34 full/prefix aliases cause duplicate insertions before top-k",
    status: "SOURCE_LOGIC_PROXY_ONLY__NOT_NATIVE_DOTNET__NOT_REAL_LEARNING_QUALITY",
    condition: "example.SemanticSignature == sourceTokens.PrefixSignature, same lesson inserted twice in same bucket; first 3 targets assumed incompatible, latest assumed compatible; k=5",
    baseline: inspect(build(false)),
    deduplicatedWithinLesson: inspect(build(true)),
    caveat: "The proxy intentionally does not implement native tokenizer admission, TranslationMemorySourceLaw, phrase/word/exact preemption, restore, or final TranslationMachine."
  };
}
process.stdout.write(JSON.stringify(runN34Proxy(), null, 2) + '\n');
