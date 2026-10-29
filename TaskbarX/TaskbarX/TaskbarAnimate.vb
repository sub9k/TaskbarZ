Option Strict On

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading

Public Class TaskbarAnimate
    Private Class AnimationState
        Public Handle As IntPtr
        Public Position As Integer
        Public Target As Integer
        Public Orientation As String
        Public Easing As EasingDelegate
        Public Duration As Integer
        Public IsPrimary As Boolean
        Public Version As Integer
        Public Current As Double
        Public Following As Boolean
    End Class

    Private Shared ReadOnly active As New Dictionary(Of IntPtr, AnimationState)()

    Public Shared Sub Animate(hwnd As IntPtr, oldpos As Integer, orient As String, easing As EasingDelegate, valueToReach As Integer, duration As Integer, isPrimary As Boolean, width As Integer)
        If hwnd = IntPtr.Zero Then Return

        If Settings.RevertZeroBeyondTray = 1 Then
            Dim bounds As Win32.RECT
            Win32.GetWindowRect(Win32.GetParent(hwnd), bounds)
            If valueToReach + width >= If(orient = "H", bounds.Right - bounds.Left, bounds.Bottom - bounds.Top) Then
                valueToReach = 0
            End If
        End If

        SyncLock active
            Dim state As AnimationState = Nothing
            If active.TryGetValue(hwnd, state) Then
                If state.Target = valueToReach AndAlso state.Orientation = orient Then Return
                state.Target = valueToReach
                state.Orientation = orient
                state.Easing = easing
                state.Duration = Math.Max(1, duration)
                state.Version += 1
                state.Following = True
                Return
            End If

            If oldpos = valueToReach Then Return

            state = New AnimationState With {
                .Handle = hwnd,
                .Position = oldpos,
                .Target = valueToReach,
                .Orientation = orient,
                .Easing = easing,
                .Duration = Math.Max(1, duration),
                .IsPrimary = isPrimary,
                .Version = 1,
                .Current = oldpos
            }
            active.Add(hwnd, state)
            If isPrimary Then TaskbarCenter.isanimating = True
            ThreadPool.QueueUserWorkItem(Sub() Run(state))
        End SyncLock
    End Sub

    Private Shared Sub Run(state As AnimationState)
        Try
            Dim start As Integer = state.Position
            Dim duration As Integer = state.Duration
            Dim easing As EasingDelegate = state.Easing
            Dim orientation As String = state.Orientation
            Dim target As Integer = state.Target
            Dim timer As Stopwatch = Stopwatch.StartNew()

            Do
                SyncLock active
                    If state.Following Then Exit Do
                End SyncLock

                Dim elapsed As Integer = CInt(Math.Min(timer.ElapsedMilliseconds, CLng(duration)))
                Dim position As Integer
                If elapsed >= duration Then
                    position = target
                ElseIf start < target Then
                    position = CInt(easing(elapsed, start, target - start, duration))
                Else
                    position = start + target - CInt(easing(elapsed, target, start - target, duration))
                End If
                SetPosition(state.Handle, orientation, position)
                SyncLock active
                    state.Position = position
                    state.Current = position
                    If elapsed >= duration AndAlso Not state.Following Then
                        active.Remove(state.Handle)
                        If state.IsPrimary Then TaskbarCenter.isanimating = False
                        Return
                    End If
                End SyncLock
                Thread.Sleep(Math.Max(1, 16 - CInt(timer.ElapsedMilliseconds Mod 16)))
            Loop

            'Explorer animates button widths during window close. Track the latest
            'target without restarting the selected easing curve on each sample.
            Dim followClock As Stopwatch = Stopwatch.StartNew()
            Dim previousTime As Long = 0
            Do
                Dim elapsed As Long = followClock.ElapsedMilliseconds
                Dim delta As Long = Math.Max(1, elapsed - previousTime)
                previousTime = elapsed
                Dim nextValue As Double
                Dim currentTarget As Integer
                SyncLock active
                    currentTarget = state.Target
                    orientation = state.Orientation
                    Dim timeConstant As Double = Math.Max(30.0, state.Duration / 4.0)
                    nextValue = state.Current + (currentTarget - state.Current) * (1.0 - Math.Exp(-CDbl(delta) / timeConstant))
                    If Math.Abs(nextValue - currentTarget) < 0.5 Then nextValue = currentTarget
                End SyncLock
                Dim position As Integer = CInt(Math.Round(nextValue))
                SetPosition(state.Handle, orientation, position)
                SyncLock active
                    state.Current = nextValue
                    state.Position = position
                    If state.Target = position AndAlso Math.Abs(nextValue - state.Target) < 0.5 Then
                        active.Remove(state.Handle)
                        If state.IsPrimary Then TaskbarCenter.isanimating = False
                        Return
                    End If
                End SyncLock
                Thread.Sleep(Math.Max(1, 16 - CInt(followClock.ElapsedMilliseconds Mod 16)))
            Loop
        Catch ex As Exception
            SyncLock active
                active.Remove(state.Handle)
                If state.IsPrimary Then TaskbarCenter.isanimating = False
            End SyncLock
            Console.WriteLine("@Animation | " & ex.Message)
        End Try
    End Sub

    Private Shared Sub SetPosition(hwnd As IntPtr, orient As String, position As Integer)
        If orient = "H" Then
            Win32.SetWindowPos(hwnd, IntPtr.Zero, position, 0, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
        Else
            Win32.SetWindowPos(hwnd, IntPtr.Zero, 0, position, 0, 0, Win32.SWP_NOSIZE Or Win32.SWP_NOACTIVATE Or Win32.SWP_NOZORDER Or Win32.SWP_NOSENDCHANGING)
        End If
    End Sub
End Class
