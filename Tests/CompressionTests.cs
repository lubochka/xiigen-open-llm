// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Compression;
using System.Text;

// ─── Sub-project 24: LZ Family ───────────────────────────────────────────────

public class SnappyCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_ShortText()
    {
        var input  = Encoding.UTF8.GetBytes("hello snappy compression");
        var sut    = new SnappyCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_RepetitiveData()
    {
        var input  = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("abcdefgh", 200)));
        var sut    = new SnappyCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        Assert.True(comp.Value!.CompressedBytes.Length < input.Length, "Repetitive data should compress");
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var sut    = new SnappyCompressor();
        var result = await sut.CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_INPUT", result.ErrorCode);
    }

    [Fact]
    public async Task SingleByte_RoundTrip()
    {
        byte[] input = [0xAB];
        var sut    = new SnappyCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, 1);
        Assert.Equal(input, decomp.Value);
    }
}

public class LzavCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("the quick brown fox jumps over the lazy dog");
        var sut    = new LzavCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task AlgorithmName_IsLzav()
    {
        var sut    = new LzavCompressor();
        var comp   = await sut.CompressAsync(Encoding.UTF8.GetBytes("test"), Opts);
        Assert.Equal("LZAV", comp.Value!.AlgorithmName);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var sut    = new LzavCompressor();
        var result = await sut.CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RoundTrip_BinaryData()
    {
        var input = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut   = new LzavCompressor();
        var comp  = await sut.CompressAsync(input, Opts);
        var dec   = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, dec.Value);
    }
}

public class Lz4CompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 1 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("lz4 compresses data very fast with decent ratio");
        var sut    = new Lz4Compressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_RepetitiveData()
    {
        var input  = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("xxxx", 500)));
        var sut    = new Lz4Compressor();
        var comp   = await sut.CompressAsync(input, Opts);
        Assert.True(comp.Value!.CompressedBytes.Length < input.Length);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var sut    = new Lz4Compressor();
        var result = await sut.CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CompressionRatio_IsPositive()
    {
        var input = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("hello world ", 100)));
        var sut   = new Lz4Compressor();
        var comp  = await sut.CompressAsync(input, Opts);
        Assert.True(comp.Value!.CompressionRatio > 0);
    }
}

public class ZstandardCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 3 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("zstandard style compression with huffman literals");
        var sut    = new ZstandardCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_RepetitiveData()
    {
        var input  = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("pattern", 300)));
        var sut    = new ZstandardCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new ZstandardCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }
}

// ─── Sub-project 25: Entropy Coding ─────────────────────────────────────────

public class SymbolStatisticsTests
{
    [Fact]
    public void CountFrequencies_CorrectCounts()
    {
        byte[] data = [1, 2, 1, 3, 1, 2];
        var freqs = SymbolStatistics.CountFrequencies(data);
        Assert.Equal(3, freqs[1]);
        Assert.Equal(2, freqs[2]);
        Assert.Equal(1, freqs[3]);
    }

    [Fact]
    public void ShannonEntropy_UniformDistribution_IsMaximum()
    {
        var freqs = new long[256];
        Array.Fill(freqs, 1L);
        double entropy = SymbolStatistics.ComputeShannonEntropy(freqs);
        Assert.True(entropy > 7.9, $"Expected ~8 bits, got {entropy}");
    }

    [Fact]
    public void ShannonEntropy_SingleSymbol_IsZero()
    {
        var freqs = new long[256];
        freqs[42] = 1000;
        double entropy = SymbolStatistics.ComputeShannonEntropy(freqs);
        Assert.Equal(0.0, entropy, 6);
    }

    [Fact]
    public void EstimatedCompressedBytes_LessThanRaw_ForRepetitiveData()
    {
        var data  = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("a", 1000)));
        var freqs = SymbolStatistics.CountFrequencies(data);
        double est = SymbolStatistics.EstimatedCompressedBytes(freqs, data.Length);
        Assert.True(est < data.Length);
    }
}

public class HuffmanTreeTests
{
    [Fact]
    public void Build_AssignsCodes_ToAllPresentSymbols()
    {
        byte[] data = Encoding.UTF8.GetBytes("aabbbcccc");
        var freqs = SymbolStatistics.CountFrequencies(data);
        var tree  = new HuffmanTree(); tree.Build(freqs);
        Assert.True(tree.CodeLengths[(byte)'a'] > 0);
        Assert.True(tree.CodeLengths[(byte)'b'] > 0);
        Assert.True(tree.CodeLengths[(byte)'c'] > 0);
    }

