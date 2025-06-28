using System.Globalization;
using System.Text;

namespace QLSuaChuaVaLapDat.Models.TimKiem
{
    public static class Utils
    {
        public static string RemoveDiacritics(string input)
        {
            var normalizedString = input.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

    }

}
