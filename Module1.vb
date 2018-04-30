'參考要加 System.ServiceModel.Web.dll
'參考要加 System.Runtime.Serialization.dll
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json

Imports System.Configuration
Imports System.IO
Imports System.Text.Encoding
Imports System.Globalization
Imports System.Net
Imports NPCLibv2.udPDF
Imports System.Text
Imports iTextSharp.text.pdf
Imports iTextSharp.text.pdf.parser
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security

Module Module1
#Region "JSON 序列化"
    Friend Class clsJSONHelper
        Public Function ToJSON(ByVal _oVal As Object, ByVal _typObj As Type) As String
            Dim oJSON As New DataContractJsonSerializer(_typObj)
            Dim oBuf As New MemoryStream  '因為無法知道會寫多長，只好每次重新起始
            oJSON.WriteObject(oBuf, _oVal)
            Dim strOut As String = UTF8.GetString(oBuf.ToArray)
            oBuf.Close()
            oBuf.Dispose()
            oBuf = Nothing
            Return strOut
        End Function

        Public Function FromJSON(ByVal _strTest As String, ByVal _typObj As Type) As Object
            Throw New NotImplementedException("用到再處理")
            Dim oOutput As Object
            Dim oJSON As New DataContractJsonSerializer(_typObj)
            Dim oBuf As New MemoryStream  '因為無法知道會寫多長，只好每次重新起始
            Dim aryBuf As Byte() = UTF8.GetBytes(_strTest)
            oBuf.Write(aryBuf, 0, aryBuf.Count)
            oOutput = oJSON.ReadObject(oBuf)  '--這裡有問題，不過目前用不到，所以不處理
            oBuf.Close()
            oBuf.Dispose()
            oBuf = Nothing
            Return oOutput
        End Function
    End Class
    <DataContract()> _
    Friend Class clsTestObj
        Public Shared Function CreateTestObjArray() As clsTestObj()
            Dim lstTest As New ArrayList
            Dim oObj As clsTestObj
            For nI As Integer = 0 To 5
                oObj = New clsTestObj
                With oObj
                    .SN = nI
                    .Name = "Name" & nI
                    .Comment = "nothing"
                    .ProcTime = Now
                End With
                lstTest.Add(oObj)
            Next
            Dim aryObj As clsTestObj() = lstTest.ToArray(GetType(clsTestObj))
            Return aryObj
        End Function

#Region "variable/property"
        <DataMember()> _
        Public SN As Integer
        <DataMember()> _
        Public Name As String
        <DataMember()> _
        Public Comment As String
        <DataMember()> _
        Public ProcTime As DateTime
#End Region

    End Class