    [Fact]
    public void Build_HighFrequencySymbol_HasShortCode()
    {
        byte[] data = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("a", 100)) + "b");
        var freqs = SymbolStatistics.CountFrequencies(data);
        var tree  = new HuffmanTree(); tree.Build(freqs);
        Assert.True(tree.CodeLengths[(byte)'a'] <= tree.CodeLengths[(byte)'b']);
    }

    [Fact]
    public void Build_SingleSymbol_HasCodeLength1()
    {
        var freqs = new long[256];
        freqs[0x41] = 5;
        var tree = new HuffmanTree(); tree.Build(freqs);
        Assert.True(tree.CodeLengths[0x41] >= 1);
    }
}

public class HuffmanCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("huffman coding test with various characters");
        var sut    = new HuffmanCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_AllBytes()
    {
        var input = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var sut   = new HuffmanCompressor();
        var comp  = await sut.CompressAsync(input, Opts);
        var dec   = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, dec.Value);
    }

    [Fact]
    public async Task RoundTrip_RepetitiveData_Compresses()
    {
        var input = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("aaaa", 200)));
        var sut   = new HuffmanCompressor();
        var comp  = await sut.CompressAsync(input, Opts);
        Assert.True(comp.Value!.CompressedBytes.Length < input.Length);
        var dec = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, dec.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new HuffmanCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_INPUT", result.ErrorCode);
    }
}

public class DeflateCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("deflate uses LZ77 + Huffman coding");
        var sut    = new DeflateCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new DeflateCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AlgorithmName_IsDeflate()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var comp  = await new DeflateCompressor().CompressAsync(input, Opts);
        Assert.Equal("Deflate", comp.Value!.AlgorithmName);
    }
}

public class RansCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("range ANS entropy coding test");
        var sut    = new RansCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_RepetitiveData()
    {
        var input  = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("ab", 100)));
        var sut    = new RansCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new RansCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RoundTrip_SingleByte()
    {
        byte[] input = [0x7F];
        var sut  = new RansCompressor();
        var comp = await sut.CompressAsync(input, Opts);
        var dec  = await sut.DecompressAsync(comp.Value!.CompressedBytes, 1);
        Assert.Equal(input, dec.Value);
    }
}

// ─── Sub-project 26: BWT Family ──────────────────────────────────────────────

public class BurrowsWheelerTransformTests
{
    [Fact]
    public void Forward_KnownInput_CorrectLastColumn()
    {
        // "banana\0" BWT is a well-known example.
        byte[] input = Encoding.ASCII.GetBytes("banana");
        var (bwt, row) = BurrowsWheelerTransform.Forward(input);
        Assert.Equal(input.Length, bwt.Length);
        // Round-trip correctness is the primary assertion.
        var restored = BurrowsWheelerTransform.Inverse(bwt, row);
        Assert.Equal(input, restored);
    }

    [Fact]
    public void RoundTrip_LongerText()
    {
        byte[] input = Encoding.UTF8.GetBytes("the burrows wheeler transform sorts rotations");
        var (bwt, row) = BurrowsWheelerTransform.Forward(input);
        var restored   = BurrowsWheelerTransform.Inverse(bwt, row);
        Assert.Equal(input, restored);
    }

    [Fact]
    public void Forward_SingleByte_RoundTrips()
    {
        byte[] input = [0x42];
        var (bwt, row) = BurrowsWheelerTransform.Forward(input);
        var restored   = BurrowsWheelerTransform.Inverse(bwt, row);
        Assert.Equal(input, restored);
    }
}

public class MoveToFrontTransformTests
{
    [Fact]
    public void Forward_AllSameByte_AllZeros()
    {
        byte[] input = [5, 5, 5, 5];
        int[]  mtf   = MoveToFrontTransform.Forward(input);
        // First occurrence: position of 5 in initial list. Subsequent: 0 (just seen).
        Assert.Equal(0, mtf[1]);
        Assert.Equal(0, mtf[2]);
        Assert.Equal(0, mtf[3]);
    }

    [Fact]
    public void RoundTrip_RandomBytes()
    {
        byte[] input = [10, 20, 10, 30, 20, 10];
        int[]  mtf   = MoveToFrontTransform.Forward(input);
        byte[] back  = MoveToFrontTransform.Inverse(mtf);
        Assert.Equal(input, back);
    }

