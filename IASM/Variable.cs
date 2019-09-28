using System;
using System.Runtime.Serialization;

namespace IASM
{
    public class Variable
    {
        public int Type;

        public Value Val;

        public Variable(int Type, Value Val)
        {
            this.Type = Type;

            this.Val = Val;
        }

        public long IntValue()
        {
            if (Val is IntValue iv)
            {
                return iv.Val;
            }
            else if (Val is DoubleValue dv)
            {
                return (long)dv.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public string StringValue()
        {
            return Val.ToString();
        }

        public double DoubleValue()
        {
            if (Val is DoubleValue dv)
            {
                return dv.Val;
            }
            else if (Val is IntValue iv)
            {
                return iv.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public void SetValue(string val)
        {
            long intVal = 0;
            double doubleVal = 0;
            if (long.TryParse(val, out intVal))
            {
                Val = new IntValue(intVal);
            }
            else if (double.TryParse(val, out doubleVal))
            {
                Val = new DoubleValue(doubleVal);
            }
            else
            {
                Val = new StringValue(val);
            }
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    public interface Value
    {
        Value Add(Value b);
        Value Sub(Value b);
        Value Mul(Value b);
        Value Div(Value b);
        Value Mod(Value b);
        int Length();
        bool Eq(Value b);
        bool Gt(Value b);
        bool Lt(Value b);
    }

    public class IntValue : Value
    {
        private long val;
        public long Val { get => val; set => val = value; }

        public IntValue(long Val)
        {
            this.Val = Val;
        }

        public Value Add(Value b)
        {
            if (b is IntValue v)
            {
                return new IntValue(Val + v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val + dv.Val);
            }
            else if (b is StringValue sv)
            {
                return new StringValue(Val + sv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Div(Value b)
        {
            if (b is IntValue v)
            {
                return new IntValue(Val / v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val / dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public bool Eq(Value b)
        {
            if (b is IntValue v)
            {
                return Val == v.Val;
            }
            else
            {
                return false;
            }
        }

        public bool Gt(Value b)
        {
            if (b is IntValue v)
            {
                return Val > v.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public int Length()
        {
            return Val.ToString().Length;
        }

        public bool Lt(Value b)
        {
            if (b is IntValue v)
            {
                return Val < v.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Mod(Value b)
        {
            if (b is IntValue v)
            {
                return new IntValue(Val % v.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Mul(Value b)
        {
            if (b is IntValue v)
            {
                return new IntValue(Val * v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val * dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Sub(Value b)
        {
            if (b is IntValue v)
            {
                return new IntValue(Val * v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val * dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    [Serializable]
    internal class OperationNotImplementedException : Exception
    {
        public OperationNotImplementedException()
        {
        }

        public OperationNotImplementedException(string message) : base(message)
        {
        }

        public OperationNotImplementedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OperationNotImplementedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class DoubleValue : Value
    {
        private double val;

        public double Val { get => val; set => val = value; }

        public DoubleValue(double Val)
        {
            this.Val = Val;
        }

        public Value Add(Value b)
        {
            if (b is IntValue v)
            {
                return new DoubleValue(Val + v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val + dv.Val);
            }
            else if (b is StringValue sv)
            {
                return new StringValue(Val + sv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Div(Value b)
        {
            if (b is IntValue v)
            {
                return new DoubleValue(Val / v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val / dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public bool Eq(Value b)
        {
            if (b is DoubleValue v)
            {
                return Val == v.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public bool Gt(Value b)
        {
            if (b is DoubleValue v)
            {
                return Val < v.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public int Length()
        {
            return Val.ToString().Length;
        }

        public bool Lt(Value b)
        {
            if (b is DoubleValue v)
            {
                return Val < v.Val;
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Mod(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public Value Mul(Value b)
        {
            if (b is IntValue v)
            {
                return new DoubleValue(Val * v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val * dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Sub(Value b)
        {
            if (b is IntValue v)
            {
                return new DoubleValue(Val - v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new DoubleValue(Val - dv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }

    public class StringValue : Value
    {
        private string val;

        public string Val { get => val; set => val = value; }

        public StringValue(string Val)
        {
            this.Val = Val;
        }

        public Value Add(Value b)
        {
            if (b is IntValue v)
            {
                return new StringValue(Val + v.Val);
            }
            else if (b is DoubleValue dv)
            {
                return new StringValue(Val + dv.Val);
            }
            else if (b is StringValue sv)
            {
                return new StringValue(Val + sv.Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public Value Div(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public bool Eq(Value b)
        {
            if (b is StringValue v)
            {
                return v.Val.Equals(Val);
            }
            else
            {
                throw new OperationNotImplementedException();
            }
        }

        public bool Gt(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public int Length()
        {
            return Val.Length;
        }

        public bool Lt(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public Value Mod(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public Value Mul(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public Value Sub(Value b)
        {
            throw new OperationNotImplementedException();
        }

        public override string ToString()
        {
            return Val.ToString();
        }
    }
}
