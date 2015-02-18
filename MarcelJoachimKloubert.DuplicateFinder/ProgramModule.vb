'' LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
''
'' s. https://github.com/mkloubert/DuplicateFinder

Imports System.IO
Imports System.Reflection

Module ProgramModule

#Region "Methods (4)"

    ''' <summary>
    ''' Entry point.
    ''' </summary>
    ''' <param name="args">Command line arguments.</param>
    Sub Main(args() As String)
        Try
            PrintHeader()

            Dim actionToInvoke As Action = Nothing

            Dim settings As AppSettings = New AppSettings()

            Dim normalizedArgs As String() = args.Select(Function(x)
                                                             Return x.Trim()
                                                         End Function) _
                                                 .Where(Function(x)
                                                            Return x <> String.Empty
                                                        End Function) _
                                                 .ToArray()

            If normalizedArgs.Length < 1 Then
                settings.Directories _
                        .Add(New DirectoryInfo(Environment.CurrentDirectory))
            Else
                For i As Integer = 1 To normalizedArgs.Length
                    Dim a As String = normalizedArgs(i - 1).TrimStart()

                    If a.ToLower().Trim() = "/r" Or a.ToLower().Trim() = "/recursive" Then
                        '' scan recursive
                        settings.Recursive = True
                    ElseIf a.ToLower().Trim() = "/d" Or a.ToLower().Trim() = "/del" Or a.ToLower().Trim() = "/delete" Then
                        '' delete duplicates
                        settings.DeleteDuplicates = True
                    ElseIf a.ToLower().Trim() = "/?" Or a.ToLower().Trim() = "/h" Or a.ToLower().Trim() = "/help" Then
                        '' show help
                        actionToInvoke = New Action(AddressOf ShowHelp)
                        settings = Nothing

                        Exit For
                    ElseIf a.ToLower().Trim() = "/b" Or a.ToLower().Trim() = "/bash" Or a.ToLower().StartsWith("/b:") Or a.ToLower().StartsWith("/bash:") Then
                        '' bash script
                        Dim bashPath As String = Nothing

                        If a.Contains(":") Then
                            bashPath = a.Substring(a.IndexOf(":") + 1) _
                                        .Trim()
                        End If

                        If String.IsNullOrWhiteSpace(bashPath) Then
                            bashPath = "./DuplicateFinder_del_duplicates.sh"
                        End If

                        If Not Path.IsPathRooted(bashPath) Then
                            bashPath = Path.Combine(Environment.CurrentDirectory, bashPath)
                        End If

                        settings.BashScript = New FileInfo(bashPath)
                    ElseIf a.ToLower().Trim() = "/wb" Or a.ToLower().Trim() = "/batch" Or a.ToLower().StartsWith("/wb:") Or a.ToLower().StartsWith("/batch:") Then
                        '' windows batch
                        Dim batchPath As String = Nothing

                        If a.Contains(":") Then
                            batchPath = a.Substring(a.IndexOf(":") + 1) _
                                        .Trim()
                        End If

                        If String.IsNullOrWhiteSpace(batchPath) Then
                            batchPath = "./DuplicateFinder_del_duplicates.cmd"
                        End If

                        If Not Path.IsPathRooted(batchPath) Then
                            batchPath = Path.Combine(Environment.CurrentDirectory, batchPath)
                        End If

                        settings.WindowsBatch = New FileInfo(batchPath)
                    ElseIf a.ToLower().Trim() = "/p" Or a.ToLower().Trim() = "/php" Or a.ToLower().StartsWith("/p:") Or a.ToLower().StartsWith("/php:") Then
                        '' PHP script
                        Dim phpPath As String = Nothing

                        If a.Contains(":") Then
                            phpPath = a.Substring(a.IndexOf(":") + 1) _
                                        .Trim()
                        End If

                        If String.IsNullOrWhiteSpace(phpPath) Then
                            phpPath = "./DuplicateFinder_del_duplicates.php"
                        End If

                        If Not Path.IsPathRooted(phpPath) Then
                            phpPath = Path.Combine(Environment.CurrentDirectory, phpPath)
                        End If

                        settings.PhpScript = New FileInfo(phpPath)
                    Else
                        '' handle as directory

                        settings.Directories _
                                .Add(New DirectoryInfo(a))
                    End If
                Next
            End If

            If settings IsNot Nothing Then
                If settings.Directories.Count < 1 Then
                    settings = Nothing
                End If
            End If

            If settings IsNot Nothing Then
                actionToInvoke = Sub()
                                     Dim operation As ScanOperation = New ScanOperation(settings)
                                     operation.Start()
                                 End Sub
            End If

            If actionToInvoke IsNot Nothing Then
                actionToInvoke()
            End If
        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine()

            ConsoleHelper.InvokeForColor(Sub()
                                             Console.WriteLine(ex)
                                         End Sub, ConsoleColor.Yellow _
                                                , ConsoleColor.Red)

            Console.WriteLine()
            Console.WriteLine()
        End Try

#If DEBUG Then
        Console.WriteLine()
        Console.WriteLine()

        Console.WriteLine("===== ENTER =====")
        Console.ReadLine()
#End If
    End Sub

    Private Sub PrintHeader()
        Dim title As String = String.Format("DuplicateFinder {0}", _
                                            Assembly.GetExecutingAssembly().GetName().Version)

        Console.WriteLine(title)
        Console.WriteLine(String.Concat(Enumerable.Repeat("=", _
                                                          title.Length + 5)))
        Console.WriteLine()
    End Sub

    Private Sub ShowHelp()
        Console.WriteLine("Scans directories for duplicate files.")
        Console.WriteLine()
        Console.WriteLine()

        Console.WriteLine("DuplicateFinder DIRS OPTS")
        Console.WriteLine()
        Console.WriteLine("  DIRS              One or more directory to scan.")
        Console.WriteLine()
        Console.WriteLine("  OPTS              One or more options.")
        Console.WriteLine()
        Console.WriteLine("    /?                Show this help screen.")
        Console.WriteLine()
        Console.WriteLine("    /d                Delete duplicates.")
        Console.WriteLine()
        Console.WriteLine("    /b:[FILE]         Write bash script that deletes duplicates.")
        Console.WriteLine()
        Console.WriteLine("    /p:[FILE]         Write PHP script that deletes duplicates.")
        Console.WriteLine()
        Console.WriteLine("    /wb:[FILE]        Write Windows batch file that deletes duplicates.")
        Console.WriteLine()
        Console.WriteLine("    /r                Scan recursive.")
    End Sub

#End Region

End Module
