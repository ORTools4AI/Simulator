// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.dist
{
    public class MersenneTwisterRandomGenerator : UniformRandomGenerator
    {
        protected int[] mersenneTwister;
        protected int currentIndex;

        public MersenneTwisterRandomGenerator() : this(42) { }

        public MersenneTwisterRandomGenerator(long seed)
        {
            mersenneTwister = new int[624];
            setSeed(seed);
        }

        public double nextDouble()
        {
            return (((long)nextInt(26) << 27) + nextInt(27)) / (double)(1L << 53);
        }

        public long nextLong()
        {
            return ((long)nextInt(32) << 32) + nextInt(32);
        }

        public int nextInt(int bits)
        {
            int y = mersenneTwister[currentIndex];
            currentIndex++;
            if (currentIndex > 623)
                twistNumbers();
            y = y ^ (y >>> 11);
            y = y ^ ((y << 7) & unchecked((int)0x9d2c5680));
            y = y ^ ((y << 15) & unchecked((int)0xefc60000));
            y = y ^ (y >>> 18);
            return y >>> (32 - bits);
        }

        public void twistNumbers()
        {
            for (int i = 0; i < 624; i++)
            {
                int y = (mersenneTwister[i] & unchecked((int)0x80000000))
                      | (mersenneTwister[(i + 1) % 624] & 0x7fffffff);
                if (y % 2 == 0)
                    mersenneTwister[i] = mersenneTwister[(i + 397) % 624] ^ (y >>> 1);
                else
                    mersenneTwister[i] = mersenneTwister[(i + 397) % 624] ^ (y >>> 1) ^ unchecked((int)0x9908b0df);
            }
            currentIndex = 0;
        }

        public void setSeed(long newSeed)
        {
            var rng = new Random((int)(newSeed ^ (newSeed >> 32)));
            for (int i = 0; i < 624; i++)
                mersenneTwister[i] = rng.Next();
            twistNumbers();
        }
    }
}
