using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core
{
    [Serializable]
    public class TokenList : List<Token>, ICloneable<TokenList>
    {
        public TokenList()
        {
        }

        public TokenList(IEnumerable<Token> tokens)
          : base(tokens)
        {
        }

        public Token GetLabel() => this.Find((Predicate<Token>)(each => each.Type == TokenType.Label));

        public Token GetMnemonic()
        {
            return this.Find((Predicate<Token>)(each => each.Type == TokenType.Mnemonic));
        }

        public bool HasOnlyLabel()
        {
            return this.Count > 0 && this.FindAll((Predicate<Token>)(each => each.Type == TokenType.Label)).Count == this.Count;
        }

        public bool HasOnlyComments()
        {
            return this.Count > 0 && this.FindAll((Predicate<Token>)(each => each.Type == TokenType.Comment)).Count == this.Count;
        }

        public TokenList GetSymbols()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => (each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol) && !each.Value.Equals("TRUE") && !each.Value.Equals("FALSE"))));
        }

        public TokenList GetSymbolsAndLiterals()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => each.Type == TokenType.Symbol || each.Type == TokenType.ConstSymbol || each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral)));
        }

        public TokenList GetTokensAfterMnemonic()
        {
            int index1 = this.FindIndex((Predicate<Token>)(each => each.Type == TokenType.Mnemonic));
            if (index1 == -1)
                return new TokenList();
            TokenList tokensAfterMnemonic = new TokenList();
            for (int index2 = index1 + 1; index2 < this.Count; ++index2)
                tokensAfterMnemonic.Add(this[index2]);
            return tokensAfterMnemonic;
        }

        public TokenList GetAllLiterals()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => each.Type == TokenType.StringLiteral || each.Type == TokenType.NumericLiteral)));
        }

        public TokenList GetKeyValuePairs()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => each.IsKeyValuePair)));
        }

        public KeyValuePair GetKeyValuePair(string key)
        {
            Token token = this.Find((Predicate<Token>)(each => each.IsKeyValuePair && string.Compare(((KeyValuePair)each.ConvertValue()).Key, key, true) == 0));
            return token != null ? (KeyValuePair)token.ConvertValue() : (KeyValuePair)null;
        }

        public TokenList GetNumericLiterals()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => each.Type == TokenType.NumericLiteral)));
        }

        public TokenList GetStringLiterals()
        {
            return new TokenList((IEnumerable<Token>)this.FindAll((Predicate<Token>)(each => each.Type == TokenType.StringLiteral)));
        }

        public TokenList Clone(params object[] parms)
        {
            TokenList tokenList = new TokenList();
            foreach (Token token in (List<Token>)this)
                tokenList.Add(token.Clone(new object[0]));
            return tokenList;
        }
    }
}
