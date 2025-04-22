namespace HackRfSweep

module private ConfigInternal =
    let hackRfDirs = [ ".\\"; "d:\\Projects\\SDR\\hackRF\\_build\\release\\msvc" ]

module Config =
    [<Literal>]
    let AppName = "HackRF Sweep"

    let isWindows =
        let pID = System.Environment.OSVersion.Platform
        pID = System.PlatformID.Win32NT || pID = System.PlatformID.Win32Windows

    let hackRfSweepName = if isWindows then "hackrf_sweep.exe" else "hackrf_sweep"
    let hackRfInfoName = if isWindows then "hackrf_info.exe" else "hackrf_info"

    let envVar (name: string) =
        try
            match System.Environment.GetEnvironmentVariable name with
            | null -> None
            | value -> Some value
        with _ex ->
            None

    let currentDirectory () =
        try
            System.IO.Directory.GetCurrentDirectory()
        with _ex ->
            ""

    let hackRfSweep =
        let searchPath =
            [ (envVar "HACKRF_DIR" |> Option.defaultValue (currentDirectory ())) ]
            @ ConfigInternal.hackRfDirs
            |> List.map (fun dir -> System.IO.Path.Combine(dir, hackRfSweepName))

        searchPath |> List.find (fun path -> System.IO.File.Exists path)

    let hackRfInfo =
        let searchPath =
            [ (envVar "HACKRF_DIR" |> Option.defaultValue (currentDirectory ())) ]
            @ ConfigInternal.hackRfDirs
            |> List.map (fun dir -> System.IO.Path.Combine(dir, hackRfInfoName))

        searchPath |> List.find (fun path -> System.IO.File.Exists path)

    let logPath () = "hackrf_sweep.log"
