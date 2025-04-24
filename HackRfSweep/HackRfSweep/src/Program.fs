namespace HackRfSweep

open System
open Avalonia
open Avalonia.Controls.ApplicationLifetimes

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(Themes.Fluent.FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Default

    override this.OnFrameworkInitializationCompleted() =
        // If we have a desktop lifetime, create and show our MainWindow
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime -> desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

        base.OnFrameworkInitializationCompleted()

module Main =
    [<STAThread>]
    [<EntryPoint>]
    let main args =

        SimpleLog.Log.addLogger (SimpleLog.FileLogger("HackRF Logger", Config.logPath ()))
        SimpleLog.Log.logInfo "HackRF Sweep started"

        let version =
            Reflection.Assembly.GetExecutingAssembly().GetName().Version
            |> Option.ofObj
            |> Option.map (fun v -> v.ToString())
            |> Option.defaultValue "Unknown"

        SimpleLog.Log.logInfo $"HackRF Sweep version {version}"

        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime args
