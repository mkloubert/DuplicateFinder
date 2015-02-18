'' LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
''
'' s. https://github.com/mkloubert/DuplicateFinder

Imports System.IO
Imports System.Text

''' <summary>
''' Handles a scan operation.
''' </summary>
Public NotInheritable Class ScanOperation

#Region "Fields (1)"

    Private ReadOnly _SETTINGS As AppSettings

#End Region

#Region "Constructors (1)"

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AppSettings" /> class.
    ''' </summary>
    ''' <param name="settings">The underlying settings.</param>
    Sub New(settings As AppSettings)
        Me._SETTINGS = settings
    End Sub

#End Region

#Region "Properties (1)"

    ''' <summary>
    ''' Gets the underlying settings.
    ''' </summary>
    Public ReadOnly Property Settings As AppSettings
        Get
            Return Me._SETTINGS
        End Get
    End Property

#End Region

#Region "Methods (2)"

    Private Sub ScanDirectory(dir As DirectoryInfo, knownFiles As HashSet(Of HashedFile))
        If Me.Settings.Recursive Then
            For Each subDir As DirectoryInfo In dir.EnumerateDirectories()
                Me.ScanDirectory(subDir, knownFiles)
            Next
        End If

        For Each file As FileInfo In dir.EnumerateFiles() _
                                        .OrderBy(Function(x)
                                                     Return x.FullName.Length
                                                 End Function) _
                                        .ThenBy(Function(x)
                                                    Return x.FullName
                                                End Function, StringComparer.CurrentCultureIgnoreCase)

            Try
                Console.Write("Checking '{0}'... ", file.FullName)

                Dim hashed As HashedFile = HashedFile.FromPath(file.FullName)

                Dim existingEntry As HashedFile = knownFiles.FirstOrDefault(Function(x)
                                                                                Return x = hashed
                                                                            End Function)

                If existingEntry Is Nothing Then
                    knownFiles.Add(hashed)

                    ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                                 ConsoleColor.Green)
                Else
                    existingEntry.AddDuplicate(hashed)

                    ConsoleHelper.InvokeForColor(Sub()
                                                     Console.WriteLine("[DUPLICATE!]")
                                                 End Sub, ConsoleColor.Yellow)
                End If
            Catch ex As Exception
                ConsoleHelper.InvokeForColor(Sub()
                                                 Console.WriteLine("[ERROR: {0}]", _
                                                                   ex.GetType().FullName)
                                             End Sub, ConsoleColor.Red)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Starts the operation.
    ''' </summary>
    Public Sub Start()
        Dim knownFiles As HashSet(Of HashedFile) = New HashSet(Of HashedFile)
        For Each dir As DirectoryInfo In Me.Settings.Directories
            Try
                Me.ScanDirectory(dir, knownFiles)
            Catch ex As Exception
                ConsoleHelper.InvokeForColor(Sub()
                                                 Console.WriteLine("[ERROR: {0}]", _
                                                                   ex.GetType().FullName)
                                             End Sub, ConsoleColor.Red)
            End Try
        Next

        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine()

        Dim withDuplicates() As HashedFile = knownFiles.Where(Function(x) x.HasDuplicates) _
                                                       .ToArray()

        If Me.Settings.DeleteDuplicates Then
            For Each duplicate As HashedFile In withDuplicates.SelectMany(Function(x) x.Duplicates)
                Try
                    Console.Write("Delete duplicate '{0}'... ", duplicate.Path)

                    Dim file As FileInfo = New FileInfo(duplicate.Path)
                    If file.Exists Then
                        file.Delete()
                    End If

                    ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                                 ConsoleColor.Green)
                Catch ex As Exception
                    ConsoleHelper.InvokeForColor(Sub()
                                                     Console.WriteLine("[ERROR: {0}]", _
                                                                       ex.GetType().FullName)
                                                 End Sub, ConsoleColor.Red)
                End Try
            Next

            Console.WriteLine()
            Console.WriteLine()
        End If

        '' create files (if defined)
        Me.WriteBashFile(withDuplicates)
        Me.WriteWindowsBatch(withDuplicates)
        Me.WritePhpFile(withDuplicates)
        Me.WriteXml(withDuplicates)

        Dim duplicateLineColor As ConsoleColor? = Nothing
        If withDuplicates.Length > 0 Then
            duplicateLineColor = ConsoleColor.Yellow
        End If

        Console.WriteLine()
        Console.WriteLine()

        Console.WriteLine("Summary")
        Console.WriteLine("======================")
        Console.WriteLine("Scanned files  : {0}", knownFiles.Sum(Function(x)
                                                                     Return CType(1 + CType(x.Duplicates.Count, Long), ULong)
                                                                 End Function))
        Console.WriteLine("With duplicates: {0}", withDuplicates.Length)
        ConsoleHelper.InvokeForColor(Sub()
                                         Console.WriteLine("Duplicates     : {0}", knownFiles.Sum(Function(x As HashedFile) As ULong
                                                                                                      Return CType(x.Duplicates.Count, ULong)
                                                                                                  End Function), duplicateLineColor)
                                     End Sub, duplicateLineColor)
    End Sub

    Private Sub WriteBashFile(withDuplicates As IList(Of HashedFile))
        If Me.Settings.BashScript Is Nothing Then
            '' not defined
            Return
        End If

        Try
            Console.Write("Create bash script in '{0}'... ", Me.Settings.BashScript.FullName)

            Me.Settings.BashScript.Refresh()
            If Me.Settings.BashScript.Exists Then
                Me.Settings.BashScript.Delete()
                Me.Settings.BashScript.Refresh()
            End If

            Using stream As FileStream = Me.Settings.BashScript.OpenWrite()
                Using writer As StreamWriter = New StreamWriter(stream, Encoding.UTF8)
                    writer.WriteLine("#!/bin/bash")
                    writer.WriteLine("# The script was generated by DuplicateFinder (https://github.com/mkloubert/DuplicateFinder)")
                    writer.WriteLine("# ")
                    writer.WriteLine("# It deletes all found duplicate files.")
                    writer.WriteLine()

                    For Each file As HashedFile In withDuplicates
                        writer.WriteLine()

                        writer.WriteLine("# FILE: '{0}'", file.Path)
                        writer.WriteLine("# ID  : {0}::{1}", file.HashString, file.Size)
                        writer.WriteLine("# ")
                        writer.WriteLine("# rm ""{0}""", file.Path)

                        For Each duplicate As HashedFile In file.Duplicates
                            writer.WriteLine("rm ""{0}""", duplicate.Path)
                        Next
                    Next
                End Using

                ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                             ConsoleColor.Green)
            End Using
        Catch ex As Exception
            ConsoleHelper.InvokeForColor(Sub()
                                             Console.WriteLine("[ERROR: {0}]", _
                                                               ex.GetType().FullName)
                                         End Sub, ConsoleColor.Red)
        End Try
    End Sub

    Private Sub WritePhpFile(withDuplicates As IList(Of HashedFile))
        If Me.Settings.PhpScript Is Nothing Then
            '' not defined
            Return
        End If

        Try
            Console.Write("Create PHP script in '{0}'... ", Me.Settings.PhpScript.FullName)

            Me.Settings.PhpScript.Refresh()
            If Me.Settings.PhpScript.Exists Then
                Me.Settings.PhpScript.Delete()
                Me.Settings.PhpScript.Refresh()
            End If

            Using stream As FileStream = Me.Settings.PhpScript.OpenWrite()
                Using writer As StreamWriter = New StreamWriter(stream, Encoding.UTF8)
                    writer.WriteLine("#!/usr/bin/php")
                    writer.WriteLine("<?php")
                    writer.WriteLine("# The script was generated by DuplicateFinder (https://github.com/mkloubert/DuplicateFinder)")
                    writer.WriteLine("# ")
                    writer.WriteLine("# It deletes all found duplicate files.")
                    writer.WriteLine()

                    For Each file As HashedFile In withDuplicates
                        writer.WriteLine()

                        writer.WriteLine("// FILE: '{0}'", file.Path)
                        writer.WriteLine("// ID  : {0}::{1}", file.HashString, file.Size)
                        writer.WriteLine("// ")
                        writer.WriteLine("// unlink(""{0}"")", file.Path.Replace("\", "\\"))

                        For Each duplicate As HashedFile In file.Duplicates
                            writer.WriteLine("unlink(""{0}"")", duplicate.Path.Replace("\", "\\"))
                        Next
                    Next
                End Using

                ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                             ConsoleColor.Green)
            End Using
        Catch ex As Exception
            ConsoleHelper.InvokeForColor(Sub()
                                             Console.WriteLine("[ERROR: {0}]", _
                                                               ex.GetType().FullName)
                                         End Sub, ConsoleColor.Red)
        End Try
    End Sub

    Private Sub WriteWindowsBatch(withDuplicates As IList(Of HashedFile))
        If Me.Settings.WindowsBatch Is Nothing Then
            '' not defined
            Return
        End If

        Try
            Console.Write("Create Windows batch file in '{0}'... ", Me.Settings.WindowsBatch.FullName)

            Me.Settings.WindowsBatch.Refresh()
            If Me.Settings.WindowsBatch.Exists Then
                Me.Settings.WindowsBatch.Delete()
                Me.Settings.WindowsBatch.Refresh()
            End If

            Using stream As FileStream = Me.Settings.WindowsBatch.OpenWrite()
                Using writer As StreamWriter = New StreamWriter(stream, Encoding.Default)
                    writer.WriteLine("REM The script was generated by DuplicateFinder (https://github.com/mkloubert/DuplicateFinder)")
                    writer.WriteLine("REM ")
                    writer.WriteLine("REM It deletes all found duplicate files.")
                    writer.WriteLine()
                    writer.WriteLine()

                    writer.WriteLine("@ECHO OFF")
                    writer.WriteLine("CLS")

                    For Each file As HashedFile In withDuplicates
                        writer.WriteLine()

                        writer.WriteLine("REM FILE: '{0}'", file.Path)
                        writer.WriteLine("REM ID  : {0}::{1}", file.HashString, file.Size)
                        writer.WriteLine("REM ")
                        writer.WriteLine("REM del ""{0}""", file.Path)

                        For Each duplicate As HashedFile In file.Duplicates
                            writer.WriteLine("del ""{0}""", duplicate.Path)
                        Next
                    Next
                End Using

                ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                             ConsoleColor.Green)
            End Using
        Catch ex As Exception
            ConsoleHelper.InvokeForColor(Sub()
                                             Console.WriteLine("[ERROR: {0}]", _
                                                               ex.GetType().FullName)
                                         End Sub, ConsoleColor.Red)
        End Try
    End Sub

    Private Sub WriteXml(withDuplicates As IList(Of HashedFile))
        If Me.Settings.XmlFile Is Nothing Then
            '' not defined
            Return
        End If

        Try
            Console.Write("Create XML file in '{0}'... ", Me.Settings.XmlFile.FullName)

            Me.Settings.XmlFile.Refresh()
            If Me.Settings.XmlFile.Exists Then
                Me.Settings.XmlFile.Delete()
                Me.Settings.XmlFile.Refresh()
            End If

            Using stream As FileStream = Me.Settings.XmlFile.OpenWrite()
                Dim xml As XDocument = New XDocument(New XDeclaration("1.0", Encoding.UTF8.WebName, "yes"))

                Dim rootElement As XElement = New XElement("duplicates")

                For Each file As HashedFile In withDuplicates
                    Dim newFileElement As XElement = New XElement("file")
                    newFileElement.SetAttributeValue("path", file.Path)
                    newFileElement.SetAttributeValue("hash", file.HashString)
                    newFileElement.SetAttributeValue("size", file.Size)

                    For Each duplicate As HashedFile In file.Duplicates
                        Dim newDuplicateElement As XElement = New XElement("duplicate")
                        newDuplicateElement.SetAttributeValue("path", duplicate.Path)

                        newFileElement.Add(newDuplicateElement)
                    Next

                    rootElement.Add(newFileElement)
                Next

                xml.Add(rootElement)

                xml.Save(stream)

                ConsoleHelper.InvokeForColor(Sub() Console.WriteLine("[OK]"), _
                                             ConsoleColor.Green)
            End Using
        Catch ex As Exception
            ConsoleHelper.InvokeForColor(Sub()
                                             Console.WriteLine("[ERROR: {0}]", _
                                                               ex.GetType().FullName)
                                         End Sub, ConsoleColor.Red)
        End Try
    End Sub

#End Region

End Class