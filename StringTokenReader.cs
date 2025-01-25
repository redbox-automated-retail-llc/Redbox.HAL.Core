using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    public class StringTokenReader : ITokenReader, IDisposable
    {
        private readonly string m_statement;

        public StringTokenReader(int row, string statement)
        {
            this.m_statement = statement;
            this.Row = (ushort)row;
            this.Column = (ushort)0;
        }

        public char GetCurrentToken() => this.m_statement[(int)this.Column];

        public bool MoveToNextToken()
        {
            ++this.Column;
            return (int)this.Column < this.m_statement.Length;
        }

        public void Reset() => this.Column = (ushort)0;

        public char? PeekNextToken(int i)
        {
            return (int)this.Column + i >= this.m_statement.Length ? new char?() : new char?(this.m_statement[(int)this.Column + i]);
        }

        public char? PeekNextToken() => this.PeekNextToken(1);

        public bool IsEmpty() => string.IsNullOrEmpty(this.m_statement);

        public void IncrementRowCount() => ++this.Row;

        public void Dispose()
        {
        }

        public ushort Column { get; private set; }

        public ushort Row { get; private set; }

        public string RemainingTokens => this.m_statement.Substring((int)this.Column);
    }
}
