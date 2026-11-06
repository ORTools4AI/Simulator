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
	 * Base class for all pseudo random number distributions used in this package.
	 * Defines a set of methods usefull for all kinds of random distributions that
	 * can be based upon a stream of uniform distributed pseudo random numbers.
	 * Prefabricated distributions implemented in this package can handle uniform,
	 * normal (gaussian), bernoulli, poisson and heuristic distributions with return
	 * values of the primitive data types double (floating point), long (integer)
	 * and boolean (true or false). Inherit from this class if you want to implement
	 * new types of distributions handing back values of other types than those
	 * listed above. Basic idea is to use a pseudo random generator which produces a
	 * uniformly distributed stream of double numbers between 0 and 1 use inverse
	 * transformation to generate the desired distribution. See also [Page91, p.
	 * 107] Note that although this class implements all methods, it is set to be
	 * abstract, since instantiating this class would not produce any meaningfull
	 * distribution to be used by a client.
	 * 
	 * @see desmoj.core.dist.UniformRandomGenerator
	 * @see desmoj.core.dist.LinearCongruentialRandomGenerator
	 */
	public abstract class Distribution : Reportable
	{

		/**
		 * The underlying uniform pseudo random generator available to every
		 * distribution inheriting from this abstract class. Valid generators have
		 * to implement the <code>desmoj.dist.UniformRandomGenerator</code>
		 * interface. By default the <code>desmoj.dist.DefaultRandomGenerator</code>
		 * is used.
		 * 
		 * @see desmoj.core.dist.UniformRandomGenerator
		 * @see desmoj.core.dist.LinearCongruentialRandomGenerator
		 */
		protected UniformRandomGenerator randomGenerator;

		/**
		 * The status of the random number generation. If set to true, antithetic
		 * values are delivered. These depend upon the kind of distribution, so this
		 * value here will probably be most useful to switch the algorithm in the
		 * implementation of the abstract <code>sample()</code> method between
		 * "normal" and "antithetic" value generation. This feature is not
		 * associated to the pseudo random generator since the algorithm for
		 * calculating antithetic values might not require antithetic uniformly
		 * distributed values.
		 */
		protected bool antithetic;

		/**
		 * The seed of the underlying pseudorandom generator. The seed value is
		 * passed on to the underlying <code>UniformRandomGenerator</code> but since
		 * those generators are not supposed to keep track of their initial seed
		 * value it is stored here to make sure they are not lost.
		 */
		protected long initialSeed;

		/**
		 * This flag shows, if a distribution may produce negative samples or not.
		 * This is important, if the value of a distribution's sample is to be used
		 * for creating a TimeSpan object, which allows positive values only. If
		 * this switch is set to <code>true</code>, the distribution will only
		 * return positive samples. If a negative sample is drawn, it will be
		 * dismissed and new samples will be drawn until a positive is produced,
		 * which will be returned.
		 */
		protected bool nonNegative;

		/// <summary>
		/// Creates a RandomDistribution object which gets its initial seed from the experiment's seedgenerator.
		/// The <c>LinearCongruentialRandomGenerator</c> is used as the underlying uniform pseudo random number
		/// generator for all pseudo random distribution .
		/// </summary>
		/// <param name="owner">The distribution's owner</param>
		/// <param name="name">The distribution's name</param>
		/// <param name="showInReport">Flag to show distribution in report</param>
		/// <param name="showInTrace">Flag to show distribution in trace</param>
		public Distribution(Model owner, String name, bool showInReport,
				bool showInTrace) : base(owner, name, showInReport, showInTrace)
		{

			//super(owner, name, showInReport, showInTrace); // construct the
			// reportable
			if (randomGenerator == null)
			{
				randomGenerator = owner.getExperiment()
					.getDistributionManager().getRandomNumberGenerator();

				if (randomGenerator==null)
                    randomGenerator = new LinearCongruentialRandomGenerator();
			}

			owner.getExperiment().getDistributionManager().register(this);

			// set seed in case experiment running
			// (for not yet running experiments, this happens automatically
			// when the experiment is started)
			if (owner.getExperiment().isRunning())
			{
				randomGenerator.setSeed(initialSeed);
			}
		}

		/// <summary>
		/// Changes the underlying random generator to the one given as a parameter. Custom random generators have
		/// to implement the UniormRandomGenerator interface. Note that changing the underlying random generator
		/// forces a reset, since a new generator might produce a completely different stream of pseudo random
		/// numbers that won't enable us to reproduce the stream of numbers probably delivered by the previously
		/// used generator.
		/// </summary>
		/// <param name="randomGenerator">the random generator used for creating distributions</param>
		public void changeRandomGenerator(
				UniformRandomGenerator randomGenerator)
		{

			this.randomGenerator = randomGenerator;
			reset();

		}

		/// <summary>
		/// Creates the default reporter associated with this distribution. The basic <c>DistributionReporter</c>
		/// returned as a default implementation of this method simply reports the distribution's name, number of
		/// observations (samples given), seed and point of simulation time of the last reset.
		/// </summary>
		/// <returns>The reporter associated with this distribution</returns>
		/// <remarks>See also DistributionReporter</remarks>
		protected override Reporter createDefaultReporter()
		{

			return new DistributionReporter(this);

		}

		/// <summary>Returns the seed value since last reset.</summary>
		/// <returns>The initial seed value</returns>
		public long getInitialSeed()
		{

			return initialSeed;

		}

		/// <summary>Tells if this distribution can return negative samples.</summary>
		/// <returns>If <c>true</c> it returns positive samples only</returns>
		public bool getNonNegative()
		{

			return nonNegative;

		}

		/// <summary>
		/// Returns the number of Samples given by this distribution. The number of samples is increased whenever
		/// the sample() method is called. It is based on the random numbers of the distribution, not on the number
		/// of random numbers produced by the underlying random generator, since some distributions use algorithms
		/// consuming more than one uniformly distributed random number to produce one sample following the desired
		/// distribution.
		/// </summary>
		/// <returns>the number of samples given to clients</returns>
		public long getNumSamples()
		{

			return getObservations();

		}

		/// <summary>
		/// Returns the current status for antithetic random number generation in this distribution.
		/// </summary>
		/// <returns>The status of antithetic pseudo random number generation</returns>
		/// <remarks>See also Distribution.setAntithetic</remarks>
		public bool isAntithetic()
		{

			return antithetic;

		}

		/**
		 * Resets the pseudo random generator's seed and the number of samples given
		 * to zero. The field antithetic keeps the value it has had before the
		 * reset.
		 */
		public void reset()
		{

			if (randomGenerator == null)
			{
				try
				{
					randomGenerator = this.getModel().getExperiment()
							.getDistributionManager().getRandomNumberGenerator(); // default RandomGenerator
				}
				catch (Exception e)
				{
					randomGenerator = new LinearCongruentialRandomGenerator();
				}
			}

			// sets seed to the seed specified in constructor or by call to
			// setSeed(long)
			randomGenerator.setSeed(initialSeed); // initialSeed stays unchanged
												  // here

			// antithetic = false;
			/*
			 * no need to change this to false. If this distribution has delivered
			 * antithetic random number than it will do so after the reset, too.
			 */

			base.reset(); // reset the Reportable, too.
		}

		/// <summary>
		/// Resets the pseudo random generator's seed to the value passed, the number of samples given to zero and
		/// sets antithetic to false for this distribution. Acts the same as a call of method <c>reset()</c> and a
		/// consecutive call to <c>setSeed(long)</c>.
		/// </summary>
		/// <param name="newSeed">new seed to be used by underlying random number generator after reset</param>
		public void reset(long newSeed)
		{

			randomGenerator.setSeed(newSeed);

			this.initialSeed = newSeed; // initialSeed is changed here

			// antithetic = false;
			/*
			 * no need to change this to false. If this distribution has delivered
			 * antithetic random number than it will do so after the reset, too.
			 */

			base.reset(); // reset the Reportable, too.

		}

		/// <summary>
		/// Convenience method to return the distribution's sample as <c>Object</c>. For type safety, method
		/// <c>sample()</c> should be preferred. However, this method is useful for environments requiring a
		/// non-genetic access point to obtain samples from any distribution.
		/// </summary>
		/// <returns>A sample from this this distribution wrapped as <c>Object</c>.</returns>
		public abstract Object sampleObject();

		/// <summary>
		/// Switches this distribution to produce antithetic samples. To obtain antithetic random numbers, call
		/// this method with the parameter <c>true</c>. Antithetic random numbers are used to minimize the standard
		/// deviation of a series of simulation runs. The results of a run with normal random numbers has to be
		/// standardized with the results of a run using antithetic random numbers, thus doubling the number of
		/// samples needed, but also lowering the standard deviation of the results of that simulation. See
		/// [Page91, p.139].
		/// </summary>
		/// <param name="newAntiStatus">
		/// Parameter <c>true</c> switches antithetic mode on, <c>false</c> switches antithetic mode off
		/// </param>
		public void setAntithetic(bool newAntiStatus)
		{

			antithetic = newAntiStatus;
			reset();

		}

		/// <summary>
		/// Sets the nonNegative switch to the given value. If nonNegative is set to <c>true</c> the distribution
		/// returns positive samples only, otherwise it also produces negative samples, if possible.
		/// </summary>
		/// <param name="newValue">
		/// If <c>true</c> the distribution is set to return positive samples only, otherwise it also produces
		/// negative samples, if possible.
		/// </param>
		public void setNonNegative(bool newValue)
		{
			this.nonNegative = newValue;
		}

		/// <summary>
		/// Sets the underlying pseudo random number generator's seed to the value given. The seed controls the
		/// starting value of the random generators and all following generated pseudo random numbers. Resetting
		/// the seed between two simulation runs will let you use identical streams of random numbers. That will
		/// enable you to compare different strategies within your model based on the same random number stream
		/// produced by the random generator.
		/// </summary>
		/// <param name="newSeed">new seed used by underlying pseudo random number generator</param>
		public void setSeed(long newSeed)
		{

			randomGenerator.setSeed(newSeed); // well, the seed is passed on...
											  // ;-)
			initialSeed = newSeed; // remember new seed for next reset()
			reset(); // and do a reset of statistics to display when a new seed
					 // was
					 // set

		}

		/// <summary>Generates the trace output of each sample. This method is called by sample().</summary>
		/// <param name="sample">The last sample, converted to a String</param>
		protected void traceLastSample(string sample)
		{

			if (this.currentlySendTraceNotes())
				this.sendTraceNote("samples " + sample + " from " + this.getName());

		}
	}
}