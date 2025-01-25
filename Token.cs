using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Core
{
    [Serializable]
    public class Token : ICloneable<Token>
    {
        public bool IsSymbolOrConst
        {
            get => this.Type == TokenType.Symbol || this.Type == TokenType.ConstSymbol;
        }

        public Token(TokenType type, string value, string pairSeparator)
          : this(type, value, true)
        {
            this.PairSeparator = pairSeparator;
        }

        public Token(TokenType type, string value, bool isKeyValuePair)
        {
            this.Type = type;
            this.Value = value;
            this.IsKeyValuePair = isKeyValuePair;
            this.PairSeparator = "=";
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            return obj is Token token && this.Value == token.Value && this.Type == token.Type && this.IsKeyValuePair == token.IsKeyValuePair;
        }

        public override string ToString()
        {
            return string.Format("Type={0}, Value={1}, IsKeyValuePair={2}", (object)this.Type, (object)this.Value, (object)this.IsKeyValuePair);
        }

        public override int GetHashCode() => this.ToString().GetHashCode();

        public object ConvertValue()
        {
            if (this.IsKeyValuePair)
                return (object)new KeyValuePair(this.Value, this.Type, this.PairSeparator);
            if (this.Type == TokenType.NumericLiteral)
                return this.Value.IndexOf(".") != -1 ? (object)Decimal.Parse(this.Value) : (object)int.Parse(this.Value);
            if (this.Type == TokenType.Operator && this.Value.IndexOf("..") != -1)
                return (object)new Range(this.Value);
            if (string.Compare(this.Value, bool.TrueString, true) == 0)
                return (object)true;
            return string.Compare(this.Value, bool.FalseString, true) == 0 ? (object)false : (object)this.Value;
        }

        public Token Clone(params object[] parms)
        {
            return new Token(this.Type, this.Value, this.IsKeyValuePair)
            {
                PairSeparator = this.PairSeparator
            };
        }

        public string Value { get; private set; }

        public TokenType Type { get; private set; }

        public bool IsKeyValuePair { get; private set; }

        public string PairSeparator { get; private set; }
    }
}
