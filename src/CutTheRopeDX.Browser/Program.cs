using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using CutTheRopeDX.Browser;

using SkiaSharp;

[assembly: SupportedOSPlatform("browser")]

await GLContextInterop.ImportAsync();

int fbo = GLContextInterop.CreateContext("game");
int[] size = GLContextInterop.CanvasSize("game");
Console.WriteLine($"gl: fbo={fbo} size={size[0]}x{size[1]}");

using SkiaSurface surface = new(fbo, size[0], size[1]);
surface.Canvas.Clear(new SKColor(0x2E, 0x86, 0xC1));

// Font smoke test. Fonts ship as subset TTF because SkiaSharp 4.151.1's WebAssembly
// build compiles FreeType without FT_CONFIG_OPTION_USE_BROTLI - its archive has zero
// woff2 and zero Brotli symbols - so WOFF2 cannot be decoded. Assert the format that IS
// shipped actually loads, because a silent failure here means every glyph renders blank.
byte[] font = await FetchProbeAsync("./content/fonts/gooddog_new-webfont.ttf");
if (font.Length > 0)
{
    using SKData data = SKData.CreateCopy(font);
    using SKTypeface typeface = SKTypeface.FromData(data);
    Console.WriteLine(typeface is null
        ? "FAIL: SkiaSharp could not decode the subset font"
        : $"font ok: {typeface.FamilyName}");
}
else
{
    Console.WriteLine("font probe skipped: run scripts/build_web_content.py first");
}

surface.Flush();
Console.WriteLine("graphics ok");

static async Task<byte[]> FetchProbeAsync(string url)
{
    using System.Net.Http.HttpClient client = new()
    {
        BaseAddress = new Uri(GLContextInterop.DocumentBaseUrl()),
    };
    try
    {
        return await client.GetByteArrayAsync(url);
    }
    catch (Exception)
    {
        return [];
    }
}
