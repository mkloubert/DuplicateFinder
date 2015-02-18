'' LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
''
'' s. https://github.com/mkloubert/DuplicateFinder

Imports System.IO

''' <summary>
''' Stores application settings.
''' </summary>
Public NotInheritable Class AppSettings

#Region "Fields (1)"

    Private ReadOnly _DIRECTORIES As List(Of DirectoryInfo)

#End Region

#Region "Constructors (1)"

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AppSettings" /> class.
    ''' </summary>
    Sub New()
        Me._DIRECTORIES = New List(Of DirectoryInfo)()
    End Sub

#End Region

#Region "Properties (6)"

    ''' <summary>
    ''' Gets or sets the output file for the BASH script.
    ''' </summary>
    Public Property BashScript As FileInfo

    ''' <summary>
    ''' Gets or sets if duplicate files should be deleted or not.
    ''' </summary>
    Public Property DeleteDuplicates As Boolean

    ''' <summary>
    ''' Gets the list of directories to scan.
    ''' </summary>
    Public ReadOnly Property Directories As List(Of DirectoryInfo)
        Get
            Return Me._DIRECTORIES
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the output file for the PHP script.
    ''' </summary>
    Public Property PhpScript As FileInfo

    ''' <summary>
    ''' Gets or sets if directories should be scanned recursivly or not.
    ''' </summary>
    Public Property Recursive As Boolean

    ''' <summary>
    ''' Gets or sets the output file for the Windows batch file.
    ''' </summary>
    Public Property WindowsBatch As FileInfo

#End Region

End Class