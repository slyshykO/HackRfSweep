module SimpleLog

open System
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// interface with write method
[<Interface>]
type ILog =
    abstract member write: string -> unit
    abstract member name: unit -> string

module private LogInternalState =
    let mutable private logLevel = 0

type LogLevel =
    | Debug
    | Info
    | Warning
    | Error

    override this.ToString() =
        match this with
        | Debug -> "DEBUG"
        | Info -> "INFO"
        | Warning -> "WARNING"
        | Error -> "ERROR"

    static member ToInt(l: LogLevel) =
        match l with
        | Debug -> 0
        | Info -> 1
        | Warning -> 2
        | Error -> 3

    static member FromInt(i: int) =
        match i with
        | 0 -> Some Debug
        | 1 -> Some Info
        | 2 -> Some Warning
        | 3 -> Some Error
        | _ -> None



type Log() =

    static let mutable instance_: Log = Log()

    let listOfLoggers_ = ResizeArray<ILog>()
    let mutable logLevel_ = LogLevel.Info


    member _.logLevel
        with get () = logLevel_
        and set value = logLevel_ <- value

    member _.listOfLoggers = listOfLoggers_

    member _.addLogger(logger: ILog) = instance_.listOfLoggers.Add logger

    static member private formatMsg(msg: string, level: LogLevel, line: int, file: string) =
        let fileName =
            try
                System.IO.Path.GetFileName file |> Option.ofObj |> Option.defaultValue file
            with _ ->
                file

        let dateTime =
            try
                DateTime.Now.ToString "yyyy-MM-dd HH:mm:ss"
            with _ ->
                ""

        $"{dateTime},{level, -7},{msg},[{fileName}:{line}]"

    static member logWrite(msg: string, level: LogLevel, line: int, file: string) =
        if LogLevel.ToInt level >= LogLevel.ToInt instance_.logLevel then
            let msg = Log.formatMsg (msg, level, line, file)
            instance_.listOfLoggers |> Seq.iter (fun logger -> logger.write msg)


    static member log
        (
            msg: string,
            level: LogLevel,
            [<CallerLineNumber; Optional; DefaultParameterValue(-1)>] line: int,
            [<CallerFilePath; Optional; DefaultParameterValue("<unknown>")>] file: string
        ) =
        Log.logWrite (msg, level, line, file)

    static member inline logInfo
        (
            msg: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(-1)>] line: int,
            [<CallerFilePath; Optional; DefaultParameterValue("<unknown>")>] file: string
        ) =
        Log.logWrite (msg, LogLevel.Info, line, file)

    static member inline logDebug
        (
            msg: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(-1)>] line: int,
            [<CallerFilePath; Optional; DefaultParameterValue("<unknown>")>] file: string
        ) =
        Log.logWrite (msg, LogLevel.Debug, line, file)

    static member inline logWarning
        (
            msg: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(-1)>] line: int,
            [<CallerFilePath; Optional; DefaultParameterValue("<unknown>")>] file: string
        ) =
        Log.logWrite (msg, LogLevel.Warning, line, file)

    static member inline logError
        (
            msg: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(-1)>] line: int,
            [<CallerFilePath; Optional; DefaultParameterValue("<unknown>")>] file: string
        ) =
        Log.logWrite (msg, LogLevel.Error, line, file)


type FileLogger(name: string, fileName: string) =
    let fileName_ = fileName
    let name_ = name


    interface ILog with
        member _.write(msg: string) =
            try
                use file = IO.File.Open(fileName_, IO.FileMode.Append, IO.FileAccess.Write)

                use writer = new IO.StreamWriter(file, Text.UTF8Encoding false)
                writer.WriteLine msg
                writer.Flush()
                writer.Close()
            with _ ->
                ()

        member _.name() = name_
