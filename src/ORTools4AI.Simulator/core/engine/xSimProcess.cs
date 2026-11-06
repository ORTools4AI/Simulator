// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.dist;
using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORTools4AI.Simulator.core.engine
{
    //public abstract class SimProcess:SimProcess
    //{
    //    public SimProcess(Model owner, string name, bool repeating, bool showInTrace) : base(owner, name, repeating, showInTrace)
    //    {
    //    }

    //    public SimProcess(Model owner, string name, bool showInTrace) : base(owner, name, showInTrace)
    //    {

    //    }

    //    /**
    //    * Override this method in a subclass of SimProcess to implement the
    //    * specific behaviour of this SimProcess. This method starts after a SimProcess
    //    * has been created and activated. Note that this method will be executed once 
    //    * or repeatedly, depending on the repeating status of the SimProcess.  
    //    */
    //    public abstract void lifeCycle();

    //    /**
    // * Starts the simthread associated with this SimProcess. This is method must
    // * be called the first time a SimProcess is supposed to start processing
    // * its <code>lifeCycle()</code> method.
    // */
    //    public override void start()
    //    {

    //        // set up simthread
    //        _myThread = new SimThread(getModel().getExperiment().getThreadGroup(), this);

    //        // setting this flag shows that the simthread is now ready to take over
    //        // control from the scheduler's thread
    //        _isRunning = true;

    //        // start thread and let it run into the block
    //        _myThread.start();

    //        // put thread in to a wait for synchronization
    //        try
    //        {
    //            _myThread.WaitThreadRun();

    //        }
    //        catch (InterruptException irqEx)
    //        {
    //            // create eror message
    //            ErrorMessage errmsg = new ErrorMessage(getModel(),
    //                    "Simulation stopped!", "Exception thrown by Java VM"
    //                            + irqEx, "Thread conflict assumed.",
    //                    "Check Java VM.", presentTime());
    //            // throw it back to Experiment's start routine
    //            throw (new DESMOJException(errmsg));
    //        }

    //        // check if simulation has been stopped in between and throw SimFinished
    //        if (getModel().getExperiment().isAborted())
    //        {
    //            throw (new SimFinishedException(getModel(),
    //                    getName(), presentTime()));
    //        }
    //    }

    //    /**
    // * Passivates the SimProcess for an indefinite time. This method must be
    // * called by the SimProcess' own Thread only. The SimProcess can only be
    // * reactivated by another SimProcess or Entity.
    // */
    //    public  void passivate()
    //    {

    //        if (currentlySendTraceNotes())
    //        {
    //            if (this == currentSimProcess())
    //            {
    //                sendTraceNote("passivates");
    //            }
    //            else
    //            {
    //                sendTraceNote("passivates " + getQuotedName());
    //            }
    //        }

    //        //notify(); // frees the scheduler after wait()

    //        _myThread.RunScheduler();

    //        try
    //        {
    //            //wait();
    //            _myThread.WaitSchedulerRun();
    //        }
    //        catch (InterruptException ioEx)
    //        {
    //            // create eror message
    //            ErrorMessage errmsg = new ErrorMessage(getModel(),
    //                    "Simulation stopped!",
    //                    "Exception thrown by Java VM" + ioEx,
    //                    "Thread conflict assumed.", "Check Java VM.", presentTime());
    //            // throw it back to Experiment's start routine
    //            throw (new DESMOJException(errmsg));
    //        }

    //        // if simulation is not running, throw SimFinishedException to stop
    //        // thread
    //        if (getModel().getExperiment().isAborted())
    //        {
    //            throw (new SimFinishedException(getModel(),
    //                    getName(), presentTime()));
    //        }

    //        if (_irqException != null)
    //        {
    //            // The SimProcess has been interrupted. First reset the
    //            // _irqException, then throw the exception so that it can be caught
    //            // in the lifeCycle() method of this SimProcess.

    //            if (currentlySendTraceNotes())
    //            {
    //                sendTraceNote("throwing "
    //                        + _irqException.GetType().Name
    //                        + " to interrupt the process");
    //            }

    //            InterruptException tmpIrqException = _irqException;
    //            _irqException = null;

    //            throw tmpIrqException;
    //        }
    //    }


    //    /**
    // * Used to synchronize the change of control between scheduler and
    // * SimProcesses. This method must only be called by the scheduler resp. the
    // * experiment's main thread in order to prevent multiple SimProcess' threads
    // * running in parallel which has to be avoided.
    // */
    //    public override void resume()
    //    {

    //        // check that the SimThread has not finished yet
    //        if (_isTerminated)
    //        {
    //            sendWarning(
    //                    "Can't activate SimProcess! Command ignored.",
    //                    "SimProcess : " + getName() + " Method: void resume()",
    //                    "The SimProcess' lifeCycle method has already terminated.",
    //                    "Be sure to check the SimProcess' status before resuming."
    //                            + " Use method isTerminated() to check the current status");
    //            return;
    //        }

    //        // wake up the SimThread waiting in a block for the SimProcess' lock
    //        // to be released
    //        //notify();
    //        _myThread.RunThread();
    //        // now go wait until the next notification by the SimThread
    //        // of this SimProcess
    //        try
    //        {
    //            _myThread.WaitThreadRun();
    //        }
    //        catch (InterruptException irqEx)
    //        { // must be caught when using
    //          // wait
    //          // create eror message
    //            ErrorMessage errmsg = new ErrorMessage(getModel(),
    //                    "Simulation stopped!",
    //                    "InterruptedException thrown by Java VM : " + irqEx,
    //                    "Thread conflict assumed.", "Check Java VM.", presentTime());
    //            // throw it back to Experiment's start routine
    //            throw (new DESMOJException(errmsg));
    //        }

    //    }

    //    public override void hold(TimeSpan dt)
    //    {
    //        base.hold(dt);

    //        // hand control over to scheduler only if this is
    //        // a running thread of SimProcess

    //        passivate();
    //    }

    //    public override void hold(NumericalDist<Distribution> dist)
    //    {
    //        base.hold(dist); 
    //        // hand control over to scheduler only if this is
    //        // a running thread of SimProcess
    //        passivate();
    //    }

    //    public override void hold(TimeInstant until)
    //    {
    //        base.hold(until);

    //        // hand control over to scheduler only if this is
    //        // a running thread of SimProcess
    //        passivate();
    //    }
    //}
}
