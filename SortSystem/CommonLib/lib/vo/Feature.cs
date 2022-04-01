namespace CommonLib.lib.vo
{
    internal class Feature
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int Level { get; set; }
    }

    internal class FeatureList : List<Feature>
    {
    
    }    

    internal class FeatureClassifier
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public float[] Boundaries { get; set; }

        public int Max { get; set; }
        public int Min { get; set; }

        private FeatureClassifier()
        {
            throw new NotImplementedException();
        }

        public FeatureClassifier(int id, string name, int numOfIntervals)
        {
            Id = id;
            Name = name;
            Boundaries = new float[numOfIntervals - 1];
        }

        public FeatureClassifier(int id, string name, float[] boundaries)
        {
            Id = id;
            Name = name;
            Boundaries = boundaries;

            if (boundaries == null)
                throw new ArgumentNullException(nameof(boundaries));

            Array.Sort(boundaries);
        }
        public int Classify(Feature feature)
        {
            if (feature == null)
                return -1;

            if (feature.Level == -1)
                return -1;

            if (feature.Value < 0)
                return -1;

            feature.Level=Classify(feature.Value);
             
            return feature.Level;
        }
        public int Classify(float value)
        {
            
            //the max possible interval index
            //note this value is one greater than the number of boundary
            int maxLevel = Boundaries.Length;

            //the min possible interval index
            int minLevel = 0;

            int i;

            do
            {
                i = (maxLevel - minLevel) / 2 + minLevel;
                if (value > Boundaries[i])
                {
                    //if it's the 'i'th boundary,
                    //and the value is greater than the boundary
                    //then the min possible interval index will be i+1
                    minLevel = i + 1;
                }
                else
                {
                    //if it's the 'i'th boundary,
                    //and the value is smaller than the boundary
                    //then the max possible interval index will be i
                    maxLevel = i;
                }

            } while (maxLevel > minLevel);

            return minLevel;
        }

    }
}
