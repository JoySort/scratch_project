using CommonLib.lib.vo;
using Newtonsoft.Json;

namespace CommonLib.lib.sort
{
    public class Condition :IComparable<Condition>
    {
        public Dictionary<int, int[]> subConditions;

        bool Sorted; 

        public Condition()
        {
            subConditions = new Dictionary<int, int[]>();
            Sorted = false;
        }

        public static Condition? GetConditionFromJson(string jsonStr)
        {            
            if (string.IsNullOrEmpty(jsonStr))
                return null;

            Condition? cd = JsonConvert.DeserializeObject<Condition>(jsonStr);

            if (cd != null)
                cd.SortSubConditions();

            return cd;
        }


        public int AddSubCondition(int key,int[] levels)
        {
            int updated=0;

            if (subConditions.Keys.Contains(key))
            {
                subConditions[key]= levels;
                updated = 1;
            }
            else
                subConditions.Add(key, levels);

            SortSubConditions();

            return updated;

        }

        public void SortSubConditions()
        {
            subConditions = subConditions.OrderByDescending(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (KeyValuePair<int, int[]> kvp in subConditions)
            {
                subConditions[kvp.Key]=kvp.Value.OrderByDescending(v => v).ToArray();
            }
            Sorted = true;
        }

        //public int CompareTo(Condition? other)
        //{ 
        //    if(other == null)
        //        return 1;

        //    //if ((this.Sorted && other.Sorted) == false)
        //    //{
        //    //throw new Exception("the subconditons are not sorted!");
        //    //}

        //    if (!this.Sorted)
        //        this.SortSubConditions();
        //    if (!other.Sorted)
        //        other.SortSubConditions();

        //    int[] keysThisArray = subConditions.Keys.ToArray();
        //    int[] keysOtherArray = other.subConditions.Keys.ToArray();

        //    int ret = arrayCompare(keysThisArray, keysOtherArray);
        //    if (ret != 0)
        //        return ret;

        //    for (int i = 0; i < keysThisArray.Length; i++)
        //    {
        //        ret = arrayCompare(this.subConditions[keysThisArray[i]], other.subConditions[keysThisArray[i]]);
        //        if (ret != 0)
        //            return ret;
        //    }

        //    return 0;
        //}

        public int CompareTo(Condition? other)
        {
            if (other == null)
                return 1;

            //if ((this.Sorted && other.Sorted) == false)
            //{
            //throw new Exception("the subconditons are not sorted!");
            //}

            if (!this.Sorted)
                this.SortSubConditions();
            if (!other.Sorted)
                other.SortSubConditions();

            int[] keysThisArray = subConditions.Keys.ToArray();
            int[] keysOtherArray = other.subConditions.Keys.ToArray();

            if (keysThisArray.Length > keysOtherArray.Length)
                return 1;
            if (keysThisArray.Length < keysOtherArray.Length)
                return -1;

            for (int i = 0; i < keysThisArray.Length; i++)
            {
                if (keysThisArray[i] > keysOtherArray[i])
                    return 1;

                if (keysThisArray[i] < keysOtherArray[i])
                    return -1;

                int ret = arrayCompare(this.subConditions[keysThisArray[i]], other.subConditions[keysThisArray[i]]);
                if (ret != 0)
                    return ret;
            }

            return 0;
        }

        private static int arrayCompare(int[] a, int[] b)
        {
            if (a.Length > b.Length)
                return 1;

            if (a.Length < b.Length)
                return -1;

            
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] > b[i])
                    return 1;
                if (a[i] < b[i])
                    return -1;
            }

            return 0;
        }

        public bool VerifySingleFeature(int featureId, int level)
        {
            int[]? levels;
            if (!subConditions.TryGetValue(featureId, out levels))
            {
                return true;
            }

            if (levels == null)
                return true;

            for (int i = 0; i < level; i++)
            {
                if (levels[i] == level)
                    return true;
            }

            return false;
        }

        public bool Verify(int[] featureIds, int[] levels)
        {
            for (int i = 0; i < featureIds.Length; i++)
            {
                if (!VerifySingleFeature(featureIds[i], levels[i]))
                    return false;
            }
            return true;
        }

        internal bool Verify(FeatureList features)
        {
            foreach (Feature feature in features)
            {
                if (!VerifySingleFeature(feature.Id, feature.Level))
                    return false;
            }

            return true;
        }
    }

    internal class ConditionList : List<Condition>, IComparable<ConditionList>
    {
        public bool Verify(int[] featureIds, int[] levels)
        {
            //if no condition in list, return false;
            foreach (Condition condition in this)
            {
                if (condition.Verify(featureIds, levels))
                    return true;
            }
            return false;
        }

        public bool Verify(FeatureList features)
        {
            //if no condition in list, return false;
            foreach (Condition condition in this)
            {
                if (condition.Verify(features))
                    return true;
            }
            return false;
        }

        public int CompareTo(ConditionList? other)
        {
            if (other == null)
                return 1;

            if (this.Count > other.Count)
                return 1;
            if (this.Count < other.Count)
                return -1;

            Condition[] a = this.ToArray();
            Condition[] b = other.ToArray();

            Array.Sort(a);
            Array.Sort(b);
            a = a.Reverse().ToArray();
            b = b.Reverse().ToArray();

            for (int i = 0; i < a.Length; i++)
            {
                int temp = a[i].CompareTo(b[i]);
                if (temp != 0)
                    return temp;
            }

            return 0;
        }
    }

}