#End Region
    Sub Main()
        WebClientDL()
        'CopyPDF()
        'ReplaceSoftHyphen()
        'ExtractPDFText()

        'Dim strA As String = Nothing
        'If String.IsNullOrEmpty(strA) OrElse strA.Length < 4 Then
        '    ol("#1 !!")
        'End If
        'If String.IsNullOrEmpty(strA) Or strA.Length < 4 Then
        '    ol("#2 !!")
        'End If
        'GetTIPOTerm()
        'GetEnvInfo()
        'TestUnitTest()
        'TestPDF()
        'TestEmail()
        'TestSetting()

        'ol(GetMgrUIdByCustCode("IND", "15"))
        'ol(GetMgrUIdByCustCode("ind", "152"))
        'ol(GetMgrUIdByCustCode("AST", "0"))
        'ol(GetMgrUIdByCustCode("AST", "1"))
        'ol(GetMgrUIdByCustCode("AST", "9"))
        'ol(GetMgrUIdByCustCode("ast", "10"))
        'ol(GetMgrUIdByCustCode("aSt", "100"))
        'ol(GetMgrUIdByCustCode("asT", "1001"))


    End Sub

    ''' <summary>
    ''' Webs the client dl.
    ''' </summary>
    ''' <remarks>
    ''' 2017/10/11  Gilbert  看來USPTO有做檢查，WebClient無法直接取得，要加處理，Ian直接用IE Browser就好
    ''' </remarks>
    Private Sub WebClientDL()
        Dim oClient As New WebClient, oDoc As New HtmlAgilityPack.HtmlDocument
        Dim oNode As HtmlAgilityPack.HtmlNode, oNode2 As HtmlAgilityPack.HtmlNode, strTest As String
        Dim strUrl As String = "https://assignment.uspto.gov/patent/index.html#/patent/search/resultAbstract?id=9029703&type=patNum"
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf MyCallback)
        oClient.DownloadFile(strUrl, "d:\__buf\USPTOAssign_" & Now.ToString("yyyyMMdd_HHmmss") & ".txt") ' .DownloadString(strUrl)
        Dim strHTML As String = ""
        oDoc.LoadHtml(strHTML)
        ' GetElementbyId( )
        oNode = oDoc.GetElementbyId("printable-area")
        oNode2 = oNode.SelectSingleNode("./") '<h3 class="headline ng-binding">
        strTest = oNode2.InnerText  'Assignments (1 total)						
    End Sub

    Private Function MyCallback( _
 ByVal sender As Object, _
 ByVal certificate As X509Certificate, _
 ByVal chain As X509Chain, _
 ByVal sslPolicyErrors As SslPolicyErrors _
) As Boolean
        Return True
    End Function


    Private Sub CopyPDF()
        Dim strFileOutPath As String = "d:\w\NPCSrvAP.root\Internet\PVIGO.root\App_Data\Templete_PaymentOrder_out.pdf"
        File.Delete(strFileOutPath)
        Dim oReader As PdfReader = New PdfReader("d:\w\NPCSrvAP.root\Internet\PVIGO.root\App_Data\Templete_PaymentOrder.pdf")
        Dim oStamper As PdfStamper = New PdfStamper(oReader, File.OpenWrite(strFileOutPath))
        'stamper.setPdfVersion(PdfWriter.PDF_VERSION_1_4);
        oStamper.Close()

    End Sub

    ''' <summary>
    ''' Replaces the soft hyphen.
    ''' CS遇到REG0.20161201.­105322，-是0xC2 0xAD，utf-8 for "soft hyphen"，不是ASCII的- 0x2D
    ''' 參專案的test­.txt
    ''' </summary>
    Private Sub ReplaceSoftHyphen()
        Dim aryBuf As Byte() = {&H31, &H31, &H33, &H38, &H2D, &H2D, &H2D, &H33, &H38, &HC2, &HAD, &HC2, &HAD, &HC2, &HAD, &HC2, &HAD, &H31, &H32}
        Dim aryT As Byte() = {&HC2, &HAD}
        Dim strT As String = System.Text.Encoding.UTF8.GetString(aryT)
        Dim strTest As String = System.Text.Encoding.UTF8.GetString(aryBuf)
        Trace.WriteLine(strTest.Replace("-"c, ""))
        Trace.WriteLine(strTest.Replace("-", ""))
        Trace.WriteLine(strTest.Replace(strT, ""))
        Trace.WriteLine(strTest.Replace(strT, "").Replace("-", ""))
    End Sub

    Private Sub ExtractPDFText()
        'http://stackoverflow.com/questions/4711134/itextsharp-text-extraction
        Dim strPDFPath As String = "d:\download\MAC.pdf"
        Dim strTextOutPath As String = "d:\download\MAC.pdf.txt"
        File.Delete(strTextOutPath)
        Dim oReader As PdfReader = New PdfReader(strPDFPath)
        Dim oSW As StreamWriter = New StreamWriter(strTextOutPath)
        For nI As Integer = 1 To oReader.NumberOfPages
            oSW.WriteLine(PdfTextExtractor.GetTextFromPage(oReader, nI))
        Next

        oSW.Close()
        oSW.Dispose()
    End Sub
    Private Function GetMgrUIdByCustCode(ByVal _strCustCode As String, ByVal _strCustSer As String) As String
        Dim strMgrId As String = "", nCustSer As Integer = CInt(_strCustSer)
        _strCustCode = _strCustCode.ToUpper()
        If _strCustCode = "IND" Then
            strMgrId = _strCustCode & nCustSer.ToString("000")
        Else
            If nCustSer = 0 Then
                strMgrId = _strCustCode & "001"
            Else
                strMgrId = _strCustCode & "_" & nCustSer.ToString("00")
            End If
        End If
        Return strMgrId
    End Function

    Sub GetTIPOTerm()
        Dim strTest As String
        Using oClient As New WebClient
            With oClient
                strTest = Encoding.UTF8.GetString(.DownloadData("http://paterm.tipo.gov.tw/IPOTechTerm/doIPOTechTermIndex.do"))
                If strTest.IndexOf("請輸入驗證碼，大小寫不區分", StringComparison.Ordinal) Then
                    '進行驗證
                    .DownloadFile("http://paterm.tipo.gov.tw/IPOTechTerm/jcaptcha", "d:\download\t.tt")
                Else
                    '抓取資料
                End If
            End With
        End Using
    End Sub



    Sub GetEnvInfo()
        With My.Computer.Info.InstalledUICulture

            ol(CultureInfo.CurrentCulture.ToString()) '"zh-TW"
            ol(CultureInfo.CurrentUICulture.ToString()) '"zh-TW"
            ol(.DisplayName) '	"中文 (繁體，台灣)"	String
            ol(.EnglishName) '	"Chinese (Traditional, Taiwan)"	String
            ol(.IetfLanguageTag) '	"zh-TW"	String
            ol(CultureInfo.InstalledUICulture.ToString()) '	{zh-TW}	System.Globalization.CultureInfo
            ol(.Name) '	"zh-TW"	String
            ol(.NativeName) '	"中文(中華民國)"	String

        End With
    End Sub

    Sub TestUnitTest()
        Dim oT As New clsUnitTest
        oT.Go()
        oT.Close()
        oT = Nothing
    End Sub
    Sub TestPDF()
        Dim target As clsPDFUtil = New clsPDFUtil
        Dim _strSrcFilePath As String
        Dim _strDestFilePath As String

        Dim _strValueTest As String
        Dim _strValueNew As String
        Dim _bIgnoreCase As Boolean
        Dim expected As String
        Dim actual As String
        Dim enumReadonly As clsPDFUtil.DocChanged

        '#5 字型
        _strSrcFilePath = "c:\UT_Temp\Field_Font.pdf"
        _strDestFilePath = _strSrcFilePath & "a.pdf"
        File.Delete(_strDestFilePath)
        _strValueTest = "/Sign Here/"
        _strValueNew = "/WINSTON HSU/"
        _bIgnoreCase = True
        enumReadonly = clsPDFUtil.DocChanged.None
        expected = String.Empty '表示沒錯
        actual = target.FindFieldByValue(_strSrcFilePath, _strDestFilePath, _strValueTest, _strValueNew, _bIgnoreCase, enumReadonly)
        Dim bTest As Boolean = expected = actual
        bTest = File.Exists(_strDestFilePath)
        bTest = target.CountField(_strDestFilePath, _strValueNew, True) = 2
        bTest = True
    End Sub
    Function GetSet(ByVal _strKey As String) As String
        Return ConfigurationManager.AppSettings(_strKey)
    End Function
    Sub TestSetting()
        ol(GetSet("CommonVal_u"))
        ol(GetSet("CommonVal_a"))
        ol(GetSet("AppVal_u"))
        ol(GetSet("AppVal_a"))
        ol(GetSet("WebVal_u"))
        ol(GetSet("WebVal_a"))
        ol(GetSet("LibVal_u"))
        ol(GetSet("LibVal_a"))
    End Sub

    Sub TestEmail()
        Dim strTo As String = "gilbertkuo@naipo.com.tw; gjguo.csie@gmail.com; "
        Dim strFrom As String = "gilbertkuo@naipo.com.tw"
        strTo = strTo.Replace(";", ",").Trim
        If strTo.EndsWith(",") Then
            strTo = strTo.Substring(0, strTo.Length - 1)
        End If

        Dim oMail As New System.Net.Mail.MailMessage(strFrom, strTo, "test multi to", "test")
        Dim oClient As New System.Net.Mail.SmtpClient("nposvrtw1s11")
        oClient.Send(oMail)

        oClient.Send(strFrom, strTo, "test multi to 2", "test")
        oClient = Nothing
    End Sub



    Private Sub TestJson()
        '----------------------
        Dim aryTest As clsTestObj() = clsTestObj.CreateTestObjArray()
        Dim oJSON As New clsJSONHelper()
        Dim strJSON As String = oJSON.ToJSON(aryTest, aryTest.GetType())
        Trace.WriteLine(strJSON)
        '--------------------
        Trace.WriteLine("=====================")
        'Dim aryOutput As clsTestObj() = CType(oJSON.FromJSON(strJSON, aryTest.GetType()), clsTestObj())
        'For Each oTest As clsTestObj In aryOutput
        '    With oTest
        '        Trace.WriteLine(.SN)
        '        Trace.WriteLine(.Name)
        '        Trace.WriteLine(.Comment)
        '        Trace.WriteLine(.ProcTime)
        '        Trace.WriteLine("==============================")
        '    End With
        'Next
    End Sub


    Private Sub ol(ByVal _strMessage As String)
        Console.WriteLine(_strMessage)
        Trace.WriteLine(_strMessage)
    End Sub

    Private Sub o(ByVal _strMessage As String)
        Console.Write(_strMessage)
        Trace.Write(_strMessage)
    End Sub
End Module

