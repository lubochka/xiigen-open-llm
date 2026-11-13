using System.Text;

namespace DatasetPipeline;

/// <summary>
/// Writes uint16 arrays in NumPy .npy v1.0 format.
/// Layout: 6-byte magic + 2-byte version + 2-byte header-length + header + data.
/// </summary>
public static class NpyWriter
{
    public static void Write(string path, ushort[] data)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        var header = BuildHeader(data.Length);
        var headerPadded = PadHeader(header);

        // Magic: \x93NUMPY
        bw.Write((byte)0x93);
        bw.Write(Encoding.ASCII.GetBytes("NUMPY"));
        // Version 1.0
        bw.Write((byte)1);
        bw.Write((byte)0);
        // Header length (little-endian uint16)
        bw.Write((ushort)headerPadded.Length);
        bw.Write(Encoding.ASCII.GetBytes(headerPadded));

        foreach (var v in data)
            bw.Write(v);
    }

    private static string BuildHeader(int length) =>
        $"{{'descr': '<u2', 'fortran_order': False, 'shape': ({length},), }}";

    private static string PadHeader(string header)
    {
        // Total prefix before header data = 10 bytes; align to 64-byte boundary
        int prefixLen = 10;
        int needed = prefixLen + header.Length + 1; // +1 for \n
        int aligned = ((needed + 63) / 64) * 64;
        int pad = aligned - prefixLen - header.Length - 1;
        return header + new string(' ', pad) + '\n';
    }
}
