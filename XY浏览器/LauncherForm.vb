Public Class LauncherForm
    Public Const MainHomeURL As String = "http://www.baidu.com/"
    Public LoadedBrowserCount As Integer = 0

    Public Sub LoadNewBrowser(Optional ByVal HomeURL As String = MainHomeURL)
        Dim NewXYBrowser As Form = New XYBrowser
        NewXYBrowser.Tag = HomeURL
        LoadedBrowserCount += 1
        NewXYBrowser.Show()
        NewXYBrowser.Focus()
    End Sub

    Private Sub LauncherForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadNewBrowser()
    End Sub

    Private Sub LauncherForm_VisibleChanged(sender As Object, e As EventArgs) Handles Me.VisibleChanged
        Me.Hide()
    End Sub
End Class