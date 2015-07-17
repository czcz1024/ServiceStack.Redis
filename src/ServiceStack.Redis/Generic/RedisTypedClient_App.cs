using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Text;

namespace ServiceStack.Redis.Generic
{
    public partial class RedisTypedClient<T>
    {
        private string GetChildReferenceSetKey<TChild>(object parentId)
        {
            return string.Concat(client.NamespacePrefix, "ref:", typeof(T).Name, "/", typeof(TChild).Name, ":", parentId);
        }

        private string GetChildReferenceSetKey<TChild>(object parentId,string referenceType)
        {
            return string.Concat(client.NamespacePrefix, "ref:", typeof(T).Name, "/", typeof(TChild).Name,"/",referenceType, ":", parentId);
        }

        public void StoreRelatedEntities<TChild>(object parentId, List<TChild> children)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId);
            var childKeys = children.ConvertAll(x => client.UrnKey(x));

            using (var trans = client.CreateTransaction())
            {
                //Ugly but need access to a generic constraint-free StoreAll method
                trans.QueueCommand(c => ((RedisClient)c)._StoreAll(children));
                trans.QueueCommand(c => c.AddRangeToSet(childRefKey, childKeys));

                trans.Commit();
            }
        }

        public void StoreRelatedEntities<TChild>(object parentId,string relatedType, List<TChild> children)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId, relatedType);
            var childKeys = children.ConvertAll(x => client.UrnKey(x));

            using (var trans = client.CreateTransaction())
            {
                //Ugly but need access to a generic constraint-free StoreAll method
                trans.QueueCommand(c => ((RedisClient)c)._StoreAll(children));
                trans.QueueCommand(c => c.AddRangeToSet(childRefKey, childKeys));

                trans.Commit();
            }
        }

        public void StoreRelatedEntities<TChild>(object parentId, params TChild[] children)
        {
            StoreRelatedEntities(parentId, new List<TChild>(children));
        }

        public void StoreRelatedEntities<TChild>(object parentId,string relatedType, params TChild[] children)
        {
            StoreRelatedEntities(parentId,relatedType, new List<TChild>(children));
        }

        public void DeleteRelatedEntity<TChild>(object parentId, object childId)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId);

            //client.RemoveItemFromSet(childRefKey, TypeSerializer.SerializeToString(childId));
            //add by cz : this is a bug
            client.RemoveItemFromSet(childRefKey, client.UrnKey<TChild>(childId));
        }

        public void DeleteRelatedEntity<TChild>(object parentId,string relatedType, object childId)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId,relatedType);
            client.RemoveItemFromSet(childRefKey, client.UrnKey<TChild>(childId));
        }

        public void DeleteRelatedEntities<TChild>(object parentId)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId);
            client.Remove(childRefKey);
        }

        public void DeleteRelatedEntities<TChild>(object parentId, string relatedType)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId,relatedType);
            client.Remove(childRefKey);
        }

        public List<TChild> GetRelatedEntities<TChild>(object parentId)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId);
            var childKeys = client.GetAllItemsFromSet(childRefKey).ToList();

            return client.As<TChild>().GetValues(childKeys);
        }

        public List<TChild> GetRelatedEntities<TChild>(object parentId,string relatedType)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId, relatedType);
            var childKeys = client.GetAllItemsFromSet(childRefKey).ToList();

            return client.As<TChild>().GetValues(childKeys);
        }

        public long GetRelatedEntitiesCount<TChild>(object parentId)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId);
            return client.GetSetCount(childRefKey);
        }

        public long GetRelatedEntitiesCount<TChild>(object parentId,string relatedType)
        {
            var childRefKey = GetChildReferenceSetKey<TChild>(parentId, relatedType);
            return client.GetSetCount(childRefKey);
        }

        public void AddToRecentsList(T value)
        {
            var key = client.UrnKey(value);
            var nowScore = DateTime.UtcNow.ToUnixTime();
            client.AddItemToSortedSet(RecentSortedSetKey, key, nowScore);
        }

        public List<T> GetLatestFromRecentsList(int skip, int take)
        {
            var toRank = take - 1;
            var keys = client.GetRangeFromSortedSetDesc(RecentSortedSetKey, skip, toRank);
            var values = GetValues(keys);
            return values;
        }

        public List<T> GetEarliestFromRecentsList(int skip, int take)
        {
            var toRank = take - 1;
            var keys = client.GetRangeFromSortedSet(RecentSortedSetKey, skip, toRank);
            var values = GetValues(keys);
            return values;
        }

        private string GetRecentSetKey(object parentId,string recentType)
        {
            return string.Concat(client.NamespacePrefix, "recent:", typeof(T).Name,"/",recentType,":",parentId);
        }

        public void AddToRecentsList(object parentId,string recentType,T value)
        {
            var key = client.UrnKey(value);
            var nowScore = DateTime.UtcNow.ToUnixTime();
            var recentSetKey = GetRecentSetKey(parentId, recentType);
            client.AddItemToSortedSet(recentSetKey, key, nowScore);
        }

        public List<T> GetLatestFromRecentsList(object parentId,string recentType,int skip, int take)
        {
            var toRank = take - 1;
            var recentSetKey = GetRecentSetKey(parentId, recentType);
            var keys = client.GetRangeFromSortedSetDesc(recentSetKey, skip, toRank);
            var values = GetValues(keys);
            return values;
        }

        public List<T> GetEarliestFromRecentsList(object parentId,string recentType,int skip, int take)
        {
            var toRank = take - 1;
            var recentSetKey = GetRecentSetKey(parentId, recentType);
            var keys = client.GetRangeFromSortedSet(recentSetKey, skip, toRank);
            var values = GetValues(keys);
            return values;
        }
    }
}