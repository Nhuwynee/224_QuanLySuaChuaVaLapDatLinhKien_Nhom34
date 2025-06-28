using PdfSharpCore.Fonts;

namespace HIENMAUNHANDAO.BaoCao
{
    public class CustomFontResolver : IFontResolver
    {
        private static readonly string FontName = "ArialCustom";
        private static byte[] _fontData;

        public CustomFontResolver()
        {
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial.ttf");
            _fontData = File.ReadAllBytes(fontPath);
        }

        public string DefaultFontName => throw new NotImplementedException();

        public byte[] GetFont(string faceName)
        {
            if (faceName == FontName)
            {
                return _fontData;
            }

            throw new InvalidOperationException("Font not found: " + faceName);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Bạn có thể xử lý isBold, isItalic nếu cần nhiều font khác nhau
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo(FontName);
            }

            return null;
        }
    }
}
