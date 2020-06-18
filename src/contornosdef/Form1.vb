Imports Emgu.CV
Imports Emgu.CV.Structure
Imports System.IO

Public Class Form1

    Dim imgEntrada, imgEntradaOriginal As Image(Of Bgr, Byte)
    Dim imgSaida As Image(Of Gray, Byte)
    Dim hierarq As New Mat
    Dim IsBlur, IsMedian, IsGaussian, GrayS As Boolean
    Dim wBlur, hBlur, kGauss, sizeMedian, cannyThreshold, cannyThresholdLinking As Integer
    Dim minor As Byte
    Dim major As Byte

    'rotation control
    Dim rotCCW, rotCW As Integer
    '-----------

    'Vars for ROI selection on picture1
    Dim XStart, YStart As Integer
    Dim XCurrent, YCurrent As Integer
    Dim selRET As Rectangle
    '------------------------------------------------

    'Vars for video control
    Dim cap As VideoCapture
    Dim totalFrames As Integer
    Dim currentFrameNo As Integer
    Dim currentFrame As New Mat
    Dim FPS As Integer
    '------------------------------------------------

    'Vars for Dataset
    Dim dsParâmetros As DataSet = New DataSet("dsParâmetros")
    Dim varRet As DataTable = dsParâmetros.Tables.Add("varRet")
    Dim posTempo As Double
    Dim posFrame As Double
    Dim IniX As Integer
    Dim IniY As Integer
    Dim testingX As Integer = 0
    Dim HeightCalib As Double
    Dim WidthCalib As Double
    Dim RetAreaCalib As Double
    Dim ElipAreaCalib As Double
    Dim RetAlt As Double
    Dim RetLarg As Double
    Dim Arrea As Double 'rectangle area in px
    Dim ArreaElip As Integer
    '----------------------------------------------------

    '====================== Preview =====================
    Private gr, gr2 As Graphics
    Private TR As Trace = New Trace
    Dim grbuff As Bitmap
    Dim ngraph As Integer
    Dim Step_X As Integer
    Dim prevX, prevY As Integer
    Dim actX, actY As Integer
    Dim graphXY(2, 0) As Integer
    Dim W_Signal0, H_Signal0, Q_Signal0 As Integer
    Dim maxY, minY As Integer
    Dim zerou As Boolean = False
    '====================================================

    Dim tratamento As String
    Dim iniFrame As Integer
    Dim rotacaoOn As Boolean = False

    Public Function CannyDetec()
        If GrayS Then
            imgSaida = imgEntrada.Convert(Of Gray, Byte).PyrDown.PyrUp() 'standard gray scale
        Else
            imgSaida = imgEntrada.Convert(Of Gray, Byte).ThresholdBinary(New Gray(minor), New Gray(major)) 'threshold gray scale
        End If

        If Not selRET = Nothing Then 'was the ROI defined?
            imgSaida.ROI = selRET 'create ROI
        End If

        If IsBlur Then imgSaida.SmoothBlur(wBlur, hBlur).CopyTo(imgSaida)
        If IsMedian Then imgSaida.SmoothMedian(sizeMedian).CopyTo(imgSaida)
        If IsGaussian Then imgSaida.SmoothGaussian(kGauss).CopyTo(imgSaida)

        imgSaida = imgSaida.Canny(cannyThreshold, cannyThresholdLinking)

        Dim contornos As New Emgu.CV.Util.VectorOfVectorOfPoint
        Dim imgContorno As New Image(Of Bgr, Byte)(imgEntrada.Width, imgEntrada.Height, New Bgr(0, 0, 0))

        If Not selRET = Nothing Then 'was the ROI defined?
            CvInvoke.FindContours(imgSaida, contornos, hierarq, Emgu.CV.CvEnum.RetrType.External, CvEnum.ChainApproxMethod.ChainApproxSimple, selRET.Location)
        Else
            CvInvoke.FindContours(imgSaida, contornos, hierarq, Emgu.CV.CvEnum.RetrType.External, CvEnum.ChainApproxMethod.ChainApproxSimple)
        End If

        CvInvoke.DrawContours(imgContorno, contornos, -1, New MCvScalar(255, 255, 255))

        'Start detection:
        Try
            Dim teste As New Emgu.CV.Util.VectorOfVectorOfPoint
            Dim perimetro, bigestPER As Double
            bigestPER = 0
            Dim bigestINDX As Integer 'index to the bigest rectangle
            bigestINDX = 0
            Dim retangulo As New List(Of Rectangle)() 'list to store the detected rectangles
            Dim fatorTamnho As Double
            Dim rectUnion As New Rectangle

            'detect all rectangles and store them
            For i = 0 To contornos.Size - 1
                retangulo.Add(CvInvoke.BoundingRectangle(contornos(i))) 'add a new in the list with index i
                perimetro = (retangulo(i).Width * 2) + (retangulo(i).Height * 2) 'calculate the perimeter of the new rectangle
                If perimetro > bigestPER Then 'Is the new rectangle the bigest one?
                    bigestPER = perimetro 'If so, add it as the bigest.
                    bigestINDX = i 'store the index to find the bigest rectangle in the list 
                End If
            Next
            '----------------------------

            fatorTamnho = Convert.ToDouble(FatorTamTB.Text) / 100 'define how big the rectangle must be to be considered

            rectUnion = retangulo(bigestINDX) 'create a rectangle from the union of all considered rectangles

            'search for rectangles that meet the minimum size to be considered
            For Each R As Rectangle In retangulo
                perimetro = (R.Width * 2) + (R.Height * 2)
                If perimetro >= fatorTamnho * bigestPER Then

                    rectUnion = Rectangle.Union(rectUnion, R)

                End If
            Next

            CvInvoke.Rectangle(imgContorno, rectUnion, New MCvScalar(255, 0, 0), 2) 'draw the rectangle
            '--------------------------

            'draw the ellipse:
            'define a box to meet the size and location of the detected rectangle
            Dim boxELIP As RotatedRect
            boxELIP.Center.X = rectUnion.X + (rectUnion.Width / 2)
            boxELIP.Center.Y = rectUnion.Y + (rectUnion.Height / 2)
            boxELIP.Size.Height = rectUnion.Width
            boxELIP.Size.Width = rectUnion.Height
            '--------------------
            CvInvoke.Ellipse(imgContorno, boxELIP, New MCvScalar(0, 255, 0), 2) 'draw the ellipse
            '------------------------------------------

            'Draw the rectangle on the picturebox1:
            PictureBox1.Image = imgEntrada.Bitmap  'clear picturebox1
            Dim imageTest As New Image(Of Bgr, Byte)(PictureBox1.Image)
            CvInvoke.Rectangle(imageTest, rectUnion, New MCvScalar(255, 0, 0), 2)
            CvInvoke.Ellipse(imageTest, boxELIP, New MCvScalar(0, 255, 0), 2)
            PictureBox1.Image = imageTest.Bitmap
            '---------------------------------------

            'register important parameters:
            IniX = rectUnion.X
            IniY = rectUnion.Y
            Arrea = rectUnion.Height * rectUnion.Width
            RetAlt = rectUnion.Height
            RetLarg = rectUnion.Width
            ArreaElip = (boxELIP.Size.Height / 2) * (boxELIP.Size.Width / 2) * Math.PI
            HeightCalib = Math.Round(RetAlt / calibIndex, 6)
            WidthCalib = Math.Round(RetLarg / calibIndex, 6)
            RetAreaCalib = Math.Round(Arrea / calibIndex, 6)
            ElipAreaCalib = Math.Round(ArreaElip / calibIndex, 6)

        Catch ex As ArgumentOutOfRangeException
            '========== if no rectangle is detected in the frame =============
            IniX = 0
            IniY = 0
            HeightCalib = 0
            WidthCalib = 0
            RetAreaCalib = 0
            ElipAreaCalib = 0
            RetAlt = 0
            RetLarg = 0
            Arrea = 0
            ArreaElip = 0
            '==========

        Catch ex As Exception

            MsgBox(ex.ToString)

        End Try

        If Not selRET = Nothing Then 'was the ROI defined?
            imgSaida.ROI = Rectangle.Empty 'clar ROI
        End If

        PictureBox2.Image = imgContorno.Bitmap
        Return True
    End Function

    Public Function TheWriter()
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim newTXT As StreamWriter
            Dim lineTXT As String = ""
            newTXT = New StreamWriter(SaveFileDialog1.FileName)

            'Create a .txt fle in which the first line contain
            'the title of each column
            lineTXT = OpenFileDialog1.FileName & vbNewLine & "Duration(s): " & Math.Round(posTempo / 1000, 5) &
                " | " & "Frame count: " & totalFrames & " | " & "Initial Frame: " _
                & iniFrame & " | " & "Rotation: " & (rotCW + rotCCW) & "°" & vbNewLine
            newTXT.WriteLine(lineTXT)
            lineTXT = ""

            lineTXT = "Applied Parameters" & vbNewLine & "Gray Scale - Minor: " & minor & " | " & "Major: " & major _
            & " | " & "CannyThreshold: " & cannyThreshold & " | " & "ThresholdLinking: " & cannyThresholdLinking & vbNewLine _
            & "Image treatment: " & tratamento & " | " & "P.S.: Not applicable if no image treatment was used." & vbNewLine _
            & "Blur - Width: " & wBlur & " | " & "Height: " & hBlur & " | " & "Size Median: " & sizeMedian & " | " & "kGauss: " & kGauss & vbNewLine _
            & "Calibration Index: " & calibIndex & " | " & "Factor: " & Convert.ToDouble(FatorTamTB.Text)
            newTXT.WriteLine(lineTXT)
            lineTXT = ""

            lineTXT = "PosTime" & vbTab & "FrameNo" & vbTab & "iniX (px)" & vbTab & "iniY (px)" & vbTab & "Height (nm)" & vbTab & "Width (nm)" &
                vbTab & "Ret. Area (nm2)" & vbTab & "Elip. Area (nm2)" & vbTab & "Height (px)" & vbTab & "Width (px)" & vbTab &
                "Ret. Area (px2)" & vbTab & "Elip. Area (px2)"
            newTXT.WriteLine(lineTXT)
            lineTXT = ""

            For Each dr As DataRow In varRet.Rows

                For Each dc As DataColumn In varRet.Columns

                    If lineTXT = "" Then
                        lineTXT = dr(dc.ColumnName).ToString
                    Else
                        lineTXT = lineTXT & vbTab & dr(dc.ColumnName).ToString
                    End If

                Next

                newTXT.WriteLine(lineTXT)
                lineTXT = ""

            Next

            newTXT.Close() 'close the created file
            newTXT.Dispose() 'release the used memory

            Me.Cursor = Cursors.Default

        Catch ex As Exception

            Me.Cursor = Cursors.Default
            MsgBox("ERROR: " & ex.Message)
        End Try


        Me.Cursor = Cursors.Default
        Return True
    End Function

    '======================================Configuration managing functions================================================
    Public Function ConfigWriter()

        Me.Cursor = Cursors.WaitCursor
        Dim configTXT As StreamWriter
        Dim lineTXT As String = ""
        configTXT = New StreamWriter(SaveFileDialog2.FileName)

        'Creates a TXT file where each line is a configurable parameter, so the user can save used settings for a future experiment
        lineTXT = "cannyThreshold: " & cannyThreshold & vbNewLine &
                  "thresholdLinking: " & cannyThresholdLinking & vbNewLine &
                  "standartGrayScaleCheck: " & RadioButton1.Checked & vbNewLine &
                  "customGrayScaleCheck: " & RadioButton2.Checked & vbNewLine &
                  "minorGrayScale: " & minor & vbNewLine &
                  "majorGrayScale: " & major & vbNewLine &
                  "blurBeingUsed: " & CheckBox1.Checked & vbNewLine &
                  "medianBeingUsed: " & CheckBox2.Checked & vbNewLine &
                  "gaussianBeingUsed: " & CheckBox3.Checked & vbNewLine &
                  "noTreatment: " & CheckBox4.Checked & vbNewLine &
                  "blurWidth: " & wBlur & vbNewLine &
                  "blurHeight: " & hBlur & vbNewLine &
                  "sizeMedian: " & sizeMedian & vbNewLine &
                  "kGauss: " & kGauss & vbNewLine &
                  "sizeFactor: " & FatorTamTB.Text & vbNewLine &
                  "calibrationFactor: " & calibIndex & vbNewLine &
                  "Please: " & "note that all parameters were exported to this file. If you didn't alter some of them, the default values were 
                  exported instead."

        configTXT.WriteLine(lineTXT)
        lineTXT = ""

        configTXT.Close() 'close the created file
        configTXT.Dispose() 'release the used memory

        Me.Cursor = Cursors.Default
        Return True

    End Function

    Public Function ConfigReader(ByVal File As String, ByVal Identifier As String) As String

        Dim S As New IO.StreamReader(File)
        Dim Result As String = ""

        Do While (S.Peek <> -1)
            Dim Line As String = S.ReadLine
            If Line.ToLower.StartsWith(Identifier.ToLower & ":") Then
                Result = Line.Substring(Identifier.Length + 2)
            End If
        Loop

        Return Result

    End Function
    '=====================================================================================================================

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Define the "default" values for each var:

        GrayS = True
        minor = 100
        major = 255

        rotCCW = 0
        rotCW = 0

        IsBlur = False
        IsMedian = False
        IsGaussian = False

        wBlur = 10
        hBlur = 10
        kGauss = 5
        sizeMedian = 5

        cannyThreshold = 25 '180
        cannyThresholdLinking = 15 '150

        TextBox1.Text = cannyThreshold
        TextBox2.Text = cannyThresholdLinking
        FatorTamTB.Text = 50
        TextBox4.Text = 100
        TextBox5.Text = 255
        BlurWidthTB.Text = 10
        BlurHeightTB.Text = 10
        NumericUpDown1.Value = sizeMedian
        NumericUpDown2.Value = kGauss
        '------------------------

        '----- load the dataset -----
        With varRet

            .Columns.Add("posTempo", System.Type.GetType("System.Double"))
            .Columns.Add("frameNo", System.Type.GetType("System.Double"))
            .Columns.Add("iniX", System.Type.GetType("System.Double"))
            .Columns.Add("iniY", System.Type.GetType("System.Double"))
            .Columns.Add("HeightCalib", System.Type.GetType("System.Double"))
            .Columns.Add("WidthCalib", System.Type.GetType("System.Double"))
            .Columns.Add("RetAreaCalib", System.Type.GetType("System.Double"))
            .Columns.Add("ElipAreaCalib", System.Type.GetType("System.Double"))
            .Columns.Add("Altura", System.Type.GetType("System.Double"))
            .Columns.Add("Largura", System.Type.GetType("System.Double"))
            .Columns.Add("AreaRet", System.Type.GetType("System.Double"))
            .Columns.Add("AreaEli", System.Type.GetType("System.Double"))

        End With

        '---------------------

        tratamento = "None"
        SalvarNovamenteToolStripMenuItem.Visible = 0
        Label13.Text = "Selected:" & vbNewLine & tratamento

        Label14.Text = ""

        'Avoid errors with decimal separators in different languages:
        'It is used the US standard (. for decimal separator)
        System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US", False)
        '---------------------------------------------

    End Sub


    'Treatment selection controls section**********************************************************************************
    Private Sub GroupBox1_Enter(sender As Object, e As EventArgs) Handles Treatment.Enter
        Me.Name = "Image treatment"
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Me.Name = "Blur"
        If CheckBox1.Checked Then

            IsBlur = True
            IsMedian = False
            IsGaussian = False

            CheckBox2.Enabled = 0
            CheckBox3.Enabled = 0
            CheckBox4.Enabled = 0

            tratamento = "Blur"
        Else
            IsBlur = False
            IsMedian = False
            IsGaussian = False

            CheckBox2.Enabled = 1
            CheckBox3.Enabled = 1
            CheckBox4.Enabled = 1
            CheckBox2.CheckState = 0
            CheckBox3.CheckState = 0
            CheckBox4.CheckState = 0

            tratamento = "None"
        End If

        Label13.Text = "Selected:" & vbNewLine & tratamento

    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged
        Me.Name = "Median"
        If CheckBox2.Checked Then
            IsBlur = False
            IsMedian = True
            IsGaussian = False

            CheckBox1.Enabled = 0
            CheckBox3.Enabled = 0
            CheckBox4.Enabled = 0

            tratamento = "Median"

        Else
            IsBlur = False
            IsMedian = False
            IsGaussian = False

            CheckBox1.Enabled = 1
            CheckBox3.Enabled = 1
            CheckBox4.Enabled = 1
            CheckBox1.CheckState = 0
            CheckBox3.CheckState = 0
            CheckBox4.CheckState = 0

            tratamento = "None"

        End If

        Label13.Text = "Selected:" & vbNewLine & tratamento

    End Sub

    Private Sub CheckBox3_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox3.CheckedChanged
        Me.Name = "Gaussian"
        If CheckBox3.Checked Then
            IsBlur = False
            IsMedian = False
            IsGaussian = True

            CheckBox1.Enabled = 0
            CheckBox2.Enabled = 0
            CheckBox4.Enabled = 0

            tratamento = "Gaussian"

        Else
            IsBlur = False
            IsMedian = False
            IsGaussian = False

            CheckBox1.Enabled = 1
            CheckBox2.Enabled = 1
            CheckBox4.Enabled = 1
            CheckBox1.CheckState = 0
            CheckBox2.CheckState = 0
            CheckBox4.CheckState = 0

            tratamento = "None"

        End If

        Label13.Text = "Selected:" & vbNewLine & tratamento

    End Sub

    Private Sub CheckBox4_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox4.CheckedChanged
        Me.Name = "None"
        If CheckBox4.Checked Then
            IsBlur = False
            IsMedian = False
            IsGaussian = False

            CheckBox1.Enabled = 0
            CheckBox2.Enabled = 0
            CheckBox3.Enabled = 0

            tratamento = "None"

        Else
            IsBlur = False
            IsMedian = False
            IsGaussian = False

            CheckBox1.Enabled = 1
            CheckBox2.Enabled = 1
            CheckBox3.Enabled = 1
            CheckBox1.CheckState = 0
            CheckBox2.CheckState = 0
            CheckBox3.CheckState = 0

            tratamento = "None"

        End If

        Label13.Text = "Selected:" & vbNewLine & tratamento

    End Sub

    '**********************************************************************************************************************

    Private Sub AbrirToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AbrirToolStripMenuItem1.Click

        With OpenFileDialog1
            If OpenFileDialog1.ShowDialog() = DialogResult.OK Then

                Dim cap As New VideoCapture(OpenFileDialog1.FileName)
                totalFrames = cap.GetCaptureProperty(CvEnum.CapProp.FrameCount)

                currentFrameNo = 0
                cap.SetCaptureProperty(1, currentFrameNo)
                cap.Read(currentFrame)

                imgEntrada = New Image(Of Bgr, Byte)(currentFrame.Width, currentFrame.Height, New Bgr(0, 0, 0))
                imgEntrada = currentFrame.ToImage(Of Bgr, Byte)
                imgEntrada = ResizeImage(imgEntrada, PictureBox1.Width, PictureBox1.Height)
                PictureBox1.Image = imgEntrada.Bitmap

                imgEntradaOriginal = imgEntrada 'stores a copy of the original image without any treatment
                currentFrameNo += 1

                rotCCW = 0
                rotCW = 0

                TrackBar1.Value = 0
                TrackBar1.Minimum = 0
                TrackBar1.Maximum = totalFrames - 1

            End If

        End With

        Label11.Text = "Frame: " & currentFrameNo & "/" & totalFrames
        Label10.Text = "Rotation: " & (rotCW + rotCCW) & "°"

    End Sub

    Private Sub AcharContornosToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AcharContornosToolStripMenuItem1.Click

        If PictureBox1.Image Is Nothing Then

            Exit Sub

        Else

            CannyDetec()

        End If

    End Sub

    '******************************************************************************************************************************************************
    'Defines the parameters from the textboxes
    'Canny control:
    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

        Dim textoNUM As String
        textoNUM = TextBox1.Text

        If textoNUM = "" Then

            cannyThreshold = 0
            TextBox1.Text = ""
            textoNUM = "0"

        Else

            cannyThreshold = TextBox1.Text

        End If

    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

        Dim textoNUM As String
        textoNUM = TextBox2.Text

        If textoNUM = "" Then

            cannyThresholdLinking = 0
            TextBox2.Text = ""
            textoNUM = "0"

        Else

            cannyThresholdLinking = TextBox2.Text

        End If


    End Sub

    Private Sub DefaultCanny_Click(sender As Object, e As EventArgs) Handles DefaultCanny.Click
        TextBox1.Text = 25
        TextBox2.Text = 15
    End Sub

    'grayscale control:
    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged
        Dim textoNUM As String
        textoNUM = TextBox4.Text

        If textoNUM = "" Then
            minor = Byte.MinValue
            TextBox4.Text = ""
            textoNUM = "0"

        ElseIf textoNUM > Byte.MaxValue Then
            minor = Byte.MaxValue
            TextBox4.Text = minor
        Else
            minor = TextBox4.Text
        End If
    End Sub

    Private Sub TextBox5_TextChanged(sender As Object, e As EventArgs) Handles TextBox5.TextChanged
        Dim textoNUM As String
        textoNUM = TextBox5.Text

        If textoNUM = "" Then
            major = Byte.MinValue
            TextBox5.Text = ""
            textoNUM = "0"

        ElseIf textoNUM > Byte.MaxValue Then
            major = Byte.MaxValue
            TextBox5.Text = major
        Else
            major = TextBox5.Text
        End If
    End Sub

    'Blur control:
    Private Sub BlurWidth_TextChanged(sender As Object, e As EventArgs) Handles BlurWidthTB.TextChanged

        Dim textoNUM As String
        textoNUM = BlurWidthTB.Text

        If textoNUM = "" Then

            wBlur = 0
            BlurWidthTB.Text = ""
            textoNUM = "0"

        Else

            wBlur = BlurWidthTB.Text

        End If

    End Sub

    Private Sub BlurHeight_TextChanged(sender As Object, e As EventArgs) Handles BlurHeightTB.TextChanged

        Dim textoNUM As String
        textoNUM = BlurHeightTB.Text

        If textoNUM = "" Then

            hBlur = 0
            BlurHeightTB.Text = ""
            textoNUM = "0"

        Else

            hBlur = BlurHeightTB.Text

        End If

    End Sub

    Private Sub DefaultBlur_Click(sender As Object, e As EventArgs) Handles DefaultBlur.Click
        BlurWidthTB.Text = "10"
        BlurHeightTB.Text = "10"
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        GrayS = False
        TextBox4.Enabled = True
        TextBox5.Enabled = True

    End Sub

    Private Async Sub ObterTabelaToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ObterTabelaToolStripMenuItem.Click

        varRet.Clear()
        currentFrameNo = TrackBar1.Value

        If PictureBox1.Image Is Nothing Then

            Exit Sub

        Else
            Me.Enabled = False
            '============= Preview config ====================
            gr = PictureBox4.CreateGraphics
            gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

            'create bitmap to draw the preview graph then copy the bitmap
            'avoid flickering
            grbuff = New Bitmap(PictureBox4.Width, PictureBox4.Height)
            gr2 = Graphics.FromImage(grbuff)
            gr2.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            '------------------------------------------------------

            'reduce line flickering:
            Me.SetStyle(ControlStyles.UserPaint, True)
            Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
            Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
            '------------------------------------------------------

            Step_X = 3 'the step of the preview in X

            'define the size of the de preview:
            W_Signal0 = PictureBox4.Width - Step_X
            H_Signal0 = PictureBox4.Height
            '-----------------------------------------------

            prevY = H_Signal0
            actY = 0

            maxY = Integer.MinValue
            minY = Integer.MaxValue

            prevX = W_Signal0 - Step_X - 1
            actX = W_Signal0 - 1

            ngraph = 0
            '==========================================


            Dim cap As New VideoCapture(OpenFileDialog1.FileName)
            For currentFrameNo = currentFrameNo To totalFrames - 1

                cap.SetCaptureProperty(1, currentFrameNo)
                cap.Read(currentFrame)

                GC.Collect()


                imgEntrada = currentFrame.ToImage(Of Bgr, Byte)
                imgEntrada = ResizeImage(imgEntrada, PictureBox1.Width, PictureBox1.Height)
                imgEntrada = imgEntrada.Rotate(rotCW + rotCCW, New PointF(PictureBox1.Width / 2, PictureBox1.Height / 2), CvEnum.Inter.Linear, New Bgr(0, 0, 0), True)
                posTempo = cap.GetCaptureProperty(CvEnum.CapProp.PosMsec)
                posTempo = Math.Round(posTempo, 5) 'round the time position of the frame
                iniFrame = TrackBar1.Value + 1

                CannyDetec()

                Dim linha As DataRow = varRet.NewRow

                linha("frameNo") = currentFrameNo
                linha("posTempo") = posTempo
                linha("iniX") = IniX
                linha("iniY") = IniY
                linha("HeightCalib") = HeightCalib
                linha("WidthCalib") = WidthCalib
                linha("RetAreaCalib") = RetAreaCalib
                linha("ElipAreaCalib") = ElipAreaCalib
                linha("Altura") = RetAlt
                linha("Largura") = RetLarg
                linha("AreaRet") = Arrea
                linha("AreaEli") = ArreaElip

                varRet.Rows.Add(linha)

                Await Task.Delay(10)

                Label11.Text = "Frame: " & currentFrameNo & "/" & totalFrames
                ProgressBar1.Maximum = totalFrames
                ProgressBar1.Increment(1)
                TrackBar1.Hide()

                '=========================== Preview creation ===============================================
                'create trace movement:
                Dim A As New Area
                PictureBox4.Image = CType(A.Copy(PictureBox4, New RectangleF(Step_X, 0, W_Signal0, H_Signal0)), Bitmap).Clone
                A = Nothing
                PictureBox4.Refresh()
                '--------------------------------------

                Dim Y As Integer
                If RadioButton3.Checked Then
                    Y = Arrea
                End If

                If RadioButton4.Checked Then
                    Y = RetLarg
                End If
                If RadioButton5.Checked Then
                    Y = RetAlt
                End If
                If RadioButton6.Checked Then
                    Y = ArreaElip
                End If

                testingX += 1
                '---------------------------------

                'adjust Y scale
                Try
                    actY = ((minY - Y) / (minY - maxY)) * H_Signal0
                Catch ex As Exception
                    actY = 0
                End Try
                '--------------------------------------------------

                Dim change_scale As Boolean = False

                'if the graph reach the edge:
                If zerou Then
                    maxY = Integer.MinValue
                    minY = Y
                    graphXY(1, ngraph) = Y
                    graphXY(2, ngraph) = actY
                    For i = 0 To ngraph - 1
                        graphXY(1, i) = graphXY(1, i + 1)
                        graphXY(2, i) = graphXY(2, i + 1)
                        If graphXY(1, i) > maxY Then
                            maxY = graphXY(1, i)
                        End If
                        If graphXY(1, i) < minY Then
                            minY = graphXY(1, i)
                        End If
                    Next
                Else
                    ReDim Preserve graphXY(2, ngraph)
                    graphXY(0, ngraph) = actX - (Step_X * ngraph)
                    graphXY(1, ngraph) = Y
                    graphXY(2, ngraph) = actY
                End If
                '-------------------------------------------------

                'finding min and max Y:
                If Y > maxY Then
                    maxY = Y
                    change_scale = True
                End If

                If Y < minY Then
                    minY = Y
                    change_scale = True
                End If
                '---------------------------------

                'drawing the preview graph:
                Dim pr As New Pen(Color.Red)
                pr.Width = 1
                If change_scale Then
                    Dim x1, x2, y1, y2 As Integer
                    gr.Clear(PictureBox4.BackColor)
                    gr2.Clear(PictureBox4.BackColor)
                    For i = 0 To ngraph - 1 Step 1
                        Try
                            graphXY(2, i) = ((minY - graphXY(1, i)) / (minY - maxY)) * H_Signal0
                        Catch ex As Exception
                            graphXY(2, i) = 0
                        End Try
                        y1 = graphXY(2, i)
                        Try
                            graphXY(2, i + 1) = ((minY - graphXY(1, i + 1)) / (minY - maxY)) * H_Signal0
                        Catch ex As Exception
                            graphXY(2, i + 1) = 0
                        End Try
                        y2 = graphXY(2, i + 1)
                        x1 = graphXY(0, ngraph - i)
                        x2 = graphXY(0, ngraph - (i + 1))

                        TR.Graph(gr2, pr, x1, y1, x2, y2)
                    Next
                Else
                    gr2.DrawImage(PictureBox4.Image, 0, 0)
                End If

                If graphXY(0, ngraph) <= 0 Then
                    zerou = True
                Else
                    ngraph += 1
                End If

                TR.Graph(gr2, pr, prevX, prevY, actX, actY)
                pr.Dispose()
                pr = Nothing
                prevY = actY
                '---------------------------------------------------

                PictureBox4.Image = grbuff
                '=====================================================================
            Next

            TrackBar1.Show()

            'config the sequencial name of the files:
            Dim seqNAME = UniqueName(SaveFileDialog1.FileName)
            If seqNAME(0) Then
                SaveFileDialog1.FileName = seqNAME(1)
            End If
            '-----------------------------------------
            SaveFileDialog1.ShowDialog()
            SalvarNovamenteToolStripMenuItem.Visible = 1
            ProgressBar1.Value = 0
            ProgressBar1.ResetText()
            TrackBar1.ResetText()
            '======== return to the first frame ========
            cap.SetCaptureProperty(1, TrackBar1.Value)
            cap.Read(currentFrame)
            imgEntrada = currentFrame.ToImage(Of Bgr, Byte)
            imgEntrada = ResizeImage(imgEntrada, PictureBox1.Width, PictureBox1.Height)
            imgEntrada = imgEntrada.Rotate(rotCW + rotCCW, New PointF(PictureBox1.Width / 2, PictureBox1.Height / 2), CvEnum.Inter.Linear, New Bgr(0, 0, 0), True)
            PictureBox1.Image = imgEntrada.Bitmap
            '===========================================

            '============== Preview clear ==================
            zerou = False
            '===============================================
            Me.Enabled = True
        End If

    End Sub


    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        If PictureBox1.Image Is Nothing Then

            TrackBar1.Value = 0
            Exit Sub

        Else


            If imgEntrada.Height > 1 Then
                Dim cap As New VideoCapture(OpenFileDialog1.FileName)
                currentFrameNo = TrackBar1.Value
                cap.SetCaptureProperty(1, currentFrameNo)
                cap.Read(currentFrame)

                imgEntrada = currentFrame.ToImage(Of Bgr, Byte)
                imgEntrada = ResizeImage(imgEntrada, PictureBox1.Width, PictureBox1.Height)
                PictureBox1.Image = imgEntrada.Bitmap

                imgEntradaOriginal = imgEntrada 'store the original image without any treatment
                currentFrameNo += 1
                iniFrame = TrackBar1.Value + 1

            End If

        End If

        Label11.Text = "Frame: " & currentFrameNo & "/" & totalFrames

    End Sub

    Private Sub SaveFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog1.FileOk
        TheWriter()
        SaveFileDialog1.DefaultExt = ".txt"
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        GrayS = True
        TextBox4.Enabled = False
        TextBox5.Enabled = False

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If PictureBox1.Image Is Nothing Then

            Exit Sub 'Exit sub if no frame was load in the picturebox

        End If

        rotCCW += -10
        imgEntrada = imgEntradaOriginal 'load the original image
        imgEntrada = imgEntrada.Rotate(rotCW + rotCCW, New PointF(PictureBox1.Width / 2, PictureBox1.Height / 2), CvEnum.Inter.Linear, New Bgr(0, 0, 0), True)


        If rotCCW < -350 Then
            rotCCW = 0
        End If
        PictureBox1.Image = imgEntrada.Bitmap

        Label10.Text = "Rotation: " & (rotCW + rotCCW) & "°"

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If PictureBox1.Image Is Nothing Then
            Exit Sub 'Exit sub if no frame was load in the picturebox
        End If
        rotCW += 10
        imgEntrada = imgEntradaOriginal 'load the original image
        imgEntrada = imgEntrada.Rotate(rotCW + rotCCW, New PointF(PictureBox1.Width / 2, PictureBox1.Height / 2), CvEnum.Inter.Linear, New Bgr(0, 0, 0), True)

        If rotCW > 350 Then
            rotCW = 10
        End If

        PictureBox1.Image = imgEntrada.Bitmap

        Label10.Text = "Rotation: " & (rotCW + rotCCW) & "°"

    End Sub

    'Median control
    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        sizeMedian = NumericUpDown1.Value
    End Sub

    Private Sub SaveFileDialog2_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles SaveFileDialog2.FileOk

        ConfigWriter()

    End Sub

    Private Sub SaveConfigsToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles SaveConfigsToolStripMenuItem1.Click

        'This dialog refers to the config file
        SaveFileDialog2.ShowDialog()

    End Sub

    Private Sub LoadConfigsToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles LoadConfigsToolStripMenuItem1.Click

        OpenFileDialog2.ShowDialog()

    End Sub

    Private Sub OpenFileDialog2_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog2.FileOk

        Try
            TextBox1.Text = ConfigReader(OpenFileDialog2.FileName(), "cannyThreshold")
            TextBox2.Text = ConfigReader(OpenFileDialog2.FileName(), "thresholdLinking")
            RadioButton1.Checked = ConfigReader(OpenFileDialog2.FileName(), "standartGrayScaleCheck")
            RadioButton2.Checked = ConfigReader(OpenFileDialog2.FileName(), "customGrayScaleCheck")
            TextBox4.Text = ConfigReader(OpenFileDialog2.FileName(), "minorGrayScale")
            TextBox5.Text = ConfigReader(OpenFileDialog2.FileName(), "majorGrayScale")
            CheckBox1.Checked = ConfigReader(OpenFileDialog2.FileName(), "blurBeingUsed")
            CheckBox2.Checked = ConfigReader(OpenFileDialog2.FileName(), "medianBeingUsed")
            CheckBox3.Checked = ConfigReader(OpenFileDialog2.FileName(), "gaussianBeingUsed")
            CheckBox4.Checked = ConfigReader(OpenFileDialog2.FileName(), "noTreatment")
            BlurWidthTB.Text = ConfigReader(OpenFileDialog2.FileName(), "blurWidth")
            BlurHeightTB.Text = ConfigReader(OpenFileDialog2.FileName(), "blurHeight")
            NumericUpDown1.Value = ConfigReader(OpenFileDialog2.FileName(), "sizeMedian")
            NumericUpDown2.Value = ConfigReader(OpenFileDialog2.FileName(), "kGauss")
            FatorTamTB.Text = ConfigReader(OpenFileDialog2.FileName(), "sizeFactor")
            calibIndex = ConfigReader(OpenFileDialog2.FileName(), "calibrationFactor")

            If calibIndex > 0 Then
                Label14.Text = "Calib. Factor: " & calibIndex
            End If

        Catch ex As Exception

            MsgBox("Please, select a valid .dfl config file from ContHeart")

        End Try

    End Sub

    Private Sub DefaultMedian_Click(sender As Object, e As EventArgs) Handles DefaultMedian.Click
        NumericUpDown1.Value = "5"
    End Sub

    'Gaussian control
    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        kGauss = NumericUpDown2.Value
    End Sub

    Private Sub DefaultGaussian_Click(sender As Object, e As EventArgs) Handles DefaultGaussian.Click
        NumericUpDown2.Value = "5"
    End Sub

    Private Sub SalvarNovamenteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SalvarNovamenteToolStripMenuItem.Click
        'config the sequencial name of the files
        Dim seqNAME = UniqueName(SaveFileDialog1.FileName)
        If seqNAME(0) Then
            SaveFileDialog1.FileName = seqNAME(1)
        End If
        '----------------------------------------
        SaveFileDialog1.ShowDialog()

    End Sub

    Private Sub CalibrarrToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CalibrarToolStripMenuItem.Click
        Form2.Show()
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        'ROI on picturebox1:
        If PictureBox1.Image Is Nothing Then
            Exit Sub 'Exit sub if no frame was load in the picturebox
        End If

        XStart = e.X 'store the mouse X pos
        YStart = e.Y 'store the mouse Y pos
        '-------------------------------------

        Label12.Text = e.X & " , " & e.Y

    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        'CROI on picturebox1:
        If PictureBox1.Image Is Nothing Then
            Exit Sub 'Exit sub if no frame was load in the picturebox
        End If

        If Not e.Button = MouseButtons.Left Then
            Exit Sub 'Exit sub if the user is not holding the mouse click
        End If

        XCurrent = e.X 'store the mouse X pos
        YCurrent = e.Y 'store the mouse Y pos

        selRET = getRect(XStart, YStart, XCurrent, YCurrent) 'define the ROI parameters to evoke getRect function

        PictureBox1.Invalidate() 'clear the ROI

        '-------------------------------------------------------------

    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
        If Not selRET = Nothing Then 'was the ROI defined?
            Dim caneta As New Pen(Brushes.Red, 2) 'ROI color and style
            caneta.DashStyle = Drawing2D.DashStyle.Dash
            e.Graphics.DrawRectangle(caneta, selRET)
        End If

    End Sub

    Private Function getRect(ByVal startX As Integer, ByVal startY As Integer, ByVal currentX As Integer, ByVal currentY As Integer) As Rectangle
        Dim rect As New Rectangle
        rect.X = Math.Min(startX, currentX)
        rect.Y = Math.Min(startY, currentY)
        rect.Width = Math.Abs(startX - currentX)
        rect.Height = Math.Abs(startY - currentY)

        Return rect

    End Function

    'Resize image to the picturebox:   
    Private Function ResizeImage(ByVal InputImage As Image(Of Bgr, Byte), ByVal I_width As Integer, ByVal I_height As Integer) As Image(Of Bgr, Byte)
        Dim resizedIMG As Image(Of Bgr, Byte)
        resizedIMG = InputImage.Resize(I_width, I_height, CvEnum.Inter.Linear)
        Return resizedIMG
    End Function

    '**************************************************************************************************************************************************************

    Private Sub TextBox4_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox4.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If

    End Sub

    Private Sub TextBox5_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox5.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub BlurWidthTB_KeyPress(sender As Object, e As KeyPressEventArgs) Handles BlurWidthTB.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub BlurHeightTB_KeyPress(sender As Object, e As KeyPressEventArgs) Handles BlurHeightTB.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub FatorTamTB_KeyPress(sender As Object, e As KeyPressEventArgs) Handles FatorTamTB.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub TextBox2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox2.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Public Function UniqueName(pathe As String)
        Dim dir As String = Path.GetDirectoryName(pathe)
        Dim filename As String = Path.GetFileNameWithoutExtension(pathe)
        Dim sLast As String
        Dim fileEx As String = Path.GetExtension(pathe)
        Dim exist As Boolean = 0
        Dim i As Integer = 1
        Dim slastIndex As Integer

        If filename.EndsWith(")") Then
            If filename.Contains("(") Then
                slastIndex = filename.LastIndexOf("(")
                sLast = filename.Substring(slastIndex + 1, filename.Length - 2 - slastIndex)
                If IsNumeric(sLast) Then
                    filename = filename.Remove(slastIndex)
                End If
            End If
        End If

        While File.Exists(pathe)
            exist = 1
            pathe = Path.Combine(dir, filename & "(" & Convert.ToString(i) & ")" & fileEx)
            i += 1
        End While

        Return {exist, pathe}
    End Function

End Class