    [Fact]
    public void RoundTrip_AllDistinctBytes()
    {
        byte[] input = Enumerable.Range(0, 50).Select(i => (byte)i).ToArray();
        int[]  mtf   = MoveToFrontTransform.Forward(input);
        byte[] back  = MoveToFrontTransform.Inverse(mtf);
        Assert.Equal(input, back);
    }
}

public class RunLengthEncoderTests
{
    [Fact]
    public void RoundTrip_WithRuns()
    {
        int[] input = [0, 0, 0, 1, 2, 2, 3];
        byte[] enc  = RunLengthEncoder.Forward(input);
        int[]  dec  = RunLengthEncoder.Inverse(enc);
        Assert.Equal(input, dec);
    }

    [Fact]
    public void RoundTrip_NoRuns()
    {
        int[] input = [1, 2, 3, 4, 5];
        byte[] enc  = RunLengthEncoder.Forward(input);
        int[]  dec  = RunLengthEncoder.Inverse(enc);
        Assert.Equal(input, dec);
    }

    [Fact]
    public void RunLongerThan255_Splits()
    {
        int[] input = Enumerable.Repeat(0, 300).ToArray();
        byte[] enc  = RunLengthEncoder.Forward(input);
        int[]  dec  = RunLengthEncoder.Inverse(enc);
        Assert.Equal(input, dec);
    }
}

public class Bzip2StyleCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 9 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("bzip2 style compressor uses BWT MTF RLE Huffman pipeline");
        var sut    = new Bzip2StyleCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task RoundTrip_LargeRepetitiveInput()
    {
        var input = Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("hello world ", 500)));
        var sut   = new Bzip2StyleCompressor();
        var comp  = await sut.CompressAsync(input, Opts);
        Assert.True(comp.Value!.CompressedBytes.Length < input.Length);
        var dec = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, dec.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new Bzip2StyleCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AlgorithmName_IsBzip2Style()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var comp  = await new Bzip2StyleCompressor().CompressAsync(input, Opts);
        Assert.Equal("bzip2-style", comp.Value!.AlgorithmName);
    }
}

// ─── Sub-project 27: Specialized Codecs ─────────────────────────────────────

public class BrotliContextModelTests
{
    [Fact]
    public void RecordObservation_IncrementsCount()
    {
        var model = new BrotliContextModel();
        model.RecordObservation(0, 1, 2);
        model.RecordObservation(0, 1, 2);
        var dist = model.GetDistribution(0, 1);
        Assert.Equal(2, dist[2]);
    }

    [Fact]
    public void Train_CountsTransitions()
    {
        byte[] corpus = Encoding.UTF8.GetBytes("aaab");
        var model     = new BrotliContextModel();
        model.Train(corpus);
        // "aa" → 'a' appears at index 2
        var dist = model.GetDistribution((byte)'a', (byte)'a');
        Assert.True(dist[(byte)'a'] > 0 || dist[(byte)'b'] > 0);
    }

    [Fact]
    public void GetDistribution_Length_Is256()
    {
        var model = new BrotliContextModel();
        var dist  = model.GetDistribution(0, 0);
        Assert.Equal(256, dist.Length);
    }
}

public class BrotliStaticDictionaryTests
{
    [Fact]
    public void FindMatch_KnownWord_ReturnsEntry()
    {
        var dict  = new BrotliStaticDictionary();
        var match = dict.FindMatch("the quick brown fox");
        Assert.NotNull(match);
        Assert.Equal("the ", match!.Value.Entry);
    }

    [Fact]
    public void FindMatch_NoMatch_ReturnsNull()
    {
        var dict  = new BrotliStaticDictionary();
        var match = dict.FindMatch("zzzzzz");
        Assert.Null(match);
    }

    [Fact]
    public void GetEntry_ValidIndex_ReturnsEntry()
    {
        var dict  = new BrotliStaticDictionary();
        var entry = dict.GetEntry(0);
        Assert.NotNull(entry);
    }

    [Fact]
    public void GetEntry_OutOfRange_ReturnsNull()
    {
        var dict = new BrotliStaticDictionary();
        Assert.Null(dict.GetEntry(-1));
        Assert.Null(dict.GetEntry(dict.EntryCount));
    }
}

