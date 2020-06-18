Imports Emgu.CV
Imports Emgu.CV.Structure

Public Class Form2

    Dim xInicial, yInicial, xFinal, yFinal As Integer
    Dim drawingLine As Boolean = False
    Dim CalibOK As Boolean = False
    Dim realValue As Double

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        CalibOK = False

        If Form1.PictureBox1.Image Is Nothing Then

            Exit Sub

        Else

            PictureBox1.Image = Form1.PictureBox1.Image
            TextBox1.Enabled = True
        End If


    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) And Not e.KeyChar = "." Then
            e.Handled = True
        End If
        If e.KeyChar = "." Then
            If InStr(TextBox1.Text, ".") <> 0 Then e.Handled = True
            If TextBox1.Text = "" Then
                e.Handled = True
                TextBox1.Text = "0."
                TextBox1.Select(TextBox1.Text.Length, 0)
            End If
        End If
    End Sub

    Private Sub textbox2_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox2.KeyPress
        If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) And Not e.KeyChar = "." Then
            e.Handled = True
        End If
        If e.KeyChar = "." Then
            If InStr(TextBox2.Text, ".") <> 0 Then e.Handled = True
            If TextBox2.Text = "" Then
                e.Handled = True
                TextBox2.Text = "0."
                TextBox2.Select(TextBox2.Text.Length, 0)
            End If
        End If
    End Sub

    Private Sub SelecionarOutroArquivoToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SelecionarOutroArquivoToolStripMenuItem.Click

        With OpenFileDialog1()

            If .ShowDialog() = DialogResult.OK Then
                Dim imgCALIB = New Image(Of Bgr, Byte)(.FileName)
                imgCALIB = imgCALIB.Resize(PictureBox1.Width, PictureBox1.Height, CvEnum.Inter.Linear)
                PictureBox1.Image = imgCALIB.Bitmap
                TextBox1.Enabled = True
                TextBox1.Text = ""
                TextBox2.Text = ""
                TextBox2.Enabled = False
            End If

        End With

    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown

        If PictureBox1.Image Is Nothing Then
            Exit Sub
        End If

        xInicial = e.X
        yInicial = e.Y
        Label3.Text = e.X & " , " & e.Y

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If TextBox1.Enabled Then
            If TextBox1.Text IsNot "" Then
                If Not Convert.ToDouble(TextBox1.Text) = 0 Then
                    Dim point1, point2 As Integer
                    point1 = xFinal - xInicial
                    point2 = yFinal - yInicial
                    calibIndex = Convert.ToDouble(TextBox1.Text)
                    calibIndex = calibIndex / Math.Sqrt(point1 ^ 2 + point2 ^ 2)
                    calibIndex = Math.Round(calibIndex, 6)
                    CalibOK = True
                    Form1.Label14.Text = "Calib. Factor: " & calibIndex
                    Label7.Text = Math.Round(Convert.ToDouble(TextBox1.Text), 6)
                    Label9.Text = Math.Round(calibIndex, 6)
                End If
            End If

            If TextBox2.Text IsNot "" Then
                If Not Convert.ToDouble(TextBox2.Text) = 0 Then
                    calibIndex = Convert.ToDouble(TextBox2.Text)
                    calibIndex = Math.Round(calibIndex, 6)
                    CalibOK = True
                    Form1.Label14.Text = "Calib. Factor: " & calibIndex
                End If
            End If

        Else

                If TextBox2.Text IsNot "" Then
                If Not Convert.ToDouble(TextBox2.Text) = 0 Then
                    calibIndex = Convert.ToDouble(TextBox2.Text)
                    calibIndex = Math.Round(calibIndex, 6)
                    CalibOK = True
                    Form1.Label14.Text = "Calib. Factor: " & calibIndex
                End If
            End If
        End If
    End Sub


    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove

        If PictureBox1.Image Is Nothing Then
            Exit Sub
        End If
        If Not e.Button = MouseButtons.Left Then
            Exit Sub
        End If

        xFinal = e.X
        yFinal = e.Y
        Label5.Text = e.X & " , " & e.Y

        PictureBox1.Invalidate()

    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp

        If CalibOK Then
            Dim point1, point2 As Integer
            point1 = xFinal - xInicial
            point2 = yFinal - yInicial
            Dim Pitag As Double
            Pitag = Math.Sqrt(point1 ^ 2 + point2 ^ 2) * calibIndex
            Label7.Text = Math.Round(Pitag, 6)
            Label9.Text = Math.Round(calibIndex, 6)
        End If

    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint

        Dim caneta As New Pen(Brushes.Red, 3)
        caneta.DashStyle = Drawing2D.DashStyle.Solid
        e.Graphics.DrawLine(caneta, xInicial, yInicial, xFinal, yFinal)

    End Sub
End Class