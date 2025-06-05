namespace HackRfSweep

open System
open System.Diagnostics

/// Represents a single sweep of frequency/magnitude pairs
[<Struct>]
type SweepData =
    { Frequencies: float[]
      Magnitudes: float[] }

module HackRfProcess =

    let private tryParseFloat (s: string) =
        match Double.TryParse(s, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
        | true, v -> Some v
        | _ -> None

    /// Parse one line of hackrf_sweep CSV output
    let private parseLine (line: string) : SweepData option =
        let parts = line.Split([| ',' |], StringSplitOptions.RemoveEmptyEntries)

        if parts.Length < 6 then
            None
        else
            match tryParseFloat parts.[2], tryParseFloat parts.[3], tryParseFloat parts.[4] with
            | Some startFreq, Some endFreq, Some step ->
                let magnitudes = parts |> Array.skip 5 |> Array.choose tryParseFloat

                if magnitudes.Length = 0 then
                    None
                else
                    let count = magnitudes.Length

                    let freqStep =
                        if step > 0.0 then
                            step
                        else
                            (endFreq - startFreq) / float count

                    let freqs = Array.init count (fun i -> startFreq + freqStep * float i)

                    Some
                        { Frequencies = freqs
                          Magnitudes = magnitudes }
            | _ -> None

    /// Start hackrf_sweep and stream parsed data to the callback
    let start (arguments: string) (onData: SweepData -> unit) =
        async {
            let exe = Config.hackRfSweep

            if not (IO.File.Exists exe) then
                SimpleLog.Log.logError ($"hackrf_sweep not found: {exe}")
            else
                let psi = ProcessStartInfo(exe, arguments)
                psi.UseShellExecute <- false
                psi.RedirectStandardOutput <- true
                psi.RedirectStandardError <- true

                use proc = new Process()
                proc.StartInfo <- psi

                try
                    proc.Start() |> ignore
                    use output = proc.StandardOutput

                    while not output.EndOfStream do
                        let! line = output.ReadLineAsync() |> Async.AwaitTask

                        let line =
                            match line with
                            | null -> ""
                            | l -> l.Trim()

                        match parseLine line with
                        | Some data -> onData data
                        | None -> ()
                with ex ->
                    SimpleLog.Log.logError ($"hackrf_sweep failed: {ex.Message}")
        }
        |> Async.Start
