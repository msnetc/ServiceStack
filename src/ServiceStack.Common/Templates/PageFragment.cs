using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;

#if NETSTANDARD1_3
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Templates
{
    public abstract class PageFragment {}

    public class PageVariableFragment : PageFragment
    {
        public StringSegment OriginalText { get; set; }
        private byte[] originalTextBytes;
        public byte[] OriginalTextBytes => originalTextBytes ?? (originalTextBytes = OriginalText.ToUtf8Bytes());
        
        public StringSegment Name { get; set; }
        private string nameString;
        public string NameString => nameString ?? (nameString = Name.Value);
        
        public object Value { get; set; }
        
        public Command[] FilterCommands { get; set; }

        public PageVariableFragment(StringSegment originalText, StringSegment name, List<Command> filterCommands)
        {
            OriginalText = originalText;
            FilterCommands = filterCommands?.ToArray() ?? TypeConstants<Command>.EmptyArray;

            ParseLiteral(name, out StringSegment outName, out object value, out Command cmd);

            Name = outName;
            Value = value;
        }

        public void ParseLiteral(StringSegment literal, out StringSegment name, out object value, out Command cmd)
        {
            name = default(StringSegment);
            value = null;
            cmd = null;

            if (literal.IsNullOrEmpty())
                return;

            if (literal.StartsWith("'") || literal.StartsWith("\""))
            {
                if (!literal.EndsWith("'") && !literal.EndsWith("\""))
                    throw new Exception($"Invalid literal: {literal} in '{OriginalText}'");

                value = literal.Substring(1, literal.Length - 2);
            }
            else if (literal.GetChar(0) >= '0' && literal.GetChar(0) <= '9' || literal.GetChar(0) == '-' || literal.GetChar(0) == '+')
            {
                //don't convert into ternary to avoid Type coercion
                if (literal.IndexOf('.') >= 0) 
                    value = double.Parse(literal.Value);
                else 
                    value = int.Parse(literal.Value);
            }
            else if (literal.Equals("true"))
            {
                value = true;
            }
            else if (literal.Equals("false"))
            {
                value = false;
            }
            else if (literal.IndexOf('(') >= 0)
            {
                cmd = literal.ParseCommands().FirstOrDefault();
            }
            else
            {
                name = literal;
            }
        }
    }

    public class PageStringFragment : PageFragment
    {
        public StringSegment Value { get; set; }

        private byte[] valueBytes;
        public byte[] ValueBytes => valueBytes ?? (valueBytes = Value.ToUtf8Bytes());

        public PageStringFragment(StringSegment value)
        {
            Value = value;
        }
    }
}