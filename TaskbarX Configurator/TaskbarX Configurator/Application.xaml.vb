Class Application

    Private settingsWindow As MainWindow

    Protected Overrides Sub OnStartup(e As System.Windows.StartupEventArgs)
        MyBase.OnStartup(e)
        AddHandler Global.TaskbarX.Main.OpenSettingsRequested, AddressOf ShowSettings
        Global.TaskbarX.Main.Main()
        If System.Array.Exists(e.Args, Function(arg) arg.Equals("-config", System.StringComparison.OrdinalIgnoreCase)) Then
            ShowSettings()
        End If
    End Sub

    Private Sub ShowSettings()
        If settingsWindow Is Nothing Then
            settingsWindow = New MainWindow()
            AddHandler settingsWindow.Closed, AddressOf SettingsClosed
        End If
        settingsWindow.Show()
        settingsWindow.WindowState = System.Windows.WindowState.Normal
        settingsWindow.Activate()
    End Sub

    Private Sub SettingsClosed(sender As Object, e As System.EventArgs)
        settingsWindow = Nothing
        'The full WPF settings UI is only needed while it is open.
        Dispatcher.BeginInvoke(New System.Action(Sub() Global.TaskbarX.Main.ClearMemory()), System.Windows.Threading.DispatcherPriority.ApplicationIdle)
    End Sub

End Class
