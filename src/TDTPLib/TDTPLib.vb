Imports System.Drawing
Imports System.IO
Imports System.IO.Pipes

''' <summary>
''' <c>Controller</c>, Namespace for control TDTP classes.
''' </summary>
Namespace Controller
    ''' <summary>
    ''' <c>TDTPConnector</c>, Class for control TDTP.
    ''' </summary>
    Public Class TDTPConnector
        Implements IDisposable

        Private ComThread As Threading.Thread
        Private TDTPPipeToSend As NamedPipeClientStream
        Private TDTPPipeToRead As NamedPipeClientStream
        Private TDTPPipeReader As StreamReader
        Private TDTPPipeWriter As StreamWriter

        ''' <summary>
        ''' Fired when shortcut button was press, this event not fired if <c>ChangeShorcutMode</c> is set to <c>false</c>.
        ''' </summary>
        Public Event SortcutPressEvent(ByVal ShortcutID As Single)
        Public Sub New()
            TDTPPipeToSend = New NamedPipeClientStream("TDTPLibConSend")
            TDTPPipeToRead = New NamedPipeClientStream("TDTPLibConRead")
            TDTPPipeReader = New StreamReader(TDTPPipeToRead)
            TDTPPipeWriter = New StreamWriter(TDTPPipeToSend)
            Try
                TDTPPipeToRead.Connect(3000)
                TDTPPipeToRead.WaitForPipeDrain()
                TDTPPipeToSend.Connect(3000)
            Catch e As Exception
                Throw New System.Exception($"Cannot connect to TDTP: {e.Message}{vbCrLf}{e.StackTrace}")
            End Try
            TDTPPipeWriter.AutoFlush = True
            ComThread = New Threading.Thread(New Threading.ThreadStart(AddressOf CommandThread))
            ComThread.Start()
        End Sub

        <Obsolete("[DISABLED] This function was used for testing.", True)>
        <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
        Public Sub SendTestMessageDoNotUseForCoding()
            TDTPPipeWriter.WriteLine("Test, 12345 2ByteTest:[;:.,@`!""#$%&'()=~\]")
        End Sub

        Private Sub ChangeStateComThread(Enable As Boolean)
            If Enable Then
                ComThread.Start()
            Else
                ComThread.Interrupt()
            End If
        End Sub

        Private Sub CommandThread()
            Do While True
                Dim Message As String = TDTPPipeReader.ReadLine()
                If Message IsNot Nothing Then
                    Dim Command As String() = Message.Split(",")
                    If Command(0) = "Event" Then
                        'Event here
                        If Command(1) = "Shortcut" Then
                            RaiseEvent SortcutPressEvent(Single.Parse(Command(2)))
                        End If
                    End If
                End If
            Loop
        End Sub
        Protected Sub WritePipe(Message As String)
            TDTPPipeWriter.WriteLine(Message)
        End Sub
        ''' <summary>
        ''' Change TDTP Background image.
        ''' This function may take a few secound.
        ''' </summary>
        ''' <param name="ImagePath"><c>ImagePath</c> is path to image. image should can load on bitmap class.</param>
        Public Sub ChangeBackgroundImg(ImagePath As String)
            Dim TestBitMap As Bitmap
            Try
                TestBitMap = New Bitmap(ImagePath)
            Catch e As Exception
                Throw New Exception("Cannot load bitmap: " + e.Message + vbCrLf + e.StackTrace)
            End Try
            If TestBitMap.Width > 800 Then
                Throw New Exception("Width should 800 or less than 800.")
            End If
            If TestBitMap.Height > 480 Then
                Throw New Exception("Height should 480 or less than 480.")
            End If
            ChangeStateComThread(False)
            WritePipe("ChangeBackgroundImg," + ImagePath)
            Do While True
                Dim Message As String = TDTPPipeReader.ReadLine()
                If Message = "ImageChangeComplete" Then
                    ChangeStateComThread(True)
                    Exit Do
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Set button text.
        ''' </summary>
        ''' <param name="ButtonID">Button ID.</param>
        ''' <param name="Text">String to set.</param>
        Public Sub SetButtonText(ButtonID As Integer, Text As String)
            WritePipe($"ChangeButtonText,{ButtonID},{Text}")
        End Sub

        ''' <summary>
        ''' Disable button shortcut and enable button press event.
        ''' </summary>
        ''' <param name="Activate"><c>Activate</c> is boolean, disable or activate.</param>
        Public Sub ChangeShortcutMode(Activate As Boolean)
            WritePipe("ChangeShortcutEnabled," + (Not Activate).ToString())
            WritePipe("ChangeEventEnabled," + Activate.ToString())
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If TDTPPipeToSend.IsConnected Then TDTPPipeToSend.Close()
            If TDTPPipeToRead.IsConnected Then TDTPPipeToRead.Close()
            TDTPPipeToSend.Dispose()
            TDTPPipeToRead.Dispose()
            TDTPPipeReader.Close()
            TDTPPipeReader.Dispose()
            TDTPPipeWriter.Close()
            TDTPPipeWriter.Dispose()
            ComThread.Abort()
        End Sub
    End Class
End Namespace
