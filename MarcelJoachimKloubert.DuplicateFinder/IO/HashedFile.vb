'' LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
''
'' s. https://github.com/mkloubert/DuplicateFinder

Imports System.Security.Cryptography
Imports System.IO

''' <summary>
''' A hashed file.
''' </summary>
Public NotInheritable Class HashedFile
    Implements IEquatable(Of HashedFile)

#Region "Fields (6)"

    Private _DUPLICATEOF As List(Of HashedFile)
    Private ReadOnly _DUPLICATES As List(Of HashedFile)
    Private ReadOnly _HASH As Byte()
    Private ReadOnly _PATH As String
    Private ReadOnly _SIZE As Long
    Private ReadOnly _SYNC As Object = New Object()

#End Region

#Region "Constructors (1)"

    Private Sub New(path As String, hash() As Byte, size As Long)
        Me._DUPLICATEOF = New List(Of HashedFile)()
        Me._DUPLICATES = New List(Of HashedFile)()
        Me._HASH = hash
        Me._PATH = path
        Me._SIZE = size
    End Sub

#End Region

#Region "Properties (7)"

    ''' <summary>
    ''' Gets the current list of files that entry is duplicate of.
    ''' </summary>
    Public ReadOnly Property DuplicateOf As IList(Of HashedFile)
        Get
            Return Me._DUPLICATEOF
        End Get
    End Property

    ''' <summary>
    ''' Gets the current list of duplicates.
    ''' </summary>
    Public ReadOnly Property Duplicates As IList(Of HashedFile)
        Get
            Return Me._DUPLICATES
        End Get
    End Property

    ''' <summary>
    ''' Gets if that file has duplicates or not.
    ''' </summary>
    Public ReadOnly Property HasDuplicates As Boolean
        Get
            SyncLock Me._SYNC
                Return Me._DUPLICATES.Count > 0
            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Gets the MD5 hash.
    ''' </summary>
    Public ReadOnly Property Hash As Byte()
        Get
            Return Me._HASH
        End Get
    End Property

    ''' <summary>
    ''' Gets a lower case hex string of <see cref="HashedFile.Hash" /> property value.
    ''' </summary>
    Public ReadOnly Property HashString As String
        Get
            Return String.Concat(Me.Hash.Select(Function(x)
                                                    Return x.ToString("x2")
                                                End Function))
        End Get
    End Property

    ''' <summary>
    ''' Gets the full path.
    ''' </summary>
    Public ReadOnly Property Path As String
        Get
            Return Me._PATH
        End Get
    End Property

    ''' <summary>
    ''' Gets the size in bytes.
    ''' </summary>
    Public ReadOnly Property Size As Long
        Get
            Return Me._SIZE
        End Get
    End Property

#End Region

#Region "Methods (5)"

    ''' <summary>
    ''' Adds a new duplicate.
    ''' </summary>
    ''' <param name="duplicate">The duplicate to add.</param>
    ''' <returns>Item was added or not.</returns>
    Public Function AddDuplicate(duplicate As HashedFile) As Boolean
        If Not Object.ReferenceEquals(duplicate, Me) Then
            SyncLock Me._SYNC
                If Not Me._DUPLICATES.Any(Function(x) Object.ReferenceEquals(x, duplicate)) Then
                    If Not duplicate._DUPLICATEOF.Any(Function(x) Object.ReferenceEquals(x, Me)) Then
                        SyncLock duplicate._SYNC
                            duplicate._DUPLICATEOF.Add(Me)
                        End SyncLock
                    End If

                    Me._DUPLICATES.Add(duplicate)
                    Return True
                End If
            End SyncLock
        End If

        Return False
    End Function

    ''' <inheriteddoc />
    Public Overloads Function Equals(other As HashedFile) As Boolean Implements IEquatable(Of HashedFile).Equals
        If other IsNot Nothing Then
            Return (other._SIZE = Me._SIZE) AndAlso _
                   other._HASH.SequenceEqual(Me._HASH)
        End If

        Return False
    End Function

    ''' <inheriteddoc />
    Public Overrides Function Equals(other As Object) As Boolean
        If TypeOf other Is HashedFile Then
            Return Me.Equals(CType(other, HashedFile))
        End If

        Return MyBase.Equals(other)
    End Function

    ''' <summary>
    ''' Creates a new instance from an existing file.
    ''' </summary>
    ''' <param name="path">The path of the existing file.</param>
    ''' <returns>The new instance.</returns>
    Public Shared Function FromPath(path As String) As HashedFile
        Dim file As FileInfo = New FileInfo(path)

        Using fileStream As FileStream = file.OpenRead()
            Using md5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider()
                Return New HashedFile(file.FullName, _
                                      md5.ComputeHash(fileStream), _
                                      file.Length)
            End Using
        End Using
    End Function

    ''' <inheriteddoc />
    Public Overrides Function GetHashCode() As Integer
        Return MyBase.GetHashCode()
    End Function

#End Region

#Region "Operators (2)"

    ''' <summary>
    ''' Compares two objects if they are equal.
    ''' </summary>
    ''' <param name="left">The left object.</param>
    ''' <param name="right">The right object.</param>
    ''' <returns>Are equal or not.</returns>
    Public Shared Operator =(left As HashedFile, right As HashedFile) As Boolean
        Return Object.Equals(left, right)
    End Operator

    ''' <summary>
    ''' Compares two objects if they are NOT equal.
    ''' </summary>
    ''' <param name="left">The left object.</param>
    ''' <param name="right">The right object.</param>
    ''' <returns>Are equal (<see langword="False" />) or not (<see langword="True" />).</returns>
    Public Shared Operator <>(left As HashedFile, right As HashedFile) As Boolean
        Return Not left = right
    End Operator

#End Region

End Class