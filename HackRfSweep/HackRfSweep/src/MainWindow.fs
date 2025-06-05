namespace HackRfSweep

open Avalonia.Controls
open Avalonia.Threading
open ScottPlot
open ScottPlot.Avalonia

module Utils =
    open System

    let copySinToHeatmap (sin: float[]) (heatmap: float[,]) =
        // sanity check
        let rows = heatmap.GetLength 0
        let cols = heatmap.GetLength 1

        SimpleLog.Log.logInfo $"rows: {rows}, cols: {cols}"

        if sin.Length <> cols then
            invalidArg "sin" $"sin.Length = {sin.Length} but heatmap has {cols} columns"

        let bytesPerRow = cols * sizeof<float> // sizeof<float> = 8 on F# (double)
        let sinBytes = 0 // always start at 0 in the source

        for r = 0 to rows - 1 do
            // copy one whole row at a time
            Buffer.BlockCopy(
                sin,
                sinBytes, // source + offset
                heatmap,
                r * bytesPerRow, // dest   + offset
                bytesPerRow
            ) // how many bytes

    /// shift every row of `heatmap` down by one, discard the last row,
    /// and copy `sin` into row 0
    let scrollDownAndInsert
        (sin: float[]) // 1-D source vector
        (heatmap: float[,]) // 2-D target matrix, modified in-place
        =
        // --- guards ----------------------------------------------------------
        let rows = heatmap.GetLength 0
        let cols = heatmap.GetLength 1

        if sin.Length <> cols then
            invalidArg "sin" $"sin.Length = {sin.Length} but heatmap has {cols} columns"

        // --- fast block moves ------------------------------------------------
        let bytesPerRow = cols * sizeof<float>

        // 1. Shift the whole block (rows-1 rows) down by one row in a single
        //    native memmove.  Source starts at 0, destination starts at 1 row.
        if rows > 1 then
            Buffer.BlockCopy(
                heatmap, // source array
                0, // byte offset of row 0
                heatmap, // destination is the same array
                bytesPerRow, // byte offset of row 1
                (rows - 1) * bytesPerRow
            ) // total bytes (rows-1 rows)

        // 2. Copy the new sin vector into row 0
        Buffer.BlockCopy(
            sin,
            0, // copy the whole vector
            heatmap,
            0, // into the very first row
            bytesPerRow
        )


type MainWindow() as this =
    inherit Window()

    let avaloniaPlot = AvaPlot()
    let dockPanel = DockPanel()

    let mutable signalData: float[] = Array.empty
    let mutable heatmapData: float[,] = Array2D.zeroCreate 0 0
    let mutable sigPlot: Plottables.Signal option = None
    let mutable heatPlot: Plottables.Heatmap option = None

    do
        // Basic window setup
        this.Title <- "HackRF Sweep"
        this.Width <- 1200.0
        this.Height <- 800.0

        avaloniaPlot.Multiplot.AddPlots 2
        //move bottom axis to top
        let plot0 = avaloniaPlot.Multiplot.GetPlot 0
        let plot1 = avaloniaPlot.Multiplot.GetPlot 1

        signalData <- Array.create 1000 0.0

        let sig0 = plot0.Add.Signal(signalData)
        sig0.Axes.XAxis <- plot0.Axes.Top
        sigPlot <- Some sig0

        let width = 256
        heatmapData <- Array2D.zeroCreate 200 width
        heatPlot <- Some(plot1.Add.Heatmap(heatmapData))

        DockPanel.SetDock(avaloniaPlot, Dock.Top)
        dockPanel.Children.Add(avaloniaPlot)

        this.Content <- dockPanel

// let handleSweep (data: SweepData) =
//     Dispatcher.UIThread.Post(fun _ ->
//         signalData <- data.Magnitudes

//         match sigPlot with
//         | Some sp -> sp.Data <- signalData
//         | None -> ()

//         if heatmapData.GetLength 1 <> data.Magnitudes.Length then
//             heatmapData <- Array2D.zeroCreate (heatmapData.GetLength 0) data.Magnitudes.Length

//             match heatPlot with
//             | Some hp -> hp.Data <- heatmapData
//             | None -> ()

//         Utils.scrollDownAndInsert data.Magnitudes heatmapData

//         match heatPlot with
//         | Some hp -> hp.Data <- heatmapData
//         | None -> ()

//         avaloniaPlot.Refresh())

// HackRfProcess.start "" handleSweep
