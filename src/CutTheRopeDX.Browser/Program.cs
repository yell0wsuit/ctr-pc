using System;
using System.Runtime.Versioning;

using CutTheRopeDX.Browser;
using CutTheRopeDX.Framework.Platform;

using SkiaSharp;

[assembly: SupportedOSPlatform("browser")]

await GLContextInterop.ImportAsync();

int fbo = GLContextInterop.CreateContext("game");
int[] size = GLContextInterop.CanvasSize("game");
Console.WriteLine($"gl: fbo={fbo} size={size[0]}x{size[1]}");

using SkiaSurface surface = new(fbo, size[0], size[1]);
surface.Canvas.Clear(new SKColor(0x2E, 0x86, 0xC1));

await FetchInterop.ImportAsync();

BrowserContentStore content = new("./content/");
await content.LoadTier0Async("./content/tier0.json");
PlatformServices.Content = content;

Console.WriteLine("tier0 loaded");

surface.Flush();
Console.WriteLine("graphics ok");
