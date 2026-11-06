// Derived from DESMO-J through a C# port.
// SPDX-License-Identifier: Apache-2.0
// @version DESMO-J, Ver. 2.4.1 copyright (c) 2014
// @author Tim Lechler
// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.report;
using ORTools4AI.Simulator.core.engine;

namespace ORTools4AI.Simulator.core.dist
{

    /**
     * Reports all information about a ContDistExponential 
     * (or ContDistweibull) distribution.
     */
    public class ContDistExponReporter : DistributionReporter
    {
        /// <summary>Creates a new ContDistExponentialReporter.</summary>
        /// <param name="informationSource">The ContDistExponential distribution to report about</param>
        public ContDistExponReporter(
                Reportable informationSource) : base(informationSource)
        {
            groupID = 153;

        }

        /// <summary>
        /// Returns the array of strings containing all information about the ContDistExponential
        /// distribution.
        /// </summary>
        /// <returns>
        /// The array of Strings containing all information about the ContDistExponential distribution
        /// </returns>
        public string[] getEntries()
        {

            //if (source  is ContDistWeibull) {

            //       // use casted ide as a shortcut for source
            //       desmoj.core.dist.ContDistWeibull rdw = (desmoj.core.dist.ContDistWeibull) source;
            //       // Title
            //       entries[0] = rdw.getName();
            //       // (Re)set
            //       entries[1] = rdw.resetAt().toString();
            //       // Obs
            //       entries[2] = Long.toString(rdw.getObservations());
            //       // Type
            //       entries[3] = "Cont Weibull";
            //       // param1
            //       entries[4] = Double.toString(rdw.getMean());
            //       // param2
            //       entries[5] = Double.toString(rdw.getBeta());
            //       // param3
            //       entries[6] = " ";
            //       // seed
            //       entries[7] = Long.toString(rdw.getInitialSeed());

            //   } else

            if (source is ContDistExponential)
            {

                // use casted ide as a shortcut for source
                var rdx = (ContDistExponential)source;
                //if (source instanceof desmoj.core.dist.ContDistWeibull) {
                //    desmoj.core.dist.ContDistWeibull rdw = (desmoj.core.dist.ContDistWeibull) rdx;
                //}
                // Title
                entries[0] = rdx.getName();
                // (Re)set
                entries[1] = rdx.resetAt().toString();
                // Obs
                entries[2] = rdx.getObservations().ToString();
                // Type
                entries[3] = "Cont Exponential";
                // param1
                entries[4] = rdx.getMean().ToString();
                // param2
                entries[5] = " ";
                // param3
                entries[6] = " ";
                // seed
                entries[7] = rdx.getInitialSeed().ToString();

            }
            else
            {

                for (int i = 0; i < numColumns; i++)
                {
                    entries[i] = "Invalid source!";
                }

            }

            return entries;

        }
    }
}