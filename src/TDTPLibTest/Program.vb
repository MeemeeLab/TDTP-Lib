Imports TDTPLib.Controller

Module Program
    Sub Main(args As String())
        Dim TDTPConnector As TDTPConnector = New TDTPConnector()
        'TDTPConnector.ChangeBackgroundImg("C:\img1.jpg")
        TDTPConnector.SetButtonText(1, "Test.")
        AddHandler TDTPConnector.SortcutPressEvent, Sub(Val As Integer)
                                                        Console.WriteLine($"Button {Val} touched.")
                                                    End Sub
        TDTPConnector.ChangeShortcutMode(True)
        Console.ReadKey()
    End Sub
End Module
