// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    //public abstract class FastSimProcess : SimProcess
    //{
    //    public FastSimProcess(Model owner, string name, bool repeating, bool showInTrace) : base(owner, name, repeating, showInTrace)
    //    {
    //    }

    //    public FastSimProcess(Model owner, string name, bool showInTrace):base(owner, name, showInTrace)
    //    {

    //    }

    //    private IEnumerator<RunStatus>? _generator_function = null;

    //    public abstract IEnumerable<RunStatus> lifeCycle();
      
    //    public override void start()
    //    {
    //        // setting this flag shows that the simthread is now ready to take over
    //        // control from the scheduler's thread
    //        _isRunning = true; long run = 1;
    //        if (currentlySendTraceNotes())
    //        {
    //            if (isRepeating())
    //                sendTraceNote("starts (run #" + run + ")");
    //            else
    //                sendTraceNote("starts");
    //        }
    //        _generator_function = lifeCycle().GetEnumerator();
    //        if (_generator_function.MoveNext())
    //        {
    //            if (currentlySendTraceNotes())
    //            {
    //                if (this == currentSimProcess())
    //                {
    //                    sendTraceNote("passivates");
    //                }
    //                else
    //                {
    //                    sendTraceNote("passivates " + getQuotedName());
    //                }
    //            }

    //            var val = _generator_function.Current;
    //        }
            
    //    }

    //    public override void resume()
    //    {
    //        if (_generator_function !=null && _generator_function.MoveNext())
    //        {
    //            if (currentlySendTraceNotes())
    //            {
    //                if (this == currentSimProcess())
    //                {
    //                    sendTraceNote("passivates");
    //                }
    //                else
    //                {
    //                    sendTraceNote("passivates " + getQuotedName());
    //                }
    //            }
    //            var val = _generator_function.Current;
    //        }
    //    }

     
    //}

  

}
