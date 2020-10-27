Imports System.Web.Script.Serialization
Imports System.IO

Public Class Form1


    Public MembersData As List(Of Member) = Nothing
    Private myFileDataName As String = "members.json"


    Public Sub New()


        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Call Me.ClearMe()

    End Sub


    Private Sub ClearMe()

        Me.txtCodeID.Text = ""
        Me.txtName.Text = ""
        Me.txtJobName.Text = ""
        Me.txtAge.Value = 0
        Me.txtEmail.Text = ""
        Me.txtGender.Text = ""
        Me.txtBirthDate.Value = Now
        Me.chkIsActive.Checked = False

    End Sub
    Private Sub BtnNew_Click(sender As Object, e As EventArgs) Handles BtnNew.Click

        Call Me.ClearMe()

    End Sub



    Private Sub BtnSave_Click(sender As Object, e As EventArgs) Handles BtnSave.Click


#Region " Validation Fields"
        If String.IsNullOrWhiteSpace(Me.txtName.Text) Then

            MsgBox("يرجى ادخال اسم المستخدم", MsgBoxStyle.Exclamation, "Warning")
            Me.txtName.Focus()
            Exit Sub

        ElseIf String.IsNullOrWhiteSpace(Me.txtJobName.Text) Then

            MsgBox("يرجى تحديد الوظيفة", MsgBoxStyle.Exclamation, "Warning")
            Me.txtJobName.Focus()
            Exit Sub


        ElseIf String.IsNullOrWhiteSpace(Me.txtEmail.Text) Then

            MsgBox("يرجى تحديد الايميل", MsgBoxStyle.Exclamation, "Warning")
            Me.txtEmail.Focus()
            Exit Sub

        ElseIf Me.txtAge.Value <= 15 Then

            MsgBox("يرجى تحديد السن بشكل صحيح 15 سنة أو أكبر", MsgBoxStyle.Exclamation, "Warning")
            Me.txtAge.Focus()
            Exit Sub

        End If
