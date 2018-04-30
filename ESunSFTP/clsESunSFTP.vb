Imports System.IO
Public Class clsESunSFTP


    Public Function CheckTransFile(ByVal _strDir As String, ByVal _strDirDone As String) As DataTable
        '\\NPOSVRTW1X3\SSH Data\ESUN Data
        Dim aryFile As String() = Directory.GetFiles(_strDir, "*.7z")
        Dim oDT As New DataTable
        For Each strFilePath As String In aryFile

        Next
        Return oDT
    End Function

    '7z 7995

End Class
