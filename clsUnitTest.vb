Imports WatiN.Core

Public Class clsUnitTest
    Private Const cstrIDMainFrame As String = "fraMain"
    Public Sub Go()
        TestLogin_1tx6()
        'TestLogin_1x6()
    End Sub

    Public Sub Close()

    End Sub

    Public Sub TestLogin_1tx6()
        BasicCheck("http://nposvrtw1tx6/custmem/login.aspx" _
                                , "na10030001", "ec001", "123456")
    End Sub

    Public Sub TestLogin_1x6()
        BasicCheck("https://eip.naipo.com/custmem/login.aspx" _
                        , "na10090007", "MIS_Gilbert", "123456")
    End Sub

    Private Sub BasicCheck(ByVal _strSiteUrl As String, ByVal _streCustID As String, ByVal _strAccount As String, ByVal _strPassword As String)
        Using browser As Browser = New IE(_strSiteUrl)

            browser.TextField(Find.ById("txteCustID")).TypeText(_streCustID)
            browser.TextField(Find.ById("txtUID")).TypeText(_strAccount)
            browser.TextField(Find.ById("txtPWD")).TypeText(_strPassword)

            browser.Button(Find.ById("btnLogin")).Click()

            If browser.ContainsText("第二重密碼") Then
                Dim oText As TextField
                For nI As Integer = 1 To 8
                    oText = browser.TextField(Find.ById("txtPWD2ND" & nI))
                    If oText.Value <> "＊" Then oText.TypeText(nI.ToString)
                Next

                browser.Button(Find.ById("btnCfmPWD2ND")).Click()
            End If
            Assert.IsTrue(browser.ContainsText("最新消息"))

            '基本資料 
            CheckFunction(browser, "memNAECPatentData", "商務型會員客戶清單")
            'oFrame.Image(Find.ByTitle("View")).Click()
            'Assert.IsTrue(browser.ContainsText("客戶資料"))
            '案件核准資料 
            CheckFunction(browser, "memNAECRptAllowCase", "案件核准資料查詢")
            CheckQuery(browser, "btnSearch", "案件核准資料查詢結果")

            '案件目前狀況資料 
            CheckFunction(browser, "memNAECRptCurrStatus", "案件目前狀況資料查詢") 'memNAECRptCaseStatus
            CheckQuery(browser, "btnSrh", "案件目前狀況資料查詢結果")

            '案件待確認資料 
            CheckFunction(browser, "memNAECRptWait4Conf", "案件待確認資料查詢")
            CheckQuery(browser, "btnSrh", "案件待確認資料查詢結果")

            '期限案件資料 
            CheckFunction(browser, "memNAECRptDueCase", "期限案件資料查詢")
            CheckQuery(browser, "btnSrh", "期限案件資料查詢結果")
            '[案件資訊]顯示方式 
            CheckFunction(browser, "memNAECCaseDisplay", "案件資訊功能")
            CheckQuery(browser, "btnConfCustName", "自訂欄位名稱")

            CheckFunction(browser, "memNAECPatentData", "商務型會員客戶清單") 'reset menu
            CheckFunction(browser, "memNAECCaseDisplay", "案件資訊功能")
            CheckQuery(browser, "btnConfDisplay", "自訂欄位顯示及顯示順序")

            CheckFunction(browser, "memNAECPatentData", "商務型會員客戶清單") 'reset menu
            CheckFunction(browser, "memNAECCaseDisplay", "案件資訊功能")
            CheckQuery(browser, "btnConfOrderby", "自訂資料排序")



            '個人基本資料 
            CheckFunction(browser, "memNAECMemData", "登入資訊")
            CheckQuery(browser, "btnNextP1", "專利知識")
            'btnSubmitUpd  '不測試避免資料更新

            '變更密碼 
            CheckFunction(browser, "memNAECChangePW", "變更 密碼(第一重)")
            '使用者管理 
            CheckFunction(browser, "memNAECMangMem", "狀態")
            '角色管理 
            CheckFunction(browser, "memNAECMangRole", "角色管理查詢")
            CheckQuery(browser, "btnSearch", "角色管理查詢結果")


            '設定角色權限 
            CheckFunction(browser, "memNAECUserAuth", "角色權限查詢")
            CheckQuery(browser, "btnSearch", "角色 ：")
            '設定個人權限
            CheckFunction(browser, "memNAECMemAuth", "個人權限查詢")
            CheckQuery(browser, "btnSearch", "設定個人權限")

            '登出
            CheckFunction(browser, "memNAECLogout", "環境偵測", False) '回到登入頁
        End Using
    End Sub


    Private Sub ClickButton(ByVal _oBrowser As Browser, ByVal _strBtnID As String, Optional ByVal _bInMainFrame As Boolean = True)
        With _oBrowser
            Dim oBtn As Button
            If _bInMainFrame Then
                Dim oFrame As Frame = _oBrowser.Frame(cstrIDMainFrame)
                oBtn = oFrame.Button(_strBtnID)
            Else
                oBtn = .Button(_strBtnID)
            End If

            Assert.IsTrue(oBtn.Exists)
            oBtn.Click()
        End With
    End Sub

    Private Sub CheckQuery(ByVal _oBrowser As Browser, ByVal _strBtnID As String, ByVal _strCheckText As String)
        ClickButton(_oBrowser, _strBtnID)
        CheckMainFrame(_oBrowser, _strCheckText)
    End Sub
    Private Sub CheckMainFrame(ByVal _oBrowser As Browser, ByVal _strCheckText As String)
        Dim oFrame As Frame = _oBrowser.Frame(cstrIDMainFrame)
        Assert.IsTrue(oFrame.ContainsText(_strCheckText))
    End Sub

    Private Sub Check(ByVal _oBrowser As Browser, ByVal _strCheckTest As String)
        Assert.IsTrue(_oBrowser.ContainsText(_strCheckTest))
    End Sub
    Private Sub CheckFunction(ByVal _oBrowser As Browser, ByVal _strMenuID As String, ByVal _strCheckText As String _
            , Optional ByVal _bInMainFrame As Boolean = True)
        ClickMenu(_oBrowser, _strMenuID)
        If _bInMainFrame Then
            CheckMainFrame(_oBrowser, _strCheckText)
        Else
            Check(_oBrowser, _strCheckText)
        End If

    End Sub

    Private Function ClickMenu(ByVal _oBrowser As Browser, ByVal _strMenuID As String) As Boolean
        Dim bFound As Boolean = False
        Dim oDiv As Div
        For Each oDiv In _oBrowser.Divs
            If Not String.IsNullOrEmpty(oDiv.Id) Then
                If String.Compare(oDiv.Id.Trim, _strMenuID) = 0 Then
                    bFound = True
                    'oDiv.Click()
                    oDiv.Elements.Item(0).Click()
                    Exit For
                End If
            End If
        Next

        Return bFound
    End Function
    Public Class Assert
        Public Shared Sub IsTrue(ByVal _bResult As Boolean)
            If Not _bResult Then
                Console.WriteLine("not Success")
            End If
        End Sub
    End Class
End Class
