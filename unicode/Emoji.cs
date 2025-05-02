using System.Collections.Generic;

namespace NeoSmart.Unicode;

// We hereby declare emoji to be a zero plural marker noun (in short, "emoji" is both the
// singular and the plural form). This class refers to emoji in the plural.
public static partial class Emoji {
    static Emoji() {
        // Populate the list of available emoji in the system font.
        // For now, the font is hard-coded to Segoe UI Emoji.
    }

    /// <summary>
    /// ZWJ is used to combine multiple emoji codepoints into a single emoji symbol.
    /// </summary>
    public static readonly Codepoint ZeroWidthJoiner = Codepoints.ZWJ;

    public static readonly Codepoint ObjectReplacementCharacter = Codepoints.ORC;

    public static readonly Codepoint Keycap = Codepoints.Keycap;

    /// <summary>
    /// The Emoji VS indicates that the preceding (non-emoji) unicode codepoint should be represented as an emoji.
    /// </summary>
    public static readonly Codepoint VariationSelector = Codepoints.VariationSelectors.EmojiSymbol;

    public static class SkinTones {
        public static readonly Codepoint Light = new("U+1F3FB");
        public static readonly Codepoint Fitzpatrick12 = Light;
        public static readonly Codepoint MediumLight = new("U+1F3FC");
        public static readonly Codepoint Fitzpatrick3 = MediumLight;
        public static readonly Codepoint Medium = new("U+1F3FD");
        public static readonly Codepoint Fitzpatrick4 = Medium;
        public static readonly Codepoint MediumDark = new("U+1F3FE");
        public static readonly Codepoint Fitzpatrick5 = MediumDark;
        public static readonly Codepoint Dark = new("U+1F3FF");
        public static readonly Codepoint Fitzpatrick6 = Dark;

        /// <summary>
        /// Helper object, most useful for checking if a codepoint is a skin tone quickly.
        /// </summary>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static readonly List<Codepoint> All = new() { Light, MediumLight, Medium, MediumDark, Dark };
    }

    /// <summary>
    /// Combines multiple emoji with a zero-width-joiner to (potentially) create a new symbol
    /// </summary>
    /// <param name="emoji"></param>
    /// <returns></returns>
    public static string Combine(IEnumerable<SingleEmoji> emoji) {
#if !NET20
        return string.Join(ZeroWidthJoiner.AsString(), emoji);
#else
            return string.Join(ZeroWidthJoiner.AsString(), emoji.Select(e => e.ToString()).ToArray());
#endif
    }

    /// <summary>
    /// Combines multiple emoji with a zero-width-joiner to (potentially) create a new symbol
    /// </summary>
    /// <param name="emoji"></param>
    /// <returns></returns>
    public static string Combine(params SingleEmoji[] emoji) {
        return Combine((IEnumerable<SingleEmoji>)emoji);
    }

    /// <summary>
    /// Determines whether a string is comprised solely of emoji, optionally with a maximum number of drawn symbols.
    /// Can be used to determine whether a message consists of ≦ x emoji for purposes such as displaying at a larger size. Since one emoji symbol can be formed
    /// from many separate emoji "characters" combined with zero-width joiners or even non-emoji characters followed by a "use emoji representation" marker, this
    /// cannot be determined solely from the codepoints.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="maxSymbolCount"></param>
    /// <returns></returns>
    public static bool IsEmoji(string message, int maxSymbolCount = int.MaxValue) {
        var codepoints = message.Codepoints();

        // ReSharper disable once InconsistentNaming
        var nextMustBeVS = false;
        var zwj = ZeroWidthJoiner.AsString();
        var variationSelector = VariationSelector.AsString();
        var ignoreNext = false;
        var count = 0;
        foreach (var cp in codepoints) {
            // We used to have `message = message.trim()` previously. This avoids the extra
            // allocation, hepful in case of long messages. This was not premature optimization,
            // it came out of necessity.
            if (cp == "\n" || cp == "\r" || cp == "\t" || cp == " ") {
                continue;
            }

            if (nextMustBeVS) {
                nextMustBeVS = false;
                if (cp != variationSelector) {
                    // A non-emoji codepoint was found and this (the codepoint after it) is not
                    // the variation selector
                    return false;
                }
            }

            if (cp.In(SkinTones.All)) {
                // Don't count the skin tone as part of the length
                continue;
            }

            if (cp == zwj) {
                ignoreNext = true;
                continue;
            }

            if (cp == variationSelector) {
                continue;
            }

            if (cp == Codepoints.ObjectReplacementCharacter) {
                // This is explicitly blacklisted for UI purposes
                return false;
            }

            if (cp == Keycap) {
                // This is not in the UTR, but is used to box symbols in an icon.
                // Do not consider it part of the length.
                continue;
            }

            if (!ignoreNext) {
                ++count;
                if (count > maxSymbolCount) {
                    return false;
                }

                // By default, the UTR lists numbers, the asterisk, and the number sign as
                // emoji, but we won't consider them as such unless they are followed by a VS.
                if (Languages.ArabicNumerals.Contains(cp) || cp == "#" || cp == "*") {
                    nextMustBeVS = true;
                    continue;
                } else if (Languages.Emoji.Contains(cp)) {
                    continue;
                } else {
                    // We've either encountered a non-emoji character OR a non-emoji codepoint
                    // that should be treated as an emoji if followed by the variation selector
                    // codepoint.
                    nextMustBeVS = true;
                    continue;
                }
            }

            ignoreNext = false;
        }

        if (nextMustBeVS) {
            return false;
        }

        return count > 0 && count <= maxSymbolCount;
    }
}
