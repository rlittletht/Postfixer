using System;
using System.Collections.Generic;
using System.Text;

namespace TCore.PostfixText
{
    // A token is a single token, as in:
    public class Value
    {
        public enum ValueType
        {
            String, // enclosed in literals
            Number, // leading digit
            DateTime, // enclosed in {}
            Field // enclosed in []
        }

        internal string m_value;
        internal StringBuilder m_sbValue;
        internal ValueType m_type;
        internal bool m_fParsingEscape = false;

        /*----------------------------------------------------------------------------
            %%Function: Value
            %%Qualified: PostfixText.Parser.Value.Value
            
            Create a new value
        ----------------------------------------------------------------------------*/
        public Value(char chFirst, ValueType type)
        {
            m_sbValue = new StringBuilder();
            // don't append the enclosing characters for strings or dates
            if (type != ValueType.DateTime && type != ValueType.String && type != ValueType.Field)
                m_sbValue.Append(chFirst);
            m_type = type;
        }

        /*----------------------------------------------------------------------------
            %%Function: FAcceptValueStart
            %%Qualified: PostfixText.Parser.Value.FAcceptValueStart
            
            will this value accept this character as the start of a value? if so, 
            create a new value. otherwise, return false and value is null
        ----------------------------------------------------------------------------*/
        public static bool FAcceptParseStart(char ch, out Value value)
        {
            value = null;

            if (ch == '[')
            {
                value = new Value(ch, ValueType.Field);
                return true;
            }

            if (char.IsDigit(ch))
            {
                value = new Value(ch, ValueType.Number);
                return true;
            }

            if (ch == '\'')
            {
                value = new Value(ch, ValueType.String);
                return true;
            }

            if (ch == '{')
            {
                value = new Value(ch, ValueType.DateTime);
                return true;
            }

            return false;
        }

        /*----------------------------------------------------------------------------
            %%Function: FinishParse
            %%Qualified: PostfixText.Parser.Value.FinishParse
            
            Finish this value parse (propagate sbValue to value, free sbValue)
        ----------------------------------------------------------------------------*/
        private void FinishParse()
        {
            m_value = m_sbValue.ToString();
            m_sbValue = null;
        }

        /*----------------------------------------------------------------------------
            %%Function: ParseNextValueChar
            %%Qualified: PostfixText.Parser.Value.ParseNextValueChar
            
            Parse the next value character. Return true to continue parsing this
            value, otherwise finish the value and return whether to unget this 
            character or not
        ----------------------------------------------------------------------------*/
        public bool ParseNextValueChar(char ch, out bool fUnget)
        {
            fUnget = false;

            switch (m_type)
            {
                case ValueType.DateTime:
                {
                    if (ch == '}')
                    {
                        FinishParse();
                        return false;
                    }

                    m_sbValue.Append(ch);
                    return true;
                }

                case ValueType.Number:
                    if (!char.IsDigit(ch))
                    {
                        if (char.IsWhiteSpace(ch) || ch == '\0')
                        {
                            FinishParse();
                            return false;
                        }

                        if (ch == '&' || ch == '|') // this terminates our parse and we push it back
                        {
                            fUnget = true;
                            FinishParse();
                            return false;
                        }

                        throw new Exception($"encountered non-digit {ch} while parsing number without intervening whitespace");
                    }

                    m_sbValue.Append(ch);
                    return true;
                case ValueType.String:
                    if (m_fParsingEscape)
                    {
                        // only allow escaped literal characters (for now)
                        if (ch != '\'')
                            throw new Exception($"only support escaped literal characters. encountered {ch}");

                        m_fParsingEscape = false;
                    }
                    else if (ch == '\'')
                    {
                        // done with parse
                        FinishParse();
                        return false;
                    }

                    if (ch == '\\')
                    {
                        m_fParsingEscape = true;
                        return true; // eat the escape character
                    }

                    // otherwise, add this character (including the fallthrough from the escaped literal)
                    m_sbValue.Append(ch);
                    return true;
                case ValueType.Field:
                {
                    if (ch == ']')
                    {
                        FinishParse();
                        return false;
                    }

                    m_sbValue.Append(ch);
                    return true;
                }
            }

            throw new Exception("unknown value type");
        }

        public void SetTokenString(string token)
        {
            m_value = token;
        }

        public string GetTokenString()
        {
            return m_value;
        }
    }
}
