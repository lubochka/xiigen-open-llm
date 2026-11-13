#!/usr/bin/env python3
"""Copy a fixed, hash-checked native source closure into this planning packet.

This script neither compiles nor trains. --core-root is read-only.
"""
from pathlib import Path
import argparse
import difflib
import hashlib
import json


def git_blob(data):
    return hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()


def modify_linear(text):
    marker = 'public class Linear : Module\n{'
    additions = '''public class Linear : Module
{
    // EXPERIMENT ONLY. Lifetime: one immutable base after initial load; no restore.
    public static bool ExperimentReuseFrozenTranspose { get; set; }
    public static long ExperimentCacheHits { get; private set; }
    public static long ExperimentCacheMisses { get; private set; }
    public static long ExperimentCachedFloats { get; private set; }
    private Tensor? _experimentFrozenTranspose;

    private Tensor GetExperimentFrozenTranspose(CancellationToken cancellationToken)
    {
        if (_experimentFrozenTranspose is not null)
        {
            ExperimentCacheHits++;
            return _experimentFrozenTranspose;
        }
        var value = TensorOps.Transpose(Weight.Value, 0, 1, cancellationToken);
        _experimentFrozenTranspose = value;
        ExperimentCacheMisses++;
        ExperimentCachedFloats += value.Data.LongLength;
        return value;
    }
'''
    original = 'var weightT = TensorOps.Transpose(Weight.Value, 0, 1, cancellationToken);'
    replacement = '''var weightT = ExperimentReuseFrozenTranspose && !Weight.Value.RequiresGrad
            ? GetExperimentFrozenTranspose(cancellationToken)
            : TensorOps.Transpose(Weight.Value, 0, 1, cancellationToken);'''
    if text.count(marker) != 1 or text.count(original) != 1:
        raise SystemExit('PINNED_LINEAR_PATCH_ANCHOR_MISMATCH')
    return text.replace(marker, additions).replace(original, replacement)


def modify_bpe(text):
    marker = 'public class BpeTokenizer\n{'
    additions = '''public class BpeTokenizer
{
    // EXPERIMENT ONLY. A uses the untouched native body below the branch.
    public static bool ExperimentElideUnchangedChunkCopies { get; set; }
    public static long ExperimentNoMatchReuses { get; private set; }
    public static long ExperimentChangedChunkCopies { get; private set; }

    private static List<int> MergeChunkWithoutNoMatchCopy(List<int> ids, (int, int) pair, int newToken)
    {
        List<int>? output = null;
        int i = 0;
        while (i < ids.Count)
        {
            if (i < ids.Count - 1 && ids[i] == pair.Item1 && ids[i + 1] == pair.Item2)
            {
                if (output is null)
                {
                    output = new List<int>(ids.Count);
                    for (int prefix = 0; prefix < i; prefix++) output.Add(ids[prefix]);
                }
                output.Add(newToken);
                i += 2;
            }
            else
            {
                output?.Add(ids[i]);
                i++;
            }
        }
        if (output is null)
        {
            ExperimentNoMatchReuses++;
            return ids;
        }
        ExperimentChangedChunkCopies++;
        return output;
    }
'''
    method = '''    private static List<int> MergeChunk(List<int> ids, (int, int) pair, int newToken)
    {'''
    branch = method + '''
        if (ExperimentElideUnchangedChunkCopies)
            return MergeChunkWithoutNoMatchCopy(ids, pair, newToken);'''
    if text.count(marker) != 1 or text.count(method) != 1:
        raise SystemExit('PINNED_BPE_PATCH_ANCHOR_MISMATCH')
    return text.replace(marker, additions).replace(method, branch)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--core-root', type=Path, required=True)
    args = parser.parse_args()
    packet = Path(__file__).resolve().parent
    meta = json.loads((packet / 'experiment.json').read_text())
    manifest = json.loads((packet / 'source-manifest.json').read_text())
    native_root = args.core_root.resolve(strict=True)
    host = packet / '35.ExperimentsV2/Experiments/Rp01Micro' / meta['id']
    artifact = packet / 'experiments-v2/RP01-MICRO' / meta['id']
    if artifact.exists():
        raise SystemExit('PREPARED_COPY_EXISTS: preserve it; do not overwrite. Use its existing verified manifest and runner.')
    # Validate every native input before writing anything. A changed documentation
    # HEAD does not matter; changed bytes of a used source do.
    captured = []
    for entry in manifest['files']:
        source = (native_root / entry['path']).resolve(strict=True)
        if not source.is_relative_to(native_root):
            raise SystemExit('SOURCE_ESCAPES_READ_ONLY_CORE_ROOT')
        data = source.read_bytes()
        if git_blob(data) != entry['git_blob_sha1']:
            raise SystemExit('USED_SOURCE_BYTES_CHANGED: ' + entry['path'])
        captured.append((entry, data))
    artifact.mkdir(parents=True)
    recorded = []
    diffs = []
    for entry, before in captured:
        after = before
        if meta['kind'] == 'linear-transpose' and entry['path'] == '00.TensorCore/Layers/Linear.cs':
            after = modify_linear(before.decode('utf8')).encode('utf8')
        if meta['kind'] == 'bpe-copy' and entry['path'] == '01.Tokenizer/BpeTokenizer.cs':
            after = modify_bpe(before.decode('utf8')).encode('utf8')
        target = artifact / 'src' / entry['path']
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(after)
        recorded.append({**entry, 'copied_sha256': hashlib.sha256(after).hexdigest(), 'changed': before != after})
        if before != after:
            diffs.extend(difflib.unified_diff(before.decode('utf8').splitlines(True), after.decode('utf8').splitlines(True), fromfile=entry['path'], tofile='experiment/' + entry['path']))
    program = (host / 'Program.cs.template').read_bytes()
    (artifact / 'Program.cs').write_bytes(program)
    (artifact / 'source-copy-manifest.json').write_text(json.dumps({'core_commit':manifest['core_commit'],'source_root_read_only':str(native_root),'program_sha256':hashlib.sha256(program).hexdigest(),'files':recorded},indent=2)+'\n')
    (artifact / 'one-factor.diff').write_text(''.join(diffs))
    print(json.dumps({'status':'PREPARED_NOT_COMPILED_NOT_RUN','id':meta['id'],'native_source_count':len(recorded),'changed_native_files':sum(x['changed'] for x in recorded),'artifact':str(artifact)}))


if __name__ == '__main__':
    main()
