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
        SimpleLog.Log.logWarning "HackRF Sweep version 0.1.0"


        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime args