#End Region




        If IsNothing(MembersData) Then
            MembersData = New List(Of Member)
        End If

        'MembersData            (NOT NULL)      {Count = 0}



        '1:  get All Members from Data file : 
        MembersData = getListMembers()



        '2: Save this New record to datafile

        ' للحصول على عدد الاعضاء المسجلين
        Dim membersCount As Integer = MembersData.Count

        Dim addMember As New Member

        With addMember
            .CodeId = membersCount + 1
            .MemberName = Me.txtName.Text
            .JobName = Me.txtJobName.Text
            .Email = Me.txtEmail.Text
            .Age = Me.txtAge.Value
            .Gender = Me.txtGender.Text
            .BirthDate = Me.txtBirthDate.Value.Date
            .IsActive = Me.chkIsActive.Checked
        End With

        Me.MembersData.Add(addMember)

        Call Me.goSaveRecords(Me.MembersData)


        'Refresh Display :
        BtnRefresh.PerformClick()


    End Sub




    Public Function goSaveRecords(uList As List(Of Member), Optional _showMsg As Boolean = True) As Boolean

        '=> passing List ( Members )
        If (IsNothing(uList)) Then

            If _showMsg = True Then
                MsgBox("عفواً، لم يتم العثور على بيانات للحفظ", MsgBoxStyle.Exclamation)
            End If

            Return False
        End If




        '=> Save to Json Data File
        '   A     get Path file 
        '   B     Convert list to Json List 
        '   C     Save List ()


        '   A     get Path file 
        Dim fileName As String = getFilePath(myFileDataName)

        If File.Exists(fileName) Then
            'Make backup 
            'Try
            '    'My.Computer.FileSystem.CopyFile(fileName, (fileName & ".backup"), True, FileIO.UICancelOption.DoNothing)
            'Catch ex As Exception : End Try


            'Delete last file: 
            IO.File.Delete(fileName)

        End If


        '   B     Convert list to Json List 
        Try

            ' استدعاء في الفورم
            'Imports System.Web.Script.Serialization

            Dim json As String = New JavaScriptSerializer().Serialize(uList)


            'save to datafile :

            System.IO.File.WriteAllText(fileName, json)

            If _showMsg = True Then
                MsgBox("تم الحفظ بنجاح")
            End If



            Return True

        Catch ex As Exception
            If _showMsg = True Then
                MsgBox("Exception : " & ex.Message.ToString, MsgBoxStyle.Critical, "Oops Warning")
            End If
        End Try


        Return False

    End Function

    Public Function getListMembers() As List(Of Member)

        '01 get Path file : 
        Dim fileName As String = getFilePath(myFileDataName)
        If File.Exists(fileName) = False Then
            Return New List(Of Member)
        End If

        '02 Convert file to Json Data
        '=>  get Content from data file:
        Dim MyDataFileText As String = File.ReadAllText(fileName)

        Dim myList = New JavaScriptSerializer().Deserialize(Of List(Of Member))(MyDataFileText)

        If (IsNothing(myList)) OrElse myList.Count = 0 Then
            Return New List(Of Member)
        End If


        Return myList



    End Function

    Private Sub BtnRefresh_Click(sender As Object, e As EventArgs) Handles BtnRefresh.Click

        Try


            Call Me.ClearMe()

            Me.dgv.DataSource = Nothing

            Dim all As List(Of Member) = getListMembers()

            '=> myList (Members)

            Dim dt As DataTable = Module1.ConvertList_toDatatable(Of Member)(all)

            If (IsNothing(dt)) OrElse dt.Rows.Count = 0 Then
                MsgBox("عفواً، لم يتم العثور على اي بيانات", MsgBoxStyle.Information, "Alert")
                Exit Sub
            End If


            Me.dgv.DataSource = dt


        Catch ex As Exception
            MsgBox("Sory , " & ex.Message, MsgBoxStyle.Critical)
        End Try


    End Sub

    Private Function getCurrentIDFromDGV() As Integer
        Try

            If dgv.Rows.Count = 0 Then
                Return 0
            End If

            If Not IsNothing(dgv.CurrentRow) Then
                If dgv.CurrentRow.Cells("CodeId").Value Is Nothing Then
                    Return 0
                End If

                Return Convert.ToInt32(dgv.CurrentRow.Cells("CodeId").Value)

            End If
        Catch ex As Exception : End Try

        Return 0

    End Function
    Private Function getCurrent_EmailFromDGV() As String

        Try

            If dgv.Rows.Count = 0 Then
                Return 0
            End If

            If Not IsNothing(dgv.CurrentRow) Then
                If dgv.CurrentRow.Cells("CodeId").Value Is Nothing Then
                    Return 0
                End If

                Return Convert.ToString(dgv.CurrentRow.Cells("email").Value)
            End If

        Catch ex As Exception : End Try

        Return ""

    End Function



    Private Sub BtnDelete_Click(sender As Object, e As EventArgs) Handles BtnDelete.Click


        Dim id As Integer = Me.getCurrentIDFromDGV
        Dim email As String = Me.getCurrent_EmailFromDGV


        If id > 0 Then

            'get From Function:
            Dim _Members = Me.getListMembers

            If Not IsNothing(_Members) Then
                If _Members.Count > 0 Then


                    Dim member = (From M In _Members
                                  Where M.CodeId = id And
                                      Convert.ToString(M.Email).ToLower.Equals(email)).FirstOrDefault()


                    If Not IsNothing(member) Then

                        'Delete Current Object 
                        _Members.Remove(member)


                        'Save to Data file: 

                        Call Me.goSaveRecords(_Members, False)

                        MsgBox("تم حذف الملف بنجاح", MsgBoxStyle.Information, "Deleted Done")

                        'Refresh : 

                        Me.BtnRefresh.PerformClick()

                    Else
                        MsgBox("عفواً لم يتم العثور على الملف")
                        Exit Sub
                    End If



                End If
            End If



        End If



    End Sub


    Private Function getOneMemberByID_Email(id As Integer, email As String) As Member

        Try


            If id > 0 AndAlso Not String.IsNullOrWhiteSpace(email) Then

                'get From Function:
                Dim _Members = Me.getListMembers

                If Not IsNothing(_Members) Then
                    If _Members.Count > 0 Then

                        Dim member = (From M In _Members
                                      Where M.CodeId = id And
                                          Convert.ToString(M.Email).ToLower.Equals(email.ToLower)).FirstOrDefault()


                        Return member

                    End If
                End If


            End If

        Catch ex As Exception

        End Try

        Return Nothing


    End Function




    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.BtnRefresh.PerformClick()

    End Sub

    Private Sub BtnUpdate_Click(sender As Object, e As EventArgs) Handles BtnUpdate.Click


        Dim id As Integer = 0
        Dim email As String = ""

        Try
            id = Convert.ToInt32(Me.txtCodeID.Text)
            email = Convert.ToString(Me.txtEmail.Text)
        Catch ex As Exception : End Try


        If id > 0 AndAlso Not String.IsNullOrWhiteSpace(email) Then


            Dim currentList As List(Of Member) = Me.getListMembers


            For i As Integer = 0 To currentList.Count - 1

                'Ahmed@Yaho.com .ToLower == ahmed@yahoo.com.ToLower

                If currentList(i).CodeId = id AndAlso currentList(i).Email.ToLower = email.ToLower Then

                    With currentList(i)
                        .MemberName = Me.txtName.Text
                        .JobName = Me.txtJobName.Text
                        .Age = Me.txtAge.Value
                        .Gender = Me.txtGender.Text
                        .BirthDate = Me.txtBirthDate.Value.Date
                        .IsActive = Me.chkIsActive.Checked
                    End With


                End If

            Next

            Me.MembersData = currentList
            Call Me.goSaveRecords(currentList)



        End If


    End Sub


    Private Sub dgv_CurrentCellChanged(sender As Object, e As EventArgs) Handles dgv.CurrentCellChanged

        Try

            Me.ClearMe()


            Dim id As Integer = Me.getCurrentIDFromDGV
            Dim email As String = Me.getCurrent_EmailFromDGV

            Dim member As Member = getOneMemberByID_Email(id, email)

            If Not IsNothing(member) Then

                Me.txtCodeID.Text = member.CodeId
                Me.txtName.Text = member.MemberName
                Me.txtEmail.Text = member.Email
                Me.txtJobName.Text = member.JobName

                Me.txtAge.Value = member.Age
                Me.txtBirthDate.Value = member.BirthDate
                Me.txtGender.Text = member.Gender
                Me.chkIsActive.Checked = member.IsActive
            End If

        Catch ex As Exception
            Console.WriteLine("Exception:" & ex.Message.ToString)
        End Try

    End Sub


End Class
