// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.dist
{
    public interface Operator
    {
        double result(double operand1, double operand2);
        string getDescription();

        static readonly Operator PLUS = new OperatorImpl((a, b) => a + b, "plus");
        static readonly Operator MINUS = new OperatorImpl((a, b) => a - b, "minus");
        static readonly Operator MULTIPLY = new OperatorImpl((a, b) => a * b, "multiply");
        static readonly Operator DIVIDE = new OperatorImpl((a, b) => a / b, "divide");
        static readonly Operator POW = new OperatorImpl((a, b) => Math.Pow(a, b), "pow");
        static readonly Operator ABS_DIFF = new OperatorImpl((a, b) => Math.Abs(a - b), "diff");
    }

    internal sealed class OperatorImpl : Operator
    {
        private readonly Func<double, double, double> _fn;
        private readonly string _desc;
        internal OperatorImpl(Func<double, double, double> fn, string desc) { _fn = fn; _desc = desc; }
        public double result(double a, double b) => _fn(a, b);
        public string getDescription() => _desc;
    }
}
