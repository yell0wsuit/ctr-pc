using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using CutTheRopeDX.Browser;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

[assembly: SupportedOSPlatform("browser")]

await GLContextInterop.ImportAsync();
await RenderProbeInterop.ImportAsync();
if (RenderProbeInterop.IsRequested())
{
    await WorkerRenderProbe.RunAsync();
    return;
}

await FetchInterop.ImportAsync();
await AudioInterop.ImportAsync();
await StorageInterop.ImportAsync();
await BrowserCursorService.ImportAsync();
await BrowserVideoPlayer.ImportAsync();

// Announced before the content bundle starts downloading, so the level transfer overlaps a ~56 MB
// load rather than following it. A normal launch returns immediately.
bool playtest = await PlaytestSession.BeginAsync();
Console.WriteLine($"playtest: {(playtest ? "active" : "inactive")}");

// The canvas moves to this thread before Skia exists, and never moves back: the
// released SkiaSharp archive calls GL on whichever thread it is running on, so the
// thread that renders has to be the thread that owns the context.
_ = HostShim.InstallCanvasListener();
int[] canvas = GLContextInterop.TransferCanvasToThread("game", HostShim.ThreadId());
if (canvas.Length != 4)
{
    throw new InvalidOperationException("Could not transfer the canvas to the game thread.");
}

int deliveryAttempts = 0;
while (HostShim.CanvasReceived() == 0 && deliveryAttempts < 200)
{
    deliveryAttempts++;
    await Task.Delay(25);
}
if (HostShim.CanvasReceived() == 0)
{
    throw new InvalidOperationException("The transferred canvas never arrived.");
}

if (HostShim.CreateWorkerContext(canvas[2], canvas[3]) == 0)
{
    throw new InvalidOperationException("Could not create the game thread's WebGL context.");
}

GLContextInterop.WatchCanvas("game");
int[] size = [canvas[2], canvas[3]];
Console.WriteLine($"gl: size={size[0]}x{size[1]}");

SkiaSurface surface = new(0, size[0], size[1]);

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

GameLoop.Start();

Console.WriteLine("boot complete");
