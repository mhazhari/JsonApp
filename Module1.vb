Module Module1

    Public Function getFilePath(uFileName As String) As String

        Dim folderName As String = IO.Path.GetDirectoryName(Application.ExecutablePath) & "\JsonData"

        If Not IO.Directory.Exists(folderName) Then
            IO.Directory.CreateDirectory(folderName)
        End If

        Return folderName & "\" & uFileName


    End Function




    Public Function ConvertList_toDatatable(Of T)(list As List(Of T)) As DataTable

        Try

            If (IsNothing(list)) OrElse list.Count = 0 Then
                Return Nothing
            End If
            'If Not list.Any Then
            '    Return Nothing
            'End If


            Dim _table As New DataTable()


            '=> Columns : 

            Dim fields() = list.First.GetType.GetProperties
            For Each f In fields
                _table.Columns.Add(f.Name, f.PropertyType)
            Next


            '=> ROWS :
            For Each item As T In list

                Dim row As DataRow = _table.NewRow

                'get all Values :

                For Each f In fields
                    If IsNothing(f.GetValue(item)) Then
                        Continue For
                    Else
                        row(f.Name) = f.GetValue(item)
                    End If
                Next

                _table.Rows.Add(row)

            Next

            Return _table


        Catch ex As Exception
            MsgBox("Sory , " & ex.Message, MsgBoxStyle.Critical)
        End Try

        Return New DataTable

    End Function







End Module
