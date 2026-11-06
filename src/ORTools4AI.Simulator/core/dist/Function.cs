// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.dist
{
    public interface Function
    {
        string getDescription();
        double value(double x);
    }
}