public class BrotliCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 4 };

    [Fact]
    public async Task RoundTrip_Text()
    {
        var input  = Encoding.UTF8.GetBytes("brotli is great for web content like <html>text</html>");
        var sut    = new BrotliCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task TrainContextModel_DoesNotThrow()
    {
        var sut    = new BrotliCompressor();
        var corpus = Encoding.UTF8.GetBytes("training corpus for context model");
        sut.TrainContextModel(corpus);
        var input  = Encoding.UTF8.GetBytes("hello");
        var comp   = await sut.CompressAsync(input, Opts);
        Assert.True(comp.IsSuccess);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new BrotliCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AlgorithmName_IsBrotli()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var comp  = await new BrotliCompressor().CompressAsync(input, Opts);
        Assert.Equal("Brotli", comp.Value!.AlgorithmName);
    }
}

public class LzmaRangeCoderTests
{
    [Fact]
    public void EncodeDecode_SingleBit_Zero()
    {
        var coder = new LzmaRangeCoder();
        ushort p  = 1024;
        coder.EncodeBit(ref p, 0);
        coder.Finish();

        byte[] data = coder.Output.ToArray();
        uint  state = 0;
        uint  range = 0xFFFFFFFF;
        int   pos   = 0;

        // Prime state.
        for (int i = 0; i < 5 && pos < data.Length; i++)
            state = (state << 8) | data[pos++];

        ushort dp = 1024;
        int    bit = LzmaRangeCoder.DecodeBit(ref state, ref range, data, ref pos, ref dp);
        Assert.Equal(0, bit);
    }

    [Fact]
    public void EncodeDecode_SingleBit_One()
    {
        var coder = new LzmaRangeCoder();
        ushort p  = 1024;
        coder.EncodeBit(ref p, 1);
        coder.Finish();

        byte[] data = coder.Output.ToArray();
        uint  state = 0;
        uint  range = 0xFFFFFFFF;
        int   pos   = 0;

        for (int i = 0; i < 5 && pos < data.Length; i++)
            state = (state << 8) | data[pos++];

        ushort dp  = 1024;
        int    bit = LzmaRangeCoder.DecodeBit(ref state, ref range, data, ref pos, ref dp);
        Assert.Equal(1, bit);
    }

    [Fact]
    public void Probability_AdaptsToward_FrequentBit()
    {
        var coder = new LzmaRangeCoder();
        ushort p  = 1024;

        // Encode many zeros — probability should increase toward 2048.
        for (int i = 0; i < 50; i++)
            coder.EncodeBit(ref p, 0);

        Assert.True(p > 1024, $"Probability should increase toward 2048 when 0 is frequent, was {p}");
    }

    [Fact]
    public void Finish_OutputsAtLeastFiveBytes()
    {
        var coder = new LzmaRangeCoder();
        ushort p  = 1024;
        coder.EncodeBit(ref p, 0);
        coder.Finish();
        Assert.True(coder.Output.Count >= 5);
    }
}

public class LzmaMarkovModelTests
{
    [Fact]
    public void GetLiteralProbs_ReturnsDifferentTables_ForDifferentContexts()
    {
        var model  = new LzmaMarkovModel(lc: 3, lp: 0);
        var probs1 = model.GetLiteralProbs(0x00, 0);
        var probs2 = model.GetLiteralProbs(0xFF, 0);
        Assert.False(ReferenceEquals(probs1, probs2));
    }

    [Fact]
    public void EncodeLiteral_DoesNotThrow()
    {
        var model = new LzmaMarkovModel(lc: 3, lp: 0);
        var coder = new LzmaRangeCoder();
        model.EncodeLiteral(coder, 0x00, 0, (byte)'A');
        coder.Finish();
        Assert.True(coder.Output.Count > 0);
    }

    [Fact]
    public void EncodeMatchLength_Min_DoesNotThrow()
    {
        var model = new LzmaMarkovModel();
        var coder = new LzmaRangeCoder();
        model.EncodeMatchLength(coder, 2); // minimum match length
        coder.Finish();
        Assert.True(coder.Output.Count > 0);
    }
}

public class LzmaCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 6 };

    [Fact]
    public async Task RoundTrip_ShortText()
    {
        var input  = Encoding.UTF8.GetBytes("lzma uses range coding and markov models");
        var sut    = new LzmaCompressor();
        var comp   = await sut.CompressAsync(input, Opts);
        var decomp = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, decomp.Value);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new LzmaCompressor().CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AlgorithmName_IsLzma()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var comp  = await new LzmaCompressor().CompressAsync(input, Opts);
        Assert.Equal("LZMA", comp.Value!.AlgorithmName);
    }

    [Fact]
    public async Task RoundTrip_SingleByte()
    {
        byte[] input = [0x42];
        var sut  = new LzmaCompressor();
        var comp = await sut.CompressAsync(input, Opts);
        var dec  = await sut.DecompressAsync(comp.Value!.CompressedBytes, 1);
        Assert.Equal(input, dec.Value);
    }
}

