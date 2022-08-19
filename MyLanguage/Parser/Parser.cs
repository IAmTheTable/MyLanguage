using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLanguage.Parser
{
    internal class Parser
    {
        internal enum TokenType
        {
            Invalid,

            Add,
            Sub,
            Mul,
            Div,
            Pow,
            Mod,

            LParen,
            RParen,
            LBrack,
            RBrack,
            LSqBrack,
            RSqBrack,

            Not,
            Gt,
            Lt,
            Eq,
            LtEq,
            GtEq,

            And,
            Or,
            Xor,

            Lsl,
            Lsr,

            Space,
            Identifier,

            EOL,
            EOF,

            PreProc,
        }

        internal Dictionary<string, TokenType> TokenMapping = new()
        {
            {"+", TokenType.Add},
            {"-", TokenType.Sub},
            {"*", TokenType.Mul},
            {"/", TokenType.Div},
            {"^", TokenType.Pow},
            {"%", TokenType.Mod},

            {"(", TokenType.LParen},
            {")", TokenType.RParen},

            {"{", TokenType.LBrack},
            {"}", TokenType.RBrack},

            {"[", TokenType.LSqBrack},
            {"]", TokenType.RSqBrack},

            {"!", TokenType.Not},
            {">", TokenType.Gt},
            {"<", TokenType.Lt},
            {">=", TokenType.GtEq},
            {"<=", TokenType.LtEq},

            {"=", TokenType.LtEq},
            {"<<", TokenType.Lsl},
            {">>", TokenType.Lsr},

            {"#", TokenType.PreProc},
            {"&", TokenType.And},
            {"|", TokenType.Or},
            {";", TokenType.EOL},
            {"\0", TokenType.EOF},
            {" ", TokenType.Space}
        };

        private string _parseData = "";
        private int _currentPos = 0;

        /// <summary>
        ///  The constructor for the parser class.
        /// </summary>
        /// <param name="input">The input string that it will parse.</param>
        public Parser(string input)
        {
            this._parseData = input;
        }

        class Token
        {
            public TokenType type { get; set; }
            public object value { get; set; }
        }

        public bool Parse()
        {
            List<Token> tokens = new();

            int step = 0;
            while (true)
            {
                if (IsNumber(true, out int numRes))
                {
                    tokens.Add(new() { type = TokenType.Identifier, value = numRes });
                }
                else if (IsToken(true, out TokenType tokenRes))
                {
                    if (tokenRes == TokenType.Space)
                        continue;

                    tokens.Add(new()
                    {
                        type = tokenRes,
                        value = ""
                    });

                    if (tokenRes == TokenType.EOL)
                        break;
                }

                step++; // increase only at the end so we know where it stopped at.
            }


            // basic stack environment

            int result = 0;

            while (tokens.Count > 1)
            {
                if (tokens.Any(x => x.type == TokenType.Mul))
                {
                    var baseIdx = tokens.FindIndex(x => x.type == TokenType.Mul);

                    if (baseIdx == -1)
                    {
                        //No multiplication found.
                        Console.WriteLine("no multiplication needed.");
                    }
                    else
                    {
                        var result2 = (int)tokens[baseIdx - 1].value * (int)tokens[baseIdx + 1].value;

                        if (tokens[baseIdx + 2].type != TokenType.EOF)
                            tokens.Remove(tokens[baseIdx + 2]);

                        tokens.Remove(tokens[baseIdx + 1]);
                        tokens.Remove(tokens[baseIdx]);
                        tokens.Remove(tokens[baseIdx - 1]);

                        tokens.Add(new() { type = TokenType.Identifier, value = result2 });
                    }
                }
                else if (tokens.Any(x => x.type == TokenType.Div))
                {
                    var baseIdx = tokens.FindIndex(x => x.type == TokenType.Div);

                    if (baseIdx == -1)
                    {
                        //No multiplication found.
                        Console.WriteLine("no division needed.");
                    }
                    else
                    {
                        var result2 = (int)tokens[baseIdx - 1].value / (int)tokens[baseIdx + 1].value;

                        tokens.Remove(tokens[baseIdx + 1]);
                        tokens.Remove(tokens[baseIdx]);
                        tokens.Remove(tokens[baseIdx - 1]);

                        tokens.Add(new() { type = TokenType.Identifier, value = result2 });
                    }
                }
                else if (tokens.Any(x => x.type == TokenType.Add) || tokens.Any(x => x.type == TokenType.Sub))
                {
                    var baseIdx = tokens.FindIndex(x => x.type == TokenType.Add || x.type == TokenType.Sub);

                    if (baseIdx == -1)
                    {
                        //No multiplication found.
                        Console.WriteLine("no addition needed.");
                    }
                    else
                    {
                        int result2 = 0;
                        if (tokens[baseIdx].type == TokenType.Add)
                            result2 += (int)tokens[baseIdx - 1].value + (int)tokens[baseIdx + 1].value;
                        else
                            result2 += (int)tokens[baseIdx - 1].value - (int)tokens[baseIdx + 1].value;

                        tokens.Remove(tokens[baseIdx + 1]);
                        tokens.Remove(tokens[baseIdx]);
                        tokens.Remove(tokens[baseIdx - 1]);

                        tokens.Add(new() { type = TokenType.Identifier, value = result2 });
                    }
                }
            }

            Console.WriteLine($"Compiled the result to be: {tokens[0].value}");

            return false;
        }

        /// <summary>
        /// Check if the current character is a number.
        /// </summary>
        /// <param name="peak">Increase read pos</param>
        /// <param name="value">The result of the number if found.</param>
        /// <returns>True or False if the result is a number.</returns>
        public bool IsNumber(bool peak, out int value)
        {
            int start = 0;
            string cache = "";

            while (int.TryParse(this._parseData[this._currentPos + start].ToString(), out value))
            {
                cache += this._parseData[this._currentPos + start].ToString();
                start++;
            }

            if (string.IsNullOrEmpty(cache))
            {
                value = -1;
                return false;
            }

            if (peak)
                this._currentPos += start;

            value = int.Parse(cache);
            return true;
        }

        public bool IsToken(bool peak, out TokenType token)
        {
            if (peak)
            {
                int _ = this._currentPos + 1 > this._parseData.Length ? this._currentPos : this._currentPos++;
            }

            if (this.TokenMapping.ContainsKey(this._parseData[peak ? this._currentPos - 1 : this._currentPos].ToString()))
            {
                token = this.TokenMapping[this._parseData[peak ? this._currentPos - 1 : this._currentPos].ToString()];
                return true;
            }
            else
            {
                this._currentPos--;
                token = TokenType.Invalid;
                return false;
            }
        }

        /// <summary>
        /// Expect a certain token.
        /// </summary>
        /// <param name="token">The token to expect.</param>
        /// <returns>True or False if the token is found next in the sequence.</returns>
        public bool Expect(TokenType token)
        {
            return false;
        }


    }
}
