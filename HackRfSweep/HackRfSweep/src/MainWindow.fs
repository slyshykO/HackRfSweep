namespace HackRfSweep

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Media

type MainWindow() as this =
    inherit Window()

    do
        // Basic window setup
        this.Title <- "Avalonia F# Waterfall App"
        this.Width <- 1200.0
        this.Height <- 800.0

