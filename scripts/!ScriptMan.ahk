#Requires AutoHotkey v2
#SingleInstance Force
#NoTrayIcon
A_WorkingDir := A_ScriptDir
TraySetIcon('..\distribution\icons\CutTheRopeDXIcon.ico', , 1)
ProjectName := ParentDirectory()[-1]
A_ScriptName := 'ScriptMan (' ProjectName ')'

; Clean up any leftover run logs from crashed/killed sessions.
CleanupLogs(*) {
    loop files A_Temp '\scriptman_*.txt' {
        FileDelete(A_LoopFileFullPath)
    }
}
CleanupLogs()

; Retrieves the parent directory. (\..)
ParentDirectory(dir := A_ScriptDir) {
    i := StrSplit(dir, '\')
    i.Pop
    return i
}

; WHY IS THIS NOT BUILT-IN!!?
Join(sep, array*) {
    for i, value in array
        str .= value . sep
    return SubStr(str, 1, -StrLen(sep))
}

obj := ctrScriptMan()
obj.Show

; Basically all the code :P.
class ctrScriptMan {
    static ExePath := A_ScriptDir '\..\CutTheRopeDX\bin\Debug\net10.0\CutTheRope-DX.exe'
    static ContentDir := Join('\', ParentDirectory()*) '\content'
    static DefLevel := Join('\', ParentDirectory()*) '\content\maps\1_1.xml'

    Window := Gui('+DPIScale +Resize +MinSize300x200')
    LevelLauncher := Gui('+DPIScale Owner' this.Window.Hwnd, 'Select a level')
    Menu := {
        Window: unset,
        Command: unset,
        Macro: unset,
        TBuild: unset,
    }
    Output := {
        Tabs: unset,
        CMD: unset,
        BLD: unset,
    }
    StdOut := {
        CMD: unset,
        BLD: unset,
    }

    ; Creates a new ScriptMan window.
    __New(*) {
        this.LevelLauncher.SetFont 'S11'
        this.Window.SetFont 'S11'

        this.Directory := this.LevelLauncher.AddEdit('r1 w' 300 - this.LevelLauncher.MarginX * 2, ctrScriptMan.DefLevel)
        this.LevelLauncher.AddButton('r1 xp w' 150 - this.LevelLauncher.MarginX * 1.25, 'Choose any XML').OnEvent('Click', (*) => this.ChooseLevelFile())
        this.LevelLauncher.AddButton('r1 yp w' 150 - this.LevelLauncher.MarginX * 1.25 ' Disabled', 'Placeholder').OnEvent('Click', (*) => MsgBox)
        this.LevelLauncher.AddButton('r1 xm w' 300 - this.LevelLauncher.MarginX * 2, 'Launch').OnEvent('Click', (*) => this.LaunchLevel())

        this.Menu.Window := Menu()
        this.Menu.Window.Add('&Clear output', (*) => this.ClearCurrentOutput())
        this.Menu.Window.Add('&Pin', (*) => WinSetAlwaysOnTop(-1, this.Window.Hwnd))
        this.Menu.Window.Add()
        this.Menu.Window.Add('&Open folder', (*) => Run(A_ScriptDir '\..'))
        this.Menu.Window.Add()
        this.Menu.Window.Add('&Reload', (*) => Reload())
        this.Menu.Window.Add('E&xit', (*) => ExitApp())

        this.Menu.Command := Menu()
        this.Menu.Command.Add('Generate release notes', (*) => this.StdOut.CMD.Run('python -u generate_release_notes.py'))
        this.Menu.Command.Add('Bundle content', (*) => this.StdOut.CMD.Run('python -u bundle_content.py "' ctrScriptMan.ContentDir '"'))
        this.Menu.Command.Add('Test build', (*) => this.StdOut.CMD.Run('dotnet build -f net10.0', A_ScriptDir '\..'))

        this.Menu.Macro := Menu()
        this.Menu.Macro.Add('Standalone actions', this.Menu.Command)
        this.Menu.Macro.Add('Make a test build with new assets', (*) => this.StdOut.CMD.Run('python -u bundle_content.py "' ctrScriptMan.ContentDir '"', , (*) => this.StdOut.CMD.Run('dotnet build -f net10.0', A_ScriptDir '\..')))

        this.Menu.TBuild := Menu()
        this.Menu.TBuild.Add('Normal launch', (*) => this.StdOut.BLD.Run('"' ctrScriptMan.ExePath '"', A_ScriptDir))
        this.Menu.TBuild.Add('Level launch', (*) => this.ShowLevelLauncher())

        this.Window.MenuBar := MenuBar()
        this.Window.MenuBar.Add('&Window...', this.Menu.Window)
        this.Window.MenuBar.Add('&Commands...', this.Menu.Macro)
        this.Window.MenuBar.Add('Test &build...', this.Menu.TBuild)

        this.Output.Tabs := this.Window.AddTab3('-Wrap', ['Commands', 'Test build'])
        this.Output.Tabs.UseTab 1
        this.Output.CMD := this.Window.AddEdit('r15 w500 ReadOnly +VScroll', 'Choose one of the "Commands..." options to start.`n')
        this.Output.Tabs.UseTab 2
        this.Output.BLD := this.Window.AddEdit('r15 w500 ReadOnly +VScroll', 'Choose one of the "Test build..." options to start.`n')
        if !FileExist(ctrScriptMan.ExePath) {
            this.Output.BLD.Value := 'No test build found yet. Options for this tab won`'t work until you build one.`nUse "Make a test build with new assets" on the "Commands..." menu.`n'
        }

        this.StdOut.CMD := ScriptRunner(this.Output.CMD)
        this.StdOut.BLD := ScriptRunner(this.Output.BLD)

        ; Needs to explicitly bind `this` or else it will lose it. Why must you be like this AHK...
        this.Window.OnEvent('Size', this.ChangeElementsSize.Bind(this))
        this.Window.OnEvent('Close', (*) => (CleanupLogs(), ExitApp()))
    }

    ; Shows the window.
    Show(*) {
        this.Window.Show
    }

    ; Hides the window.
    Hide(*) {
        this.Window.Hide
    }

    ; Clears all output on the current tab.
    ClearCurrentOutput(*) {
        if (this.Output.Tabs.Value = 1) {
            this.Output.CMD.Value := ''
        } else {
            this.Output.BLD.Value := ''
        }
    }

    ; Shows the level-launcher popup window.
    ShowLevelLauncher(*) {
        this.LevelLauncher.Show
    }

    ; Hides the level-launcher popup window.
    HideLevelLauncher(*) {
        this.LevelLauncher.Hide
    }

    ; Opens a file-select dialog and, if a file is picked, writes its full path back into that field.
    ChooseLevelFile(*) {
        file := FileSelect('1', this.Directory.Value, 'Select a level to launch', 'Level files (*.xml)')
        if !file
            return
        this.Directory.Value := file
    }

    ; Runs the test build with the level in the Directory field passed via --level, then hides the launcher window (hold Shift to keep it open).
    LaunchLevel(*) {
        this.StdOut.BLD.Run('"' ctrScriptMan.ExePath '" --level "' this.Directory.Value '"', A_ScriptDir)
        if !GetKeyState('Shift', 'P')
            this.LevelLauncher.Hide
    }

    ; Handles element resizing when resizing the window.
    ChangeElementsSize(GuiObj, MinMax, Width, Height) {
        if (MinMax = -1)
            return
        this.Output.Tabs.Move(, , Width - this.Window.MarginX * 2, Height - this.Window.MarginY * 2)
        this.Output.CMD.Move(, , Width - this.Window.MarginX * 4, Height - 11 - this.Window.MarginY * 6)
        this.Output.BLD.Move(, , Width - this.Window.MarginX * 4, Height - 11 - this.Window.MarginY * 6)
    }

}

; Runs one command at a time against a single output control so each tab owns its own instance.
; Now a run on one tab never blocks or interleaves with a run on another.
class ScriptRunner {
    PID := 0
    TempFile := ''
    FilePos := 0
    Leftover := ''
    OnDone := ''
    OutCtrl := ''
    PollCallback := ''

    __New(outCtrl) {
        this.OutCtrl := outCtrl
        this.PollCallback := this.PollOutput.Bind(this)
    }

    ; True if this runner currently has a live process attached.
    IsRunning() {
        return this.PID && ProcessExist(this.PID)
    }

    ; Starts cmd asynchronously and streams its output to this runner's control as it runs.
    ; onDone fires once the process exits, letting commands be chained.
    Run(cmd, workDir := A_ScriptDir, onDone := '') {
        if this.IsRunning() {
            this.OutCtrl.Value .= '[A command is already running, please wait for it to finish.]`n'
            ControlSend('^{End}', this.OutCtrl)
            return
        }
        this.TempFile := A_Temp '\scriptman_' A_TickCount '.txt'
        this.FilePos := 0
        this.Leftover := ''
        this.OnDone := onDone

        Run(A_ComSpec ' /c "' cmd ' > "' this.TempFile '" 2>&1"', workDir, 'Hide', &pid)
        this.PID := pid

        SetTimer(this.PollCallback, 150)
    }

    ; Timer callback: reads any new output written since the last poll and appends it to the GUI.
    ; Once the process exits, it stops the timer, flushes remaining output, cleans up the temp file, and fires onDone if one was given.
    PollOutput() {
        if FileExist(this.TempFile) {
            try {
                f := FileOpen(this.TempFile, 'r', 'UTF-8')
                f.Pos := this.FilePos
                newText := f.Read()
                this.FilePos := f.Pos
                f.Close()

                if (newText != '') {
                    newText := this.Leftover . newText
                    lines := StrSplit(newText, '`n', '`r')
                    this.Leftover := lines.Pop()
                    for line in lines {
                        if (line != '') {
                            this.OutCtrl.Value .= line '`n'
                            ControlSend('^{End}', this.OutCtrl)
                        }
                    }
                }
            } catch as err {
                this.OutCtrl.Value .= '[poll error: ' err.Message ']`n'
            }
        }

        if !this.IsRunning() {
            SetTimer(this.PollCallback, 0)
            if (this.Leftover != '')
                this.OutCtrl.Value .= this.Leftover '`n'
            FileDelete(this.TempFile)

            if (this.OnDone != '') {
                callback := this.OnDone
                this.OnDone := ''
                callback()
            }
        }
    }

    ; Clears this runner's output control.
    Clear(*) {
        this.OutCtrl.Value := ''
    }
}
