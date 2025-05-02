using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoSmart.Unicode;

/// <summary>
/// A UnicodeSequence is a combination of one or more codepoints.
/// </summary>
public readonly struct UnicodeSequence : IComparable<UnicodeSequence>, IEquatable<UnicodeSequence>, IEquatable<string> {
    private static readonly char[] s_SequenceSplitChars = [',', ' '];

    private readonly Codepoint[] m_Codepoints;
    public IReadOnlyList<Codepoint> Codepoints => m_Codepoints;

    public UnicodeSequence(string sequence) {
        if (sequence.Contains("-")) {
            var values = sequence.Split('-');

            if (values.Length == 2) {
                var begin = new Codepoint(values[0]);
                var end = new Codepoint(values[1]);
                if (end.Value < begin.Value) {
                    throw new InvalidRangeException();
                }

                m_Codepoints = new Codepoint[end.Value - begin.Value + 1];
                for (var i = 0; begin.Value + i <= end.Value; ++i) {
                    m_Codepoints[i] = new Codepoint(begin.Value + i);
                }
            } else {
                throw new InvalidRangeException();
            }
        } else {
            var values = sequence.Split(s_SequenceSplitChars, StringSplitOptions.RemoveEmptyEntries);
            m_Codepoints = values.Select(x => new Codepoint(x)).ToArray();
        }
    }

    // The usage of `params` here should hopefully allocate on the stack for short lengths,
    // making this the most optimized version of the routine.
    public UnicodeSequence(params Codepoint[] codepoints) {
        m_Codepoints = codepoints;
    }

    public UnicodeSequence(IEnumerable<Codepoint> codepoints) {
        m_Codepoints = codepoints.ToArray();
    }

    public bool Contains(Codepoint codepoint) {
        return codepoint.In(m_Codepoints);
    }

    public IEnumerable<uint> AsUtf32() {
        foreach (var cp in m_Codepoints) {
            yield return cp.AsUtf32();
        }
    }

    public IEnumerable<byte> AsUtf32Bytes() {
        foreach (var u32 in AsUtf32()) {
            // Little Endian byte order
            yield return (byte)(u32 & 0xFF);
            yield return (byte)((u32 >> 8) & 0xFF);
            yield return (byte)((u32 >> 16) & 0xFF);
            yield return (byte)(u32 >> 24);
        }
    }

    public readonly IEnumerable<ushort> AsUtf16() {
#if NETSTANDARD2_1_OR_GREATER
            var words = new UInt16[2];
#endif
        foreach (var cp in m_Codepoints) {
#if NETSTANDARD2_1_OR_GREATER
                int count = cp.AsUtf16(words);
                yield return words[0];
                if (count == 2) {
                    yield return words[1];
                }
#else
            foreach (var u16 in cp.AsUtf16()) {
                yield return u16;
            }
#endif
        }
    }

    // Little Endian byte order
    public readonly IEnumerable<byte> AsUtf16Bytes() {
#if NETSTANDARD2_1_OR_GREATER
            var bytes = new byte[4];
            foreach (var cp in _codepoints) {
                var count = cp.AsUtf16Bytes(bytes);
                for (int i = 0; i < count; ++i) {
                    yield return bytes[i];
                }
            }
#else
        foreach (var u16 in AsUtf16()) {
            yield return (byte)(u16 & 0xFF);
            yield return (byte)(u16 >> 8);
        }
#endif
    }

    public IEnumerable<byte> AsUtf8() {
        foreach (var cp in m_Codepoints) {
            foreach (var b in cp.AsUtf8()) {
                yield return b;
            }
        }
    }

    public string AsString() {
        return Encoding.Unicode.GetString(AsUtf16Bytes().ToArray());
    }

    public int CompareTo(UnicodeSequence other) {
        return m_Codepoints.SequenceCompareTo(other.m_Codepoints);
    }

    public bool Equals(UnicodeSequence other) {
        return m_Codepoints.SequenceEqual(other.m_Codepoints);
    }

    public static bool operator ==(UnicodeSequence a, UnicodeSequence b) {
        return a.Equals(b);
    }

    public static bool operator !=(UnicodeSequence a, UnicodeSequence b) {
        return !a.Equals(b);
    }

    public static bool operator <(UnicodeSequence a, UnicodeSequence b) {
        return a.CompareTo(b) < 0;
    }

    public static bool operator >(UnicodeSequence a, UnicodeSequence b) {
        return a.CompareTo(b) > 0;
    }

    public override bool Equals(object? b) {
        return b is UnicodeSequence other && Equals(other);
    }

    public override int GetHashCode() {
        return m_Codepoints.GetHashCode();
    }

    public bool Equals(string? other) {
        return other is not null && other.Codepoints().SequenceEqual(m_Codepoints);
    }

    public override string ToString() {
        return string.Join(" ", Codepoints);
    }
}
