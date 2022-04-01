using System.Collections.Generic;
using System.Linq;
using CommonLib.lib.vo;

namespace CommonLib.lib.sort
{
    internal class Channel
    {
        internal int channelId;//start with 0
        private int groupId;
        private int totalCount=0;
        internal int[] colsCounts;
        internal List<int> mateChIds;
        internal ConditionHelper conditions;
        internal ChannelList Parent;

        private void Reset(int _colsCount)
        {
            this.colsCounts = new int[_colsCount];
            for (var i = 0; i < colsCounts.Length; i++)
            {
                colsCounts[i] = 0;
            }
            totalCount = 0;
            conditions = new ConditionHelper();
            mateChIds = new List<int>() { channelId };
        }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Channel(int id, int _colsCount)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            channelId = id;
            groupId = id;
            Reset(_colsCount);
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public Channel(int id, int _colsCount, int gid)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
            channelId = id;
            groupId = gid;
            Reset(_colsCount);
        }

        public void SetConditions(ConditionHelper _conditions)
        {
            conditions = _conditions;
        }

        public bool TryToMakeFriend(Channel other)
        {
            if (Parent == null || Parent != other.Parent)
                return false;

            if (conditions == null)
                return false;

            if (this == other)
                return false;
            if (channelId == other.channelId)
                return false;

            if (conditions.CompareTo(other.conditions) != 0)
                return false;

            Parent.ExchangeAddrBook(this, other);

            return true;
        }
    }

    internal class ChannelList
    {
        Dictionary<int, Channel> channels = new Dictionary<int, Channel>();

        public enum Policy
        {
            PriorityDesc,
            PriorityAsc,
            PriorityNone
        }

        //return channel index started with 0
        //return -1 if no channel is ok
        public int findAChannel(FeatureList features, int col)
        {
            List<int> canIds = new List<int>();
            for (int i = channels.Count - 1; i >= 0; i--)
            {
                if (channels[i].conditions != null)
                {
                    //if conditions length==0 return false                    
                    if (channels[i].conditions.Verify(features))

                    {
                        canIds.Add(i);
                    }
                }
            }

            if (canIds.Count == 0)
                return -1;
            else if (canIds.Count == 1)
                return canIds[0];

            return doLoadBalance(canIds, col);
        }

        public int findAChannel(FeatureList features, int col, Policy policy)
        {
            if (policy == Policy.PriorityNone)
                return findAChannel(features, col);

            int firstValidId = -1;

            if (policy == Policy.PriorityAsc)
            {
                for (int i = channels.Count - 1; i >= 0; i--)
                {
                    if (channels[i].conditions != null)
                    {
                        if (channels[i].conditions.Verify(features))
                        {
                            firstValidId = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < channels.Count; i++)
                {
                    if (channels[i].conditions.Verify(features))
                    {
                        firstValidId = i;
                        break;
                    }
                }
            }

            if (firstValidId == -1)
                return -1;
            if (channels[firstValidId].mateChIds == null)
                return firstValidId;

            List<int>? canIds = channels[firstValidId].mateChIds;

            return doLoadBalance(canIds, col);

        }

        private int doLoadBalance(List<int>? canIds, int col)
        {
            if (canIds == null)
                return -1;

            int min = int.MaxValue;
            int minId = 0;

            if (col % 2 == 0)
            {
                for (int i = 0; i < canIds.Count; i++)
                {
                    if (min > channels[canIds[i]].colsCounts[col])
                    {
                        minId = canIds[i];
                        min = channels[minId].colsCounts[col];

                    }
                }

            }
            else
            {
                for (int i = canIds.Count - 1; i >= 0; i--)
                {
                    if (min > channels[canIds[i]].colsCounts[col])
                    {
                        minId = canIds[i];
                        min = channels[minId].colsCounts[col];

                    }
                }
            }
            channels[minId].colsCounts[col]++;
            return minId;
        }

        public bool ExchangeAddrBook(Channel a, Channel b)
        {
            if (a.Parent != this || b.Parent != this)
                return false;

            Dictionary<int, int> ids = new Dictionary<int, int>();

            ids[a.channelId] = 0;
            ids[b.channelId] = 0;

            if (a.mateChIds != null)
            {
                foreach (int id in a.mateChIds)
                {
                    ids[id] = 0;
                }
            }

            if (b.mateChIds != null)
            {
                foreach (int id in b.mateChIds)
                {
                    ids[id] = 0;
                }
            }

            List<int> mateIds = new List<int>();
            foreach (int id in ids.Keys)
            {
                mateIds.Add(id);
            }
            foreach (int id in mateIds)
            {
                this.channels[id].mateChIds = mateIds;
            }

            return true;
        }
        public Channel this[int key]
        {
            get { return channels[key]; }
            set { AddChannel(value); }
        }

        public void AddChannel(Channel ch)
        {
            if (ch == null)
                return;

            channels[ch.channelId] = ch;
            ch.Parent = this;

            Dictionary<int, Channel> channelsold = channels;
            channels = channelsold.OrderBy(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            while (channelsold.Count > 1)
            {
                Channel channela = channelsold.FirstOrDefault().Value;

                foreach (Channel channelb in channelsold.Values)
                {
                    channela.TryToMakeFriend(channelb);
                }
                if (channela.mateChIds != null)
                    foreach (int key in channela.mateChIds)
                    {
                        channelsold.Remove(key);
                    }
            }

            foreach (Channel channela in channels.Values)
            {
                foreach (Channel channelb in channelsold.Values)
                {
                    channela.TryToMakeFriend(channelb);
                }
            }

        }

    }
}
