﻿Imports System.ComponentModel

'打开Interner选项
'Shell("control.exe Inetcpl.cpl", vbNormalFocus)

Public Class XYBrowser
    Private Declare Function ReleaseCapture Lib "user32" () As Integer
    Private Declare Function SendMessageA Lib "user32" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, lParam As VariantType) As Integer
    Private Const DefaultPixelFormat As Imaging.PixelFormat = Imaging.PixelFormat.Format32bppArgb
    Private Const WS_THICKFRAME As Int32 = &H40000
    Private Const BorderWidth As Integer = 14 'WS_THICKFRAME 样式会让窗体产生14像素的边框
    Dim GoingToExit As Boolean
    Dim BtnRectangle() As Rectangle = {New Rectangle(0, 0, 28, 28), New Rectangle(28, 0, 28, 28), New Rectangle(56, 0, 28, 28), New Rectangle(84, 0, 28, 28)}
    Dim RestoreSize As Point = New Point(800, 600)
    Dim MouseDowned As Boolean
    Dim DistancePoint As Point
    Dim NowButton As Label
    Dim FullScreenMode As Boolean

    Private Sub XYBrowser_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Icon = My.Resources.XYBrowserRes.XYBrowser
        Btn_GoBack.Image = My.Resources.XYBrowserRes.GoBack.Clone(BtnRectangle(3), DefaultPixelFormat)
        Btn_GoForward.Image = My.Resources.XYBrowserRes.GoForward.Clone(BtnRectangle(3), DefaultPixelFormat)
        Btn_Refresh.Image = My.Resources.XYBrowserRes.Refresh.Clone(BtnRectangle(0), DefaultPixelFormat)
        Btn_Home.Image = My.Resources.XYBrowserRes.Home.Clone(BtnRectangle(0), DefaultPixelFormat)
        Btn_NowStop.Image = My.Resources.XYBrowserRes.NowStop.Clone(BtnRectangle(3), DefaultPixelFormat)
        Btn_GoNavigate.Image = My.Resources.XYBrowserRes.GoNavigate.Clone(BtnRectangle(0), DefaultPixelFormat)
        Btn_Search.Image = My.Resources.XYBrowserRes.Search.Clone(BtnRectangle(0), DefaultPixelFormat)
        BrowserAddress.Items.Add(IIf(Me.Tag = vbNullString, LauncherForm.MainHomeURL, Me.Tag))
        BrowserAddress.Text = BrowserAddress.Items(0).ToString
        MainWebBrowser.Navigate(BrowserAddress.Text)
    End Sub

    Private Sub XYBrowserMouseMove(sender As Object, e As MouseEventArgs) Handles BrowserState.MouseDown, BrowserTitle.MouseDown
        Call ReleaseCapture() '鼠标拖动
        Call SendMessageA(Me.Handle, &HA1, 2, 0&)
        If MousePosition.Y = 0 Then Me.WindowState = FormWindowState.Maximized
    End Sub

    Private Sub MainWebBrowser_NewWindow(sender As Object, e As CancelEventArgs) Handles MainWebBrowser.NewWindow
        '试图打开新网页
        e.Cancel = True '禁用新窗口打开
        Application.DoEvents()
        Dim NewUrl As String = MainWebBrowser.StatusText  '即将打开页面的地址赋予NewUrl
        If NewUrl <> "完成" Then LauncherForm.LoadNewBrowser(NewUrl)
    End Sub

    Private Sub MainWebBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles MainWebBrowser.DocumentCompleted
        '网页加载完成
        Btn_GoBack.Image = My.Resources.XYBrowserRes.GoBack.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoBack, 0, 3)), DefaultPixelFormat)
        Btn_GoForward.Image = My.Resources.XYBrowserRes.GoForward.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoForward, 0, 3)), DefaultPixelFormat)
        Btn_Refresh.Image = My.Resources.XYBrowserRes.Refresh.Clone(BtnRectangle(IIf(MainWebBrowser.Url Is Nothing, 3, 0)), DefaultPixelFormat)
        Btn_GoForward.Visible = True
        Btn_NowStop.Visible = False
        Dim TempString As String = MainWebBrowser.Document.Url.ToString
        BrowserAddress.Text = IIf(TempString.StartsWith("res://ieframe.dll/"), TempString.Substring(InStr(TempString, "#")), TempString)
        If Me.Text <> MainWebBrowser.DocumentTitle Then Me.Text = MainWebBrowser.DocumentTitle : BrowserTitle.Text = MainWebBrowser.DocumentTitle
        Application.DoEvents()
    End Sub

    Private Sub MainWebBrowser_DocumentTitleChanged(sender As Object, e As EventArgs) Handles MainWebBrowser.DocumentTitleChanged
        '网页标题改变
        If Me.Text <> MainWebBrowser.DocumentTitle Then Me.Text = MainWebBrowser.DocumentTitle : BrowserTitle.Text = MainWebBrowser.DocumentTitle
        Dim TempString As String = MainWebBrowser.Document.Url.ToString
        BrowserAddress.Text = IIf(TempString.StartsWith("res://ieframe.dll/"), TempString.Substring(InStr(TempString, "#")), TempString)
        Application.DoEvents()
    End Sub

    Private Sub MainWebBrowser_StatusTextChanged(sender As Object, e As EventArgs) Handles MainWebBrowser.StatusTextChanged
        '网页加载状态改变
        BrowserState.Text = MainWebBrowser.StatusText
        Application.DoEvents()
    End Sub

    Private Sub MainWebBrowser_Navigating(sender As Object, e As WebBrowserNavigatingEventArgs) Handles MainWebBrowser.Navigating
        '正在导航
        Btn_GoBack.Image = My.Resources.XYBrowserRes.GoBack.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoBack, 0, 3)), DefaultPixelFormat)
        Btn_GoForward.Image = My.Resources.XYBrowserRes.GoForward.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoForward, 0, 3)), DefaultPixelFormat)
        Btn_Refresh.Image = My.Resources.XYBrowserRes.Refresh.Clone(BtnRectangle(IIf(MainWebBrowser.Url Is Nothing, 3, 0)), DefaultPixelFormat)
        Btn_NowStop.Image = My.Resources.XYBrowserRes.NowStop.Clone(BtnRectangle(0), DefaultPixelFormat)
        Btn_NowStop.Visible = True
        Btn_GoForward.Visible = False
        Dim TempString As String = MainWebBrowser.Document.Url.ToString
        BrowserAddress.Text = IIf(TempString.StartsWith("res://ieframe.dll/"), TempString.Substring(InStr(TempString, "#")), TempString)
        Application.DoEvents()
    End Sub

    '点击浏览器按钮
    Private Sub BrowserButton_Click(sender As Object, e As EventArgs) Handles Btn_GoBack.Click, Btn_GoForward.Click, Btn_Home.Click, Btn_NowStop.Click, Btn_Refresh.Click
        NowButton = CType(sender, Label)
        Select Case NowButton.Tag
            Case "GoBack" '后退
                If MainWebBrowser.CanGoBack Then MainWebBrowser.GoBack()
            Case "GoForward" '前进
                If MainWebBrowser.CanGoForward Then MainWebBrowser.GoForward()
            Case "NowStop" '停止
                MainWebBrowser.Stop()
                Btn_GoBack.Image = My.Resources.XYBrowserRes.GoBack.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoBack, 0, 3)), DefaultPixelFormat)
                Btn_GoForward.Image = My.Resources.XYBrowserRes.GoForward.Clone(BtnRectangle(IIf(MainWebBrowser.CanGoForward, 0, 3)), DefaultPixelFormat)
                Btn_GoForward.Visible = True
                Btn_NowStop.Visible = False
                If Me.Text <> MainWebBrowser.DocumentTitle Then Me.Text = MainWebBrowser.DocumentTitle : BrowserTitle.Text = MainWebBrowser.DocumentTitle
            Case "Home" '主页
                MainWebBrowser.Navigate(LauncherForm.MainHomeURL)
            Case "Refresh" '刷新
                MainWebBrowser.Refresh()
        End Select
        Application.DoEvents()
    End Sub

    '点击标题栏按钮
    Private Sub TitleButton_Click(sender As Object, e As EventArgs) Handles Btn_Min.Click, Btn_Max.Click, Btn_Restore.Click, Btn_Close.Click, Btn_FullScreen.Click
        NowButton = CType(sender, Label)
        Select Case NowButton.Tag
            Case "Min" '最小化
                Me.WindowState = FormWindowState.Minimized
            Case "Max" '最大化
                Btn_Restore.Visible = True
                Btn_Max.Visible = False
                Me.WindowState = FormWindowState.Maximized
            Case "Restore" '还原
                Btn_Max.Visible = True
                Btn_Restore.Visible = False
                Me.WindowState = FormWindowState.Normal
            Case "FullScreen"
                If FullScreenMode Then '退出全屏
                    Me.WindowState = FormWindowState.Normal
                    Btn_FullScreen.Parent = TopPanel
                    Btn_FullScreen.BorderStyle = BorderStyle.None
                    Btn_FullScreen.Location = New Point(TopPanel.Width - 112, 0)
                    MainWebBrowser.Location = New Point(0, TopPanel.Bottom)
                    MainWebBrowser.Size = New Point(TopPanel.Width, BrowserState.Top - TopPanel.Bottom)
                Else '进入全屏
                    Me.WindowState = FormWindowState.Maximized
                    Btn_FullScreen.Parent = MainWebBrowser
                    Btn_FullScreen.BorderStyle = BorderStyle.FixedSingle
                    Btn_FullScreen.Location = New Point(Me.Width - Btn_FullScreen.Width - BorderWidth, 0)
                    MainWebBrowser.Location = New Point(0, 0)
                    MainWebBrowser.Size = New Size(Me.Width - BorderWidth, Me.Height - BorderWidth)
                    Btn_FullScreen.BringToFront()
                End If
                Btn_FullScreen.Image = My.Resources.XYBrowserRes.FullScreen_N
                FullScreenMode = Not FullScreenMode
            Case "Close" '关闭
                CloseBrowser()
        End Select
    End Sub

    Private Sub XYBrowser_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If GoingToExit Then Exit Sub
        e.Cancel = True
        CloseBrowser()
    End Sub

    Private Sub CloseBrowser()
        GoingToExit = True
        MainWebBrowser.Stop()
        MainWebBrowser.Navigate("about:blank ")
        MainWebBrowser.Dispose()
        LauncherForm.LoadedBrowserCount -= 1
        If LauncherForm.LoadedBrowserCount = 0 Then End Else Me.Close()
    End Sub

    '浏览器模块：“前进、后退、主页、停止、刷新”按钮响应鼠标的动态效果
    Private Sub BrowserButton_MouseEnter(sender As Object, e As EventArgs) Handles Btn_GoBack.MouseEnter, Btn_GoForward.MouseEnter, Btn_Home.MouseEnter, Btn_NowStop.MouseEnter, Btn_Refresh.MouseEnter, Btn_GoNavigate.MouseEnter, Btn_Search.MouseEnter
        NowButton = CType(sender, Label)
        If (Not MainWebBrowser.CanGoBack And NowButton Is Btn_GoBack) Or (Not MainWebBrowser.CanGoForward And NowButton Is Btn_GoForward) Then Exit Sub
        NowButton.Image = CType(My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag), Bitmap).Clone(BtnRectangle(1), DefaultPixelFormat)
    End Sub

    Private Sub BrowserButton_MouseLeave(sender As Object, e As EventArgs) Handles Btn_GoBack.MouseLeave, Btn_GoForward.MouseLeave, Btn_Home.MouseLeave, Btn_NowStop.MouseLeave, Btn_Refresh.MouseLeave, Btn_GoNavigate.MouseLeave, Btn_Search.MouseLeave
        NowButton = CType(sender, Label)
        If (Not MainWebBrowser.CanGoBack And NowButton Is Btn_GoBack) Or (Not MainWebBrowser.CanGoForward And NowButton Is Btn_GoForward) Then Exit Sub
        NowButton.Image = CType(My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag), Bitmap).Clone(BtnRectangle(0), DefaultPixelFormat)
    End Sub

    Private Sub BrowserButton_MouseDown(sender As Object, e As MouseEventArgs) Handles Btn_GoBack.MouseDown, Btn_GoForward.MouseDown, Btn_Home.MouseDown, Btn_NowStop.MouseDown, Btn_Refresh.MouseDown, Btn_GoNavigate.MouseDown, Btn_Search.MouseDown
        NowButton = CType(sender, Label)
        If (Not MainWebBrowser.CanGoBack And NowButton Is Btn_GoBack) Or (Not MainWebBrowser.CanGoForward And NowButton Is Btn_GoForward) Then Exit Sub
        NowButton.Image = CType(My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag), Bitmap).Clone(BtnRectangle(2), DefaultPixelFormat)
    End Sub

    Private Sub BrowserButton_MouseUp(sender As Object, e As MouseEventArgs) Handles Btn_GoBack.MouseUp, Btn_GoForward.MouseUp, Btn_Home.MouseUp, Btn_NowStop.MouseUp, Btn_Refresh.MouseUp, Btn_GoNavigate.MouseUp, Btn_Search.MouseUp
        NowButton = CType(sender, Label)
        If (Not MainWebBrowser.CanGoBack And NowButton Is Btn_GoBack) Or (Not MainWebBrowser.CanGoForward And NowButton Is Btn_GoForward) Then Exit Sub
        NowButton.Image = CType(My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag), Bitmap).Clone(BtnRectangle(1), DefaultPixelFormat)
    End Sub

    '系统标题栏：“全屏、最小化、最大化、还原、关闭”按钮响应鼠标的动态效果
    Private Sub TitleButton_MouseEnter(sender As Object, e As EventArgs) Handles Btn_Min.MouseEnter, Btn_Max.MouseEnter, Btn_Restore.MouseEnter, Btn_Close.MouseEnter, Btn_FullScreen.MouseEnter
        NowButton = CType(sender, Label)
        NowButton.Image = My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag & "_E")
    End Sub

    Private Sub TitleButton_MouseLeave(sender As Object, e As EventArgs) Handles Btn_Min.MouseLeave, Btn_Max.MouseLeave, Btn_Restore.MouseLeave, Btn_Close.MouseLeave, Btn_FullScreen.MouseLeave
        NowButton = CType(sender, Label)
        NowButton.Image = My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag & "_N")
    End Sub

    Private Sub TitleButton_MouseDown(sender As Object, e As MouseEventArgs) Handles Btn_Min.MouseDown, Btn_Max.MouseDown, Btn_Restore.MouseDown, Btn_Close.MouseDown, Btn_FullScreen.MouseDown
        NowButton = CType(sender, Label)
        NowButton.Image = My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag & "_D")
    End Sub

    Private Sub TitleButton_MouseUp(sender As Object, e As MouseEventArgs) Handles Btn_Min.MouseUp, Btn_Max.MouseUp, Btn_Restore.MouseUp, Btn_Close.MouseUp, Btn_FullScreen.MouseUp
        NowButton = CType(sender, Label)
        NowButton.Image = My.Resources.XYBrowserRes.ResourceManager.GetObject(NowButton.Tag & "_E")
    End Sub

    '窗体大小改变，重新规划控件位置
    Private Sub XYBrowser_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Normal And Btn_Restore.Visible Then Btn_Max.Show() : Btn_Restore.Hide()
        If Me.WindowState = FormWindowState.Maximized And Btn_Max.Visible Then Btn_Restore.Show() : Btn_Max.Hide()
        TopPanel.Location = New Point(0, 0)
        TopPanel.Width = Me.Width - BorderWidth
        Btn_FullScreen.Left = TopPanel.Width - 112
        Btn_Min.Left = Btn_FullScreen.Right
        Btn_Restore.Left = Btn_Min.Right
        Btn_Max.Left = Btn_Restore.Left
        Btn_Close.Left = Btn_Max.Right

        BrowserTitle.Width = Btn_FullScreen.Left
        BrowserAddress.Width = TopPanel.Width * 0.6
        Btn_GoNavigate.Left = BrowserAddress.Right
        Btn_Search.Left = TopPanel.Width - 28
        SearchTextBox.Left = Btn_GoNavigate.Right
        SearchTextBox.Width = Btn_Search.Left - SearchTextBox.Left

        BrowserState.Location = New Point(0, Me.Height - BrowserState.Height - BorderWidth)
        BrowserState.Width = TopPanel.Width
        MainWebBrowser.Location = New Point(0, TopPanel.Bottom)
        MainWebBrowser.Size = New Point(TopPanel.Width, BrowserState.Top - TopPanel.Bottom)
    End Sub

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get '浏览器控件会使 WS_THICKFRAME 样式失效，所以需要监听
            If Not DesignMode Then
                Dim cp As CreateParams = MyBase.CreateParams
                cp.Style = cp.Style Or WS_THICKFRAME
                Return cp
            Else
                Return MyBase.CreateParams
            End If
        End Get
    End Property

    '百度搜索功能
    Private Sub Btn_Search_Click(sender As Object, e As EventArgs) Handles Btn_Search.Click
        MainWebBrowser.Navigate("http://www.baidu.com/s?wd=" & IIf(SearchTextBox.Text = "点此搜索...", vbNullString, SearchTextBox.Text))
    End Sub

    Private Sub SearchTextBox_KeyPress(sender As Object, e As KeyPressEventArgs) Handles SearchTextBox.KeyPress
        If Asc(e.KeyChar) = 13 Then MainWebBrowser.Navigate("http://www.baidu.com/s?wd=" & SearchTextBox.Text)
    End Sub
    Private Sub SearchTextBox_LostFocus(sender As Object, e As EventArgs) Handles SearchTextBox.LostFocus
        SearchTextBox.ForeColor = Color.DimGray
        If SearchTextBox.Text = vbNullString Then SearchTextBox.Text = "点此搜索..."
    End Sub
    Private Sub SearchTextBox_GotFocus(sender As Object, e As EventArgs) Handles SearchTextBox.GotFocus
        SearchTextBox.ForeColor = Color.Black
        If SearchTextBox.Text = "点此搜索..." Then SearchTextBox.Text = vbNullString
    End Sub

    '地址栏导航
    Private Sub Btn_GoNavigate_Click(sender As Object, e As EventArgs) Handles Btn_GoNavigate.Click
        NavigateToAddress()
    End Sub
    Private Sub BrowserAddress_KeyPress(sender As Object, e As KeyPressEventArgs) Handles BrowserAddress.KeyPress
        If Asc(e.KeyChar) = 13 Then NavigateToAddress()
    End Sub
    Private Sub NavigateToAddress()
        Dim TempString As String = BrowserAddress.Text
        Dim Index As Integer = BrowserAddress.Items.IndexOf(BrowserAddress.Text)
        If Index > -1 Then BrowserAddress.Items.RemoveAt(Index) : BrowserAddress.Text = TempString
        BrowserAddress.Items.Insert(0, BrowserAddress.Text)
        MainWebBrowser.Navigate(TempString)
    End Sub
    Private Sub BrowserAddress_GotFocus(sender As Object, e As EventArgs) Handles BrowserAddress.GotFocus
        BrowserAddress.ForeColor = Color.Black
    End Sub
    Private Sub BrowserAddress_LostFocus(sender As Object, e As EventArgs) Handles BrowserAddress.LostFocus
        BrowserAddress.ForeColor = Color.DimGray
    End Sub

End Class
