namespace Redbox.HAL.Core
{
    public class Tokenizer<T> : TokenizerBase<T>
    {
        public Tokenizer(int lineNumber, string statement)
          : base(lineNumber, statement)
        {
            this.Tokens = new TokenList();
        }

        public TokenList Tokens { get; protected set; }

        protected internal void AddTokenAndReset(TokenType type, bool isKeyValuePair)
        {
            this.Tokens.Add(new Token(type, type == TokenType.Comment ? this.m_tokenReader.RemainingTokens.Trim() : this.GetAccumulatedToken(), isKeyValuePair));
            this.ResetAccumulator();
        }

        protected internal void AddTokenAndReset(TokenType type, string pairSeparator)
        {
            this.Tokens.Add(new Token(type, type == TokenType.Comment ? this.m_tokenReader.RemainingTokens.Trim() : this.GetAccumulatedToken(), pairSeparator));
            this.ResetAccumulator();
        }
    }
}
