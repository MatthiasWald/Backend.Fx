﻿namespace Backend.Fx.EfCorePersistence.Mssql
{
    using System;

    // borrowed from http://www.blackbeltcoder.com/articles/strings/a-text-parsing-helper-class
    // licensed under the CPOL
    public class TextParser
    {
        public static char NullChar = (char) 0;

        public TextParser()
        {
            Reset(null);
        }

        public TextParser(string text)
        {
            Reset(text);
        }

        public string Text { get; private set; }
        public int Position { get; private set; }

        public int Remaining => Text.Length - Position;

        /// <summary>
        ///     Indicates if the current position is at the end of the current document
        /// </summary>
        public bool EndOfText => Position >= Text.Length;

        /// <summary>
        ///     Extracts a substring from the specified position to the end of the text
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public string Extract(int start)
        {
            return Extract(start, Text.Length);
        }

        /// <summary>
        ///     Extracts a substring from the specified range of the current text
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string Extract(int start, int end)
        {
            return Text.Substring(start, end - start);
        }

        /// <summary>
        ///     Moves the current position ahead one character
        /// </summary>
        public void MoveAhead()
        {
            MoveAhead(1);
        }

        /// <summary>
        ///     Moves the current position ahead the specified number of characters
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead)
        {
            Position = Math.Min(Position + ahead, Text.Length);
        }

        /// <summary>
        ///     Moves to the next occurrence of any character that is not one
        ///     of the specified characters
        /// </summary>
        /// <param name="chars">Array of characters to move past</param>
        public void MovePast(char[] chars)
        {
            while (IsInArray(Peek(), chars))
            {
                MoveAhead();
            }
        }

        /// <summary>
        ///     Moves the current position to the next character that is not whitespace
        /// </summary>
        public void MovePastWhitespace()
        {
            while (char.IsWhiteSpace(Peek()))
            {
                MoveAhead();
            }
        }

        /// <summary>
        ///     Moves to the next occurrence of the specified string
        /// </summary>
        /// <param name="s">String to find</param>
        /// <param name="ignoreCase">
        ///     Indicates if case-insensitive comparisons
        ///     are used
        /// </param>
        public void MoveTo(string s, bool ignoreCase = false)
        {
            Position = Text.IndexOf(s, Position, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (Position < 0)
            {
                Position = Text.Length;
            }
        }

        /// <summary>
        ///     Moves to the next occurrence of the specified character
        /// </summary>
        /// <param name="c">Character to find</param>
        public void MoveTo(char c)
        {
            Position = Text.IndexOf(c, Position);
            if (Position < 0)
            {
                Position = Text.Length;
            }
        }

        /// <summary>
        ///     Moves to the next occurrence of any one of the specified
        ///     characters
        /// </summary>
        /// <param name="chars">Array of characters to find</param>
        public void MoveTo(char[] chars)
        {
            Position = Text.IndexOfAny(chars, Position);
            if (Position < 0)
            {
                Position = Text.Length;
            }
        }

        /// <summary>
        ///     Moves the current position to the first character that is part of a newline
        /// </summary>
        public void MoveToEndOfLine()
        {
            var c = Peek();
            while (c != '\r' && c != '\n' && !EndOfText)
            {
                MoveAhead();
                c = Peek();
            }
        }

        /// <summary>
        ///     Returns the character at the current position, or a null character if we're
        ///     at the end of the document
        /// </summary>
        /// <returns>The character at the current position</returns>
        public char Peek()
        {
            return Peek(0);
        }

        /// <summary>
        ///     Returns the character at the specified number of characters beyond the current
        ///     position, or a null character if the specified position is at the end of the
        ///     document
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead)
        {
            var pos = Position + ahead;
            if (pos < Text.Length)
            {
                return Text[pos];
            }
            return NullChar;
        }

        /// <summary>
        ///     Resets the current position to the start of the current document
        /// </summary>
        public void Reset()
        {
            Position = 0;
        }

        /// <summary>
        ///     Sets the current document and resets the current position to the start of it
        /// </summary>
        /// <param name="pText"></param>
        public void Reset(string pText)
        {
            Text = pText ?? string.Empty;
            Position = 0;
        }

        /// <summary>
        ///     Determines if the specified character exists in the specified
        ///     character array.
        /// </summary>
        /// <param name="c">Character to find</param>
        /// <param name="chars">Character array to search</param>
        /// <returns></returns>
        protected bool IsInArray(char c, char[] chars)
        {
            foreach (var ch in chars)
            {
                if (c == ch)
                {
                    return true;
                }
            }
            return false;
        }
    }
}