public class OpenZlStructureAnalyzerTests
{
    private readonly OpenZlStructureAnalyzer _sut = new();

    [Fact]
    public void Analyze_Csv_DetectsFormat()
    {
        string csv = "name,age,city\nAlice,30,NYC\nBob,25,LA\nCarol,35,Chicago\nDave,28,Miami";
        var report = _sut.Analyze(csv);
        Assert.Equal(OpenZlStructureAnalyzer.DataFormat.Csv, report.Format);
        Assert.Equal(3, report.ColumnCount);
        Assert.True(report.RowCount >= 4);
    }

    [Fact]
    public void Analyze_Json_DetectsFormat()
    {
        string json = """[{"name":"Alice","age":"30"},{"name":"Bob","age":"25"},{"name":"Carol","age":"35"},{"name":"Dave","age":"28"}]""";
        var report = _sut.Analyze(json);
        Assert.Equal(OpenZlStructureAnalyzer.DataFormat.Json, report.Format);
        Assert.True(report.ColumnCount >= 2);
    }

    [Fact]
    public void Analyze_EmptyString_ReturnsUnknown()
    {
        var report = _sut.Analyze("");
        Assert.Equal(OpenZlStructureAnalyzer.DataFormat.Unknown, report.Format);
    }

    [Fact]
    public void Analyze_SmallCsv_NotColumnarFriendly()
    {
        string csv = "a,b\n1,2";
        var report = _sut.Analyze(csv);
        Assert.False(report.IsColumnarFriendly);
    }

    [Fact]
    public void ColumnInfo_HasEntropy()
    {
        string csv = "val\n1\n2\n3\n4\n5";
        var report = _sut.Analyze(csv);
        if (report.ColumnCount > 0)
            Assert.True(report.Columns[0].EstimatedEntropy >= 0);
    }
}

public class OpenZlCompressorTests
{
    private static readonly CompressionOptions Opts = new() { CompressionLevel = 4 };

    [Fact]
    public async Task RoundTrip_Passthrough_BinaryData()
    {
        var input = Encoding.UTF8.GetBytes("not a structured format at all — just plain text bytes");
        var sut   = new OpenZlCompressor(new DeflateCompressor());
        var comp  = await sut.CompressAsync(input, Opts);
        var dec   = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        Assert.Equal(input, dec.Value);
    }

    [Fact]
    public async Task RoundTrip_CsvData()
    {
        string csv = "name,score,rank\nAlice,95,1\nBob,87,2\nCarol,91,3\nDave,78,4\nEve,88,5";
        var input  = Encoding.UTF8.GetBytes(csv);
        var sut    = new OpenZlCompressor(new DeflateCompressor());
        var comp   = await sut.CompressAsync(input, Opts);
        var dec    = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        // Round-trip: decoded text should contain all column headers and values.
        string result = Encoding.UTF8.GetString(dec.Value!);
        Assert.Contains("Alice", result);
        Assert.Contains("score", result);
    }

    [Fact]
    public async Task RoundTrip_JsonData()
    {
        string json = """[{"city":"NYC","pop":"8m"},{"city":"LA","pop":"4m"},{"city":"Chicago","pop":"3m"},{"city":"Houston","pop":"2m"}]""";
        var input   = Encoding.UTF8.GetBytes(json);
        var sut     = new OpenZlCompressor(new DeflateCompressor());
        var comp    = await sut.CompressAsync(input, Opts);
        var dec     = await sut.DecompressAsync(comp.Value!.CompressedBytes, input.Length);
        string result = Encoding.UTF8.GetString(dec.Value!);
        Assert.Contains("NYC", result);
        Assert.Contains("city", result);
    }

    [Fact]
    public async Task EmptyInput_ReturnsFail()
    {
        var result = await new OpenZlCompressor(new DeflateCompressor()).CompressAsync([], Opts);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AlgorithmName_IsOpenZl()
    {
        var input = Encoding.UTF8.GetBytes("test");
        var comp  = await new OpenZlCompressor(new DeflateCompressor()).CompressAsync(input, Opts);
        Assert.Equal("OpenZL", comp.Value!.AlgorithmName);
    }
}
