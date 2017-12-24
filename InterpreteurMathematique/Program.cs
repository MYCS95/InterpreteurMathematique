using System;
using System.Collections;
using System.Linq;

namespace InterpreteurMathematique
{
    class Program
    {        
        static bool sb_error_occured = false;
        static String s_expressionBuffer;
        
        static void error(String msg, ref String remainder)
        {
            if (!sb_error_occured)
            {
                Console.Out.WriteLine(msg + " at: " + remainder);
                sb_error_occured = true;
            }
        }

        static IEnumerator CopieEnum(IEnumerator enu, String s)
        {
            IEnumerator ie_enu = s.GetEnumerator();
            bool res = ie_enu.MoveNext();
            while (ie_enu.Current.ToString() != enu.Current.ToString())
                ie_enu.MoveNext();
            return ie_enu;
        }

        static bool Contient(String s, String[] t)
        {
            return t.Contains(s);
        }

        static void Move(IEnumerator enu, String s)
        {
            int size = s.Length;
            for (int i = 0; i < size-1; i++)
                enu.MoveNext();
        }

        static double strtod(IEnumerator exp, ref String endptr)
        {
            double result = 0;
            IEnumerator ie_temp;
            ie_temp = CopieEnum(exp, s_expressionBuffer);
            String toConvert = null;
            bool res;
            do
            {
                toConvert += ie_temp.Current.ToString();
                res = ie_temp.MoveNext();
            } while (res && (Contient(ie_temp.Current.ToString(), new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })));
            result = Convert.ToDouble(toConvert);
            Move(exp, toConvert);
            while (res)
            {
                endptr += ie_temp.Current.ToString();
                res = ie_temp.MoveNext();
            }
                
            if (endptr == null)
                endptr = "\0";
            return result;
        }

        static IEnumerator skipws(IEnumerator exp)
        {
            while(String.IsNullOrWhiteSpace(exp.Current.ToString()))
            {
                exp.MoveNext();
            }
            return exp;
        }

        static bool isAlpha(String s)
        {
            double d;
            return Double.TryParse(s, out d);
        }

        static double eval_factor(IEnumerator exp, ref IEnumerator end)
        {
            String s_temp;
            double result = 0;
            exp = skipws(exp);
            if(exp.Current.ToString() == "(")
            {
                exp.MoveNext();
                result = eval_exp(exp, ref end);
                if(end.Current.ToString() != ")")
                {
                    s_temp = end.Current.ToString();
                    error("missing closing parenthesis", ref s_temp);
                }
                else
                {
                    s_temp = "\0";
                    bool res = end.MoveNext();
                    if (!res)
                    {
                        end = s_temp.GetEnumerator();
                        end.MoveNext();
                    }
                        
                }
            }
            else if(exp.Current.ToString() == "+")
            {
                exp.MoveNext();
                result = eval_factor(exp, ref end);
            }
            else if(exp.Current.ToString() == "-")
            {
                exp.MoveNext();
                result = -eval_factor(exp, ref end);
            }
            else if(!isAlpha(exp.Current.ToString()))
            {
                bool res = true;
                s_temp = exp.Current.ToString();
                error("Bad function name", ref s_temp);
                while(res && !isAlpha(exp.Current.ToString()) && exp.Current.ToString() != "/" && exp.Current.ToString() != "*" && exp.Current.ToString() != "-" && exp.Current.ToString() != "+" && exp.Current.ToString() != "(" && exp.Current.ToString() != ")")
                {
                    res = exp.MoveNext();
                }   
            }
            else
            {
                String endptr = null;
                result = strtod(exp,ref endptr);
                s_expressionBuffer = String.Copy(endptr);
                end = endptr.GetEnumerator();
                end.MoveNext();
                if (end.Current.ToString() == exp.Current.ToString())
                {
                    s_temp = end.Current.ToString();
                    error("invalid number", ref s_temp);
                }
            }

            return result;
        }

        static double eval_term(IEnumerator exp, ref IEnumerator end)
        {
            string s_temp = "\0";
            end = s_temp.GetEnumerator();
            end.MoveNext();
            double result = eval_factor(exp, ref end);
            end = skipws(end);
            while(end.Current.ToString() == "*" || end.Current.ToString() == "/")
            {
                String op = end.Current.ToString();
                end.MoveNext();
                double factor = eval_factor(end, ref end);
                if(op == "*")
                {
                    result *= factor;
                }
                else if(factor != 0.0)
                {
                    result /= factor;
                }
                else
                {
                    error("divide by 0.0", ref op);
                }
                end = skipws(end);
            }
            return result;
        }

        static double eval_exp(IEnumerator exp, ref IEnumerator end)
        {
            
            double result = eval_term(exp, ref end);
            end = skipws(end);
            while(end.Current.ToString() == "+" || end.Current.ToString() == "-")
            {
                String op = end.Current.ToString();
                end.MoveNext();
                double term = eval_term(end, ref end);
                if(op == "+")
                {
                    result += term;
                }
                else
                {
                    result -= term;
                }
                end = skipws(end);
            }
            return result;
        }

        static double eval(ref IEnumerator exp)
        {
            exp.MoveNext();
            IEnumerator ie_end = null;
            double result = eval_exp(exp, ref ie_end);
            String s_end = ie_end.Current.ToString();
            if (s_end != "\0")
                error("trailing data after expression", ref s_end);

            return result;
        }

        static void Main(string[] args)
        {
            Console.Write("Entrer l'expression à évaluer : ");
            String s_expressionMathematique = Console.ReadLine();
            double d_result = 0;
            sb_error_occured = false;
            Console.Write("evaluating " + s_expressionMathematique + "\n -> ");
            IEnumerator ie_expressionMathematique = s_expressionMathematique.GetEnumerator();
            s_expressionBuffer = String.Copy(s_expressionMathematique);
            d_result = eval(ref ie_expressionMathematique);
            if (!sb_error_occured)
            {
                Console.WriteLine(d_result.ToString());
                Console.WriteLine("operation success !");
            }
            else
            {
                Console.WriteLine("operation failed !");
            }
                
            Console.ReadKey();

        }
    }
}
