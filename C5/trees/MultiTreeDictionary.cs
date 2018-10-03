using System;
using System.Text;
using C5;
using SCG = System.Collections.Generic;

namespace C5
{
    // Here we implement a multivalued tree as a tree dictionary
    // from keys to value collections.  The value collections may have
    // set or bag semantics.

    // The value collections are externally modifiable (as in Peter
    // Golde's PowerCollections library), and therefore:
    //
    //  * A value collection associated with a key may be null or
    //  non-empty.  Hence for correct semantics, the Contains(k) method
    //  must check that the value collection associated with a key is
    //  non-null and non-empty.
    //  
    //  * A value collection may be shared between two or more keys.
    //

    [Serializable]
    public class MultiTreeDictionary<K, V, VC> : TreeDictionary<K, VC>
      where VC : ICollection<V>, new()
        where K : IComparable

    {
        private int count = 0;      // Cached value count, updated by events only

        private void IncrementCount(Object sender, ItemCountEventArgs<V> args)
        {
            count += args.Count;
        }

        private void DecrementCount(Object sender, ItemCountEventArgs<V> args)
        {
            count -= args.Count;
        }

        private void ClearedCount(Object sender, ClearedEventArgs args)
        {
            count -= args.Count;
        }

        public MultiTreeDictionary()
        {
            ItemsAdded +=
              delegate(Object sender, ItemCountEventArgs<KeyValuePair<K, VC>> args)
              {
                  VC values = args.Item.Value;
                  if (values != null)
                  {
                      count += values.Count;
                      values.ItemsAdded += IncrementCount;
                      values.ItemsRemoved += DecrementCount;
                      values.CollectionCleared += ClearedCount;
                  }
              };
            ItemsRemoved +=
              delegate(Object sender, ItemCountEventArgs<KeyValuePair<K, VC>> args)
              {
                  VC values = args.Item.Value;
                  if (values != null)
                  {
                      count -= values.Count;
                      values.ItemsAdded -= IncrementCount;
                      values.ItemsRemoved -= DecrementCount;
                      values.CollectionCleared -= ClearedCount;
                  }
              };
        }

        // Return total count of values associated with keys.  

        public new virtual int Count
        {
            get
            {
                return count;
            }
        }

        public override Speed CountSpeed
        {
            get { return Speed.Constant; }
        }

        // Add a (key,value) pair

        public virtual void Add(K k, V v)
        {
            VC values;
            if (!base.Find(k, out values) || values == null)
            {
                values = new VC();
                Add(k, values);
            }
            values.Add(v);
        }

        // Remove a single (key,value) pair, if present; return true if
        // anything was removed, else false

        public virtual bool Remove(K k, V v)
        {
            VC values;
            if (base.Find(k, out values) && values != null)
            {
                if (values.Remove(v))
                {
                    if (values.IsEmpty)
                        base.Remove(k);
                    return true;
                }
            }
            return false;
        }

        // Determine whether key k is associated with a value

        public override bool Contains(K k)
        {
            VC values;
            return Find(k, out values) && values != null && !values.IsEmpty;
        }

        // Determine whether each key in ks is associated with a value

        public override bool ContainsAll<U>(SCG.IEnumerable<U> ks)
        {
            foreach (K k in ks)
                if (!Contains(k))
                    return false;
            return true;
        }

        // Get or set the value collection associated with key k

        public override VC this[K k]
        {
            get
            {
                VC values;
                return base.Find(k, out values) && values != null ? values : new VC();
            }
            set
            {
                base[k] = value;
            }
        }

        public override void Clear()
        {
            foreach (VC values in Values)
                if (values != null)
                {
                    count -= values.Count;
                    values.ItemsAdded -= IncrementCount;
                    values.ItemsRemoved -= DecrementCount;
                    values.CollectionCleared -= ClearedCount;
                }
            base.Clear();
        }
    }
}
