using System.Collections.Generic;
using System.Linq;
using NeoSmart.Collections;

namespace NeoSmart.Unicode;

public class MultiRange {
    private readonly SortedList<Range> m_Ranges = new();
    public IReadOnlyList<Range> Ranges => m_Ranges;

    public MultiRange(params string[] ranges) {
        m_Ranges.AddRange(ranges.Select(r => new Range(r)));
    }

    public MultiRange(params Range[] ranges) {
        m_Ranges.AddRange(ranges);
    }

    public MultiRange(IEnumerable<Range> ranges) {
        m_Ranges.AddRange(ranges);
    }

    public bool Contains(Codepoint codepoint) {
        var index = m_Ranges.IndexOf(new Range(codepoint));
        if (index > 0) {
            return true;
        }

        // No match, value is complement of Count or next greatest index
        index = ~index;
        if (index == 0) {
            return false;
        }

        // In case of range including this codepoint...
        return m_Ranges[index - 1].Contains(codepoint)
               // and in case of range starting with this codepoint
            || (index < m_Ranges.Count && m_Ranges[index].Contains(codepoint));
    }
}
