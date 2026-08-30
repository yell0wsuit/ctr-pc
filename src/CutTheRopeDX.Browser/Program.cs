using System;
using System.Runtime.Versioning;

using CutTheRopeDX.Browser;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

[assembly: SupportedOSPlatform("browser")]

await GLContextInterop.ImportAsync();
await FetchInterop.ImportAsync();
await AudioInterop.ImportAsync();
await StorageInterop.ImportAsync();
await BrowserCursorService.ImportAsync();
await BrowserVideoPlayer.ImportAsync();

// Announced before the content bundle starts downloading, so the level transfer overlaps a ~56 MB
// load rather than following it. A normal launch returns immediately.
bool playtest = await PlaytestSession.BeginAsync();
Console.WriteLine($"playtest: {(playtest ? "active" : "inactive")}");

int fbo = GLContextInterop.CreateContext("game");
// Installed before the first measurement so the loop never has to measure again: from here on
// the canvas reports its own changes.
GLContextInterop.WatchCanvas("game");
int[] size = GLContextInterop.CanvasSize("game");
Console.WriteLine($"gl: fbo={fbo} size={size[0]}x{size[1]}");

SkiaSurface surface = new(fbo, size[0], size[1]);

BrowserContentStore content = new("./content/");
WebAudioBackend audio = new("./content/");
await content.LoadTier0Async("./content/tier0.json");
await content.LoadAllAssetsAsync("./content/assets.json", audio);
PlatformServices.Content = content;

BrowserHostApp host = new();
PlatformServices.Host = host;
PlatformServices.Render = new SkiaRenderBackend(surface);
PlatformServices.Preferences = new LocalStoragePreferenceStore();
PlatformServices.Cursor = new BrowserCursorService();
PlatformServices.VideoPlayerFactory = () => new BrowserVideoPlayer();

BrowserAssetPlatform assets = new(surface);

ScreenPresentation.Instance =
    new ScreenPresentation((int)ViewportLayout.DesignWidth, (int)ViewportLayout.DesignHeight);
CutTheRopeDX.CtrBootstrap.Initialize(
    assets,
    audio,
    size[0],
    size[1],
    LanguageHelper.Current,
    (float)GLContextInterop.CanvasDevicePixelRatio());

GameLoop.Surface = surface;
GameLoop.Host = host;
InputRouter.Host = host;

Console.WriteLine("boot complete");
