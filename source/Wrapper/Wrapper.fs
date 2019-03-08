module Wrapper
open System
// curry String.IndexOfAny method
let indexOfAny chars (str: string) initial =
  match str.IndexOfAny (chars, initial) with
  | -1 -> None
  | index -> Some index
// sequence of string's index positions matching a character in char array
let findSeq chars (str: string) =
  Seq.unfold (indexOfAny chars str
              >> Option.map (fun index -> (index, index + 1)))
             0
// https://msdn.microsoft.com/en-us/library/17w5ykft.aspx
// blank characters are space or tab
let blankChars = " 	".ToCharArray()
// wrap with " only when blanks are present
// double \ preceding "
// escape only non-delimiter "
let escapeArg arg =
  let partial = match indexOfAny [|'"'|] arg 0 with
                | None -> arg
                | Some _ -> (arg, @"$1$1\""") |> (Text.RegularExpressions.Regex @"(\*)""").Replace
  match indexOfAny blankChars arg 0 with
  | None -> partial
  | Some _ -> (partial, @"$1$1") |> (Text.RegularExpressions.Regex @"(\+)$").Replace |> sprintf """%s"""
// start process directly (no intervening shell)
// return its exit code
let startProcess fileName arguments =
  use proc = new Diagnostics.Process()
  let psi = proc.StartInfo
  psi.UseShellExecute <- false
  psi.FileName <- fileName
  psi.Arguments <- arguments
  proc.Start() |> ignore
  proc.WaitForExit()
  proc.ExitCode
// pass the current base filename with the same command arguments to WSL
[<EntryPoint; Diagnostics.Switch("SourceSwitch", typeof<Diagnostics.SourceSwitch>)>]
let main _ =
  let ts = Diagnostics.TraceSource "Wrapper"
  let program = (Environment.GetCommandLineArgs()).[0]
  let cmdArgLine = (
    let findBlankSeq = findSeq blankChars
    let blankCount = program |> findBlankSeq |> Seq.length
    let index = Environment.CommandLine + " " |> findBlankSeq |> Seq.item blankCount
    if index = Environment.CommandLine.Length
    then
      ""
    else
      Environment.CommandLine.Substring(index)
  )
  let cmdArgLine = sprintf "%s%s" (program |> IO.Path.GetFileNameWithoutExtension |> escapeArg) cmdArgLine
  cmdArgLine |> sprintf "wsl %s" |> ts.TraceInformation
  startProcess "wsl.exe" cmdArgLine // return an integer exit code