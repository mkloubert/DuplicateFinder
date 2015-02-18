'' LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
''
'' s. https://github.com/mkloubert/DuplicateFinder

''' <summary>
''' Helper module for console operations.
''' </summary>
Module ConsoleHelper

#Region "Methods (1)"

    ''' <summary>
    ''' Invokes an action for a specific fore and background color.
    ''' </summary>
    ''' <param name="action">The action to invoke.</param>
    ''' <param name="foreColor">If defined: The text color to set.</param>
    ''' <param name="bgColor">If defined: The background color to set.</param>
    ''' <remarks>
    ''' Original colors are restored after invokation.
    ''' </remarks>
    Public Sub InvokeForColor(action As Action, Optional foreColor As ConsoleColor? = Nothing, Optional bgColor As ConsoleColor? = Nothing)
        Dim oldBG As ConsoleColor = Console.BackgroundColor
        Dim oldFG As ConsoleColor = Console.ForegroundColor

        Try
            If bgColor.HasValue Then
                Console.BackgroundColor = bgColor.Value
            End If

            If foreColor.HasValue Then
                Console.ForegroundColor = foreColor.Value
            End If

            action()
        Finally
            Console.BackgroundColor = oldBG
            Console.ForegroundColor = oldFG
        End Try
    End Sub

#End Region

End Module
