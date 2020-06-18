Module Module1

    Public calibIndex As Double

End Module

Public Class Area
    Private Declare Auto Function BitBlt Lib "GDI32.DLL" (
     ByVal hdcDest As IntPtr,
     ByVal nXDest As Integer,
     ByVal nYDest As Integer,
     ByVal nWidth As Integer,
     ByVal nHeight As Integer,
     ByVal hdcSrc As IntPtr,
     ByVal nXSrc As Integer,
     ByVal nYSrc As Integer,
     ByVal dwRop As Int32) As Boolean

    Private Const SRCCOPY = &HCC0020

    Public Function Copy(ByVal srcPB As PictureBox, ByVal rectRF As RectangleF) As Bitmap
        Dim srcPic As Graphics = srcPB.CreateGraphics
        Dim srcBmp As New Bitmap(srcPB.Width, srcPB.Height, srcPic)
        Dim srcMem As Graphics = Graphics.FromImage(srcBmp)
        Dim PicHdc As IntPtr = srcPic.GetHdc
        Dim MemHdc As IntPtr = srcMem.GetHdc
        BitBlt(MemHdc, 0, 0, rectRF.Width, rectRF.Height, PicHdc, rectRF.X, rectRF.Y, SRCCOPY)
        Copy = srcBmp.Clone()
        srcPic.ReleaseHdc(PicHdc)
        srcMem.ReleaseHdc(MemHdc)
        srcPic.Dispose()
        srcBmp.Dispose()
        srcMem.Dispose()
        srcPic = Nothing
        srcBmp = Nothing
        srcMem = Nothing
        PicHdc = Nothing
        MemHdc = Nothing
        GC.Collect()
    End Function
End Class

Public Class Trace
    Public Sub Graph(ByVal Grph As Graphics, ByVal PenColor As Pen, ByVal X1 As Integer, ByVal Y1 As Integer, ByVal X2 As Integer, ByVal Y2 As Integer)
        Grph.DrawLine(PenColor, X1, Y1, X2, Y2)
    End Sub
End Class